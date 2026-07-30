using System.Diagnostics;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming;

// Avalonia twin of GitUI/Theming/ThemeModule.cs. Theme loading stays identical; applying
// WinForms system colors and ICSharpCode.TextEditor settings is owned by the UI framework.
public static class ThemeModule
{
    public static ThemeSettings Settings { get; private set; } = ThemeSettings.Default;

    private static ThemeRepository Repository { get; } = new();

    public static void Load()
    {
        Settings = LoadThemeSettings(Repository);

        // AvaloniaThemeSettings owns the framework color-mode update because Avalonia exposes
        // the actual platform variant instead of WinForms' Application.SetColorMode API.
        ColorHelper.ThemeSettings = Settings;
    }

    private static ThemeSettings LoadThemeSettings(IThemeRepository repository)
    {
        Theme invariantTheme;
        try
        {
            invariantTheme = repository.GetInvariantTheme();
        }
        catch (ThemeException ex)
        {
            MessageBoxes.ShowError(null, $"Failed to load invariant theme: {ex.Message}"
                    + $"{Environment.NewLine}{Environment.NewLine}See also https://github.com/gitextensions/gitextensions/wiki/Dark-Mode");
            return ThemeSettings.Default;
        }

        ThemeId themeId = AppSettings.ThemeId;
        if (string.IsNullOrEmpty(themeId.Name))
        {
            // Migrate from default invariant/light to Windows default color mode
            themeId = ThemeId.WindowsAppColorModeId;
            AppSettings.ThemeId = themeId;
        }

        bool systemVisualStyle = AppSettings.UseSystemVisualStyle;
        if (themeId == ThemeId.WindowsAppColorModeId)
        {
            // fix systemVisualStyle for WindowsAppColorModeId mode (always for DefaultLight)
            // This is also how it is presented in Settings
            themeId = ThemeId.ColorModeThemeId;
            systemVisualStyle = themeId == ThemeId.DefaultLight;
        }

        string[] variations = AppSettings.ThemeVariations;
        if (themeId == ThemeId.DefaultLight)
        {
            // default/invariant/light theme
            return CreateFallbackSettings(invariantTheme, variations);
        }

        Theme theme;
        try
        {
            theme = repository.GetTheme(themeId, variations);
        }
        catch (ThemeException ex)
        {
            Trace.WriteLine($"Failed to load {(themeId.IsBuiltin ? "preinstalled" : "user-defined")} theme {themeId.Name}: {ex}");
            MessageBoxes.ShowError(null, $"Failed to load {(themeId.IsBuiltin ? "preinstalled" : "user-defined")} theme {themeId.Name}: {ex.Message}"
                    + $"{Environment.NewLine}{Environment.NewLine}See also https://github.com/gitextensions/gitextensions/wiki/Dark-Mode");
            AppSettings.ThemeId = ThemeId.DefaultLight;
            return CreateFallbackSettings(invariantTheme, variations);
        }

        return new ThemeSettings(theme, invariantTheme, variations, systemVisualStyle);
    }

    private static ThemeSettings CreateFallbackSettings(Theme invariantTheme, string[] variations) =>
        new(Theme.CreateDefaultTheme(variations), invariantTheme, variations, useSystemVisualStyle: true);

    internal static class TestAccessor
    {
        public static void ReloadThemeSettings(IThemeRepository repository)
        {
            Settings = LoadThemeSettings(repository);
            ColorHelper.ThemeSettings = Settings;
        }
    }
}
