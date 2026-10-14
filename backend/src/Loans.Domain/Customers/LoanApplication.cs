using Loans.Domain.Common;

namespace Loans.Domain.Customers;

/// <summary>
/// The customer's loan application. Part of the <see cref="Customer"/> aggregate: it is
/// created and changed only through the customer, so its factory and mutator are internal.
/// </summary>
public sealed class LoanApplication : Entity
{
    public Guid CustomerId { get; private set; }
    public decimal RequestedAmount { get; private set; }

    // Used by EF Core when loading from the database.
    private LoanApplication() { }

    private LoanApplication(Guid id, Guid customerId, decimal requestedAmount) : base(id)
    {
        CustomerId = customerId;
        RequestedAmount = requestedAmount;
    }

    internal static LoanApplication Create(Guid customerId, decimal requestedAmount) =>
        new(Guid.CreateVersion7(), customerId, ValidAmount(requestedAmount));

    internal void ChangeAmount(decimal requestedAmount) => RequestedAmount = ValidAmount(requestedAmount);

    private static decimal ValidAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Requested amount must be greater than 0.");
        if (decimal.Round(amount, 2) != amount)
            throw new DomainException("Requested amount can have at most 2 decimal places.");

        return amount;
    }
}
