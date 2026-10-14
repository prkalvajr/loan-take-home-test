using System.Text.Json;
using Loans.Application.Events;
using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.UnitTests.Application.Events;

public class ApplicationEventTests
{
    private static Customer NewCustomer() =>
        Customer.Register(
            Ssn.Create("123-45-6789"),
            PersonalDetails.Create("Jane", "Doe", "Acme LLC", Address.Create("100 Main St", "Austin", "TX", "73301")),
            25_000m);

    [Fact]
    public void Created_CopiesCustomerAndApplication_WithOnlyLastFourSsnDigits()
    {
        var customer = NewCustomer();

        var applicationEvent = ApplicationEvent.Created(customer);

        Assert.Equal(ApplicationEventType.ApplicationCreated, applicationEvent.Type);
        Assert.Equal(customer.Id, applicationEvent.CustomerId);
        Assert.Equal(
            new ApplicationPayload(
                customer.Application.Id,
                customer.Id,
                25_000m,
                new CustomerPayload("Jane", "Doe", "6789", "Acme LLC", new AddressPayload("100 Main St", "Austin", "TX", "73301"))),
            applicationEvent.Payload);
    }

    [Fact]
    public void Updated_HasUpdatedType()
    {
        Assert.Equal(ApplicationEventType.ApplicationUpdated, ApplicationEvent.Updated(NewCustomer()).Type);
    }

    [Fact]
    public void Payload_SerializesToTheExternalServiceContract_WithoutTheFullSsn()
    {
        var json = JsonSerializer.Serialize(ApplicationEvent.Created(NewCustomer()).Payload, JsonSerializerOptions.Web);

        Assert.Contains("\"ssnLast4\":\"6789\"", json);
        Assert.Contains("\"requestedAmount\":25000", json);
        Assert.Contains("\"address\":{\"street\":\"100 Main St\"", json);
        Assert.DoesNotContain("123456789", json);
    }
}
