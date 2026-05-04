namespace GitUI.ConsoleEmulation.ConEmu;

internal class ConEmuConsoleEmulator : IConsoleEmulator
{
    private const string DarkThemeFallback = "<Tomorrow Night>";
    private const string LightThemeFallback = "<Tomorrow>";

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

    public IConsoleCommandRunner CreateCommandRunner(string? theme)
    {
        return new ConEmuConsoleCommandRunner(ResolveTheme(theme));
    }

    public IConsoleShellRunner CreateShellRunner(string? theme)
    {
        return new ConEmuConsoleShellRunner(ResolveTheme(theme));
    }

    internal static string ResolveTheme(string? configuredTheme)
    {
        return string.IsNullOrEmpty(configuredTheme)
            ? Application.IsDarkModeEnabled ? DarkThemeFallback : LightThemeFallback
            : configuredTheme;
    }
}
