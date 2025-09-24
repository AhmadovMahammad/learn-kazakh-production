using LearnKazakh.Core.Repositories;
using LearnKazakh.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LearnKazakh.Persistence.Repositories;

public class RoleRepository : EfRepository<Role>, IRoleRepository
{
    public RoleRepository(IHttpContextAccessor httpContextAccessor, LearnKazakhContext context) : base(httpContextAccessor, context)
    {
    }
}
