using LearnKazakh.Domain.Common;
using LearnKazakh.Domain.Common.Interfaces;

namespace LearnKazakh.Domain.Entities;

public class Category : EntityBase, IAggregateRoot, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = String.Empty;
    public string Icon { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Section> Sections { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}