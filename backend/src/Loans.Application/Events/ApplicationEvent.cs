using Loans.Domain.Customers;

namespace Loans.Application.Events;

/// <summary>
/// Something the external service must hear about: a new or an updated application.
/// Built before saving, which works because entity IDs are generated in code.
/// </summary>
public sealed record ApplicationEvent(ApplicationEventType Type, Guid CustomerId, ApplicationPayload Payload)
{
    public static ApplicationEvent Created(Customer customer) =>
        new(ApplicationEventType.ApplicationCreated, customer.Id, ApplicationPayload.From(customer));

    public static ApplicationEvent Updated(Customer customer) =>
        new(ApplicationEventType.ApplicationUpdated, customer.Id, ApplicationPayload.From(customer));
}
