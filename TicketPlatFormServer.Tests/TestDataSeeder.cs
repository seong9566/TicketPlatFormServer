using MySqlConnector;

namespace TicketPlatFormServer.Tests;

/// <summary>
/// Creates minimal test fixtures with Guid-based unique values.
/// All test emails follow: test_{guid}@test.com
/// </summary>
public class TestDataSeeder
{
    private readonly MySqlConnection _conn;

    public TestDataSeeder(MySqlConnection conn)
    {
        _conn = conn;
    }

    public async Task<(int userId, string email, string password)> CreateUserAsync(
        string? email = null, string role = "user")
    {
        if (_conn.State != System.Data.ConnectionState.Open)
            await _conn.OpenAsync();

        email ??= $"test_{Guid.NewGuid():N}@test.com";
        var rawPassword = "Test1234!@#";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var phone = $"010{new Random().Next(10000000, 99999999)}";

        await using var cmd = new MySqlCommand(@"
            INSERT INTO users (email, password_hash, role, phone, created_at, updated_at)
            VALUES (@email, @hash, @role, @phone, NOW(), NOW())", _conn);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@hash", hashedPassword);
        cmd.Parameters.AddWithValue("@role", role);
        cmd.Parameters.AddWithValue("@phone", phone);
        await cmd.ExecuteNonQueryAsync();

        var userId = (int)cmd.LastInsertedId;

        // Create user_profile
        await using var profileCmd = new MySqlCommand(@"
            INSERT INTO user_profile (user_id, nickname, created_at, updated_at)
            VALUES (@userId, @nickname, NOW(), NOW())", _conn);
        profileCmd.Parameters.AddWithValue("@userId", userId);
        profileCmd.Parameters.AddWithValue("@nickname", $"TestUser{userId}");
        await profileCmd.ExecuteNonQueryAsync();

        return (userId, email, rawPassword);
    }
}
