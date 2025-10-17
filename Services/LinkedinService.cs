using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Linkedin.Mcp.Models;

namespace Linkedin.Mcp.Services;

/// <summary>
/// Service for fetching LinkedIn profile data via the Apify LinkedIn Profile Scraper.
/// </summary>
/// <remarks>
/// Requires APIFY_TOKEN environment variable for API access.
/// API Documentation: https://apify.com/dev_fusion/Linkedin-Profile-Scraper
/// </remarks>
public class LinkedinService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedinService"/> class.
    /// </summary>
    public LinkedinService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.apify.com/v2/"),
            Timeout = TimeSpan.FromMinutes(5) // Apify scraping can take time
        };
        _apiKey = Environment.GetEnvironmentVariable("APIFY_TOKEN");
    }

    /// <summary>
    /// Gets raw JSON profile data from a LinkedIn profile URL using Apify scraper.
    /// </summary>
    /// <param name="profileUrl">The LinkedIn profile URL.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Raw JSON string containing profile data, or null if an error occurred.</returns>
    public async Task<string?> GetProfileJsonAsync(string profileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("APIFY_TOKEN environment variable is not set");
        }

        var normalizedUrl = NormalizeLinkedInUrl(profileUrl);
        if (string.IsNullOrEmpty(normalizedUrl))
        {
            throw new ArgumentException($"Invalid LinkedIn profile URL: {profileUrl}", nameof(profileUrl));
        }

        var request = new ApifyLinkedInRequest
        {
            ProfileUrls = new List<string> { normalizedUrl }
        };

        var endpoint = $"acts/dev_fusion~Linkedin-Profile-Scraper/run-sync-get-dataset-items?token={_apiKey}";
        var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Apify API error: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return content;
    }

    /// <summary>
    /// Normalizes a LinkedIn profile URL to a standard format.
    /// </summary>
    /// <param name="url">The LinkedIn profile URL.</param>
    /// <returns>The normalized LinkedIn URL, or null if the URL is invalid.</returns>
    /// <remarks>
    /// Supports various LinkedIn URL formats and converts them to:
    /// https://www.linkedin.com/in/username
    /// </remarks>
    private string? NormalizeLinkedInUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        url = url.Trim();

        // Extract username from various formats
        var regex = new Regex(@"(?:https?:\/\/)?(?:www\.)?linkedin\.com\/in\/([^\/\?]+)", RegexOptions.IgnoreCase);
        var match = regex.Match(url);

        if (!match.Success)
            return null;

        var username = match.Groups[1].Value;
        return $"https://www.linkedin.com/in/{username}";
    }
}

/// <summary>
/// Request model for Apify LinkedIn Profile Scraper.
/// </summary>
internal class ApifyLinkedInRequest
{
    /// <summary>
    /// Gets or sets the list of LinkedIn profile URLs to scrape.
    /// </summary>
    [JsonPropertyName("profileUrls")]
    public List<string> ProfileUrls { get; set; } = new();
}
