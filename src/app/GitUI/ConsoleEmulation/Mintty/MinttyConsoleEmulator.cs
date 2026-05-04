using System.Diagnostics.CodeAnalysis;
using GitCommands;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttyConsoleEmulator : IConsoleEmulator
{
    private const string PreferredDefaultTheme = "monokai-dimmed";

    private IReadOnlyCollection<string>? _availableThemes;

    public string Name => "mintty";

    public string DisplayName => "Mintty";

    public bool IsSupportedInCurrentEnvironment => OperatingSystem.IsWindows() && TryResolvePaths(out _, out _);

    public IReadOnlyCollection<string> AvailableThemes => _availableThemes ??= LoadAvailableThemes();

    public string? DefaultTheme =>
        AvailableThemes.Contains(PreferredDefaultTheme, StringComparer.OrdinalIgnoreCase)
            ? PreferredDefaultTheme
            : null;

    public IConsoleCommandRunner CreateCommandRunner(ConsoleEmulatorSettings settings)
    {
        (string minttyPath, string bashPath) = ResolvePaths();
        return new MinttyCommandRunner(minttyPath, bashPath, settings);
    }

    public IConsoleShellRunner CreateShellRunner(ConsoleEmulatorSettings settings)
    {
        (string minttyPath, string bashPath) = ResolvePaths();
        return new MinttyShellRunner(minttyPath, bashPath, settings);
    }

    private static (string MinttyPath, string BashPath) ResolvePaths()
    {
        if (!TryResolvePaths(out string? minttyPath, out string? bashPath))
        {
            throw new InvalidOperationException($"mintty or bash not found. Check {nameof(IsSupportedInCurrentEnvironment)} before calling.");
        }

        return (minttyPath, bashPath);
    }

    private static bool TryResolvePaths([NotNullWhen(true)] out string? minttyPath, [NotNullWhen(true)] out string? bashPath)
    {
        bool foundMintty = PathUtil.TryFindShellPath(@"usr\bin\mintty.exe", out minttyPath);
        bool foundBash = PathUtil.TryFindShellPath("bash.exe", out bashPath);
        return foundMintty && foundBash;
    }

    private static IReadOnlyCollection<string> LoadAvailableThemes()
    {
        if (!TryResolvePaths(out string? minttyPath, out _))
        {
            return [];
        }

        try
        {
            string themesDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(minttyPath)!, "..", "share", "mintty", "themes"));
            if (!Directory.Exists(themesDir))
            {
                return [];
            }

            return Directory.EnumerateFiles(themesDir)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name!)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            return [];
        }
    }
}
