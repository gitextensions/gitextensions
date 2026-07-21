using GitCommands;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.ConsoleEmulation.PlainText;

// Avalonia twin of PlainTextConsoleEmulatorsRegistry. Unlike the WinForms fallback, the
// Avalonia implementation also provides the portable embedded plain-text shell.
internal sealed class PlainTextConsoleEmulatorsRegistry : IConsoleEmulatorsRegistry
{
    private static readonly PlainTextConsoleEmulator _emulator = new();
    private readonly Func<WinFormsShims.Font?> _consoleFont;

    public static PlainTextConsoleEmulatorsRegistry Instance { get; } = new(() => AppSettings.ConEmuConsoleFont);

    private PlainTextConsoleEmulatorsRegistry(Func<WinFormsShims.Font?> consoleFont)
    {
        _consoleFont = consoleFont;
    }

    public IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators
        => _emulator.IsSupportedInCurrentEnvironment ? [_emulator] : [];

    public IConsoleCommandRunner CreateCommandController()
    {
        return _emulator.CreateCommandRunner(GetSettings());
    }

    public IConsoleShellRunner? CreateShellRunner()
        => _emulator.IsSupportedInCurrentEnvironment
            ? _emulator.CreateShellRunner(GetSettings())
            : null;

    private ConsoleEmulatorSettings GetSettings()
        => new(Theme: null, Font: _consoleFont());
}
