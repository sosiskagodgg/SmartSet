// FitnessTracker.ConsoleTest/TestScenarios/WorkoutGenerationScenario.cs
using FitnessTracker.AI.PublicServices;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.ConsoleTest.TestHelpers;

namespace FitnessTracker.ConsoleTest.TestScenarios;

public class WorkoutGenerationScenario
{
    private readonly WorkoutGenerationService _workoutGeneration;
    private readonly IUserWorkoutService _userWorkoutService;
    private readonly IUserParametersService _userParametersService;

    public WorkoutGenerationScenario(
        WorkoutGenerationService workoutGeneration,
        IUserWorkoutService userWorkoutService,
        IUserParametersService userParametersService)
    {
        _workoutGeneration = workoutGeneration;
        _userWorkoutService = userWorkoutService;
        _userParametersService = userParametersService;
    }

    public async Task RunAsync(long userId)
    {
        ConsoleHelper.WriteHeader("ТЕСТИРОВАНИЕ ГЕНЕРАЦИИ ТРЕНИРОВОК");

        while (true)
        {
            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1. 🤖 Сгенерировать новую программу");
            Console.WriteLine("2. 📋 Просмотреть мои тренировки");
            Console.WriteLine("3. 👀 Посмотреть детали тренировки");
            Console.WriteLine("4. 🗑️ Удалить тренировку");
            Console.WriteLine("5. 🧹 Очистить все тренировки");
            Console.WriteLine("0. 🔙 Назад");

            var choice = ConsoleHelper.ReadInput("Ваш выбор");

            switch (choice)
            {
                case "1":
                    await GenerateWorkoutProgram(userId);
                    break;
                case "2":
                    await ListWorkouts(userId);
                    break;
                case "3":
                    await ViewWorkoutDetails(userId);
                    break;
                case "4":
                    await DeleteWorkout(userId);
                    break;
                case "5":
                    await ClearAllWorkouts(userId);
                    break;
                case "0":
                    return;
                default:
                    ConsoleHelper.WriteError("Неверный выбор");
                    break;
            }

            ConsoleHelper.WaitForKey();
        }
    }

    private async Task GenerateWorkoutProgram(long userId)
    {
        ConsoleHelper.WriteHeader("ГЕНЕРАЦИЯ НОВОЙ ПРОГРАММЫ");

        // Показываем текущие параметры
        var currentParams = await _userParametersService.GetUserParametersAsync(userId);
        ConsoleHelper.WriteInfo("Текущие параметры:");
        Console.WriteLine($"  Рост: {currentParams?.Height ?? 0} см");
        Console.WriteLine($"  Вес: {currentParams?.Weight ?? 0} кг");
        Console.WriteLine($"  Процент жира: {currentParams?.BodyFat ?? 0}%");
        Console.WriteLine($"  Опыт: {currentParams?.Experience ?? "не указан"}");
        Console.WriteLine($"  Цель: {currentParams?.Goals ?? "не указана"}");

        if (currentParams == null)
        {
            ConsoleHelper.WriteWarning("Рекомендуется сначала заполнить параметры!");
            if (ConsoleHelper.ReadInput("Продолжить? (y/n)").ToLower() != "y")
                return;
        }

        // Сбор параметров для генерации
        int daysPerWeek;
        while (true)
        {
            var daysStr = ConsoleHelper.ReadInput("Сколько дней в неделю тренироваться? (2-6)");
            if (int.TryParse(daysStr, out daysPerWeek) && daysPerWeek >= 2 && daysPerWeek <= 6)
                break;
            ConsoleHelper.WriteError("Введите число от 2 до 6");
        }

        string goal;
        Console.WriteLine("\nВыберите цель:");
        Console.WriteLine("1. 💪 Сила (strength)");
        Console.WriteLine("2. 🏋️ Масса (mass)");
        Console.WriteLine("3. ⚖️ Похудение (weight_loss)");
        Console.WriteLine("4. ❤️ Выносливость (endurance)");
        Console.WriteLine("5. ✨ Тонус (tone)");

        var goalChoice = ConsoleHelper.ReadInput("Ваш выбор");
        goal = goalChoice switch
        {
            "1" => "strength",
            "2" => "mass",
            "3" => "weight_loss",
            "4" => "endurance",
            "5" => "tone",
            _ => "strength"
        };

        string experience;
        Console.WriteLine("\nВыберите уровень:");
        Console.WriteLine("1. 🌱 Начинающий (beginner)");
        Console.WriteLine("2. 🌿 Средний (intermediate)");
        Console.WriteLine("3. 🌳 Продвинутый (advanced)");

        var expChoice = ConsoleHelper.ReadInput("Ваш выбор");
        experience = expChoice switch
        {
            "1" => "beginner",
            "2" => "intermediate",
            "3" => "advanced",
            _ => "intermediate"
        };

        string location;
        Console.WriteLine("\nГде тренируетесь:");
        Console.WriteLine("1. 🏢 Фитнес-клуб (gym)");
        Console.WriteLine("2. 🏠 Дома (home)");
        Console.WriteLine("3. 🌳 Улица (street)");

        var locChoice = ConsoleHelper.ReadInput("Ваш выбор");
        location = locChoice switch
        {
            "1" => "gym",
            "2" => "home",
            "3" => "street",
            _ => "gym"
        };

        int duration;
        Console.WriteLine("\nДлительность тренировки (мин):");
        Console.WriteLine("1. ⏱️ 30 мин");
        Console.WriteLine("2. ⌛ 45 мин");
        Console.WriteLine("3. ⏰ 60 мин");
        Console.WriteLine("4. ⌛ 90 мин");

        var durChoice = ConsoleHelper.ReadInput("Ваш выбор");
        duration = durChoice switch
        {
            "1" => 30,
            "2" => 45,
            "3" => 60,
            "4" => 90,
            _ => 60
        };

        string gender;
        Console.WriteLine("\nВаш пол:");
        Console.WriteLine("1. 👨 Мужской (male)");
        Console.WriteLine("2. 👩 Женский (female)");

        var genderChoice = ConsoleHelper.ReadInput("Ваш выбор");
        gender = genderChoice switch
        {
            "1" => "male",
            "2" => "female",
            _ => "male"
        };

        ConsoleHelper.WriteInfo("\n🤔 Генерирую программу тренировок...");
        ConsoleHelper.WriteInfo("Это может занять 10-20 секунд...");

        var startTime = DateTime.Now;

        try
        {
            var success = await _workoutGeneration.CreateWorkoutProgramAsync(
                userId,
                daysPerWeek,
                goal,
                experience,
                location,
                duration,
                gender);

            var elapsed = DateTime.Now - startTime;

            if (success)
            {
                ConsoleHelper.WriteSuccess($"✅ Программа успешно создана за {elapsed.TotalSeconds:F1} сек!");

                // Показываем созданные тренировки
                await ListWorkouts(userId);
            }
            else
            {
                ConsoleHelper.WriteError("❌ Не удалось создать программу. Проверьте логи.");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
        }
    }

    private async Task ListWorkouts(long userId)
    {
        var workouts = await _userWorkoutService.GetAllUserWorkoutsAsync(userId);

        if (!workouts.Any())
        {
            ConsoleHelper.WriteWarning("У вас пока нет тренировок");
            return;
        }

        ConsoleHelper.WriteHeader("МОИ ТРЕНИРОВКИ");

        foreach (var workout in workouts.OrderBy(w => w.DayNumber))
        {
            var dayName = GetDayName(workout.DayNumber);
            Console.WriteLine($"📅 {dayName} (День {workout.DayNumber}): {workout.Name}");
            Console.WriteLine($"   Упражнений: {workout.Exercises.Count}");

            // Показываем первые 2 упражнения
            foreach (var ex in workout.Exercises.Take(2))
            {
                Console.WriteLine($"   • {ex.Name}");
            }
            if (workout.Exercises.Count > 2)
                Console.WriteLine($"   • ... и еще {workout.Exercises.Count - 2}");

            Console.WriteLine();
        }
    }

    private async Task ViewWorkoutDetails(long userId)
    {
        var workouts = await _userWorkoutService.GetAllUserWorkoutsAsync(userId);

        if (!workouts.Any())
        {
            ConsoleHelper.WriteWarning("У вас пока нет тренировок");
            return;
        }

        ConsoleHelper.WriteHeader("ВЫБЕРИТЕ ТРЕНИРОВКУ");

        foreach (var workout in workouts.OrderBy(w => w.DayNumber))
        {
            var dayName = GetDayName(workout.DayNumber);
            Console.WriteLine($"{workout.DayNumber}. {dayName} - {workout.Name}");
        }

        var choice = ConsoleHelper.ReadInput("Введите номер дня");
        if (!int.TryParse(choice, out int dayNumber))
        {
            ConsoleHelper.WriteError("Неверный номер");
            return;
        }

        var selected = workouts.FirstOrDefault(w => w.DayNumber == dayNumber);
        if (selected == null)
        {
            ConsoleHelper.WriteError("Тренировка не найдена");
            return;
        }

        ConsoleHelper.WriteHeader($"ТРЕНИРОВКА: {selected.Name}");
        Console.WriteLine($"День {selected.DayNumber} - {GetDayName(selected.DayNumber)}");
        Console.WriteLine($"\nУпражнения ({selected.Exercises.Count}):");

        var index = 1;
        foreach (var ex in selected.Exercises)
        {
            Console.WriteLine($"\n{index}. {FormatExerciseDetails(ex)}");
            index++;
        }
    }

    private async Task DeleteWorkout(long userId)
    {
        var workouts = await _userWorkoutService.GetAllUserWorkoutsAsync(userId);

        if (!workouts.Any())
        {
            ConsoleHelper.WriteWarning("У вас пока нет тренировок");
            return;
        }

        ConsoleHelper.WriteHeader("УДАЛЕНИЕ ТРЕНИРОВКИ");

        foreach (var workout in workouts.OrderBy(w => w.DayNumber))
        {
            var dayName = GetDayName(workout.DayNumber);
            Console.WriteLine($"{workout.DayNumber}. {dayName} - {workout.Name}");
        }

        var choice = ConsoleHelper.ReadInput("Введите номер дня для удаления");
        if (!int.TryParse(choice, out int dayNumber))
        {
            ConsoleHelper.WriteError("Неверный номер");
            return;
        }

        if (ConsoleHelper.ReadInput($"Удалить тренировку за {GetDayName(dayNumber)}? (y/n)").ToLower() == "y")
        {
            await _userWorkoutService.DeleteUserWorkoutAsync(userId, dayNumber);
            ConsoleHelper.WriteSuccess("Тренировка удалена");
        }
    }

    private async Task ClearAllWorkouts(long userId)
    {
        var workouts = await _userWorkoutService.GetAllUserWorkoutsAsync(userId);

        if (!workouts.Any())
        {
            ConsoleHelper.WriteWarning("У вас пока нет тренировок");
            return;
        }

        ConsoleHelper.WriteWarning($"Будет удалено {workouts.Count} тренировок!");

        if (ConsoleHelper.ReadInput("Вы уверены? (y/n)").ToLower() == "y")
        {
            foreach (var workout in workouts)
            {
                await _userWorkoutService.DeleteUserWorkoutAsync(userId, workout.DayNumber);
            }
            ConsoleHelper.WriteSuccess("Все тренировки удалены");
        }
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

    private string FormatExerciseDetails(Exercise ex)
    {
        return ex switch
        {
            StrengthExercise s => $"💪 {s.Name} - {s.Sets}×{s.Reps} · {s.Weight?.Kilograms ?? 0} кг · {GetMuscleGroupName(s.MuscleGroup)}",
            RunningExercise r => $"🏃 {r.Name} - {r.DurationMinutes} мин · {r.DistanceKm} км · темп {r.Pace?.ToString("F1") ?? "?"} мин/км",
            CardioExercise c => $"❤️ {c.Name} - {c.DurationMinutes} мин · {GetIntensityName(c.Intensity)}",
            StaticExercise st => $"🧘 {st.Name} - {st.Sets}×{st.HoldSeconds} сек",
            _ => ex.Name
        };
    }

    private string GetMuscleGroupName(string muscleGroup) => muscleGroup?.ToLower() switch
    {
        "chest" => "Грудные",
        "back" => "Спина",
        "legs" => "Ноги",
        "shoulders" => "Плечи",
        "arms" => "Руки",
        "abs" => "Пресс",
        _ => muscleGroup ?? "Не указано"
    };

    private string GetIntensityName(Domain.Enums.CardioIntensity intensity) => intensity switch
    {
        Domain.Enums.CardioIntensity.Low => "Низкая",
        Domain.Enums.CardioIntensity.Moderate => "Средняя",
        Domain.Enums.CardioIntensity.High => "Высокая",
        _ => intensity.ToString()
    };
}