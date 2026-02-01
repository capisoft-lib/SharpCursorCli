namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Resolves the Cursor Agent executable path on Linux: .env (CURSOR_AGENT_PATH) then environment variable then "agent" on PATH.
/// </summary>
internal static class AgentPathResolver
{
    /// <summary>Returns (fileName, baseArgs). On Linux baseArgs is always empty; run the agent binary/script directly.</summary>
    public static (string FileName, List<string> Args) Resolve()
    {
        var path = EnvLoader.GetCursorAgentPath();
        if (!string.IsNullOrWhiteSpace(path))
        {
            path = path!.Trim();
            if (Path.IsPathRooted(path) && !File.Exists(path))
                throw new InvalidOperationException($"CURSOR_AGENT_PATH is set to '{path}' but the file does not exist.");
            if (!Path.IsPathRooted(path))
                return (path, new List<string>());
            return (path, new List<string>());
        }
        return ("agent", new List<string>());
    }
}
