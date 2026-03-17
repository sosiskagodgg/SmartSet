// FitnessTracker.ConsoleTest/TestScenarios/UserParametersTestScenario.cs
using FitnessTracker.Application.Interfaces;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.ConsoleTest.TestHelpers;
namespace FitnessTracker.ConsoleTest.TestScenarios;

public class UserParametersTestScenario
{
    private readonly IUserParametersService _parametersService;
    private readonly UserParametersAIService _parametersAI;

    public UserParametersTestScenario(
        IUserParametersService parametersService,
        UserParametersAIService parametersAI)
    {
        _parametersService = parametersService;
        _parametersAI = parametersAI;
    }

    public async Task RunAsync(long userId)
    {
        ConsoleHelper.WriteHeader("ТЕСТИРОВАНИЕ ПАРАМЕТРОВ ПОЛЬЗОВАТЕЛЯ");

        while (true)
        {
            await ShowCurrentParameters(userId);

            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1. ✏️ Обновить через естественный язык");
            Console.WriteLine("2. 📏 Обновить рост");
            Console.WriteLine("3. ⚖️ Обновить вес");
            Console.WriteLine("4. 🧪 Обновить процент жира");
            Console.WriteLine("5. 📊 Обновить уровень опыта");
            Console.WriteLine("6. 🎯 Обновить цели");
            Console.WriteLine("7. 🧹 Очистить все параметры");
            Console.WriteLine("0. 🔙 Назад");

            var choice = ConsoleHelper.ReadInput("Ваш выбор");

            switch (choice)
            {
                case "1":
                    await UpdateViaNaturalLanguage(userId);
                    break;
                case "2":
                    await UpdateHeight(userId);
                    break;
                case "3":
                    await UpdateWeight(userId);
                    break;
                case "4":
                    await UpdateBodyFat(userId);
                    break;
                case "5":
                    await UpdateExperience(userId);
                    break;
                case "6":
                    await UpdateGoals(userId);
                    break;
                case "7":
                    await ClearParameters(userId);
                    break;
                case "0":
                    return;
                default:
                    ConsoleHelper.WriteError("Неверный выбор");
                    break;
            }

            ConsoleHelper.WaitForKey();
        }
    }

    private async Task ShowCurrentParameters(long userId)
    {
        var parameters = await _parametersService.GetUserParametersAsync(userId);

        ConsoleHelper.WriteHeader("ТЕКУЩИЕ ПАРАМЕТРЫ");

        if (parameters == null)
        {
            ConsoleHelper.WriteWarning("Параметры не заданы");
            return;
        }

        Console.WriteLine($"📏 Рост: {parameters.Height?.ToString() ?? "не указан"} см");
        Console.WriteLine($"⚖️ Вес: {parameters.Weight?.ToString() ?? "не указан"} кг");
        Console.WriteLine($"🧪 Процент жира: {parameters.BodyFat?.ToString() ?? "не указан"}%");
        Console.WriteLine($"📊 Опыт: {FormatExperience(parameters.Experience)}");
        Console.WriteLine($"🎯 Цели: {parameters.Goals ?? "не указаны"}");
    }

    private async Task UpdateViaNaturalLanguage(long userId)
    {
        ConsoleHelper.WriteHeader("ОБНОВЛЕНИЕ ЧЕРЕЗ ЕСТЕСТВЕННЫЙ ЯЗЫК");

        ConsoleHelper.WriteInfo("Примеры:");
        ConsoleHelper.WriteInfo("  • мой рост 180 см");
        ConsoleHelper.WriteInfo("  • вес 75 кг");
        ConsoleHelper.WriteInfo("  • процент жира 15");
        ConsoleHelper.WriteInfo("  • опыт средний");
        ConsoleHelper.WriteInfo("  • хочу набрать массу");
        ConsoleHelper.WriteInfo("  • рост 190 вес 85");

        var input = ConsoleHelper.ReadInput("Введите сообщение");

        if (string.IsNullOrWhiteSpace(input))
        {
            ConsoleHelper.WriteError("Сообщение не может быть пустым");
            return;
        }

        ConsoleHelper.WriteInfo("🤔 Обрабатываю...");

        try
        {
            var result = await _parametersAI.UpdateParametersDirectAsync(userId, input);
            ConsoleHelper.WriteSuccess(result);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
        }
    }

    private async Task UpdateHeight(long userId)
    {
        var heightStr = ConsoleHelper.ReadInput("Введите рост в см (например, 180)");
        if (int.TryParse(heightStr, out var height))
        {
            try
            {
                await _parametersService.UpdateHeightAsync(userId, height);
                ConsoleHelper.WriteSuccess($"Рост обновлен: {height} см");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            }
        }
        else
        {
            ConsoleHelper.WriteError("Некорректное значение");
        }
    }

    private async Task UpdateWeight(long userId)
    {
        var weightStr = ConsoleHelper.ReadInput("Введите вес в кг (например, 75.5)");
        if (decimal.TryParse(weightStr, out var weight))
        {
            try
            {
                await _parametersService.UpdateWeightAsync(userId, weight);
                ConsoleHelper.WriteSuccess($"Вес обновлен: {weight} кг");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            }
        }
        else
        {
            ConsoleHelper.WriteError("Некорректное значение");
        }
    }

    private async Task UpdateBodyFat(long userId)
    {
        var fatStr = ConsoleHelper.ReadInput("Введите процент жира (например, 15.5)");
        if (decimal.TryParse(fatStr, out var fat))
        {
            try
            {
                await _parametersService.UpdateBodyFatAsync(userId, fat);
                ConsoleHelper.WriteSuccess($"Процент жира обновлен: {fat}%");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            }
        }
        else
        {
            ConsoleHelper.WriteError("Некорректное значение");
        }
    }

    private async Task UpdateExperience(long userId)
    {
        Console.WriteLine("\nВыберите уровень:");
        Console.WriteLine("1. 🌱 Начинающий (beginner)");
        Console.WriteLine("2. 🌿 Средний (intermediate)");
        Console.WriteLine("3. 🌳 Продвинутый (advanced)");

        var choice = ConsoleHelper.ReadInput("Ваш выбор");

        string experience = choice switch
        {
            "1" => "beginner",
            "2" => "intermediate",
            "3" => "advanced",
            _ => null
        };

        if (experience != null)
        {
            try
            {
                await _parametersService.UpdateExperienceAsync(userId, experience);
                ConsoleHelper.WriteSuccess($"Опыт обновлен: {FormatExperience(experience)}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            }
        }
        else
        {
            ConsoleHelper.WriteError("Неверный выбор");
        }
    }

    private async Task UpdateGoals(long userId)
    {
        Console.WriteLine("\nВыберите цель:");
        Console.WriteLine("1. ⚖️ Похудение (lose_weight)");
        Console.WriteLine("2. 🏋️ Набор массы (gain_muscle)");
        Console.WriteLine("3. ✨ Поддержание формы (maintain)");
        Console.WriteLine("4. ❤️ Выносливость (endurance)");
        Console.WriteLine("5. 💪 Сила (strength)");
        Console.WriteLine("6. ✏️ Своя цель");

        var choice = ConsoleHelper.ReadInput("Ваш выбор");

        string goals = choice switch
        {
            "1" => "lose_weight",
            "2" => "gain_muscle",
            "3" => "maintain",
            "4" => "endurance",
            "5" => "strength",
            "6" => ConsoleHelper.ReadInput("Введите свою цель"),
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(goals))
        {
            try
            {
                await _parametersService.UpdateGoalsAsync(userId, goals);
                ConsoleHelper.WriteSuccess($"Цели обновлены: {goals}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            }
        }
        else
        {
            ConsoleHelper.WriteError("Цель не может быть пустой");
        }
    }

    private async Task ClearParameters(long userId)
    {
        if (ConsoleHelper.ReadInput("Вы уверены? (y/n)").ToLower() == "y")
        {
            await _parametersService.DeleteUserParametersAsync(userId);
            ConsoleHelper.WriteSuccess("Параметры очищены");
        }
    }

    private string FormatExperience(string? experience) => experience switch
    {
        "beginner" => "🌱 Начинающий",
        "intermediate" => "🌿 Средний",
        "advanced" => "🌳 Продвинутый",
        _ => "не указан"
    };
}