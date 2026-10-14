namespace Loans.Application.Events;

public sealed record AddressPayload(string Street, string City, string State, string ZipCode);
