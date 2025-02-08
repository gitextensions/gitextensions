using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
using GitUI.Theming;

namespace GitUITests.Editor.Diff;

[TestFixture]
public class AnsiEscapeUtilitiesTest_TryGetColors
{
    private const int _blackId = 0;
    private readonly Color _textColor = Color.FromArgb(0, 0, 0);
    private const int _redId = 1;
    private const int _yellowId = 3;
    private readonly List<Color> _redAnsiTheme = [
        Color.FromArgb(211, 0, 11), Color.FromArgb(232, 127, 132), Color.FromArgb(255, 94, 94), Color.FromArgb(254, 174, 174),
        Color.FromArgb(255, 200, 200), Color.FromArgb(254, 227, 227), Color.FromArgb(255, 165, 165), Color.FromArgb(254, 209, 209),
        Color.Empty, Color.Empty, Color.FromArgb(255, 145, 145), Color.FromArgb(254, 174, 174),
        Color.Empty, Color.Empty, Color.FromArgb(255, 216, 216), Color.FromArgb(254, 235, 235)];

    private ThemeId _themeId;
    private string[] _themeVariations;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _redAnsiTheme[8] = _redAnsiTheme[2];
        _redAnsiTheme[9] = _redAnsiTheme[3];
        _redAnsiTheme[12] = _redAnsiTheme[6];
        _redAnsiTheme[13] = _redAnsiTheme[7];
        _themeId = AppSettings.ThemeId;
        _themeVariations = AppSettings.ThemeVariations;
        AppSettings.ThemeId = ThemeId.Default;
        AppSettings.ThemeVariations = ThemeVariations.None;
        ThemeModule.Load();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        AppSettings.ThemeId = _themeId;
        AppSettings.ThemeVariations = _themeVariations;
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldReset_WhenEscapeCodesIsEmpty()
    {
        // currentColorId should be preserved
        List<int> escapeCodes = [];
        int currentColorId = _yellowId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeFalse();
        backColor.Should().BeNull();
        foreColor.Should().BeNull();
        currentColorId.Should().Be(_blackId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBackColorAndForeColorToNull_WhenEscapeCodeIs0()
    {
        // currentColorId should be reset
        List<int> escapeCodes = [0];
        int currentColorId = _yellowId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeFalse();
        backColor.Should().BeNull();
        foreColor.Should().BeNull();
        currentColorId.Should().Be(_blackId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldAndForeColor_WhenEscapeCodeIs1()
    {
        // currentColorId should be unchanged, just bold
        List<int> escapeCodes = [1];
        int currentColorId = _redId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[2]);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldAndForeColor_WhenEscapeCodeIsx1()
    {
        // currentColorId should be unchanged, just bold
        List<int> escapeCodes = [1];
        int currentColorId = _redId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[2]);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBold()
    {
        // Some special cases, not fully specified
        int currentColorId = _yellowId;

        List<int> escapeCodes = [0, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[8]);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldBold()
    {
        // Some special cases, not fully specified
        int currentColorId = _yellowId;

        List<int> escapeCodes = [1, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[10]);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldDim()
    {
        // Some special cases, not fully specified
        int currentColorId = _yellowId;

        List<int> escapeCodes = [1, 2, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[11]);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldDim2()
    {
        // Some special cases, not fully specified
        int currentColorId = _yellowId;

        List<int> escapeCodes = [2, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[9]);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBackBoldBold()
    {
        // Some special cases, not fully specified
        int currentColorId = _yellowId;

        List<int> escapeCodes = [1, 101];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().Be(_redAnsiTheme[14]);
        currentColorId.Should().Be(_yellowId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBackDimBold()
    {
        // Some special cases, not fully specified
        int currentColorId = _yellowId;

        List<int> escapeCodes = [1, 2, 101];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().Be(_redAnsiTheme[15]);
        currentColorId.Should().Be(_yellowId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldKeepThemeDim()
    {
        // Some special cases, not fully specified
        int currentColorId = _yellowId;

        List<int> escapeCodes = [1, 2, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[3]);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetThemeBold()
    {
        // Adjusting difftastic colors
        int currentColorId = _yellowId;

        List<int> escapeCodes = [1, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().Be(_redAnsiTheme[4]);
        foreColor.Should().Be(_textColor);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetThemeBold2()
    {
        // Adjusting difftastic colors
        int currentColorId = _yellowId;

        List<int> escapeCodes = [1, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().Be(_redAnsiTheme[12]);
        foreColor.Should().Be(_textColor);
        currentColorId.Should().Be(_redId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetThemeNormal()
    {
        // Adjusting difftastic colors
        int currentColorId = _yellowId;

        List<int> escapeCodes = [0, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().Be(_redAnsiTheme[5]);
        foreColor.Should().Be(_textColor);
        currentColorId.Should().Be(_redId);
    }
}
