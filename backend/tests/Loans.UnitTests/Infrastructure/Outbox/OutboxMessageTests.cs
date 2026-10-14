using Loans.Infrastructure.Outbox;

namespace Loans.UnitTests.Infrastructure.Outbox;

public class OutboxMessageTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    private static OutboxMessage NewMessage() =>
        OutboxMessage.Create(Guid.NewGuid(), "ApplicationCreated", "{}", Now);

    [Fact]
    public void Create_IsPendingAndDueImmediately()
    {
        var message = NewMessage();

        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(0, message.Attempts);
        Assert.Equal(Now, message.NextAttemptAt);
        Assert.NotEqual(Guid.Empty, message.MessageId);
    }

    [Fact]
    public void MarkProcessed_RecordsTheAttemptAndTime()
    {
        var message = NewMessage();

        message.MarkProcessed(Now);

        Assert.Equal(OutboxMessageStatus.Processed, message.Status);
        Assert.Equal(1, message.Attempts);
        Assert.Equal(Now, message.ProcessedAt);
    }

    [Fact]
    public void RetryableFailures_BackOff_2_4_8_16_Seconds_ThenFailAfterFiveAttempts()
    {
        var message = NewMessage();
        var expectedDelays = new[] { 2, 4, 8, 16 };

        foreach (var delay in expectedDelays)
        {
            message.RecordFailure("500 Internal Server Error", retryable: true, Now);
            Assert.Equal(OutboxMessageStatus.Pending, message.Status);
            Assert.Equal(Now.AddSeconds(delay), message.NextAttemptAt);
        }

        message.RecordFailure("500 Internal Server Error", retryable: true, Now);

        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Equal(OutboxMessage.MaxAttempts, message.Attempts);
        Assert.Equal("500 Internal Server Error", message.LastError);
    }

    [Fact]
    public void NonRetryableFailure_FailsImmediately()
    {
        var message = NewMessage();

        message.RecordFailure("404 Not Found", retryable: false, Now);

        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Equal(1, message.Attempts);
    }

    [Fact]
    public void LongErrors_AreTruncated()
    {
        var message = NewMessage();

        message.RecordFailure(new string('x', 5000), retryable: true, Now);

        Assert.Equal(2000, message.LastError!.Length);
    }
}
