namespace Loans.Domain.Common;

/// <summary>
/// Base class for every entity: identity plus audit timestamps. CreatedAt and UpdatedAt are
/// set by the persistence layer on save, so business methods never have to remember them.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; private set; }

    /// <summary>When the row was first saved.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When the row was last changed; null if it was never updated after creation.</summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Used by EF Core when loading from the database.
    protected Entity() { }

    protected Entity(Guid id) => Id = id;
}
