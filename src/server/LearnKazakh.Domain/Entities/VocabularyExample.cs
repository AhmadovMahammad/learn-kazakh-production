using LearnKazakh.Domain.Common;
using LearnKazakh.Domain.Common.Interfaces;

namespace LearnKazakh.Domain.Entities;

public class VocabularyExample : EntityBase, IAggregateRoot
{
    public string SentenceKazakh { get; set; } = string.Empty;
    public string SentenceTranslation { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }

    // Navigation properties
    public Guid? VocabularyId { get; set; }
    public Vocabulary? Vocabulary { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}