namespace Linkedin.Mcp.Models;

/// <summary>
/// Represents a LinkedIn profile with basic information and work history.
/// </summary>
public class LinkedinProfile
{
    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the professional headline.
    /// </summary>
    public string? Headline { get; set; }

    /// <summary>
    /// Gets or sets the profile summary/about section.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the full location string.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the state/province.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the profile picture URL.
    /// </summary>
    public string? ProfilePicture { get; set; }

    /// <summary>
    /// Gets or sets the background/cover image URL.
    /// </summary>
    public string? BackgroundImage { get; set; }

    /// <summary>
    /// Gets or sets the LinkedIn username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the public identifier.
    /// </summary>
    public string? PublicIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the list of work experiences.
    /// </summary>
    public List<LinkedinExperience>? Experiences { get; set; }

    /// <summary>
    /// Gets or sets the list of educational background.
    /// </summary>
    public List<LinkedinEducation>? Education { get; set; }

    /// <summary>
    /// Gets or sets the list of skills.
    /// </summary>
    public List<string>? Skills { get; set; }

    /// <summary>
    /// Gets or sets the list of certifications.
    /// </summary>
    public List<LinkedinCertification>? Certifications { get; set; }

    /// <summary>
    /// Gets or sets the list of languages.
    /// </summary>
    public List<LinkedinLanguage>? Languages { get; set; }
}

/// <summary>
/// Represents a work experience entry on LinkedIn.
/// </summary>
public class LinkedinExperience
{
    /// <summary>
    /// Gets or sets the job title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the company name.
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Gets or sets the company LinkedIn URL.
    /// </summary>
    public string? CompanyUrl { get; set; }

    /// <summary>
    /// Gets or sets the job location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the job description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the current position.
    /// </summary>
    public bool? IsCurrent { get; set; }
}

/// <summary>
/// Represents an education entry on LinkedIn.
/// </summary>
public class LinkedinEducation
{
    /// <summary>
    /// Gets or sets the school name.
    /// </summary>
    public string? School { get; set; }

    /// <summary>
    /// Gets or sets the degree name.
    /// </summary>
    public string? Degree { get; set; }

    /// <summary>
    /// Gets or sets the field of study.
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the grade/GPA.
    /// </summary>
    public string? Grade { get; set; }

    /// <summary>
    /// Gets or sets the activities and societies.
    /// </summary>
    public string? Activities { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Represents a certification on LinkedIn.
/// </summary>
public class LinkedinCertification
{
    /// <summary>
    /// Gets or sets the certification name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the issuing authority.
    /// </summary>
    public string? Authority { get; set; }

    /// <summary>
    /// Gets or sets the license number.
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Gets or sets the issue date.
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the certification URL.
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// Represents a language skill on LinkedIn.
/// </summary>
public class LinkedinLanguage
{
    /// <summary>
    /// Gets or sets the language name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the proficiency level.
    /// </summary>
    public string? Proficiency { get; set; }
}
