using LearnKazakh.Core.Repositories;
using LearnKazakh.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LearnKazakh.Persistence.Repositories;

public class SectionRepository : EfRepository<Section>, ISectionRepository
{
    public SectionRepository(IHttpContextAccessor httpContextAccessor, LearnKazakhContext context) : base(httpContextAccessor, context)
    {
    }
}
