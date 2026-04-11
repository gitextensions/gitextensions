using GitCommands.Settings;
using GitUI.ConsoleEmulation.ConEmu;
using GitUI.ConsoleEmulation.PlainText;

namespace GitUI.ConsoleEmulation;

internal class ConsoleEmulatorsRegistry(
    IConsoleEmulator[] consoleEmulators,
    ISetting<bool> useConsoleEmulation,
    ISetting<string> consoleEmulatorName,
    ISetting<string> consoleEmulatorTheme)
    : IConsoleEmulatorsRegistry
{
    public IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators { get; } =
        consoleEmulators.Where(x => x.IsSupportedInCurrentEnvironment).ToArray();

    /// <summary>
    ///  Creates a console process controller for the configured emulator.
    /// </summary>
    public IConsoleCommandRunner CreateCommandController()
    {
        if (!useConsoleEmulation.Value)
        {
            return new PlainTextConsoleCommandRunner();
        }

        if (TryGetConfiguredConsoleEmulator() is { } configuredEmulator)
        {
            return configuredEmulator.CreateCommandRunner(ResolveTheme(configuredEmulator));
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateCommandRunner(ResolveTheme(fallbackConsoleEmulator));
        }

        // Fallback to no console emulation
        return new PlainTextConsoleCommandRunner();
    }

    /// <summary>
    ///  Creates a console shell controller for the configured emulator, if available.
    /// </summary>
    public IConsoleShellRunner? CreateShellRunner()
    {
        if (TryGetConfiguredConsoleEmulator() is { } configuredEmulator)
        {
            return configuredEmulator.CreateShellRunner(ResolveTheme(configuredEmulator));
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateShellRunner(ResolveTheme(fallbackConsoleEmulator));
        }

        return null;
    }

    private string? ResolveTheme(IConsoleEmulator emulator)
    {
        string? configured = consoleEmulatorTheme.Value;
        if (!string.IsNullOrEmpty(configured)
            && emulator.AvailableThemes.Contains(configured, StringComparer.OrdinalIgnoreCase))
        {
            return configured;
        }

        return emulator.DefaultTheme;
    }

    private IConsoleEmulator? TryGetConfiguredConsoleEmulator()
    {
        return AvailableConsoleEmulators
            .FirstOrDefault(p => string.Equals(p.Name, consoleEmulatorName.Value, StringComparison.OrdinalIgnoreCase));
    }

    private IConsoleEmulator? TryGetFallbackConsoleEmulator()
    {
        return AvailableConsoleEmulators.OfType<ConEmuConsoleEmulator>().FirstOrDefault();
    }
}
