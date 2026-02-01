# SharpCursorCli Examples

Separate, minimal examples per use case. Copy `.env.example` to `.env` in the **repository root** (or in this folder) and set `CURSOR_AGENT_PATH` to your Cursor Agent path (e.g. `C:\Users\AI\AppData\Local\cursor-agent\agent.ps1`).

| Project | Use case |
|--------|----------|
| **Example.RunAsync** | Print mode, single run; get `AgentRunResult` (SessionId, RequestId, Result, DurationMs). |
| **Example.Stream** | Stream mode with `IAsyncEnumerable<StreamEvent>`; handle init, assistant, terminal events. |
| **Example.Resume** | Resume by session ID: `WithResume(sessionId)` (pass session ID as first argument). |
| **Example.Storage** | Save conversation after run; list recent / list session IDs; get by session ID. |
| **Example.FullFlow** | Run → save to storage → resume by session ID from storage. |
| **Example.Parameters** | Complex options: `--model`, `--mode plan|ask`, `--resume`, `--force`, `--stream-partial`, `--extra`; API key from env; Json vs StreamJson. |

Run from the solution directory:

```bash
dotnet run --project Example.RunAsync
dotnet run --project Example.Stream
dotnet run --project Example.Resume -- <sessionId>
dotnet run --project Example.Storage
dotnet run --project Example.FullFlow
dotnet run --project Example.Parameters
dotnet run --project Example.Parameters -- --mode ask --force
dotnet run --project Example.Parameters -- --stream-partial
dotnet run --project Example.Parameters -- --resume <sessionId> --model sonnet-4.5
```
