// Example.Resume: Resume by session ID. First call creates a session; second call reuses its SessionId with WithResume(sessionId).
// Call 1 uses stream-json so we get SessionId from the terminal result event (print+json may not emit session_id on all CLI versions).

using Capisoft.Lib.SharpCursorCli;

var runner = CursorAgent.CreateRunner();
var baseDir = "../../..";

// First call: create a session (no WithResume). Use stream-json so we get SessionId from the final result.
Console.WriteLine("--- Call 1: Create session ---");
var options1 = new AgentChatOptions()
    .WithPrint()
    .WithWorkingDirectory(baseDir)
    .WithPrompt("Remember this: we are testing the resume example. Reply with OK.")
    .WithOutputFormat(AgentOutputFormat.StreamJson);

var run1 = runner.RunStreamWithResultAsync(options1);
await foreach (var _ in run1.Stream) { /* consume stream */ }
var result1 = await run1.ResultTask;
Console.WriteLine($"ExitCode: {result1.ExitCode}");
Console.WriteLine($"SessionId: {result1.SessionId}");
Console.WriteLine("--- Result ---");
Console.WriteLine(result1.Result ?? "(null)");

var sessionId = result1.SessionId;
if (string.IsNullOrWhiteSpace(sessionId))
{
    Console.WriteLine("No SessionId from first run; cannot resume.");
    return 1;
}

// Second call: resume reusing the id from the first call
Console.WriteLine();
Console.WriteLine($"--- Call 2: Resume session {sessionId} ---");
var options2 = new AgentChatOptions()
    .WithPrint()
    .WithWorkingDirectory(baseDir)
    .WithResume(sessionId)
    .WithPrompt("Continue from where we left off: summarize what we did in one line.")
    .WithOutputFormat(AgentOutputFormat.Text);

var result2 = await runner.RunAsync(options2);
Console.WriteLine($"ExitCode: {result2.ExitCode}");
Console.WriteLine($"SessionId: {result2.SessionId}");
Console.WriteLine("--- Result ---");
Console.WriteLine(result2.Result ?? "(null)");
return 0;
