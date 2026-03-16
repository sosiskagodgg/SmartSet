// FitnessTracker.Domain/Common/DomainException.cs
namespace FitnessTracker.Domain.Common;

/// <summary>
/// Базовое исключение для всех доменных ошибок.
/// Позволяет отличать бизнес-ошибки от технических.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}