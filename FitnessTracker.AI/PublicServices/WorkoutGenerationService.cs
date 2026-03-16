// FitnessTracker.AI/PublicServices/WorkoutGenerationService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;  // ← ДОБАВЛЕНО для AiOptions
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Enums;
using System.Text.Json;

namespace FitnessTracker.AI.PublicServices;

/// <summary>
/// Сервис для генерации тренировок через AI
/// </summary>
public class WorkoutGenerationService
{
    private readonly IUserWorkoutService _userWorkoutService;
    private readonly IUserService _userService;
    private readonly IUserParametersService _userParametersService;
    private readonly ILogger<WorkoutGenerationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public WorkoutGenerationService(
        IUserWorkoutService userWorkoutService,
        IUserService userService,
        IUserParametersService userParametersService,
        ILogger<WorkoutGenerationService> logger,
        IServiceProvider serviceProvider)
    {
        _userWorkoutService = userWorkoutService;
        _userService = userService;
        _userParametersService = userParametersService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Создать программу тренировок
    /// </summary>
    public async Task<bool> CreateWorkoutProgramAsync(
        long userId,
        int daysPerWeek,
        string goal,
        string experience,
        string location,
        int durationMinutes,
        string gender,
        CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();

        try
        {
            _logger.LogInformation("Creating workout program for user {UserId}: {DaysPerWeek} days, goal: {Goal}",
                userId, daysPerWeek, goal);

            var user = await _userService.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return false;
            }

            var userParams = await _userParametersService.GetUserParametersAsync(userId, ct);

            // Формируем промпт
            var prompt = BuildPrompt(user, userParams, daysPerWeek, goal, experience, location, durationMinutes, gender);

            // Отправляем запрос к AI
            var aiResponse = await aiProvider.AskStructuredAsync<List<WorkoutDay>>(
                prompt,
                new AiOptions
                {
                    SystemPrompt = "Ты - профессиональный фитнес-тренер. Составляй программы тренировок в JSON формате.",
                    Temperature = 0.3,
                    MaxTokens = 3000
                },
                ct);

            if (!aiResponse.IsSuccess || aiResponse.Data == null || !aiResponse.Data.Any())
            {
                _logger.LogWarning("Empty or invalid response from AI");
                return false;
            }

            // Удаляем старые тренировки
            var allWorkouts = await _userWorkoutService.GetAllUserWorkoutsAsync(userId, ct);
            foreach (var workout in allWorkouts)
            {
                await _userWorkoutService.DeleteUserWorkoutAsync(userId, workout.DayNumber, ct);
            }

            // Создаем новые тренировки
            foreach (var day in aiResponse.Data)
            {
                var exercises = ConvertToExercises(day.Exercises);
                if (exercises.Any())
                {
                    var dayName = GetDayName(day.DayNumber);
                    await _userWorkoutService.CreateOrUpdateUserWorkoutAsync(
                        userId,
                        day.DayNumber,
                        $"📅 {dayName} - {GetGoalName(goal)}",
                        exercises,
                        ct);
                }
            }

            _logger.LogInformation("Successfully created {DaysPerWeek} workouts for user {UserId}",
                daysPerWeek, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workout program for user {UserId}", userId);
            return false;
        }
    }

    private string BuildPrompt(
        User user,
        UserParameters? userParams,
        int daysPerWeek,
        string goal,
        string experience,
        string location,
        int durationMinutes,
        string gender)
    {
        return $@"
Составь программу тренировок на основе следующих данных:

Данные пользователя:
- Пол: {(gender == "male" ? "мужской" : gender == "female" ? "женский" : "другой")}
- Рост: {userParams?.Height?.ToString() ?? "не указан"} см
- Вес: {userParams?.Weight?.ToString() ?? "не указан"} кг
- Процент жира: {userParams?.BodyFat?.ToString() ?? "не указан"}%
- Цель: {goal}
- Уровень: {experience}
- Дней в неделю: {daysPerWeek}
- Место тренировок: {location}
- Длительность: {durationMinutes} мин

Доступное оборудование (в зависимости от места):
{(location switch
        {
            "street" => "- турник, брусья, скамья, свой вес (НЕТ: штанги, гантелей)",
            "home" => "- гантели, резинки, свой вес (НЕТ: штанги, тренажеров)",
            "gym" => "- всё оборудование доступно",
            _ => "- любое оборудование"
        })}

Требования к цели {goal}:
{(goal switch
        {
            "strength" => "- сила: 4-6 подходов × 4-8 повторений, вес 75-85%, отдых 2-3 мин",
            "mass" => "- масса: 3-4 подхода × 8-12 повторений, вес 65-75%, отдых 60-90 сек",
            "weight_loss" => "- похудение: 3-4 подхода × 12-20 повторений, вес 50-60%, отдых 30-45 сек, обязательно кардио",
            "endurance" => "- выносливость: 3-4 подхода × 15-25 повторений, вес 40-50%, отдых 30-45 сек",
            "tone" => "- тонус: 3 подхода × 12-15 повторений, отдых 60 сек",
            _ => "- умеренные нагрузки"
        })}

Верни JSON массив дней тренировок в формате:
[
  {{
    ""dayNumber"": 1,
    ""focus"": ""грудь+трицепс"",
    ""exercises"": [
      {{
        ""name"": ""Жим штанги лежа"",
        ""sets"": 4,
        ""reps"": 8,
        ""weight"": 60,
        ""muscleGroup"": ""chest"",
        ""equipment"": ""barbell""
      }}
    ]
  }}
]

Важно:
- Учитывай доступное оборудование
- Соблюдай требования к цели
- День 1 = понедельник, день 7 = воскресенье
- Не повторяй одинаковые упражнения
- Верни ТОЛЬКО JSON, без пояснений
";
    }

    // ИСПРАВЛЕНО: переименовал параметр исключения, чтобы не конфликтовать с переменной ex в цикле
    private List<Exercise> ConvertToExercises(List<ExerciseDto> exerciseDtos)
    {
        var exercises = new List<Exercise>();

        foreach (var exDto in exerciseDtos)  // ← переименовал ex → exDto
        {
            try
            {
                // Определяем тип упражнения по equipment
                if (exDto.Equipment?.ToLower() == "running" || exDto.Name?.ToLower().Contains("бег") == true)
                {
                    exercises.Add(new RunningExercise(
                        name: exDto.Name ?? "Бег",
                        met: 8.0f,
                        durationMinutes: 20,
                        distanceKm: 3,
                        surface: RunningSurface.Treadmill,
                        intensity: CardioIntensity.Moderate
                    ));
                }
                else
                {
                    // ИСПРАВЛЕНО: exerciseType → strengthExerciseType
                    exercises.Add(new StrengthExercise(
                        name: exDto.Name ?? "Упражнение",
                        met: 4.0f,
                        sets: exDto.Sets,
                        reps: exDto.Reps,
                        muscleGroup: exDto.MuscleGroup ?? "other",
                        strengthExerciseType: StrengthExerciseType.Compound,  // ← ИСПРАВЛЕНО
                        equipment: ParseEquipment(exDto.Equipment ?? "bodyweight"),
                        weightKg: exDto.Weight > 0 ? (decimal?)exDto.Weight : null
                    ));
                }
            }
            catch (Exception ex)  // ← это исключение не конфликтует
            {
                _logger.LogError(ex, "Error creating exercise {ExerciseName}", exDto.Name);
            }
        }

        return exercises;
    }

    private Equipment ParseEquipment(string equipment)
    {
        return equipment.ToLower() switch
        {
            "barbell" => Equipment.Barbell,
            "dumbbell" or "dumbbells" => Equipment.Dumbbell,
            "machine" => Equipment.Machine,
            "kettlebell" or "kettlebells" => Equipment.Kettlebell,
            "pullup" or "pull-up" or "pullupbar" => Equipment.PullUpBar,
            "bars" or "parallelbars" => Equipment.ParallelBars,
            "resistance" => Equipment.Resistance,
            "cable" or "cables" => Equipment.Cable,
            _ => Equipment.Bodyweight
        };
    }

    private string GetDayName(int day) => day switch
    {
        1 => "Понедельник",
        2 => "Вторник",
        3 => "Среда",
        4 => "Четверг",
        5 => "Пятница",
        6 => "Суббота",
        7 => "Воскресенье",
        _ => $"День {day}"
    };

    private string GetGoalName(string goal) => goal switch
    {
        "strength" => "Силовая",
        "mass" => "Масса",
        "weight_loss" => "Похудение",
        "endurance" => "Выносливость",
        "tone" => "Тонус",
        _ => goal
    };

    private class WorkoutDay
    {
        public int DayNumber { get; set; }
        public string Focus { get; set; } = string.Empty;
        public List<ExerciseDto> Exercises { get; set; } = new();
    }

    private class ExerciseDto
    {
        public string Name { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; }
        public string? MuscleGroup { get; set; }
        public string? Equipment { get; set; }
    }
}