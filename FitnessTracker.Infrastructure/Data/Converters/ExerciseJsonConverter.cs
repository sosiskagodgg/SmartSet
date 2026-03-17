// FitnessTracker.Infrastructure/Data/Converters/ExerciseJsonConverter.cs
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        // Читаем поле type
        if (!root.TryGetProperty("type", out var typeProperty))
        {
            throw new JsonException("Missing type discriminator");
        }

        var typeName = typeProperty.GetString()?.ToLowerInvariant();

        return typeName switch
        {
            "strength" => CreateStrengthExercise(root),
            "running" => CreateRunningExercise(root),
            "cardio" => CreateCardioExercise(root),
            "static" => CreateStaticExercise(root),
            _ => throw new JsonException($"Unknown exercise type: {typeName}")
        };
    }

    private StrengthExercise CreateStrengthExercise(JsonElement root)
    {
        var name = GetString(root, "name") ?? "Упражнение";
        var sets = GetInt(root, "sets", 3);
        var reps = GetInt(root, "reps", 10);
        var muscleGroup = GetString(root, "muscleGroup") ?? "other";
        var equipment = GetString(root, "equipment") ?? "bodyweight";
        var weight = GetFloat(root, "weight");

        return new StrengthExercise(
            name: name,
            met: 4.0f,
            sets: sets,
            reps: reps,
            muscleGroup: muscleGroup,
            strengthExerciseType: StrengthExerciseType.Compound,
            equipment: ParseEquipment(equipment),
            weightKg: weight > 0 ? (decimal?)weight : null
        );
    }

    private RunningExercise CreateRunningExercise(JsonElement root)
    {
        var name = GetString(root, "name") ?? "Бег";
        var duration = GetInt(root, "durationMinutes", 20);
        var distance = GetFloat(root, "distanceKm");
        var surface = GetString(root, "surface") ?? "treadmill";
        var intensity = GetString(root, "intensity") ?? "moderate";

        return new RunningExercise(
            name: name,
            met: 8.0f,
            durationMinutes: duration,
            distanceKm: distance > 0 ? distance : null,
            surface: ParseRunningSurface(surface),
            intensity: ParseIntensity(intensity)
        );
    }

    private CardioExercise CreateCardioExercise(JsonElement root)
    {
        var name = GetString(root, "name") ?? "Кардио";
        var duration = GetInt(root, "durationMinutes", 20);
        var intensity = GetString(root, "intensity") ?? "moderate";
        var cardioType = GetString(root, "cardioType") ?? "liss";
        var distance = GetFloat(root, "distanceKm");

        return new CardioExercise(
            name: name,
            met: 6.0f,
            durationMinutes: duration,
            intensity: ParseIntensity(intensity),
            cardioType: ParseCardioType(cardioType),
            distanceKm: distance > 0 ? distance : null
        );
    }

    private StaticExercise CreateStaticExercise(JsonElement root)
    {
        var name = GetString(root, "name") ?? "Статика";
        var holdSeconds = GetInt(root, "holdSeconds", 30);
        var sets = GetInt(root, "sets", 3);
        var staticType = GetString(root, "staticType") ?? "plank";

        return new StaticExercise(
            name: name,
            met: 3.0f,
            holdSeconds: holdSeconds,
            sets: sets,
            staticType: ParseStaticType(staticType)
        );
    }

    public override void Write(Utf8JsonWriter writer, Exercise value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", value.Type.ToString().ToLowerInvariant());
        writer.WriteString("name", value.Name);
        writer.WriteNumber("met", value.MET);

        switch (value)
        {
            case StrengthExercise s:
                writer.WriteNumber("sets", s.Sets);
                writer.WriteNumber("reps", s.Reps);
                if (s.Weight != null)
                    writer.WriteNumber("weight", (double)s.Weight.Kilograms);
                writer.WriteString("muscleGroup", s.MuscleGroup);
                writer.WriteString("equipment", s.Equipment.ToString().ToLowerInvariant());
                break;

            case RunningExercise r:
                writer.WriteNumber("durationMinutes", r.DurationMinutes);
                if (r.DistanceKm.HasValue)
                    writer.WriteNumber("distanceKm", r.DistanceKm.Value);
                writer.WriteString("intensity", r.Intensity.ToString().ToLowerInvariant());
                writer.WriteString("surface", r.Surface.ToString().ToLowerInvariant());
                break;

            case CardioExercise c:
                writer.WriteNumber("durationMinutes", c.DurationMinutes);
                if (c.DistanceKm.HasValue)
                    writer.WriteNumber("distanceKm", c.DistanceKm.Value);
                writer.WriteString("intensity", c.Intensity.ToString().ToLowerInvariant());
                writer.WriteString("cardioType", c.CardioType.ToString().ToLowerInvariant());
                break;

            case StaticExercise st:
                writer.WriteNumber("holdSeconds", st.HoldSeconds);
                writer.WriteNumber("sets", st.Sets);
                writer.WriteString("staticType", st.StaticType.ToString().ToLowerInvariant());
                break;
        }

        if (!string.IsNullOrEmpty(value.Description))
            writer.WriteString("description", value.Description);

        writer.WriteEndObject();
    }

    // Вспомогательные методы
    private string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }

    private int GetInt(JsonElement element, string propertyName, int defaultValue = 0)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
            ? prop.GetInt32()
            : defaultValue;
    }

    private float GetFloat(JsonElement element, string propertyName, float defaultValue = 0)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
            ? (float)prop.GetDouble()
            : defaultValue;
    }

    private Equipment ParseEquipment(string value) => value.ToLower() switch
    {
        "barbell" => Equipment.Barbell,
        "dumbbell" or "dumbbells" => Equipment.Dumbbell,
        "machine" => Equipment.Machine,
        "kettlebell" => Equipment.Kettlebell,
        "pullup" or "pull-up-bar" => Equipment.PullUpBar,
        "dips" or "parallelbars" => Equipment.ParallelBars,
        "resistance" => Equipment.Resistance,
        "cable" => Equipment.Cable,
        _ => Equipment.Bodyweight
    };

    private RunningSurface ParseRunningSurface(string value) => value.ToLower() switch
    {
        "treadmill" => RunningSurface.Treadmill,
        "track" => RunningSurface.Track,
        "road" => RunningSurface.Road,
        "trail" => RunningSurface.Trail,
        "trailhill" => RunningSurface.TrailHill,
        _ => RunningSurface.Treadmill
    };

    private CardioIntensity ParseIntensity(string value) => value.ToLower() switch
    {
        "low" => CardioIntensity.Low,
        "moderate" => CardioIntensity.Moderate,
        "high" => CardioIntensity.High,
        _ => CardioIntensity.Moderate
    };

    private CardioType ParseCardioType(string value) => value.ToLower() switch
    {
        "liss" => CardioType.LISS,
        "hiit" => CardioType.HIIT,
        _ => CardioType.LISS
    };

    private StaticType ParseStaticType(string value) => value.ToLower() switch
    {
        "plank" => StaticType.Plank,
        "stretching" => StaticType.Stretching,
        "yoga" => StaticType.Yoga,
        "balance" => StaticType.Balance,
        "wallsit" => StaticType.WallSit,
        "hollowhold" => StaticType.HollowHold,
        _ => StaticType.Plank
    };
}