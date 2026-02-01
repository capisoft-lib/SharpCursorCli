using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Windows implementation of <see cref="ICursorAgentRunner"/>.
/// Resolves the Cursor Agent path from .env (CURSOR_AGENT_PATH), then environment variable, then PATH.
/// On Windows, .ps1 paths are executed via pwsh or powershell.
/// </summary>
public sealed class CursorAgentRunnerWindows : ICursorAgentRunner
{
    /// <inheritdoc />
    public async Task<AgentRunResult> RunAsync(AgentChatOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var (fileName, baseArgs) = AgentPathResolver.Resolve();
        var agentArgs = new List<string>();
        ArgumentBuilder.Build(options, agentArgs);
        var fullArgs = new List<string>(baseArgs);
        fullArgs.AddRange(agentArgs);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = options.WorkingDirectory ?? Environment.CurrentDirectory
            }
        };
        foreach (var a in fullArgs)
            process.StartInfo.ArgumentList.Add(a);

        process.Start();

        // Read stdout/stderr from raw streams with UTF-8 so encoding is correct on Windows
        // (BeginOutputReadLine can ignore StandardOutputEncoding on some configurations).
        var stdout = new List<string>();
        var stderr = new List<string>();
        var stdoutTask = ReadLinesUtf8Async(process.StandardOutput.BaseStream, stdout, cancellationToken);
        var stderrTask = ReadLinesUtf8Async(process.StandardError.BaseStream, stderr, cancellationToken);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);

        var exitCode = process.ExitCode;
        var outText = string.Join(Environment.NewLine, stdout);
        var errText = string.Join(Environment.NewLine, stderr);

        if (options.OutputFormat == AgentOutputFormat.Json && !string.IsNullOrWhiteSpace(outText))
        {
            var parsed = StreamJsonParser.ParseFinalJson(outText.Trim(), exitCode, errText);
            if (parsed != null)
                return parsed;
        }

        return new AgentRunResult
        {
            SessionId = null,
            RequestId = null,
            Result = outText,
            ExitCode = exitCode,
            ErrorMessage = string.IsNullOrWhiteSpace(errText) ? null : errText
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamEvent> RunStreamAsync(AgentChatOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var run = RunStreamWithResultAsync(options, cancellationToken);
        await foreach (var evt in run.Stream.WithCancellation(cancellationToken).ConfigureAwait(false))
            yield return evt;
    }

    /// <inheritdoc />
    public AgentStreamRun RunStreamWithResultAsync(AgentChatOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var (fileName, baseArgs) = AgentPathResolver.Resolve();
        var agentArgs = new List<string>();
        ArgumentBuilder.Build(options, agentArgs);
        var fullArgs = new List<string>(baseArgs);
        fullArgs.AddRange(agentArgs);

        var tcs = new TaskCompletionSource<AgentRunResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        async IAsyncEnumerable<StreamEvent> StreamCore([EnumeratorCancellation] CancellationToken ct)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    CreateNoWindow = true,
                    WorkingDirectory = options.WorkingDirectory ?? Environment.CurrentDirectory
                }
            };
            foreach (var a in fullArgs)
                process.StartInfo.ArgumentList.Add(a);

            var stderrLines = new List<string>();
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) stderrLines.Add(e.Data); };
            process.Start();
            process.BeginErrorReadLine();

            using var reader = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8);
            string? line;
            AgentRunResult? finalResult = null;
            try
            {
                while ((line = await reader.ReadLineAsync(ct).ConfigureAwait(false)) != null)
                {
                    var evt = StreamJsonParser.TryParseLine(line);
                    if (evt != null)
                    {
                        if (evt is TerminalResultEvent term)
                        {
                            finalResult = new AgentRunResult
                            {
                                SessionId = term.SessionId,
                                RequestId = term.RequestId,
                                Result = term.Result,
                                DurationMs = term.DurationMs,
                                DurationApiMs = term.DurationApiMs,
                                IsError = term.IsError,
                                ExitCode = 0,
                                ErrorMessage = stderrLines.Count > 0 ? string.Join(Environment.NewLine, stderrLines) : null
                            };
                        }
                        yield return evt;
                    }
                }
            }
            finally
            {
                if (!process.HasExited)
                    try { process.Kill(); } catch { /* ignore */ }
                var exitCode = process.HasExited ? process.ExitCode : -1;
                if (finalResult == null)
                    finalResult = new AgentRunResult
                    {
                        ExitCode = exitCode,
                        ErrorMessage = stderrLines.Count > 0 ? string.Join(Environment.NewLine, stderrLines) : null
                    };
                tcs.TrySetResult(finalResult);
            }
        }

        return new AgentStreamRun(StreamCore(cancellationToken), tcs.Task);
    }

    /// <summary>Read lines from stream as UTF-8 into the list (used so Windows gets correct encoding).</summary>
    private static async Task ReadLinesUtf8Async(Stream stream, List<string> lines, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
            lines.Add(line);
    }
}
