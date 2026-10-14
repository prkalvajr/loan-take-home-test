using Loans.Domain.Common;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.Domain.Customers;

/// <summary>
/// Aggregate root. One customer per SSN, and each customer owns exactly one
/// <see cref="LoanApplication"/>; a returning customer updates it instead of creating another.
/// </summary>
public sealed class Customer : Entity
{
    public Ssn Ssn { get; private set; } = null!;
    public PersonalDetails Details { get; private set; } = null!;
    public LoanApplication Application { get; private set; } = null!;

    // Used by EF Core when loading from the database.
    private Customer() { }

    private Customer(Guid id, Ssn ssn, PersonalDetails details, decimal requestedAmount) : base(id)
    {
        Ssn = ssn;
        Details = details;
        Application = LoanApplication.Create(id, requestedAmount);
    }

    /// <summary>A first-time applicant: creates the customer and its application.</summary>
    public static Customer Register(Ssn ssn, PersonalDetails details, decimal requestedAmount)
    {
        ArgumentNullException.ThrowIfNull(ssn);
        ArgumentNullException.ThrowIfNull(details);

        return new Customer(Guid.CreateVersion7(), ssn, details, requestedAmount);
    }

    /// <summary>A returning applicant: replaces the details and updates the same application.</summary>
    public void Resubmit(PersonalDetails details, decimal requestedAmount)
    {
        ArgumentNullException.ThrowIfNull(details);

        // Amount first: if it is invalid this throws before anything changes.
        Application.ChangeAmount(requestedAmount);
        Details = details;
    }
}
