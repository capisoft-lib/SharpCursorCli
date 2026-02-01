namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Agent mode for the Cursor CLI (maps to <c>--mode</c>).
/// See <see href="https://cursor.com/docs/agent/modes">Agent Modes</see>.
/// </summary>
public enum AgentMode
{
    /// <summary>Full access to all tools (default).</summary>
    Agent,

    /// <summary>Plan before coding; clarifying questions.</summary>
    Plan,

    /// <summary>Read-only exploration; no file edits.</summary>
    Ask,
}
