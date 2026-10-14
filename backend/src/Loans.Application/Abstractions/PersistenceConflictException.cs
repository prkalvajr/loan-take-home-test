namespace Loans.Application.Abstractions;

/// <summary>
/// The save broke a uniqueness rule because another request saved the same data first
/// (e.g. two first-time submissions with the same SSN at the same moment). Nothing was saved.
/// </summary>
public sealed class PersistenceConflictException(string message, Exception innerException)
    : Exception(message, innerException);
