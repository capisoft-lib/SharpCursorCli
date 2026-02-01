namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Lightweight persistence for Cursor Agent conversation IDs so they can be retrieved across restarts.
/// Not a full transcript system; use for saving session/request IDs and prompt previews for resume.
/// </summary>
public interface ICursorAgentStorage
{
    /// <summary>Saves a conversation (session_id, optional request_id, prompt preview, optional metadata).</summary>
    void SaveConversation(string sessionId, string? requestId = null, string? promptPreview = null, string? metadata = null);

    /// <summary>Gets a conversation by session ID, or null if not found.</summary>
    ConversationRecord? GetConversation(string sessionId);

    /// <summary>Lists the most recent conversations (newest first).</summary>
    IReadOnlyList<ConversationRecord> GetRecentConversations(int limit = 10);

    /// <summary>Lists all known session IDs (for use with --resume).</summary>
    IReadOnlyList<string> ListSessionIds();
}
