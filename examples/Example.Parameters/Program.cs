// Example.Parameters: Demonstrates complex AgentChatOptions (model, mode, force, resume, stream-partial, extra args).
// Optional CLI args: [--model <id>] [--mode plan|ask] [--resume <sessionId>] [--force] [--stream-partial] [--extra <arg> ...]
// Environment: CURSOR_API_KEY (optional, can also use WithApiKey).

using Capisoft.Lib.SharpCursorCli;

var (model, mode, resume, force, streamPartial, extraArgs) = ParseArgs(args);
var baseDir = "../../..";

var runner = CursorAgent.CreateRunner();

// Build options with multiple parameters: print, output format, model, mode, force, resume, working dir, prompt, extra args.
var options = new AgentChatOptions()
    .WithPrint()
    .WithWorkingDirectory(baseDir)
    .WithPrompt("Reply in one short sentence: what mode are you in and do you have force enabled?")
    .WithOutputFormat(streamPartial ? AgentOutputFormat.StreamJson : AgentOutputFormat.Json)
    .WithStreamPartialOutput(streamPartial)
    .WithModel(model)           // e.g. --model sonnet-4.5 (optional)
    .WithMode(mode)             // Agent (default), Plan, or Ask
    .WithForce(force)           // --force
    .WithResume(resume)         // --resume <sessionId> to continue a chat
    .WithApiKey(Environment.GetEnvironmentVariable("CURSOR_API_KEY"))  // optional; CLI also reads env
    .WithExtraArguments(extraArgs);  // pass-through for CLI flags not yet in the API

Console.WriteLine("Options: Model={0}, Mode={1}, Force={2}, Resume={3}, StreamPartial={4}, Extra={5}",
    model ?? "(default)", mode, force, resume ?? "(none)", streamPartial,
    extraArgs.Length > 0 ? string.Join(", ", extraArgs) : "(none)");
Console.WriteLine();

if (streamPartial)
{
    // Stream run with partial output: consume events and get final result.
    var run = runner.RunStreamWithResultAsync(options);
    await foreach (var evt in run.Stream)
    {
        if (evt is SystemInitEvent init)
            Console.WriteLine($"[init] session={init.SessionId}, cwd={init.Cwd}, model={init.Model}");
        else if (evt is AssistantMessageEvent am && am.IsDelta && !string.IsNullOrEmpty(am.Text))
            Console.Write(am.Text);
        else if (evt is TerminalResultEvent term)
            Console.WriteLine("\n[terminal] session={0}, result length={1}", term.SessionId, term.Result?.Length ?? 0);
    }
    var result = await run.ResultTask;
    Console.WriteLine("ExitCode: {0}, SessionId: {1}", result.ExitCode, result.SessionId);
    Console.WriteLine("--- Result ---");
    Console.WriteLine(result.Result ?? "(null)");
}
else
{
    var result = await runner.RunAsync(options);
    Console.WriteLine("ExitCode: {0}, SessionId: {1}, RequestId: {2}",
        result.ExitCode, result.SessionId, result.RequestId);
    if (result.DurationMs.HasValue)
        Console.WriteLine("DurationMs: {0}", result.DurationMs);
    Console.WriteLine("--- Result ---");
    Console.WriteLine(result.Result ?? "(null)");
}

return 0;

static (string? model, AgentMode mode, string? resume, bool force, bool streamPartial, string[] extraArgs) ParseArgs(string[] args)
{
    string? model = null;
    var mode = AgentMode.Agent;
    string? resume = null;
    var force = false;
    var streamPartial = false;
    var extra = new List<string>();

    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--model" when i + 1 < args.Length:
                model = args[++i];
                break;
            case "--mode" when i + 1 < args.Length:
                mode = args[++i].ToLowerInvariant() switch
                {
                    "plan" => AgentMode.Plan,
                    "ask" => AgentMode.Ask,
                    _ => AgentMode.Agent
                };
                break;
            case "--resume" when i + 1 < args.Length:
                resume = args[++i];
                break;
            case "--force":
                force = true;
                break;
            case "--stream-partial":
                streamPartial = true;
                break;
            case "--extra":
                i++;
                while (i < args.Length && !args[i].StartsWith("--"))
                    extra.Add(args[i++]);
                i--;
                break;
            default:
                if (args[i].StartsWith("--"))
                    extra.Add(args[i]);
                break;
        }
    }

    return (model, mode, resume, force, streamPartial, extra.ToArray());
}
