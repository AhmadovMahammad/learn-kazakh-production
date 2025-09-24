using LearnKazakh.Core.Repositories;
using LearnKazakh.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LearnKazakh.Persistence.Repositories;

public class ContentRepository : EfRepository<Content>, IContentRepository
{
    public ContentRepository(IHttpContextAccessor httpContextAccessor, LearnKazakhContext context) : base(httpContextAccessor, context)
    {
    }
}
