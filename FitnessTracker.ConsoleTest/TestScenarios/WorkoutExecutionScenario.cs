// FitnessTracker.ConsoleTest/TestScenarios/WorkoutExecutionScenario.cs
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.ConsoleTest.TestHelpers;

namespace FitnessTracker.ConsoleTest.TestScenarios;

public class WorkoutExecutionScenario
{
    private readonly IUserWorkoutService _userWorkoutService;
    private readonly IWorkoutService _workoutService;

    public WorkoutExecutionScenario(
        IUserWorkoutService userWorkoutService,
        IWorkoutService workoutService)
    {
        _userWorkoutService = userWorkoutService;
        _workoutService = workoutService;
    }

    public async Task RunAsync(long userId)
    {
        ConsoleHelper.WriteHeader("ТЕСТИРОВАНИЕ ВЫПОЛНЕНИЯ ТРЕНИРОВОК");

        while (true)
        {
            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1. 🏋️ Начать тренировку на сегодня");
            Console.WriteLine("2. 📋 Мои тренировки на сегодня");
            Console.WriteLine("3. 📊 История выполненных тренировок");
            Console.WriteLine("4. 👀 Просмотреть выполненную тренировку");
            Console.WriteLine("5. 🗑️ Удалить выполненную тренировку");
            Console.WriteLine("0. 🔙 Назад");

            var choice = ConsoleHelper.ReadInput("Ваш выбор");

            switch (choice)
            {
                case "1":
                    await StartTodaysWorkout(userId);
                    break;
                case "2":
                    await ViewTodaysWorkout(userId);
                    break;
                case "3":
                    await ViewWorkoutHistory(userId);
                    break;
                case "4":
                    await ViewCompletedWorkout(userId);
                    break;
                case "5":
                    await DeleteCompletedWorkout(userId);
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

    private async Task StartTodaysWorkout(long userId)
    {
        var today = DateTime.UtcNow.Date;
        var todayWorkout = await _workoutService.GetWorkoutByDateAsync(userId, today);

        if (todayWorkout != null)
        {
            ConsoleHelper.WriteWarning("На сегодня уже есть выполненная тренировка!");
            if (ConsoleHelper.ReadInput("Хотите начать заново? (y/n)").ToLower() != "y")
                return;

            await _workoutService.DeleteWorkoutAsync(userId, today);
        }

        // Получаем шаблон тренировки на сегодня
        var dayOfWeek = (int)today.DayOfWeek;
        var dayNumber = dayOfWeek == 0 ? 7 : dayOfWeek; // Воскресенье = 7

        var template = await _userWorkoutService.GetUserWorkoutAsync(userId, dayNumber);

        if (template == null)
        {
            ConsoleHelper.WriteError("У вас нет тренировки на сегодня!");
            ConsoleHelper.WriteInfo("Сначала создайте программу тренировок или выберите другой день.");
            return;
        }

        ConsoleHelper.WriteHeader($"ТРЕНИРОВКА НА СЕГОДНЯ: {template.Name}");
        Console.WriteLine($"День {dayNumber} - {GetDayName(dayNumber)}");
        Console.WriteLine($"\nУпражнений: {template.Exercises.Count}\n");

        // Создаем новую тренировку на сегодня
        var workout = await _workoutService.CreateWorkoutAsync(userId, today, template.Exercises);

        await SimulateWorkoutExecution(userId, workout, template.Exercises.ToList());
    }

    private async Task SimulateWorkoutExecution(long userId, Workout workout, List<Exercise> exercises)
    {
        var completedExercises = new List<Exercise>();
        var totalSets = 0;
        var completedSets = 0;

        foreach (var exercise in exercises)
        {
            ConsoleHelper.WriteHeader($"УПРАЖНЕНИЕ: {exercise.Name}");
            Console.WriteLine(FormatExerciseDetails(exercise));

            var sets = GetExerciseSets(exercise);
            totalSets += sets;

            Console.WriteLine($"\nПодходов: {sets}");

            for (int set = 1; set <= sets; set++)
            {
                Console.Write($"\nПодход {set}/{sets}. Выполнить? (y/n/[вес]): ");
                var input = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (input == "y" || input == "yes" || input == "да" || input == "")
                {
                    completedSets++;

                    // Если ввели вес, сохраняем его
                    if (decimal.TryParse(input, out var weight) && exercise is StrengthExercise s)
                    {
                        ConsoleHelper.WriteInfo($"Вес записан: {weight} кг");
                    }

                    ConsoleHelper.WriteSuccess($"✅ Подход {set} выполнен!");
                }
                else
                {
                    ConsoleHelper.WriteWarning($"⚠️ Подход {set} пропущен");
                }
            }

            completedExercises.Add(exercise);
            ConsoleHelper.WriteSuccess($"\n✅ Упражнение завершено!");
        }

        // Обновляем тренировку с выполненными упражнениями
        await _workoutService.UpdateWorkoutAsync(userId, workout.Date, completedExercises);

        ConsoleHelper.WriteHeader("🎉 ТРЕНИРОВКА ЗАВЕРШЕНА!");
        Console.WriteLine($"Выполнено упражнений: {completedExercises.Count}");
        Console.WriteLine($"Выполнено подходов: {completedSets}/{totalSets}");

        // Рассчитываем примерные калории
        var calories = EstimateCalories(completedExercises);
        Console.WriteLine($"🔥 Сожжено калорий: ~{calories} ккал");
    }

    private async Task ViewTodaysWorkout(long userId)
    {
        var today = DateTime.UtcNow.Date;
        var workout = await _workoutService.GetWorkoutByDateAsync(userId, today);

        if (workout == null)
        {
            ConsoleHelper.WriteInfo("На сегодня нет выполненных тренировок");

            // Проверяем, есть ли шаблон на сегодня
            var dayOfWeek = (int)today.DayOfWeek;
            var dayNumber = dayOfWeek == 0 ? 7 : dayOfWeek;

            var template = await _userWorkoutService.GetUserWorkoutAsync(userId, dayNumber);

            if (template != null)
            {
                ConsoleHelper.WriteInfo($"Есть запланированная тренировка: {template.Name}");
                if (ConsoleHelper.ReadInput("Начать её сейчас?").ToLower() == "y")
                {
                    await StartTodaysWorkout(userId);
                }
            }
            return;
        }

        ConsoleHelper.WriteHeader($"ТРЕНИРОВКА НА {today:dd.MM.yyyy}");
        Console.WriteLine($"Статус: {GetStatusName(workout.Status)}");
        Console.WriteLine($"Упражнений выполнено: {workout.Exercises.Count}\n");

        var index = 1;
        foreach (var ex in workout.Exercises)
        {
            Console.WriteLine($"{index}. {FormatExerciseDetails(ex)}");
            index++;
        }

        if (!string.IsNullOrEmpty(workout.Notes))
        {
            Console.WriteLine($"\nЗаметки: {workout.Notes}");
        }
    }

    private async Task ViewWorkoutHistory(long userId)
    {
        var workouts = await _workoutService.GetUserWorkoutsAsync(userId, 50);

        if (!workouts.Any())
        {
            ConsoleHelper.WriteWarning("История тренировок пуста");
            return;
        }

        ConsoleHelper.WriteHeader("ИСТОРИЯ ТРЕНИРОВОК");

        foreach (var workout in workouts.OrderByDescending(w => w.Date))
        {
            var status = workout.Status == "completed" ? "✅" : "⏳";
            Console.WriteLine($"{status} {workout.Date:dd.MM.yyyy} - {workout.Exercises.Count} упражнений");
        }
    }

    private async Task ViewCompletedWorkout(long userId)
    {
        var workouts = await _workoutService.GetUserWorkoutsAsync(userId, 50);

        if (!workouts.Any())
        {
            ConsoleHelper.WriteWarning("История тренировок пуста");
            return;
        }

        ConsoleHelper.WriteHeader("ВЫБЕРИТЕ ДАТУ");

        var dates = workouts.OrderByDescending(w => w.Date).Take(10).ToList();
        for (int i = 0; i < dates.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {dates[i].Date:dd.MM.yyyy} - {dates[i].Exercises.Count} упражнений");
        }

        var choice = ConsoleHelper.ReadInput("Введите номер");
        if (!int.TryParse(choice, out int index) || index < 1 || index > dates.Count)
        {
            ConsoleHelper.WriteError("Неверный выбор");
            return;
        }

        var selected = dates[index - 1];

        ConsoleHelper.WriteHeader($"ТРЕНИРОВКА ОТ {selected.Date:dd.MM.yyyy}");

        var exerciseIndex = 1;
        foreach (var ex in selected.Exercises)
        {
            Console.WriteLine($"\n{exerciseIndex}. {FormatExerciseDetails(ex)}");
            exerciseIndex++;
        }
    }

    private async Task DeleteCompletedWorkout(long userId)
    {
        var workouts = await _workoutService.GetUserWorkoutsAsync(userId, 50);

        if (!workouts.Any())
        {
            ConsoleHelper.WriteWarning("История тренировок пуста");
            return;
        }

        ConsoleHelper.WriteHeader("ВЫБЕРИТЕ ТРЕНИРОВКУ ДЛЯ УДАЛЕНИЯ");

        var dates = workouts.OrderByDescending(w => w.Date).Take(10).ToList();
        for (int i = 0; i < dates.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {dates[i].Date:dd.MM.yyyy} - {dates[i].Exercises.Count} упражнений");
        }

        var choice = ConsoleHelper.ReadInput("Введите номер");
        if (!int.TryParse(choice, out int index) || index < 1 || index > dates.Count)
        {
            ConsoleHelper.WriteError("Неверный выбор");
            return;
        }

        var selected = dates[index - 1];

        if (ConsoleHelper.ReadInput($"Удалить тренировку от {selected.Date:dd.MM.yyyy}? (y/n)").ToLower() == "y")
        {
            await _workoutService.DeleteWorkoutAsync(userId, selected.Date);
            ConsoleHelper.WriteSuccess("Тренировка удалена");
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
            StrengthExercise s => $"💪 {s.Name} - {s.Sets}×{s.Reps} · {s.Weight?.Kilograms ?? 0} кг",
            RunningExercise r => $"🏃 {r.Name} - {r.DurationMinutes} мин · {r.DistanceKm} км",
            CardioExercise c => $"❤️ {c.Name} - {c.DurationMinutes} мин",
            StaticExercise st => $"🧘 {st.Name} - {st.Sets}×{st.HoldSeconds} сек",
            _ => ex.Name
        };
    }

    private int GetExerciseSets(Exercise ex) => ex switch
    {
        StrengthExercise s => s.Sets,
        RunningExercise _ => 1,
        CardioExercise _ => 1,
        StaticExercise st => st.Sets,
        _ => 1
    };

    private int EstimateCalories(List<Exercise> exercises)
    {
        int total = 0;
        foreach (var ex in exercises)
        {
            switch (ex)
            {
                case StrengthExercise s:
                    total += (int)(s.MET * 3); // ~3 мин на подход
                    break;
                case CardioExercise c:
                    total += (int)(c.MET * c.DurationMinutes * 0.1);
                    break;
            }
        }
        return total;
    }

    private string GetStatusName(string status) => status switch
    {
        "planned" => "⏳ Запланирована",
        "completed" => "✅ Выполнена",
        "cancelled" => "❌ Отменена",
        _ => status
    };
}