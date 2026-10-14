namespace Loans.Application.Events;

/// <summary>Only the last 4 SSN digits leave our system; the external service does not need the full number.</summary>
public sealed record CustomerPayload(
    string FirstName,
    string LastName,
    string SsnLast4,
    string CompanyName,
    AddressPayload Address);
