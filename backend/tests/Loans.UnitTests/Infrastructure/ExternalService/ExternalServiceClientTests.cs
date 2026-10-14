using System.Net;
using Loans.Infrastructure.ExternalService;
using Loans.Infrastructure.Outbox;
using Loans.UnitTests.Fakes;

namespace Loans.UnitTests.Infrastructure.ExternalService;

public class ExternalServiceClientTests
{
    private const string ApplicationId = "01a13c45-2858-751e-8393-8b6eab2ac26b";
    private const string Payload = $$"""{"applicationId":"{{ApplicationId}}","requestedAmount":100}""";

    private static OutboxMessage Message(string type) =>
        OutboxMessage.Create(Guid.NewGuid(), type, Payload, DateTimeOffset.UtcNow);

    private static ExternalServiceClient Client(StubHttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://external.test/") });

    [Fact]
    public async Task Created_IsSentAsPost_WithIdempotencyKeyAndPayload()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.Created);
        var message = Message("ApplicationCreated");

        var result = await Client(handler).SendAsync(message, CancellationToken.None);

        Assert.True(result.Succeeded);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/applications", request.Path);
        Assert.Equal(message.MessageId.ToString(), request.IdempotencyKey);
        Assert.Equal(Payload, request.Body);
    }

    [Fact]
    public async Task Updated_IsSentAsPut_ToTheApplicationsUrl()
    {
        var handler = new StubHttpMessageHandler();

        await Client(handler).SendAsync(Message("ApplicationUpdated"), CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"/applications/{ApplicationId}", request.Path);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.RequestTimeout, true)]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public async Task ErrorStatus_IsClassifiedAsRetryableOrPermanent(HttpStatusCode status, bool retryable)
    {
        var result = await Client(new StubHttpMessageHandler(status)).SendAsync(Message("ApplicationCreated"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(retryable, result.Retryable);
        Assert.StartsWith(((int)status).ToString(), result.Error);
    }

    [Fact]
    public async Task NetworkError_IsRetryable()
    {
        var handler = new StubHttpMessageHandler(throws: new HttpRequestException("Connection refused"));

        var result = await Client(handler).SendAsync(Message("ApplicationCreated"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Retryable);
        Assert.Contains("Connection refused", result.Error);
    }

    [Fact]
    public async Task Timeout_IsRetryable()
    {
        var handler = new StubHttpMessageHandler(throws: new TaskCanceledException("timeout"));

        var result = await Client(handler).SendAsync(Message("ApplicationCreated"), CancellationToken.None);

        Assert.True(result.Retryable);
        Assert.StartsWith("Timed out", result.Error);
    }
}
