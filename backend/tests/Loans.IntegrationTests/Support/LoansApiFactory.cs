using Loans.Infrastructure.ExternalService;
using Loans.Infrastructure.Outbox;
using Loans.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Loans.IntegrationTests.Support;

/// <summary>
/// Runs the real Api in memory against a throwaway Postgres container (Testcontainers).
/// Shared by every test class through <see cref="ApiCollection"/>, so the container starts once.
/// </summary>
public sealed class LoansApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();

    /// <summary>Replaces the real external service; tests decide how it answers.</summary>
    public FakeExternalService ExternalService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Loans", _postgres.GetConnectionString());

        // No background worker: tests run the outbox processor by hand, so timing is deterministic.
        builder.UseSetting("Outbox:WorkerEnabled", "false");

        builder.ConfigureTestServices(services => services
            .AddHttpClient<ExternalServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => ExternalService)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan));
    }

    /// <summary>Runs the outbox processor until no message is due, like the worker does on one tick.</summary>
    public async Task ProcessOutboxAsync()
    {
        for (var i = 0; i < 100; i++)
        {
            await using var scope = Services.CreateAsyncScope();
            if (!await scope.ServiceProvider.GetRequiredService<OutboxProcessor>().ProcessNextAsync(CancellationToken.None))
                return;
        }

        throw new InvalidOperationException("Outbox did not drain after 100 messages.");
    }

    /// <summary>A fresh DbContext to check what the Api saved.</summary>
    public LoansDbContext CreateDbContext() =>
        Services.CreateScope().ServiceProvider.GetRequiredService<LoansDbContext>();

    /// <summary>Removes data written by earlier tests; the seeded blacklist stays.</summary>
    public async Task ResetDatabaseAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE outbox_messages, applications, customers");
        ExternalService.Reset();
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
