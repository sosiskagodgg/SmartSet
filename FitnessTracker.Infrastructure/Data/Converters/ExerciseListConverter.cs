// FitnessTracker.Infrastructure/Data/Converters/ExerciseListConverter.cs
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FitnessTracker.Domain.Entities.Exercises;
using System.Text.Json;

namespace FitnessTracker.Infrastructure.Data.Converters;

public class ExerciseListConverter : ValueConverter<List<Exercise>, string>
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new ExerciseJsonConverter() }
    };

    public ExerciseListConverter()
        : base(
            v => JsonSerializer.Serialize(v, _options),
            v => JsonSerializer.Deserialize<List<Exercise>>(v, _options) ?? new List<Exercise>())
    {
    }
}