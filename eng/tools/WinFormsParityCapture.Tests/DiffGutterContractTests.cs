using System.Drawing;
using AwesomeAssertions;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class DiffGutterContractTests
{
    [TestCase(false, 0)]
    [TestCase(true, 0)]
    [TestCase(false, 1)]
    [TestCase(true, 9)]
    [TestCase(false, 10)]
    [TestCase(true, 100)]
    public void Source_gutter_width_should_use_zero_digits_for_header_only_lines(bool showLeftColumn, int maximum)
    {
        using TextEditorControl editor = new() { Text = "header" };
        TextArea textArea = editor.ActiveTextAreaControl.TextArea;
        using Form host = new() { ClientSize = new Size(480, 200), ShowInTaskbar = false };
        editor.Dock = DockStyle.Fill;
        host.Controls.Add(editor);
        host.Show();
        Application.DoEvents();
        DiffViewerLineNumberControl margin = new(textArea);
        DiffLinesInfo lines = new();
        lines.Add(new DiffLineInfo
        {
            LineNumInDiff = 1,
            LeftLineNumber = maximum == 0 ? DiffLineInfo.NotApplicableLineNum : maximum,
            RightLineNumber = DiffLineInfo.NotApplicableLineNum,
            LineType = DiffLineType.Header,
        });
        margin.DisplayLineNum(lines, showLeftColumn);
        int digits = maximum > 0 ? (int)Math.Log10(maximum) + 1 : 0;
        textArea.TextView.WideSpaceWidth.Should().BeGreaterThan(0);
        margin.MaxLineNumber.Should().Be(maximum);
        margin.Width.Should().Be(4 + (textArea.TextView.WideSpaceWidth * (showLeftColumn ? 2 : 1) * (1 + digits)));
        margin.SetVisibility(false);
        margin.Width.Should().Be(0);
        margin.SetVisibility(true);
        margin.Clear();
        margin.Width.Should().Be(0);
        host.Close();
    }

    [Test]
    public void Source_disabled_gutter_should_fill_context_rows_with_raw_InactiveBorder()
    {
        using TextEditorControl editor = new() { Text = "context\ncontext" };
        TextArea textArea = editor.ActiveTextAreaControl.TextArea;
        using Form host = new() { ClientSize = new Size(480, 200), ShowInTaskbar = false };
        editor.Dock = DockStyle.Fill;
        host.Controls.Add(editor);
        host.Show();
        Application.DoEvents();
        DiffViewerLineNumberControl margin = new(textArea);
        DiffLinesInfo lines = new();
        for (int line = 1; line <= 2; line++)
        {
            lines.Add(new DiffLineInfo
            {
                LineNumInDiff = line,
                LeftLineNumber = line,
                RightLineNumber = line,
                LineType = DiffLineType.Context,
            });
        }

        margin.DisplayLineNum(lines, showLeftColumn: true);
        margin.DrawingPosition = new Rectangle(0, 0, margin.Width, textArea.TextView.FontHeight * 2);
        using Bitmap bitmap = new(margin.DrawingPosition.Width, margin.DrawingPosition.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);
        host.Enabled = false;
        textArea.Enabled.Should().BeFalse();
        margin.Paint(graphics, margin.DrawingPosition);
        bitmap.GetPixel(1, textArea.TextView.FontHeight / 2).ToArgb().Should().Be(SystemColors.InactiveBorder.ToArgb());
        bitmap.GetPixel(1, textArea.TextView.FontHeight + (textArea.TextView.FontHeight / 2)).ToArgb()
            .Should().Be(SystemColors.InactiveBorder.ToArgb());
        TestContext.Progress.WriteLine($"nativeInactiveBorder=#{SystemColors.InactiveBorder.ToArgb():X8} fontHeight={textArea.TextView.FontHeight} gutterWidth={margin.Width}");
        host.Close();
    }

    [Test]
    public void Source_margin_SelectedLineChanged_should_invalidate_only_new_marked_lines()
    {
        using TextEditorControl editor = new() { Text = "first\nsecond" };
        TextArea textArea = editor.ActiveTextAreaControl.TextArea;
        using Form host = new() { ClientSize = new Size(480, 200), ShowInTaskbar = false };
        editor.Dock = DockStyle.Fill;
        host.Controls.Add(editor);
        host.Show();
        Application.DoEvents();
        DiffViewerLineNumberControl margin = new(textArea);
        int invalidations = 0;
        textArea.Invalidated += (_, _) => invalidations++;
        margin.SelectedLineChanged(0);
        invalidations.Should().Be(1);
        margin.SelectedLineChanged(0);
        invalidations.Should().Be(1);
        margin.SelectedLineChanged(1);
        invalidations.Should().Be(2);
        margin.MarkSelectedLine = false;
        margin.SelectedLineChanged(0);
        invalidations.Should().Be(2);
        host.Close();
    }
}
