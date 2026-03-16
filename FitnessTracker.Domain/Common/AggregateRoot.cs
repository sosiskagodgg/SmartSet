// FitnessTracker.Domain/Common/AggregateRoot.cs
namespace FitnessTracker.Domain.Common;

/// <summary>
/// Маркерный класс для корней агрегатов.
/// Агрегат гарантирует согласованность изменений внутри своих границ.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }
}