using Loans.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Loans.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Sets <see cref="Entity.CreatedAt"/> when a row is inserted and <see cref="Entity.UpdatedAt"/>
/// when it is updated, so every entity gets audit timestamps without the domain setting them.
/// </summary>
public sealed class AuditTimestampsInterceptor(TimeProvider clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
            return;

        var now = clock.GetUtcNow();
        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(e => e.CreatedAt).CurrentValue = now;
            else if (entry.State == EntityState.Modified || HasChangedOwnedData(entry))
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
        }
    }

    /// <summary>
    /// Value objects mapped as owned types (e.g. PersonalDetails) are tracked as their own
    /// entries, so replacing one does not mark the owning entity as Modified by itself.
    /// </summary>
    private static bool HasChangedOwnedData(EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry is { } owned
            && owned.Metadata.IsOwned()
            && (owned.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                || HasChangedOwnedData(owned)));
}
