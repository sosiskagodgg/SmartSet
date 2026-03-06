namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Типы упражнений в системе.
/// Соответствует значениям, которые хранятся в поле "ExerciseType" таблицы user_program_exercises.
/// </summary>
public enum ExerciseType
{
    Unknown = 0,

    /// <summary>Кардио</summary>
    Cardio,

    /// <summary>Силовые</summary>
    Strength,

    /// <summary>Гибкость / растяжка</summary>
    Flexibility,

    /// <summary>Статические упражнения (изометрия)</summary>
    Static
}
