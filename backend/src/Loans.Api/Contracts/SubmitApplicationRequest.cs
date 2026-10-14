using System.ComponentModel.DataAnnotations;
using Loans.Application.UseCases.SubmitApplication;

namespace Loans.Api.Contracts;

/// <summary>
/// Body of POST /api/applications. Validated at the boundary so the client gets a 400 with
/// errors per field; the domain value objects check the same rules again as invariants.
/// Properties are nullable so a missing field is reported as missing instead of becoming "" or 0.
/// </summary>
public sealed record SubmitApplicationRequest
{
    [Required(ErrorMessage = "Enter your first name."), MaxLength(100)]
    public string? FirstName { get; init; }

    [Required(ErrorMessage = "Enter your last name."), MaxLength(100)]
    public string? LastName { get; init; }

    [Required(ErrorMessage = "Enter your company name."), MaxLength(200)]
    public string? CompanyName { get; init; }

    [Required(ErrorMessage = "Enter the business address.")]
    public AddressRequest? Address { get; init; }

    [Required(ErrorMessage = "Enter the requested amount."), Money]
    public decimal? RequestedAmount { get; init; }

    [Required(ErrorMessage = "Enter your SSN."), RegularExpression("^[0-9]{3}-?[0-9]{2}-?[0-9]{4}$", ErrorMessage = "Enter a 9-digit SSN, like 123-45-6789.")]
    public string? Ssn { get; init; }

    /// <summary>Only called after validation passed, so the null-forgiving operators are safe.</summary>
    public SubmitApplicationCommand ToCommand() => new(
        FirstName!,
        LastName!,
        CompanyName!,
        Address!.Street!,
        Address.City!,
        Address.State!,
        Address.ZipCode!,
        RequestedAmount!.Value,
        Ssn!);
}
