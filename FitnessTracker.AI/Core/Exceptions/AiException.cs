// FitnessTracker.AI/Core/Exceptions/AiException.cs
namespace FitnessTracker.AI.Core.Exceptions;

/// <summary>
/// Базовое исключение для AI слоя
/// </summary>
public abstract class AiException : Exception
{
    public string ErrorCode { get; }

    protected AiException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Ошибка при обращении к AI провайдеру
/// </summary>
public sealed class AiProviderException : AiException
{
    public string Provider { get; }

    public AiProviderException(string provider, string message, string errorCode = "AI_PROVIDER_ERROR")
        : base($"AI provider '{provider}' error: {message}", errorCode)
    {
        Provider = provider;
    }
}

/// <summary>
/// Плагин не найден
/// </summary>
public sealed class PluginNotFoundException : AiException
{
    public string PluginId { get; }

    public PluginNotFoundException(string pluginId)
        : base($"Plugin with id '{pluginId}' not found", "PLUGIN_NOT_FOUND")
    {
        PluginId = pluginId;
    }
}