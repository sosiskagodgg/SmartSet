// FitnessTracker.TelegramBot/Commands/WorkoutExecutionCommand.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Enums;
using FitnessTracker.Domain.ValueObjects;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для выполнения тренировки (пошаговое выполнение упражнений)
/// </summary>
public class WorkoutExecutionCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly IUserWorkoutService _userWorkoutService;
    private readonly IWorkoutService _workoutService;
    private readonly ILogger<WorkoutExecutionCommand> _logger;
    private IUserStateManager? _stateManager;

    public string Name => "workout_execute";

    private static class Const
    {
        public static class Action
        {
            public const string Start = "start";
            public const string CompleteSet = "complete_set";
            public const string Next = "next";
            public const string Prev = "prev";
            public const string Finish = "finish";
        }

        public static class State
        {
            public const string Executing = "executing";
        }

        public static class Text
        {
            public const string WorkoutDisplay = """
                🏋️ <b>{workoutName}</b>
                
                День {dayNumber} | Упражнение {currentIndex} из {total}
                
                {exerciseInfo}
                
                {progressBar}
                """;

            public const string WorkoutCompleted = """
                🎉 <b>Тренировка завершена!</b>
                
                Ты отлично поработал! 🏆
                
                ✅ Выполнено упражнений: {completedExercises}
                🔥 Всего подходов: {totalSets}
                
                Хочешь посмотреть статистику?
                """;

            public const string WorkoutNotFound = "❌ Тренировка не найдена";
            public const string ButtonNext = "▶️ Далее";
            public const string ButtonPrev = "◀️ Назад";
            public const string ButtonFinish = "🏁 Завершить";
            public const string ButtonStats = "📊 Статистика";
        }

        public static Keyboard AfterWorkoutKeyboard()
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonStats, "stats:workout")
                },
                new List<Button> {
                    Models.Button.Create("◀️ К тренировкам", "workouts:main")
                }
            );
        }
    }

    public WorkoutExecutionCommand(
        ITelegramBotAdapter adapter,
        IUserWorkoutService userWorkoutService,
        IWorkoutService workoutService,
        ILogger<WorkoutExecutionCommand> logger)
    {
        Console.WriteLine($"\n🎯 WorkoutExecutionCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  userWorkoutService is null: {userWorkoutService == null}");
        Console.WriteLine($"  workoutService is null: {workoutService == null}");
        Console.WriteLine($"  logger is null: {logger == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _userWorkoutService = userWorkoutService ?? throw new ArgumentNullException(nameof(userWorkoutService));
        _workoutService = workoutService ?? throw new ArgumentNullException(nameof(workoutService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Console.WriteLine($"✅ WorkoutExecutionCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"🎯 WorkoutExecutionCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText) => false;

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
        => await Task.CompletedTask;

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WorkoutExecutionCommand received: {Action} with data: {Data}",
            context.Action, string.Join(",", context.Parameters));

        try
        {
            if (context.Parameters.Length == 0)
            {
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                return;
            }

            var dayNumber = int.Parse(context.Parameters[0]);

            switch (context.Action)
            {
                case Const.Action.Start:
                    await StartWorkout(context.UserId, context.MessageId, dayNumber, cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.CompleteSet:
                    if (context.Parameters.Length >= 3)
                    {
                        var exerciseIndex = int.Parse(context.Parameters[1]);
                        var setNumber = int.Parse(context.Parameters[2]);
                        await CompleteSet(context.UserId, context.MessageId, dayNumber, exerciseIndex, setNumber, context.CallbackQueryId, cancellationToken);
                    }
                    break;

                case Const.Action.Next:
                    if (context.Parameters.Length >= 2)
                    {
                        var nextPage = int.Parse(context.Parameters[1]);
                        await ShowExercisePage(context.UserId, context.MessageId, dayNumber, nextPage, cancellationToken);
                        await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    }
                    break;

                case Const.Action.Prev:
                    if (context.Parameters.Length >= 2)
                    {
                        var prevPage = int.Parse(context.Parameters[1]);
                        await ShowExercisePage(context.UserId, context.MessageId, dayNumber, prevPage, cancellationToken);
                        await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    }
                    break;

                case Const.Action.Finish:
                    await FinishWorkout(context.UserId, context.MessageId, dayNumber, cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, "🎉 Тренировка завершена!", false, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WorkoutExecutionCommand");
            await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
        => await Task.CompletedTask;

    private async Task StartWorkout(long userId, int messageId, int dayNumber, CancellationToken cancellationToken)
    {
        if (_stateManager == null)
        {
            _logger.LogError("StateManager not initialized for WorkoutExecutionCommand");
            return;
        }

        var workout = await _userWorkoutService.GetUserWorkoutAsync(userId, dayNumber, cancellationToken);
        if (workout == null || !workout.Exercises.Any())
        {
            await _adapter.EditMessageAsync(userId, messageId, Const.Text.WorkoutNotFound, cancellationToken: cancellationToken);
            return;
        }

        var today = DateTime.UtcNow.Date;
        var todayWorkout = await _workoutService.GetWorkoutByDateAsync(userId, today, cancellationToken);
        if (todayWorkout == null)
        {
            todayWorkout = await _workoutService.CreateWorkoutAsync(userId, today, new List<Exercise>(), cancellationToken);
        }

        var stateData = new Dictionary<string, object>
        {
            ["workout_name"] = workout.Name,
            ["day_number"] = dayNumber,
            ["total_exercises"] = workout.Exercises.Count,
            ["current_page"] = 0,
            ["completed_sets"] = new Dictionary<int, List<int>>(),
            ["started_at"] = DateTime.UtcNow
        };

        _stateManager.SetState(userId, new UserState
        {
            CommandName = Name,
            Step = Const.State.Executing,
            Data = stateData
        });

        await ShowExercisePage(userId, messageId, dayNumber, 0, cancellationToken);
    }

    private async Task ShowExercisePage(long userId, int messageId, int dayNumber, int pageIndex, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var workout = await _userWorkoutService.GetUserWorkoutAsync(userId, dayNumber, cancellationToken);
        var state = _stateManager.GetState(userId);

        if (workout == null || state?.Data == null || pageIndex < 0 || pageIndex >= workout.Exercises.Count)
        {
            return;
        }

        var exercise = workout.Exercises.ElementAt(pageIndex);
        var totalSets = GetTotalSets(exercise);
        var completedSets = GetCompletedSetsFromState(state, pageIndex);

        var keyboard = BuildExerciseKeyboard(dayNumber, pageIndex, totalSets, completedSets,
            pageIndex > 0, pageIndex < workout.Exercises.Count - 1);

        var text = FormatExerciseDisplay(
            (string)state.Data["workout_name"],
            dayNumber,
            exercise,
            pageIndex + 1,
            workout.Exercises.Count,
            completedSets,
            totalSets);

        await _adapter.EditMessageAsync(userId, messageId, text, keyboard, cancellationToken);

        state.Data["current_page"] = pageIndex;
        _stateManager.SetState(userId, state);
    }

    private async Task CompleteSet(long userId, int messageId, int dayNumber, int exerciseIndex, int setNumber, string callbackQueryId, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        if (!state.Data.ContainsKey("completed_sets"))
            state.Data["completed_sets"] = new Dictionary<int, List<int>>();

        var completedSetsDict = (Dictionary<int, List<int>>)state.Data["completed_sets"];

        if (!completedSetsDict.ContainsKey(exerciseIndex))
            completedSetsDict[exerciseIndex] = new List<int>();

        if (!completedSetsDict[exerciseIndex].Contains(setNumber))
        {
            completedSetsDict[exerciseIndex].Add(setNumber);
            await SaveCompletedSetToWorkout(userId, dayNumber, exerciseIndex, setNumber, cancellationToken);
            await _adapter.AnswerCallbackAsync(callbackQueryId, $"✅ Подход {setNumber} выполнен!", false, cancellationToken);
        }
        else
        {
            await _adapter.AnswerCallbackAsync(callbackQueryId, $"⚠️ Подход {setNumber} уже отмечен", true, cancellationToken);
            return;
        }

        _stateManager.SetState(userId, state);
        await ShowExercisePage(userId, messageId, dayNumber, exerciseIndex, cancellationToken);
    }

    private async Task FinishWorkout(long userId, int messageId, int dayNumber, CancellationToken cancellationToken)
    {
        if (_stateManager == null)
        {
            await _adapter.EditMessageAsync(
                userId,
                messageId,
                "❌ Ошибка состояния тренировки",
                cancellationToken: cancellationToken);
            return;
        }

        var state = _stateManager.GetState(userId);
        if (state?.Data == null)
        {
            await _adapter.EditMessageAsync(
                userId,
                messageId,
                "❌ Состояние тренировки не найдено. Начните заново.",
                cancellationToken: cancellationToken);
            return;
        }

        var completedSetsDict = state.Data.ContainsKey("completed_sets")
            ? (Dictionary<int, List<int>>)state.Data["completed_sets"]
            : new Dictionary<int, List<int>>();

        var totalSets = completedSetsDict.Values.Sum(list => list.Count);
        var completedExercises = completedSetsDict.Count;

        _stateManager.ClearState(userId);

        var text = Const.Text.WorkoutCompleted
            .Replace("{completedExercises}", completedExercises.ToString())
            .Replace("{totalSets}", totalSets.ToString());

        await _adapter.EditMessageAsync(userId, messageId, text, Const.AfterWorkoutKeyboard(), cancellationToken);
    }

    private async Task SaveCompletedSetToWorkout(long userId, int dayNumber, int exerciseIndex, int setNumber, CancellationToken cancellationToken)
    {
        var workout = await _userWorkoutService.GetUserWorkoutAsync(userId, dayNumber, cancellationToken);
        if (workout == null || exerciseIndex >= workout.Exercises.Count) return;

        var exercise = workout.Exercises.ElementAt(exerciseIndex);
        var today = DateTime.UtcNow.Date;
        var todayWorkout = await _workoutService.GetWorkoutByDateAsync(userId, today, cancellationToken);

        if (todayWorkout == null)
        {
            todayWorkout = await _workoutService.CreateWorkoutAsync(userId, today, new List<Exercise>(), cancellationToken);
        }

        var completedExercise = CloneExerciseWithSet(exercise, setNumber);

        var updatedExercises = todayWorkout.Exercises.ToList();
        updatedExercises.Add(completedExercise);

        await _workoutService.UpdateWorkoutAsync(userId, today, updatedExercises, cancellationToken);
    }

    private Exercise CloneExerciseWithSet(Exercise exercise, int setNumber)
    {
        return exercise switch
        {
            StrengthExercise s => new StrengthExercise(
                name: s.Name,
                met: s.MET,
                sets: 1,
                reps: s.Reps,
                muscleGroup: s.MuscleGroup,
                strengthExerciseType: s.StrengthExerciseType,
                equipment: s.Equipment,
                weightKg: s.Weight?.Kilograms
            ),

            RunningExercise r => new RunningExercise(
                name: r.Name,
                met: r.MET,
                durationMinutes: r.DurationMinutes,
                distanceKm: r.DistanceKm,
                surface: r.Surface,
                intensity: r.Intensity,
                avgHeartRate: r.AvgHeartRate,
                elevationGain: r.ElevationGain
            ),

            CardioExercise c => new CardioExercise(
                name: c.Name,
                met: c.MET,
                durationMinutes: c.DurationMinutes,
                intensity: c.Intensity,
                cardioType: c.CardioType,
                distanceKm: c.DistanceKm,
                avgHeartRate: c.AvgHeartRate
            ),

            StaticExercise st => new StaticExercise(
                name: st.Name,
                met: st.MET,
                holdSeconds: st.HoldSeconds,
                sets: 1,
                staticType: st.StaticType
            ),

            _ => throw new ArgumentException($"Unknown exercise type: {exercise.GetType().Name}")
        };
    }

    private Keyboard BuildExerciseKeyboard(int dayNumber, int exIndex, int totalSets, List<int> completedSets, bool hasPrev, bool hasNext)
    {
        var rows = new List<List<Button>>();

        var currentRow = new List<Button>();
        for (int set = 1; set <= totalSets; set++)
        {
            var isCompleted = completedSets.Contains(set);
            var buttonText = isCompleted ? $"✅ {set}" : $"{set}";

            currentRow.Add(Models.Button.Create(
                buttonText,
                $"{Name}:{Const.Action.CompleteSet}:{dayNumber}:{exIndex}:{set}"
            ));

            if (currentRow.Count == 3 || set == totalSets)
            {
                rows.Add(new List<Button>(currentRow));
                currentRow.Clear();
            }
        }

        var navRow = new List<Button>();
        if (hasPrev)
            navRow.Add(Models.Button.Create(Const.Text.ButtonPrev, $"{Name}:{Const.Action.Prev}:{dayNumber}:{exIndex - 1}"));
        if (hasNext)
            navRow.Add(Models.Button.Create(Const.Text.ButtonNext, $"{Name}:{Const.Action.Next}:{dayNumber}:{exIndex + 1}"));
        if (navRow.Any())
            rows.Add(navRow);

        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonFinish, $"{Name}:{Const.Action.Finish}:{dayNumber}")
        });

        return Keyboard.FromRows(rows.ToArray());
    }

    private string FormatExerciseDisplay(string workoutName, int dayNumber, Exercise exercise, int currentIndex, int total, List<int> completedSets, int totalSets)
    {
        var exerciseInfo = FormatExerciseInfo(exercise);
        var progressBar = BuildProgressBar(completedSets, totalSets);

        return Const.Text.WorkoutDisplay
            .Replace("{workoutName}", workoutName)
            .Replace("{dayNumber}", dayNumber.ToString())
            .Replace("{currentIndex}", currentIndex.ToString())
            .Replace("{total}", total.ToString())
            .Replace("{exerciseInfo}", exerciseInfo)
            .Replace("{progressBar}", progressBar);
    }

    private string FormatExerciseInfo(Exercise exercise)
    {
        return exercise switch
        {
            StrengthExercise s => $"""
                <b>{s.Name}</b>
                🔹 {s.Sets}×{s.Reps} · {s.Weight?.Kilograms ?? 0} кг
                🔸 {GetMuscleGroupName(s.MuscleGroup)}
                """,

            RunningExercise r => $"""
                <b>{r.Name}</b>
                🔹 {r.DurationMinutes} мин · {r.DistanceKm} км
                🔸 темп {r.Pace?.ToString("F1") ?? "?"} мин/км
                """,

            CardioExercise c => $"""
                <b>{c.Name}</b>
                🔹 {c.DurationMinutes} мин
                🔸 {GetIntensityName(c.Intensity)}
                """,

            StaticExercise st => $"""
                <b>{st.Name}</b>
                🔹 {st.Sets}×{st.HoldSeconds} сек
                🔸 {GetStaticTypeName(st.StaticType)}
                """,

            _ => exercise.Name
        };
    }

    private string BuildProgressBar(List<int> completedSets, int totalSets)
    {
        var bars = new List<string>();
        for (int i = 1; i <= totalSets; i++)
        {
            bars.Add(completedSets.Contains(i) ? "✅" : "⬜");
        }
        return $"Прогресс: {string.Join(" ", bars)}";
    }

    private int GetTotalSets(Exercise exercise) => exercise switch
    {
        StrengthExercise s => s.Sets,
        RunningExercise _ => 1,
        CardioExercise _ => 1,
        StaticExercise st => st.Sets,
        _ => 1
    };

    private List<int> GetCompletedSetsFromState(UserState state, int exerciseIndex)
    {
        if (!state.Data.ContainsKey("completed_sets"))
            return new List<int>();

        var completedSetsDict = (Dictionary<int, List<int>>)state.Data["completed_sets"];
        return completedSetsDict.ContainsKey(exerciseIndex) ? completedSetsDict[exerciseIndex] : new List<int>();
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

    private string GetIntensityName(CardioIntensity intensity) => intensity switch
    {
        CardioIntensity.Low => "Низкая",
        CardioIntensity.Moderate => "Средняя",
        CardioIntensity.High => "Высокая",
        _ => intensity.ToString()
    };

    private string GetStaticTypeName(StaticType type) => type switch
    {
        StaticType.Plank => "Планка",
        StaticType.Stretching => "Растяжка",
        StaticType.Yoga => "Йога",
        StaticType.Balance => "Баланс",
        StaticType.WallSit => "Стульчик",
        StaticType.HollowHold => "Лодочка",
        _ => type.ToString()
    };
}