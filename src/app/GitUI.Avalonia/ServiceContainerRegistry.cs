using System.ComponentModel.Design;
using GitCommands;
using GitExtUtils;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.ConsoleEmulation.PlainText;
using GitUI.Hotkey;
using GitUI.Models;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI;

// Twin of GitUI/ServiceContainerRegistry.cs (reduced): services are added here as their
// implementations get ported. ConEmu and Mintty are Windows-only, so the Avalonia UI always
// uses the plain text console emulation.
public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        ScriptsManager scriptsManager = new();
        OutputHistoryModel outputHistoryModel = new(AppSettings.OutputHistoryDepth.Value);
        serviceContainer.GetRequiredService<ISubscribableTraceListener>().TraceReceived += (in string message) =>
        {
#if DEBUG
            const char noBreakSpace = '\u00a0';
            if (message.Contains("Exception") || message.Contains($":{noBreakSpace}"))
#endif
            {
                outputHistoryModel.RecordHistory(message);
            }
        };

        serviceContainer.AddService<IConsoleEmulatorsRegistry>(PlainTextConsoleEmulatorsRegistry.Instance);
        serviceContainer.AddService<IScriptsManager>(scriptsManager);
        serviceContainer.AddService<IScriptsRunner>(scriptsManager);
        serviceContainer.AddService<ISimplePromptCreator>(new SimplePromptCreator());
        serviceContainer.AddService<IFilePromptCreator>(new FilePromptCreator());
        serviceContainer.AddService<IHotkeySettingsLoader>(new HotkeySettingsManager());
        serviceContainer.AddService<IOutputHistoryProvider>(outputHistoryModel);
        serviceContainer.AddService<IOutputHistoryRecorder>(outputHistoryModel);
        serviceContainer.AddService<ITerminalLauncher>(new TerminalLauncher());
    }
}
