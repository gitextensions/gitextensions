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
    private const int _redId = 1;
    private const int _yellowId = 3;
    private readonly List<Color> _redAnsiTheme = [Color.FromArgb(212, 44, 58), Color.FromArgb(233, 149, 156), Color.FromArgb(255, 118, 118), Color.FromArgb(254, 186, 186),
        Color.FromArgb(212, 44, 58), Color.FromArgb(233, 149, 156), Color.FromArgb(255, 118, 118), Color.FromArgb(254, 186, 186)];

    private ThemeId _themeId;
    private string[] _themeVariations;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
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
    public void TryGetColorsFromEscapeSequence_ShouldReturnFalse_WhenEscapeCodesIsEmpty()
    {
        // currentColorId should be preserved
        IList<int> escapeCodes = new List<int>();
        int currentColorId = _yellowId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeFalse();
        backColor.Should().BeNull();
        foreColor.Should().BeNull();
        currentColorId.Should().Be(_yellowId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBackColorAndForeColorToNull_WhenEscapeCodeIs0()
    {
        // currentColorId should be reset
        IList<int> escapeCodes = new List<int> { 0 };
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
        IList<int> escapeCodes = new List<int> { 1 };
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
        IList<int> escapeCodes = new List<int> { 1 };
        int currentColorId = _redId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(_redAnsiTheme[2]);
        currentColorId.Should().Be(_redId);
    }
}
