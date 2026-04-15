using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUITests.Editor.Diff;

public abstract class AnsiEscapeUtilitiesTestBase
{
    protected const int BlackId = 0;
    protected const int RedId = 1;
    protected const int YellowId = 3;

    // The expected ANSI theme colors for the red palette (DefaultLight theme, no variations).
    // Non bright and dim are theme colors.
    // The expected dimmed and bright ("high-intensity") are the expected calculated values.
    // Entries where the bright color matches the base are Color.Empty.
    private static readonly Color[,,,] _redAnsiThemeColors =
    {
        // bright=false
        {
            // back=false (fore)
            {
                /* bold=false */ { Color.FromArgb(211, 0, 11), Color.FromArgb(234, 188, 188) }, // dim=false, dim=true
                /* bold=true  */ { Color.FromArgb(255, 94, 94), Color.FromArgb(255, 197, 197) }, // dim=false, dim=true
            },
            // back=true
            {
                /* bold=false */ { Color.FromArgb(255, 200, 200), Color.FromArgb(255, 230, 230) }, // dim=false, dim=true
                /* bold=true  */ { Color.FromArgb(255, 165, 165), Color.FromArgb(255, 216, 216) }, // dim=false, dim=true
            },
        },
        // bright=true
        {
            // back=false (fore)
            {
                /* bold=false */ { Color.Empty, Color.Empty }, // dim=false, dim=true
                /* bold=true  */ { Color.FromArgb(255, 145, 145), Color.FromArgb(255, 197, 197) }, // dim=false, dim=true
            },
            // back=true
            {
                /* bold=false */ { Color.Empty, Color.Empty }, // dim=false, dim=true
                /* bold=true  */ { Color.FromArgb(255, 216, 216), Color.FromArgb(255, 237, 237) }, // dim=false, dim=true
            },
        },
    };

    /// <summary>
    ///  Returns the expected ANSI theme color for <paramref name="colorId"/> and the given attribute flags.
    /// </summary>
    /// <remarks>
    ///  Only <see cref="RedId"/> is currently supported; other values will cause the assertion to fail.
    /// </remarks>
    protected static Color GetAnsiColor(int colorId = RedId, bool fore = true, bool bold = false, bool dim = false, bool bright = false)
    {
        colorId.Should().Be(RedId, "only the red palette is defined in the test base");
        return _redAnsiThemeColors[bright ? 1 : 0, fore ? 0 : 1, bold ? 1 : 0, dim ? 1 : 0];
    }

    private ThemeId _themeId;
    private string[] _themeVariations = null!;

    [OneTimeSetUp]
    public void BaseOneTimeSetUp()
    {
        _themeId = AppSettings.ThemeId;
        _themeVariations = AppSettings.ThemeVariations;
        AppSettings.ThemeId = ThemeId.DefaultLight;
        AppSettings.ThemeVariations = ThemeVariations.None;
        ThemeModule.Load();
    }

    [OneTimeTearDown]
    public void BaseOneTimeTearDown()
    {
        AppSettings.ThemeId = _themeId;
        AppSettings.ThemeVariations = _themeVariations;
        ThemeModule.Load();
    }
}
