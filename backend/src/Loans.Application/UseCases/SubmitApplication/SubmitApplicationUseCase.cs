using Loans.Application.Abstractions;
using Loans.Application.Events;
using Loans.Application.Rules;
using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Loans.Application.UseCases.SubmitApplication;

/// <summary>
/// Submits a loan application: runs the rules, then creates a new customer or updates the
/// returning one, and records the event for the external service, all in one transaction.
/// Orchestration only: business rules live in the rule engine and the Customer aggregate.
/// </summary>
public sealed class SubmitApplicationUseCase(
    RuleEngine ruleEngine,
    ICustomerRepository customers,
    IOutbox outbox,
    IUnitOfWork unitOfWork,
    ILogger<SubmitApplicationUseCase> logger)
{
    public async Task<SubmitApplicationResult> HandleAsync(SubmitApplicationCommand command, CancellationToken cancellationToken)
    {
        // Value objects validate the input; invalid data throws DomainException.
        var ssn = Ssn.Create(command.Ssn);
        var address = Address.Create(command.Street, command.City, command.State, command.ZipCode);
        var details = PersonalDetails.Create(command.FirstName, command.LastName, command.CompanyName, address);

        // Rules run first, for every submission: a denied returning customer changes nothing.
        var decision = await ruleEngine.EvaluateAsync(
            new LoanApplicationInput(ssn, address.State, command.RequestedAmount), cancellationToken);

        logger.LogInformation(
            "Application for SSN {Ssn} {Decision}. Rule results: {RuleResults}",
            ssn.Masked,
            decision.IsApproved ? "approved" : "denied",
            decision.Results);

        if (!decision.IsApproved)
            return new SubmitApplicationResult.Denied(decision.DenialCodes);

        var customer = await customers.GetBySsnAsync(ssn, cancellationToken);
        var isReturningCustomer = customer is not null;

        if (customer is null)
        {
            customer = Customer.Register(ssn, details, command.RequestedAmount);
            customers.Add(customer);
            outbox.Add(ApplicationEvent.Created(customer));
        }
        else
        {
            customer.Resubmit(details, command.RequestedAmount);
            outbox.Add(ApplicationEvent.Updated(customer));
        }

        try
        {
            // One call, one transaction: customer, application and outbox row together.
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (PersistenceConflictException e)
        {
            logger.LogWarning(e, "Concurrent first submission for SSN {Ssn}; nothing was saved", ssn.Masked);
            return new SubmitApplicationResult.Conflict();
        }

        return new SubmitApplicationResult.Approved(customer.Application.Id, customer.Id, isReturningCustomer);
    }
}
