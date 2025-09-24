namespace LearnKazakh.Shared.DTOs;

public class SectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class CreateSectionDto
{
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public Guid CategoryId { get; set; }
}

public class UpdateSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public Guid CategoryId { get; set; }
}

public class DetailedSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ContentCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}