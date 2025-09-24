using LearnKazakh.Core.Repositories;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Persistence;
using LearnKazakh.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;

namespace LearnKazakh.Application;

public class UnitOfWork(IHttpContextAccessor contextAccessor, LearnKazakhContext context) : IUnitOfWork
{
    private readonly LearnKazakhContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private IDbContextTransaction? _dbContextTransaction;

    private ICategoryRepository? _categoryRepository;
    private ISectionRepository? _sectionRepository;
    private IContentRepository? _contentRepository;
    private IVocabularyRepository? _vocabularyRepository;
    private IVocabularyExampleRepository? _vocabularyExampleRepository;
    private IRoleRepository? _roleRepository;

    public object Context => _context;

    public async Task BeginTransactionAsync()
    {
        _dbContextTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_dbContextTransaction != null)
        {
            await _dbContextTransaction.CommitAsync();
            await _dbContextTransaction.DisposeAsync();

            _dbContextTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_dbContextTransaction != null)
        {
            await _dbContextTransaction.RollbackAsync();
            await _dbContextTransaction.DisposeAsync();

            _dbContextTransaction = null;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // Repositories with lazy loading
    public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(contextAccessor, _context);
    public ISectionRepository SectionRepository => _sectionRepository ??= new SectionRepository(contextAccessor, _context);
    public IContentRepository ContentRepository => _contentRepository ??= new ContentRepository(contextAccessor, _context);
    public IVocabularyRepository VocabularyRepository => _vocabularyRepository ??= new VocabularyRepository(contextAccessor, _context);
    public IVocabularyExampleRepository VocabularyExampleRepository => _vocabularyExampleRepository ??= new VocabularyExampleRepository(contextAccessor, _context);
    public IRoleRepository RoleRepository => _roleRepository ??= new RoleRepository(contextAccessor, _context);
}