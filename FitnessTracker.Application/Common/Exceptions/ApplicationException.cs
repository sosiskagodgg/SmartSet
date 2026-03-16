// FitnessTracker.Application/Common/Exceptions/ApplicationException.cs
namespace FitnessTracker.Application.Common.Exceptions;

/// <summary>
/// Базовое исключение для Application слоя
/// </summary>
public abstract class ApplicationException : Exception
{
    public string ErrorCode { get; }

    protected ApplicationException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}