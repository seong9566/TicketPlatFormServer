namespace TicketPlatFormServer.Common;

public static class EnvFileLoader
{
    public static void Load(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.Ordinal))
            {
                line = line.Substring("export ".Length).TrimStart();
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line.Substring(0, separatorIndex).Trim();
            if (key.Length == 0)
            {
                continue;
            }

            var value = line.Substring(separatorIndex + 1).Trim();
            value = UnwrapQuotes(value);

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string UnwrapQuotes(string value)
    {
        if (value.Length < 2)
        {
            return value;
        }

        var first = value[0];
        var last = value[^1];
        if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
        {
            return value.Substring(1, value.Length - 2);
        }

        return value;
    }
}
