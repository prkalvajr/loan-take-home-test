using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Loans.Infrastructure.Outbox;

/// <summary>
/// Polls the outbox every 2 s and delivers every due message, outside any HTTP request.
/// Polling is the only delivery path (no send right after commit), so there is one code path
/// to reason about. A single Api instance is assumed: with several, the query would need
/// SELECT ... FOR UPDATE SKIP LOCKED so two workers never send the same message.
/// </summary>
public sealed class OutboxBackgroundService(IServiceScopeFactory scopes, ILogger<OutboxBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            try
            {
                await DeliverDueMessagesAsync(stoppingToken);
            }
            catch (Exception e) when (!stoppingToken.IsCancellationRequested)
            {
                // E.g. the database is briefly unavailable: log and try again on the next tick.
                logger.LogError(e, "Outbox processing failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeliverDueMessagesAsync(CancellationToken cancellationToken)
    {
        bool processed;
        do
        {
            // A new scope (and DbContext) per message keeps change tracking small.
            await using var scope = scopes.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
            processed = await processor.ProcessNextAsync(cancellationToken);
        }
        while (processed && !cancellationToken.IsCancellationRequested);
    }
}
