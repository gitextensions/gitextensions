using GitExtUtils.GitUI.Theming;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

internal sealed class DashboardTheme
{
    public static readonly DashboardTheme Light;
    public static readonly DashboardTheme Dark;

    static DashboardTheme()
    {
        // Palette URL: http://paletton.com/#uid=13I0u0k7UUa3cZA5wXlaiQ5cFL3
        Light = new DashboardTheme(searchBackColor: Color.FromArgb(248, 248, 255),
                                   startBackColor: Color.FromArgb(219, 235, 248),
                                   contributeBackColor: Color.FromArgb(230, 241, 250),
                                   headerBackColor: Color.FromArgb(172, 208, 239),
                                   logoBackColor: Color.FromArgb(19, 122, 212),
                                   primaryText: Color.FromArgb(30, 30, 30),
                                   secondaryText: Color.FromArgb(100, 127, 210),
                                   accentedText: Color.DarkGoldenrod,
                                   primaryHeadingText: Color.FromArgb(24, 29, 35),
                                   secondaryHeadingText: Color.DimGray,
                                   backgroundImage: Images.DashboardBackgroundBlue);

        Dark = new DashboardTheme(searchBackColor: SystemColors.Control,
                                  startBackColor: SystemColors.Control,
                                  contributeBackColor: SystemColors.ControlLight,
                                  headerBackColor: SystemColors.ControlDark,
                                  logoBackColor: SystemColors.ControlDarkDark,
                                  primaryText: SystemColors.WindowText,
                                  secondaryText: Color.LightSkyBlue,
                                  accentedText: Color.Goldenrod.AdaptBackColor(),
                                  primaryHeadingText: SystemColors.ControlText,
                                  secondaryHeadingText: SystemColors.GrayText,
                                  backgroundImage: Images.DashboardBackgroundGrey);
    }

    private DashboardTheme(Color searchBackColor, Color startBackColor, Color contributeBackColor,
                             Color headerBackColor, Color logoBackColor,
                             Color primaryText, Color secondaryText, Color accentedText,
                             Color primaryHeadingText, Color secondaryHeadingText,
                             Image backgroundImage)
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
    public Image BackgroundImage { get; }
    public Color SearchBackColor { get; }
    public Color HeaderBackColor { get; }
    public Color PrimaryHeadingText { get; }
    public Color StartBackColor { get; }
    public Color PrimaryText { get; }
    public Color LogoBackColor { get; }
    public Color ContributeBackColor { get; }
    public Color SecondaryHeadingText { get; }
    public Color SecondaryText { get; }
}
