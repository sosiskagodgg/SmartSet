// FitnessTracker.ConsoleTest/TestHelpers/ConsoleHelper.cs
namespace FitnessTracker.ConsoleTest.TestHelpers;

public static class ConsoleHelper
{
    public static void WriteHeader(string text)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"╔═══ {text} ═══");
        Console.ResetColor();
    }

    public static void WriteSuccess(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {text}");
        Console.ResetColor();
    }

    public static void WriteError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {text}");
        Console.ResetColor();
    }

    public static void WriteInfo(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ️ {text}");
        Console.ResetColor();
    }

    public static void WriteWarning(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️ {text}");
        Console.ResetColor();
    }

    public static void WriteJson(string json)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(json);
        Console.ResetColor();
    }

    public static string ReadInput(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"\n{prompt}: ");
        Console.ResetColor();
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static void WaitForKey()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    public static void ClearScreen()
    {
        Console.Clear();
    }
}