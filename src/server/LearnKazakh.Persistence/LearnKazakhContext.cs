using LearnKazakh.Domain.Common.Interfaces;
using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace LearnKazakh.Persistence;

public class LearnKazakhContext(DbContextOptions<LearnKazakhContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<Vocabulary> Vocabularies { get; set; }
    public DbSet<VocabularyExample> VocabularyExamples { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Writing this query for all entities.
        // modelBuilder.Entity<TEntity:ISoftDeletable>().HasQueryFilter(e => !e.IsDeleted);

        // With the lack of custom conventions,
        // you could use the typical modelBuilder.Model.GetEntityTypes() loop,
        // identify the target entity types and invoke common configuration. (stackoverflow)

        List<Type> skippedTypes = [typeof(User)];

        foreach (IMutableEntityType mutableEntityType in modelBuilder.Model.GetEntityTypes())
        {
            if (skippedTypes.Contains(mutableEntityType.ClrType))
            {
                continue;
            }

            Type clrType = mutableEntityType.ClrType;

            if (typeof(ISoftDeletable).IsAssignableFrom(clrType)
                && typeof(ModelBuilder).GetMethod("Entity", Type.EmptyTypes)?.MakeGenericMethod(clrType) is { } methodInfo)
            {
                object? entityTypeBuilder = methodInfo.Invoke(modelBuilder, null);
                if (entityTypeBuilder == null)
                {
                    continue;
                }

                ParameterExpression parameterExpression = Expression.Parameter(clrType, "e");
                MemberExpression memberExpression = Expression.Property(parameterExpression, nameof(ISoftDeletable.IsDeleted));
                BinaryExpression binaryExpression = Expression.Equal(memberExpression, Expression.Constant(false));
                LambdaExpression lambdaExpression = Expression.Lambda(binaryExpression, parameterExpression);

                MethodInfo? hasQueryFilterMethod = entityTypeBuilder.GetType()
                    .GetMethods()
                    .FirstOrDefault(m =>
                        m.Name == "HasQueryFilter" &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType == typeof(LambdaExpression));

                hasQueryFilterMethod?.Invoke(entityTypeBuilder, [lambdaExpression]);
            }
        }

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearnKazakhContext).Assembly);
    }
}
