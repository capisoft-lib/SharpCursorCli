// Example.RunAsync: Print mode, single run. Create runner (path from .env), build options with prompt and Print/OutputFormat,
// call RunAsync, print AgentRunResult (SessionId, RequestId, Result, DurationMs).

using Capisoft.Lib.SharpCursorCli;

var runner = CursorAgent.CreateRunner();
var options = new AgentChatOptions()
    .WithPrint()
    .WithWorkingDirectory("../../..")
    .WithPrompt("List the files in the current directory. One line only.")
    .WithOutputFormat(AgentOutputFormat.Text);

Console.WriteLine("Running agent (print mode, text output)...");
var result = await runner.RunAsync(options);

Console.WriteLine($"ExitCode: {result.ExitCode}");
Console.WriteLine($"SessionId: {result.SessionId}");
Console.WriteLine($"RequestId: {result.RequestId}");
if (result.DurationMs.HasValue)
    Console.WriteLine($"DurationMs: {result.DurationMs}");
Console.WriteLine("--- Result ---");
Console.WriteLine(result.Result ?? "(null)");
if (!string.IsNullOrEmpty(result.ErrorMessage))
    Console.WriteLine("--- Stderr ---\n" + result.ErrorMessage);
