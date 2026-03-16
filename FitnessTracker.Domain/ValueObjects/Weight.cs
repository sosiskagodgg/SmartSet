// FitnessTracker.Domain/ValueObjects/Weight.cs
using FitnessTracker.Domain.Common;

namespace FitnessTracker.Domain.ValueObjects;

/// <summary>
/// Value Object для веса
/// </summary>
public sealed class Weight : ValueObject
{
    public decimal Kilograms { get; }

    private Weight() { } // Для EF Core

    private Weight(decimal kilograms)
    {
        Kilograms = kilograms;
    }

    public static Weight Create(decimal kilograms)
    {
        if (kilograms < 0 || kilograms > 1000)
            throw new ArgumentException($"Weight must be between 0 and 1000 kg. Received: {kilograms}", nameof(kilograms));
        // ИСПРАВЛЕНО: используем ArgumentException вместо DomainException

        return new Weight(kilograms);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Kilograms;
    }
}