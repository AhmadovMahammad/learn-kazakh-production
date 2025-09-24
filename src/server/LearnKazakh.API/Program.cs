using LearnKazakh.Application;
using LearnKazakh.Application.Seed;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Infrastructure.Authentication;
using LearnKazakh.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace LearnKazakh.API;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRateLimiter(options =>
            {
                // For simple cases, you can just set the status code:
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }
                    );
                });
            });

            // Add services
            builder.Services.AddOpenApi();
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.ConfigureHealthCheck(builder.Configuration);
            builder.Services.AddSignalR();

            if (builder.Configuration.GetConnectionString("DefaultConnection") is { } connectionString)
            {
                builder.Services.AddDatabase(connectionString);
            }

            builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IJwtService, JwtService>();
            builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();
            builder.Services.AddTransient<IPasswordService, PasswordService>();

            IConfigurationSection jwtSection = builder.Configuration.GetSection("Authentication:Jwt");
            JwtSettings? jwtSettings = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings are not configured properly.");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = false,

                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.IssuerSigningKey)),
                    };
                });

            builder.Services.AddAuthorization();

            // CORS - allow all origins (development mode)
            string[] allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty).Split(',');
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("wasm", policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin()      // Allow any origin, e.g., http://localhost:3000, http://example.com
                                  .AllowAnyHeader()  // Allow any header, e.g., Content-Type, Authorization
                                  .AllowAnyMethod(); // Allow any method, e.g., GET, POST, PUT, DELETE
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
                    }
                });
            });

            // -----------------------
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            using var scope = app.Services.CreateScope();
            var dataContext = scope.ServiceProvider.GetRequiredService<LearnKazakhContext>();

            await dataContext.Database.MigrateAsync();
            await builder.Services.SeedAsync(() => builder.Configuration["Authentication:DefaultPassword"] ?? string.Empty, dataContext);

            app.UseCors("wasm");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<PresenceHub>("/hubs/presence");
            app.MapGet("/", () => "Welcome to the Learn Kazakh API!");
            app.MapHealthChecks("/api/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        details = report.Entries.Select(e => new
                        {
                            key = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description
                        })
                    });

                    await context.Response.WriteAsync(result);
                }
            });

            app.UseHttpsRedirection();
            app.Run();

            //{
            //    // docker log for bind mount
            //    string filePath = "/app/logs/my-app.log";
            //    File.AppendAllText(filePath, $"{DateTime.UtcNow}: API Solution has started.\n");
            //}
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while starting the application: {ex.Message}");
            Environment.Exit(1);
        }
    }
}