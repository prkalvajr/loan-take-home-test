using Loans.Application.Events;

namespace Loans.Application.Abstractions;

/// <summary>
/// Records an event to deliver later. Only tracks it: the event is written by the same
/// <see cref="IUnitOfWork.SaveChangesAsync"/> that saves the customer, so both commit or neither does.
/// </summary>
public interface IOutbox
{
    void Add(ApplicationEvent applicationEvent);
}
