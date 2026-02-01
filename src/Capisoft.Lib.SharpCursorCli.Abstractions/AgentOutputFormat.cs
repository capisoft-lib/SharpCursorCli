namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Output format for the Cursor Agent CLI when using print mode.
/// See <see href="https://cursor.com/docs/cli/reference/output-format">Output Format</see>.
/// </summary>
public enum AgentOutputFormat
{
    /// <summary>Plain text; only the final assistant message.</summary>
    Text,

    /// <summary>Single JSON object when the run completes.</summary>
    Json,

    /// <summary>Newline-delimited JSON (NDJSON) stream; use with <see cref="AgentChatOptions.StreamPartialOutput"/> for character-level streaming.</summary>
    StreamJson,
}
