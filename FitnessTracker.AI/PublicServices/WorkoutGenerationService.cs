// FitnessTracker.AI/PublicServices/WorkoutGenerationService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;

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

            if (!aiResponse.IsSuccess || aiResponse.Data == null)
            {
                _logger.LogWarning("AI response failed: {Error}", aiResponse.Error);
                return false;
            }

            _logger.LogInformation("AI returned {Count} days", aiResponse.Data.Count);

            // СОЗДАЕМ ПРАВИЛЬНОЕ РАСПИСАНИЕ с днями отдыха
            var workoutDays = CreateWorkoutSchedule(aiResponse.Data, daysPerWeek);

            if (!workoutDays.Any())
            {
                _logger.LogWarning("No valid workout days after processing");
                return false;
            }

            // Удаляем старые тренировки
            var allWorkouts = await _userWorkoutService.GetAllUserWorkoutsAsync(userId, ct);
            foreach (var workout in allWorkouts)
            {
                await _userWorkoutService.DeleteUserWorkoutAsync(userId, workout.DayNumber, ct);
            }

            // Создаем новые тренировки
            int createdCount = 0;
            foreach (var day in workoutDays)
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

                    createdCount++;
                    _logger.LogInformation("Created workout for day {DayNumber} ({DayName}) with {Count} exercises",
                        day.DayNumber, dayName, exercises.Count);
                }
            }

            _logger.LogInformation("Successfully created {CreatedCount} workouts for user {UserId}",
                createdCount, userId);

            return createdCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workout program for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Создает правильное расписание с днями отдыха
    /// </summary>
    private List<WorkoutDay> CreateWorkoutSchedule(List<WorkoutDay> aiDays, int daysPerWeek)
    {
        var result = new List<WorkoutDay>();

        // Оптимальное расписание для разного количества дней
        var schedule = daysPerWeek switch
        {
            2 => new[] { 2, 5 },           // Вторник, Пятница
            3 => new[] { 1, 3, 5 },         // Пн, Ср, Пт
            4 => new[] { 1, 2, 4, 6 },      // Пн, Вт, Чт, Сб
            5 => new[] { 1, 2, 3, 5, 6 },   // Пн, Вт, Ср, Пт, Сб
            6 => new[] { 1, 2, 3, 4, 5, 6 },// Пн-Сб, Вс отдых
            _ => new[] { 1, 3, 5 }          // По умолчанию 3 дня
        };

        _logger.LogInformation("Creating schedule for {DaysPerWeek} days: [{Schedule}]",
            daysPerWeek, string.Join(", ", schedule));

        // Берем дни из AI ответа и распределяем по расписанию
        for (int i = 0; i < Math.Min(schedule.Length, aiDays.Count); i++)
        {
            var aiDay = aiDays[i];
            var targetDayNumber = schedule[i];

            var workoutDay = new WorkoutDay
            {
                DayNumber = targetDayNumber,
                Focus = aiDay.Focus ?? GetDefaultFocus(targetDayNumber),
                Exercises = aiDay.Exercises ?? new List<ExerciseDto>()
            };

            // Если упражнений нет, создаем заглушку
            if (workoutDay.Exercises == null || !workoutDay.Exercises.Any())
            {
                workoutDay.Exercises = CreateDefaultExercises(targetDayNumber);
            }

            result.Add(workoutDay);
            _logger.LogInformation("Assigned AI day {AIIndex} to day {DayNumber} with {Count} exercises",
                i + 1, targetDayNumber, workoutDay.Exercises.Count);
        }

        return result;
    }

    private string GetDefaultFocus(int dayNumber) => dayNumber switch
    {
        1 => "Грудь + Трицепс",
        2 => "Спина + Бицепс",
        3 => "Ноги + Плечи",
        4 => "Грудь + Спина",
        5 => "Плечи + Руки",
        6 => "Ноги + Кардио",
        7 => "Отдых",
        _ => "Тренировка"
    };

    private List<ExerciseDto> CreateDefaultExercises(int dayNumber)
    {
        var exercises = new List<ExerciseDto>();

        switch (dayNumber)
        {
            case 1: // Грудь + Трицепс
                exercises.Add(new ExerciseDto { Name = "Жим штанги лежа", Sets = 4, Reps = 8, Weight = 60, MuscleGroup = "chest", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Жим гантелей на наклонной", Sets = 3, Reps = 10, Weight = 25, MuscleGroup = "chest", Equipment = "dumbbell" });
                exercises.Add(new ExerciseDto { Name = "Отжимания на брусьях", Sets = 3, Reps = 10, MuscleGroup = "triceps", Equipment = "dips" });
                exercises.Add(new ExerciseDto { Name = "Французский жим", Sets = 3, Reps = 12, Weight = 30, MuscleGroup = "triceps", Equipment = "dumbbell" });
                break;

            case 2: // Спина + Бицепс
                exercises.Add(new ExerciseDto { Name = "Подтягивания широким хватом", Sets = 4, Reps = 8, MuscleGroup = "back", Equipment = "pull-up-bar" });
                exercises.Add(new ExerciseDto { Name = "Тяга штанги в наклоне", Sets = 4, Reps = 8, Weight = 60, MuscleGroup = "back", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Подъем штанги на бицепс", Sets = 3, Reps = 10, Weight = 40, MuscleGroup = "biceps", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Молотковые сгибания", Sets = 3, Reps = 12, Weight = 20, MuscleGroup = "biceps", Equipment = "dumbbell" });
                break;

            case 3: // Ноги + Плечи
                exercises.Add(new ExerciseDto { Name = "Приседания со штангой", Sets = 4, Reps = 8, Weight = 80, MuscleGroup = "legs", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Жим ногами", Sets = 4, Reps = 10, Weight = 150, MuscleGroup = "legs", Equipment = "machine" });
                exercises.Add(new ExerciseDto { Name = "Армейский жим", Sets = 3, Reps = 8, Weight = 40, MuscleGroup = "shoulders", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Махи гантелями в стороны", Sets = 3, Reps = 12, Weight = 15, MuscleGroup = "shoulders", Equipment = "dumbbell" });
                break;

            case 4: // Грудь + Спина
                exercises.Add(new ExerciseDto { Name = "Жим штанги лежа", Sets = 4, Reps = 8, Weight = 60, MuscleGroup = "chest", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Тяга штанги в наклоне", Sets = 4, Reps = 8, Weight = 60, MuscleGroup = "back", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Жим гантелей", Sets = 3, Reps = 10, Weight = 25, MuscleGroup = "chest", Equipment = "dumbbell" });
                exercises.Add(new ExerciseDto { Name = "Подтягивания", Sets = 3, Reps = 8, MuscleGroup = "back", Equipment = "pull-up-bar" });
                break;

            case 5: // Плечи + Руки
                exercises.Add(new ExerciseDto { Name = "Жим гантелей сидя", Sets = 4, Reps = 8, Weight = 25, MuscleGroup = "shoulders", Equipment = "dumbbell" });
                exercises.Add(new ExerciseDto { Name = "Подъем штанги на бицепс", Sets = 3, Reps = 10, Weight = 40, MuscleGroup = "biceps", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Французский жим", Sets = 3, Reps = 10, Weight = 30, MuscleGroup = "triceps", Equipment = "dumbbell" });
                exercises.Add(new ExerciseDto { Name = "Махи гантелями", Sets = 3, Reps = 12, Weight = 15, MuscleGroup = "shoulders", Equipment = "dumbbell" });
                break;

            case 6: // Ноги + Кардио
                exercises.Add(new ExerciseDto { Name = "Приседания", Sets = 4, Reps = 10, Weight = 60, MuscleGroup = "legs", Equipment = "barbell" });
                exercises.Add(new ExerciseDto { Name = "Выпады с гантелями", Sets = 3, Reps = 12, Weight = 20, MuscleGroup = "legs", Equipment = "dumbbell" });
                exercises.Add(new ExerciseDto { Name = "Бег на дорожке", Sets = 1, Reps = 20, Equipment = "running", MuscleGroup = "cardio" });
                break;

            default: // Заглушка
                exercises.Add(new ExerciseDto { Name = "Базовое упражнение", Sets = 3, Reps = 10, Weight = 40, MuscleGroup = "other", Equipment = "barbell" });
                break;
        }

        return exercises;
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
Ты - профессиональный фитнес-тренер. Составь программу тренировок на {daysPerWeek} дней в неделю.

Данные пользователя:
- Пол: {(gender == "male" ? "мужской" : gender == "female" ? "женский" : "другой")}
- Рост: {userParams?.Height?.ToString() ?? "не указан"} см
- Вес: {userParams?.Weight?.ToString() ?? "не указан"} кг
- Цель: {GetGoalDescription(goal)}
- Уровень: {GetExperienceDescription(experience)}
- Место тренировок: {GetLocationDescription(location)}
- Длительность тренировки: {durationMinutes} минут

Оборудование: {GetEquipmentDescription(location)}

Требования к цели {goal}:
{GetGoalRequirements(goal)}

Создай ровно {daysPerWeek} тренировочных дней (без дней отдыха).
Каждый тренировочный день должен содержать 4-6 упражнений.

Верни ТОЛЬКО JSON массив в таком формате (без пояснений):

[
  {{
    ""dayNumber"": 1,
    ""focus"": ""Грудь + Трицепс"",
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

ВАЖНО: 
- Дни должны идти по порядку: 1, 2, 3, 4, 5, 6, 7
- НИКОГДА не используй dayNumber = 0
- Не включай дни отдыха в программу
- Верни ТОЛЬКО JSON, без пояснений
";
    }

    private List<Exercise> ConvertToExercises(List<ExerciseDto> exerciseDtos)
    {
        var exercises = new List<Exercise>();

        if (exerciseDtos == null || !exerciseDtos.Any())
        {
            _logger.LogWarning("ConvertToExercises received empty list");
            return exercises;
        }

        foreach (var exDto in exerciseDtos)
        {
            try
            {
                if (string.IsNullOrEmpty(exDto.Name))
                {
                    _logger.LogWarning("Exercise has no name, skipping");
                    continue;
                }

                // Определяем тип упражнения
                if (exDto.Equipment?.ToLower() == "running" || exDto.Name?.ToLower().Contains("бег") == true)
                {
                    exercises.Add(new RunningExercise(
                        name: exDto.Name,
                        met: 8.0f,
                        durationMinutes: 20,
                        distanceKm: 3,
                        surface: RunningSurface.Treadmill,
                        intensity: CardioIntensity.Moderate
                    ));
                }
                else
                {
                    var equipment = ParseEquipment(exDto.Equipment ?? "bodyweight");
                    var weight = exDto.Weight > 0 ? (decimal?)exDto.Weight : null;

                    exercises.Add(new StrengthExercise(
                        name: exDto.Name,
                        met: 4.0f,
                        sets: exDto.Sets > 0 ? exDto.Sets : 3,
                        reps: exDto.Reps > 0 ? exDto.Reps : 10,
                        muscleGroup: string.IsNullOrEmpty(exDto.MuscleGroup) ? "other" : exDto.MuscleGroup,
                        strengthExerciseType: StrengthExerciseType.Compound,
                        equipment: equipment,
                        weightKg: weight
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exercise {ExerciseName}", exDto.Name);
            }
        }

        _logger.LogInformation("Converted {Count} exercises", exercises.Count);
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
            "pullup" or "pull-up" or "pullupbar" or "pull-up-bar" => Equipment.PullUpBar,
            "bars" or "parallelbars" or "dips" => Equipment.ParallelBars,
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

    private string GetGoalDescription(string goal) => goal switch
    {
        "strength" => "увеличение силы",
        "mass" => "набор мышечной массы",
        "weight_loss" => "похудение",
        "endurance" => "повышение выносливости",
        "tone" => "поддержание тонуса",
        _ => goal
    };

    private string GetExperienceDescription(string experience) => experience switch
    {
        "beginner" => "начинающий",
        "intermediate" => "средний",
        "advanced" => "продвинутый",
        _ => experience
    };

    private string GetLocationDescription(string location) => location switch
    {
        "gym" => "фитнес-клуб",
        "home" => "дома",
        "street" => "на улице",
        _ => location
    };

    private string GetEquipmentDescription(string location) => location switch
    {
        "street" => "турник, брусья, скамья, свой вес",
        "home" => "гантели, резинки, свой вес",
        "gym" => "все оборудование доступно",
        _ => "базовое оборудование"
    };

    private string GetGoalRequirements(string goal) => goal switch
    {
        "strength" => "4-5 подходов × 4-6 повторений, вес 80-85%",
        "mass" => "3-4 подхода × 8-12 повторений, вес 70-75%",
        "weight_loss" => "3-4 подхода × 15-20 повторений, вес 50-60%, добавить кардио",
        "endurance" => "3-4 подхода × 15-25 повторений, вес 40-50%",
        "tone" => "3 подхода × 12-15 повторений, вес 60-70%",
        _ => "умеренные нагрузки"
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