using LearnKazakh.Domain.Common;
using LearnKazakh.Domain.Common.Interfaces;
using LearnKazakh.Shared.Enums;

namespace LearnKazakh.Domain.Entities;

public class Vocabulary : EntityBase, IAggregateRoot
{
    public string WordKazakh { get; set; } = string.Empty;
    public string WordAzerbaijani { get; set; } = string.Empty;
    public string? Pronounciation { get; set; }
    public string? AudioUrl { get; set; }
    public VocabularyType Type { get; set; } = VocabularyType.All;

    // Navigation properties
    public ICollection<VocabularyExample> VocabularyExamples { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}