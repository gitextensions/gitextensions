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

        contents.Width.Should().BeGreaterThan(0);
        int textWidth = TextRenderer.MeasureText(hash, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
        Point valueOrigin = editor.GetPositionFromCharIndex(editor.Text.IndexOf(hash, StringComparison.Ordinal));
        NativeRectangle format = default;
        SendMessage(editor.Handle, GetFormatRectangle, 0, ref format);
        editor.SelectionTabs.Should().BeEmpty("the source anchor insertion replaces the paragraph through SelectedRtf");
        format.Left.Should().Be(1);
        (editor.ClientSize.Width - format.Right).Should().Be(1);
        valueOrigin.X.Should().Be(96 + format.Left);
        if (sizeInPoints == 9)
        {
            int longestValueWidth = new[] { WebUtility.HtmlDecode(author), "9 months ago (1/2/2026 12:00:00 PM)", hash }
                .Max(value => TextRenderer.MeasureText(value, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width);
            contents.Width.Should().Be(96 + longestValueWidth + format.Left + editor.ClientSize.Width - format.Right);
        }

        TestContext.Progress.WriteLine($"font={sizeInPoints} hash={hash} labelTab={firstTab} storedTabs={string.Join(',', editor.SelectionTabs)} glyphWidth={textWidth} valueX={valueOrigin.X} contents={contents} format=({format.Left},{format.Top},{format.Right},{format.Bottom})");
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, nint longParameter);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, ref NativeRectangle rectangle);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
