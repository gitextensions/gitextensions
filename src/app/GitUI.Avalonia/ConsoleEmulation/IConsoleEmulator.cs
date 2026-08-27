namespace GitUI.ConsoleEmulation;

/// <summary>
///  Represents a console emulator integration.
/// </summary>
public interface IConsoleEmulator
{
    /// <summary>
    ///  Gets the emulator name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///  Gets the emulator name shown in the UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    ///  Gets a value indicating whether this emulator can run in the current environment
    ///  (for example, because its required executables are present).
    /// </summary>
    bool IsSupportedInCurrentEnvironment { get; }

    /// <summary>
    ///  Gets the themes supported by this emulator. Empty when the emulator
    ///  does not support theme selection or none are installed.
    /// </summary>
    IReadOnlyCollection<string> AvailableThemes { get; }

    /// <summary>
    ///  Gets the default theme to use when the configured theme is missing or
    ///  unknown. Returns <see langword="null"/> when no usable default exists.
    ///  When non-null, the value is guaranteed to be present in <see cref="AvailableThemes"/>.
    /// </summary>
    string? DefaultTheme { get; }

    /// <summary>
    ///  Creates a console process runner for command dialogs.
    /// </summary>
    IConsoleCommandRunner CreateCommandRunner(ConsoleEmulatorSettings settings);

    /// <summary>
    ///  Creates a console shell runner for the repository browser's terminal tab.
    /// </summary>
    IConsoleShellRunner CreateShellRunner(ConsoleEmulatorSettings settings);
}
