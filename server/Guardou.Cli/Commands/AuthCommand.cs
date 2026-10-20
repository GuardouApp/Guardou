using System.CommandLine;
using Guardou.Application.Authentication;
using Guardou.Cli.Hosting;

namespace Guardou.Cli.Commands;

internal static class AuthCommand
{
    public static Command Create()
    {
        Command command = new("auth", "Manage the authentication.");
        command.Subcommands.Add(CreateEnablePasswordLogin());
        return command;
    }

    private static Command CreateEnablePasswordLogin()
    {
        Command command = new("enable-password-login", "Enable the password login again.");
        command.SetAction((_, cancellationToken) => CliHost.ExecuteAsync<OidcSettingsAdministration>(
            async administration =>
            {
                await administration.EnablePasswordLoginAsync(cancellationToken);
                return 0;
            },
            cancellationToken));
        return command;
    }
}
