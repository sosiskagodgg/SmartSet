// FitnessTracker.Domain/ValueObjects/SubscriptionInfo.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Exceptions;

namespace FitnessTracker.Domain.ValueObjects;

/// <summary>
/// Value Object для информации о подписке
/// Объединяет статус и дату окончания в один объект
/// </summary>
public sealed class SubscriptionInfo : ValueObject
{
    public string Status { get; }
    public DateTime? EndDate { get; }

    private SubscriptionInfo(string status, DateTime? endDate)
    {
        Status = status;
        EndDate = endDate;
    }

    /// <summary>
    /// Создание активной подписки
    /// </summary>
    public static SubscriptionInfo CreateActive(DateTime endDate)
    {
        if (endDate <= DateTime.UtcNow)
            throw new InvalidSubscriptionDateException("End date must be in the future");

        return new SubscriptionInfo("active", endDate);
    }

    /// <summary>
    /// Создание неактивной подписки
    /// </summary>
    public static SubscriptionInfo CreateInactive()
    {
        return new SubscriptionInfo("inactive", null);
    }

    /// <summary>
    /// Создание из примитивных значений (для маппинга)
    /// </summary>
    public static SubscriptionInfo CreateFromPrimitives(string status, DateTime? endDate)
    {
        var allowedStatuses = new[] { "active", "inactive", "expired", "cancelled" };
        if (!allowedStatuses.Contains(status.ToLowerInvariant()))
            throw new InvalidSubscriptionStatusException(status);

        return new SubscriptionInfo(status.ToLowerInvariant(), endDate);
    }

    public bool IsActive => Status == "active" && EndDate > DateTime.UtcNow;
    public bool CanBeActivated => Status == "inactive" || Status == "expired";
    public bool CanBeCancelled => Status == "active";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Status;
        yield return EndDate ?? DateTime.MinValue;
    }
}