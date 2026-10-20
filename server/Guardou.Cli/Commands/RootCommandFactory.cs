using System.CommandLine;
using Guardou.Domain;

namespace Guardou.Cli.Commands;

internal static class RootCommandFactory
{
    public static RootCommand Create()
    {
        Option<bool> noInput = new("--no-input")
        {
            Description = "Never prompt for input. Fail when a value is missing.",
            Recursive = true,
        };

        RootCommand root = new($"{Product.Name}, a self-hosted file manager.");
        root.Options.Add(noInput);
        root.Subcommands.Add(RunCommand.Create());
        root.Subcommands.Add(VersionCommand.Create());
        root.Subcommands.Add(UserCommand.Create(noInput));
        root.Subcommands.Add(AuthCommand.Create());
        return root;
    }
}
