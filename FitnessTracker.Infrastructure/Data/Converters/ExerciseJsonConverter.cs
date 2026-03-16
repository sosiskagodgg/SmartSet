// FitnessTracker.Infrastructure/Data/Converters/ExerciseJsonConverter.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Enums;

namespace FitnessTracker.Infrastructure.Data.Converters;

public class ExerciseJsonConverter : JsonConverter<Exercise>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exercise).IsAssignableFrom(typeToConvert);
    }

    public override Exercise? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement.Clone();

        // Определяем тип по дискриминатору
        if (!root.TryGetProperty("type", out var typeProperty))
        {
            // Если нет type, пробуем определить по наличию специфических полей
            if (root.TryGetProperty("sets", out _) && root.TryGetProperty("reps", out _))
            {
                return JsonSerializer.Deserialize<StrengthExercise>(root.GetRawText(), options);
            }
            if (root.TryGetProperty("durationMinutes", out _) && root.TryGetProperty("distanceKm", out _))
            {
                return JsonSerializer.Deserialize<RunningExercise>(root.GetRawText(), options);
            }
            if (root.TryGetProperty("durationMinutes", out _) && root.TryGetProperty("cardioType", out _))
            {
                return JsonSerializer.Deserialize<CardioExercise>(root.GetRawText(), options);
            }
            if (root.TryGetProperty("holdSeconds", out _) && root.TryGetProperty("staticType", out _))
            {
                return JsonSerializer.Deserialize<StaticExercise>(root.GetRawText(), options);
            }

            throw new JsonException("Missing type discriminator and cannot infer exercise type");
        }

        var typeName = typeProperty.GetString()?.ToLowerInvariant();

        return typeName switch
        {
            "strength" => JsonSerializer.Deserialize<StrengthExercise>(root.GetRawText(), options),
            "cardio" => JsonSerializer.Deserialize<CardioExercise>(root.GetRawText(), options),
            "running" => JsonSerializer.Deserialize<RunningExercise>(root.GetRawText(), options),
            "static" => JsonSerializer.Deserialize<StaticExercise>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown exercise type: {typeName}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Exercise value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Записываем дискриминатор типа
        writer.WriteString("type", value.Type.ToString().ToLowerInvariant());

        // Записываем свойства в зависимости от типа
        switch (value)
        {
            case StrengthExercise s:
                writer.WriteNumber("sets", s.Sets);
                writer.WriteNumber("reps", s.Reps);
                if (s.Weight != null)
                    writer.WriteNumber("weight", (double)s.Weight.Kilograms);
                writer.WriteString("muscleGroup", s.MuscleGroup);
                writer.WriteNumber("strengthExerciseType", (int)s.StrengthExerciseType);
                writer.WriteNumber("equipment", (int)s.Equipment);
                break;

            case RunningExercise r:
                writer.WriteNumber("durationMinutes", r.DurationMinutes);
                if (r.DistanceKm.HasValue)
                    writer.WriteNumber("distanceKm", r.DistanceKm.Value);
                if (r.AvgHeartRate.HasValue)
                    writer.WriteNumber("avgHeartRate", r.AvgHeartRate.Value);
                if (r.Pace.HasValue)
                    writer.WriteNumber("pace", r.Pace.Value);
                writer.WriteNumber("cardioIntensity", (int)r.Intensity);
                writer.WriteNumber("runningSurface", (int)r.Surface);
                if (r.ElevationGain.HasValue)
                    writer.WriteNumber("elevationGain", r.ElevationGain.Value);
                break;

            case CardioExercise c:
                writer.WriteNumber("durationMinutes", c.DurationMinutes);
                if (c.DistanceKm.HasValue)
                    writer.WriteNumber("distanceKm", c.DistanceKm.Value);
                if (c.AvgHeartRate.HasValue)
                    writer.WriteNumber("avgHeartRate", c.AvgHeartRate.Value);
                writer.WriteNumber("cardioIntensity", (int)c.Intensity);
                writer.WriteNumber("cardioType", (int)c.CardioType);
                break;

            case StaticExercise st:
                writer.WriteNumber("holdSeconds", st.HoldSeconds);
                writer.WriteNumber("sets", st.Sets);
                writer.WriteNumber("staticType", (int)st.StaticType);
                break;
        }

        // Общие свойства
        writer.WriteString("name", value.Name);
        if (!string.IsNullOrEmpty(value.Description))
            writer.WriteString("description", value.Description);
        writer.WriteNumber("met", value.MET);

        writer.WriteEndObject();
    }
}