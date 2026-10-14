namespace Loans.IntegrationTests.Support;

/// <summary>Request bodies for POST /api/applications, valid unless a test overrides a value.</summary>
public static class ApplicationRequests
{
    public const string BlacklistedSsn = "000-00-0001";

    public static object Valid(
        string ssn = "123-45-6789", string state = "TX", string firstName = "Jane", decimal requestedAmount = 25_000m) => new
    {
        firstName,
        lastName = "Doe",
        companyName = "Acme LLC",
        address = new { street = "100 Main St", city = "Austin", state, zipCode = "73301" },
        requestedAmount,
        ssn,
    };
}
