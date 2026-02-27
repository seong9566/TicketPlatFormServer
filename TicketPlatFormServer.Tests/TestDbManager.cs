using MySqlConnector;
using System.Diagnostics;

namespace TicketPlatFormServer.Tests;

public static class TestDbManager
{
    private const string Host = "127.0.0.1";
    private const string Port = "3306";
    private const string User = "root";
    private const string Password = "stecdev1234!";
    private const string TestDbName = "TicketPlatFormDB_Test";

    // Get the absolute path to the database_history directory
    // The test runs from bin/Debug/net9.0/, so go up to find the repo root
    private static string GetDatabaseHistoryPath()
    {
        // Walk up from current directory to find the repo root
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "TicketPlatFormServer")))
        {
            dir = dir.Parent;
        }
        if (dir == null)
            throw new InvalidOperationException("Could not find repo root");
        return Path.Combine(dir.FullName, "TicketPlatFormServer", "TicketPlatFormServer", "database_history");
    }

    public static async Task InitializeAsync()
    {
        // Step 1: Create test database if not exists
        using var conn = new MySqlConnection(
            $"Server={Host};Port={Port};User={User};Password={Password};SslMode=None;AllowPublicKeyRetrieval=True;");
        await conn.OpenAsync();
        await using var cmd = new MySqlCommand(
            $"CREATE DATABASE IF NOT EXISTS `{TestDbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
            conn);
        await cmd.ExecuteNonQueryAsync();

        // Step 2: Check if tables already exist (idempotent)
        await using var checkCmd = new MySqlCommand(
            $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{TestDbName}';",
            conn);
        var tableCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

        if (tableCount == 0)
        {
            // Step 3: Apply dump + migrations
            var dbHistoryPath = GetDatabaseHistoryPath();
            await ApplySqlFileAsync(Path.Combine(dbHistoryPath, "TicketPlatFormDB_dump.sql"), stripGtid: true);
            await ApplySqlFileAsync(Path.Combine(dbHistoryPath, "TASK-008-migration.sql"), force: true);
            await ApplySqlFileAsync(Path.Combine(dbHistoryPath, "TASK-012-migration.sql"));
            await ApplySqlFileAsync(Path.Combine(dbHistoryPath, "TASK-013-migration.sql"));
            await ApplySqlFileAsync(Path.Combine(dbHistoryPath, "BALANCE-001-migration.sql"));
        }
    }

    private static async Task ApplySqlFileAsync(string filePath, bool force = false, bool stripGtid = false)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"SQL file not found: {filePath}");

        var psi = new ProcessStartInfo
        {
            FileName = "mysql",
            Arguments = $"-h {Host} -P {Port} -u {User} -p{Password} {(force ? "--force" : "")} {TestDbName}",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start mysql process");
        var sql = await File.ReadAllTextAsync(filePath);

        // Strip GTID_PURGED lines — dev dump has GTIDs that conflict when restoring to test DB
        if (stripGtid)
        {
            sql = string.Join("\n", sql.Split('\n')
                .Where(line => !line.Contains("GTID_PURGED") &&
                               !System.Text.RegularExpressions.Regex.IsMatch(line, @"^[0-9a-f]{8}-")));
        }
        await process.StandardInput.WriteAsync(sql);
        process.StandardInput.Close();

        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"MySQL error applying {Path.GetFileName(filePath)}: {stderr}");
    }

    public static async Task CleanupAsync()
    {
        await using var conn = new MySqlConnection(
            $"Server={Host};Port={Port};Database={TestDbName};User={User};Password={Password};SslMode=None;AllowPublicKeyRetrieval=True;");
        await conn.OpenAsync();

        var commands = new[]
        {
            "DELETE pi FROM profile_image pi INNER JOIN users u ON pi.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
            "DELETE up FROM user_profile up INNER JOIN users u ON up.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
            "DELETE rt FROM refresh_token rt INNER JOIN users u ON rt.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
            "DELETE FROM users WHERE email LIKE 'test_%@test.com'"
        };

        foreach (var sql in commands)
        {
            await using var deleteCmd = new MySqlCommand(sql, conn);
            await deleteCmd.ExecuteNonQueryAsync();
        }
    }
}
