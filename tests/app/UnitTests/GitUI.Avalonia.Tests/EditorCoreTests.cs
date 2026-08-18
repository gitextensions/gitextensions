using System.Drawing;
using System.Text;
using Avalonia.Headless.NUnit;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Editor;
using GitUI.Editor.Diff;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class EditorCoreTests
{
    [Test]
    public void Ansi_escape_parser_should_resolve_true_color_and_remove_control_sequences()
    {
        StringBuilder output = new();
        List<TextMarker> markers = [];

        AnsiEscapeUtilities.ParseEscape("plain \u001b[38;2;1;2;3mcolor\u001b[0m text", output, markers);

        output.ToString().Should().Be("plain color text");
        markers.Should().ContainSingle();
        markers[0].Offset.Should().Be(6);
        markers[0].Length.Should().Be(5);
        markers[0].ForeColor.Should().Be(Color.FromArgb(1, 2, 3));
    }

    [Test]
    public void Ansi_escape_parser_should_resolve_256_color_palette_to_argb()
    {
        StringBuilder output = new();
        List<TextMarker> markers = [];

        AnsiEscapeUtilities.ParseEscape("\u001b[38;5;196mred\u001b[0m", output, markers);

        output.ToString().Should().Be("red");
        markers.Should().ContainSingle();
        markers[0].ForeColor.Should().Be(Color.FromArgb(255, 0, 0));
    }

    [Test]
    public void Ansi_palette_should_preserve_original_cube_gray_and_range_contracts()
    {
        AnsiEscapeUtilities.TestAccessor.Get8bitColor(196, fore: true, bold: false, dim: false)
            .Should().Be(Color.FromArgb(255, 0, 0));
        AnsiEscapeUtilities.TestAccessor.Get8bitColor(231, fore: true, bold: false, dim: false)
            .Should().Be(Color.FromArgb(255, 255, 255));
        AnsiEscapeUtilities.TestAccessor.Get8bitColor(232, fore: true, bold: false, dim: false)
            .Should().Be(Color.FromArgb(0, 0, 0));
        AnsiEscapeUtilities.TestAccessor.Get8bitColor(255, fore: true, bold: false, dim: false)
            .Should().Be(Color.FromArgb(253, 253, 253));
        ((Action)(() => AnsiEscapeUtilities.TestAccessor.Get8bitColor(
            256,
            fore: true,
            bold: false,
            dim: false))).Should().Throw<ArgumentOutOfRangeException>();
    }

    [AvaloniaTest]
    public void Grep_highlighter_should_strip_line_prefixes_and_preserve_semantic_lines()
    {
        TextEditor editor = new();
        DiffViewerLineNumberControl margin = new(editor);
        string text = "10:\u001b[31mmatch\u001b[0m\n11-context\n--\n12=header\n13:next\n";

        GrepHighlightService service = new(ref text, margin);

        text.Should().Be("match\ncontext\nheader\nnext\n");
        service.LinesInfo.DiffLines.Values.Should().Contain(line => line.RightLineNumber == 10 && line.LineType == DiffLineType.Grep);
        service.LinesInfo.DiffLines.Values.Should().Contain(line => line.RightLineNumber == 11 && line.LineType == DiffLineType.Context);
        service.LinesInfo.DiffLines.Values.Should().Contain(line => line.RightLineNumber == 12 && line.LineType == DiffLineType.Header);
        service.TextMarkers.Should().ContainSingle(marker => marker.ForeColor.HasValue);
        service.GetGrepLineNum(rowIndexInText: 1, next: true).Should().Be(3);
        service.GetGrepLineNum(rowIndexInText: 4, next: false).Should().Be(2);
    }

    [AvaloniaTest]
    public void Difftastic_highlighter_should_record_header_and_resolved_ansi_ranges()
    {
        TextEditor editor = new();
        DiffViewerLineNumberControl margin = new(editor);
        string text = "\u001b[38;2;4;5;6mfile.cs ---\u001b[0m\n";

        DifftasticHighlightService service = new(ref text, margin, out int rightColumnStart);

        text.Should().Be("file.cs ---\n");
        rightColumnStart.Should().Be(0);
        service.LinesInfo.DiffLines.Values.Should().ContainSingle(line => line.LineType == DiffLineType.Header);
        service.TextMarkers.Should().ContainSingle(marker => marker.ForeColor == Color.FromArgb(4, 5, 6));
    }

    [AvaloniaTest]
    public void Difftastic_highlighter_should_match_original_sample_line_contracts()
    {
        string sample = Path.Combine(TestContext.CurrentContext.TestDirectory, "Editor", "Diff", "SampleDifftastic.diff");
        string text = File.ReadAllText(sample);
        TextEditor editor = new();
        DiffViewerLineNumberControl margin = new(editor);
        EnvironmentAbstraction environment = new();
        string? oldWidth = environment.GetEnvironmentVariable("DFT_WIDTH");
        bool oldReverseGitColoring = AppSettings.ReverseGitColoring.Value;
        try
        {
            environment.SetEnvironmentVariable("DFT_WIDTH", "200");
            AppSettings.ReverseGitColoring.Value = false;

            DifftasticHighlightService service = new(ref text, margin, out int rightColumnStart);

            rightColumnStart.Should().Be(97);
            service.LinesInfo.DiffLines[5].Should().Match<DiffLineInfo>(line =>
                line.LeftLineNumber == DiffLineInfo.NotApplicableLineNum
                && line.RightLineNumber == 16
                && line.LineType == DiffLineType.PlusRight);
            service.LinesInfo.DiffLines[22].Should().Match<DiffLineInfo>(line =>
                line.LeftLineNumber == 51
                && line.RightLineNumber == DiffLineInfo.NotApplicableLineNum
                && line.LineType == DiffLineType.MinusLeft);
            service.LinesInfo.DiffLines[33].Should().Match<DiffLineInfo>(line =>
                line.LeftLineNumber == 109
                && line.RightLineNumber == 114
                && line.LineType == DiffLineType.MinusPlus);
        }
        finally
        {
            AppSettings.ReverseGitColoring.Value = oldReverseGitColoring;
            environment.SetEnvironmentVariable("DFT_WIDTH", oldWidth);
        }
    }

    [Test]
    public void Diff_segment_primitives_should_preserve_zero_based_line_contracts()
    {
        TextDocument document = new("first\nsecond");
        LineSegmentGetter getter = new();
        ISegment first = getter.GetSegment(document, lineNumber: 0);
        ISegment second = getter.GetSegment(document, lineNumber: 1);
        Segment segment = new() { Offset = 4, Length = 7 };
        TextMarker marker = new(offset: 4, length: 7, default);

        first.Offset.Should().Be(0);
        first.Length.Should().Be(5);
        second.Offset.Should().Be(6);
        second.Length.Should().Be(6);
        segment.EndOffset.Should().Be(11);
        marker.EndOffset.Should().Be(10);
    }

    [AvaloniaTest]
    public void Commit_message_highlighter_should_report_summary_spacing_and_description_limits()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GetEffectiveSetting("core.commentchar", "#").Returns("#");
        CommitMessageHighlightingStrategy strategy = new(module);
        TextDocument document = new(new string('s', 51) + "\n" + new string('d', 81));

        strategy.UpdateValidationMarkers(document);

        strategy.ValidationMarkers.Should().Contain(marker => marker.ToolTip == "Summary line is too long.");
        strategy.ValidationMarkers.Should().Contain(marker => marker.ToolTip == "There must be a blank line after the summary.");
        strategy.ValidationMarkers.Should().Contain(marker => marker.ToolTip == "Line is too long.");
    }

    [AvaloniaTest]
    public void FileViewerInternal_should_select_git_specific_highlighting_strategies()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GetEffectiveSetting("core.commentchar", "#").Returns("#");
        FileViewerInternal viewer = new();

        viewer.SetHighlightingForFile("COMMIT_EDITMSG", module);
        viewer.GetTestAccessor().HighlightingStrategy.Should().BeOfType<CommitMessageHighlightingStrategy>();

        viewer.SetHighlightingForFile("git-rebase-todo", module);
        viewer.GetTestAccessor().HighlightingStrategy.Should().BeOfType<RebaseTodoHighlightingStrategy>();

        viewer.SetHighlightingForFile("ordinary.cs", module);
        viewer.GetTestAccessor().HighlightingStrategy.Should().BeNull();
    }

    [AvaloniaTest]
    public void Git_grep_dialog_should_preserve_text_when_search_history_changes()
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(Substitute.For<IGitModule>());
        FormFindInCommitFilesGitGrep form = new(commands);
        string? located = null;
        form.FilesGitGrepLocator = value => located = value;
        form.GitGrepExpressionText = "needle";

        form.SetSearchItems(["first", "second"]);
        form.GetTestAccessor().Search();

        form.GitGrepExpressionText.Should().Be("needle");
        form.GetTestAccessor().SearchBox.Items.Count.Should().Be(2);
        located.Should().Be("needle");
    }
}
