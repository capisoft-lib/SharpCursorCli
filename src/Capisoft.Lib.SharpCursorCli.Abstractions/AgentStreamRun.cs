namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Wrapper for a stream run: exposes the event stream and a task that completes with the final <see cref="AgentRunResult"/> when the stream ends.
/// Use this when you need both real-time events and the final session/request IDs for persistence.
/// </summary>
public sealed class AgentStreamRun
{
    /// <summary>Newline-delimited stream of events (system init, user, assistant, tool_call, result).</summary>
    public IAsyncEnumerable<StreamEvent> Stream { get; }

    /// <summary>Completes when the stream ends with the terminal result (session_id, request_id, duration, etc.).</summary>
    public Task<AgentRunResult> ResultTask { get; }

    /// <summary>Creates a stream run wrapper (used by platform implementations).</summary>
    public AgentStreamRun(IAsyncEnumerable<StreamEvent> stream, Task<AgentRunResult> resultTask)
    {
        Stream = stream;
        ResultTask = resultTask;
    }

    /// <summary>Convenience: await the final result (e.g. after consuming the stream).</summary>
    public Task<AgentRunResult> GetResultAsync(CancellationToken cancellationToken = default) => ResultTask.WaitAsync(cancellationToken);
}
