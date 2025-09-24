using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LearnKazakh.Persistence;

public static class ConfigureDatabase
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<LearnKazakhContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(LearnKazakhContext).Assembly.FullName);
            });

            options.UseSnakeCaseNamingConvention();
        });

        return services;
    }
}
