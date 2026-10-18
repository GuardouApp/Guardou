using System.CommandLine;
using System.Text;

namespace Guardou.Cli.Input;

internal static class PasswordPrompt
{
    public static string? Read(bool noInput, InvocationConfiguration configuration)
    {
        if (noInput || Console.IsInputRedirected)
        {
            configuration.Error.WriteLine("The password is missing. Give it with --password.");
            return null;
        }

        string password = ReadHidden(configuration, "Password: ");
        string confirmation = ReadHidden(configuration, "Confirm the password: ");
        if (password == confirmation)
        {
            return password;
        }

        configuration.Error.WriteLine("The passwords are different.");
        return null;
    }

    private static string ReadHidden(InvocationConfiguration configuration, string prompt)
    {
        configuration.Output.Write(prompt);
        StringBuilder value = new();
        for (ConsoleKeyInfo key = Console.ReadKey(intercept: true); key.Key != ConsoleKey.Enter; key = Console.ReadKey(intercept: true))
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                value.Length = Math.Max(0, value.Length - 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                value.Append(key.KeyChar);
            }
        }

        configuration.Output.WriteLine();
        return value.ToString();
    }
}
