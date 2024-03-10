using System.Text;
using FluentAssertions;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor.Document;

namespace GitUITests.Editor.Diff;

[TestFixture]
public class AnsiEscapeUtilitiesTest_ParseEscape
{
    private string _escape_sequence = "\u001b[";
    private readonly Color _normalRedAnsiTheme = Color.FromArgb(212, 44, 58);
    private readonly Color _boldRedAnsiTheme = Color.FromArgb(255, 118, 118);
    private readonly Color _dimRedAnsiTheme = Color.FromArgb(208, 142, 147);

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
            and some {_escape_sequence}mdefault{_escape_sequence}0m
              
            """.ReplaceLineEndings("\n");
        string expected = """
            Text with red escape sequence
            extra line
            Some bold red
            Now bold reverse red
            and some dim red
            and some default
              
            """.ReplaceLineEndings("\n");
        StringBuilder sb = new();
        List<TextMarker> textMarkers = [];

        AnsiEscapeUtilities.ParseEscape(in_text, sb, textMarkers);

        // sb.ToString().Should().Be("Text with red escape sequence\nextra line\nSome bold red\nNow bold reverse red\nand some dim red\nand some default\n  \n  ");
        sb.ToString().Should().Be(expected);
        textMarkers.Should().HaveCount(4);

        textMarkers[0].Offset.Should().Be(10);
        textMarkers[0].Length.Should().Be(3);
        textMarkers[0].Color.Should().Be(Color.White);
        textMarkers[0].ForeColor.Should().Be(_normalRedAnsiTheme);

        textMarkers[1].Offset.Should().Be(46);
        textMarkers[1].Length.Should().Be(8);
        textMarkers[1].Color.Should().Be(Color.White);
        textMarkers[1].ForeColor.Should().Be(_boldRedAnsiTheme);

        textMarkers[2].Offset.Should().Be(59);
        textMarkers[2].Length.Should().Be(16);
        textMarkers[2].Color.Should().Be(_boldRedAnsiTheme);
        textMarkers[2].ForeColor.ToArgb().Should().Be(Color.Black.ToArgb());

        textMarkers[3].Offset.Should().Be(85);
        textMarkers[3].Length.Should().Be(7);
        textMarkers[3].Color.Should().Be(Color.White);
        textMarkers[3].ForeColor.Should().Be(_dimRedAnsiTheme);
    }

    [Test]
    public void ParseEscape_ShouldHandleNonConformingEscapes()
    {
        string in_text = $"""
            Text with {_escape_sequence}38;5;196mred without end
            extra line
            then {_escape_sequence}0;48:5:46:1;31mreverse green,
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
        textMarkers[0].Color.Should().Be(Color.White);
        textMarkers[0].ForeColor.Should().Be(Color.FromArgb(255, 255, 0, 0));

        textMarkers[1].Offset.Should().Be(42);
        textMarkers[1].Length.Should().Be(20);
        textMarkers[1].Color.Should().Be(Color.FromArgb(255, 0, 255, 0));
        textMarkers[1].ForeColor.Should().Be(Color.FromArgb(255, 255, 118, 118)); // selected from backcolor

        textMarkers[2].Offset.Should().Be(62);
        textMarkers[2].Length.Should().Be(6);
        textMarkers[2].Color.Should().Be(Color.White);
        textMarkers[2].ForeColor.Should().Be(Color.FromArgb(255, 0, 0, 255));

        textMarkers[3].Offset.Should().Be(100);
        textMarkers[3].Length.Should().Be(62);
        textMarkers[3].Color.Should().Be(Color.White);
        textMarkers[3].ForeColor.ToArgb().Should().Be(Color.Black.ToArgb());
    }
}
