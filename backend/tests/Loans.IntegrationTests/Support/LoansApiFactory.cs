using Loans.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Loans", _postgres.GetConnectionString());
    }

    /// <summary>A fresh DbContext to check what the Api saved.</summary>
    public LoansDbContext CreateDbContext() =>
        Services.CreateScope().ServiceProvider.GetRequiredService<LoansDbContext>();

    /// <summary>Removes data written by earlier tests; the seeded blacklist stays.</summary>
    public async Task ResetDatabaseAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE outbox_messages, applications, customers");
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
