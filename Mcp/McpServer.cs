using System.Text.Json;
using System.Text.Json.Nodes;
using Linkedin.Mcp.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Linkedin.Mcp.Mcp;

/// <summary>
/// MCP server that handles STDIN/STDOUT communication using JSON-RPC 2.0
/// </summary>
public class McpServer
{
    private readonly LinkedinService _linkedinService;
    private readonly ISerializer _yamlSerializer;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpServer()
    {
        _linkedinService = new LinkedinService();
        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Runs the MCP server, reading from STDIN and writing to STDOUT
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        // Check for APIFY_TOKEN on startup
        var apiToken = Environment.GetEnvironmentVariable("APIFY_TOKEN");
        if (string.IsNullOrEmpty(apiToken))
        {
            await Console.Error.WriteLineAsync("ERROR: APIFY_TOKEN environment variable is not set");
            Environment.Exit(1);
        }

        using var reader = new StreamReader(Console.OpenStandardInput());
        using var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line, _jsonOptions);
                if (request == null)
                    continue;

                var response = await HandleRequestAsync(request, cancellationToken);
                var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                await writer.WriteLineAsync(responseJson);
            }
            catch (Exception ex)
            {
                var errorResponse = new McpResponse
                {
                    Id = null,
                    Error = new McpError
                    {
                        Code = -32603,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                await writer.WriteLineAsync(errorJson);
            }
        }
    }

    private async Task<McpResponse> HandleRequestAsync(McpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            object? result = request.Method switch
            {
                "initialize" => HandleInitialize(request),
                "tools/list" => HandleListTools(),
                "tools/call" => await HandleCallToolAsync(request, cancellationToken),
                _ => throw new InvalidOperationException($"Unknown method: {request.Method}")
            };

            return new McpResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32603,
                    Message = ex.Message,
                    Data = ex.ToString()
                }
            };
        }
    }

    private InitializeResult HandleInitialize(McpRequest request)
    {
        return new InitializeResult
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolsCapability()
            },
            ServerInfo = new ServerInfo
            {
                Name = "linkedin-mcp",
                Version = "1.0.0"
            }
        };
    }

    private ListToolsResult HandleListTools()
    {
        return new ListToolsResult
        {
            Tools = new List<Tool>
            {
                new Tool
                {
                    Name = "fetch-profile",
                    Description = "Fetches a LinkedIn profile and returns it in YAML format",
                    InputSchema = new ToolInputSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, SchemaProperty>
                        {
                            ["profile_url"] = new SchemaProperty
                            {
                                Type = "string",
                                Description = "The LinkedIn profile URL (e.g., https://www.linkedin.com/in/username)"
                            }
                        },
                        Required = new List<string> { "profile_url" }
                    }
                }
            }
        };
    }

    private async Task<CallToolResult> HandleCallToolAsync(McpRequest request, CancellationToken cancellationToken)
    {
        var paramsJson = JsonSerializer.Serialize(request.Params, _jsonOptions);
        var callParams = JsonSerializer.Deserialize<CallToolParams>(paramsJson, _jsonOptions);

        if (callParams == null || callParams.Name != "fetch-profile")
        {
            throw new InvalidOperationException($"Unknown tool: {callParams?.Name}");
        }

        if (callParams.Arguments == null || !callParams.Arguments.TryGetValue("profile_url", out var profileUrlObj))
        {
            throw new ArgumentException("Missing required argument: profile_url");
        }

        var profileUrl = profileUrlObj?.ToString();
        if (string.IsNullOrEmpty(profileUrl))
        {
            throw new ArgumentException("profile_url cannot be empty");
        }

        // Fetch the profile JSON
        var profileJson = await _linkedinService.GetProfileJsonAsync(profileUrl, cancellationToken);
        if (string.IsNullOrEmpty(profileJson))
        {
            throw new InvalidOperationException("Failed to fetch LinkedIn profile");
        }

        // Parse JSON to object for YAML conversion
        var jsonObject = JsonSerializer.Deserialize<JsonNode>(profileJson);

        // Convert to YAML
        var yaml = _yamlSerializer.Serialize(jsonObject);

        return new CallToolResult
        {
            Content = new List<ToolContent>
            {
                new ToolContent
                {
                    Type = "text",
                    Text = yaml
                }
            }
        };
    }
}
