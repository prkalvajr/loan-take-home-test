using Loans.Application.Abstractions;
using Loans.Application.Events;

namespace Loans.UnitTests.Fakes;

public sealed class FakeOutbox : IOutbox
{
    public List<ApplicationEvent> Events { get; } = [];

    public void Add(ApplicationEvent applicationEvent) => Events.Add(applicationEvent);
}
