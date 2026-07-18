using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using GitUI.Editor;
using GitUI.Editor.Diff;

namespace GitExtensionsTests;

[TestFixture]
public sealed class DiffRenderingTests
{
    [Test]
    public void Patch_analyzer_should_map_replacement_insertion_and_multiple_hunks()
    {
        const string patch =
            "diff --git a/file.cs b/file.cs\n"
            + "@@ -10,3 +20,4 @@\n"
            + " context\n"
            + "-old value\n"
            + "+new value\n"
            + " tail\n"
            + "+inserted\n"
            + "\\ No newline at end of file\n"
            + "@@ -30,0 +40,2 @@\n"
            + "+first\n"
            + "+second\n"
            + "@@ -60,2 +70,0 @@\n"
            + "-obsolete one\n"
            + "-obsolete two\n";
        string text = patch;

        PatchHighlightService service = new(ref text, useGitColoring: false);
        DiffLineInfo[] lines = [.. service.LinesInfo.DiffLines.Values.OrderBy(line => line.LineNumInDiff)];

        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Minus && line.LeftLineNumber == 11);
        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Plus && line.RightLineNumber == 21);
        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Context && line.LeftLineNumber == 12 && line.RightLineNumber == 22);
        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Header && line.LineNumInDiff == 8);
        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Plus && line.RightLineNumber == 40);
        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Plus && line.RightLineNumber == 41);
        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Minus && line.LeftLineNumber == 60);
        lines.Should().ContainSingle(line => line.LineType == DiffLineType.Minus && line.LeftLineNumber == 61);
        service.LinesInfo.MaxLineNumber.Should().Be(61);
    }

    [Test]
    public void Patch_highlighter_should_dim_common_spans_inside_a_replacement()
    {
        string text = "@@ -1 +1 @@\n-return alpha + beta;\n+return alpha + gamma;\n";

        PatchHighlightService service = new(ref text, useGitColoring: false);

        service.InlineMarkers.Should().Contain(marker => marker.IsRemoved && marker.Length >= "return alpha + ".Length);
        service.InlineMarkers.Should().Contain(marker => !marker.IsRemoved && marker.Length >= "return alpha + ".Length);
        service.InlineMarkers.Should().Contain(marker => marker.IsRemoved && marker.Length >= 1);
        service.InlineMarkers.Should().Contain(marker => !marker.IsRemoved && marker.Length >= 1);
    }

    [Test]
    public void Patch_highlighter_should_keep_an_anchor_for_an_inline_insertion()
    {
        string text = "@@ -1 +1 @@\n-abc\n+abXc\n";

        PatchHighlightService service = new(ref text, useGitColoring: false);

        service.InlineMarkers.Should().ContainSingle(marker => marker.IsAnchor && !marker.IsRemoved);
    }

    [Test]
    public void Git_word_diff_should_strip_ANSI_and_map_both_sides()
    {
        string text = "@@ -3 +7 @@\n\u001b[31mold\u001b[0m value \u001b[32mnew\u001b[0m\n";

        PatchHighlightService service = new(ref text, useGitColoring: true, isGitWordDiff: true);

        text.Should().Be("@@ -3 +7 @@\nold value new\n");
        DiffLineInfo wordLine = service.LinesInfo.DiffLines.Values.Single(line => line.LineType == DiffLineType.MinusPlus);
        wordLine.LeftLineNumber.Should().Be(3);
        wordLine.RightLineNumber.Should().Be(7);
        service.TextMarkers.Should().Contain(marker => marker.Kind == DiffMarkerKind.Removed);
        service.TextMarkers.Should().Contain(marker => marker.Kind == DiffMarkerKind.Added);
    }

    [Test]
    public void Combined_diff_should_only_number_the_result_side()
    {
        string text = "@@@ -1,1 -1,1 +4,2 @@@\n--removed\n++added\n  context\n";

        CombinedDiffHighlightService service = new(ref text, useGitColoring: false);

        service.LinesInfo.DiffLines.Values.Should().ContainSingle(
            line => line.LineType == DiffLineType.Minus && line.RightLineNumber == DiffLineInfo.NotApplicableLineNum);
        service.LinesInfo.DiffLines.Values.Should().ContainSingle(
            line => line.LineType == DiffLineType.Plus && line.RightLineNumber == 4);
        service.LinesInfo.DiffLines.Values.Should().ContainSingle(
            line => line.LineType == DiffLineType.Context && line.RightLineNumber == 5);
    }

    [AvaloniaTest]
    public void FileViewer_should_navigate_by_the_right_then_left_file_line()
    {
        FileViewer viewer = new();
        viewer.ViewPatch("@@ -10,2 +20,2 @@\n-old\n+new\n context\n");

        viewer.GoToLine(20);
        viewer.CurrentFileLine.Should().Be(20);

        viewer.GoToLine(10);
        viewer.CurrentFileLine.Should().Be(10);
    }

    [AvaloniaTest]
    public void Diff_margin_should_match_WinForms_two_column_geometry_and_monospace_markers()
    {
        FileViewer viewer = new();
        viewer.ViewPatch("@@ -100 +100 @@\n-old\n+new\n");
        Window window = new()
        {
            Width = 480,
            Height = 240,
            Content = viewer,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TextEditor editor = viewer.TextEditor;
            DiffViewerLineNumberControl margin = editor.TextArea.LeftMargins
                .OfType<DiffViewerLineNumberControl>()
                .Single();
            FormattedText digit = CreateFormattedText(editor, "0");
            FormattedText plus = CreateFormattedText(editor, "+");
            FormattedText minus = CreateFormattedText(editor, "-");
            (double backgroundSplit, double rightNumberX) =
                DiffViewerLineNumberControl.GetTwoColumnGeometry(margin.Bounds.Width);

            margin.Bounds.Width.Should().Be(Math.Ceiling(4 + (2 * digit.Width * 4)));
            backgroundSplit.Should().Be(margin.Bounds.Width / 2);
            rightNumberX.Should().Be(backgroundSplit + 2);
            plus.Width.Should().BeApproximately(minus.Width, 0.01);
            plus.Width.Should().BeApproximately(digit.Width, 0.01);
        }
        finally
        {
            window.Close();
        }
    }

    private static FormattedText CreateFormattedText(TextEditor editor, string text)
        => new(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(editor.FontFamily, editor.FontStyle, editor.FontWeight),
            editor.FontSize,
            Brushes.Black);
}
