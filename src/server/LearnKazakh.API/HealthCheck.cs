using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LearnKazakh.API;

public static class HealthCheck
{
    public static void ConfigureHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        services.AddHealthChecks()
            .AddNpgSql(connectionString,
                       healthQuery: "select 1",
                       name: "PostgreSQL",
                       failureStatus: HealthStatus.Unhealthy,
                       tags: ["db"]);

        services.AddHealthChecksUI(opt =>
        {
            opt.SetEvaluationTimeInSeconds(10);
            opt.MaximumHistoryEntriesPerEndpoint(60);
            opt.SetApiMaxActiveRequests(1);

            if (configuration["HealthChecks:SelfUrl"] is string selfUrl)
            {
                opt.AddHealthCheckEndpoint("feedback api", selfUrl);
            }
        }).AddInMemoryStorage();
    }
}
