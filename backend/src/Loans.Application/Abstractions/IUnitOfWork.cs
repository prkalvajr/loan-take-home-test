namespace Loans.Application.Abstractions;

/// <summary>
/// Commits everything tracked in this request (customer, application and outbox message)
/// in one database transaction. Either all of it is saved or none of it is.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
