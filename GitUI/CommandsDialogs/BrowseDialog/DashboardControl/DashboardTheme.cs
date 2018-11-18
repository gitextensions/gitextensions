using System.Drawing;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    internal sealed class DashboardTheme
    {
        public static readonly DashboardTheme Light;
        public static readonly DashboardTheme Dark;

        static DashboardTheme()
        {
            // Palette URL: http://paletton.com/#uid=13I0u0k7UUa3cZA5wXlaiQ5cFL3
            Light = new DashboardTheme(primary: Color.FromArgb(248, 248, 255), // 238, 243, 253), // Color.FromArgb(184, 203, 237),
                                       primaryLight: Color.FromArgb(219, 235, 248),
                                       primaryVeryLight: Color.FromArgb(230, 241, 250),
                                       primaryDark: Color.FromArgb(172, 208, 239),
                                       primaryVeryDark: Color.FromArgb(19, 122, 212),
                                       primaryText: Color.FromArgb(30, 30, 30),
                                       secondaryText: Color.FromArgb(100, 127, 210),
                                       accentedText: Color.DarkGoldenrod,
                                       primaryHeadingText: Color.FromArgb(24, 29, 35),
                                       secondaryHeadingText: Color.DimGray,
                                       backgroundImage: Images.DashboardBackgroundBlue);

            // Palette URL: http://paletton.com/#uid=13I0u0k3V4WaYgf7Lb1ac80gJaQ
            Dark = new DashboardTheme(primary: Color.FromArgb(23, 24, 26),
                                      primaryLight: Color.FromArgb(46, 50, 58),
                                      primaryVeryLight: Color.FromArgb(59, 69, 86),
                                      primaryDark: Color.FromArgb(30, 34, 42),
                                      primaryVeryDark: Color.FromArgb(30, 40, 57),
                                      primaryText: Color.Silver,
                                      secondaryText: Color.LightSkyBlue,
                                      accentedText: Color.Goldenrod,
                                      primaryHeadingText: Color.White,
                                      secondaryHeadingText: Color.Gray,
                                      backgroundImage: Images.DashboardBackgroundGrey);
        }

        private DashboardTheme(Color primary, Color primaryLight, Color primaryVeryLight,
                                 Color primaryDark, Color primaryVeryDark,
                                 Color primaryText, Color secondaryText, Color accentedText,
                                 Color primaryHeadingText, Color secondaryHeadingText,
                                 Image backgroundImage)
        {
            Primary = primary;
            PrimaryLight = primaryLight;
            PrimaryVeryLight = primaryVeryLight;
            PrimaryDark = primaryDark;
            PrimaryVeryDark = primaryVeryDark;
            PrimaryText = primaryText;
            SecondaryText = secondaryText;
            AccentedText = accentedText;
            PrimaryHeadingText = primaryHeadingText;
            SecondaryHeadingText = secondaryHeadingText;
            BackgroundImage = backgroundImage;
        }

        public Color AccentedText { get; }
        public Image BackgroundImage { get; }
        public Color Primary { get; }
        public Color PrimaryDark { get; }
        public Color PrimaryHeadingText { get; }
        public Color PrimaryLight { get; }
        public Color PrimaryText { get; }
        public Color PrimaryVeryDark { get; }
        public Color PrimaryVeryLight { get; }
        public Color SecondaryHeadingText { get; }
        public Color SecondaryText { get; }
    }
}