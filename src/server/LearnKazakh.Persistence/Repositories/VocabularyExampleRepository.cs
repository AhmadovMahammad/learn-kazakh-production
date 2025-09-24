using LearnKazakh.Core.Repositories;
using LearnKazakh.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LearnKazakh.Persistence.Repositories;

public class VocabularyExampleRepository : EfRepository<VocabularyExample>, IVocabularyExampleRepository
{
    public VocabularyExampleRepository(IHttpContextAccessor httpContextAccessor, LearnKazakhContext context) : base(httpContextAccessor, context)
    {
    }
}
