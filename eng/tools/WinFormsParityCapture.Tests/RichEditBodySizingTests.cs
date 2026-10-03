using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// The source body and revision-info controls leave WordWrap enabled and size rows from
// actual ContentsResized rectangles. This low-level consumer avoids AppSettings entirely;
// integration with the original CommitInfo parent is a separate isolated-worker contract.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RichEditBodySizingTests
{
    private const uint GetFormatRectangle = 0xB2;
    private const uint RequestResize = 0x441;
    private const string PlainParagraphs = "first short line\nsecond line with descenders gjpq\nlast line";
    private const string WrappedParagraph = "first short line second line with descenders gjpq last line repeated ordinary words for genuine RichEdit word wrapping";
    private const string MixedParagraphs = "plain <a href='https://example.invalid/source'>caption with affinity</a> tail\nsecond line with descenders gjpq\nlast line";
    private static readonly Type Extension = typeof(GitUI.CommitInfo.CommitInfoHeader).Assembly.GetType(
        "GitUI.Editor.RichTextBoxExtension.RichTextBoxXhtmlSupportExtension", throwOnError: true)
        ?? throw new InvalidOperationException("The unchanged source XHTML loader must be present.");
    private static readonly MethodInfo Loader = Extension.GetMethod("SetXHTMLText", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("The unchanged source XHTML conversion method must be present.");
    private static readonly MethodInfo GetCharacterFormat = Extension.GetMethod("GetCharFormat", BindingFlags.Public | BindingFlags.Static, [typeof(RichTextBox)])
        ?? throw new InvalidOperationException("The unchanged source native character-format reader must be present.");

    [TestCase("Segoe UI", 9)]
    [TestCase("Segoe UI", 11)]
    [TestCase("Segoe UI", 18)]
    [TestCase("Segoe UI", 22)]
    [TestCase("Consolas", 9)]
    [TestCase("Consolas", 11)]
    [TestCase("Consolas", 18)]
    [TestCase("Consolas", 22)]
    public void Source_body_contents_should_report_native_font_wrapping_and_final_paragraph_metrics(string family, int points)
    {
        using Font font = new(family, points, FontStyle.Regular, GraphicsUnit.Point);
        using Form host = new() { ClientSize = new Size(700, 1000), ShowInTaskbar = false };

        // These inputs are the source Designer's native100 child widths, not measurements
        // projected out of a screenshot. WordWrap remains the source default (true).
        using RichTextBox body = CreateSourceBody(font, new Size(440, 900));
        using RichTextBox refs = CreateSourceBody(font, new Size(448, 900));
        host.Controls.Add(body);
        host.Controls.Add(refs);
        body.BringToFront();
        host.Show();
        Application.DoEvents();
        body.DeviceDpi.Should().Be(96, "this probe measures native100 only; it must not synthesize a DPI context");
        using ResizeObserver bodyObserver = new(body);
        using ResizeObserver refsObserver = new(refs);
        ProbeEditor(body, bodyObserver, "commit-body", font);
        refs.BringToFront();
        ProbeEditor(refs, refsObserver, "revision-info", font);
        host.Close();
    }

    private static RichTextBox CreateSourceBody(Font font, Size size)
        => new()
        {
            Font = font,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            ReadOnly = true,
            Size = size,
        };

    private static void ProbeEditor(RichTextBox editor, ResizeObserver observer, string role, Font font)
    {
        int authoredWidth = editor.Width;
        observer.Request("empty", role);
        SetSourceXhtml(editor, PlainParagraphs);
        Snapshot plain = observer.Request("plain", role);
        SetSourceXhtml(editor, PlainParagraphs + "\n");
        Snapshot trailing = observer.Request("trailing-newline", role);
        trailing.Contents.Height.Should().BeGreaterThan(plain.Contents.Height,
            "a source trailing newline retains another native paragraph; this is not an estimated line count");
        SetSourceXhtml(editor, MixedParagraphs);
        observer.Request("mixed-anchor", role);
        SetSourceXhtml(editor, WrappedParagraph);
        Snapshot wide = observer.Request("wrapped-wide", role);
        editor.Width = 160;
        Snapshot narrow = observer.Request("wrapped-narrow", role);
        narrow.Contents.Height.Should().BeGreaterThanOrEqualTo(wide.Contents.Height);
        editor.Width = authoredWidth;
        Snapshot restored = observer.Request("wrapped-restored", role);
        restored.Contents.Should().Be(wide.Contents,
            "restoring the same native client width must restore the actual contents rectangle");
        using Font grownFont = new(font.Name, font.SizeInPoints * 2, font.Style, GraphicsUnit.Point);
        editor.Font = grownFont;
        observer.Request("font-grow", role);
        editor.Font = font;
        observer.Request("font-shrink", role);
        editor.Clear();
        observer.Request("clear", role);
    }

    private static void SetSourceXhtml(RichTextBox editor, string xhtml)
        => Loader.Invoke(null, [editor, xhtml]);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint window, uint message, nint firstParameter, nint secondParameter);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint window, uint message, nint firstParameter, ref NativeRectangle rectangle);

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint value);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", EntryPoint = "GetTextMetricsW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTextMetrics(nint deviceContext, out NativeTextMetrics metrics);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeTextMetrics
    {
        public int Height;
        public int Ascent;
        public int Descent;
        public int InternalLeading;
        public int ExternalLeading;
        public int AverageCharacterWidth;
        public int MaximumCharacterWidth;
        public int Weight;
        public int Overhang;
        public int DigitizedAspectX;
        public int DigitizedAspectY;
        public char FirstCharacter;
        public char LastCharacter;
        public char DefaultCharacter;
        public char BreakCharacter;
        public byte Italic;
        public byte Underlined;
        public byte StruckOut;
        public byte PitchAndFamily;
        public byte CharacterSet;
    }

    private sealed record Snapshot(Rectangle Contents);

    private sealed class ResizeObserver : IDisposable
    {
        private readonly RichTextBox _editor;
        private Rectangle? _contents;

        internal ResizeObserver(RichTextBox editor)
        {
            _editor = editor;
            _editor.ContentsResized += ContentsResized;
        }

        public void Dispose()
            => _editor.ContentsResized -= ContentsResized;

        internal Snapshot Request(string stage, string role)
        {
            _contents = null;
            Application.DoEvents();
            SendMessage(_editor.Handle, RequestResize, 0, 0);
            Application.DoEvents();
            Rectangle contents = _contents
                ?? throw new InvalidOperationException("RichEdit must deliver an actual ContentsResized rectangle.");
            contents.Height.Should().BeGreaterThan(0);
            NativeRectangle format = default;
            SendMessage(_editor.Handle, GetFormatRectangle, 0, ref format);
            int selectionStart = _editor.SelectionStart;
            int selectionLength = _editor.SelectionLength;
            int lineCount = _editor.GetLineFromCharIndex(_editor.TextLength) + 1;
            List<object> lines = [];
            for (int line = 0; line < lineCount; line++)
            {
                int start = _editor.GetFirstCharIndexFromLine(line);
                if (start < 0)
                {
                    throw new InvalidOperationException("Every observed native visual line must expose its real character origin.");
                }

                lines.Add(new { line, character = start, origin = _editor.GetPositionFromCharIndex(start).ToString() });
            }

            List<object> fonts = [];
            int caption = _editor.Text.IndexOf("caption", StringComparison.Ordinal);
            int tail = _editor.Text.IndexOf(" tail", StringComparison.Ordinal);
            foreach (int character in new[] { 0, caption, tail, Math.Max(0, _editor.TextLength - 1) }.Where(index => index >= 0).Distinct())
            {
                _editor.Select(character, character < _editor.TextLength ? 1 : 0);
                using Font selectedFont = _editor.SelectionFont
                    ?? throw new InvalidOperationException("The selected native character must expose one actual font.");
                object characterFormat = GetCharacterFormat.Invoke(null, [_editor])
                    ?? throw new InvalidOperationException("The original native CHARFORMAT must be available.");
                int twips = characterFormat.GetType().GetField("yHeight")?.GetValue(characterFormat) is int height
                    ? height
                    : throw new InvalidOperationException("The original character format must expose font height in twips.");
                selectedFont.SizeInPoints.Should().Be(twips / 20f);
                nint nativeFont = selectedFont.ToHfont();
                nint deviceContext = GetDC(_editor.Handle);
                nint previousFont = 0;
                NativeTextMetrics metrics = default;
                try
                {
                    deviceContext.Should().NotBe(0);
                    previousFont = SelectObject(deviceContext, nativeFont);
                    previousFont.Should().NotBe(0);
                    previousFont.Should().NotBe(-1);
                    GetTextMetrics(deviceContext, out metrics).Should().BeTrue();
                }
                finally
                {
                    if (previousFont != 0 && previousFont != -1)
                    {
                        SelectObject(deviceContext, previousFont);
                    }

                    if (deviceContext != 0)
                    {
                        ReleaseDC(_editor.Handle, deviceContext);
                    }

                    DeleteObject(nativeFont);
                }

                fonts.Add(new
                {
                    character,
                    family = selectedFont.Name,
                    points = selectedFont.SizeInPoints,
                    style = selectedFont.Style.ToString(),
                    twips,
                    metrics.Height,
                    metrics.Ascent,
                    metrics.Descent,
                    metrics.InternalLeading,
                    metrics.ExternalLeading,
                });
            }

            _editor.Select(selectionStart, selectionLength);
            TestContext.Out.WriteLine(JsonSerializer.Serialize(new
            {
                stage,
                role,
                dpi = _editor.DeviceDpi,
                requestedFamily = _editor.Font.Name,
                requestedPoints = _editor.Font.SizeInPoints,
                width = _editor.ClientSize.Width,
                wordWrap = _editor.WordWrap,
                paragraphSeparators = _editor.Text.Count(character => character == '\n'),
                textLength = _editor.TextLength,
                contents = contents.ToString(),
                formatRectangle = new { format.Left, format.Top, format.Right, format.Bottom },
                lines,
                fonts,
            }));
            return new Snapshot(contents);
        }

        private void ContentsResized(object? sender, ContentsResizedEventArgs e)
            => _contents = e.NewRectangle;
    }
}
