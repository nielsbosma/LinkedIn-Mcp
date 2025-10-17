# LinkedIn MCP Server

A Model Context Protocol (MCP) server for fetching LinkedIn profiles via the Apify LinkedIn Profile Scraper API. Returns profile data in YAML format.

## Prerequisites

- .NET 9.0 or later
- An Apify API token ([Get one here](https://apify.com/))

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
- `include` (optional): Array of optional sections to include in the output. If not specified, all sections are included.

**Available optional sections:**
- `experiences` - Work experience history
- `updates` - Recent activity and posts
- `profilePicAllDimensions` - All profile picture dimensions
- `skills` - Skills and endorsements
- `educations` - Educational background
- `licenseAndCertificates` - Professional certifications
- `honorsAndAwards` - Honors and awards
- `languages` - Language proficiencies
- `volunteerAndAwards` - Volunteer work
- `verifications` - Profile verifications
- `promos` - Promotional content
- `highlights` - Profile highlights
- `projects` - Projects
- `publications` - Publications
- `patents` - Patents
- `courses` - Courses
- `testScores` - Test scores
- `organizations` - Organizations
- `volunteerCauses` - Volunteer causes
- `interests` - Interests
- `recommendations` - Recommendations

**Note:** Basic profile fields (name, headline, location, summary, etc.) are always included regardless of the `include` parameter.

**Example prompts:**
- "Fetch the LinkedIn profile for https://www.linkedin.com/in/username"
- "Get me the profile information for this LinkedIn URL: https://www.linkedin.com/in/username"
- "Show me the work experience from https://www.linkedin.com/in/username"
- "Fetch the profile but only include experiences and skills sections"
- "Get the profile with just educations and certifications"

**Supported URL formats:**
- `https://www.linkedin.com/in/username`
- `https://linkedin.com/in/username`
- `http://www.linkedin.com/in/username`
- `linkedin.com/in/username`

## Output Format

The tool returns LinkedIn profile data in YAML format.

**Always included fields:**
- **Personal Information**: Name, headline, location, profile picture
- **Professional Summary**: About/summary section
- **Contact Information**: LinkedIn URL, public identifier

**Optional sections** (included by default, can be filtered with the `include` parameter):
- **Work Experience**: Job titles, companies, dates, descriptions
- **Education**: Schools, degrees, fields of study
- **Skills**: List of professional skills and endorsements
- **Certifications**: Professional certifications and licenses
- **Languages**: Language proficiencies
- **Projects**: Personal and professional projects
- **Publications**: Published works
- **Patents**: Patent information
- **And more**: Updates, honors, awards, recommendations, etc.

Use the `include` parameter to fetch only the sections you need, reducing response size and processing time.