using Loans.Application.Abstractions;
using Loans.Domain.Customers;

namespace Loans.UnitTests.Fakes;

/// <summary>In-memory blacklist; records which SSNs were looked up.</summary>
public sealed class FakeSsnBlacklist(params string[] blacklisted) : ISsnBlacklist
{
    private readonly HashSet<Ssn> _blacklisted = blacklisted.Select(Ssn.Create).ToHashSet();

    public List<Ssn> Queried { get; } = [];

    public Task<bool> ContainsAsync(Ssn ssn, CancellationToken cancellationToken)
    {
        Queried.Add(ssn);
        return Task.FromResult(_blacklisted.Contains(ssn));
    }
}
