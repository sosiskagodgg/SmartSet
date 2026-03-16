// FitnessTracker.Domain/Common/Entity.cs
namespace FitnessTracker.Domain.Common;

/// <summary>
/// Базовый класс для всех сущностей.
/// Сущности имеют идентичность и могут изменяться во времени.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; }

    protected Entity(TId id)
    {
        Id = id;
    }

    // Для EF Core
    protected Entity() { }

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}