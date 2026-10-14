using Loans.Application.Abstractions;
using Loans.Domain.Customers.ValueObjects;
using Loans.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Loans.Infrastructure.Blacklist;

/// <summary><see cref="ISsnBlacklist"/> backed by the blacklisted_ssns table.</summary>
public sealed class SsnBlacklist(LoansDbContext db) : ISsnBlacklist
{
    public Task<bool> ContainsAsync(Ssn ssn, CancellationToken cancellationToken) =>
        db.BlacklistedSsns.AnyAsync(b => b.Ssn == ssn.Value, cancellationToken);
}
