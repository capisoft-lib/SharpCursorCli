namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Builds ProcessStartInfo.ArgumentList from AgentChatOptions for macOS.
/// </summary>
internal static class ArgumentBuilder
{
    public static void Build(AgentChatOptions options, List<string> outArgs)
    {
        if (options.Print)
        {
            outArgs.Add("--print");
            outArgs.Add("--output-format");
            outArgs.Add(options.OutputFormat switch
            {
                AgentOutputFormat.Text => "text",
                AgentOutputFormat.Json => "json",
                AgentOutputFormat.StreamJson => "stream-json",
                _ => "text"
            });
            if (options.StreamPartialOutput)
                outArgs.Add("--stream-partial-output");
        }

        if (!string.IsNullOrWhiteSpace(options.Resume))
        {
            outArgs.Add("--resume");
            outArgs.Add(options.Resume!.Trim());
        }
        if (!string.IsNullOrWhiteSpace(options.Model))
        {
            outArgs.Add("--model");
            outArgs.Add(options.Model!.Trim());
        }
        // Cursor CLI only accepts --mode plan or --mode ask. Omit --mode for Agent (default).
        switch (options.Mode)
        {
            case AgentMode.Plan:
                outArgs.Add("--mode");
                outArgs.Add("plan");
                break;
            case AgentMode.Ask:
                outArgs.Add("--mode");
                outArgs.Add("ask");
                break;
            default:
                // AgentMode.Agent or any other: do not pass --mode
                break;
        }
        if (options.Force)
            outArgs.Add("--force");
        if (options.Background)
            outArgs.Add("--background");
        if (options.Fullscreen)
            outArgs.Add("--fullscreen");
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            outArgs.Add("--api-key");
            outArgs.Add(options.ApiKey!.Trim());
        }
        foreach (var extra in options.ExtraArguments)
            outArgs.Add(extra);
        if (!string.IsNullOrWhiteSpace(options.Prompt))
            outArgs.Add(options.Prompt!.Trim());
    }
}
