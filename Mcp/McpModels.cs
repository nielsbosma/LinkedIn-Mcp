using System.Text.Json.Serialization;

namespace Linkedin.Mcp.Mcp;

public class McpRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

public class McpResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpError? Error { get; set; }
}

public class McpError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public class InitializeParams
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public ClientCapabilities? Capabilities { get; set; }

    [JsonPropertyName("clientInfo")]
    public ClientInfo? ClientInfo { get; set; }
}

public class ClientCapabilities
{
    [JsonPropertyName("roots")]
    public RootsCapability? Roots { get; set; }

    [JsonPropertyName("sampling")]
    public object? Sampling { get; set; }
}

public class RootsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

public class ClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

public class InitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("serverInfo")]
    public ServerInfo ServerInfo { get; set; } = new();
}

public class ServerCapabilities
{
    [JsonPropertyName("tools")]
    public ToolsCapability Tools { get; set; } = new();
}

public class ToolsCapability
{
}

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "linkedin-mcp";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}

public class ListToolsResult
{
    [JsonPropertyName("tools")]
    public List<Tool> Tools { get; set; } = new();
}

public class Tool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public ToolInputSchema InputSchema { get; set; } = new();
}

public class ToolInputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, SchemaProperty> Properties { get; set; } = new();

    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = new();
}

public class SchemaProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaItems? Items { get; set; }
}

public class SchemaItems
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Enum { get; set; }
}

public class CallToolParams
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }
}

public class CallToolResult
{
    [JsonPropertyName("content")]
    public List<ToolContent> Content { get; set; } = new();
}

public class ToolContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
