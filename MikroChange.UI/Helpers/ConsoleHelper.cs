using System.Text;


namespace MikroChange.UI.Helpers;


public static class ConsoleHelper
{
    public static void PrintLine(ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }


    public static void PrintSuccess(string message)
    {
        PrintLine(ConsoleColor.Green, message);
    }


    public static void PrintWarning(string message)
    {
        PrintLine(ConsoleColor.DarkYellow, message);
    }


    public static void PrintError(string message)
    {
        PrintLine(ConsoleColor.Red, message);
    }


    public static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var password = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }

        return password.ToString();
    }


    public static string ReadNonEmpty(string prompt, string? nonEmptyAlert = default)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            PrintLine(ConsoleColor.Red, nonEmptyAlert ?? "O valor não pode ser vazio.");
        }
    }


    public static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} (s/n): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        return response is "s" or "sim" or "y" or "yes";
    }


    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Pressione ENTER para continuar...");
        Console.ReadLine();
    }
}
