// FitnessTracker.Infrastructure/Data/Converters/UtcDateTimeConverter.cs
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FitnessTracker.Infrastructure.Data.Converters;

/// <summary>
/// Конвертер для DateTime, который ensures that Kind=Utc при чтении из БД
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v,  // при записи оставляем как есть (EF Core сам преобразует)
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))  // при чтении устанавливаем Kind=Utc
    {
    }
}

/// <summary>
/// Конвертер для nullable DateTime
/// </summary>
public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter()
        : base(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}