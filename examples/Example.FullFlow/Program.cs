// Example.FullFlow: End-to-end. Run with prompt -> save to storage -> later resume by session ID from storage.

using Capisoft.Lib.SharpCursorCli;

var dbPath = "conversations_fullflow.db";
var storage = new SqliteCursorAgentStorage(dbPath);
var runner = CursorAgent.CreateRunner();

// Step 1: Run with prompt
var prompt = "Reply with exactly: OK";
// Json output is required so RunAsync can parse session_id/request_id for save/resume.
var options = new AgentChatOptions()
    .WithPrint()
    .WithPrompt(prompt)
    .WithOutputFormat(AgentOutputFormat.Json);

Console.WriteLine("1) Run agent...");
var result = await runner.RunAsync(options);
var sessionId = result.SessionId;
if (string.IsNullOrEmpty(sessionId))
{
    Console.WriteLine("No SessionId returned; cannot save/resume.");
    return 1;
}

// Step 2: Save to storage
var preview = prompt.Length > 200 ? prompt[..200] + "..." : prompt;
storage.SaveConversation(sessionId, result.RequestId, preview);
Console.WriteLine($"2) Saved SessionId={sessionId}");

// Step 3: Later: resume by session ID from storage
var loaded = storage.GetConversation(sessionId);
if (loaded == null)
{
    Console.WriteLine("3) Could not load conversation from storage.");
    return 1;
}

var resumeOptions = new AgentChatOptions()
    .WithPrint()
    .WithResume(loaded.SessionId)
    .WithPrompt("What did I just ask? One line.")
    .WithOutputFormat(AgentOutputFormat.Json);

Console.WriteLine("3) Resume same session...");
var resumeResult = await runner.RunAsync(resumeOptions);
Console.WriteLine("--- Resumed result ---");
Console.WriteLine(resumeResult.Result ?? "(null)");
Console.WriteLine("Done.");
return 0;
