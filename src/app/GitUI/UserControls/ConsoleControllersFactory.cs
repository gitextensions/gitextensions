using GitCommands;
using GitExtensions.Extensibility.Plugins;
using GitUIPluginInterfaces;

namespace GitUI.UserControls;

public static class ConsoleControllersFactory
{
    /// <summary>
    /// Returns the available console emulator options as (key, label) pairs.
    /// </summary>
    public static IReadOnlyList<EmulatorInfo> GetAvailableEmulators()
    {
        List<EmulatorInfo> list = [];

        if (ConEmuConsoleProcessController.IsSupportedInThisEnvironment)
        {
            list.Add(new("", "ConEmu (built-in)"));
        }

        foreach (IConsolePlugin plugin in GetSupportedConsolePlugins())
        {
            list.Add(new(plugin.Name!, plugin.Name!));
        }

        return list;
    }

    /// <summary>
    /// Create a new instance of configured console process controller.
    /// </summary>
    public static IConsoleProcessController CreateConsoleProcessController()
    {
        if (!AppSettings.UseConsoleEmulatorForCommands)
        {
            return new EditboxBasedConsoleProcessController();
        }

        IConsolePlugin? configuredPlugin = GetSupportedConsolePlugins()
            .FirstOrDefault(p => string.Equals(p.Name, AppSettings.ConsoleEmulatorName, StringComparison.OrdinalIgnoreCase));

        if (configuredPlugin != null)
        {
            return configuredPlugin.CreateConsoleProcessController();
        }

        // Fallback to ConEmu if supported
        if (ConEmuConsoleProcessController.IsSupportedInThisEnvironment)
        {
            return new ConEmuConsoleProcessController();
        }

        // Fallback to no console emulation
        return new EditboxBasedConsoleProcessController();
    }

    /// <summary>
    /// Try creating configured console shell controller.
    /// </summary>
    public static IConsoleShellController? CreateConsoleShellControl()
    {
        string configured = AppSettings.ConsoleEmulatorName;

        IConsolePlugin? configuredPlugin = null;
        if (!string.IsNullOrEmpty(configured))
        {
            configuredPlugin = GetSupportedConsolePlugins()
                .FirstOrDefault(p => string.Equals(p.Name, configured, StringComparison.OrdinalIgnoreCase));
        }

        if (configuredPlugin is not null)
        {
            return configuredPlugin.CreateConsoleShellController();
        }

        // Fall back to ConEmu.
        if (ConEmuConsoleProcessController.IsSupportedInThisEnvironment)
        {
            return new ConEmuConsoleShellController();
        }

        return null;
    }

    private static IEnumerable<IConsolePlugin> GetSupportedConsolePlugins()
    {
        return PluginRegistry.Plugins
            .OfType<IConsolePlugin>()
            .Where(p => p.IsSupportedInCurrentEnvironment)
            .Select(p => p);
    }

    public record EmulatorInfo(string Key, string Label);
}
