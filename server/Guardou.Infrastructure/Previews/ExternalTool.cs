using System.Diagnostics;

namespace Guardou.Infrastructure.Previews;

internal sealed class ExternalTool(string name)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

    private readonly Lazy<string?> _path = new(() => Find(name));

    public bool IsAvailable => _path.Value is not null;

    public async Task<byte[]> RunAsync(IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        ProcessStartInfo start = new(_path.Value ?? throw new InvalidOperationException($"The tool '{name}' is not installed."))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using CancellationTokenSource limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(Timeout);
        using Process process = Process.Start(start) ?? throw new InvalidOperationException($"The tool '{name}' did not start.");
        using MemoryStream output = new();
        try
        {
            Task<string> error = process.StandardError.ReadToEndAsync(limit.Token);
            await process.StandardOutput.BaseStream.CopyToAsync(output, limit.Token);
            await process.WaitForExitAsync(limit.Token);
            return process.ExitCode == 0
                ? output.ToArray()
                : throw new InvalidDataException($"{name} failed: {(await error).Trim()}");
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }
    }

    private static string? Find(string name)
    {
        string[] candidates = OperatingSystem.IsWindows() ? [name + ".exe", name] : [name];
        return (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .SelectMany(directory => candidates.Select(candidate => Path.Combine(directory, candidate)))
            .FirstOrDefault(File.Exists);
    }
}
