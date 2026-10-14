using Loans.Application.UseCases.SubmitApplication;

namespace Loans.Api.Contracts;

/// <summary>200 body when approved. isReturningCustomer drives "received" vs "updated" on the approved page.</summary>
public sealed record ApprovedApplicationResponse(Guid ApplicationId, Guid CustomerId, bool IsReturningCustomer)
{
    public string Decision => "approved";

    public static ApprovedApplicationResponse From(SubmitApplicationResult.Approved approved) =>
        new(approved.ApplicationId, approved.CustomerId, approved.IsReturningCustomer);
}
