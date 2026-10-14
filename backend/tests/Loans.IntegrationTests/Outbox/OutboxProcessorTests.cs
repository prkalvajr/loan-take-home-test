using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Loans.Infrastructure.Outbox;
using Loans.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Loans.IntegrationTests.Outbox;

/// <summary>The outbox processor against real Postgres, with a fake external service.</summary>
[Collection(ApiCollection.Name)]
public sealed class OutboxProcessorTests(LoansApiFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task Submit(string ssn, string firstName, decimal amount = 1_000m)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/applications", ApplicationRequests.Valid(ssn: ssn, firstName: firstName, requestedAmount: amount));
        response.EnsureSuccessStatusCode();
    }

    private async Task<List<(string Type, OutboxMessageStatus Status, int Attempts)>> OutboxRows()
    {
        await using var db = factory.CreateDbContext();
        var rows = await db.OutboxMessages.OrderBy(m => m.Id).ToListAsync();
        return rows.Select(m => (m.Type, m.Status, m.Attempts)).ToList();
    }

    // Parsed rather than matched as text: jsonb storage reformats the payload (e.g. spaces after colons).
    private static string? SsnLast4(string body) =>
        JsonDocument.Parse(body).RootElement.GetProperty("customer").GetProperty("ssnLast4").GetString();

    [Fact]
    public async Task DeliversMessagesInOrder_CreatedAsPost_UpdatedAsPut_AndMarksThemProcessed()
    {
        await Submit("111-11-1111", "Alice");
        await Submit("222-22-2222", "Bob");
        await Submit("111-11-1111", "Alice", amount: 2_000m);

        await factory.ProcessOutboxAsync();

        Assert.Equal(
            [(HttpMethod.Post, "Alice"), (HttpMethod.Post, "Bob"), (HttpMethod.Put, "Alice")],
            factory.ExternalService.Requests.Select(r => (r.Method, r.Body.Contains("Alice") ? "Alice" : "Bob")));
        Assert.All(await OutboxRows(), row =>
        {
            Assert.Equal(OutboxMessageStatus.Processed, row.Status);
            Assert.Equal(1, row.Attempts);
        });
    }

    [Fact]
    public async Task FailingCustomer_BlocksOnlyItsOwnLaterMessages()
    {
        // The external service rejects Alice (ssnLast4 1111) with a 500 and accepts Bob.
        factory.ExternalService.Respond = body =>
            SsnLast4(body) == "1111" ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;

        await Submit("111-11-1111", "Alice");
        await Submit("111-11-1111", "Alice", amount: 2_000m);
        await Submit("222-22-2222", "Bob");

        await factory.ProcessOutboxAsync();

        // Alice's update was never sent: her create has not landed yet, and a PUT would get 404.
        Assert.Equal(
            [(HttpMethod.Post, "/applications"), (HttpMethod.Post, "/applications")],
            factory.ExternalService.Requests.Select(r => (r.Method, r.Path)));
        Assert.Equal(
            [
                ("ApplicationCreated", OutboxMessageStatus.Pending, 1),
                ("ApplicationUpdated", OutboxMessageStatus.Pending, 0),
                ("ApplicationCreated", OutboxMessageStatus.Processed, 1),
            ],
            await OutboxRows());
    }

    [Fact]
    public async Task PermanentFailure_IsMarkedFailed_AndKeepsBlockingThatCustomer()
    {
        factory.ExternalService.Respond = _ => HttpStatusCode.BadRequest;

        await Submit("111-11-1111", "Alice");
        await Submit("111-11-1111", "Alice", amount: 2_000m);

        await factory.ProcessOutboxAsync();

        Assert.Single(factory.ExternalService.Requests);
        Assert.Equal(
            [
                ("ApplicationCreated", OutboxMessageStatus.Failed, 1),
                ("ApplicationUpdated", OutboxMessageStatus.Pending, 0),
            ],
            await OutboxRows());
    }
}
