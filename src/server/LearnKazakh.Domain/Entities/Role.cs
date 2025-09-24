using LearnKazakh.Domain.Common;
using LearnKazakh.Domain.Common.Interfaces;

namespace LearnKazakh.Domain.Entities;

public class Role : EntityBase, IAggregateRoot, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}