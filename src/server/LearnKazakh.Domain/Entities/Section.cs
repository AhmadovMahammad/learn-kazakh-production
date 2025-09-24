using LearnKazakh.Domain.Common;
using LearnKazakh.Domain.Common.Interfaces;

namespace LearnKazakh.Domain.Entities;

public class Section : EntityBase, IAggregateRoot, IAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; } = 0;

    // Navigation properties
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Content> Contents { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}