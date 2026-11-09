using System.ComponentModel;
using System.Diagnostics;

namespace GitUI.Compat;

/// <summary>
///  Resolves and launches host executables through Flatpak's explicitly granted host-command service.
/// </summary>
internal sealed class FlatpakHost
{
    private const int ToolNotFoundExitCode = 44;

    internal static readonly FlatpakHost Default = new();

    private readonly Func<ProcessStartInfo, (int ExitCode, string Output, string Error)> _query;

    internal FlatpakHost()
        : this(Query)
    {
    }

    internal FlatpakHost(Func<ProcessStartInfo, (int ExitCode, string Output, string Error)> query)
        => _query = query;

    internal string? ResolveExecutable(string executable)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executable);
        ProcessStartInfo request = new("/bin/sh");
        request.ArgumentList.Add("-c");
        request.ArgumentList.Add($"command -v \"$1\" || exit {ToolNotFoundExitCode}");
        request.ArgumentList.Add("gitextensions-tool-lookup");
        request.ArgumentList.Add(executable);
        ForwardToHost(request);
        try
        {
            (int exitCode, string output, string error) = _query(request);
            return exitCode switch
            {
                0 when !string.IsNullOrWhiteSpace(output) => output.TrimEnd('\r', '\n'),
                ToolNotFoundExitCode => null,
                _ => throw Unavailable(error),
            };
        }
        catch (Win32Exception exception)
        {
            throw Unavailable(exception.Message);
        }
    }

    internal static void ForwardToHost(ProcessStartInfo startInfo)
    {
        if (startInfo.Arguments.Length != 0)
        {
            throw new ArgumentException("Host launches require a separate argument vector.", nameof(startInfo));
        }

        string executable = startInfo.FileName;
        string[] arguments = [.. startInfo.ArgumentList];
        startInfo.FileName = "/usr/bin/flatpak-spawn";
        startInfo.UseShellExecute = false;
        startInfo.ArgumentList.Clear();
        startInfo.ArgumentList.Add("--host");
        startInfo.ArgumentList.Add("--watch-bus");
        if (!string.IsNullOrEmpty(startInfo.WorkingDirectory))
        {
            startInfo.ArgumentList.Add($"--directory={startInfo.WorkingDirectory}");
        }

        startInfo.ArgumentList.Add(executable);
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
    }

    private static PlatformNotSupportedException Unavailable(string reason)
        => new($"Host tools are not available in this Flatpak installation. {reason}");

    internal static (int ExitCode, string Output, string Error) Query(ProcessStartInfo startInfo)
        => DispatcherPump.Wait(() => QueryAsync(startInfo));

    private static async Task<(int ExitCode, string Output, string Error)> QueryAsync(ProcessStartInfo startInfo)
    {
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;
        using Process process = Process.Start(startInfo) ?? throw Unavailable("The host launcher could not start.");
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw Unavailable("The host-command service did not respond.");
        }

        return (process.ExitCode, await output, await error);
    }
}
