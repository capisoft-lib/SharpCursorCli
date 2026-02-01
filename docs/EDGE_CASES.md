# Edge Cases and Limitations

This document lists known edge cases where the library may fail, behave unexpectedly, or require care from the caller.

## Path resolution

- **Agent not on PATH**: If `CURSOR_AGENT_PATH` is unset and the fallback command is `"agent"` (or a custom non-rooted name), the process is started by name. If that command is not on the system PATH, `Process.Start` will throw (e.g. `System.ComponentModel.Win32Exception`). We do not validate that the command exists before starting.
- **Windows – no PowerShell**: If the path is a `.ps1` script we run it via `pwsh` or `powershell`. We look for `pwsh` then fall back to `powershell` by name; we do not resolve the full path. If neither is on PATH (or `PWSH` env points to a missing executable), `Process.Start` may throw.
- **Rooted path that disappears**: We check `File.Exists(path)` for rooted paths at resolve time. If the file is deleted or the volume is unmounted before `Process.Start`, the process will fail at start.

## Process execution

- **`options` null**: Throws `ArgumentNullException` (guarded in all runners).
- **Working directory missing**: `WorkingDirectory` is passed to the process as-is. If the directory does not exist, behavior is platform-dependent (e.g. `Process.Start` may throw or the child may fail).
- **No timeout**: `RunAsync` and stream methods wait until the agent process exits. If the agent hangs (e.g. waiting for network or user input), the call will not return until the process ends or the process is killed externally. There is no built-in timeout.
- **RunAsync cancellation**: When `CancellationToken` is cancelled, we stop waiting and throw `OperationCanceledException`. The child process is **not** killed; it may keep running until it exits on its own or is killed by the OS/user.
- **Very long prompt / arguments**: The full argument list is passed to the process. On Windows the command line length limit is typically ~32K characters; exceeding it can cause `Process.Start` to fail.

## Stream mode

- **Consumer stops iterating**: If the caller stops consuming the stream (e.g. `break` or exception) without disposing the async enumerator, the underlying process may not be killed until the enumerator is disposed. Prefer `await using` or a `try/finally` that disposes the enumerator so the process is always cleaned up.
- **Stream encoding**: We assume the agent outputs UTF-8. If it prints another encoding or binary data, decoded text may be wrong or contain replacement characters.

## JSON parsing

- **Huge lines**: `TryParseLine` and `ParseFinalJson` parse each line with `JsonDocument.Parse`. Extremely long lines (e.g. very large base64 or message content) could cause high memory use or out-of-memory. Normal CLI output is unlikely to hit this.
- **Malformed / unknown lines**: Unknown event types or invalid JSON are ignored (`TryParseLine` returns `null`); `ParseFinalJson` on parse failure returns an `AgentRunResult` with the raw string in `Result` and the given exit code and stderr.

## Storage

- **Empty or null session ID**: `SaveConversation` throws `ArgumentException` if `sessionId` is null or whitespace. `GetConversation` returns `null` for null/whitespace without querying.
- **Empty or null db path**: The `SqliteCursorAgentStorage(string dbPath)` constructor throws `ArgumentException` if `dbPath` is null or whitespace.
- **Read-only or full disk**: Creating or writing to the database will throw if the filesystem is read-only or out of space.
- **Concurrent access**: Multiple processes or threads can use the same DB file; SQLite handles locking. Schema is created under a static lock so the first writer wins; subsequent calls are idempotent.

## .env loading

- **Current directory**: `.env` is searched from `Environment.CurrentDirectory` upward (up to 6 levels). If the app changes current directory after creating the runner, the path used for the next run was still resolved from the original current directory at the time of the call.
- **Errors**: If `.env` cannot be read (permission, lock, etc.), we ignore the error and treat the value as missing (fall back to environment variable or PATH).

## Summary of guards added

- Runners: `ArgumentNullException.ThrowIfNull(options)` in `RunAsync` and `RunStreamWithResultAsync`.
- Storage: validation of `sessionId` (non-empty for save; null/empty returns null for get) and `dbPath` (non-empty in string constructor).

Other cases above are documented for caller awareness; fixing them would require API or behavior changes (e.g. timeouts, cancellation killing the process, or optional path pre-validation).
