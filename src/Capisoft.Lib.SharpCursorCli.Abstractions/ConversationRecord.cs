namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Lightweight record for a persisted conversation (session ID, prompt preview, timestamps).
/// Used by <see cref="ICursorAgentStorage"/> for retrieval across restarts.
/// </summary>
public sealed class ConversationRecord
{
    /// <summary>Internal id (e.g. row id).</summary>
    public long Id { get; init; }

    /// <summary>Session ID from the CLI (use with <see cref="AgentChatOptions.WithResume"/>).</summary>
    public string SessionId { get; init; } = "";

    /// <summary>Optional request ID from the API.</summary>
    public string? RequestId { get; init; }

    /// <summary>Short preview of the prompt (e.g. first 200 chars).</summary>
    public string? PromptPreview { get; init; }

    /// <summary>When the conversation was saved (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Optional JSON or key-value metadata.</summary>
    public string? Metadata { get; init; }
}
