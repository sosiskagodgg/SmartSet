using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Commands.Base;

namespace FitnessTracker.AI.Commands.Base;

/// <summary>
/// Встроенная команда помощи, которая работает всегда
/// </summary>
public class HelpCommand : BaseCommand
{
    private readonly ICommandRegistry _commandRegistry;

    public override string Name => "Help";
    public override string Description => "Показывает список доступных команд";
    public override string Category => "System";
    public override string Group => "System";
    public override double ConfidenceThreshold => 0.9;

    public override List<string> TrainingPhrases { get; } = new()
    {
        "помощь",
        "help",
        "что ты умеешь",
        "команды",
        "справка",
        "что можно сделать"
    };

    public HelpCommand(
        ICommandRegistry commandRegistry,
        ILogger<HelpCommand> logger) : base(logger)
    {
        _commandRegistry = commandRegistry;
    }

    public override Task<bool> CanHandleAsync(string message, CancellationToken cancellationToken = default)
    {
        // Help команда всегда может сработать, но с низкой уверенностью
        // Это fallback для неизвестных сообщений
        return base.CanHandleAsync(message, cancellationToken);
    }

    public override async Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        LogExecution(context);

        var commands = _commandRegistry.GetAllCommands().ToList();

        if (!commands.Any())
        {
            return Success(EscapeMarkdown(
                "👋 Привет! Я фитнес-ассистент.\n\n" +
                "📝 Система пока не настроена. Обратись к администратору для добавления команд.\n\n" +
                "Доступные встроенные команды:\n" +
                "• помощь / help → показать это сообщение\n" +  // Заменили "-" на "→"
                "• отмена / cancel → отменить текущее действие"   // Заменили "-" на "→"
            ));
        }

        var groupedCommands = commands
            .Where(c => c.Name != "Help")
            .GroupBy(c => c.Category)
            .OrderBy(g => g.Key);

        var response = new List<string> { "🤖 Доступные команды:\n" };

        foreach (var group in groupedCommands)
        {
            response.Add($"📌 {EscapeMarkdown(group.Key)}:");
            foreach (var command in group.OrderBy(c => c.Name))
            {
                response.Add($"  • {EscapeMarkdown(command.Name)} → {EscapeMarkdown(command.Description)}"); // Заменили "-" на "→"
            }
            response.Add("");
        }

        response.Add("💡 Просто напиши что хочешь сделать естественным языком!");

        return Success(string.Join("\n", response));
    }
    private string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return string.Create(text.Length * 2, text, (span, input) =>
        {
            int writeIndex = 0;
            for (int readIndex = 0; readIndex < input.Length; readIndex++)
            {
                char c = input[readIndex];
                if (MarkdownV2SpecialChars.Contains(c))
                {
                    span[writeIndex++] = '\\';
                }
                span[writeIndex++] = c;
            }
        });
    }
    private static readonly HashSet<char> MarkdownV2SpecialChars = new HashSet<char>
{
    '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!', '\\', '<', '&'
};
}