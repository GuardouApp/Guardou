using System.CommandLine;
using Guardou.Application.Users;
using Guardou.Cli.Hosting;
using Guardou.Cli.Input;

namespace Guardou.Cli.Commands;

internal static class UserCommand
{
    public static Command Create(Option<bool> noInput)
    {
        Command command = new("user", "Manage users.");
        command.Subcommands.Add(CreateSet(noInput));
        command.Subcommands.Add(CreatePromote());
        return command;
    }

    private static Command CreateSet(Option<bool> noInput)
    {
        Argument<string> username = new("username") { Description = "The username." };
        Option<string> displayName = new("--display-name", "-d") { Description = "The display name. The username when not given for a new user." };
        Option<string> email = new("--email", "-e") { Description = "The email. Required to create a user." };
        Option<string> password = new("--password", "-p") { Description = "The password. Prompt when not given." };
        Option<bool> admin = new("--admin") { Description = "Give the admin role to the user." };

        Command command = new("set", "Create a user or change the password, the display name and the email of a user.")
        {
            username,
            displayName,
            email,
            password,
            admin,
        };
        command.SetAction((parseResult, cancellationToken) =>
        {
            string? givenPassword = parseResult.GetValue(password);
            string? resolvedPassword = givenPassword ?? PasswordPrompt.Read(parseResult.GetValue(noInput), parseResult.InvocationConfiguration);
            if (resolvedPassword is null)
            {
                return Task.FromResult(1);
            }

            return CliHost.ExecuteAsync(
                parseResult,
                administration => administration.SetAsync(
                    parseResult.GetRequiredValue(username),
                    parseResult.GetValue(displayName),
                    parseResult.GetValue(email),
                    resolvedPassword,
                    parseResult.GetValue(admin)),
                cancellationToken);
        });
        return command;
    }

    private static Command CreatePromote()
    {
        Argument<string> username = new("username") { Description = "The username." };

        Command command = new("promote", "Give the admin role to a user.") { username };
        command.SetAction((parseResult, cancellationToken) => CliHost.ExecuteAsync(
            parseResult,
            administration => administration.PromoteAsync(parseResult.GetRequiredValue(username)),
            cancellationToken));
        return command;
    }
}
