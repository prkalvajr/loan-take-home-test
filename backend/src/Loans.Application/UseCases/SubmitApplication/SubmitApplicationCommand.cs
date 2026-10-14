namespace Loans.Application.UseCases.SubmitApplication;

/// <summary>The submitted form, as plain values. The use case turns them into value objects.</summary>
public sealed record SubmitApplicationCommand(
    string FirstName,
    string LastName,
    string CompanyName,
    string Street,
    string City,
    string State,
    string ZipCode,
    decimal RequestedAmount,
    string Ssn);
