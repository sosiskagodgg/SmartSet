using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Commands.Base;

namespace FitnessTracker.AI.Commands.Base;

/// <summary>
/// Встроенная команда отмены
/// </summary>
public class CancelCommand : BaseCommand
{
    public override string Name => "Cancel";
    public override string Description => "Отменяет текущее действие";
    public override string Category => "System";
    public override string Group => "System";
    public override double ConfidenceThreshold => 0.9;

    public override List<string> TrainingPhrases { get; } = new()
    {
        "отмена",
        "назад",
        "cancel",
        "не надо",
        "отменить",
        "выйти"
    };

    public CancelCommand(ILogger<CancelCommand> logger) : base(logger)
    {
    }

    public override Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        LogExecution(context);

        return Task.FromResult(Success(
            "❌ Действие отменено.\n" +
            "Чем еще могу помочь?"
        ));
    }
}