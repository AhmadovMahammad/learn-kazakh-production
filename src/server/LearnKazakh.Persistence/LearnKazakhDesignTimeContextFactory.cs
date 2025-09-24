using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LearnKazakh.Persistence;

internal class LearnKazakhDesignTimeContextFactory : IDesignTimeDbContextFactory<LearnKazakhContext>
{
    public LearnKazakhContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<LearnKazakhContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<LearnKazakhContext>();

        string connectionString = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING")
            ?? throw new InvalidOperationException("Design Time Connection String is not defined.");

        dbContextOptionsBuilder.UseNpgsql(connectionString, npgsqloptions =>
        {
            npgsqloptions.MigrationsAssembly(typeof(LearnKazakhContext).Assembly.FullName);
        }).UseSnakeCaseNamingConvention();

        return new LearnKazakhContext(dbContextOptionsBuilder.Options);
    }
}
