namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Result of a single Cursor Agent run (print or stream completion).
/// SessionId and RequestId can be used with <see cref="AgentChatOptions.WithResume"/> or persisted for resume across restarts.
/// </summary>
public sealed class AgentRunResult
{
    /// <summary>Unique session identifier from the CLI (use for --resume).</summary>
    public string? SessionId { get; init; }

    /// <summary>Optional request identifier from the API.</summary>
    public string? RequestId { get; init; }

    /// <summary>Full assistant response text (when format is text or json).</summary>
    public string? Result { get; init; }

    /// <summary>Total execution time in milliseconds.</summary>
    public long? DurationMs { get; init; }

    /// <summary>API request time in milliseconds.</summary>
    public long? DurationApiMs { get; init; }

    /// <summary>Whether the run reported an error.</summary>
    public bool IsError { get; init; }

    /// <summary>Process exit code.</summary>
    public int ExitCode { get; init; }

    /// <summary>Error message from stderr, if any.</summary>
    public string? ErrorMessage { get; init; }
}
