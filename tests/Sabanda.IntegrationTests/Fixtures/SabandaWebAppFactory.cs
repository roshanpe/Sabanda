using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.IntegrationTests.Fixtures;

public class SabandaWebAppFactory : WebApplicationFactory<Program>
{
    public string ConnectionString { get; init; } = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace the DbContext registration with one using the test database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SabandaDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<SabandaDbContext>((sp, options) =>
            {
                options.UseNpgsql(ConnectionString)
                    .UseSnakeCaseNamingConvention();
            });
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override critical config for testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "test-jwt-signing-key-that-is-at-least-32-chars-long",
                ["Jwt:Issuer"] = "sabanda",
                ["Jwt:Audience"] = "sabanda",
                ["QrToken:SigningKey"] = "test-qr-token-signing-key-at-least-32chars",
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["SendGrid:ApiKey"] = "test-key",
                ["RateLimiter:LoginPerMinute"] = "100",
            });
        });
    }
}
