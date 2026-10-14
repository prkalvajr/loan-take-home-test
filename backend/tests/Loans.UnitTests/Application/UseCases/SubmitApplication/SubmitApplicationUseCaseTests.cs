using Loans.Application.Events;
using Loans.Application.Rules;
using Loans.Application.Rules.DenyRules;
using Loans.Application.UseCases.SubmitApplication;
using Loans.Domain.Common;
using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;
using Loans.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Loans.UnitTests.Application.UseCases.SubmitApplication;

/// <summary>Real rule engine and domain; fake repository, outbox and unit of work.</summary>
public class SubmitApplicationUseCaseTests
{
    private const string ApplicantSsn = "123-45-6789";
    private const string BlacklistedSsn = "000-00-0001";

    private readonly FakeOutbox _outbox = new();

    private static SubmitApplicationCommand Command(
        string ssn = ApplicantSsn, string state = "TX", string firstName = "Jane", decimal amount = 25_000m) =>
        new(firstName, "Doe", "Acme LLC", "100 Main St", "Austin", state, "73301", amount, ssn);

    private static Customer ExistingCustomer() =>
        Customer.Register(
            Ssn.Create(ApplicantSsn),
            PersonalDetails.Create("Jane", "Doe", "Acme LLC", Address.Create("100 Main St", "Austin", "TX", "73301")),
            10_000m);

    private SubmitApplicationUseCase UseCase(FakeCustomerRepository repository, FakeUnitOfWork unitOfWork) =>
        new(
            new RuleEngine([new NewYorkStateRule(), new SsnBlacklistRule(new FakeSsnBlacklist(BlacklistedSsn))]),
            repository,
            _outbox,
            unitOfWork,
            NullLogger<SubmitApplicationUseCase>.Instance);

    [Fact]
    public async Task NewSsn_CreatesCustomer_RecordsCreatedEvent_AndSavesOnce()
    {
        var repository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await UseCase(repository, unitOfWork).HandleAsync(Command(), CancellationToken.None);

        var approved = Assert.IsType<SubmitApplicationResult.Approved>(result);
        Assert.False(approved.IsReturningCustomer);
        var customer = Assert.Single(repository.Added);
        Assert.Equal(customer.Application.Id, approved.ApplicationId);
        Assert.Equal(customer.Id, approved.CustomerId);
        var applicationEvent = Assert.Single(_outbox.Events);
        Assert.Equal(ApplicationEventType.ApplicationCreated, applicationEvent.Type);
        Assert.Equal(1, unitOfWork.Saves);
    }

    [Fact]
    public async Task ReturningSsn_UpdatesSameCustomer_RecordsUpdatedEvent_AndSavesOnce()
    {
        var existing = ExistingCustomer();
        var repository = new FakeCustomerRepository(existing);
        var unitOfWork = new FakeUnitOfWork();

        var result = await UseCase(repository, unitOfWork)
            .HandleAsync(Command(firstName: "Janet", amount: 40_000m), CancellationToken.None);

        var approved = Assert.IsType<SubmitApplicationResult.Approved>(result);
        Assert.True(approved.IsReturningCustomer);
        Assert.Equal(existing.Id, approved.CustomerId);
        Assert.Equal(existing.Application.Id, approved.ApplicationId);
        Assert.Empty(repository.Added);
        Assert.Equal("Janet", existing.Details.FirstName);
        Assert.Equal(40_000m, existing.Application.RequestedAmount);
        var applicationEvent = Assert.Single(_outbox.Events);
        Assert.Equal(ApplicationEventType.ApplicationUpdated, applicationEvent.Type);
        Assert.Equal(1, unitOfWork.Saves);
    }

    [Theory]
    [InlineData("NY", ApplicantSsn, "STATE_NOT_ALLOWED")]
    [InlineData("TX", BlacklistedSsn, "SSN_BLACKLISTED")]
    public async Task Denied_SavesNothing_AndNeverLooksUpTheCustomer(string state, string ssn, string expectedReason)
    {
        var repository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await UseCase(repository, unitOfWork).HandleAsync(Command(ssn: ssn, state: state), CancellationToken.None);

        var denied = Assert.IsType<SubmitApplicationResult.Denied>(result);
        Assert.Equal([expectedReason], denied.Reasons);
        Assert.Equal(0, repository.Lookups);
        Assert.Empty(repository.Added);
        Assert.Empty(_outbox.Events);
        Assert.Equal(0, unitOfWork.Saves);
    }

    [Fact]
    public async Task DeniedReturningCustomer_LeavesExistingDataUnchanged()
    {
        var existing = ExistingCustomer();
        var unitOfWork = new FakeUnitOfWork();

        var result = await UseCase(new FakeCustomerRepository(existing), unitOfWork)
            .HandleAsync(Command(state: "NY", firstName: "Janet", amount: 99_000m), CancellationToken.None);

        Assert.IsType<SubmitApplicationResult.Denied>(result);
        Assert.Equal("Jane", existing.Details.FirstName);
        Assert.Equal(10_000m, existing.Application.RequestedAmount);
        Assert.Empty(_outbox.Events);
        Assert.Equal(0, unitOfWork.Saves);
    }

    [Fact]
    public async Task UniqueViolationOnSave_ReturnsConflict()
    {
        var result = await UseCase(new FakeCustomerRepository(), new FakeUnitOfWork { ThrowConflict = true })
            .HandleAsync(Command(), CancellationToken.None);

        Assert.IsType<SubmitApplicationResult.Conflict>(result);
    }

    [Fact]
    public async Task InvalidInput_ThrowsDomainException_BeforeRulesOrSaving()
    {
        var repository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        await Assert.ThrowsAsync<DomainException>(() =>
            UseCase(repository, unitOfWork).HandleAsync(Command(ssn: "123"), CancellationToken.None));

        Assert.Equal(0, repository.Lookups);
        Assert.Equal(0, unitOfWork.Saves);
    }
}
