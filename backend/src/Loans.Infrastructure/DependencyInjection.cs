using Loans.Application.Abstractions;
using Loans.Infrastructure.Blacklist;
using Loans.Infrastructure.ExternalService;
using Loans.Infrastructure.Outbox;
using Loans.Infrastructure.Persistence;
using Loans.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Loans.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, Uri externalServiceBaseUrl)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditTimestampsInterceptor>();

        services.AddDbContext<LoansDbContext>((provider, options) => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(provider.GetRequiredService<AuditTimestampsInterceptor>()));

        // The DbContext is the unit of work, so the repository and outbox share its transaction.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LoansDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISsnBlacklist, SsnBlacklist>();
        services.AddScoped<IOutbox, OutboxWriter>();

        services.AddHttpClient<ExternalServiceClient>(http =>
        {
            http.BaseAddress = externalServiceBaseUrl;
            http.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddScoped<OutboxProcessor>();

        return services;
    }

    /// <summary>
    /// Starts the background worker that delivers outbox messages. Separate from
    /// <see cref="AddInfrastructure"/> so tests can run the processor by hand instead.
    /// </summary>
    public static IServiceCollection AddOutboxWorker(this IServiceCollection services) =>
        services.AddHostedService<OutboxBackgroundService>();

    /// <summary>
    /// Creates or upgrades the database schema. Run on startup so `docker compose up` needs no
    /// manual step; production would run migrations as a separate deployment step instead.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoansDbContext>();
        await db.Database.MigrateAsync();
    }
}
