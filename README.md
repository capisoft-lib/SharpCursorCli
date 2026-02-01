# SharpCursorCli

C# wrapper for the [Cursor CLI](https://cursor.com/docs/cli/overview) agent. Run the Cursor Agent from .NET with full parameter support, streaming, and optional SQLite persistence for conversation IDs across restarts.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Cursor CLI](https://cursor.com/docs/cli/installation) installed (agent on PATH or configured path)

## Platform support

The library provides implementations for **Windows**, **Linux**, and **macOS**.

| Platform | Status |
|----------|--------|
| Windows  | ✅ Tested |
| Linux    | ⚠️ **Not tested** — builds included, same API expected |
| macOS    | ⚠️ **Not tested** — builds included, same API expected |

If you run SharpCursorCli successfully on **Linux** or **macOS**, please open an issue to confirm it works so we can update this section. Feedback and PRs for those platforms are welcome.

## Configuration

Copy `.env.example` to `.env` and set your Cursor Agent path:

```bash
cp .env.example .env
```

Edit `.env`:

```
CURSOR_AGENT_PATH=C:\Users\...\AppData\Local\cursor-agent\agent.ps1
```

If `CURSOR_AGENT_PATH` is not set, the wrapper falls back to `agent` (or `agent.cmd` on Windows) on PATH.

## Quick Start

```csharp
using Capisoft.Lib.SharpCursorCli;

var runner = CursorAgent.CreateRunner();
var options = new AgentChatOptions()
    .WithPrint()
    .WithPrompt("List files in the current directory")
    .WithOutputFormat(AgentOutputFormat.Text);

var result = await runner.RunAsync(options);
Console.WriteLine($"SessionId: {result.SessionId}");
Console.WriteLine(result.Result);
```

## Stream Mode

With `--stream-partial-output`, the CLI sends incremental assistant deltas and may also send a final full message. To avoid duplicate output, either print only deltas (`AssistantMessageEvent.IsDelta`) or deduplicate by skipping a full message that matches already-printed content (see [Example.Stream](examples/Example.Stream/)).

```csharp
var options = new AgentChatOptions()
    .WithPrint()
    .WithPrompt("Explain this codebase")
    .WithOutputFormat(AgentOutputFormat.StreamJson)
    .WithStreamPartialOutput();

await foreach (var evt in runner.RunStreamAsync(options))
{
    if (evt is AssistantMessageEvent assistant when !string.IsNullOrEmpty(assistant.Text))
        Console.Write(assistant.Text); // For no-duplicate output, use dedupe logic as in Example.Stream
}
```

## Resume by Session ID

```csharp
var options = new AgentChatOptions()
    .WithPrint()
    .WithResume(sessionId)
    .WithPrompt("Continue from where we left off");
var result = await runner.RunAsync(options);
```

## Storage (SQLite)

Lightweight persistence for conversation IDs so you can resume across restarts. See [Capisoft.Lib.SharpCursorCli.Storage](src/Capisoft.Lib.SharpCursorCli.Storage/) and the [examples](examples/).

```csharp
var storage = new SqliteCursorAgentStorage("conversations.db");
storage.SaveConversation(result.SessionId, result.RequestId, promptPreview: "List files...");
var recent = storage.GetRecentConversations(10);
```

## Solution Structure

| Project | Description |
|--------|-------------|
| [Capisoft.Lib.SharpCursorCli.Abstractions](src/Capisoft.Lib.SharpCursorCli.Abstractions/) | Interfaces, options, result and stream event DTOs |
| [Capisoft.Lib.SharpCursorCli](src/Capisoft.Lib.SharpCursorCli/) | Main library; facade and platform detection |
| [Capisoft.Lib.SharpCursorCli.Windows](src/Capisoft.Lib.SharpCursorCli.Windows/) | Windows implementation (agent path from .env or PATH) |
| [Capisoft.Lib.SharpCursorCli.Linux](src/Capisoft.Lib.SharpCursorCli.Linux/) | Linux implementation (agent path from .env or PATH) |
| [Capisoft.Lib.SharpCursorCli.OSX](src/Capisoft.Lib.SharpCursorCli.OSX/) | macOS implementation (agent path from .env or PATH) |
| [Capisoft.Lib.SharpCursorCli.Storage](src/Capisoft.Lib.SharpCursorCli.Storage/) | Lightweight SQLite storage for conversation IDs |

## Repository layout

- **Library only:** build from repo root with `SharpCursorCli.sln`.
- **Examples:** they reference the library via relative paths. From repo root: `dotnet run --project examples/Example.RunAsync` (or open `examples/SharpCursorCli.Examples.sln`).

## Examples

Separate, minimal examples per use case are in [examples/](examples/):

- **Example.RunAsync** – Print mode, single run, get `AgentRunResult`
- **Example.Stream** – Stream mode with `IAsyncEnumerable<StreamEvent>`
- **Example.Resume** – Resume by session ID
- **Example.Storage** – Save/load conversations with SQLite
- **Example.FullFlow** – Run → save → resume (end-to-end)

Copy `.env.example` to `.env` in the repo root (or in the example project) and set `CURSOR_AGENT_PATH` before running examples.

## Edge cases and limitations

For known edge cases (path resolution, timeouts, cancellation, encoding, storage), see [docs/EDGE_CASES.md](docs/EDGE_CASES.md).

## Cursor CLI Reference

- [CLI Overview](https://cursor.com/docs/cli/overview)
- [Parameters](https://cursor.com/docs/cli/reference/parameters)
- [Output Format](https://cursor.com/docs/cli/reference/output-format) (text, json, stream-json)

## Maintenance and contributions

Maintenance time for this project is limited, but **issues are welcome**—please open one if you hit a bug or have a question. **Pull requests** are appreciated when something is really needed; I’ll try to review and merge them as often as possible. I’ll also do my best to triage and address issues when I can. Thanks for your understanding.

## License

MIT
