using Loans.Infrastructure.ExternalService;
using Loans.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loans.Infrastructure.Outbox;

/// <summary>
/// Delivers one outbox message per call: the oldest due message whose customer has no earlier
/// message still waiting or failed. That keeps each customer's Created before its Updated,
/// while one customer's failing message does not hold back anyone else.
/// </summary>
public sealed class OutboxProcessor(
    LoansDbContext db,
    ExternalServiceClient client,
    TimeProvider clock,
    ILogger<OutboxProcessor> logger)
{
    /// <summary>Returns false when there was nothing to send.</summary>
    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();

        var message = await db.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending && m.NextAttemptAt <= now)
            .Where(m => !db.OutboxMessages.Any(earlier =>
                earlier.CustomerId == m.CustomerId
                && earlier.Id < m.Id
                && earlier.Status != OutboxMessageStatus.Processed))
            .OrderBy(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (message is null)
            return false;

        var result = await client.SendAsync(message, cancellationToken);

        if (result.Succeeded)
        {
            message.MarkProcessed(clock.GetUtcNow());
            logger.LogInformation("Outbox message {OutboxId} ({Type}) delivered", message.Id, message.Type);
        }
        else
        {
            message.RecordFailure(result.Error!, result.Retryable, clock.GetUtcNow());
            logger.LogWarning(
                "Outbox message {OutboxId} ({Type}) attempt {Attempt} failed: {Error}. Status {Status}, next attempt {NextAttemptAt}",
                message.Id, message.Type, message.Attempts, message.LastError, message.Status, message.NextAttemptAt);
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
