using Avalonia.Media;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Properties;
using GitUI.Theming;
using Color = Avalonia.Media.Color;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

internal sealed class DashboardTheme
{
    public static readonly DashboardTheme Light;
    public static readonly DashboardTheme Dark;

    static DashboardTheme()
    {
        // Palette URL: http://paletton.com/#uid=13I0u0k7UUa3cZA5wXlaiQ5cFL3
        Light = new DashboardTheme(searchBackColor: Color.FromRgb(248, 248, 255),
                                   startBackColor: Color.FromRgb(219, 235, 248),
                                   contributeBackColor: Color.FromRgb(230, 241, 250),
                                   headerBackColor: Color.FromRgb(172, 208, 239),
                                   logoBackColor: Color.FromRgb(19, 122, 212),
                                   primaryText: Color.FromRgb(30, 30, 30),
                                   secondaryText: Color.FromRgb(100, 127, 210),
                                   accentedText: Colors.DarkGoldenrod,
                                   primaryHeadingText: Color.FromRgb(24, 29, 35),
                                   secondaryHeadingText: Colors.DimGray,
                                   backgroundImage: Images.DashboardBackgroundBlue);

        // Avalonia resolves the original SystemColors through its matching cross-platform theme resources.
        Dark = new DashboardTheme(searchBackColor: ResolveSystemColor(KnownColor.Control),
                                  startBackColor: ResolveSystemColor(KnownColor.Control),
                                  contributeBackColor: ResolveSystemColor(KnownColor.ControlLight),
                                  headerBackColor: ResolveSystemColor(KnownColor.ControlDark),
                                  logoBackColor: ResolveSystemColor(KnownColor.ControlDarkDark),
                                  primaryText: ResolveSystemColor(KnownColor.WindowText),
                                  secondaryText: Colors.LightSkyBlue,
                                  accentedText: AvaloniaThemeResources.ToMediaColor(DrawingColor.Goldenrod.AdaptBackColor()),
                                  primaryHeadingText: ResolveSystemColor(KnownColor.ControlText),
                                  secondaryHeadingText: ResolveSystemColor(KnownColor.GrayText),
                                  backgroundImage: Images.DashboardBackgroundGrey);
    }

    private DashboardTheme(Color searchBackColor, Color startBackColor, Color contributeBackColor,
                             Color headerBackColor, Color logoBackColor,
                             Color primaryText, Color secondaryText, Color accentedText,
                             Color primaryHeadingText, Color secondaryHeadingText,
                             IImage backgroundImage)
    {
        SearchBackColor = searchBackColor;
        StartBackColor = startBackColor;
        ContributeBackColor = contributeBackColor;
        HeaderBackColor = headerBackColor;
        LogoBackColor = logoBackColor;
        PrimaryText = primaryText;
        SecondaryText = secondaryText;
        AccentedText = accentedText;
        PrimaryHeadingText = primaryHeadingText;
        SecondaryHeadingText = secondaryHeadingText;
        BackgroundImage = backgroundImage;
    }

    public Color AccentedText { get; }
    public IImage BackgroundImage { get; }
    public Color SearchBackColor { get; }
    public Color HeaderBackColor { get; }
    public Color PrimaryHeadingText { get; }
    public Color StartBackColor { get; }
    public Color PrimaryText { get; }
    public Color LogoBackColor { get; }
    public Color ContributeBackColor { get; }
    public Color SecondaryHeadingText { get; }
    public Color SecondaryText { get; }

    private static Color ResolveSystemColor(KnownColor color)
        => AvaloniaThemeResources.ToMediaColor(AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, color));
}
