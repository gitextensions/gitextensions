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

    private readonly DrawingColor _searchBackColor;
    private readonly DrawingColor _startBackColor;
    private readonly DrawingColor _contributeBackColor;
    private readonly DrawingColor _headerBackColor;
    private readonly DrawingColor _logoBackColor;
    private readonly DrawingColor _primaryText;
    private readonly DrawingColor _secondaryText;
    private readonly DrawingColor _accentedText;
    private readonly DrawingColor _primaryHeadingText;
    private readonly DrawingColor _secondaryHeadingText;

    static DashboardTheme()
    {
        // Palette URL: http://paletton.com/#uid=13I0u0k7UUa3cZA5wXlaiQ5cFL3
        Light = new DashboardTheme(searchBackColor: DrawingColor.FromArgb(248, 248, 255),
                                   startBackColor: DrawingColor.FromArgb(219, 235, 248),
                                   contributeBackColor: DrawingColor.FromArgb(230, 241, 250),
                                   headerBackColor: DrawingColor.FromArgb(172, 208, 239),
                                   logoBackColor: DrawingColor.FromArgb(19, 122, 212),
                                   primaryText: DrawingColor.FromArgb(30, 30, 30),
                                   secondaryText: DrawingColor.FromArgb(100, 127, 210),
                                   accentedText: DrawingColor.DarkGoldenrod,
                                   primaryHeadingText: DrawingColor.FromArgb(24, 29, 35),
                                   secondaryHeadingText: DrawingColor.DimGray,
                                   backgroundImage: Images.DashboardBackgroundBlue);

        // Avalonia resolves the original SystemColors through its matching cross-platform theme resources.
        Dark = new DashboardTheme(searchBackColor: DrawingColor.FromKnownColor(KnownColor.Control),
                                  startBackColor: DrawingColor.FromKnownColor(KnownColor.Control),
                                  contributeBackColor: DrawingColor.FromKnownColor(KnownColor.ControlLight),
                                  headerBackColor: DrawingColor.FromKnownColor(KnownColor.ControlDark),
                                  logoBackColor: DrawingColor.FromKnownColor(KnownColor.ControlDarkDark),
                                  primaryText: DrawingColor.FromKnownColor(KnownColor.WindowText),
                                  secondaryText: DrawingColor.LightSkyBlue,
                                  accentedText: DrawingColor.Goldenrod.AdaptBackColor(),
                                  primaryHeadingText: DrawingColor.FromKnownColor(KnownColor.ControlText),
                                  secondaryHeadingText: DrawingColor.FromKnownColor(KnownColor.GrayText),
                                  backgroundImage: Images.DashboardBackgroundGrey);
    }

    private DashboardTheme(DrawingColor searchBackColor, DrawingColor startBackColor, DrawingColor contributeBackColor,
                             DrawingColor headerBackColor, DrawingColor logoBackColor,
                             DrawingColor primaryText, DrawingColor secondaryText, DrawingColor accentedText,
                             DrawingColor primaryHeadingText, DrawingColor secondaryHeadingText,
                             IImage backgroundImage)
    {
        _searchBackColor = searchBackColor;
        _startBackColor = startBackColor;
        _contributeBackColor = contributeBackColor;
        _headerBackColor = headerBackColor;
        _logoBackColor = logoBackColor;
        _primaryText = primaryText;
        _secondaryText = secondaryText;
        _accentedText = accentedText;
        _primaryHeadingText = primaryHeadingText;
        _secondaryHeadingText = secondaryHeadingText;
        BackgroundImage = backgroundImage;
    }

    public Color AccentedText => Resolve(_accentedText);
    public IImage BackgroundImage { get; }
    public Color SearchBackColor => Resolve(_searchBackColor);
    public Color HeaderBackColor => Resolve(_headerBackColor);
    public Color PrimaryHeadingText => Resolve(_primaryHeadingText);
    public Color StartBackColor => Resolve(_startBackColor);
    public Color PrimaryText => Resolve(_primaryText);
    public Color LogoBackColor => Resolve(_logoBackColor);
    public Color ContributeBackColor => Resolve(_contributeBackColor);
    public Color SecondaryHeadingText => Resolve(_secondaryHeadingText);
    public Color SecondaryText => Resolve(_secondaryText);

    // WinForms Color retains KnownColor identity until painting. Avalonia Color snapshots ARGB,
    // so resolve the retained source identity at access time rather than caching another theme's palette.
    private static Color Resolve(DrawingColor color)
        => AvaloniaThemeResources.ToMediaColor(color.IsSystemColor
            ? AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, color.ToKnownColor())
            : color);
}
