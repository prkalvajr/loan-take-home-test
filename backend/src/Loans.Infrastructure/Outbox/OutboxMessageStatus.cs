namespace Loans.Infrastructure.Outbox;

public enum OutboxMessageStatus
{
    /// <summary>Waiting to be sent, or waiting for its next retry.</summary>
    Pending,

    /// <summary>Delivered: the external service answered 2xx.</summary>
    Processed,

    /// <summary>Gave up after the last retry or on a 4xx; kept for manual replay.</summary>
    Failed,
}
