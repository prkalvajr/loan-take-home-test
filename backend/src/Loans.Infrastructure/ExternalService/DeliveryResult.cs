namespace Loans.Infrastructure.ExternalService;

/// <summary>Outcome of sending one outbox message to the external service.</summary>
public sealed record DeliveryResult(bool Succeeded, string? Error, bool Retryable)
{
    public static DeliveryResult Success() => new(true, null, false);

    /// <summary>5xx, timeout or network error: may work if tried again later.</summary>
    public static DeliveryResult RetryableFailure(string error) => new(false, error, true);

    /// <summary>4xx: the request itself is wrong, so retrying will not help.</summary>
    public static DeliveryResult PermanentFailure(string error) => new(false, error, false);
}
