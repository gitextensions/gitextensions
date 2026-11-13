using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using GitExtUtils;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

// Clipboard operations use only the isolated shim service, never the owner's OS clipboard.
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CommitInfoParagraphCopyTests
{
    private const string VisibleCaption = "A & B";
    private const string FirstParagraph = "plain ä\tvalue";
    private const string LastParagraph = "last e\u0301";

    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void XHTML_paragraphs_should_retain_native_LF_plain_copy_and_one_caret_position_on_every_platform(
        [Values("header", "body", "refs")] string role,
        [Values("\n", "\r\n", "\r", "<br/>")] string separator,
        [Values(false, true)] bool trailingParagraph)
    {
        WithSurface(role, (_, block, window) =>
        {
            string xhtml = FirstParagraph + separator + "<a href='gitext://author'>A &amp; B</a>"
                + separator + LastParagraph + (trailingParagraph ? separator : string.Empty);
            string expected = FirstParagraph + '\n' + VisibleCaption + '\n' + LastParagraph
                + (trailingParagraph ? "\n" : string.Empty);
            block.SetXHTMLText(xhtml);
            RunLayout(window);
            block.GetPlainText().Should().Be(expected);
            string layoutText = block.Inlines?.Text
                ?? throw new AssertionException("The source XHTML must expose its actual logical inline positions.");
            layoutText.Should().NotContain("\r");
            layoutText.Count(character => character == '\n').Should().Be(trailingParagraph ? 3 : 2);
            GetLayout(block).TextLines.Should().HaveCount(trailingParagraph ? 4 : 3);
            block.SelectAll();
            block.SelectionEnd.Should().Be(layoutText.Length);
            block.GetSelectionPlainText().Should().Be(expected);
            block.GetSelectionPlainText().Should().NotContain("\uFFFC");

            int firstBreak = layoutText.IndexOf('\n');
            firstBreak.Should().Be(FirstParagraph.Length,
                "tabs and UTF16 source characters precede a single native paragraph position");
            SelectAndAssert(block, firstBreak, firstBreak + 1, "\n");
            int lastBreak = layoutText.LastIndexOf('\n', layoutText.Length - (trailingParagraph ? 2 : 1));
            SelectAndAssert(block, lastBreak, lastBreak + 3, "\nla");
            SelectAndAssert(block, lastBreak + 3, lastBreak, "\nla");
            if (trailingParagraph)
            {
                SelectAndAssert(block, layoutText.Length - 1, layoutText.Length, "\n");
            }
        });
    }

    [AvaloniaTest]
    public void Source_keyboard_copy_should_preserve_native_paragraph_selection_without_rewriting_the_caret(
        [Values("header", "body", "refs")] string role)
    {
        WithClipboard(clipboard => WithSurface(role, (_, block, window) =>
        {
            block.SetXHTMLText("first\r\nsecond\r\nthird");
            RunLayout(window);
            block.Focus().Should().BeTrue();
            SelectAndAssert(block, 5, 9, "\nsec");
            window.KeyPress(Key.C, RawInputModifiers.Control, PhysicalKey.C, keySymbol: "c");
            clipboard.Received(1).SetText("\nsec");
            block.SelectionStart.Should().Be(5);
            block.SelectionEnd.Should().Be(9);
            clipboard.ClearReceivedCalls();
            block.SelectionStart = block.SelectionEnd = 6;
            window.KeyPress(Key.C, RawInputModifiers.Control, PhysicalKey.C, keySymbol: "c");

            // The original handlers unconditionally copy the helper's empty result.
            clipboard.Received(1).SetText(string.Empty);
        }));
    }

    [AvaloniaTest]
    public void Asynchronously_replaced_XHTML_should_not_retain_the_previous_paragraph_positions_or_copy_text(
        [Values("header", "body", "refs")] string role)
    {
        WithClipboard(clipboard => WithSurface(role, (_, block, window) =>
        {
            block.SetXHTMLText("old\r\ntext\r\n");
            RunLayout(window);
            block.Focus().Should().BeTrue();
            block.SelectAll();
            block.GetSelectionPlainText().Should().Be("old\ntext\n");
            Dispatcher.UIThread.Post(() => block.SetXHTMLText("new\n<a href='gitext://author'>A &amp; B</a>\n"));
            RunLayout(window);
            block.SelectAll();
            block.GetSelectionPlainText().Should().Be("new\nA & B\n");
            window.KeyPress(Key.C, RawInputModifiers.Control, PhysicalKey.C, keySymbol: "c");
            clipboard.Received(1).SetText("new\nA & B\n");
            block.Clear();
            RunLayout(window);
            block.GetPlainText().Should().BeEmpty();
            block.GetSelectionPlainText().Should().BeEmpty();
            block.SetXHTMLText("final\r\nplain");
            RunLayout(window);
            block.Focus().Should().BeTrue();
            SelectAndAssert(block, 5, 6, "\n");
        }));
    }

    [AvaloniaTest]
    public void Source_wrapped_body_and_refs_links_should_copy_partial_caption_characters_and_wrap_as_real_text(
        [Values("body", "refs")] string role)
    {
        WithClipboard(clipboard => WithSurface(role, (_, block, window) =>
        {
            const string caption = "office affinity office affinity office affinity office affinity";
            block.SetXHTMLText($"head <a href='gitext://wrapped-body'>{caption}</a> tail\nlast");
            window.Width = 300;
            RunLayout(window);
            IEnumerable<XhtmlLinkRun> links = block.Inlines?.OfType<XhtmlLinkRun>()
                ?? throw new AssertionException("The source body must retain its selectable link inlines.");
            links.Should().ContainSingle(
                "source CFE_LINK decorates selectable characters rather than one atomic embedded button");
            block.Focus().Should().BeTrue();
            SelectAndAssert(block, 6, 10, "ffic");
            window.KeyPress(Key.C, RawInputModifiers.Control, PhysicalKey.C, keySymbol: "c");
            clipboard.Received(1).SetText("ffic");
            GetLayout(block).TextLines.Count.Should().BeGreaterThan(2);
            block.SelectAll();
            block.GetSelectionPlainText().Should().Be($"head {caption} tail\nlast",
                "wrapping changes visual lines, not authored source paragraphs or copy text");
        }));
    }

    [AvaloniaTest]
    public void Unicode_XHTML_copy_should_preserve_the_current_logical_text_without_claiming_native_surrogate_equivalence(
        [Values("header", "body", "refs")] string role)
    {
        WithClipboard(clipboard => WithSurface(role, (_, block, window) =>
        {
            const string text = "plain ä😀\ne\u0301 & 😀\nlast";
            block.SetXHTMLText("plain ä😀\n<a href='gitext://unicode'>e\u0301 &amp; 😀</a>\nlast");
            RunLayout(window);
            block.GetPlainText().Should().Be(text);
            string layoutText = block.Inlines?.Text
                ?? throw new AssertionException("The Unicode source characters must remain real text positions.");
            layoutText.Should().Be(text);
            block.Focus().Should().BeTrue();
            block.SelectAll();
            block.GetSelectionPlainText().Should().Be(text);
            window.KeyPress(Key.C, RawInputModifiers.Control, PhysicalKey.C, keySymbol: "c");
            clipboard.Received(1).SetText(text);
            int emojiStart = text.IndexOf("😀", StringComparison.Ordinal);
            SelectAndAssert(block, emojiStart, emojiStart + "😀".Length, "😀");

            // The original helper returns the same native surrogate pair for either
            // UTF16 unit and can duplicate it. Retain portable text safely here; this
            // case does not establish native Unicode helper equivalence.
            block.GetPlainText().Should().Be(text);
        }));
    }

    [AvaloniaTest]
    public void Source_body_and_refs_right_click_should_preserve_partial_caption_selection_and_query_the_fresh_context_link(
        [Values("body", "refs")] string role, [Values(false, true)] bool pressInsideSelection)
    {
        WithClipboard(clipboard => WithSurface(role, (control, block, window) =>
        {
            const string uri = "gitext://copy-caption";
            block.SetXHTMLText($"head <a href='{uri}'>caption</a> tail\nlast");
            RunLayout(window);
            block.Focus().Should().BeTrue();
            int start = pressInsideSelection ? 6 : 1;
            int end = start + (pressInsideSelection ? 4 : 2);
            string selection = pressInsideSelection ? "apti" : "ea";
            SelectAndAssert(block, start, end, selection);
            RunLayout(window);
            Rect character = GetLayout(block).HitTestTextRange(8, 1).Single();
            Point point = block.TranslatePoint(character.Center, window)
                ?? throw new AssertionException("The source caption characters must be attached to the owned window.");
            int activations = 0;
            block.LinkClicked += (_, _) => activations++;
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            try
            {
                window.MouseMove(point);
                window.MouseDown(point, MouseButton.Right);
                RunLayout(window);
                block.GetSelectionPlainText().Should().Be(selection);
                block.SelectionStart.Should().Be(start);
                block.SelectionEnd.Should().Be(end);
                window.MouseUp(point, MouseButton.Right);
                RunLayout(window);
                accessor.ContextMenu.IsOpen.Should().BeTrue();
                accessor.CopyLinkMenuItem.Tag.Should().Be(uri);
                block.SelectionStart.Should().Be(start);
                block.SelectionEnd.Should().Be(end);
                block.GetSelectionPlainText().Should().Be(selection);
                accessor.CopyLinkMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                clipboard.Received(1).SetText(uri);
                block.GetSelectionPlainText().Should().Be(selection);
                activations.Should().Be(0);
            }
            finally
            {
                accessor.ContextMenu.Close();
            }
        }));
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Copy_commit_info_should_keep_internal_LF_and_the_original_platform_separator_between_its_two_blocks(bool trailingParagraph)
    {
        WithClipboard(clipboard => WithSurface("body", (control, _, window) =>
        {
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            XhtmlTextBlock header = accessor.Header.GetTestAccessor().RevisionHeader;
            header.SetXHTMLText("Author:\tA &amp; B\r\nCommit hash:\tabc123");
            string body = "body first\nbody second" + (trailingParagraph ? "\n" : string.Empty);
            accessor.CommitMessage.SetXHTMLText(body.Replace("\n", "\r\n", StringComparison.Ordinal));
            accessor.RevisionInfo.SetXHTMLText("refs only\nmust not be in copy");
            RunLayout(window);
            accessor.Header.GetPlainText().Should().Be("Author: A & B\nCommit hash: abc123");
            MenuItem copy = accessor.ContextMenu.Items.OfType<MenuItem>()
                .Single(item => item.Name == "copyCommitInfoToolStripMenuItem");
            copy.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            string expected = "Author: A & B\nCommit hash: abc123"
                + Environment.NewLine + Environment.NewLine + body;
            clipboard.Received(1).SetText(expected);
            clipboard.DidNotReceive().SetText(Arg.Is<string>(text => text.Contains("refs only", StringComparison.Ordinal)));
        }));
    }

    private static TextLayout GetLayout(XhtmlTextBlock block)
        => block.TextLayout ?? throw new AssertionException("The attached XHTML surface must retain its real text layout.");

    private static void RunLayout(Window window)
    {
        for (int pass = 0; pass < 8; pass++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void SelectAndAssert(XhtmlTextBlock block, int start, int end, string expected)
    {
        block.SelectionStart = start;
        block.SelectionEnd = end;
        block.GetSelectionPlainText().Should().Be(expected);
        block.SelectionStart.Should().Be(start);
        block.SelectionEnd.Should().Be(end);
    }

    private static void WithClipboard(Action<WinFormsShims.IClipboard> verify)
    {
        FieldInfo clipboardField = typeof(WinFormsShims.ShimHost).GetField("_clipboard", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingFieldException(typeof(WinFormsShims.ShimHost).FullName, "_clipboard");
        object? originalClipboard = clipboardField.GetValue(null);
        WinFormsShims.IClipboard clipboard = Substitute.For<WinFormsShims.IClipboard>();
        WinFormsShims.ShimHost.Clipboard = clipboard;
        try
        {
            verify(clipboard);
        }
        finally
        {
            // Restore an originally absent service too, without touching any OS clipboard.
            clipboardField.SetValue(null, originalClipboard);
        }
    }

    private static void WithSurface(string role, Action<CommitInfo, XhtmlTextBlock, Window> verify)
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        accessor.TableLayout.IsVisible = true;
        XhtmlTextBlock block = role switch
        {
            "header" => accessor.Header.GetTestAccessor().RevisionHeader,
            "body" => accessor.CommitMessage,
            "refs" => accessor.RevisionInfo,
            _ => throw new ArgumentOutOfRangeException(nameof(role)),
        };
        Window window = new() { Width = 700, Height = 600, Content = control };
        try
        {
            window.Show();
            RunLayout(window);
            verify(control, block, window);
        }
        finally
        {
            window.Close();
        }
    }
}
