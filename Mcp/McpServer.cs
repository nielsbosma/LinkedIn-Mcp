using System.Text.Json;
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
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Runs the MCP server, reading from STDIN and writing to STDOUT
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
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

                // Notifications (messages without id) don't require a response
                if (request.Id == null)
                {
                    await Console.Error.WriteLineAsync($"[DEBUG] Received notification: {request.Method}");
                    continue;
                }

                var response = await HandleRequestAsync(request, cancellationToken);
                var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                await writer.WriteLineAsync(responseJson);
            }
            catch (Exception ex)
            {
                // Try to extract the ID from the malformed request for proper error response
                object? requestId = null;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("id", out var idProp))
                    {
                        requestId = idProp.ValueKind switch
                        {
                            JsonValueKind.String => idProp.GetString(),
                            JsonValueKind.Number => idProp.TryGetInt64(out var l) ? l : idProp.GetDouble(),
                            _ => null
                        };
                    }
                }
                catch
                {
                    // If we can't parse the ID, use null
                }

                var errorResponse = new McpResponse
                {
                    Id = requestId,
                    Error = new McpError
                    {
                        Code = -32700, // Parse error
                        Message = "Parse error",
                        Data = ex.Message
                    }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                await writer.WriteLineAsync(errorJson);
                await Console.Error.WriteLineAsync($"[ERROR] Failed to parse request: {ex.Message}");
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
                    Description = "Fetches a LinkedIn profile and returns it in YAML format. Optionally filter which optional sections to include.",
                    InputSchema = new ToolInputSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, SchemaProperty>
                        {
                            ["profile_url"] = new SchemaProperty
                            {
                                Type = "string",
                                Description = "The LinkedIn profile URL (e.g., https://www.linkedin.com/in/username)"
                            },
                            ["include"] = new SchemaProperty
                            {
                                Type = "array",
                                Description = "Optional list of sections to include. If not specified, all sections are included.",
                                Items = new SchemaItems
                                {
                                    Type = "string",
                                    Enum = new List<string>
                                    {
                                        "experiences",
                                        "updates",
                                        "profilePicAllDimensions",
                                        "skills",
                                        "educations",
                                        "licenseAndCertificates",
                                        "honorsAndAwards",
                                        "languages",
                                        "volunteerAndAwards",
                                        "verifications",
                                        "promos",
                                        "highlights",
                                        "projects",
                                        "publications",
                                        "patents",
                                        "courses",
                                        "testScores",
                                        "organizations",
                                        "volunteerCauses",
                                        "interests",
                                        "recommendations"
                                    }
                                }
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

        // Extract optional include parameter
        HashSet<string>? includeFields = null;
        if (callParams.Arguments.TryGetValue("include", out var includeObj))
        {
            if (includeObj is JsonElement includeElement && includeElement.ValueKind == JsonValueKind.Array)
            {
                includeFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in includeElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var value = item.GetString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            includeFields.Add(value);
                        }
                    }
                }
            }
        }

        // Fetch the profile JSON
        await Console.Error.WriteLineAsync($"[INFO] Fetching LinkedIn profile: {profileUrl}");
        await Console.Error.WriteLineAsync("[INFO] This may take 30-60 seconds as Apify scrapes the profile...");

        var profileJson = await _linkedinService.GetProfileJsonAsync(profileUrl, cancellationToken);
        if (string.IsNullOrEmpty(profileJson))
        {
            throw new InvalidOperationException("Failed to fetch LinkedIn profile. Check your APIFY_TOKEN (https://www.apify.com?fpr=ual7wl) and ensure the profile URL is valid.");
        }

        await Console.Error.WriteLineAsync("[INFO] Profile fetched successfully, converting to YAML...");

        // Parse JSON to native C# objects for proper YAML conversion
        using var doc = JsonDocument.Parse(profileJson);
        var jsonObject = ConvertJsonElement(doc.RootElement);

        // Filter optional fields if include parameter was provided
        if (includeFields != null)
        {
            FilterProfileFields(jsonObject, includeFields);
        }

        // Convert to YAML
        var yaml = _yamlSerializer.Serialize(jsonObject);

        await Console.Error.WriteLineAsync("[INFO] Conversion complete!");

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

    /// <summary>
    /// Converts JsonElement to native C# types for proper YAML serialization
    /// </summary>
    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => ConvertJsonElement(prop.Value)),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertJsonElement)
                .ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => null
        };
    }

    /// <summary>
    /// Filters optional profile fields based on the include list
    /// </summary>
    private static void FilterProfileFields(object? data, HashSet<string> includeFields)
    {
        // Define optional fields that can be filtered
        var optionalFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "experiences", "updates", "profilePicAllDimensions", "skills", "educations",
            "licenseAndCertificates", "honorsAndAwards", "languages", "volunteerAndAwards",
            "verifications", "promos", "highlights", "projects", "publications", "patents",
            "courses", "testScores", "organizations", "volunteerCauses", "interests",
            "recommendations"
        };

        if (data is List<object?> list)
        {
            // Process each item in the array
            foreach (var item in list)
            {
                FilterProfileFields(item, includeFields);
            }
        }
        else if (data is Dictionary<string, object?> dict)
        {
            // Remove optional fields that are not in the include list
            var keysToRemove = dict.Keys
                .Where(key => optionalFields.Contains(key) && !includeFields.Contains(key))
                .ToList();

            foreach (var key in keysToRemove)
            {
                dict.Remove(key);
            }

            // Recursively filter nested objects
            foreach (var value in dict.Values.ToList())
            {
                FilterProfileFields(value, includeFields);
            }
        }
    }
}
