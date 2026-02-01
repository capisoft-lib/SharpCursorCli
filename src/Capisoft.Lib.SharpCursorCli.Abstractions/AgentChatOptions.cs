namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Options for running the Cursor Agent in chat (or print) mode.
/// Fluent builder; maps to global CLI flags. Use <see cref="WithPrint"/> for non-interactive/script usage.
/// </summary>
public sealed class AgentChatOptions
{
    /// <summary>Use print mode (non-interactive); required for scripts/automation.</summary>
    public bool Print { get; private set; }

    /// <summary>Output format when printing (text, json, stream-json).</summary>
    public AgentOutputFormat OutputFormat { get; private set; } = AgentOutputFormat.Text;

    /// <summary>When using StreamJson, stream partial output as text deltas.</summary>
    public bool StreamPartialOutput { get; private set; }

    /// <summary>Resume an existing chat by session/chat ID.</summary>
    public string? Resume { get; private set; }

    /// <summary>Model to use (e.g. gpt-5.2, sonnet-4.5).</summary>
    public string? Model { get; private set; }

    /// <summary>Agent mode (agent, plan, ask).</summary>
    public AgentMode Mode { get; private set; } = AgentMode.Agent;

    /// <summary>Force allow commands unless explicitly denied.</summary>
    public bool Force { get; private set; }

    /// <summary>Start in background mode.</summary>
    public bool Background { get; private set; }

    /// <summary>Enable fullscreen.</summary>
    public bool Fullscreen { get; private set; }

    /// <summary>API key (or use CURSOR_API_KEY env).</summary>
    public string? ApiKey { get; private set; }

    /// <summary>Working directory for the agent process.</summary>
    public string? WorkingDirectory { get; private set; }

    /// <summary>Initial prompt when starting in chat mode.</summary>
    public string? Prompt { get; private set; }

    /// <summary>Extra argument strings for future/unmapped flags (appended to ArgumentList).</summary>
    public IReadOnlyList<string> ExtraArguments { get; private set; } = Array.Empty<string>();

    /// <summary>Sets print mode (non-interactive).</summary>
    public AgentChatOptions WithPrint(bool value = true)
    {
        Print = value;
        return this;
    }

    /// <summary>Sets the output format (only applies with print).</summary>
    public AgentChatOptions WithOutputFormat(AgentOutputFormat format)
    {
        OutputFormat = format;
        return this;
    }

    /// <summary>Enables stream-partial-output (only with StreamJson).</summary>
    public AgentChatOptions WithStreamPartialOutput(bool value = true)
    {
        StreamPartialOutput = value;
        return this;
    }

    /// <summary>Resume by chat/session ID.</summary>
    public AgentChatOptions WithResume(string? chatId)
    {
        Resume = chatId;
        return this;
    }

    /// <summary>Sets the model.</summary>
    public AgentChatOptions WithModel(string? model)
    {
        Model = model;
        return this;
    }

    /// <summary>Sets the agent mode.</summary>
    public AgentChatOptions WithMode(AgentMode mode)
    {
        Mode = mode;
        return this;
    }

    /// <summary>Sets force flag.</summary>
    public AgentChatOptions WithForce(bool value = true)
    {
        Force = value;
        return this;
    }

    /// <summary>Sets background flag.</summary>
    public AgentChatOptions WithBackground(bool value = true)
    {
        Background = value;
        return this;
    }

    /// <summary>Sets fullscreen flag.</summary>
    public AgentChatOptions WithFullscreen(bool value = true)
    {
        Fullscreen = value;
        return this;
    }

    /// <summary>Sets API key.</summary>
    public AgentChatOptions WithApiKey(string? apiKey)
    {
        ApiKey = apiKey;
        return this;
    }

    /// <summary>Sets working directory for the process.</summary>
    public AgentChatOptions WithWorkingDirectory(string? workingDirectory)
    {
        WorkingDirectory = workingDirectory;
        return this;
    }

    /// <summary>Sets the initial prompt (positional argument in chat mode).</summary>
    public AgentChatOptions WithPrompt(string? prompt)
    {
        Prompt = prompt;
        return this;
    }

    /// <summary>Sets extra argument strings (e.g. for future CLI flags). Replaces any previously set extra arguments.</summary>
    public AgentChatOptions WithExtraArguments(params string[] args)
    {
        ExtraArguments = args.Length > 0 ? args.ToList() : Array.Empty<string>();
        return this;
    }
}
