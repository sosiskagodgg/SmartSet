namespace FitnessTracker.TelegramBot.Models;

public class Button
{
    public string Text { get; set; } = string.Empty;
    public string CallbackData { get; set; } = string.Empty;

    public static Button Create(string text, string callbackData) =>
        new() { Text = text, CallbackData = callbackData };
}

public class Keyboard
{
    public List<List<Button>> Buttons { get; set; } = new();

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

    public static Keyboard FromButtons(params Button[] buttons)
    {
        return new Keyboard
        {
            Buttons = new List<List<Button>> { buttons.ToList() }
        };
    }

    public static Keyboard FromRows(params List<Button>[] rows)
    {
        return new Keyboard
        {
            Buttons = rows.ToList()
        };
    }
}