# LinkedIn MCP Server

A Model Context Protocol (MCP) server for fetching LinkedIn profiles via the Apify LinkedIn Profile Scraper API. Returns profile data in YAML format.

## Prerequisites

- .NET 9.0 or later
- An Apify API token ([Get one here](https://apify.com/dev_fusion/Linkedin-Profile-Scraper))

## Installation

Install the tool globally using dotnet:

```bash
dotnet tool install --global Linkedin.Mcp
```

## Configuration

### 1. Set up your Apify API token

The tool requires the `APIFY_TOKEN` environment variable to be set.

**Windows (PowerShell):**
```powershell
$env:APIFY_TOKEN = "your-apify-token-here"
# To persist across sessions:
[System.Environment]::SetEnvironmentVariable('APIFY_TOKEN', 'your-apify-token-here', 'User')
```

**Linux/macOS:**
```bash
export APIFY_TOKEN=your-apify-token-here
# To persist, add to ~/.bashrc or ~/.zshrc:
echo 'export APIFY_TOKEN=your-apify-token-here' >> ~/.bashrc
```

### 2. Configure Claude Code

Add the MCP server to your Claude Code configuration file.

**Location:**
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`
- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`
- Linux: `~/.config/Claude/claude_desktop_config.json`

**Configuration:**
```json
{
  "mcpServers": {
    "linkedin": {
      "command": "linkedin-mcp",
      "env": {
        "APIFY_TOKEN": "your-apify-token-here"
      }
    }
  }
}
```

Alternatively, if you've set the environment variable globally, you can omit the `env` section:

```json
{
  "mcpServers": {
    "linkedin": {
      "command": "linkedin-mcp"
    }
  }
}
```

## Usage in Claude Code

Once configured, restart Claude Code. The LinkedIn MCP server will be available with the following tool:

### `fetch-profile`

Fetches a LinkedIn profile and returns it in YAML format.

**Parameters:**
- `profile_url` (required): The LinkedIn profile URL

**Example prompts:**
- "Fetch the LinkedIn profile for https://www.linkedin.com/in/username"
- "Get me the profile information for this LinkedIn URL: https://www.linkedin.com/in/username"
- "Show me the work experience from https://www.linkedin.com/in/username"

**Supported URL formats:**
- `https://www.linkedin.com/in/username`
- `https://linkedin.com/in/username`
- `http://www.linkedin.com/in/username`
- `linkedin.com/in/username`

## Output Format

The tool returns LinkedIn profile data in YAML format, including:

- **Personal Information**: Name, headline, location, profile picture
- **Professional Summary**: About/summary section
- **Work Experience**: Job titles, companies, dates, descriptions
- **Education**: Schools, degrees, fields of study
- **Skills**: List of professional skills
- **Certifications**: Professional certifications and licenses
- **Languages**: Language proficiencies

## Credits

This tool uses the [Apify LinkedIn Profile Scraper](https://apify.com/dev_fusion/Linkedin-Profile-Scraper) API to fetch LinkedIn profile data.