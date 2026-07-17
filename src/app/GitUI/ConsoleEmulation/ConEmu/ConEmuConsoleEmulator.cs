using GitUI.Shells;

namespace GitUI.ConsoleEmulation.ConEmu;

internal sealed class ConEmuConsoleEmulator : IConsoleEmulator
{
    private readonly IShellProvider _shellProvider;

    internal ConEmuConsoleEmulator(IShellProvider shellProvider)
    {
        _shellProvider = shellProvider;
    }

    private const string DarkThemeFallback = "<Tomorrow Night>";
    private const string LightThemeFallback = "<Tomorrow>";

    private static readonly Font DefaultFont = new("Consolas", 12);

    public string Name => "conemu";

    public string DisplayName => "ConEmu";

    public bool IsSupportedInCurrentEnvironment => OperatingSystem.IsWindows();

    public IReadOnlyCollection<string> AvailableThemes { get; } =
    [
        "<Default Windows scheme>",
        "<Base16>",
        "<Cobalt2>",
        "<ConEmu>",
        "<Gamma 1>",
        "<Monokai>",
        "<Murena scheme>",
        "<PowerShell>",
        "<Solarized>",
        "<Solarized Git>",
        "<Solarized (Luke Maciak)>",
        "<Solarized (John Doe)>",
        "<Solarized Light>",
        "<SolarMe>",
        "<Standard VGA>",
        "<tc-maxx>",
        "<Terminal.app>",
        "<Tomorrow>",
        "<Tomorrow Night>",
        "<Tomorrow Night Blue>",
        "<Tomorrow Night Bright>",
        "<Tomorrow Night Eighties>",
        "<Twilight>",
        "<Ubuntu>",
        "<xterm>",
        "<Zenburn>",
    ];

    public string? DefaultTheme => null;

    public IConsoleCommandRunner CreateCommandRunner(ConsoleEmulatorSettings settings)
    {
        return new ConEmuConsoleCommandRunner(settings with
        {
            Theme = ResolveTheme(settings.Theme),
            Font = settings.Font ?? DefaultFont,
        });
    }

    public IConsoleShellRunner CreateShellRunner(ConsoleEmulatorSettings settings)
    {
        return new ConEmuConsoleShellRunner(_shellProvider, settings with
        {
            Theme = ResolveTheme(settings.Theme),
            Font = settings.Font ?? DefaultFont,
        });
    }

    internal static string ResolveTheme(string? configuredTheme)
    {
        return string.IsNullOrEmpty(configuredTheme)
            ? Application.IsDarkModeEnabled ? DarkThemeFallback : LightThemeFallback
            : configuredTheme;
    }
}
