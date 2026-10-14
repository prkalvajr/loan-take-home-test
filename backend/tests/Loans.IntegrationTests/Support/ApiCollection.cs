namespace Loans.IntegrationTests.Support;

/// <summary>
/// Test classes in this collection share one <see cref="LoansApiFactory"/> (one Postgres container)
/// and run one after another, so they never see each other's data mid-test.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<LoansApiFactory>
{
    public const string Name = "Api";
}
