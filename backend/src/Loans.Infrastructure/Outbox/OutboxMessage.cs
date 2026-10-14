namespace Loans.Infrastructure.Outbox;

/// <summary>
/// An event waiting to be delivered to the external service. Written in the same transaction
/// as the customer and application, then sent later by the background worker.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Database identity; gives the processing order.</summary>
    public long Id { get; private set; }

    /// <summary>Sent as the Idempotency-Key header, so the receiver can ignore repeats.</summary>
    public Guid MessageId { get; private set; }

    /// <summary>Lets the worker hold back a customer's later messages while an earlier one is pending or failed.</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>ApplicationCreated or ApplicationUpdated.</summary>
    public string Type { get; private set; } = null!;

    /// <summary>JSON snapshot of the customer and application at commit time.</summary>
    public string Payload { get; private set; } = null!;

    public OutboxMessageStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    // Used by EF Core when loading from the database.
    private OutboxMessage() { }

    public static OutboxMessage Create(Guid customerId, string type, string payload, DateTimeOffset now) => new()
    {
        MessageId = Guid.CreateVersion7(),
        CustomerId = customerId,
        Type = type,
        Payload = payload,
        Status = OutboxMessageStatus.Pending,
        NextAttemptAt = now,
        CreatedAt = now,
    };

    /// <summary>After this many failed attempts the message is marked Failed (about 30 s of retries).</summary>
    public const int MaxAttempts = 5;

    private const int MaxErrorLength = 2000;

    /// <summary>Wait before the next try: 2 s, 4 s, 8 s, 16 s after attempts 1 to 4.</summary>
    public static TimeSpan RetryDelay(int failedAttempts) => TimeSpan.FromSeconds(Math.Pow(2, failedAttempts));

    public void MarkProcessed(DateTimeOffset now)
    {
        Attempts++;
        Status = OutboxMessageStatus.Processed;
        ProcessedAt = now;
        LastError = null;
    }

    /// <summary>
    /// Records a failed delivery. A retryable failure (5xx, timeout, network) is tried again
    /// after <see cref="RetryDelay"/> until <see cref="MaxAttempts"/>; anything else (4xx) will
    /// not fix itself, so the message is marked Failed at once.
    /// </summary>
    public void RecordFailure(string error, bool retryable, DateTimeOffset now)
    {
        Attempts++;
        LastError = error.Length <= MaxErrorLength ? error : error[..MaxErrorLength];

        if (!retryable || Attempts >= MaxAttempts)
            Status = OutboxMessageStatus.Failed;
        else
            NextAttemptAt = now + RetryDelay(Attempts);
    }
}
