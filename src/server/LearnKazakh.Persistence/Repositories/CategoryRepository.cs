using LearnKazakh.Core.Repositories;
using LearnKazakh.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LearnKazakh.Persistence.Repositories;

public class CategoryRepository : EfRepository<Category>, ICategoryRepository
{
    public CategoryRepository(IHttpContextAccessor httpContextAccessor, LearnKazakhContext context) : base(httpContextAccessor, context)
    {
    }
}
