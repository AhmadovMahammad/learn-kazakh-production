using LearnKazakh.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace LearnKazakh.Application;

public static class ConfigureApplication
{
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddTransient<IJwtService, JwtService>();
        services.AddTransient<IRefreshTokenService, RefreshTokenService>();
        services.AddTransient<IPasswordService, PasswordService>();

        return services;
    }
}
