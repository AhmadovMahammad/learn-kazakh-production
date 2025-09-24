namespace LearnKazakh.Shared.DTOs;

public class ContentDto
{
    public Guid Id { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public string? ContentMarkdown { get; set; }
    public string? ContentHtml { get; set; }
}

public class CreateContentDto
{
    public Guid? SectionId { get; set; }
    public int Order { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public string? ContentMarkdown { get; set; }
    public string? ContentHtml { get; set; }
}

public class UpdateContentDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public string? ContentMarkdown { get; set; }
    public string? ContentHtml { get; set; }
    public Guid? SectionId { get; set; }
}

public class SectionContentDto
{
    public List<ContentDto> Contents { get; set; } = [];
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    // Navigation properties
    public bool HasPreviousSection { get; set; }
    public Guid? PreviousSectionId { get; set; }
    public string? PreviousSectionText { get; set; }

    public bool HasNextSection { get; set; }
    public Guid? NextSectionId { get; set; }
    public string? NextSectionText { get; set; }
}

public class DetailedContentDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public string? ContentMarkdown { get; set; }
    public string? ContentHtml { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionTitle { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}