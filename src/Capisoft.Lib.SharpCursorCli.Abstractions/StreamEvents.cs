namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Base type for NDJSON stream events from the Cursor CLI when using <c>--output-format stream-json</c>.
/// See <see href="https://cursor.com/docs/cli/reference/output-format">Output Format</see>.
/// </summary>
public abstract record StreamEvent
{
    /// <summary>Session ID (present on most events).</summary>
    public string? SessionId { get; init; }
}

/// <summary>System initialization event (emitted once at session start).</summary>
public sealed record SystemInitEvent : StreamEvent
{
    public string? Cwd { get; init; }
    public string? Model { get; init; }
    public string? ApiKeySource { get; init; }
    public string? PermissionMode { get; init; }
}

/// <summary>User message event (contains the prompt).</summary>
public sealed record UserMessageEvent : StreamEvent
{
    public string? Text { get; init; }
}

/// <summary>Assistant message event (text segment between tool calls, or final message).</summary>
/// <remarks>When using <c>--stream-partial-output</c>, the CLI sends incremental deltas (<see cref="IsDelta"/> = true)
/// and may also send a final full message (IsDelta = false). To avoid duplicate output, prefer printing only deltas
/// when streaming partial output, or deduplicate by skipping a full message that matches already-printed content.</remarks>
public sealed record AssistantMessageEvent : StreamEvent
{
    public string? Text { get; init; }

    /// <summary>True when this event is an incremental delta (partial chunk); false when it is the full message for the turn.</summary>
    public bool IsDelta { get; init; }
}

/// <summary>Tool call event (started or completed).</summary>
public sealed record ToolCallEvent : StreamEvent
{
    public bool IsCompleted { get; init; }
    public string? CallId { get; init; }
    public string? RawJson { get; init; }
}

/// <summary>Terminal result event (final event on success; contains session_id, request_id, result, duration).</summary>
public sealed record TerminalResultEvent : StreamEvent
{
    public string? RequestId { get; init; }
    public string? Result { get; init; }
    public long? DurationMs { get; init; }
    public long? DurationApiMs { get; init; }
    public bool IsError { get; init; }
}
