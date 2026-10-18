using System.CommandLine;
using System.Reflection;

namespace Guardou.Cli.Commands;

internal static class VersionCommand
{
    public static Command Create()
    {
        Command command = new("version", "Show the version.");
        command.SetAction(parseResult => parseResult.InvocationConfiguration.Output.WriteLine(Version));
        return command;
    }

    private static string Version =>
        typeof(VersionCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? string.Empty;
}
