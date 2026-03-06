// FitnessTracker.AI/Commands/User/UserInfoCommand.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Commands.Base;
using FitnessTracker.Application.Interfaces;
using System.Text;

namespace FitnessTracker.AI.Commands.Users;

public class UserInfoCommand : BaseCommand
{
    private readonly IUserService _userService;
    private readonly IUserParameterService _parameterService;
    private readonly ILogger<UserInfoCommand> _logger;

    public override string Name => "UserInfo";
    public override string Description => "Информация о профиле";
    public override string Category => "Профиль";
    public override string Group => "Profile";
    public override double ConfidenceThreshold => 0.7;

    // Минимум фраз - только основные варианты
    public override List<string> TrainingPhrases { get; } = new()
    {
        "мой профиль",
        "мои данные",
        "информация обо мне",
        "показать профиль"
    };

    public UserInfoCommand(
        IUserService userService,
        IUserParameterService parameterService,
        ILogger<UserInfoCommand> logger) : base(logger)
    {
        _userService = userService;
        _parameterService = parameterService;
        _logger = logger;
    }

    public override async Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        LogExecution(context);

        try
        {
            var userId = context.UserId;

            // Параллельно получаем данные
            var userTask = _userService.GetByIdAsync(userId);
            var parametersTask = _parameterService.GetCurrentAsync(userId);

            await Task.WhenAll(userTask, parametersTask);

            var user = await userTask;
            var parameters = await parametersTask;

            if (user == null)
                return Error("❌ Профиль не найден");

            var response = BuildProfileMessage(user, parameters);
            return Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return Error("❌ Ошибка при получении данных");
        }
    }

    private string BuildProfileMessage(Domain.Entities.User user, Domain.Entities.UserParameter? parameters)
    {
        var sb = new StringBuilder();

        // Функция экранирования для MarkdownV2
        string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // Экранируем все спецсимволы: _ * [ ] ( ) ~ ` > # + - = | { } . !
            return System.Text.RegularExpressions.Regex.Replace(text, @"([_*\[\]()~`>#+\-=|{}.!])", "\\$1");
        }



        // Основная информация
        sb.AppendLine("*📋 Личные данные:*");

        var fullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim();
        var displayName = string.IsNullOrEmpty(fullName) ? "❌ не указано" : EscapeMarkdown(fullName);
        sb.AppendLine($"└ Имя: {displayName}");

        var username = string.IsNullOrEmpty(user.Username) ? "❌ не указан" : $"@{EscapeMarkdown(user.Username)}";
        sb.AppendLine($"└ Username: {username}");

        sb.AppendLine($"└ ID: `{user.Id}`");
        sb.AppendLine($"└ На сайте с: {EscapeMarkdown(user.RegisteredAt.ToString("dd\\.MM\\.yyyy"))}");

        // Физические параметры
        sb.AppendLine("\n*📊 Физические параметры:*");

        if (parameters != null)
        {
            // Вес
            var weightText = parameters.WeightKg.HasValue
                ? $"{parameters.WeightKg.Value} кг"
                : "❌ не указан";
            sb.AppendLine($"└ Вес: {EscapeMarkdown(weightText)}");

            // Рост
            var heightText = parameters.HeightCm.HasValue
                ? $"{parameters.HeightCm.Value} см"
                : "❌ не указан";
            sb.AppendLine($"└ Рост: {EscapeMarkdown(heightText)}");

            // Дата рождения
            if (parameters.BirthDate.HasValue)
            {
                var age = DateTime.Today.Year - parameters.BirthDate.Value.Year;
                if (parameters.BirthDate.Value.Date > DateTime.Today.AddYears(-age)) age--;
                var birthDateStr = parameters.BirthDate.Value.ToString("dd\\.MM\\.yyyy");
                sb.AppendLine($"└ Дата рождения: {EscapeMarkdown(birthDateStr)} \\({age} лет\\)");
            }
            else
            {
                sb.AppendLine($"└ Дата рождения: ❌ не указана");
            }

            // Пол
            var genderText = string.IsNullOrEmpty(parameters.Gender) ? "❌ не указан" :
                parameters.Gender == "male" ? "Мужской" :
                parameters.Gender == "female" ? "Женский" : "Другой";
            sb.AppendLine($"└ Пол: {EscapeMarkdown(genderText)}");

            // Активность
            var activityText = string.IsNullOrEmpty(parameters.ActivityLevel) ? "❌ не указан" :
                parameters.ActivityLevel switch
                {
                    "sedentary" => "🏠 Сидячий",
                    "light" => "🚶 Легкий",
                    "moderate" => "🏃 Умеренный",
                    "very" => "🏋️ Высокий",
                    "extra" => "🔥 Экстра",
                    _ => parameters.ActivityLevel
                };
            sb.AppendLine($"└ Уровень активности: {EscapeMarkdown(activityText)}");

            // Опыт
            var expText = string.IsNullOrEmpty(parameters.ExperienceLevel) ? "❌ не указан" :
                parameters.ExperienceLevel switch
                {
                    "beginner" => "🌱 Новичок",
                    "intermediate" => "🌿 Средний",
                    "advanced" => "🌳 Продвинутый",
                    _ => parameters.ExperienceLevel
                };
            sb.AppendLine($"└ Опыт тренировок: {EscapeMarkdown(expText)}");
        }
        else
        {
            sb.AppendLine($"└ Вес: ❌ не указан");
            sb.AppendLine($"└ Рост: ❌ не указан");
            sb.AppendLine($"└ Дата рождения: ❌ не указана");
            sb.AppendLine($"└ Пол: ❌ не указан");
            sb.AppendLine($"└ Уровень активности: ❌ не указан");
            sb.AppendLine($"└ Опыт тренировок: ❌ не указан");
        }

        // Цели
        sb.AppendLine("\n*🎯 Цели тренировок:*");

        if (parameters?.FitnessGoals != null && parameters.FitnessGoals.Any())
        {
            foreach (var goal in parameters.FitnessGoals)
            {
                var goalName = goal switch
                {
                    "lose_weight" => "🏃 Похудение",
                    "build_muscle" => "💪 Набор массы",
                    "endurance" => "🏃‍♂️ Выносливость",
                    "strength" => "🏋️ Сила",
                    _ => goal
                };
                sb.AppendLine($"└ {EscapeMarkdown(goalName)}");
            }
        }
        else
        {
            sb.AppendLine($"└ ❌ цели не указаны");
        }


        return sb.ToString();
    }
}