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
}
