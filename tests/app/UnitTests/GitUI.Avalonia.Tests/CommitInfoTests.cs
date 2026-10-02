using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommitInfo;
using GitUI.Compat;
using GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using SkiaSharp;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CommitInfoTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void CommitInfo_should_reserve_scrollbar_space_when_content_overflows()
    {
        CommitInfo control = new();
        Grid table = control.FindControl<Grid>("tableLayout")!;
        table.IsVisible = true;
        control.FindControl<XhtmlTextBlock>("rtbxCommitMessage")!
            .SetXHTMLText(string.Join("\n", Enumerable.Repeat("Long commit message", 40)));
        Window window = new() { Width = 500, Height = 200, Content = control };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            ScrollViewer scroll = control.GetVisualDescendants().OfType<ScrollViewer>().First();
            scroll.AllowAutoHide.Should().BeFalse();
            scroll.Extent.Height.Should().BeGreaterThan(scroll.Viewport.Height);
            double scrollbarWidth = control.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.ScrollBar>()
                .Single(bar => bar.Orientation == Avalonia.Layout.Orientation.Vertical).Bounds.Width;
            scrollbarWidth.Should().BeGreaterThan(0);
            table.Bounds.Width.Should().BeApproximately(control.Bounds.Width - scrollbarWidth, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void XhtmlTextBlock_should_preserve_text_links_underlines_and_line_breaks()
    {
        XhtmlTextBlock block = new();
        string? activatedUri = null;
        block.LinkClicked += (_, e) => activatedUri = e.LinkUri;

        block.SetXHTMLText("Author: A &amp; B\n<u>tag</u>: <a href='gitext://gototag/v1'>v1</a><br/>done");

        block.GetPlainText().Should().Be($"Author: A & B{Environment.NewLine}tag: v1{Environment.NewLine}done");
        HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
        link.Content.Should().Be("v1");
        ToolTip.GetTip(link).Should().Be("gitext://gototag/v1");

        link.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        activatedUri.Should().Be("gitext://gototag/v1");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_copy_all_source_text_instead_of_embedded_object_placeholders()
    {
        XhtmlTextBlock block = new();
        block.SetTabStops([48, 96]);
        block.SetXHTMLText("Author:\t\t<a href='gitext://author'>A &amp; B</a><br/><u>Commit:</u>\t<a href='gitext://commit'>abc123</a>\nend");

        block.SelectAll();

        block.GetSelectionPlainText().Should().Be($"Author:\t\tA & B{Environment.NewLine}Commit:\tabc123{Environment.NewLine}end");
        block.GetSelectionPlainText().Should().Be(block.GetPlainText());
        block.GetSelectionPlainText().Should().NotContain("\uFFFC");
        block.SelectionStart.Should().Be(0);
        string layoutText = block.Inlines?.Text ?? throw new InvalidOperationException("Rendered XHTML must have inline text.");
        block.SelectionEnd.Should().Be(layoutText.Length);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(2, 4, "ad")]
    [TestCase(7, 11, "tail")]
    [TestCase(11, 7, "tail")]
    [TestCase(5, 6, "long caption")]
    [TestCase(5, 5, "")]
    public void XhtmlTextBlock_should_project_the_selected_inline_span_to_its_original_text(int start, int end, string expected)
    {
        XhtmlTextBlock block = new();
        block.SetXHTMLText("head <a href='gitext://author'>long caption</a> tail");
        block.SelectionStart = start;
        block.SelectionEnd = end;

        block.GetSelectionPlainText().Should().Be(expected);
        block.SelectionStart.Should().Be(start);
        block.SelectionEnd.Should().Be(end);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false)]
    [TestCase(true)]
    public void XhtmlTextBlock_should_copy_source_text_through_the_real_keyboard_route(bool useHeader)
    {
        WinFormsShims.IClipboard? previousClipboard = GetInstalledClipboard();
        WinFormsShims.IClipboard clipboard = Substitute.For<WinFormsShims.IClipboard>();
        WinFormsShims.ShimHost.Clipboard = clipboard;
        CommitInfoHeader header = new();
        XhtmlTextBlock block = useHeader ? header.GetTestAccessor().RevisionHeader : new();
        block.SetTabStops([], [], defaultTabInterval: 48);
        block.SetXHTMLText("Author:\t\t<a href='gitext://author'>A &amp; B</a><br/>Commit:\t<a href='gitext://commit'>abc123</a>");
        Window window = new() { Width = 500, Height = 100, Content = useHeader ? header : block };
        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.Focus().Should().BeTrue();
            block.SelectAll();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            window.KeyPress(Key.C, RawInputModifiers.Control, PhysicalKey.C, keySymbol: "c");

            clipboard.Received().SetText($"Author:\t\tA & B{Environment.NewLine}Commit:\tabc123");
            clipboard.DidNotReceive().SetText(Arg.Is<string>(text => text.Contains('\uFFFC')));
            block.ClearSelection();
            clipboard.ClearReceivedCalls();
            window.KeyPress(Key.C, RawInputModifiers.Control, PhysicalKey.C, keySymbol: "c");
            clipboard.DidNotReceive().SetText(Arg.Any<string>());
        }
        finally
        {
            window.Close();
            // Restore even an absent test-host service; the public setter's annotation cannot express that state.
            WinFormsShims.ShimHost.Clipboard = previousClipboard!;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_copy_source_text_through_the_framework_context_flyout_command()
    {
        WinFormsShims.IClipboard? previousClipboard = GetInstalledClipboard();
        WinFormsShims.IClipboard clipboard = Substitute.For<WinFormsShims.IClipboard>();
        WinFormsShims.ShimHost.Clipboard = clipboard;
        XhtmlTextBlock block = new();
        block.SetTabStops([48, 96]);
        block.SetXHTMLText("Author:\t\t<a href='gitext://author'>A &amp; B</a>");
        Window window = new() { Width = 400, Height = 100, Content = block };
        try
        {
            window.Show();
            // Borrow only for this test: the port supplies its own menus and keeps the XHTML style key.
            block.TryFindResource(typeof(SelectableTextBlock), block.ActualThemeVariant, out object? theme).Should().BeTrue();
            block.Theme = theme.Should().BeOfType<ControlTheme>().Subject;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.Focus().Should().BeTrue();
            block.SelectAll();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            MenuFlyout flyout = block.ContextFlyout.Should().BeOfType<MenuFlyout>().Subject;
            flyout.ShowAt(block);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            MenuItem copy = flyout.Items.OfType<MenuItem>().Single();
            copy.Command.Should().NotBeNull();
            copy.Command!.CanExecute(copy.CommandParameter).Should().BeTrue();

            copy.Command.Execute(copy.CommandParameter);

            clipboard.Received().SetText("Author:\t\tA & B");
            clipboard.DidNotReceive().SetText(Arg.Is<string>(text => text.Contains('\uFFFC')));
            flyout.Hide();
        }
        finally
        {
            window.Close();
            // Restore even an absent test-host service; the public setter's annotation cannot express that state.
            WinFormsShims.ShimHost.Clipboard = previousClipboard!;
        }
    }

    [AvaloniaTest]
    public void XhtmlTextBlock_should_preserve_source_tab_stop_width()
    {
        XhtmlTextBlock block = new();
        block.SetTabStops([80, 81]);

        block.SetXHTMLText("Author:\t\tA very long author identity");

        block.MinWidth.Should().BeGreaterThan(81);
        block.GetPlainText().Should().Be("Author:\t\tA very long author identity");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_place_links_at_absolute_tab_stops_on_each_line()
    {
        XhtmlTextBlock block = new();
        block.SetXHTMLText("Author:\t\t<a href='gitext://author'>author</a><br/>Commit:\t<a href='gitext://commit'>commit</a>");
        block.SetTabStops([80, 90, 100]);
        Window window = new() { Width = 300, Height = 100, Content = block };
        window.Show();

        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            HyperlinkButton[] links = [.. block.GetVisualDescendants().OfType<HyperlinkButton>()];
            links.Should().HaveCount(2);
            links[0].TranslatePoint(new Point(0, 0), block)!.Value.X.Should().BeApproximately(90, 1);
            links[1].TranslatePoint(new Point(0, 0), block)!.Value.X.Should().BeApproximately(80, 1);
            block.GetPlainText().Should().Be($"Author:\t\tauthor{Environment.NewLine}Commit:\tcommit");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_links_should_follow_the_native_light_and_dark_foregrounds()
    {
        foreach ((ThemeVariant theme, Color expected) in new[]
        {
            (ThemeVariant.Light, Color.Parse("#0066CC")),
            (ThemeVariant.Dark, Color.Parse("#FFFFFF")),
        })
        {
            XhtmlTextBlock block = new();
            block.SetXHTMLText("<a href='gitext://commit'>commit</a>");
            Window window = new() { Width = 200, Height = 40, RequestedThemeVariant = theme, Content = block };
            window.Show();

            try
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
                link.Foreground.Should().BeOfType<SolidColorBrush>().Which.Color.Should().Be(expected);
            }
            finally
            {
                window.Close();
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_apply_the_source_RichEdit_content_edge_allowance_per_instance()
    {
        XhtmlTextBlock block = new();
        block.SetTabStops([80, 81]);
        block.SetXHTMLText("Author:\tName");
        double baseline = block.MinWidth;
        double originalAllowance = block.NativeContentOverhang;

        block.NativeContentOverhang = 1;
        block.SetXHTMLText("Author:\tName");

        block.MinWidth.Should().Be(baseline - originalAllowance + 1);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_measure_native_content_width_separately_from_rendered_tabs()
    {
        XhtmlTextBlock block = new();
        block.SetTabStops([48, 96], [80, 81]);
        block.SetXHTMLText("Author:\t\tName");
        double sourceWidth = block.MinWidth;
        block.Width.Should().Be(sourceWidth);

        block.SetTabStops([48, 96]);

        block.MinWidth.Should().BeGreaterThan(sourceWidth);
        block.GetPlainText().Should().Be("Author:\t\tName");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_skip_width_tab_stops_behind_the_current_text_advance()
    {
        XhtmlTextBlock block = new() { NativeContentOverhang = 2 };
        double labelWidth = WinFormsTextMeasurer.Measure(block, "Commit hash:");
        int firstStop = (int)Math.Ceiling(labelWidth) + 10;
        int secondStop = firstStop + 10;
        block.SetTabStops([1, firstStop, secondStop], [1, firstStop, secondStop]);

        block.SetXHTMLText("Commit hash:\tvalue");

        block.Width.Should().Be(Math.Ceiling(firstStop + WinFormsTextMeasurer.Measure(block, "value") + 2));
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_continue_default_tab_intervals_after_a_long_translated_label()
    {
        const string label = "A long translated commit-header label that exceeds four half-inch intervals:";
        XhtmlTextBlock block = new() { NativeContentOverhang = 2 };
        block.SetTabStops([], [], defaultTabInterval: 48);
        block.SetXHTMLText($"{label}\t<a href='gitext://value'>value</a>");
        Window window = new() { Width = 1000, Height = 100, Content = block };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            double nativeLabelWidth = WinFormsTextMeasurer.Measure(block, label);
            double nativeStop = (Math.Floor(nativeLabelWidth / 48) + 1) * 48;
            block.Width.Should().Be(Math.Ceiling(nativeStop + WinFormsTextMeasurer.Measure(block, "value") + 2));
            TextLayout labelLayout = new(label, new Typeface(block.FontFamily, block.FontStyle, block.FontWeight), block.FontSize, foreground: null);
            double renderedLabelWidth = WinFormsRichEditTextMeasurer.TryGetCharacterAdvances(block, label, out int[] advances)
                ? advances[^1]
                : labelLayout.WidthIncludingTrailingWhitespace;
            double renderedStop = (Math.Floor(renderedLabelWidth / 48) + 1) * 48;
            HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            link.TranslatePoint(new Point(0, 0), block)!.Value.X.Should().BeApproximately(renderedStop, 1);
            renderedStop.Should().BeGreaterThan(192);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false)]
    [TestCase(true)]
    public void XhtmlTextBlock_should_refresh_contents_metrics_when_the_inherited_font_changes(bool bold)
    {
        const string label = "A translated commit-header label:";
        const string value = "A long author identity";
        XhtmlTextBlock block = new() { NativeContentOverhang = 2 };
        block.SetTabStops([], [], defaultTabInterval: 48);
        block.SetXHTMLText($"{label}\t<a href='gitext://author'>{value}</a>");
        Window window = new() { Width = 1000, Height = 100, Content = block };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            string plainText = block.GetPlainText();
            double previousWidth = block.Width;
            block.SelectionStart = 2;
            block.SelectionEnd = 5;
            if (bold)
            {
                window.FontWeight = FontWeight.Bold;
            }
            else
            {
                window.FontSize = 20;
            }

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            double labelWidth = WinFormsTextMeasurer.Measure(block, label);
            double nativeStop = (Math.Floor(labelWidth / 48) + 1) * 48;
            block.Width.Should().Be(Math.Ceiling(nativeStop + WinFormsTextMeasurer.Measure(block, value) + 2));
            block.Width.Should().BeGreaterThan(previousWidth);
            block.GetPlainText().Should().Be(plainText);
            block.SelectionStart.Should().Be(2);
            block.SelectionEnd.Should().Be(5);
            HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            link.Tag.Should().Be("gitext://author");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Capture_tree_should_preserve_XHTML_text_and_link_targets()
    {
        AvaloniaThemeResources.Apply(Application.Current!, ThemeModule.Settings);
        XhtmlTextBlock block = new() { Name = "RevisionInfo" };
        block.SetTabStops([48, 96]);
        block.SetXHTMLText("Contained in branches:<br/>branch:\t<a href='gitext://gotobranch/main'>main</a>");
        Window window = new() { Width = 200, Height = 50, Content = block };
        window.Show();

        try
        {
            CaptureNode node = new AvaloniaControlTreeReader(block, renderScale: 1)
                .ReadPrimary(block, new PixelSize(200, 50))
                .Root;

            node.Text.Should().Be("Contained in branches:\nbranch:\tmain|||gitext://gotobranch/main");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void CommitInfo_should_preserve_the_original_named_surfaces()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();

        accessor.Header.Should().NotBeNull();
        accessor.Avatar.Should().NotBeNull();
        accessor.CommitMessage.Name.Should().Be("rtbxCommitMessage");
        accessor.RevisionInfo.Name.Should().Be("RevisionInfo");
    }

    [AvaloniaTest]
    public void CommitInfoHeader_should_preserve_the_original_named_surfaces()
    {
        CommitInfoHeader header = new();
        CommitInfoHeader.TestAccessor accessor = header.GetTestAccessor();
        accessor.RevisionHeader.NativeContentOverhang.Should().Be(2,
            "the original borderless RichEdit retains one formatting pixel at each edge");
        accessor.RevisionHeader.NativeFormattingInset.Should().Be(1);
        accessor.RevisionHeader.Padding.Should().Be(new Thickness(0));
        ContextMenu contextMenu = new();

        header.SetContextMenuStrip(contextMenu);

        accessor.Avatar.Name.Should().Be("avatarControl");
        accessor.RevisionHeader.Name.Should().Be("rtbRevisionHeader");
        accessor.RevisionHeader.ContextMenu.Should().BeSameAs(contextMenu);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void CommitInfoHeader_should_follow_the_rendered_RichEdit_default_tab_interval()
    {
        CommitInfoHeader header = new();
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        block.SetXHTMLText("Author:\t\t<a href='gitext://author'>author</a><br/>Commit hash:\t<a href='gitext://commit'>commit</a>");
        Window window = new() { Width = 400, Height = 120, Content = header };
        window.Show();

        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            HyperlinkButton[] links = [.. block.GetVisualDescendants().OfType<HyperlinkButton>()];
            links.Should().HaveCount(2);
            links[0].TranslatePoint(new Point(0, 0), block)!.Value.X.Should().Be(97);
            links[1].TranslatePoint(new Point(0, 0), block)!.Value.X.Should().Be(97);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void XhtmlTextBlock_should_inset_text_selection_and_links_without_moving_its_background_or_frame()
    {
        XhtmlTextBlock block = new()
        {
            Width = 400,
            Height = 80,
            Margin = new Thickness(4),
            NativeContentOverhang = 2,
            Background = new SolidColorBrush(Color.Parse("#132F45")),
            SelectionBrush = new SolidColorBrush(Color.Parse("#CC2277")),
        };
        block.SetTabStops([48, 96]);
        block.SetXHTMLText("Author:\t\t<a href='gitext://author'>author</a><br/><u>Commit hash:</u>\tcommit");

        // Preserve negative glyph bearings on every backend rather than clipping the baseline at x=0.
        // This guard belongs to the fixture; the source-shaped control origin remains unchanged.
        Window window = new() { Width = 408, Height = 88, Background = block.Background, Content = block };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.SelectionStart = 0;
            block.SelectionEnd = 6;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            Point linkOrigin = link.TranslatePoint(new Point(0, 0), block)!.Value;
            Rect bounds = block.Bounds;
            double minimumWidth = block.MinWidth;
            string plainText = block.GetPlainText();
            using WriteableBitmap before = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The XHTML frame is unavailable.");

            block.NativeFormattingInset = 1;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            using WriteableBitmap after = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The inset XHTML frame is unavailable.");
            using MemoryStream beforeBytes = new();
            using MemoryStream afterBytes = new();
            before.Save(beforeBytes, PngBitmapEncoderOptions.Default);
            after.Save(afterBytes, PngBitmapEncoderOptions.Default);
            using SKBitmap beforePixels = SKBitmap.Decode(beforeBytes.ToArray());
            using SKBitmap afterPixels = SKBitmap.Decode(afterBytes.ToArray());
            string diagnosticsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "HeaderProbe", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(diagnosticsDirectory);
            before.Save(Path.Combine(diagnosticsDirectory, "formatting-before.png"), PngBitmapEncoderOptions.Default);
            after.Save(Path.Combine(diagnosticsDirectory, "formatting-after.png"), PngBitmapEncoderOptions.Default);
            TestContext.Progress.WriteLine($"formattingFrames={diagnosticsDirectory} beforeFormat={before.Format}/{before.AlphaFormat} afterFormat={after.Format}/{after.AlphaFormat} blockBounds={block.Bounds} windowBounds={window.Bounds}");
            foreach (int row in new[] { 0, 5, 15, beforePixels.Height - 1 })
            {
                TestContext.Progress.WriteLine($"row={row} beforeLeft={beforePixels.GetPixel(0, row)} beforeRight={beforePixels.GetPixel(beforePixels.Width - 1, row)} afterLeft={afterPixels.GetPixel(0, row)} afterRight={afterPixels.GetPixel(afterPixels.Width - 1, row)}");
            }

            List<SKColor> expected = [];
            List<SKColor> actual = [];
            for (int y = 0; y < beforePixels.Height; y++)
            {
                afterPixels.GetPixel(0, y).Should().Be(beforePixels.GetPixel(0, y),
                    "the unchanged frame guard must retain its actual background pixels");
                for (int x = 0; x < beforePixels.Width - 1; x++)
                {
                    expected.Add(beforePixels.GetPixel(x, y));
                    actual.Add(afterPixels.GetPixel(x + 1, y));
                }
            }

            actual.Should().Equal(expected, "text, underline, selection and embedded links share the real formatting origin");
            block.Bounds.Should().Be(bounds);
            block.MinWidth.Should().Be(minimumWidth);
            block.Padding.Should().Be(new Thickness(0));
            block.GetPlainText().Should().Be(plainText);
            block.GetSelectionPlainText().Should().Be("Author");
            link.TranslatePoint(new Point(0, 0), block)!.Value.Should().Be(linkOrigin + new Vector(1, 0));

            window.Width = 450;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            link.TranslatePoint(new Point(0, 0), block)!.Value.Should().Be(linkOrigin + new Vector(1, 0),
                "rearranging the same text must not accumulate formatting insets");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void CommitInfoHeader_should_hit_test_pointer_selection_at_the_native_formatting_origin()
    {
        CommitInfoHeader header = new();
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        block.SetXHTMLText("MMMMMM");
        Window window = new() { Width = 500, Height = 100, Content = header };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Rect startGlyph = block.TextLayout.HitTestTextPosition(2);
            Point start = new(startGlyph.X + (startGlyph.Width / 2) - 0.25 + block.NativeFormattingInset,
                startGlyph.Y + (startGlyph.Height / 2));
            Point startInWindow = block.TranslatePoint(start, window)!.Value;
            window.MouseDown(startInWindow, MouseButton.Left);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.SelectionStart.Should().Be(2);
            block.SelectionEnd.Should().Be(2);

            Rect endGlyph = block.TextLayout.HitTestTextPosition(4);
            Point end = new(endGlyph.X + (endGlyph.Width / 2) - 0.25 + block.NativeFormattingInset,
                endGlyph.Y + (endGlyph.Height / 2));
            Point endInWindow = block.TranslatePoint(end, window)!.Value;
            window.MouseMove(endInWindow, RawInputModifiers.LeftMouseButton);
            window.MouseUp(endInWindow, MouseButton.Left);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.SelectionStart.Should().Be(2);
            block.SelectionEnd.Should().Be(4);
            block.GetSelectionPlainText().Should().Be("MM");

            window.MouseDown(startInWindow, MouseButton.Right);
            window.MouseUp(startInWindow, MouseButton.Right);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.SelectionStart.Should().Be(2);
            block.SelectionEnd.Should().Be(4,
                "a right click inside the painted selection must preserve the copy range");
            block.GetPlainText().Should().Be("MMMMMM");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void CommitInfoHeader_should_activate_the_link_at_its_actual_inset_bounds()
    {
        CommitInfoHeader header = new();
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        block.SetXHTMLText("Author:\t\t<a href='gitext://author'>author</a>");
        string? activatedUri = null;
        block.LinkClicked += (_, e) => activatedUri = e.LinkUri;
        Window window = new() { Width = 500, Height = 100, Content = header };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            Point click = link.TranslatePoint(new Point(link.Bounds.Width / 2, link.Bounds.Height / 2), window)!.Value;
            block.AddHandler(InputElement.PointerPressedEvent, (_, e) => TracePointer("blockPressed", e),
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            block.AddHandler(InputElement.PointerReleasedEvent, (_, e) => TracePointer("blockReleased", e),
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            link.AddHandler(InputElement.PointerPressedEvent, (_, e) => TracePointer("linkPressed", e),
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            link.AddHandler(InputElement.PointerReleasedEvent, (_, e) => TracePointer("linkReleased", e),
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            TestContext.Progress.WriteLine($"beforePointer blockBounds={block.Bounds} linkBounds={link.Bounds} click={click} hit={window.InputHitTest(click)?.GetType().Name} selectedUri={block.SelectedLinkUri}");
            window.MouseMove(click);
            window.MouseDown(click, MouseButton.Left);
            TestContext.Progress.WriteLine($"afterMouseDown selectedUri={block.SelectedLinkUri} activatedUri={activatedUri}");
            window.MouseUp(click, MouseButton.Left);
            TestContext.Progress.WriteLine($"afterMouseUp selectedUri={block.SelectedLinkUri} activatedUri={activatedUri}");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            TestContext.Progress.WriteLine($"linkBounds={link.Bounds} click={click} activatedUri={activatedUri} selectedUri={block.SelectedLinkUri}");
            activatedUri.Should().Be("gitext://author");
            block.SelectedLinkUri.Should().Be("gitext://author");
            block.Padding.Should().Be(new Thickness(0));

            activatedUri = null;
            window.MouseDown(click, MouseButton.Right);
            window.MouseUp(click, MouseButton.Right);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.SelectedLinkUri.Should().Be("gitext://author",
                "right-click Copy link must retain the actual target even when Button handles the pointer event");
            activatedUri.Should().BeNull("a context-menu click must not activate the link");

            Rect labelGlyph = block.TextLayout.HitTestTextPosition(2);
            Point label = new(labelGlyph.X + block.NativeFormattingInset, labelGlyph.Y + (labelGlyph.Height / 2));
            Point labelInWindow = block.TranslatePoint(label, window)!.Value;
            window.MouseDown(labelInWindow, MouseButton.Left);
            window.MouseUp(labelInWindow, MouseButton.Left);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.SelectedLinkUri.Should().BeNull("clicking ordinary header text must clear the previous link target");

            void TracePointer(string stage, PointerEventArgs e)
            {
                TestContext.Progress.WriteLine($"stage={stage} source={e.Source?.GetType().Name}/{(e.Source as Control)?.Name} handled={e.Handled} pointer={e.GetPosition(block)} captured={e.Pointer.Captured?.GetType().Name} selectedUri={block.SelectedLinkUri}");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase("Parity Capture <parity@example.invalid>", "ceaece927abc97012d5cc36ea9dfba32321e9704")]
    [TestCase("Avalonia Contributor <avalonia@example.com>", "c932f21268731785cec9d37bfa3ff8f10485b46e")]
    public void CommitInfoHeader_should_measure_author_and_hash_rows_at_the_same_default_tab_origin(string author, string hash)
    {
        const string date = "9 months ago (1/2/2026 12:00:00 PM)";
        CommitInfoHeader header = new();
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        block.SetXHTMLText($"Author:\t\t<a href='mailto:parity@example.invalid'>{System.Net.WebUtility.HtmlEncode(author)}</a>"
            + $"<br/>Date:\t\t{date}<br/>Commit hash:\t{hash}");
        double longestValueWidth = new[] { author, date, hash }
            .Max(value => WinFormsTextMeasurer.Measure(block, value));

        block.Width.Should().Be(Math.Ceiling(96 + longestValueWidth + 2));
        block.MinWidth.Should().Be(block.Width);
        block.GetPlainText().Should().Contain($"Commit hash:\t{hash}");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase("a37bf89fffa0b681b81ac532fc2188f0d54c7a90")]
    [TestCase("54930791bbaca26f32883b799bbed65f81ec9150")]
    [TestCase("ceaece927abc97012d5cc36ea9dfba32321e9704")]
    [TestCase("c932f21268731785cec9d37bfa3ff8f10485b46e")]
    public void CommitInfoHeader_should_show_the_complete_hash_within_its_native_content_width(string hash)
    {
        CommitInfoHeader header = new();
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        block.SetXHTMLText($"Author:\t\t<a href='mailto:parity@example.invalid'>Parity Capture &lt;parity@example.invalid&gt;</a>"
            + $"<br/>Date:\t\t26 years ago (1/2/2001 1:00:00 PM)<br/>Commit hash:\t{hash}"
            + "<br/>Child:\t\t<a href='gitext://child'>Commit index</a><br/>Parent:\t\t<a href='gitext://parent'>351f58d7</a>");
        Window window = new() { Width = 500, Height = 150, Content = header };
        window.Show();

        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            string layoutText = block.Inlines?.Text
                ?? throw new InvalidOperationException("The rendered header must have inline layout text.");
            int hashStart = layoutText.IndexOf(hash, StringComparison.Ordinal);
            Rect finalCharacter = block.TextLayout.HitTestTextPosition(hashStart + hash.Length - 1);
            (block.NativeFormattingInset + finalCharacter.Right).Should().BeLessThanOrEqualTo(block.Bounds.Width - block.NativeFormattingInset,
                "the actual final hash character, not a separately measured string, must fit the native formatting rectangle");
            block.SelectionStart = hashStart + hash.Length - 4;
            block.SelectionEnd = hashStart + hash.Length;
            block.GetSelectionPlainText().Should().Be(hash[^4..]);
            block.Bounds.Width.Should().Be(block.MinWidth);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void CommitInfoHeader_should_keep_each_RTF_run_font_and_paragraph_tab_origin_at_eleven_points()
    {
        const string author = "Parity Capture <parity@example.invalid>";
        const string hash = "ceaece927abc97012d5cc36ea9dfba32321e9704";
        CommitInfoHeader header = new();
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        block.SetXHTMLText($"Author:\t\t<a href='mailto:parity@example.invalid'>{System.Net.WebUtility.HtmlEncode(author)}</a>"
            + $"<br/>Date:\t\t9 months ago (1/2/2026 12:00:00 PM)<br/>Commit hash:\t{hash}");
        Window window = new() { Width = 700, Height = 200, Content = header };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            // Resolve the AXAML font resource before exercising a live font change.
            // An unattached DynamicResource has not supplied its initial value yet.
            block.FontSize = (11d * 96) / 72;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.FontSize.Should().Be((11d * 96) / 72);
            HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            if (OperatingSystem.IsWindows())
            {
                // The native fixture has 225-twip plain text. SelectedRtf serializes its
                // author as \fs23, producing 230 twips; the two author tabs reach 144px.
                WinFormsRichEditTextMeasurer.GetFontSize(block).Should().Be(15);
                link.FontSize.Should().Be(11.5d * (96d / 72));
                link.TranslatePoint(new Point(0, 0), block)!.Value.X.Should().Be(145);
                block.Bounds.Width.Should().Be(419);
                block.Bounds.Height.Should().Be(60);
            }
            else
            {
                // A native RichEdit font is unavailable; verify the actual fallback only.
                link.FontSize.Should().Be(block.FontSize);
                block.Bounds.Width.Should().Be(block.MinWidth);
            }

            string layoutText = block.Inlines?.Text
                ?? throw new InvalidOperationException("The rendered header must have inline layout text.");
            int hashStart = layoutText.IndexOf(hash, StringComparison.Ordinal);
            Rect lastCharacter = block.TextLayout.HitTestTextPosition(hashStart + hash.Length - 1);
            (lastCharacter.Right + block.NativeFormattingInset).Should().BeLessThanOrEqualTo(block.Bounds.Width - block.NativeFormattingInset);
            block.GetPlainText().Should().Contain(author).And.Contain(hash);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase("A\u0301uthor")]
    [TestCase("مؤلف")]
    [TestCase("作者")]
    public void Native_header_advance_adapter_should_retain_framework_shaping_for_complex_text(string caption)
    {
        XhtmlTextBlock block = new() { NativeContentOverhang = 2 };
        block.SetTabStops([], [], 48);
        block.SetXHTMLText(caption);
        Window window = new() { Width = 400, Height = 100, Content = block };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            WinFormsRichEditTextMeasurer.TryGetCharacterAdvances(block, caption, out _).Should().BeFalse();
            block.GetPlainText().Should().Be(caption);
            block.TextLayout.WidthIncludingTrailingWhitespace.Should().BeGreaterThan(0);
            SolidColorBrush selectionForeground = new(Colors.Yellow);
            block.SelectionForegroundBrush = selectionForeground;
            block.SelectAll();
            block.GetSelectionPlainText().Should().Be(caption);

            // Selection changes invalidate TextBlock's inline runs until its next layout.
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.TextLayout.WidthIncludingTrailingWhitespace.Should().BeGreaterThan(0);
            block.TextLayout.TextLines.SelectMany(line => line.TextRuns).OfType<ShapedTextRun>()
                .Should().NotBeEmpty().And.OnlyContain(run => ReferenceEquals(run.Properties.ForegroundBrush, selectionForeground));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(0, 4, false)]
    [TestCase(2, 8, true)]
    [TestCase(19, 27, false)]
    [TestCase(36, 40, true)]
    public void Native_header_selection_should_change_only_the_selected_foreground_without_reshaping(int first, int last, bool reversed)
    {
        const string hash = "54930791bbaca26f32883b799bbed65f81ec9150";
        XhtmlTextBlock block = new() { NativeContentOverhang = 2, NativeFormattingInset = 1 };
        block.SetTabStops([], [], 48);
        block.SetXHTMLText($"Author:\t\t<a href='gitext://author'>author</a><br/>Commit hash:\t{hash}");
        Window window = new() { Width = 500, Height = 100, Content = block };
        window.Show();
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            SolidColorBrush normalForeground = new(Colors.Green);
            SolidColorBrush selectionForeground = new(Colors.Yellow);
            block.Foreground = normalForeground;
            block.SelectionForegroundBrush = selectionForeground;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            string layoutText = block.Inlines?.Text
                ?? throw new InvalidOperationException("The rendered header must have inline layout text.");
            int hashStart = layoutText.IndexOf(hash, StringComparison.Ordinal);
            Rect[] characterBounds = Enumerable.Range(0, hash.Length)
                .Select(index => block.TextLayout.HitTestTextPosition(hashStart + index)).ToArray();
            HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            Point linkOrigin = link.TranslatePoint(default, block)
                ?? throw new InvalidOperationException("The author link must be arranged within the header.");
            Rect bounds = block.Bounds;

            block.SelectionStart = hashStart + (reversed ? last : first);
            block.SelectionEnd = hashStart + (reversed ? first : last);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            AssertHashForeground(selected: true);
            block.GetSelectionPlainText().Should().Be(hash[first..last]);
            Enumerable.Range(0, hash.Length).Select(index => block.TextLayout.HitTestTextPosition(hashStart + index))
                .Should().Equal(characterBounds, "selection must reuse the exact native glyph advances and hit bounds");
            block.Bounds.Should().Be(bounds);
            link.TranslatePoint(default, block).Should().Be(linkOrigin);

            block.ClearSelection();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            AssertHashForeground(selected: false);
            Enumerable.Range(0, hash.Length).Select(index => block.TextLayout.HitTestTextPosition(hashStart + index))
                .Should().Equal(characterBounds);

            void AssertHashForeground(bool selected)
            {
                int inspectedCharacters = 0;
                foreach (TextLine line in block.TextLayout.TextLines)
                {
                    int position = line.FirstTextSourceIndex;
                    foreach (TextRun run in line.TextRuns)
                    {
                        if (run is ShapedTextRun shaped)
                        {
                            for (int index = 0; index < shaped.Length; index++)
                            {
                                int hashIndex = position + index - hashStart;
                                if (hashIndex >= 0 && hashIndex < hash.Length)
                                {
                                    IBrush expected = selected && hashIndex >= first && hashIndex < last
                                        ? selectionForeground : normalForeground;
                                    shaped.Properties.ForegroundBrush.Should().BeSameAs(expected,
                                        $"hash character {hashIndex} must use its actual selected or normal foreground");
                                    inspectedCharacters++;
                                }
                            }
                        }

                        position += run.Length;
                    }
                }

                inspectedCharacters.Should().Be(hash.Length);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    [Category("P8.6i.126")]
    [TestCase(true, false, 2)]
    [TestCase(true, true, 2)]
    [TestCase(false, false, 7)]
    [TestCase(false, true, 7)]
    public void CommitInfoHeader_should_auto_size_all_source_rows_and_the_visible_avatar(bool artificial, bool showAvatar, int rowCount)
    {
        bool originalShowAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
        AppSettings.ShowAuthorAvatarInCommitInfo = showAvatar;
        CommitInfoHeader header = new() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
        header.CommandClicked += (_, _) => { };
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        Window window = new() { Width = 700, Height = 400, Content = header };
        try
        {
            window.Show();
            header.ShowCommitInfo(CreateHeaderRevision(artificial), artificial ? null : [ObjectId.IndexId]);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            double lineHeight = WinFormsRichEditTextMeasurer.GetLineHeight(block);
            double contentsHeight = rowCount * lineHeight;
            block.GetPlainText().Count(character => character == '\n').Should().Be(rowCount - 1);
            block.TextLayout.TextLines.Should().HaveCount(rowCount);
            block.LineHeight.Should().Be(lineHeight);
            block.Height.Should().Be(contentsHeight);
            block.MinHeight.Should().Be(contentsHeight);
            block.Bounds.Height.Should().Be(contentsHeight);
            block.TextLayout.Height.Should().Be(contentsHeight);
            double.IsNaN(header.Height).Should().BeTrue("the original parent AutoSizes around the RichEdit client rectangle");
            header.Bounds.Height.Should().Be(Math.Max(contentsHeight, showAvatar ? AppSettings.AuthorImageSizeInCommitInfo : 0));
            header.GetTestAccessor().Avatar.IsVisible.Should().Be(showAvatar);
            foreach (HyperlinkButton link in block.GetVisualDescendants().OfType<HyperlinkButton>())
            {
                link.BorderThickness.Should().Be(new Thickness(0));
                link.Bounds.Height.Should().BeLessThanOrEqualTo(lineHeight);
                Point linkOrigin = link.TranslatePoint(new Point(0, 0), block)!.Value;
                linkOrigin.Y.Should().BeGreaterThanOrEqualTo(0);
                (linkOrigin.Y + link.Bounds.Height).Should().BeLessThanOrEqualTo(contentsHeight,
                    "the actual parent/child link control must fit inside its native client rectangle");
            }

            Point origin = block.TranslatePoint(new Point(0, 0), window)!.Value;
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The complete header frame is unavailable.");
            using MemoryStream bytes = new();
            frame.Save(bytes, PngBitmapEncoderOptions.Default);
            using SKBitmap pixels = SKBitmap.Decode(bytes.ToArray());
            int lastRowTop = (int)Math.Floor(origin.Y + ((rowCount - 1) * lineHeight));
            int lastRowBottom = (int)Math.Ceiling(origin.Y + contentsHeight);
            int lastColumn = (int)(origin.X + block.Bounds.Width - 1);
            SKColor background = pixels.GetPixel(lastColumn, lastRowTop);
            int paintedPixels = 0;
            for (int y = lastRowTop; y < lastRowBottom; y++)
            {
                for (int x = (int)origin.X + block.NativeFormattingInset; x < lastColumn; x++)
                {
                    if (pixels.GetPixel(x, y) != background)
                    {
                        paintedPixels++;
                    }
                }
            }

            paintedPixels.Should().BeGreaterThan(0, "the final Parent row must actually render inside the native formatting clip");
        }
        finally
        {
            window.Close();
            AppSettings.ShowAuthorAvatarInCommitInfo = originalShowAvatar;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    [Category("P8.6i.126")]
    public void CommitInfoHeader_should_remeasure_contents_height_on_font_changes_and_shrink_to_the_final_paragraph()
    {
        bool originalShowAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
        AppSettings.ShowAuthorAvatarInCommitInfo = false;
        CommitInfoHeader header = new() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
        header.CommandClicked += (_, _) => { };
        XhtmlTextBlock block = header.GetTestAccessor().RevisionHeader;
        Window window = new() { Width = 1200, Height = 500, Content = header };
        try
        {
            window.Show();
            header.ShowCommitInfo(CreateHeaderRevision(artificial: false), [ObjectId.IndexId]);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            double previousLineHeight = block.LineHeight;
            double previousHeight = block.Bounds.Height;
            block.FontSize *= 2;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            block.LineHeight.Should().Be(WinFormsRichEditTextMeasurer.GetLineHeight(block));
            block.LineHeight.Should().BeGreaterThan(previousLineHeight);
            block.Bounds.Height.Should().Be(7 * block.LineHeight);
            block.Bounds.Height.Should().BeGreaterThan(previousHeight);
            block.TextLayout.TextLines.Should().HaveCount(7);
            block.TextLayout.Height.Should().Be(block.Bounds.Height);

            header.ShowCommitInfo(CreateHeaderRevision(artificial: true), null);
            window.Width = 900;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            block.Bounds.Height.Should().Be(2 * block.LineHeight);
            block.TextLayout.TextLines.Should().HaveCount(2);
            header.Bounds.Height.Should().Be(block.Bounds.Height);
            block.Clear();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.Bounds.Height.Should().Be(block.LineHeight,
                "an empty RichEdit still contains its final paragraph and must shrink after a revision change");
            block.TextLayout.TextLines.Should().HaveCount(1);
            header.Bounds.Height.Should().Be(block.Bounds.Height);
            block.Width.Should().Be(block.NativeContentOverhang);
            block.MinWidth.Should().Be(block.NativeContentOverhang);

            block.FontSize /= 2;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            block.LineHeight.Should().Be(WinFormsRichEditTextMeasurer.GetLineHeight(block));
            block.Bounds.Height.Should().Be(block.LineHeight,
                "font changes must remeasure even an empty native paragraph after Clear");
        }
        finally
        {
            window.Close();
            AppSettings.ShowAuthorAvatarInCommitInfo = originalShowAvatar;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(9, 15)]
    [TestCase(11, 20)]
    [TestCase(18, 32)]
    [TestCase(22, 40)]
    public void RichEdit_line_height_should_use_the_native_control_font_conversion(int sizeInPoints, int nativeHeight)
    {
        XhtmlTextBlock block = new()
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = (sizeInPoints * 96d) / 72,
        };
        double height = WinFormsRichEditTextMeasurer.GetLineHeight(block);

        if (OperatingSystem.IsWindows())
        {
            height.Should().Be(nativeHeight,
                "the corresponding native RichEdit font/empty-paragraph probe records this HFONT line metric");
            if (sizeInPoints == 22)
            {
                WinFormsTextMeasurer.MeasureSize(block, "Mg").Height.Should().Be(41,
                    "TextRenderer's requested-font cache differs from RichEdit's 21.75pt selected native font at 22pt");
                height.Should().Be(40);
            }
        }
        else
        {
            height.Should().Be(Math.Ceiling(WinFormsTextMeasurer.MeasureSize(block, "Mg").Height),
                "off-Windows this remains an explicit platform-font fallback, not a native RichEdit parity claim");
        }

        height.Should().BeGreaterThan(0);
    }

    [AvaloniaTest]
    public void CommitInfo_should_render_the_hostless_revision_body_and_hide_a_null_revision()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        GitRevision revision = new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Subject = "Commit subject",
            Body = "Commit body",
        };

        control.Revision = revision;

        accessor.TableLayout.IsVisible.Should().BeTrue();
        accessor.CommitMessage.GetPlainText().Should().Be("Commit body");

        control.Revision = null;

        accessor.TableLayout.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void CommitInfo_should_project_the_configured_add_notes_shortcut_to_its_menu()
    {
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys(FormBrowse.HotkeySettingsName).Returns(
        [
            new HotkeyCommand((int)FormBrowse.Command.AddNotes, nameof(FormBrowse.Command.AddNotes))
            {
                KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.Shift | WinFormsShims.Keys.N,
            },
        ]);
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(false);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
        commands.GetService(typeof(ILinkFactory)).Returns(new LinkFactory());
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);

        CommitInfo control = new() { UICommandsSource = source };
        KeyGesture? gesture = control.GetTestAccessor().AddNoteMenuItem.InputGesture;

        gesture.Should().NotBeNull();
        gesture!.Key.Should().Be(Key.N);
        gesture.KeyModifiers.Should().Be(KeyModifiers.Control | KeyModifiers.Shift);
        loader.Received().LoadHotkeys(FormBrowse.HotkeySettingsName);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void CommitInfo_context_menu_should_toggle_the_original_settings()
    {
        bool originalLocal = AppSettings.CommitInfoShowContainedInBranchesLocal;
        bool originalRemote = AppSettings.CommitInfoShowContainedInBranchesRemote;
        bool originalRemoteIfNoLocal = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
        bool originalTags = AppSettings.CommitInfoShowContainedInTags;
        bool originalAnnotated = AppSettings.ShowAnnotatedTagsMessages;
        bool originalDerived = AppSettings.CommitInfoShowTagThisCommitDerivesFrom;
        try
        {
            CommitInfo.TestAccessor accessor = new CommitInfo().GetTestAccessor();

            accessor.ShowLocalBranchesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowRemoteBranchesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowRemoteBranchesIfNoLocalMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowTagsMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowAnnotatedTagMessagesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            accessor.ShowDerivedTagMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            AppSettings.CommitInfoShowContainedInBranchesLocal.Should().Be(!originalLocal);
            AppSettings.CommitInfoShowContainedInBranchesRemote.Should().Be(!originalRemote);
            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal.Should().Be(!originalRemoteIfNoLocal);
            AppSettings.CommitInfoShowContainedInTags.Should().Be(!originalTags);
            AppSettings.ShowAnnotatedTagsMessages.Should().Be(!originalAnnotated);
            AppSettings.CommitInfoShowTagThisCommitDerivesFrom.Should().Be(!originalDerived);
        }
        finally
        {
            AppSettings.CommitInfoShowContainedInBranchesLocal = originalLocal;
            AppSettings.CommitInfoShowContainedInBranchesRemote = originalRemote;
            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = originalRemoteIfNoLocal;
            AppSettings.CommitInfoShowContainedInTags = originalTags;
            AppSettings.ShowAnnotatedTagsMessages = originalAnnotated;
            AppSettings.CommitInfoShowTagThisCommitDerivesFrom = originalDerived;
        }
    }

    [AvaloniaTest]
    public void CommitInfo_should_preserve_menu_translation_keys_without_empty_text_keys()
    {
        CommitInfo control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(CommitInfo), "addNoteToolStripMenuItem", "Text", "Add &notes");
        translation.DidNotReceive().AddTranslationItem(nameof(CommitInfo), "RevisionInfo", "Text", Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(nameof(CommitInfo), "rtbxCommitMessage", "Text", Arg.Any<string>());
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

    private static GitRevision CreateHeaderRevision(bool artificial)
        => new(artificial ? ObjectId.WorkTreeId : ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Author = "Author",
            Committer = artificial ? "Author" : "Committer",
            AuthorUnixTime = 1650000000,
            CommitUnixTime = artificial ? 1650000000 : 1650000600,
            ParentIds = [ObjectId.Parse("abcdef1234567890abcdef1234567890abcdef12")],
        };
}
