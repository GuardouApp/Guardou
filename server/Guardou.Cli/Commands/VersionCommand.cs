using System.CommandLine;
using Guardou.Domain;

namespace Guardou.Cli.Commands;

internal static class VersionCommand
{
    public static Command Create()
    {
        Command command = new("version", "Show the version.");
        command.SetAction(parseResult => parseResult.InvocationConfiguration.Output.WriteLine(Product.Version));
        return command;
    }
}
