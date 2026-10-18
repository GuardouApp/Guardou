using System.CommandLine;
using Guardou.Api;
using Microsoft.AspNetCore.Builder;

namespace Guardou.Cli.Commands;

internal static class RunCommand
{
    public static Command Create()
    {
        Command command = new("run", "Start the server.");
        command.SetAction(async (_, _) =>
        {
            await using WebApplication app = WebServer.Build(["--contentRoot", AppContext.BaseDirectory]);
            await app.RunAsync();
        });
        return command;
    }
}
