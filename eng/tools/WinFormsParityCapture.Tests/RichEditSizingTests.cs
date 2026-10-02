using System.Drawing;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RichEditSizingTests
{
    private const uint GetFormatRectangle = 0xB2;
    private const uint RequestResize = 0x441;

    [Test]
    [TestCase(9, "ceaece927abc97012d5cc36ea9dfba32321e9704")]
    [TestCase(9, "c932f21268731785cec9d37bfa3ff8f10485b46e")]
    [TestCase(9, "54930791bbaca26f32883b799bbed65f81ec9150")]
    [TestCase(11, "ceaece927abc97012d5cc36ea9dfba32321e9704")]
    public void Header_contents_width_should_use_the_paragraph_tabs_remaining_after_XHTML_conversion(int sizeInPoints, string hash)
    {
        using Font font = new("Segoe UI", sizeInPoints);
        using RichTextBox editor = new()
        {
            Font = font,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            WordWrap = false,
            ReadOnly = true,
            Size = new Size(500, 100),
        };
        string[] labels = ["Author", "Author date", "Committer", "Commit date", "Commit hash", "Children", "Parents"];
        int firstTab = labels.Select(label => TextRenderer.MeasureText(label + "  ", font).Width).Max();
        editor.SelectionTabs = [firstTab, firstTab + 1, firstTab + 2, firstTab + 3];
        Rectangle contents = default;
        editor.ContentsResized += (_, e) => contents = e.NewRectangle;
        editor.CreateControl();
        string author = hash.StartsWith("c932", StringComparison.Ordinal)
            ? "Avalonia Contributor &lt;avalonia@example.com&gt;"
            : "Parity Capture &lt;parity@example.invalid&gt;";
        string xhtml = $"Author:\t\t<a href='mailto:parity@example.invalid'>{author}</a>\nDate:\t\t9 months ago (1/2/2026 12:00:00 PM)\nCommit hash:\t{hash}";
        Type extension = typeof(GitUI.CommitInfo.CommitInfoHeader).Assembly.GetType("GitUI.Editor.RichTextBoxExtension.RichTextBoxXhtmlSupportExtension", throwOnError: true)
            ?? throw new InvalidOperationException("The original XHTML loader must be present.");
        MethodInfo loader = extension.GetMethod("SetXHTMLText", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("The original XHTML conversion method must be present.");
        loader.Invoke(null, [editor, xhtml]);
        Application.DoEvents();
        SendMessage(editor.Handle, RequestResize, 0, 0);
        Application.DoEvents();

        int textWidth = TextRenderer.MeasureText(hash, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
        Point valueOrigin = editor.GetPositionFromCharIndex(editor.Text.IndexOf(hash, StringComparison.Ordinal));
        NativeRectangle format = default;
        SendMessage(editor.Handle, GetFormatRectangle, 0, ref format);
        string[] values = [WebUtility.HtmlDecode(author), "9 months ago (1/2/2026 12:00:00 PM)", hash];
        Point[] valueOrigins = values.Select(value => editor.GetPositionFromCharIndex(editor.Text.IndexOf(value, StringComparison.Ordinal))).ToArray();
        int[] nativeValueWidths = values.Select(value =>
        {
            int start = editor.Text.IndexOf(value, StringComparison.Ordinal);
            return editor.GetPositionFromCharIndex(start + value.Length).X - editor.GetPositionFromCharIndex(start).X;
        }).ToArray();
        int[] gdiValueWidths = values.Select(value => TextRenderer.MeasureText(value, font,
            new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width).ToArray();
        editor.Select(editor.Text.IndexOf(hash, StringComparison.Ordinal), hash.Length);
        using Font selectedFont = editor.SelectionFont
            ?? throw new InvalidOperationException("The source hash must have a single font.");
        MethodInfo getCharacterFormat = extension.GetMethod("GetCharFormat", BindingFlags.Public | BindingFlags.Static,
            [typeof(RichTextBox)]) ?? throw new InvalidOperationException("The original character-format reader must be present.");
        object characterFormat = getCharacterFormat.Invoke(null, [editor])
            ?? throw new InvalidOperationException("The original character format must be available.");
        int nativeTwips = characterFormat.GetType().GetField("yHeight")?.GetValue(characterFormat) is int height
            ? height
            : throw new InvalidOperationException("The original character format must expose its native font height.");
        int lineHeight = TextRenderer.MeasureText("Mg", selectedFont, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Height;
        int paragraphCount = editor.Text.Count(character => character == '\n') + 1;
        TestContext.Progress.WriteLine($"font={sizeInPoints} selectedFont={selectedFont.Name}/{selectedFont.SizeInPoints} nativeTwips={nativeTwips} lineHeight={lineHeight} paragraphs={paragraphCount} hash={hash} labelTab={firstTab} storedTabs={string.Join(',', editor.SelectionTabs)} glyphWidth={textWidth} nativeValueWidths={string.Join(',', nativeValueWidths)} gdiValueWidths={string.Join(',', gdiValueWidths)} valueX={valueOrigin.X} contents={contents} format=({format.Left},{format.Top},{format.Right},{format.Bottom})");
        foreach (string value in values)
        {
            int start = editor.Text.IndexOf(value, StringComparison.Ordinal);
            editor.Select(start, value.Length);
            using Font valueFont = editor.SelectionFont
                ?? throw new InvalidOperationException("Each visible header value must have a single font.");
            Point begin = editor.GetPositionFromCharIndex(start);
            Point last = editor.GetPositionFromCharIndex(start + value.Length - 1);
            Point end = editor.GetPositionFromCharIndex(start + value.Length);
            int selectedFontWidth = TextRenderer.MeasureText(value, valueFont,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
            nint nativeFont = valueFont.ToHfont();
            nint deviceContext = GetDC(editor.Handle);
            nint previousFont = SelectObject(deviceContext, nativeFont);
            NativeSize nativeExtent = default;
            int[] advances = new int[value.Length];
            try
            {
                GetTextExtentPoint(deviceContext, value, value.Length, out nativeExtent).Should().BeTrue();
                GetTextExtentExPoint(deviceContext, value, value.Length, int.MaxValue,
                    out int fittedCharacters, advances, out _).Should().BeTrue();
                fittedCharacters.Should().Be(value.Length);
            }
            finally
            {
                SelectObject(deviceContext, previousFont);
                ReleaseDC(editor.Handle, deviceContext);
                DeleteObject(nativeFont);
            }

            TestContext.Progress.WriteLine($"value={value} indices={start}..{start + value.Length}/{editor.TextLength} font={valueFont.Name}/{valueFont.SizeInPoints} selectedFontWidth={selectedFontWidth} selectedHfontWidth={nativeExtent.Width} begin={begin} last={last} end={end} rtf={editor.SelectedRtf.Replace('\r', ' ').Replace('\n', ' ')}");
            nativeExtent.Width.Should().Be(end.X - begin.X);
            if (sizeInPoints == 9)
            {
                selectedFontWidth.Should().Be(end.X - begin.X);
                for (int index = 0; index < value.Length; index++)
                {
                    advances[index].Should().Be(editor.GetPositionFromCharIndex(start + index + 1).X - begin.X,
                        "native GDI cumulative advances must match each RichEdit insertion position, not only the final width");
                }
            }
        }

        contents.Width.Should().BeGreaterThan(0);
        contents.Height.Should().Be(paragraphCount * lineHeight,
            "ContentsResized includes every paragraph, including the final row, using the current native font line metrics");
        editor.Select(0, 0);
        editor.SelectionTabs.Should().BeEmpty("the source anchor insertion replaces the paragraph through SelectedRtf");
        format.Left.Should().Be(1);
        (editor.ClientSize.Width - format.Right).Should().Be(1);
        valueOrigin.X.Should().Be(96 + format.Left);
        selectedFont.Name.Should().Be(font.Name);
        nativeTwips.Should().BeGreaterThan(0);
        selectedFont.SizeInPoints.Should().Be(nativeTwips / 20f,
            "RichTextBox derives SelectionFont's point size from the native CHARFORMAT height in twips");
        int characterExtent = valueOrigins.Select((origin, index) => origin.X + nativeValueWidths[index]).Max()
            + editor.ClientSize.Width - format.Right;
        contents.Width.Should().Be(characterExtent,
            "each paragraph keeps its own tab origin: a longer author label can pass the first default stop before its second tab");
        if (sizeInPoints == 9)
        {
            selectedFont.SizeInPoints.Should().Be(sizeInPoints);
            nativeTwips.Should().Be(sizeInPoints * 20);

            int longestValueWidth = gdiValueWidths.Max();
            contents.Width.Should().Be(96 + longestValueWidth + format.Left + editor.ClientSize.Width - format.Right);
        }

        editor.Clear();
        AssertEmptyParagraphHeight("clear");
        using Font resizedFont = new(font.Name, sizeInPoints * 2);
        editor.Font = resizedFont;
        AssertEmptyParagraphHeight("empty-font-grow");
        editor.Font = font;
        AssertEmptyParagraphHeight("empty-font-shrink");

        void AssertEmptyParagraphHeight(string stage)
        {
            contents = default;
            Application.DoEvents();
            SendMessage(editor.Handle, RequestResize, 0, 0);
            Application.DoEvents();
            editor.Select(0, 0);
            using Font emptyFont = editor.SelectionFont
                ?? throw new InvalidOperationException("The empty native paragraph must retain a font.");
            int currentLineHeight = TextRenderer.MeasureText("Mg", editor.Font,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Height;
            int selectedLineHeight = TextRenderer.MeasureText("Mg", emptyFont,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Height;
            TestContext.Progress.WriteLine($"stage={stage} requestedFont={editor.Font.SizeInPoints} selectedFont={emptyFont.SizeInPoints} requestedLineHeight={currentLineHeight} selectedLineHeight={selectedLineHeight} textLength={editor.TextLength} contents={contents}");
            editor.TextLength.Should().Be(0);
            contents.Width.Should().Be(format.Left + editor.ClientSize.Width - format.Right);
            contents.Height.Should().Be(selectedLineHeight,
                "the native paragraph uses its actual selected font, not TextRenderer's separately cached requested font");
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, nint longParameter);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, ref NativeRectangle rectangle);

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint value);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W", CharSet = CharSet.Unicode)]
    private static extern bool GetTextExtentPoint(nint deviceContext, string text, int length, out NativeSize extent);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentExPointW", CharSet = CharSet.Unicode)]
    private static extern bool GetTextExtentExPoint(nint deviceContext, string text, int length,
        int maximumExtent, out int fittedCharacters, [Out] int[] advances, out NativeSize extent);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
