using System.Text.Json;

namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Parses NDJSON lines from the CLI stream-json output into StreamEvent records (Linux).
/// </summary>
internal static class StreamJsonParser
{
    public static StreamEvent? TryParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            if (!root.TryGetProperty("type", out var typeEl))
                return null;
            var type = typeEl.GetString();
            var sessionId = root.TryGetProperty("session_id", out var sid) ? sid.GetString() : null;

            switch (type)
            {
                case "system":
                    if (root.TryGetProperty("subtype", out var st) && st.GetString() == "init")
                        return new SystemInitEvent
                        {
                            SessionId = sessionId,
                            Cwd = root.TryGetProperty("cwd", out var cwd) ? cwd.GetString() : null,
                            Model = root.TryGetProperty("model", out var model) ? model.GetString() : null,
                            ApiKeySource = root.TryGetProperty("apiKeySource", out var aks) ? aks.GetString() : null,
                            PermissionMode = root.TryGetProperty("permissionMode", out var pm) ? pm.GetString() : null
                        };
                    break;
                case "user":
                    var userText = ExtractMessageText(root, "message");
                    return new UserMessageEvent { SessionId = sessionId, Text = userText };
                case "assistant":
                    var assistantText = ExtractMessageText(root, "message");
                    var isDelta = root.TryGetProperty("subtype", out var ast) && ast.GetString() == "delta";
                    return new AssistantMessageEvent { SessionId = sessionId, Text = assistantText, IsDelta = isDelta };
                case "tool_call":
                    var subtype = root.TryGetProperty("subtype", out var sub) ? sub.GetString() : null;
                    var callId = root.TryGetProperty("call_id", out var cid) ? cid.GetString() : null;
                    return new ToolCallEvent
                    {
                        SessionId = sessionId,
                        IsCompleted = subtype == "completed",
                        CallId = callId,
                        RawJson = line
                    };
                case "result":
                    if (root.TryGetProperty("subtype", out var resSub) && resSub.GetString() == "success")
                        return new TerminalResultEvent
                        {
                            SessionId = sessionId,
                            RequestId = root.TryGetProperty("request_id", out var rid) ? rid.GetString() : null,
                            Result = root.TryGetProperty("result", out var res) ? res.GetString() : null,
                            DurationMs = root.TryGetProperty("duration_ms", out var dm) ? dm.GetInt64() : null,
                            DurationApiMs = root.TryGetProperty("duration_api_ms", out var dam) ? dam.GetInt64() : null,
                            IsError = root.TryGetProperty("is_error", out var ie) && ie.GetBoolean()
                        };
                    break;
            }
        }
        catch
        {
            // Ignore parse errors for unknown lines
        }
        return null;
    }

    private static string? ExtractMessageText(JsonElement root, string messageKey)
    {
        if (!root.TryGetProperty(messageKey, out var msg))
            return null;
        if (!msg.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return null;
        var parts = new List<string>();
        foreach (var item in content.EnumerateArray())
        {
            if (item.TryGetProperty("type", out var t) && t.GetString() == "text" && item.TryGetProperty("text", out var text))
                parts.Add(text.GetString() ?? "");
        }
        return parts.Count > 0 ? string.Join("", parts) : null;
    }

    public static AgentRunResult? ParseFinalJson(string json, int exitCode, string? stderr)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new AgentRunResult { ExitCode = exitCode, ErrorMessage = stderr };
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var sessionId = root.TryGetProperty("session_id", out var sid) ? sid.GetString() : null;
            var requestId = root.TryGetProperty("request_id", out var rid) ? rid.GetString() : null;
            var result = root.TryGetProperty("result", out var res) ? res.GetString() : null;
            long? durationMs = root.TryGetProperty("duration_ms", out var dm) ? dm.GetInt64() : null;
            long? durationApiMs = root.TryGetProperty("duration_api_ms", out var dam) ? dam.GetInt64() : null;
            var isError = root.TryGetProperty("is_error", out var ie) && ie.GetBoolean();
            return new AgentRunResult
            {
                SessionId = sessionId,
                RequestId = requestId,
                Result = result,
                DurationMs = durationMs,
                DurationApiMs = durationApiMs,
                IsError = isError,
                ExitCode = exitCode,
                ErrorMessage = stderr
            };
        }
        catch
        {
            return new AgentRunResult { ExitCode = exitCode, ErrorMessage = stderr, Result = json };
        }
    }
}
