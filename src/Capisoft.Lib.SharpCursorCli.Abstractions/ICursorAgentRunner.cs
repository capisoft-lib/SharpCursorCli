namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Runner for the Cursor CLI agent. Run chat (print) mode or stream mode; resume by session ID.
/// Implementations are platform-specific (Windows, Linux, OSX).
/// </summary>
public interface ICursorAgentRunner
{
    /// <summary>
    /// Runs the agent with the given options and returns when the run completes.
    /// Use <see cref="AgentChatOptions.WithPrint"/> and <see cref="AgentChatOptions.WithOutputFormat"/> (Text or Json) for non-interactive runs.
    /// </summary>
    /// <param name="options">Chat/print options (prompt, model, mode, etc.).</param>
    /// <param name="cancellationToken">Cancellation (kills the agent process).</param>
    /// <returns>Result with SessionId, RequestId, Result text, duration, exit code.</returns>
    Task<AgentRunResult> RunAsync(AgentChatOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the agent in stream mode: yields NDJSON events (system init, user, assistant, tool_call, terminal result).
    /// Use <see cref="AgentChatOptions.WithOutputFormat"/>(StreamJson) and optionally <see cref="AgentChatOptions.WithStreamPartialOutput"/>.
    /// </summary>
    /// <param name="options">Chat options with StreamJson (and optionally StreamPartialOutput).</param>
    /// <param name="cancellationToken">Cancellation (kills the agent process).</param>
    /// <returns>Async enumerable of stream events.</returns>
    IAsyncEnumerable<StreamEvent> RunStreamAsync(AgentChatOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the agent in stream mode and returns a wrapper with both the event stream and a task that completes with the final <see cref="AgentRunResult"/>.
    /// Use when you need to persist SessionId/RequestId after the stream ends.
    /// </summary>
    /// <param name="options">Chat options with StreamJson.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>Wrapper with Stream and ResultTask.</returns>
    AgentStreamRun RunStreamWithResultAsync(AgentChatOptions options, CancellationToken cancellationToken = default);
}
