// FitnessTracker.Infrastructure/Data/Converters/ExerciseReadOnlyCollectionConverter.cs
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FitnessTracker.Domain.Entities.Exercises;
using System.Text.Json;

namespace FitnessTracker.Infrastructure.Data.Converters;

public class ExerciseReadOnlyCollectionConverter : ValueConverter<IReadOnlyCollection<Exercise>, string>
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new ExerciseJsonConverter() }
    };

    public ExerciseReadOnlyCollectionConverter()
        : base(
            v => JsonSerializer.Serialize(v.ToList(), _options),
            v => (JsonSerializer.Deserialize<List<Exercise>>(v, _options) ?? new List<Exercise>()).AsReadOnly())
    {
    }
}