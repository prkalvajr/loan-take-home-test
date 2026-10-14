namespace ExternalService.Mock;

/// <summary>Body of POST /applications and PUT /applications/{applicationId}.</summary>
public sealed record ApplicationPayload(
    Guid ApplicationId,
    Guid CustomerId,
    decimal RequestedAmount,
    CustomerPayload Customer);

/// <summary>Only the last 4 SSN digits are shared with the external service.</summary>
public sealed record CustomerPayload(
    string FirstName,
    string LastName,
    string SsnLast4,
    string CompanyName,
    AddressPayload Address);

public sealed record AddressPayload(
    string Street,
    string City,
    string State,
    string ZipCode);

/// <summary>What the mock keeps per application, shown by GET /applications.</summary>
public sealed record StoredApplication(
    ApplicationPayload Data,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version);
