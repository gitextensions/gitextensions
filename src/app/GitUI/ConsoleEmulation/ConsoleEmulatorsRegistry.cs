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
    public IConsoleCommandController CreateCommandController()
    {
        if (!useConsoleEmulation.Value)
        {
            return new PlainTextConsoleCommandController();
        }

        if (TryGetConfiguredConsoleEmulator() is { } configuredEmulator)
        {
            return configuredEmulator.CreateConsoleProcessController();
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateConsoleProcessController();
        }

        // Fallback to no console emulation
        return new PlainTextConsoleCommandController();
    }

    /// <summary>
    ///  Creates a console shell controller for the configured emulator, if available.
    /// </summary>
    public IConsoleShellController? CreateShellController()
    {
        if (TryGetConfiguredConsoleEmulator() is { } configuredEmulator)
        {
            return configuredEmulator.CreateConsoleShellController();
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateConsoleShellController();
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
