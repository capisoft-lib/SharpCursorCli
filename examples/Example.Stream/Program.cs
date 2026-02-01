// Example.Stream: Stream mode. Options with OutputFormat.StreamJson and optional StreamPartialOutput;
// await foreach over RunStreamAsync; handle SystemInitEvent, AssistantMessageEvent, TerminalResultEvent.
// Deduplicates assistant output when the CLI sends both incremental deltas and a final full message.

using Capisoft.Lib.SharpCursorCli;

var runner = CursorAgent.CreateRunner();
var options = new AgentChatOptions()
    .WithPrint()
    .WithWorkingDirectory("../../..")
    .WithPrompt("Write a long poem to test the stream Cursor Wrapper. Use at least 15 stanzas of 4 lines each (60+ lines). Theme: the journey of code from idea to running program. Use rhyme and meter. Print the full poem and nothing else.")
    .WithOutputFormat(AgentOutputFormat.StreamJson)
    .WithStreamPartialOutput();

Console.WriteLine("Streaming agent events...");
string? sessionId = null;
var printedSoFar = ""; // Dedupe: skip final full message when it matches already-printed content
await foreach (var evt in runner.RunStreamAsync(options))
{
    switch (evt)
    {
        case SystemInitEvent init:
            sessionId = init.SessionId;
            printedSoFar = "";
            Console.WriteLine($"[Init] SessionId={init.SessionId}, Cwd={init.Cwd}, Model={init.Model}");
            break;
        case AssistantMessageEvent assistant when !string.IsNullOrEmpty(assistant.Text):
            var text = assistant.Text;
            if (text == printedSoFar)
                break; // Duplicate full message; already printed via deltas
            if (printedSoFar.Length > 0 && text.StartsWith(printedSoFar, StringComparison.Ordinal))
            {
                Console.Write(text.Substring(printedSoFar.Length));
                printedSoFar = text;
            }
            else
            {
                Console.Write(text);
                printedSoFar = assistant.IsDelta ? printedSoFar + text : text;
            }
            break;
        case ToolCallEvent:
            printedSoFar = ""; // New assistant segment may follow
            break;
        case TerminalResultEvent term:
            printedSoFar = "";
            Console.WriteLine();
            Console.WriteLine($"[Result] SessionId={term.SessionId}, RequestId={term.RequestId}, DurationMs={term.DurationMs}");
            break;
        default:
            break;
    }
}
Console.WriteLine("Done.");
