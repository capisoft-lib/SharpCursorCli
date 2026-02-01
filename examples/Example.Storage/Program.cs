// Example.Storage: SQLite persistence. After a run, save conversation; list GetRecentConversations / ListSessionIds; get one by session ID.

using Capisoft.Lib.SharpCursorCli;

var dbPath = "conversations_example.db";
var storage = new SqliteCursorAgentStorage(dbPath);

// 1) Run agent once and save to storage
var runner = CursorAgent.CreateRunner();
var prompt = "What is 2 + 2? Reply with one number.";
var options = new AgentChatOptions()
    .WithPrint()
    .WithWorkingDirectory("../../..")
    .WithPrompt(prompt)
    .WithOutputFormat(AgentOutputFormat.Text);

Console.WriteLine("Running agent and saving to storage...");
var result = await runner.RunAsync(options);
var preview = prompt.Length > 200 ? prompt[..200] + "..." : prompt;
storage.SaveConversation(
    result.SessionId ?? Guid.NewGuid().ToString(),
    result.RequestId,
    preview,
    metadata: null);

Console.WriteLine($"Saved SessionId={result.SessionId}");

// 2) List recent conversations and session IDs
var recent = storage.GetRecentConversations(10);
Console.WriteLine($"Recent conversations: {recent.Count}");
foreach (var r in recent)
    Console.WriteLine($"  {r.SessionId} | {r.PromptPreview?.Trim() ?? "(no preview)"}");

var sessionIds = storage.ListSessionIds();
Console.WriteLine($"ListSessionIds: {sessionIds.Count}");

// 3) Get one by session ID
if (result.SessionId != null)
{
    var one = storage.GetConversation(result.SessionId);
    Console.WriteLine(one != null ? $"GetConversation: {one.SessionId} at {one.CreatedAtUtc:O}" : "GetConversation: not found");
}
