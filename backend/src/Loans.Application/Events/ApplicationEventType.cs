namespace Loans.Application.Events;

/// <summary>Stored in the outbox; the worker sends Created as POST and Updated as PUT.</summary>
public enum ApplicationEventType
{
    ApplicationCreated,
    ApplicationUpdated,
}
