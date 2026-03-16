// FitnessTracker.TelegramBot/Commands/WorkoutCreationCommand.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.AI.PublicServices;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для создания тренировки через AI (многошаговый процесс)
/// </summary>
public class WorkoutCreationCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly ILogger<WorkoutCreationCommand> _logger;
    private readonly IUserWorkoutService _userWorkoutService;
    private readonly IUserParametersService _userParametersService;
    private readonly WorkoutGenerationService _workoutGenerationService;
    private readonly IServiceProvider _serviceProvider;
    private IUserStateManager? _stateManager;

    public string Name => "workout_create";

    private enum CreationStep
    {
        DaysPerWeek = 1,
        Goal,
        Experience,
        Location,
        Duration,
        Gender,
        Generating,
        Complete
    }

    private static class Const
    {
        public static class Action
        {
            public const string Start = "start";
            public const string SelectDays = "select_days";
            public const string SelectGoal = "select_goal";
            public const string SelectExperience = "select_exp";
            public const string SelectLocation = "select_loc";
            public const string SelectDuration = "select_dur";
            public const string SelectGender = "select_gender";
            public const string Generate = "generate";
            public const string Back = "back";
            public const string Cancel = "cancel";
        }

        public static class State
        {
            public const string Creation = "creation";
        }

        public static class Text
        {
            public const string DaysPerWeek = """
                📅 <b>Сколько дней в неделю ты готов тренироваться?</b>
                
                Выбери оптимальное количество:
                """;

            public const string Goal = """
                🎯 <b>Какая у тебя основная цель?</b>
                
                От этого зависит структура программы:
                """;

            public const string Experience = """
                📊 <b>Твой уровень подготовки</b>
                
                Честный ответ поможет составить безопасную программу:
                """;

            public const string Location = """
                📍 <b>Где ты планируешь тренироваться?</b>
                """;

            public const string Duration = """
                ⏱️ <b>Сколько времени ты готов уделять одной тренировке?</b>
                """;

            public const string Gender = """
                👤 <b>Твой пол</b>
                
                Это нужно для более точных расчетов:
                """;

            public const string Generating = """
                🤖 <b>AI создает твою программу...</b>
                
                Пожалуйста, подожди немного. Это займет несколько секунд.
                """;

            public const string Success = """
                ✅ <b>Программа тренировок готова!</b>
                
                Твоя программа на {daysPerWeek} дней создана и сохранена.
                
                Посмотреть можно в разделе "Мои тренировки".
                """;

            public const string Error = """
                ❌ <b>Ошибка при создании программы</b>
                
                Не удалось сгенерировать программу. Попробуй позже.
                """;

            public const string ButtonGenerate = "🤖 Сгенерировать";
            public const string ButtonBack = "◀️ Назад";
            public const string ButtonCancel = "❌ Отмена";
            public const string ButtonViewWorkouts = "📋 К тренировкам";
        }

        public static class Options
        {
            public static readonly Dictionary<int, string> DaysPerWeek = new()
            {
                [2] = "2 дня",
                [3] = "3 дня",
                [4] = "4 дня",
                [5] = "5 дней",
                [6] = "6 дней"
            };

            public static readonly Dictionary<string, string> Goals = new()
            {
                ["strength"] = "💪 Сила",
                ["mass"] = "🏋️ Масса",
                ["weight_loss"] = "⚖️ Похудение",
                ["endurance"] = "❤️ Выносливость",
                ["tone"] = "✨ Тонус",
                ["health"] = "🌿 Здоровье"
            };

            public static readonly Dictionary<string, string> Experience = new()
            {
                ["beginner"] = "🌱 Начинающий",
                ["intermediate"] = "🌿 Средний",
                ["advanced"] = "🌳 Продвинутый"
            };

            public static readonly Dictionary<string, string> Locations = new()
            {
                ["gym"] = "🏢 Фитнес-клуб",
                ["home"] = "🏠 Дома",
                ["street"] = "🌳 Улица",
                ["mixed"] = "🔄 Разное"
            };

            public static readonly Dictionary<int, string> Duration = new()
            {
                [30] = "⏱️ 30 мин",
                [45] = "⌛ 45 мин",
                [60] = "⏰ 60 мин",
                [90] = "⌛ 90 мин"
            };

            public static readonly Dictionary<string, string> Gender = new()
            {
                ["male"] = "👨 Мужской",
                ["female"] = "👩 Женский",
                ["other"] = "🧑 Другой"
            };
        }
    }

    public WorkoutCreationCommand(
        ITelegramBotAdapter adapter,
        ILogger<WorkoutCreationCommand> logger,
        IUserWorkoutService userWorkoutService,
        IUserParametersService userParametersService,
        WorkoutGenerationService workoutGenerationService,
        IServiceProvider serviceProvider)
    {
        Console.WriteLine($"\n🏋️ WorkoutCreationCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  logger is null: {logger == null}");
        Console.WriteLine($"  userWorkoutService is null: {userWorkoutService == null}");
        Console.WriteLine($"  userParametersService is null: {userParametersService == null}");
        Console.WriteLine($"  workoutGenerationService is null: {workoutGenerationService == null}");
        Console.WriteLine($"  serviceProvider is null: {serviceProvider == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userWorkoutService = userWorkoutService ?? throw new ArgumentNullException(nameof(userWorkoutService));
        _userParametersService = userParametersService ?? throw new ArgumentNullException(nameof(userParametersService));
        _workoutGenerationService = workoutGenerationService ?? throw new ArgumentNullException(nameof(workoutGenerationService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        Console.WriteLine($"✅ WorkoutCreationCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"🏋️ WorkoutCreationCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText)
    {
        return false;
    }

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WorkoutCreationCommand received: {Action}", context.Action);

        try
        {
            switch (context.Action)
            {
                case Const.Action.Start:
                    await StartCreation(context.UserId, context.MessageId, cancellationToken);
                    break;

                case Const.Action.SelectDays:
                    if (context.Parameters.Length > 0 && int.TryParse(context.Parameters[0], out var days))
                    {
                        await HandleDaysSelection(context.UserId, context.MessageId, days, cancellationToken);
                    }
                    break;

                case Const.Action.SelectGoal:
                    if (context.Parameters.Length > 0)
                    {
                        await HandleGoalSelection(context.UserId, context.MessageId, context.Parameters[0], cancellationToken);
                    }
                    break;

                case Const.Action.SelectExperience:
                    if (context.Parameters.Length > 0)
                    {
                        await HandleExperienceSelection(context.UserId, context.MessageId, context.Parameters[0], cancellationToken);
                    }
                    break;

                case Const.Action.SelectLocation:
                    if (context.Parameters.Length > 0)
                    {
                        await HandleLocationSelection(context.UserId, context.MessageId, context.Parameters[0], cancellationToken);
                    }
                    break;

                case Const.Action.SelectDuration:
                    if (context.Parameters.Length > 0 && int.TryParse(context.Parameters[0], out var duration))
                    {
                        await HandleDurationSelection(context.UserId, context.MessageId, duration, cancellationToken);
                    }
                    break;

                case Const.Action.SelectGender:
                    if (context.Parameters.Length > 0)
                    {
                        await HandleGenderSelection(context.UserId, context.MessageId, context.Parameters[0], cancellationToken);
                    }
                    break;

                case Const.Action.Generate:
                    await GenerateWorkout(context.UserId, context.MessageId, cancellationToken);
                    break;

                case Const.Action.Back:
                    await HandleBack(context.UserId, context.MessageId, cancellationToken);
                    break;

                case Const.Action.Cancel:
                    await CancelCreation(context.UserId, context.MessageId, cancellationToken);
                    break;
            }

            if (!string.IsNullOrEmpty(context.CallbackQueryId))
            {
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WorkoutCreationCommand for user {UserId}", context.UserId);
        }
    }

    public async Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        await _adapter.SendMessageAsync(
            userId: context.UserId,
            text: "Пожалуйста, используйте кнопки для ответов.",
            cancellationToken: cancellationToken);
    }

    private async Task StartCreation(long userId, int messageId, CancellationToken cancellationToken)
    {
        if (_stateManager == null)
        {
            _logger.LogError("StateManager not initialized for WorkoutCreationCommand");
            return;
        }

        var stateData = new Dictionary<string, object>
        {
            ["step"] = (int)CreationStep.DaysPerWeek,
            ["data"] = new Dictionary<string, object>()
        };

        _stateManager.SetState(userId, new UserState
        {
            CommandName = Name,
            Step = Const.State.Creation,
            Data = stateData
        });

        await ShowDaysPerWeek(userId, messageId, cancellationToken);
    }

    private async Task ShowDaysPerWeek(long userId, int messageId, CancellationToken cancellationToken)
    {
        var rows = BuildOptionKeyboard(Const.Options.DaysPerWeek, Const.Action.SelectDays);
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonCancel, $"{Name}:{Const.Action.Cancel}")
        });

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.DaysPerWeek,
            keyboard: Keyboard.FromRows(rows.ToArray()),
            cancellationToken: cancellationToken);
    }

    private async Task HandleDaysSelection(long userId, int messageId, int days, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var data = (Dictionary<string, object>)state.Data["data"];
        data["days_per_week"] = days;
        state.Data["step"] = (int)CreationStep.Goal;
        _stateManager.SetState(userId, state);

        await ShowGoal(userId, messageId, cancellationToken);
    }

    private async Task ShowGoal(long userId, int messageId, CancellationToken cancellationToken)
    {
        var rows = BuildOptionKeyboard(Const.Options.Goals, Const.Action.SelectGoal);
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonBack, $"{Name}:{Const.Action.Back}")
        });
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonCancel, $"{Name}:{Const.Action.Cancel}")
        });

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.Goal,
            keyboard: Keyboard.FromRows(rows.ToArray()),
            cancellationToken: cancellationToken);
    }

    private async Task HandleGoalSelection(long userId, int messageId, string goal, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var data = (Dictionary<string, object>)state.Data["data"];
        data["goal"] = goal;
        state.Data["step"] = (int)CreationStep.Experience;
        _stateManager.SetState(userId, state);

        await ShowExperience(userId, messageId, cancellationToken);
    }

    private async Task ShowExperience(long userId, int messageId, CancellationToken cancellationToken)
    {
        var rows = BuildOptionKeyboard(Const.Options.Experience, Const.Action.SelectExperience);
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonBack, $"{Name}:{Const.Action.Back}")
        });
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonCancel, $"{Name}:{Const.Action.Cancel}")
        });

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.Experience,
            keyboard: Keyboard.FromRows(rows.ToArray()),
            cancellationToken: cancellationToken);
    }

    private async Task HandleExperienceSelection(long userId, int messageId, string experience, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var data = (Dictionary<string, object>)state.Data["data"];
        data["experience"] = experience;
        state.Data["step"] = (int)CreationStep.Location;
        _stateManager.SetState(userId, state);

        await ShowLocation(userId, messageId, cancellationToken);
    }

    private async Task ShowLocation(long userId, int messageId, CancellationToken cancellationToken)
    {
        var rows = BuildOptionKeyboard(Const.Options.Locations, Const.Action.SelectLocation);
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonBack, $"{Name}:{Const.Action.Back}")
        });
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonCancel, $"{Name}:{Const.Action.Cancel}")
        });

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.Location,
            keyboard: Keyboard.FromRows(rows.ToArray()),
            cancellationToken: cancellationToken);
    }

    private async Task HandleLocationSelection(long userId, int messageId, string location, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var data = (Dictionary<string, object>)state.Data["data"];
        data["location"] = location;
        state.Data["step"] = (int)CreationStep.Duration;
        _stateManager.SetState(userId, state);

        await ShowDuration(userId, messageId, cancellationToken);
    }

    private async Task ShowDuration(long userId, int messageId, CancellationToken cancellationToken)
    {
        var rows = BuildOptionKeyboard(Const.Options.Duration, Const.Action.SelectDuration);
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonBack, $"{Name}:{Const.Action.Back}")
        });
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonCancel, $"{Name}:{Const.Action.Cancel}")
        });

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.Duration,
            keyboard: Keyboard.FromRows(rows.ToArray()),
            cancellationToken: cancellationToken);
    }

    private async Task HandleDurationSelection(long userId, int messageId, int duration, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var data = (Dictionary<string, object>)state.Data["data"];
        data["duration"] = duration;
        state.Data["step"] = (int)CreationStep.Gender;
        _stateManager.SetState(userId, state);

        await ShowGender(userId, messageId, cancellationToken);
    }

    private async Task ShowGender(long userId, int messageId, CancellationToken cancellationToken)
    {
        var rows = BuildOptionKeyboard(Const.Options.Gender, Const.Action.SelectGender);
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonBack, $"{Name}:{Const.Action.Back}")
        });
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonGenerate, $"{Name}:{Const.Action.Generate}")
        });
        rows.Add(new List<Button> {
            Models.Button.Create(Const.Text.ButtonCancel, $"{Name}:{Const.Action.Cancel}")
        });

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.Gender,
            keyboard: Keyboard.FromRows(rows.ToArray()),
            cancellationToken: cancellationToken);
    }

    private async Task HandleGenderSelection(long userId, int messageId, string gender, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var data = (Dictionary<string, object>)state.Data["data"];
        data["gender"] = gender;
        _stateManager.SetState(userId, state);

        await GenerateWorkout(userId, messageId, cancellationToken);
    }

    private async Task HandleBack(long userId, int messageId, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var step = (CreationStep)state.Data["step"];

        switch (step)
        {
            case CreationStep.Goal:
                state.Data["step"] = (int)CreationStep.DaysPerWeek;
                _stateManager.SetState(userId, state);
                await ShowDaysPerWeek(userId, messageId, cancellationToken);
                break;

            case CreationStep.Experience:
                state.Data["step"] = (int)CreationStep.Goal;
                _stateManager.SetState(userId, state);
                await ShowGoal(userId, messageId, cancellationToken);
                break;

            case CreationStep.Location:
                state.Data["step"] = (int)CreationStep.Experience;
                _stateManager.SetState(userId, state);
                await ShowExperience(userId, messageId, cancellationToken);
                break;

            case CreationStep.Duration:
                state.Data["step"] = (int)CreationStep.Location;
                _stateManager.SetState(userId, state);
                await ShowLocation(userId, messageId, cancellationToken);
                break;

            case CreationStep.Gender:
                state.Data["step"] = (int)CreationStep.Duration;
                _stateManager.SetState(userId, state);
                await ShowDuration(userId, messageId, cancellationToken);
                break;
        }
    }

    private async Task GenerateWorkout(long userId, int messageId, CancellationToken cancellationToken)
    {
        if (_stateManager == null) return;

        var state = _stateManager.GetState(userId);
        if (state?.Data == null) return;

        var data = (Dictionary<string, object>)state.Data["data"];

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.Generating,
            keyboard: null,
            cancellationToken: cancellationToken);

        try
        {
            var success = await _workoutGenerationService.CreateWorkoutProgramAsync(
                userId,
                (int)data["days_per_week"],
                data["goal"].ToString()!,
                data["experience"].ToString()!,
                data["location"].ToString()!,
                (int)data["duration"],
                data["gender"].ToString()!,
                cancellationToken);

            if (success)
            {
                var daysPerWeek = (int)data["days_per_week"];
                var text = Const.Text.Success.Replace("{daysPerWeek}", daysPerWeek.ToString());

                var keyboard = Keyboard.FromRows(
                    new List<Button> {
                        Models.Button.Create(Const.Text.ButtonViewWorkouts, "workouts:main")
                    },
                    new List<Button> {
                        Models.Button.Create(Const.Text.ButtonCancel, $"{Name}:{Const.Action.Cancel}")
                    }
                );

                await _adapter.EditMessageAsync(
                    userId: userId,
                    messageId: messageId,
                    text: text,
                    keyboard: keyboard,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _adapter.EditMessageAsync(
                    userId: userId,
                    messageId: messageId,
                    text: Const.Text.Error,
                    keyboard: null,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating workout for user {UserId}", userId);
            await _adapter.EditMessageAsync(
                userId: userId,
                messageId: messageId,
                text: Const.Text.Error,
                keyboard: null,
                cancellationToken: cancellationToken);
        }

        _stateManager.ClearState(userId);
    }

    private async Task CancelCreation(long userId, int messageId, CancellationToken cancellationToken)
    {
        _stateManager?.ClearState(userId);

        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MainCommand>>();

        var mainCommand = new MainCommand(_adapter, logger);
        mainCommand.SetStateManager(_stateManager!);

        await mainCommand.HandleCallbackAsync(
            new CallbackContext
            {
                UserId = userId,
                MessageId = messageId,
                CallbackQueryId = "",
                Action = "open",
                Parameters = Array.Empty<string>(),
                RawData = "main:open"
            },
            cancellationToken);
    }

    private List<List<Button>> BuildOptionKeyboard<T>(Dictionary<T, string> options, string action)
    {
        var rows = new List<List<Button>>();
        var currentRow = new List<Button>();

        foreach (var option in options)
        {
            currentRow.Add(Models.Button.Create(option.Value, $"{Name}:{action}:{option.Key}"));

            if (currentRow.Count == 2)
            {
                rows.Add(new List<Button>(currentRow));
                currentRow.Clear();
            }
        }

        if (currentRow.Any())
        {
            rows.Add(currentRow);
        }

        return rows;
    }
}