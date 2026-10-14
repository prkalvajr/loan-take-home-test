using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Loans.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Loans.IntegrationTests.Endpoints;

/// <summary>POST /api/applications through the real Api, use case and Postgres.</summary>
[Collection(ApiCollection.Name)]
public sealed class SubmitApplicationEndpointTests(LoansApiFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private Task<HttpResponseMessage> Submit(object body) => _client.PostAsJsonAsync("/api/applications", body);

    private static async Task<JsonElement> Json(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    [Fact]
    public async Task NewApplicant_IsApproved_AndSavesCustomerApplicationAndCreatedEvent()
    {
        var response = await Submit(ApplicationRequests.Valid());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await Json(response);
        Assert.Equal("approved", body.GetProperty("decision").GetString());
        Assert.False(body.GetProperty("isReturningCustomer").GetBoolean());

        await using var db = factory.CreateDbContext();
        var customer = await db.Customers.Include(c => c.Application).SingleAsync();
        Assert.Equal(body.GetProperty("applicationId").GetGuid(), customer.Application.Id);
        Assert.Equal(25_000m, customer.Application.RequestedAmount);
        var message = await db.OutboxMessages.SingleAsync();
        Assert.Equal("ApplicationCreated", message.Type);
        Assert.Equal(customer.Id, message.CustomerId);
    }

    [Fact]
    public async Task ReturningApplicant_UpdatesTheSameCustomerAndApplication()
    {
        var first = await Json(await Submit(ApplicationRequests.Valid(firstName: "Jane", requestedAmount: 25_000m)));

        var response = await Submit(ApplicationRequests.Valid(firstName: "Janet", state: "CA", requestedAmount: 40_000m));

        var second = await Json(response);
        Assert.Equal("approved", second.GetProperty("decision").GetString());
        Assert.True(second.GetProperty("isReturningCustomer").GetBoolean());
        Assert.Equal(first.GetProperty("applicationId").GetGuid(), second.GetProperty("applicationId").GetGuid());
        Assert.Equal(first.GetProperty("customerId").GetGuid(), second.GetProperty("customerId").GetGuid());

        await using var db = factory.CreateDbContext();
        var customer = await db.Customers.Include(c => c.Application).SingleAsync();
        Assert.Equal("Janet", customer.Details.FirstName);
        Assert.Equal("CA", customer.Details.Address.State);
        Assert.Equal(40_000m, customer.Application.RequestedAmount);
        Assert.NotNull(customer.UpdatedAt);
        Assert.Equal(1, await db.Set<Loans.Domain.Customers.LoanApplication>().CountAsync());
        Assert.Equal(
            ["ApplicationCreated", "ApplicationUpdated"],
            await db.OutboxMessages.OrderBy(m => m.Id).Select(m => m.Type).ToListAsync());
    }

    [Theory]
    [InlineData("NY", "123-45-6789", new[] { "STATE_NOT_ALLOWED" })]
    [InlineData("TX", ApplicationRequests.BlacklistedSsn, new[] { "SSN_BLACKLISTED" })]
    [InlineData("NY", ApplicationRequests.BlacklistedSsn, new[] { "STATE_NOT_ALLOWED", "SSN_BLACKLISTED" })]
    public async Task DeniedApplicant_GetsReasonCodes_AndNothingIsSaved(string state, string ssn, string[] expectedReasons)
    {
        var response = await Submit(ApplicationRequests.Valid(ssn: ssn, state: state));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await Json(response);
        Assert.Equal("denied", body.GetProperty("decision").GetString());
        Assert.Equal(expectedReasons, body.GetProperty("reasons").EnumerateArray().Select(r => r.GetString()));

        await using var db = factory.CreateDbContext();
        Assert.False(await db.Customers.AnyAsync());
        Assert.False(await db.OutboxMessages.AnyAsync());
    }

    [Fact]
    public async Task InvalidFields_Return400_WithErrorsPerField()
    {
        var response = await Submit(new
        {
            firstName = "",
            lastName = "Doe",
            companyName = "Acme LLC",
            address = new { street = "100 Main St", city = "Austin", state = "XX", zipCode = "123" },
            requestedAmount = 10.555m,
            ssn = "12",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await Json(response)).GetProperty("errors");
        Assert.Equal(
            ["Address.State", "Address.ZipCode", "FirstName", "RequestedAmount", "Ssn"],
            errors.EnumerateObject().Select(e => e.Name).Order());
    }

    [Fact]
    public async Task MalformedBody_Returns400()
    {
        var response = await _client.PostAsync(
            "/api/applications", new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
