// FitnessTracker.Infrastructure/Data/Converters/ExerciseJsonConverter.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using FitnessTracker.Domain.Entities.Exercises;

namespace FitnessTracker.Infrastructure.Data.Converters;

public class ExerciseJsonConverter : JsonConverter<Exercise>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exercise).IsAssignableFrom(typeToConvert);
    }

    // FitnessTracker.Infrastructure/Data/Converters/ExerciseJsonConverter.cs
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

        var typeName = typeProperty.GetString()?.ToLower();

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

        // Записываем все свойства через рефлексию
        foreach (var prop in value.GetType().GetProperties())
        {
            if (prop.Name == "Type") continue; // Пропускаем Type, мы его уже записали

            var propValue = prop.GetValue(value);
            if (propValue != null)
            {
                writer.WritePropertyName(options?.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name);
                JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
            }
        }

        writer.WriteEndObject();
    }
}