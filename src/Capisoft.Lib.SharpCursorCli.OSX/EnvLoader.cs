namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Minimal .env file loader for macOS. Reads KEY=value lines; skips empty and # comments.
/// Used to resolve CURSOR_AGENT_PATH from .env before falling back to environment variable and PATH.
/// </summary>
internal static class EnvLoader
{
    public const string CursorAgentPathKey = "CURSOR_AGENT_PATH";

    public static string? GetValue(string key)
    {
        var dir = Environment.CurrentDirectory;
        for (var i = 0; i < 6 && !string.IsNullOrEmpty(dir); i++)
        {
            var path = Path.Combine(dir, ".env");
            if (File.Exists(path))
            {
                var value = LoadValueFromFile(path, key);
                if (value != null)
                    return value;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    public static string? GetCursorAgentPath()
    {
        var fromEnvFile = GetValue(CursorAgentPathKey);
        if (!string.IsNullOrWhiteSpace(fromEnvFile))
            return fromEnvFile.Trim();
        var fromEnv = Environment.GetEnvironmentVariable(CursorAgentPathKey);
        return string.IsNullOrWhiteSpace(fromEnv) ? null : fromEnv.Trim();
    }

    private static string? LoadValueFromFile(string filePath, string key)
    {
        try
        {
            foreach (var line in File.ReadLines(filePath))
            {
                var s = line.Trim();
                if (s.Length == 0 || s.StartsWith('#'))
                    continue;
                var eq = s.IndexOf('=');
                if (eq <= 0)
                    continue;
                var k = s[..eq].Trim();
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                {
                    var v = s[(eq + 1)..].Trim();
                    if (v.Length >= 2 && v.StartsWith('"') && v.EndsWith('"'))
                        v = v[1..^1].Replace("\\\"", "\"");
                    return v;
                }
            }
        }
        catch
        {
            // Ignore read errors
        }
        return null;
    }
}
