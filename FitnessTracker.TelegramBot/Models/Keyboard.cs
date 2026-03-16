// FitnessTracker.TelegramBot/Models/Keyboard.cs
namespace FitnessTracker.TelegramBot.Models;

/// <summary>
/// Модель inline кнопки
/// </summary>
public class Button
{
    /// <summary>
    /// Текст на кнопке
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Данные колбэка (формат: "commandName:action:param1:param2")
    /// </summary>
    public string CallbackData { get; set; } = string.Empty;

    public static Button Create(string text, string callbackData) => new()
    {
        Text = text,
        CallbackData = callbackData
    };
}

/// <summary>
/// Модель inline клавиатуры
/// </summary>
public class Keyboard
{
    /// <summary>
    /// Ряды кнопок
    /// </summary>
    public List<List<Button>> Buttons { get; set; } = new();

    /// <summary>
    /// Создать клавиатуру с одной кнопкой
    /// </summary>
    public static Keyboard FromSingleButton(string text, string callbackData)
    {
        return new Keyboard
        {
            Buttons = new List<List<Button>>
            {
                new() { Button.Create(text, callbackData) }
            }
        };
    }

    /// <summary>
    /// Создать клавиатуру из нескольких кнопок в одном ряду
    /// </summary>
    public static Keyboard FromButtons(params Button[] buttons)
    {
        return new Keyboard
        {
            Buttons = new List<List<Button>> { buttons.ToList() }
        };
    }

    /// <summary>
    /// Создать клавиатуру из нескольких рядов кнопок
    /// </summary>
    public static Keyboard FromRows(params List<Button>[] rows)
    {
        return new Keyboard
        {
            Buttons = rows.ToList()
        };
    }
}