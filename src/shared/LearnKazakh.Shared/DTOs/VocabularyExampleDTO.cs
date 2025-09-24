namespace LearnKazakh.Shared.DTOs;

public class VocabularyExampleDto
{
    public Guid Id { get; set; }
    public string SentenceKazakh { get; set; } = string.Empty;
    public string SentenceTranslation { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
}

public class CreateVocabularyExampleDto
{
    public string SentenceKazakh { get; set; } = string.Empty;
    public string SentenceTranslation { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public Guid? VocabularyId { get; set; }
}

public class UpdateVocabularyExampleDto
{
    public Guid Id { get; set; }
    public string SentenceKazakh { get; set; } = string.Empty;
    public string SentenceTranslation { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public Guid? VocabularyId { get; set; }
}

public class DetailedVocabularyExampleDto
{
    public Guid Id { get; set; }
    public string SentenceKazakh { get; set; } = string.Empty;
    public string SentenceTranslation { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}