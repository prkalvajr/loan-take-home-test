using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Loans.Domain.Customers;
using Loans.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Loans.IntegrationTests.Persistence;

/// <summary>
/// What reaches Postgres when a submission fails part way: customer, application and outbox
/// row are written in one transaction, so either all of them are saved or none are.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class SubmitApplicationTransactionTests(LoansApiFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => factory.AllowOutboxInsertsAsync();

    private Task<HttpResponseMessage> Submit(object body) => _client.PostAsJsonAsync("/api/applications", body);

    [Fact]
    public async Task OutboxInsertFails_NewApplicant_NothingIsSaved()
    {
        await factory.RejectOutboxInsertsAsync();

        var response = await Submit(ApplicationRequests.Valid());

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await using var db = factory.CreateDbContext();
        Assert.False(await db.Customers.AnyAsync());
        Assert.False(await db.Set<LoanApplication>().AnyAsync());
        Assert.False(await db.OutboxMessages.AnyAsync());
    }

    [Fact]
    public async Task OutboxInsertFails_ReturningApplicant_KeepsThePreviousData()
    {
        (await Submit(ApplicationRequests.Valid(firstName: "Jane", requestedAmount: 25_000m))).EnsureSuccessStatusCode();
        await factory.RejectOutboxInsertsAsync();

        var response = await Submit(ApplicationRequests.Valid(firstName: "Janet", requestedAmount: 40_000m));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await using var db = factory.CreateDbContext();
        var customer = await db.Customers.Include(c => c.Application).SingleAsync();
        Assert.Equal("Jane", customer.Details.FirstName);
        Assert.Equal(25_000m, customer.Application.RequestedAmount);
        Assert.Null(customer.UpdatedAt);
        Assert.Equal("ApplicationCreated", (await db.OutboxMessages.SingleAsync()).Type);
    }

    [Fact]
    public async Task DeniedReturningApplicant_ChangesNothing()
    {
        (await Submit(ApplicationRequests.Valid(firstName: "Jane", state: "TX", requestedAmount: 25_000m))).EnsureSuccessStatusCode();

        var response = await Submit(ApplicationRequests.Valid(firstName: "Janet", state: "NY", requestedAmount: 40_000m));

        Assert.Equal("denied", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("decision").GetString());
        await using var db = factory.CreateDbContext();
        var customer = await db.Customers.Include(c => c.Application).SingleAsync();
        Assert.Equal("Jane", customer.Details.FirstName);
        Assert.Equal("TX", customer.Details.Address.State);
        Assert.Equal(25_000m, customer.Application.RequestedAmount);
        Assert.Null(customer.UpdatedAt);
        Assert.Equal("ApplicationCreated", (await db.OutboxMessages.SingleAsync()).Type);
    }
}
