using Avalonia.Media;

namespace GitUI.ConsoleEmulation;

// Twin of GitUI/ConsoleEmulation/ConsoleEmulatorSettings.cs (the font is an Avalonia font family).

/// <summary>
///  Represents the visual settings used by the console emulator.
/// </summary>
/// <param name="Theme">The name of the theme to apply, or <see langword="null"/> to use the default theme.</param>
/// <param name="Font">The font to use for console output, or <see langword="null"/> to use the default font.</param>
public record ConsoleEmulatorSettings(string? Theme, FontFamily? Font);
