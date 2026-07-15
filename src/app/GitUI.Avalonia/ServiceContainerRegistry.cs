using System.ComponentModel.Design;
using GitExtUtils;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.ConsoleEmulation.PlainText;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUI;

// Twin of GitUI/ServiceContainerRegistry.cs (reduced): services are added here as their
// implementations get ported. ConEmu and Mintty are Windows-only, so the Avalonia UI always
// uses the plain text console emulation.
public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        serviceContainer.AddService<IConsoleEmulatorsRegistry>(PlainTextConsoleEmulatorsRegistry.Instance);
        serviceContainer.AddService<IHotkeySettingsLoader>(new HotkeySettingsManager());
        serviceContainer.AddService<ITerminalLauncher>(new TerminalLauncher());
    }
}
