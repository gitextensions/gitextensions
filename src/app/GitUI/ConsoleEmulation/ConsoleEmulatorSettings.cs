namespace GitUI.ConsoleEmulation;

/// <summary>
///  Represents the visual settings used by the console emulator.
/// </summary>
/// <param name="Theme">The name of the theme to apply, or <see langword="null"/> to use the default theme.</param>
/// <param name="Font">The font to use for console output, or <see langword="null"/> to use the default font.</param>
public record ConsoleEmulatorSettings(string? Theme, Font? Font);
