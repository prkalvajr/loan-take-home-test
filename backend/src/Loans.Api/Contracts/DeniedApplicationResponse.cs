using Loans.Application.UseCases.SubmitApplication;

namespace Loans.Api.Contracts;

/// <summary>200 body when denied: a valid request with a business "no", so not an error status.</summary>
public sealed record DeniedApplicationResponse(IReadOnlyList<string> Reasons)
{
    public string Decision => "denied";

    public static DeniedApplicationResponse From(SubmitApplicationResult.Denied denied) => new(denied.Reasons);
}
