using GitCommands.Settings;
using GitUI.ConsoleEmulation.ConEmu;
using GitUI.ConsoleEmulation.PlainText;

namespace GitUI.ConsoleEmulation;

internal class ConsoleEmulatorsRegistry(
    IConsoleEmulator[] consoleEmulators,
    ISetting<bool> useConsoleEmulation,
    ISetting<string> consoleEmulatorName)
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
            return configuredEmulator.CreateCommandRunner();
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateCommandRunner();
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
            return configuredEmulator.CreateShellRunner();
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateShellRunner();
        }

        return null;
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
