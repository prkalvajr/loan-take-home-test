namespace Loans.Infrastructure.Blacklist;

/// <summary>A row of the SSN blacklist. Persistence model only: the domain sees the ISsnBlacklist port.</summary>
public sealed class BlacklistedSsn
{
    /// <summary>The 9 normalized digits.</summary>
    public string Ssn { get; init; } = null!;

    public string Reason { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
}
