using System.CommandLine;
using Guardou.Cli.Commands;

RootCommand root = RootCommandFactory.Create();
return await root.Parse(args).InvokeAsync(new InvocationConfiguration { ProcessTerminationTimeout = null });
