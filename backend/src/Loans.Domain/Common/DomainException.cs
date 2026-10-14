namespace Loans.Domain.Common;

/// <summary>
/// A domain invariant was broken (e.g. an SSN without 9 digits). The API validates input
/// first, so reaching this means invalid data got past it; the endpoint maps it to 400.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
