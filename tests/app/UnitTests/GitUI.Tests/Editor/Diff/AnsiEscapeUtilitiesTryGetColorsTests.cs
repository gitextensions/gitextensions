using GitUI.Editor.Diff;

namespace GitUITests.Editor.Diff;
public class AnsiEscapeUtilitiesTryGetColorsTests : AnsiEscapeUtilitiesTestBase
{
    private readonly Color _textColor = Color.FromArgb(0, 0, 0);

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldReset_WhenEscapeCodesIsEmpty()
    {
        // currentColorId should be preserved
        List<int> escapeCodes = [];
        int currentColorId = YellowId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeFalse();
        backColor.Should().BeNull();
        foreColor.Should().BeNull();
        currentColorId.Should().Be(BlackId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBackColorAndForeColorToNull_WhenEscapeCodeIs0()
    {
        // currentColorId should be reset
        List<int> escapeCodes = [0];
        int currentColorId = YellowId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeFalse();
        backColor.Should().BeNull();
        foreColor.Should().BeNull();
        currentColorId.Should().Be(BlackId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldAndForeColor_WhenEscapeCodeIs1()
    {
        // currentColorId should be unchanged, just bold
        List<int> escapeCodes = [1];
        int currentColorId = RedId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(GetAnsiColor(bold: true));
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldAndForeColor_WhenEscapeCodeIsx1()
    {
        // currentColorId should be unchanged, just bold
        List<int> escapeCodes = [1];
        int currentColorId = RedId;

        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(GetAnsiColor(bold: true));
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBold()
    {
        // Some special cases, not fully specified
        int currentColorId = YellowId;

        List<int> escapeCodes = [0, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(GetAnsiColor(bold: true));
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldBold()
    {
        // Some special cases, not fully specified
        int currentColorId = YellowId;

        List<int> escapeCodes = [1, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(GetAnsiColor(bold: true, bright: true));
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldDim()
    {
        // Some special cases, not fully specified
        int currentColorId = YellowId;

        List<int> escapeCodes = [1, 2, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(GetAnsiColor(bold: true, dim: true, bright: true));
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBoldDim2()
    {
        // Some special cases, not fully specified
        int currentColorId = YellowId;

        List<int> escapeCodes = [2, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(GetAnsiColor(bold: true, dim: true));
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBackBoldBold()
    {
        // Some special cases, not fully specified
        int currentColorId = YellowId;

        List<int> escapeCodes = [1, 101];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().Be(GetAnsiColor(fore: false, bold: true, bright: true));
        currentColorId.Should().Be(YellowId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetBackDimBold()
    {
        // Some special cases, not fully specified
        int currentColorId = YellowId;

        List<int> escapeCodes = [1, 2, 101];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId);

        result.Should().BeTrue();
        backColor.Should().Be(GetAnsiColor(fore: false, bold: true, dim: true, bright: true));
        currentColorId.Should().Be(YellowId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldKeepThemeDim()
    {
        // Some special cases, not fully specified
        int currentColorId = YellowId;

        List<int> escapeCodes = [1, 2, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().BeNull();
        foreColor.Should().Be(GetAnsiColor(bold: true, dim: true));
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetThemeBold()
    {
        // Adjusting difftastic colors
        int currentColorId = YellowId;

        List<int> escapeCodes = [1, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().Be(GetAnsiColor(fore: false));
        foreColor.Should().Be(_textColor);
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetThemeBold2()
    {
        // Adjusting difftastic colors
        int currentColorId = YellowId;

        List<int> escapeCodes = [1, 91];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().Be(GetAnsiColor(fore: false, bold: true));
        foreColor.Should().Be(_textColor);
        currentColorId.Should().Be(RedId);
    }

    [Test]
    public void TryGetColorsFromEscapeSequence_ShouldSetThemeNormal()
    {
        // Adjusting difftastic colors
        int currentColorId = YellowId;

        List<int> escapeCodes = [0, 31];
        bool result = AnsiEscapeUtilities.TestAccessor.TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: true);

        result.Should().BeTrue();
        backColor.Should().Be(GetAnsiColor(fore: false, dim: true));
        foreColor.Should().Be(_textColor);
        currentColorId.Should().Be(RedId);
    }
}
