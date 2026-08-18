using System.Diagnostics;
using System.Text.Json;
using GitCommands.Git.Extensions;

namespace GitExtensions.Compat;

// parity-scaffolding: records product process-tree cancellation inside the release-shaped Flatpak.
internal static class FlatpakConformanceProbe
{
    internal const string ReportPathEnvironmentVariable = "GITEXTENSIONS_FLATPAK_CONFORMANCE_REPORT";

    public static async Task<int?> RunIfRequestedAsync()
    {
        string? reportPath = Environment.GetEnvironmentVariable(ReportPathEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(reportPath))
        {
            return null;
        }

        string fullReportPath = Path.GetFullPath(reportPath);
        string childProcessPath = fullReportPath + ".child-pid";
        Directory.CreateDirectory(Path.GetDirectoryName(fullReportPath)!);
        File.Delete(childProcessPath);

        using Process process = new()
        {
            StartInfo =
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"sleep 60 & echo $! > '{childProcessPath}'; wait\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        if (!process.StartInOwnProcessGroup())
        {
            throw new InvalidOperationException("The process-group fixture did not start.");
        }

        for (int attempt = 0; attempt < 40 && !File.Exists(childProcessPath); attempt++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        int childProcessId = int.Parse(await File.ReadAllTextAsync(childProcessPath));
        process.TerminateTree();
        using CancellationTokenSource exitTimeout = new(TimeSpan.FromSeconds(4));
        await process.WaitForExitAsync(exitTimeout.Token);
        bool cancellationObserved = process.HasExited;

        string childProcessDirectory = $"/proc/{childProcessId}";
        for (int attempt = 0; attempt < 40 && IsProcessRunning(childProcessDirectory); attempt++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        bool childProcessTerminated = !IsProcessRunning(childProcessDirectory);
        bool passed = cancellationObserved && childProcessTerminated;

        await using FileStream stream = File.Create(fullReportPath);
        await JsonSerializer.SerializeAsync(stream, new
        {
            schemaVersion = 1,
            confined = File.Exists("/.flatpak-info"),
            processTreeCancellation = new
            {
                cancellationObserved,
                childProcessTerminated
            },
            passed
        }, new JsonSerializerOptions { WriteIndented = true });
        await stream.WriteAsync("\n"u8.ToArray());
        return passed ? 0 : 1;
    }

    private static bool IsProcessRunning(string processDirectory)
    {
        try
        {
            string stat = File.ReadAllText(Path.Join(processDirectory, "stat"));
            int commandEnd = stat.LastIndexOf(')');
            char state = stat[commandEnd + 2];
            return state is not ('Z' or 'X');
        }
        catch (IOException)
        {
            return false;
        }
    }
}
