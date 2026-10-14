using System.Collections.Concurrent;

namespace ExternalService.Mock;

/// <summary>
/// In-memory storage keyed by the caller's application ID. Data is lost on restart,
/// which is fine for a mock.
/// </summary>
public sealed class ApplicationStore
{
    private readonly ConcurrentDictionary<Guid, StoredApplication> _applications = new();
    private readonly ConcurrentDictionary<string, byte> _seenIdempotencyKeys = new();

    public IReadOnlyCollection<StoredApplication> All() =>
        _applications.Values.OrderBy(a => a.CreatedAt).ToList();

    public bool IsRepeatedDelivery(string idempotencyKey) => _seenIdempotencyKeys.ContainsKey(idempotencyKey);

    public void RememberDelivery(string idempotencyKey) => _seenIdempotencyKeys.TryAdd(idempotencyKey, 0);

    /// <summary>Creates the record. Returns false when the ID already exists (nothing is changed).</summary>
    public bool TryCreate(ApplicationPayload payload, DateTimeOffset now) =>
        _applications.TryAdd(payload.ApplicationId, new StoredApplication(payload, now, now, Version: 1));

    /// <summary>Replaces the record. Returns false when the ID is unknown.</summary>
    public bool TryReplace(ApplicationPayload payload, DateTimeOffset now)
    {
        while (_applications.TryGetValue(payload.ApplicationId, out var current))
        {
            var updated = current with { Data = payload, UpdatedAt = now, Version = current.Version + 1 };
            if (_applications.TryUpdate(payload.ApplicationId, updated, current))
                return true;
        }

        return false;
    }
}
