using System.Text;
using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUITests.Editor.Diff;

[TestFixture]
public class AnsiEscapeUtilitiesTest_ParseEscape
{
    private const string _escape_sequence = "\u001b[";
    private readonly List<Color> _redAnsiTheme = [Color.FromArgb(211, 0, 11), Color.FromArgb(232, 127, 132), Color.FromArgb(255, 94, 94), Color.FromArgb(254, 174, 174),
        Color.FromArgb(255, 200, 200), Color.FromArgb(254, 227, 227), Color.FromArgb(255, 165, 165), Color.FromArgb(254, 209, 209)];

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
    public void ParseEscape_ShouldAppendTextWithoutEscapeSequence_WhenNoEscapeSequenceIsPresent()
    {
        string text = "No escape sequence  ";
        StringBuilder sb = new();
        List<TextMarker> textMarkers = [];

        AnsiEscapeUtilities.ParseEscape(text, sb, textMarkers);

        sb.ToString().Should().Be(text);
        textMarkers.Should().BeEmpty();
    }

    [Test]
    public void ParseEscape_ShouldAppendTextWithCorrectEscapeSequence_WhenEscapeSequenceIsPresent()
    {
        string in_text = $"""
            Text with {_escape_sequence}31mred{_escape_sequence}0m escape sequence
            extra line
            Some {_escape_sequence}0;1;31mbold red{_escape_sequence}0m
            Now {_escape_sequence}101mbold reverse red{_escape_sequence}0m
            and some {_escape_sequence}2:31mdim red{_escape_sequence}0m
            {_escape_sequence}31m{_escape_sequence}mdefault not red{_escape_sequence}0m{_escape_sequence}31m, red no end
              
            """.ReplaceLineEndings("\n");
        string expected = """
            Text with red escape sequence
            extra line
            Some bold red
            Now bold reverse red
            and some dim red
            default not red, red no end
              
            """.ReplaceLineEndings("\n");
        StringBuilder sb = new();
        List<TextMarker> textMarkers = [];

        AnsiEscapeUtilities.ParseEscape(in_text, sb, textMarkers);

        sb.ToString().Should().Be(expected);
        textMarkers.Should().HaveCount(5);

        textMarkers[0].Offset.Should().Be(10);
        textMarkers[0].Length.Should().Be(3);
        textMarkers[0].Color.Should().Be(SystemColors.Window);
        textMarkers[0].ForeColor.Should().Be(_redAnsiTheme[0]);

        textMarkers[1].Offset.Should().Be(46);
        textMarkers[1].Length.Should().Be(8);
        textMarkers[1].Color.Should().Be(SystemColors.Window);
        textMarkers[1].ForeColor.Should().Be(_redAnsiTheme[2]);

        textMarkers[2].Offset.Should().Be(59);
        textMarkers[2].Length.Should().Be(16);
        textMarkers[2].Color.Should().Be(_redAnsiTheme[6]);
        textMarkers[2].ForeColor.Should().Be(Color.FromArgb(0, 0, 0));

        textMarkers[3].Offset.Should().Be(85);
        textMarkers[3].Length.Should().Be(7);
        textMarkers[3].Color.Should().Be(SystemColors.Window);
        textMarkers[3].ForeColor.Should().Be(_redAnsiTheme[1]);

        textMarkers[4].Offset.Should().Be(108);
        textMarkers[4].Length.Should().Be(15);
        textMarkers[4].Color.Should().Be(SystemColors.Window);
        textMarkers[4].ForeColor.Should().Be(_redAnsiTheme[0]);
    }

    [Test]
    public void ParseEscape_ShouldHandleNonConformingEscapes()
    {
        string in_text = $"""
            Text with {_escape_sequence}38;5;196mred without end
            extra line
            then {_escape_sequence}0;48:5:46;31;1mreverse green,
            then {_escape_sequence}38;2;0;0;255mblue,
            {_escape_sequence}999mcode out of range,
            {_escape_sequence}99munused code,
            {_escape_sequence}30mblack to the end
            {_escape_sequence}incomplete escape1,
            {_escape_sequence}1xmincomplete escape2
            """.ReplaceLineEndings("\n");
        string expected_text = $"""
            Text with red without end
            extra line
            then reverse green,
            then blue,
            code out of range,
            unused code,
            black to the end
            {_escape_sequence}incomplete escape1,
            {_escape_sequence}1xmincomplete escape2
            """.ReplaceLineEndings("\n");
        StringBuilder sb = new();
        List<TextMarker> textMarkers = [];

        AnsiEscapeUtilities.ParseEscape(in_text, sb, textMarkers);

        sb.ToString().Should().Be(expected_text);
        textMarkers.Should().HaveCount(4);

        textMarkers[0].Offset.Should().Be(10);
        textMarkers[0].Length.Should().Be(32);
        textMarkers[0].Color.Should().Be(SystemColors.Window);
        textMarkers[0].ForeColor.Should().Be(Color.FromArgb(255, 255, 0, 0));

        textMarkers[1].Offset.Should().Be(42);
        textMarkers[1].Length.Should().Be(20);
        textMarkers[1].Color.Should().Be(Color.FromArgb(255, 0, 255, 0));
        textMarkers[1].ForeColor.Should().Be(Color.FromArgb(255, 255, 94, 94));

        textMarkers[2].Offset.Should().Be(62);
        textMarkers[2].Length.Should().Be(6);
        textMarkers[2].Color.Should().Be(SystemColors.Window);
        textMarkers[2].ForeColor.Should().Be(Color.FromArgb(255, 0, 0, 255));

        textMarkers[3].Offset.Should().Be(100);
        textMarkers[3].Length.Should().Be(62);
        textMarkers[3].Color.Should().Be(SystemColors.Window);
        textMarkers[3].ForeColor.Should().Be(Color.FromArgb(0x00, 0x00, 0x00));
    }

    [Test]
    public void ParseEscape_ShouldHandleAdjacentAndIllegalEscapes()
    {
        // Insert \r after setting the text newline to \n
        const string carriageReturn = "\r";
        const string carriageReplacer = "%%carriageReplacer%%";
        string in_text = $"""
            Text with {_escape_sequence}38;5;196mred over
            several lines
            {_escape_sequence}m single newline with sequence{_escape_sequence}31m
            {_escape_sequence}0;1;31m merge adjacent segments{_escape_sequence}m{_escape_sequence}0;31;1m continue{_escape_sequence}m{_escape_sequence}1;31m here{_escape_sequence}m
            {_escape_sequence}0;2;31m ignore empty{_escape_sequence}m{_escape_sequence}0;2;31m{_escape_sequence}m
            {_escape_sequence}0;1;31m ignore carriage return color {_escape_sequence}m{_escape_sequence}0;1;31m{carriageReplacer}{_escape_sequence}m
            {_escape_sequence}0;1;31m but not space{_escape_sequence}m{_escape_sequence}0;31;1m {_escape_sequence}m
            {_escape_sequence}0;31m merge ending with carriage return newline{_escape_sequence}m{carriageReplacer}
            {_escape_sequence}0;31m some text{_escape_sequence}m
            """.ReplaceLineEndings("\n").Replace(carriageReplacer, carriageReturn);
        string expected_text = $"""
            Text with red over
            several lines
             single newline with sequence
             merge adjacent segments continue here
             ignore empty
             ignore carriage return color {carriageReplacer}
             but not space 
             merge ending with carriage return newline{carriageReplacer}
             some text
            """.ReplaceLineEndings("\n").Replace(carriageReplacer, carriageReturn);
        StringBuilder sb = new();
        List<TextMarker> textMarkers = [];

        AnsiEscapeUtilities.ParseEscape(in_text, sb, textMarkers);

        sb.ToString().Should().Be(expected_text);
        textMarkers.Should().HaveCount(6);

        textMarkers[0].Offset.Should().Be(10);
        textMarkers[0].Length.Should().Be(23);
        textMarkers[0].Color.Should().Be(SystemColors.Window);
        textMarkers[0].ForeColor.Should().Be(Color.FromArgb(255, 255, 0, 0));

        textMarkers[1].Offset.Should().Be(62);
        textMarkers[1].Length.Should().Be(1);
        textMarkers[1].Color.Should().Be(SystemColors.Window);
        textMarkers[1].ForeColor.Should().Be(_redAnsiTheme[0]);

        textMarkers[2].Offset.Should().Be(63);
        textMarkers[2].Length.Should().Be(38);
        textMarkers[2].Color.Should().Be(SystemColors.Window);
        textMarkers[2].ForeColor.Should().Be(_redAnsiTheme[2]);

        textMarkers[3].Offset.Should().Be(102);
        textMarkers[3].Length.Should().Be(13);
        textMarkers[3].Color.Should().Be(SystemColors.Window);
        textMarkers[3].ForeColor.Should().Be(_redAnsiTheme[1]);

        textMarkers[4].Offset.Should().Be(116);
        textMarkers[4].Length.Should().Be(47);
        textMarkers[4].Color.Should().Be(SystemColors.Window);
        textMarkers[4].ForeColor.Should().Be(_redAnsiTheme[2]);

        textMarkers[5].Offset.Should().Be(164);
        textMarkers[5].Length.Should().Be(54);
        textMarkers[5].Color.Should().Be(SystemColors.Window);
        textMarkers[5].ForeColor.Should().Be(_redAnsiTheme[0]);
    }
}
