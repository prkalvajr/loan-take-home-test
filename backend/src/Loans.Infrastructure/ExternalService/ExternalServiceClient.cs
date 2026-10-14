using System.Net;
using System.Text;
using System.Text.Json;
using Loans.Application.Events;
using Loans.Infrastructure.Outbox;

namespace Loans.Infrastructure.ExternalService;

/// <summary>
/// Sends outbox messages to the external service: ApplicationCreated as POST /applications,
/// ApplicationUpdated as PUT /applications/{applicationId}. The message ID goes in the
/// Idempotency-Key header so a repeated delivery is recognised and ignored by the receiver.
/// </summary>
public sealed class ExternalServiceClient(HttpClient http)
{
    public async Task<DeliveryResult> SendAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        using var request = BuildRequest(message);

        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            return Classify(response);
        }
        catch (HttpRequestException e)
        {
            return DeliveryResult.RetryableFailure($"Network error: {e.Message}");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return DeliveryResult.RetryableFailure($"Timed out after {http.Timeout.TotalSeconds:0} s");
        }
    }

    private static HttpRequestMessage BuildRequest(OutboxMessage message)
    {
        var request = Enum.Parse<ApplicationEventType>(message.Type) switch
        {
            ApplicationEventType.ApplicationCreated => new HttpRequestMessage(HttpMethod.Post, "applications"),
            ApplicationEventType.ApplicationUpdated => new HttpRequestMessage(HttpMethod.Put, $"applications/{ApplicationId(message)}"),
            var type => throw new InvalidOperationException($"Unknown outbox message type '{type}'."),
        };

        request.Headers.Add("Idempotency-Key", message.MessageId.ToString());
        request.Content = new StringContent(message.Payload, Encoding.UTF8, "application/json");
        return request;
    }

    private static Guid ApplicationId(OutboxMessage message)
    {
        using var payload = JsonDocument.Parse(message.Payload);
        return payload.RootElement.GetProperty("applicationId").GetGuid();
    }

    private static DeliveryResult Classify(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return DeliveryResult.Success();

        var status = (int)response.StatusCode;
        var error = $"{status} {response.ReasonPhrase}";

        // Server errors, timeouts and rate limits can pass on a later try; other 4xx will not.
        var retryable = status >= 500
            || response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests;

        return retryable ? DeliveryResult.RetryableFailure(error) : DeliveryResult.PermanentFailure(error);
    }
}
