using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.ConsoleEmulation;

// Twin of GitUI/ConsoleEmulation/ConsoleEmulatorSettings.cs. The point-based shim font keeps
// the original settings boundary; runners convert it only at the Avalonia presentation edge.

/// <summary>
///  Represents the visual settings used by the console emulator.
/// </summary>
/// <param name="Theme">The name of the theme to apply, or <see langword="null"/> to use the default theme.</param>
/// <param name="Font">The font to use for console output, or <see langword="null"/> to use the default font.</param>
public record ConsoleEmulatorSettings(string? Theme, WinFormsShims.Font? Font);
