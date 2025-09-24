using LearnKazakh.Domain.Common;
using LearnKazakh.Domain.Common.Interfaces;

namespace LearnKazakh.Domain.Entities;

public class RefreshToken : EntityBase, IAggregateRoot
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    public string? RevokedReason { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public string? RevokedByIp { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => IsExpired && !IsRevoked;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
