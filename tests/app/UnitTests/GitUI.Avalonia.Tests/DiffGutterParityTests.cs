using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using GitExtUtils;
using GitUI;
using GitUI.Editor;
using GitUI.Editor.Diff;
using Microsoft.VisualStudio.Threading;
using SkiaSharp;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class DiffGutterParityTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(false, 0)]
    [TestCase(true, 0)]
    [TestCase(false, 1)]
    [TestCase(true, 9)]
    [TestCase(false, 10)]
    [TestCase(true, 100)]
    public void Diff_gutter_width_should_reserve_only_a_trailing_space_when_no_line_has_a_number(bool showLeftColumn, int maximum)
    {
        TextEditor editor = new() { FontFamily = FontFamily.Default, FontSize = 12 };
        DiffViewerLineNumberControl margin = new(editor);
        DiffLinesInfo lines = new();
        lines.Add(new DiffLineInfo
        {
            LineNumInDiff = 1,
            LeftLineNumber = maximum == 0 ? DiffLineInfo.NotApplicableLineNum : maximum,
            RightLineNumber = DiffLineInfo.NotApplicableLineNum,
            LineType = DiffLineType.Header,
        });
        margin.DisplayLineNum(lines, showLeftColumn);
        margin.Measure(Size.Infinity);

        int digits = maximum > 0 ? (int)Math.Log10(maximum) + 1 : 0;
        int wideSpaceWidth = Math.Max(1, Math.Max(MeasureGlyph(editor, " "), MeasureGlyph(editor, "x")));
        margin.MaxLineNumber.Should().Be(maximum);
        margin.DesiredSize.Width.Should().Be(4 + ((showLeftColumn ? 2 : 1) * wideSpaceWidth * (digits + 1)));
        margin.SetVisibility(false);
        margin.Measure(Size.Infinity);
        margin.DesiredSize.Width.Should().Be(0);
        margin.SetVisibility(true);
        margin.Clear();
        margin.Measure(Size.Infinity);
        margin.DesiredSize.Width.Should().Be(0);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Moving_the_caret_without_reflow_should_repaint_the_gutter_unless_marking_was_disabled(bool disableMarking)
    {
        using TestBrushes brushes = new();
        brushes.Set("GitExtensionsDiffLineNumberBackgroundBrush", Colors.White);
        brushes.Set("GitExtensionsDiffLineNumberBrush", Colors.Blue);
        brushes.Set("GitExtensionsDiffLineNumberSelectedBrush", Colors.Red);
        FileViewerInternal viewer = new();
        TextEditor editor = viewer.Editor;
        editor.Text = "first\nsecond\nthird";
        DiffViewerLineNumberControl margin = viewer.LineNumbersControl;
        margin.DisplayLineNum(CreateLines(DiffLineType.Context, DiffLineType.Context, DiffLineType.Context), showLeftColumn: true);
        if (disableMarking)
        {
            viewer.DontMarkGutterSelectedLine();
        }

        Window window = new() { Width = 480, Height = 240, Content = viewer };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            TextView textView = editor.TextArea.TextView;
            VisualLine[] visualLines = textView.VisualLines.ToArray();
            using SKBitmap before = Capture(window);
            Rect firstRow = GetRowBounds(margin, textView, visualLines[0], window);
            Rect secondRow = GetRowBounds(margin, textView, visualLines[1], window);
            if (disableMarking)
            {
                CountRedInk(before, firstRow).Should().Be(0);
            }
            else
            {
                CountRedInk(before, firstRow).Should().BeGreaterThan(0);
            }

            CountRedInk(before, secondRow).Should().Be(0);
            editor.TextArea.Caret.Line = 2;
            Dispatcher.UIThread.RunJobs();
            textView.VisualLines.Should().Equal(visualLines, "moving a caret between already visible lines must not require a reflow to redraw its numbers");
            using SKBitmap after = Capture(window);
            CountRedInk(after, firstRow).Should().Be(0);
            if (disableMarking)
            {
                CountRedInk(after, secondRow).Should().Be(0);
            }
            else
            {
                CountRedInk(after, secondRow).Should().BeGreaterThan(0);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Disabled_gutter_should_use_native_InactiveBorder_and_keep_diff_semantic_overlays(bool disableParent)
    {
        using TestBrushes brushes = new();
        Color normal = Color.Parse("#132f45");
        Color inactive = Color.Parse("#f4f7fc");
        Color removed = Color.Parse("#d04943");
        Color added = Color.Parse("#48a34c");
        brushes.Set("GitExtensionsDiffLineNumberBackgroundBrush", normal);
        brushes.Set("GitExtensionsNativeDisabledGutterBackgroundBrush", inactive);
        brushes.Set("GitExtensionsDiffRemovedBrush", removed);
        brushes.Set("GitExtensionsDiffAddedBrush", added);
        FileViewerInternal viewer = new();
        TextEditor editor = viewer.Editor;
        editor.Text = "context\nremoved\nadded\nboth\nleft\nright";
        DiffViewerLineNumberControl margin = viewer.LineNumbersControl;
        margin.DisplayLineNum(CreateLines(DiffLineType.Context, DiffLineType.Minus, DiffLineType.Plus,
            DiffLineType.MinusPlus, DiffLineType.MinusLeft, DiffLineType.PlusRight), showLeftColumn: true);
        Border parent = new() { Child = viewer };
        Window window = new() { Width = 480, Height = 240, Content = parent };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            using SKBitmap enabled = Capture(window);
            AssertRowFill(enabled, margin, editor.TextArea.TextView, window, 1, normal, normal);
            if (disableParent)
            {
                parent.IsEnabled = false;
            }
            else
            {
                editor.IsEnabled = false;
            }

            Dispatcher.UIThread.RunJobs();
            editor.IsEffectivelyEnabled.Should().BeFalse();
            using SKBitmap disabled = Capture(window);
            AssertRowFill(disabled, margin, editor.TextArea.TextView, window, 1, inactive, inactive);
            AssertRowFill(disabled, margin, editor.TextArea.TextView, window, 2, removed, removed);
            AssertRowFill(disabled, margin, editor.TextArea.TextView, window, 3, added, added);
            AssertRowFill(disabled, margin, editor.TextArea.TextView, window, 4, removed, added);
            AssertRowFill(disabled, margin, editor.TextArea.TextView, window, 5, removed, inactive);
            AssertRowFill(disabled, margin, editor.TextArea.TextView, window, 6, inactive, added);
            parent.IsEnabled = true;
            editor.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();
            using SKBitmap restored = Capture(window);
            AssertRowFill(restored, margin, editor.TextArea.TextView, window, 1, normal, normal);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRowFill(SKBitmap bitmap, DiffViewerLineNumberControl margin, TextView textView,
        Window window, int lineNumber, Color left, Color right)
    {
        VisualLine line = textView.VisualLines.Single(line => line.FirstDocumentLine.LineNumber == lineNumber);
        Rect row = GetRowBounds(margin, textView, line, window);
        bitmap.GetPixel((int)row.X + 1, (int)(row.Y + (row.Height / 2))).Should().Be(ToSkColor(left));
        bitmap.GetPixel((int)row.Right - 1, (int)(row.Y + (row.Height / 2))).Should().Be(ToSkColor(right));
    }

    private static SKBitmap Capture(Window window)
    {
        using WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new AssertionException("The gutter fixture must render its real visual tree.");
        using MemoryStream stream = new();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }

    private static int CountRedInk(SKBitmap bitmap, Rect bounds)
    {
        int count = 0;
        for (int y = (int)Math.Ceiling(bounds.Top); y < (int)bounds.Bottom; y++)
        {
            for (int x = (int)Math.Ceiling(bounds.Left); x < (int)bounds.Right; x++)
            {
                SKColor pixel = bitmap.GetPixel(x, y);
                if (pixel.Red > pixel.Green && pixel.Red > pixel.Blue)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static DiffLinesInfo CreateLines(params DiffLineType[] types)
    {
        DiffLinesInfo lines = new();
        for (int index = 0; index < types.Length; index++)
        {
            lines.Add(new DiffLineInfo
            {
                LineNumInDiff = index + 1,
                LeftLineNumber = index + 1,
                RightLineNumber = index + 1,
                LineType = types[index],
            });
        }

        return lines;
    }

    private static Rect GetRowBounds(DiffViewerLineNumberControl margin, TextView textView, VisualLine line, Window window)
    {
        Point origin = margin.TranslatePoint(new Point(0, line.VisualTop - textView.ScrollOffset.Y), window)
            ?? throw new AssertionException("The gutter must belong to the fixture window.");
        return new Rect(origin, new Size(margin.Bounds.Width, line.Height));
    }

    private static int MeasureGlyph(TextEditor editor, string value)
        => (int)Math.Round(new FormattedText(value, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface(editor.FontFamily, editor.FontStyle, editor.FontWeight), editor.FontSize, Brushes.Black).Width,
            MidpointRounding.AwayFromZero);

    private static SKColor ToSkColor(Color color) => new(color.R, color.G, color.B, color.A);

    private sealed class TestBrushes : IDisposable
    {
        private readonly Application _application = Application.Current
            ?? throw new InvalidOperationException("The headless test application must be running.");
        private readonly Dictionary<string, object?> _previous = [];

        public void Set(string key, Color color)
        {
            _previous.Add(key, _application.Resources.TryGetValue(key, out object? value) ? value : null);
            _application.Resources[key] = new SolidColorBrush(color);
        }

        public void Dispose()
        {
            foreach ((string key, object? value) in _previous)
            {
                if (value is null)
                {
                    _application.Resources.Remove(key);
                }
                else
                {
                    _application.Resources[key] = value;
                }
            }
        }
    }
}
