using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IWorkoutTemplateService
{
    /// <summary>
    /// Получить все шаблоны тренировок
    /// </summary>
    List<WorkoutTemplate> GetAllTemplates();

    /// <summary>
    /// Получить шаблон по ID
    /// </summary>
    WorkoutTemplate? GetTemplateById(string id);

    /// <summary>
    /// Получить шаблоны по уровню (beginner/intermediate/advanced)
    /// </summary>
    List<WorkoutTemplate> GetTemplatesByLevel(string level);

    /// <summary>
    /// Получить шаблоны по цели (strength/hypertrophy/etc)
    /// </summary>
    List<WorkoutTemplate> GetTemplatesByGoal(string goal);

    /// <summary>
    /// Получить шаблоны по уровню и цели
    /// </summary>
    List<WorkoutTemplate> GetTemplatesByLevelAndGoal(string? level, string? goal);

    /// <summary>
    /// Найти наиболее подходящий шаблон
    /// </summary>
    WorkoutTemplate? FindBestMatch(string? level, string? goal, int? daysPerWeek);

    /// <summary>
    /// Перезагрузить шаблоны из файлов
    /// </summary>
    Task ReloadTemplatesAsync();
}