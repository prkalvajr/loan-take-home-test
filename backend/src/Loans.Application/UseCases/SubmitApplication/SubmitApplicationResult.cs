namespace Loans.Application.UseCases.SubmitApplication;

/// <summary>
/// Every possible outcome of a submission. Closed set: the private constructor means only the
/// nested types below exist, so the endpoint can map each one to an HTTP response.
/// </summary>
public abstract record SubmitApplicationResult
{
    private SubmitApplicationResult() { }

    /// <summary>Saved. <paramref name="IsReturningCustomer"/> tells the UI to say "updated" instead of "received".</summary>
    public sealed record Approved(Guid ApplicationId, Guid CustomerId, bool IsReturningCustomer) : SubmitApplicationResult;

    /// <summary>A rule denied it; nothing was saved. Reasons are stable codes like STATE_NOT_ALLOWED.</summary>
    public sealed record Denied(IReadOnlyList<string> Reasons) : SubmitApplicationResult;

    /// <summary>Another submission with the same SSN was saved at the same moment; nothing was saved.</summary>
    public sealed record Conflict : SubmitApplicationResult;
}
