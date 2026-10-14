using System.Text.Json;
using Loans.Application.Abstractions;
using Loans.Application.Events;
using Loans.Infrastructure.Persistence;

namespace Loans.Infrastructure.Outbox;

/// <summary><see cref="IOutbox"/> that adds an outbox_messages row to the request's DbContext.</summary>
public sealed class OutboxWriter(LoansDbContext db, TimeProvider clock) : IOutbox
{
    public void Add(ApplicationEvent applicationEvent)
    {
        // camelCase JSON, the same body the worker will send to the external service.
        var payload = JsonSerializer.Serialize(applicationEvent.Payload, JsonSerializerOptions.Web);

        db.OutboxMessages.Add(OutboxMessage.Create(
            applicationEvent.CustomerId,
            applicationEvent.Type.ToString(),
            payload,
            clock.GetUtcNow()));
    }
}
