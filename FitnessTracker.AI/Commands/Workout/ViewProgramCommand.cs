using FitnessTracker.AI.Commands.Base;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FitnessTracker.AI.Commands.Workout;

public class ViewProgramCommand : BaseCommand
{
    private readonly IProgramService _programService;
    private readonly IWorkoutService _workoutService;
    private readonly ILogger<ViewProgramCommand> _logger;

    public override string Name => "ViewProgram";
    public override string Description => "Показывает программу тренировок";
    public override string Category => "Workout";
    public override string Group => "Workout";
    public override double ConfidenceThreshold => 0.7;

    public override List<string> TrainingPhrases { get; } = new()
    {
        "покажи программу",
        "моя программа",
        "что сегодня делать",
        "тренировка на сегодня",
        "план на неделю",
        "покажи тренировку на завтра",
        "show my program",
        "what's today's workout"
    };

    public override List<EntityDefinition> RequiredEntities { get; } = new()
    {
        new()
        {
            Type = "day",
            Description = "День недели или номер (сегодня, завтра, понедельник, 1, 2...)",
            IsRequired = false
        }
    };

    public ViewProgramCommand(
        IProgramService programService,
        IWorkoutService workoutService,
        ILogger<ViewProgramCommand> logger) : base(logger)
    {
        _programService = programService;
        _workoutService = workoutService;
        _logger = logger;
    }

    public override async Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        LogExecution(context);

        try
        {
            var userId = context.UserId;
            var dayParam = context.GetEntityValue<string>("day")?.ToLower();

            // Получаем активную программу пользователя
            var program = await _programService.GetActiveProgramAsync(userId);

            if (program == null)
            {
                return Success(
                    "❌ *У вас нет активной программы*\n\n" +
                    "Создайте программу командой:\n" +
                    "• `создай программу для набора массы`\n" +
                    "• `хочу программу на 3 дня`"
                );
            }

            // Если запрошен конкретный день
            if (!string.IsNullOrEmpty(dayParam))
            {
                return await ShowSpecificDay(program, dayParam);
            }

            // Показываем всю неделю
            return await ShowFullWeek(program);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing program for user {UserId}", context.UserId);
            return Error("Произошла ошибка при получении программы");
        }
    }

    private async Task<CommandResult> ShowSpecificDay(ProgramTemplate program, string dayParam)
    {
        var dayNumber = ParseDayParameter(dayParam);

        if (!dayNumber.HasValue)
        {
            return Success(
                "❌ *Не могу определить день*\n\n" +
                "Попробуйте:\n" +
                "• `сегодня`\n" +
                "• `завтра`\n" +
                "• `понедельник`\n" +
                "• `день 3`"
            );
        }

        var day = program.Days.FirstOrDefault(d => d.DayNumber == dayNumber.Value);

        if (day == null)
        {
            return Success($"❌ *День {dayNumber} не найден в программе*");
        }

        if (day.IsRestDay)
        {
            return Success($"🛌 *День {dayNumber}* — *Выходной*\nОтдыхай и восстанавливайся! 💪");
        }

        var response = FormatDay(day, program.Name);
        return Success(response);
    }

    private async Task<CommandResult> ShowFullWeek(ProgramTemplate program)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"🏋️ *{program.Name}*\n");
        sb.AppendLine($"{program.Description}\n");
        sb.AppendLine($"📅 *Программа на {program.Days.Count} дней*\n");

        foreach (var day in program.Days.OrderBy(d => d.DayNumber))
        {
            if (day.IsRestDay)
            {
                sb.AppendLine($"*День {day.DayNumber}* — 🛌 *Выходной*");
            }
            else
            {
                sb.AppendLine($"*День {day.DayNumber}: {day.Name}*");

                foreach (var ex in day.Exercises.OrderBy(e => e.Order))
                {
                    var exerciseName = ex.Exercise?.Name ?? "Упражнение";
                    var sets = ex.TargetSets ?? 3;
                    var reps = ex.TargetRepsMax.HasValue
                        ? $"{ex.TargetRepsMin}-{ex.TargetRepsMax}"
                        : ex.TargetRepsMin.ToString() ?? "8-12";

                    sb.AppendLine($"  {ex.Order}. {exerciseName} — {sets} x {reps}");
                }
            }
            sb.AppendLine("");
        }

        sb.AppendLine("💪 *Железо не прощает слабости!*");

        return Success(sb.ToString());
    }

    private string FormatDay(ProgramDay day, string programName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"🏋️ *{programName}*");
        sb.AppendLine($"📌 *День {day.DayNumber}: {day.Name}*\n");

        foreach (var ex in day.Exercises.OrderBy(e => e.Order))
        {
            var exerciseName = ex.Exercise?.Name ?? "Упражнение";
            var sets = ex.TargetSets ?? 3;
            var reps = ex.TargetRepsMax.HasValue
                ? $"{ex.TargetRepsMin}-{ex.TargetRepsMax}"
                : ex.TargetRepsMin.ToString() ?? "8-12";

            sb.AppendLine($"{ex.Order}. *{exerciseName}* — {sets} x {reps}");
        }

        return sb.ToString();
    }

    private int? ParseDayParameter(string param)
    {
        if (string.IsNullOrEmpty(param)) return null;

        param = param.ToLower().Trim();

        // Сегодня/завтра
        if (param == "сегодня" || param == "today")
        {
            // Получаем текущий день недели (1-7, где 1 = понедельник)
            var today = DateTime.Today.DayOfWeek;
            return today == DayOfWeek.Sunday ? 7 : (int)today;
        }

        if (param == "завтра" || param == "tomorrow")
        {
            // Получаем завтрашний день недели
            var tomorrow = DateTime.Today.AddDays(1).DayOfWeek;
            return tomorrow == DayOfWeek.Sunday ? 7 : (int)tomorrow;
        }

        // Дни недели на русском
        var russianDays = new Dictionary<string, int>
        {
            ["пн"] = 1,
            ["понедельник"] = 1,
            ["monday"] = 1,
            ["вт"] = 2,
            ["вторник"] = 2,
            ["tuesday"] = 2,
            ["ср"] = 3,
            ["среда"] = 3,
            ["wednesday"] = 3,
            ["чт"] = 4,
            ["четверг"] = 4,
            ["thursday"] = 4,
            ["пт"] = 5,
            ["пятница"] = 5,
            ["friday"] = 5,
            ["сб"] = 6,
            ["суббота"] = 6,
            ["saturday"] = 6,
            ["вс"] = 7,
            ["воскресенье"] = 7,
            ["sunday"] = 7
        };

        foreach (var kv in russianDays)
        {
            if (param.Contains(kv.Key))
                return kv.Value;
        }

        // Числа
        if (int.TryParse(param, out int dayNumber) && dayNumber >= 1 && dayNumber <= 7)
            return dayNumber;

        return null;
    }

    private string EscapeMarkdownV2(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var specialChars = new HashSet<char>
        {
            '_', '*', '[', ']', '(', ')', '~', '`', '>', '#',
            '+', '-', '=', '|', '{', '}', '.', '!'
        };

        var sb = new StringBuilder();
        foreach (char c in text)
        {
            if (specialChars.Contains(c))
                sb.Append('\\');
            sb.Append(c);
        }
        return text;
    }
}