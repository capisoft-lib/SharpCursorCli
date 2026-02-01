namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Resolves the Cursor Agent executable path: .env (CURSOR_AGENT_PATH) then environment variable then PATH.
/// On Windows, if the path ends with .ps1, we use pwsh or powershell to run it.
/// </summary>
internal static class AgentPathResolver
{
    /// <summary>
    /// Returns (fileName, arguments): the executable to start and the arguments to pass (e.g. for pwsh -File agent.ps1 -- ...).
    /// </summary>
    public static (string FileName, List<string> Args) Resolve()
    {
        var path = EnvLoader.GetCursorAgentPath();
        if (!string.IsNullOrWhiteSpace(path))
        {
            path = path!.Trim();
            if (Path.IsPathRooted(path) && !File.Exists(path))
                throw new InvalidOperationException($"CURSOR_AGENT_PATH is set to '{path}' but the file does not exist.");
            if (!Path.IsPathRooted(path))
            {
                // Treat as command name (e.g. "agent"); try PATH
                return (path, new List<string>());
            }
            // Absolute path
            if (path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                var pwsh = FindPowerShell();
                var args = new List<string> { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", path };
                return (pwsh, args);
            }
            return (path, new List<string>());
        }
        // Fallback: agent or agent.cmd on PATH
        return ("agent", new List<string>());
    }

    private static string FindPowerShell()
    {
        var pwsh = Environment.GetEnvironmentVariable("PWSH") ?? "pwsh";
        try
        {
            if (Which(pwsh) != null)
                return pwsh;
        }
        catch
        {
            // ignore
        }
        return "powershell";
    }

    private static string? Which(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return null;
        var ext = Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT;.PS1";
        var exts = ext.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var basePath = Path.Combine(dir.Trim(), command);
            foreach (var e in exts)
            {
                var full = basePath + e.Trim();
                if (File.Exists(full))
                    return full;
            }
            if (File.Exists(basePath))
                return basePath;
        }
        return null;
    }
}
