namespace FitnessTracker.AI.Core.Models;

public class CommandResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public CommandResultType ResultType { get; set; } = CommandResultType.Success;

    public static CommandResult Success(string message)
    {
        return new CommandResult
        {
            IsSuccess = true,
            Message = message,
            ResultType = CommandResultType.Success
        };
    }

    public static CommandResult Error(string error)
    {
        return new CommandResult
        {
            IsSuccess = false,
            ErrorMessage = error,
            ResultType = CommandResultType.Error
        };
    }

    public static CommandResult NeedMoreInfo(string question, List<EntityDefinition> requiredEntities)
    {
        return new CommandResult
        {
            IsSuccess = false,
            Message = question,
            ResultType = CommandResultType.NeedMoreInfo,
            Data = new Dictionary<string, object> { ["required"] = requiredEntities }
        };
    }

    public static CommandResult Confirmation(string question, Dictionary<string, object>? context = null)
    {
        return new CommandResult
        {
            IsSuccess = false,
            Message = question,
            ResultType = CommandResultType.Confirmation,
            Data = context ?? new Dictionary<string, object>()
        };
    }
}

public enum CommandResultType
{
    Success,
    Error,
    NeedMoreInfo,
    Confirmation,
    Cancelled
}