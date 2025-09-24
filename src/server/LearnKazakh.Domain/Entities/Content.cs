using LearnKazakh.Domain.Common;
using LearnKazakh.Domain.Common.Interfaces;

namespace LearnKazakh.Domain.Entities;

public class Content : EntityBase, IAggregateRoot, IAuditableEntity
{
    public int Order { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public string? ContentMarkdown { get; set; }
    public string? ContentHtml { get; set; }

    // Navigation properties
    public Guid? SectionId { get; set; }
    public Section? Section { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}