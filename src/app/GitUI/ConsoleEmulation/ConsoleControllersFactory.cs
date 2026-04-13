using GitUI.ConsoleEmulation.ConEmu;
using GitUI.ConsoleEmulation.NoEmulation;

namespace GitUI.ConsoleEmulation;

internal class ConsoleControllersFactory(IConsoleEmulator[] consoleEmulators) : IConsoleControllersFactory
{
    public IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators { get; } =
        consoleEmulators.Where(x => x.IsSupportedInCurrentEnvironment).ToArray();

    /// <summary>
    ///  Creates a console process controller for the configured emulator.
    /// </summary>
    public IConsoleProcessController CreateConsoleProcessController(bool useConsoleEmulation, string configuredConsoleEmulator)
    {
        if (!useConsoleEmulation)
        {
            return new NoEmulationConsoleProcessController();
        }

        if (TryGetConfiguredConsoleEmulator(configuredConsoleEmulator) is { } configuredEmulator)
        {
            return configuredEmulator.CreateConsoleProcessController();
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateConsoleProcessController();
        }

        // Fallback to no console emulation
        return new NoEmulationConsoleProcessController();
    }

    /// <summary>
    ///  Creates a console shell controller for the configured emulator, if available.
    /// </summary>
    public IConsoleShellController? CreateConsoleShellControl(string configuredConsoleEmulator)
    {
        if (TryGetConfiguredConsoleEmulator(configuredConsoleEmulator) is { } configuredEmulator)
        {
            return configuredEmulator.CreateConsoleShellController();
        }

        if (TryGetFallbackConsoleEmulator() is { } fallbackConsoleEmulator)
        {
            return fallbackConsoleEmulator.CreateConsoleShellController();
        }

        return null;
    }

    private IConsoleEmulator? TryGetConfiguredConsoleEmulator(string configuredConsoleEmulator)
    {
        return AvailableConsoleEmulators
            .FirstOrDefault(p => string.Equals(p.Name, configuredConsoleEmulator, StringComparison.OrdinalIgnoreCase));
    }

    private IConsoleEmulator? TryGetFallbackConsoleEmulator()
    {
        return AvailableConsoleEmulators.OfType<ConEmuConsoleEmulator>().FirstOrDefault();
    }
}
