namespace LearnKazakh.Domain.Common;

public class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
}