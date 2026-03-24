#nullable enable

using System.Diagnostics;
using GitCommands;
using Microsoft.VisualStudio.Threading;

namespace GitUI;

/// <summary>
/// Represents a VS Code installation with its executable path and variant type.
/// </summary>
internal sealed record VSCodeInstallation(string ExecutablePath, VSCodeVariant Variant);

/// <summary>
/// Identifies the variant of VS Code.
/// </summary>
internal enum VSCodeVariant
{
    /// <summary>
    /// Stable VS Code (blue icon).
    /// </summary>
    Stable,

    /// <summary>
    /// VS Code Insiders (green icon).
    /// </summary>
    Insiders
}

/// <summary>
/// Provides integration with Visual Studio Code for opening files.
/// </summary>
internal static class VSCodeIntegration
{
    private static readonly object _lock = new();
    private static VSCodeInstallation? _stableInstallation;
    private static VSCodeInstallation? _insidersInstallation;

    static VSCodeIntegration()
    {
        ThreadHelper.FileAndForget(InitializeAsync);
    }

    /// <summary>
    /// Triggers initialization of VS Code detection.
    /// Call this early in the application lifecycle to ensure installations are detected
    /// before context menus are displayed.
    /// </summary>
    public static void Init()
    {
        // Just access the static constructor to trigger initialization
    }

    /// <summary>
    /// Gets a value indicating whether any VS Code installation is available.
    /// </summary>
    public static bool IsVSCodeInstalled => _stableInstallation is not null || _insidersInstallation is not null;

    /// <summary>
    /// Gets the stable VS Code installation, if available.
    /// </summary>
    public static VSCodeInstallation? StableInstallation => _stableInstallation;

    /// <summary>
    /// Gets the VS Code Insiders installation, if available.
    /// </summary>
    public static VSCodeInstallation? InsidersInstallation => _insidersInstallation;

    /// <summary>
    /// Gets all available VS Code installations.
    /// </summary>
    public static IEnumerable<VSCodeInstallation> GetInstallations()
    {
        if (_stableInstallation is not null)
        {
            yield return _stableInstallation;
        }

        if (_insidersInstallation is not null)
        {
            yield return _insidersInstallation;
        }
    }

    /// <summary>
    /// Opens a file in VS Code.
    /// </summary>
    /// <param name="installation">The VS Code installation to use.</param>
    /// <param name="filePath">The path to the file to open.</param>
    /// <param name="lineNumber">Optional line number to navigate to.</param>
    public static void OpenFile(VSCodeInstallation installation, string filePath, int lineNumber = 0)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = installation.ExecutablePath,
                Arguments = BuildArguments(filePath, lineNumber),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to open file in VS Code: {ex.Message}");
        }
    }

    private static string BuildArguments(string filePath, int lineNumber)
    {
        // --reuse-window: Reuse an existing VS Code window if available
        // --goto: Go to a specific line (format: file:line)
        if (lineNumber > 0)
        {
            return $"--reuse-window --goto \"{filePath}\":{lineNumber}";
        }

        return $"--reuse-window \"{filePath}\"";
    }

    private static async Task InitializeAsync()
    {
        await TaskScheduler.Default;

        VSCodeInstallation? stable = await DetectInstallationAsync("code", VSCodeVariant.Stable);
        VSCodeInstallation? insiders = await DetectInstallationAsync("code-insiders", VSCodeVariant.Insiders);

        lock (_lock)
        {
            _stableInstallation = stable;
            _insidersInstallation = insiders;
        }
    }

    private static Task<VSCodeInstallation?> DetectInstallationAsync(string executableName, VSCodeVariant variant)
    {
        return Task.Run(() =>
        {
            // On Windows, VS Code adds its bin folder to PATH, so 'code' and 'code-insiders' should be directly accessible
            // The executable is actually a .cmd file on Windows
            string[] candidates =
            [
                $"{executableName}.cmd",
                $"{executableName}.exe",
                executableName
            ];

            foreach (string candidate in candidates)
            {
                if (PathUtil.TryFindFullPath(candidate, out string? fullPath))
                {
                    return new VSCodeInstallation(fullPath, variant);
                }
            }

            // Also check common installation directories
            string[] commonPaths =
            [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "bin", $"{executableName}.cmd"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code Insiders", "bin", $"{executableName}.cmd"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "bin", $"{executableName}.cmd"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code Insiders", "bin", $"{executableName}.cmd"),
            ];

            foreach (string path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return new VSCodeInstallation(path, variant);
                }
            }

            return null;
        });
    }
}
