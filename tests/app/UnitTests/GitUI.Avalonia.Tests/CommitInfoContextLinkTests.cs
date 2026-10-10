using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtUtils;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CommitInfoContextLinkTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void Shared_context_menu_should_query_only_the_freshly_right_clicked_editor_and_character()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        accessor.TableLayout.IsVisible = true;
        XhtmlTextBlock header = accessor.Header.GetTestAccessor().RevisionHeader;
        XhtmlTextBlock[] surfaces = [accessor.CommitMessage, header, accessor.RevisionInfo];
        string[] links = ["gitext://body", "gitext://header", "gitext://revision"];
        for (int index = 0; index < surfaces.Length; index++)
        {
            surfaces[index].SetXHTMLText($"prefix <a href='{links[index]}'>first caption</a> plain <a href='{links[index]}/second'>second caption</a> tail");
        }

        Window window = new() { Width = 720, Height = 360, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            for (int index = 0; index < surfaces.Length; index++)
            {
                XhtmlTextBlock block = surfaces[index];
                RequestAt(window, accessor.ContextMenu, GetLinkPoint(block, window, 0));
                accessor.CopyLinkMenuItem.IsVisible.Should().BeTrue();
                accessor.CopyLinkMenuItem.Tag.Should().Be(links[index]);
                accessor.CopyLinkMenuItem.Header?.ToString().Should().Contain(links[index]);
                accessor.ContextMenu.Close();
                Dispatcher.UIThread.RunJobs();

                RequestAt(window, accessor.ContextMenu, GetLinkPoint(block, window, 1));
                accessor.CopyLinkMenuItem.Tag.Should().Be(links[index] + "/second");
                accessor.ContextMenu.Close();
                Dispatcher.UIThread.RunJobs();

                RequestAt(window, accessor.ContextMenu, GetCharacterPoint(block, window, 2));
                accessor.CopyLinkMenuItem.IsVisible.Should().BeFalse();
                accessor.CopyLinkMenuItem.Tag.Should().BeNull();
                accessor.ContextMenu.Close();
                Dispatcher.UIThread.RunJobs();
            }

            RequestAt(window, accessor.ContextMenu, GetLinkPoint(accessor.CommitMessage, window, 0));
            accessor.ContextMenu.Close();
            Dispatcher.UIThread.RunJobs();
            Point rootBlank = new(control.Bounds.Width / 2, control.Bounds.Height - 10);
            RequestAt(window, accessor.ContextMenu, control.TranslatePoint(rootBlank, window)
                ?? throw new AssertionException("The context owner must belong to the fixture window."));
            accessor.CopyLinkMenuItem.IsVisible.Should().BeFalse();
            accessor.CopyLinkMenuItem.Tag.Should().BeNull();
        }
        finally
        {
            accessor.ContextMenu.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Copy_link_click_should_copy_the_fresh_tag_without_activating_or_changing_the_context_selection()
    {
        WinFormsShims.IClipboard? previousClipboard = GetInstalledClipboard();
        WinFormsShims.IClipboard clipboard = Substitute.For<WinFormsShims.IClipboard>();
        WinFormsShims.ShimHost.Clipboard = clipboard;
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        accessor.TableLayout.IsVisible = true;
        XhtmlTextBlock header = accessor.Header.GetTestAccessor().RevisionHeader;
        header.SetXHTMLText("prefix <a href='gitext://gotorevision/second'>caption</a> tail");
        int activations = 0;
        header.LinkClicked += (_, _) => activations++;
        Window window = new() { Width = 600, Height = 260, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            header.Focus().Should().BeTrue();
            header.SelectionStart = 1;
            header.SelectionEnd = 3;
            Dispatcher.UIThread.RunJobs();
            RequestAt(window, accessor.ContextMenu, GetLinkPoint(header, window, 0));
            accessor.CopyLinkMenuItem.Tag.Should().Be("gitext://gotorevision/second");
            header.SelectionStart.Should().Be(1);
            header.SelectionEnd.Should().Be(3);
            accessor.CopyLinkMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            clipboard.Received().SetText("gitext://gotorevision/second");
            header.SelectionStart.Should().Be(1);
            header.SelectionEnd.Should().Be(3);
            activations.Should().Be(0);
        }
        finally
        {
            accessor.ContextMenu.Close();
            window.Close();
            // Restore even an absent test-host service; the public setter cannot express it.
            WinFormsShims.ShimHost.Clipboard = previousClipboard!;
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Native_header_right_click_should_preserve_selection_inside_and_outside_the_pressed_caption(bool pressInsideSelection)
    {
        const string link = "https://example.invalid/source";
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        accessor.TableLayout.IsVisible = true;
        XhtmlTextBlock header = accessor.Header.GetTestAccessor().RevisionHeader;
        header.SetXHTMLText($"head <a href='{link}'>caption</a> tail");
        int activations = 0;
        header.LinkClicked += (_, _) => activations++;
        Window window = new() { Width = 600, Height = 260, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            header.Focus().Should().BeTrue();
            int selectionStart = pressInsideSelection ? 6 : 1;
            int selectionEnd = selectionStart + (pressInsideSelection ? 4 : 2);
            header.SelectionStart = selectionStart;
            header.SelectionEnd = selectionEnd;
            Dispatcher.UIThread.RunJobs();
            Point caption = GetCharacterPoint(header, window, 8);
            window.MouseMove(caption);
            window.MouseDown(caption, MouseButton.Right);
            Dispatcher.UIThread.RunJobs();
            header.SelectionStart.Should().Be(selectionStart);
            header.SelectionEnd.Should().Be(selectionEnd);
            activations.Should().Be(0);

            window.MouseUp(caption, MouseButton.Right);
            Dispatcher.UIThread.RunJobs();
            accessor.ContextMenu.IsOpen.Should().BeTrue("the unhandled real right-button release must open the shared context menu");
            accessor.CopyLinkMenuItem.IsVisible.Should().BeTrue();
            accessor.CopyLinkMenuItem.Tag.Should().Be(link);
            header.SelectionStart.Should().Be(selectionStart);
            header.SelectionEnd.Should().Be(selectionEnd);
            header.GetSelectionPlainText().Should().Be(pressInsideSelection ? "apti" : "ea");
            activations.Should().Be(0);
            CaptureContextStateIfRequested(window);

            accessor.ContextMenu.Close();
            Dispatcher.UIThread.RunJobs();
            header.SelectionStart.Should().Be(selectionStart);
            header.SelectionEnd.Should().Be(selectionEnd);
        }
        finally
        {
            accessor.ContextMenu.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Keyboard_context_request_should_use_the_known_mouse_point_not_the_caret_or_old_link()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        accessor.TableLayout.IsVisible = true;
        XhtmlTextBlock header = accessor.Header.GetTestAccessor().RevisionHeader;
        header.SetXHTMLText("prefix <a href='gitext://first'>first</a> plain <a href='gitext://second'>second</a> tail");
        Window window = new() { Width = 600, Height = 260, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            RequestAt(window, accessor.ContextMenu, GetLinkPoint(header, window, 0));
            accessor.ContextMenu.Close();
            Dispatcher.UIThread.RunJobs();
            window.MouseMove(GetLinkPoint(header, window, 1));
            header.Focus().Should().BeTrue();
            header.SelectionStart = 0;
            header.SelectionEnd = 0;
            Dispatcher.UIThread.RunJobs();
            window.KeyPress(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, keySymbol: null);
            window.KeyRelease(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, keySymbol: null);
            Dispatcher.UIThread.RunJobs();
            accessor.ContextMenu.IsOpen.Should().BeTrue();
            accessor.CopyLinkMenuItem.Tag.Should().Be("gitext://second");
        }
        finally
        {
            accessor.ContextMenu.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Keyboard_context_request_without_a_known_pointer_after_reparenting_should_not_reuse_a_link()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        accessor.TableLayout.IsVisible = true;
        XhtmlTextBlock header = accessor.Header.GetTestAccessor().RevisionHeader;
        header.SetXHTMLText("prefix <a href='gitext://first'>caption</a>");
        Window first = new() { Width = 600, Height = 260, Content = control };
        Window second = new() { Width = 600, Height = 260 };
        first.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            RequestAt(first, accessor.ContextMenu, GetLinkPoint(header, first, 0));
            accessor.ContextMenu.Close();
            first.Content = null;
            second.Content = control;
            second.Show();
            Dispatcher.UIThread.RunJobs();
            header.Focus().Should().BeTrue();
            second.KeyPress(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, keySymbol: null);
            second.KeyRelease(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, keySymbol: null);
            Dispatcher.UIThread.RunJobs();
            accessor.ContextMenu.IsOpen.Should().BeTrue();
            accessor.CopyLinkMenuItem.IsVisible.Should().BeFalse();
            accessor.CopyLinkMenuItem.Tag.Should().BeNull();
        }
        finally
        {
            accessor.ContextMenu.Close();
            first.Close();
            second.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Context_link_lookup_should_match_source_nearest_character_semantics_in_blank_space(bool nativeHeader, bool trailingPlainText)
    {
        CommitInfoHeader header = new();
        XhtmlTextBlock block = nativeHeader ? header.GetTestAccessor().RevisionHeader : new();
        const string link = "https://example.invalid/source";
        block.SetXHTMLText($"head <a href='{link}'>caption</a>{(trailingPlainText ? " tail" : string.Empty)}");
        Window window = new() { Width = 600, Height = 220, Content = nativeHeader ? header : block };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Point captionInWindow = GetLinkPoint(block, window, 0);
            Point caption = window.TranslatePoint(captionInWindow, block)
                ?? throw new AssertionException("The source link must belong to the XHTML surface.");
            block.SelectionStart = 1;
            block.SelectionEnd = 3;
            Dispatcher.UIThread.RunJobs();
            block.GetContextLinkAtPoint(caption).Should().Be(link);
            block.GetContextLinkAtPoint(new Point(block.Bounds.Width - 1, caption.Y))
                .Should().Be(trailingPlainText ? null : link);
            block.GetContextLinkAtPoint(new Point(caption.X, block.Bounds.Height + 90)).Should().Be(link);
            Point prefix = GetCharacterPoint(block, window, 2);
            block.GetContextLinkAtPoint(window.TranslatePoint(prefix, block)
                ?? throw new AssertionException("The source prefix must belong to the XHTML surface.")).Should().BeNull();
            block.SelectionStart.Should().Be(1);
            block.SelectionEnd.Should().Be(3);
        }
        finally
        {
            window.Close();
        }
    }

    private static void CaptureContextStateIfRequested(Window window)
    {
        string? evidenceRoot = Environment.GetEnvironmentVariable("GITEXT_SPLIT_STATE_EVIDENCE");
        if (string.IsNullOrEmpty(evidenceRoot))
        {
            return;
        }

        if (!Path.IsPathFullyQualified(evidenceRoot))
        {
            throw new InvalidOperationException("Context state evidence requires an explicit absolute directory.");
        }

        window.UpdateLayout();
        using WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The context selection frame is unavailable.");
        Directory.CreateDirectory(evidenceRoot);
        string caseName = string.Concat(TestContext.CurrentContext.Test.Name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        using FileStream stream = File.Create(Path.Combine(evidenceRoot, $"context-{caseName}-{Guid.NewGuid():N}.png"));
        frame.Save(stream, PngBitmapEncoderOptions.Default);
    }

    private static Point GetCharacterPoint(XhtmlTextBlock block, Window window, int character)
    {
        Rect bounds = block.TextLayout.HitTestTextPosition(character);
        Point point = new(bounds.X + (bounds.Width / 2) + block.NativeFormattingInset + block.Padding.Left,
            bounds.Y + (bounds.Height / 2) + block.Padding.Top);
        return block.TranslatePoint(point, window)
            ?? throw new AssertionException("The XHTML surface must belong to the fixture window.");
    }

    private static WinFormsShims.IClipboard? GetInstalledClipboard()
    {
        try
        {
            return WinFormsShims.ShimHost.Clipboard;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static Point GetLinkPoint(XhtmlTextBlock block, Window window, int index)
    {
        if (block.GetVisualDescendants().OfType<HyperlinkButton>().ToArray() is { Length: > 0 } buttons)
        {
            HyperlinkButton button = buttons[index];
            return button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)
                ?? throw new AssertionException("The embedded link must belong to the fixture window.");
        }

        InlineCollection inlines = block.Inlines
            ?? throw new AssertionException("The header must contain its source inline collection.");
        XhtmlLinkRun link = inlines.OfType<XhtmlLinkRun>().ElementAt(index);
        int position = 0;
        foreach (Inline inline in inlines)
        {
            if (ReferenceEquals(inline, link))
            {
                return GetCharacterPoint(block, window, position + ((link.Text?.Length ?? 0) / 2));
            }

            position += inline switch
            {
                Run run => run.Text?.Length ?? 0,
                LineBreak => Environment.NewLine.Length,
                InlineUIContainer => 1,
                _ => 0,
            };
        }

        throw new AssertionException("The source text link must belong to its inline collection.");
    }

    private static void RequestAt(Window window, ContextMenu menu, Point point)
    {
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Right);
        window.MouseUp(point, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();
        menu.IsOpen.Should().BeTrue("the actual routed right-button request must open the shared context menu");
    }
}
