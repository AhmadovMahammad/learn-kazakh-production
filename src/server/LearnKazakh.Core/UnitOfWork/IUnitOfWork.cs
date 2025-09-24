using LearnKazakh.Core.Repositories;

namespace LearnKazakh.Core.UnitOfWork;

public interface IUnitOfWork
{
    object Context { get; }

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task SaveChangesAsync();

    ICategoryRepository? CategoryRepository { get; }
    ISectionRepository? SectionRepository { get; }
    IContentRepository? ContentRepository { get; }
    IVocabularyRepository? VocabularyRepository { get; }
    IVocabularyExampleRepository? VocabularyExampleRepository { get; }
    IRoleRepository? RoleRepository { get; }
}
