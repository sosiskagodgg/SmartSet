// FitnessTracker.TelegramBot/Commands/WorkoutsCommand.cs
using FitnessTracker.AI.PublicServices;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Enums;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для просмотра и управления тренировками
/// </summary>
public class WorkoutsCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly IUserWorkoutService _userWorkoutService;
    private readonly ILogger<WorkoutsCommand> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IUserStateManager? _stateManager;

    public string Name => "workouts";

    private static class Const
    {
        public const string Title = "💪 <b>Мои тренировки</b>\n\nВыберите тренировку для просмотра:";
        public const string NoWorkouts = """
            📭 <b>У вас пока нет тренировок</b>
            
            Нажмите кнопку "➕ Создать программу", чтобы создать свою первую тренировку.
            """;

        public static class Action
        {
            public const string Main = "main";
            public const string Create = "create";
            public const string Delete = "delete";
            public const string DeleteConfirm = "delete_confirm";
            public const string DeleteCancel = "delete_cancel";
            public const string View = "view";
        }

        public static class Text
        {
            public const string ButtonCreate = "➕ Создать программу";
            public const string ButtonBack = "◀️ Назад";
            public const string ButtonStartWorkout = "🏋️ Начать тренировку";
            public const string ButtonDelete = "🗑️ Удалить";
            public const string ButtonConfirmYes = "✅ Да, удалить";
            public const string ButtonConfirmNo = "❌ Нет, отмена";

            public const string DeleteConfirm = """
                ❓ <b>Удалить тренировку?</b>
                
                Ты уверен, что хочешь удалить тренировку за {dayName}?
                
                Это действие нельзя отменить.
                """;

            public const string DeleteSuccess = "✅ Тренировка за {dayName} успешно удалена.";
        }

        public static class Template
        {
            public const string Workout = """
                💪 <b>{name}</b>
                
                <b>День {dayNumber}</b>
                
                {exercises}
                
                🔹 <i>Нажмите "🏋️ Начать тренировку" для выполнения</i>
                """;
        }
    }

    public WorkoutsCommand(
        ITelegramBotAdapter adapter,
        IUserWorkoutService userWorkoutService,
        ILogger<WorkoutsCommand> logger,
        IServiceProvider serviceProvider)
    {
        Console.WriteLine($"\n💪 WorkoutsCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  userWorkoutService is null: {userWorkoutService == null}");
        Console.WriteLine($"  logger is null: {logger == null}");
        Console.WriteLine($"  serviceProvider is null: {serviceProvider == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _userWorkoutService = userWorkoutService ?? throw new ArgumentNullException(nameof(userWorkoutService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        Console.WriteLine($"✅ WorkoutsCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"💪 WorkoutsCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText)
    {
        var lower = messageText.ToLowerInvariant();
        return lower.Contains("тренировк") || lower.Contains("программ");
    }

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        await ShowWorkoutsList(context.UserId, context.MessageId, cancellationToken);
    }

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        switch (context.Action)
        {
            case Const.Action.Main:
                await ShowWorkoutsList(context.UserId, context.MessageId, cancellationToken);
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            case Const.Action.Create:
                _logger.LogInformation("Create workout requested - redirecting to creation wizard");
                await _adapter.AnswerCallbackAsync(
                    context.CallbackQueryId,
                    text: "Перенаправление в конструктор...",
                    cancellationToken: cancellationToken);

                await ShowWorkoutCreation(context.UserId, context.MessageId, cancellationToken);
                break;

            case Const.Action.View:
                if (context.Parameters.Length > 0 && int.TryParse(context.Parameters[0], out var dayNumber))
                {
                    await ShowWorkoutDetail(context.UserId, context.MessageId, dayNumber, cancellationToken);
                }
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            case Const.Action.Delete:
                if (context.Parameters.Length > 0 && int.TryParse(context.Parameters[0], out var deleteDay))
                {
                    await ShowDeleteConfirm(context.UserId, context.MessageId, deleteDay, cancellationToken);
                }
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            case Const.Action.DeleteConfirm:
                if (context.Parameters.Length > 0 && int.TryParse(context.Parameters[0], out var confirmDay))
                {
                    await DeleteWorkout(context.UserId, context.MessageId, confirmDay, cancellationToken);
                }
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            case Const.Action.DeleteCancel:
                await ShowWorkoutsList(context.UserId, context.MessageId, cancellationToken);
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            default:
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;
        }
    }

    public Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private async Task ShowWorkoutsList(long userId, int messageId, CancellationToken cancellationToken)
    {
        var workouts = await _userWorkoutService.GetAllUserWorkoutsAsync(userId, cancellationToken);
        var existingDays = workouts.Select(w => w.DayNumber).OrderBy(d => d).ToList();

        string text = existingDays.Any() ? Const.Title : Const.NoWorkouts;
        var keyboard = BuildWorkoutsListKeyboard(existingDays);

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task ShowWorkoutDetail(long userId, int messageId, int dayNumber, CancellationToken cancellationToken)
    {
        var workout = await _userWorkoutService.GetUserWorkoutAsync(userId, dayNumber, cancellationToken);
        if (workout == null)
        {
            await ShowWorkoutsList(userId, messageId, cancellationToken);
            return;
        }

        var exercisesText = FormatExercises(workout.Exercises.ToList());
        var text = Const.Template.Workout
            .Replace("{name}", workout.Name)
            .Replace("{dayNumber}", dayNumber.ToString())
            .Replace("{exercises}", exercisesText);

        var keyboard = Keyboard.FromRows(
            new List<Button> {
                Models.Button.Create(Const.Text.ButtonStartWorkout, $"workout_execute:start:{dayNumber}")
            },
            new List<Button> {
                Models.Button.Create(Const.Text.ButtonDelete, $"{Name}:{Const.Action.Delete}:{dayNumber}")
            },
            new List<Button> {
                Models.Button.Create(Const.Text.ButtonBack, $"{Name}:{Const.Action.Main}")
            }
        );

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task ShowDeleteConfirm(long userId, int messageId, int dayNumber, CancellationToken cancellationToken)
    {
        var dayName = GetDayName(dayNumber);
        var text = Const.Text.DeleteConfirm.Replace("{dayName}", dayName);

        var keyboard = Keyboard.FromRows(
            new List<Button> {
                Models.Button.Create(Const.Text.ButtonConfirmYes, $"{Name}:{Const.Action.DeleteConfirm}:{dayNumber}")
            },
            new List<Button> {
                Models.Button.Create(Const.Text.ButtonConfirmNo, $"{Name}:{Const.Action.Main}")
            }
        );

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task DeleteWorkout(long userId, int messageId, int dayNumber, CancellationToken cancellationToken)
    {
        await _userWorkoutService.DeleteUserWorkoutAsync(userId, dayNumber, cancellationToken);

        var dayName = GetDayName(dayNumber);
        var text = Const.Text.DeleteSuccess.Replace("{dayName}", dayName);

        var keyboard = Keyboard.FromSingleButton(Const.Text.ButtonBack, $"{Name}:{Const.Action.Main}");

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task ShowWorkoutCreation(long userId, int messageId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WorkoutCreationCommand>>();
        var userWorkoutService = scope.ServiceProvider.GetRequiredService<IUserWorkoutService>();
        var userParametersService = scope.ServiceProvider.GetRequiredService<IUserParametersService>();
        var workoutGenerationService = scope.ServiceProvider.GetRequiredService<WorkoutGenerationService>();

        var creationCommand = new WorkoutCreationCommand(
            _adapter,
            logger,
            userWorkoutService,
            userParametersService,
            workoutGenerationService,
            _serviceProvider);

        if (_stateManager != null)
        {
            creationCommand.SetStateManager(_stateManager);
        }

        await creationCommand.HandleCallbackAsync(
            new CallbackContext
            {
                UserId = userId,
                MessageId = messageId,
                CallbackQueryId = "",
                Action = "start",
                Parameters = Array.Empty<string>(),
                RawData = "workout_create:start"
            },
            cancellationToken);
    }

    private Keyboard BuildWorkoutsListKeyboard(List<int> days)
    {
        var rows = new List<List<Button>>();

        foreach (var day in days)
        {
            var dayName = GetDayName(day);
            rows.Add(new List<Button>
            {
                Models.Button.Create($"📅 {dayName}", $"{Name}:{Const.Action.View}:{day}")
            });
        }

        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonCreate, $"{Name}:{Const.Action.Create}")
        });
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonBack, "main:back")
        });

        return Keyboard.FromRows(rows.ToArray());
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

    private string FormatExercises(List<Exercise> exercises)
    {
        var result = new List<string>();
        var index = 1;

        foreach (var ex in exercises)
        {
            string formatted = ex switch
            {
                StrengthExercise s => $"{index}. <b>{s.Name}</b> — {s.Sets}×{s.Reps} · {s.Weight?.Kilograms ?? 0} кг",
                RunningExercise r => $"{index}. <b>{r.Name}</b> — {r.DurationMinutes} мин · {r.DistanceKm} км",
                CardioExercise c => $"{index}. <b>{c.Name}</b> — {c.DurationMinutes} мин",
                StaticExercise st => $"{index}. <b>{st.Name}</b> — {st.Sets}×{st.HoldSeconds} сек",
                _ => $"{index}. {ex}"
            };

            result.Add(formatted);
            index++;
        }

        return string.Join("\n", result);
    }
}