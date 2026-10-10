using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeSystemPaletteTests
{
    [AvaloniaTest]
    [TestCase(false, "GitExtensionsNativeTreeSearchBackgroundBrush", KnownColor.Info, "#FFFFE1")]
    [TestCase(false, "GitExtensionsNativeTreeSearchForegroundBrush", KnownColor.InfoText, "#000000")]
    [TestCase(false, "GitExtensionsNativeTreeDefaultBackgroundBrush", KnownColor.Window, "#FFFFFF")]
    [TestCase(false, "GitExtensionsNativeDisabledGutterBackgroundBrush", KnownColor.InactiveBorder, "#F4F7FC")]
    [TestCase(true, "GitExtensionsNativeTreeSearchBackgroundBrush", KnownColor.Info, "#50503C")]
    [TestCase(true, "GitExtensionsNativeTreeSearchForegroundBrush", KnownColor.InfoText, "#BEBEBE")]
    [TestCase(true, "GitExtensionsNativeTreeDefaultBackgroundBrush", KnownColor.Window, "#323232")]
    [TestCase(true, "GitExtensionsNativeDisabledGutterBackgroundBrush", KnownColor.InactiveBorder, "#3C3F41")]
    public void Raw_system_consumers_should_not_take_the_custom_css_override(bool dark, string key, KnownColor sourceColor, string expected)
    {
        Application application = Application.Current!;
        ThemeVariant variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        bool hadResources = application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? previous);
        ThemeSettings previousSettings = ThemeModule.Settings;
        ResourceDictionary resources = new();
        application.Resources.ThemeDictionaries[variant] = resources;
        Theme theme = new(
            new Dictionary<AppColor, DrawingColor> { [AppColor.PanelBackground] = dark ? DrawingColor.Black : DrawingColor.White },
            new Dictionary<KnownColor, DrawingColor>
            {
                [KnownColor.Info] = DrawingColor.Magenta,
                [KnownColor.InfoText] = DrawingColor.Cyan,
                [KnownColor.Window] = DrawingColor.Lime,
                [KnownColor.InactiveBorder] = DrawingColor.Red,
            },
            new ThemeId("native-system-consumers-custom"));
        try
        {
            AvaloniaThemeResources.Apply(application, new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: false));
            ((ISolidColorBrush)resources[key]!).Color.Should().Be(Color.Parse(expected));
            DrawingColor native = AvaloniaThemeResources.ResolveNativeSystemColor(dark, sourceColor);
            Color.FromArgb(native.A, native.R, native.G, native.B).Should().Be(Color.Parse(expected));
            ((ISolidColorBrush)resources["GitExtensionsWindowBackgroundBrush"]!).Color.Should().Be(Colors.Lime,
                "the raw source boundary does not disable configurable control colors");
            ((ISolidColorBrush)resources["GitExtensionsNativeToolStripSplitProfessionalSelectedBrush"]!).Color.Should()
                .Be(Color.Parse(dark ? "#2F4159" : "#B3D7F3"));
            ((ISolidColorBrush)resources["GitExtensionsNativeToolStripSplitProfessionalPressedBrush"]!).Color.Should()
                .Be(Color.Parse(dark ? "#2D4B73" : "#80BCEB"));
            ((ISolidColorBrush)resources["GitExtensionsNativeToolStripSplitProfessionalBorderBrush"]!).Color.Should()
                .Be(Color.Parse(dark ? "#2864B4" : "#0078D7"));
            ((ISolidColorBrush)resources["GitExtensionsNativeToolStripSplitProfessionalSplitterBrush"]!).Color.Should()
                .Be(dark ? Colors.Silver : Color.Parse("#0078D7"));
            LinearGradientBrush open = (LinearGradientBrush)resources["GitExtensionsNativeToolStripSplitProfessionalOpenBrush"]!;
            open.GradientStops.Select(stop => stop.Color).Should()
                .Equal(Color.Parse(dark ? "#2E2E2E" : "#FCFCFC"), Color.Parse(dark ? "#292929" : "#F8F8F8"));
            ((ISolidColorBrush)resources["GitExtensionsNativeToolStripSplitProfessionalOpenBorderBrush"]!).Color.Should()
                .Be(Color.Parse(dark ? "#6B6B6B" : "#808080"));
            resources["GitExtensionsNativeToolStripSplitUseSystemVisualStyle"].Should().Be(false);
        }
        finally
        {
            if (hadResources)
            {
                application.Resources.ThemeDictionaries[variant] = previous!;
            }
            else
            {
                application.Resources.ThemeDictionaries.Remove(variant);
            }

            ColorHelper.ThemeSettings = previousSettings;
        }
    }
}
