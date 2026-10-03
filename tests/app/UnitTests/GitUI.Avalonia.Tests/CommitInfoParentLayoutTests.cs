using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommitInfo;
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CommitInfoParentLayoutTests
{
    private const int SourceTableWidth = 472;
    private const int SourceHeaderWidth = 260;
    private const int SourceHeaderHeight = 96;
    private const int SourceCommitMessageWidth = 440;
    private const int SourceMinimumHeight = 1;
    private const string PlainParagraphs = "first short line\nsecond line with descenders gjpq\nlast line";
    private const string WrappedParagraph = "first short line second line with descenders gjpq last line repeated ordinary words for genuine RichEdit word wrapping";

    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase("Segoe UI", 9)]
    [TestCase("Segoe UI", 11)]
    [TestCase("Segoe UI", 18)]
    [TestCase("Segoe UI", 22)]
    [TestCase("Consolas", 9)]
    [TestCase("Consolas", 11)]
    [TestCase("Consolas", 18)]
    [TestCase("Consolas", 22)]
    public void CommitInfo_parent_should_use_contents_rows_and_source_cached_anchors(string family, int points)
    {
        WithConfiguredFonts(family, points, (control, window) =>
        {
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            SetConstructorHeaderBounds(accessor);
            accessor.CommitMessage.SetXHTMLText(PlainParagraphs);
            accessor.RevisionInfo.SetXHTMLText(PlainParagraphs);
            RunLayout(window);
            AssertParentLayout(control);
            accessor.CommitMessage.FontFamily.Name.Should().Be(family);
            accessor.RevisionInfo.FontFamily.Name.Should().Be(family);
            accessor.CommitMessage.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(points));
            accessor.RevisionInfo.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(points));
            accessor.TableLayout.Bounds.Width.Should().Be(SourceTableWidth);
            accessor.TableLayout.RowDefinitions[0].ActualHeight.Should().Be(SourceHeaderHeight + accessor.Header.Margin.Top + accessor.Header.Margin.Bottom);
            accessor.CommitMessage.Bounds.Width.Should().Be(SourceCommitMessageWidth);
            accessor.CommitMessage.MinWidth.Should().Be(SourceMinimumHeight);
            accessor.CommitMessage.Margin.Should().Be(new Thickness(8),
                "the source's public Margin is separate from DefaultLayout's cached anchor distances");
            double ordinaryBodyHeight = accessor.CommitMessage.Bounds.Height;
            double ordinaryRefsHeight = accessor.RevisionInfo.Bounds.Height;

            accessor.CommitMessage.SetXHTMLText(PlainParagraphs + "\n");
            accessor.RevisionInfo.SetXHTMLText(PlainParagraphs + "\n");
            RunLayout(window);
            AssertParentLayout(control);
            accessor.CommitMessage.Bounds.Height.Should().Be(ordinaryBodyHeight + accessor.CommitMessage.LineHeight);
            accessor.RevisionInfo.Bounds.Height.Should().Be(ordinaryRefsHeight + accessor.RevisionInfo.LineHeight);

            accessor.CommitMessage.SetXHTMLText("first <a href='https://example.invalid/source'>caption</a> line\nsecond line\nlast line");
            accessor.RevisionInfo.SetXHTMLText(accessor.CommitMessage.GetPlainText());
            RunLayout(window);
            AssertParentLayout(control);
            HyperlinkButton link = accessor.CommitMessage.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            Point? linkOrigin = link.TranslatePoint(default, accessor.CommitMessage);
            linkOrigin.Should().NotBeNull();
            (linkOrigin.GetValueOrDefault().Y + link.Bounds.Height).Should().BeLessThanOrEqualTo(accessor.CommitMessage.Bounds.Height);

            accessor.CommitMessage.Clear();
            accessor.RevisionInfo.Clear();
            RunLayout(window);
            AssertParentLayout(control);
            accessor.CommitMessageHeight.Should().Be((int)accessor.CommitMessage.LineHeight);
            accessor.RevisionInfoHeight.Should().Be((int)accessor.RevisionInfo.LineHeight);
            accessor.CommitMessage.Bounds.Height.Should().Be(accessor.CommitMessage.LineHeight + SourceMinimumHeight);
            accessor.RevisionInfo.Bounds.Height.Should().Be(accessor.RevisionInfo.LineHeight);
        });
    }

    [AvaloniaTest]
    [TestCase("Segoe UI", 9)]
    [TestCase("Segoe UI", 11)]
    [TestCase("Segoe UI", 18)]
    [TestCase("Segoe UI", 22)]
    [TestCase("Consolas", 9)]
    [TestCase("Consolas", 11)]
    [TestCase("Consolas", 18)]
    [TestCase("Consolas", 22)]
    public void CommitInfo_parent_should_rewrap_grow_shrink_and_reserve_only_actual_vertical_scrollbar_space(string family, int points)
    {
        WithConfiguredFonts(family, points, (control, window) =>
        {
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            SetConstructorHeaderBounds(accessor);
            accessor.CommitMessage.SetXHTMLText(WrappedParagraph);
            accessor.RevisionInfo.SetXHTMLText(WrappedParagraph);
            RunLayout(window);
            AssertParentLayout(control);
            int wideRows = GetLayout(accessor.CommitMessage).TextLines.Count;
            int wideHeight = accessor.CommitMessageHeight;
            accessor.CommitMessage.SelectionStart = 3;
            accessor.CommitMessage.SelectionEnd = 11;

            window.Width = 240;
            RunLayout(window);
            AssertParentLayout(control);
            accessor.TableLayout.Bounds.Width.Should().Be(SourceHeaderWidth + accessor.Header.Margin.Left + accessor.Header.Margin.Right);
            GetLayout(accessor.CommitMessage).TextLines.Count.Should().BeGreaterThan(wideRows);
            AssertSelection(accessor.CommitMessage);
            ScrollViewer scroll = control.GetVisualDescendants().OfType<ScrollViewer>().First();
            scroll.Extent.Width.Should().BeGreaterThan(scroll.Viewport.Width);

            window.Width = 912;
            RunLayout(window);
            AssertParentLayout(control);
            accessor.TableLayout.Bounds.Width.Should().Be(912);
            GetLayout(accessor.CommitMessage).TextLines.Count.Should().BeLessThan(wideRows);
            AssertSelection(accessor.CommitMessage);

            window.Width = SourceTableWidth;
            RunLayout(window);
            AssertParentLayout(control);
            GetLayout(accessor.CommitMessage).TextLines.Should().HaveCount(wideRows);
            accessor.CommitMessageHeight.Should().Be(wideHeight);

            AppSettings.CommitFont = new WinFormsShims.Font(family, points * 2);
            AppSettings.Font = new WinFormsShims.Font(family, points * 2);
            AvaloniaFontSettings.ApplyAppSettings();
            RunLayout(window);
            AssertParentLayout(control);
            accessor.CommitMessageHeight.Should().BeGreaterThan(wideHeight);
            AssertSelection(accessor.CommitMessage);

            AppSettings.CommitFont = new WinFormsShims.Font(family, points);
            AppSettings.Font = new WinFormsShims.Font(family, points);
            AvaloniaFontSettings.ApplyAppSettings();
            RunLayout(window);
            AssertParentLayout(control);
            accessor.CommitMessageHeight.Should().Be(wideHeight);
            AssertSelection(accessor.CommitMessage);

            window.Height = 120;
            RunLayout(window);
            AssertParentLayout(control);
            ScrollBar scrollbar = scroll.GetVisualDescendants().OfType<ScrollBar>().Single(bar => bar.Orientation == Orientation.Vertical);
            scrollbar.Bounds.Width.Should().BeGreaterThan(0);
            accessor.TableLayout.Bounds.Width.Should().Be(control.Bounds.Width - scrollbar.Bounds.Width);
            scroll.Extent.Height.Should().BeGreaterThan(scroll.Viewport.Height);

            window.Height = 900;
            RunLayout(window);
            AssertParentLayout(control);
            accessor.TableLayout.Bounds.Width.Should().Be(SourceTableWidth);
            accessor.CommitMessageHeight.Should().Be(wideHeight);
            AssertSelection(accessor.CommitMessage);
        });
    }

    [AvaloniaTest]
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void CommitInfo_parent_should_keep_header_AutoSize_when_actual_revision_or_avatar_changes(bool artificial, bool showAvatar)
    {
        bool previousShowAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
        try
        {
            AppSettings.ShowAuthorAvatarInCommitInfo = showAvatar;
            WithConfiguredFonts("Segoe UI", 9, (control, window) =>
            {
                CommitInfo.TestAccessor accessor = control.GetTestAccessor();
                accessor.Header.CommandClicked += (_, _) => { };
                accessor.Header.ShowCommitInfo(CreateRevision(artificial), artificial ? null : [ObjectId.IndexId]);
                accessor.CommitMessage.SetXHTMLText(PlainParagraphs);
                accessor.RevisionInfo.SetXHTMLText("refs");
                RunLayout(window);
                AssertParentLayout(control);
                accessor.Header.GetTestAccessor().Avatar.IsVisible.Should().Be(showAvatar);
                double firstHeaderHeight = accessor.Header.Bounds.Height;
                double bodyHeight = accessor.CommitMessage.Bounds.Height;

                accessor.Header.ShowCommitInfo(CreateRevision(!artificial), artificial ? [ObjectId.IndexId] : null);
                RunLayout(window);
                AssertParentLayout(control);
                accessor.Header.Bounds.Height.Should().NotBe(firstHeaderHeight,
                    "the real source header's rows, rather than a fixed parent height, own row zero");
                accessor.CommitMessage.Bounds.Height.Should().Be(bodyHeight);
                double.IsNaN(accessor.Header.Height).Should().BeTrue();
            });
        }
        finally
        {
            AppSettings.ShowAuthorAvatarInCommitInfo = previousShowAvatar;
        }
    }

    [AvaloniaTest]
    public void CommitInfo_parent_should_preserve_body_link_activation_and_plain_selection_after_resize()
    {
        WithConfiguredFonts("Segoe UI", 9, (control, window) =>
        {
            IGitModule module = Substitute.For<IGitModule>();
            module.IsValidGitWorkingDir().Returns(false);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.GetService(typeof(ILinkFactory)).Returns(new LinkFactory());
            IHotkeySettingsLoader hotkeys = Substitute.For<IHotkeySettingsLoader>();
            hotkeys.LoadHotkeys(GitUI.CommandsDialogs.FormBrowse.HotkeySettingsName).Returns([]);
            commands.GetService(typeof(IHotkeySettingsLoader)).Returns(hotkeys);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            control.UICommandsSource = source;
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            accessor.CommitMessage.SetXHTMLText("start <a href='gitext://gotocommit/source'>caption</a> end");
            accessor.RevisionInfo.SetXHTMLText("refs");
            string? activatedUri = null;
            CommandEventArgs? command = null;
            accessor.CommitMessage.LinkClicked += (_, e) => activatedUri = e.LinkUri;
            control.CommandClicked += (_, e) => command = e;
            RunLayout(window);
            window.Width = 700;
            RunLayout(window);
            AssertParentLayout(control);
            HyperlinkButton link = accessor.CommitMessage.GetVisualDescendants().OfType<HyperlinkButton>().Single();
            Point? linkOrigin = link.TranslatePoint(default, window);
            linkOrigin.Should().NotBeNull();
            Point click = linkOrigin.GetValueOrDefault() + new Vector(link.Bounds.Width / 2, link.Bounds.Height / 2);
            window.MouseDown(click, MouseButton.Left);
            window.MouseUp(click, MouseButton.Left);
            activatedUri.Should().Be("gitext://gotocommit/source");
            command.Should().NotBeNull("the real source LinkFactory must dispatch the body's internal command");
            CommandEventArgs actualCommand = command
                ?? throw new AssertionException("The real body link must dispatch its internal command.");
            actualCommand.Command.Should().Be("gotocommit");
            actualCommand.Data.Should().Be("source");
            accessor.CommitMessage.SelectionStart = 1;
            accessor.CommitMessage.SelectionEnd = 4;
            accessor.CommitMessage.GetSelectionPlainText().Should().Be("tar");
            window.Width = 500;
            RunLayout(window);
            accessor.CommitMessage.GetSelectionPlainText().Should().Be("tar");
        });
    }

    [AvaloniaTest]
    public void CommitInfo_parent_should_settle_unchanged_overflow_when_a_line_wraps_only_after_scrollbar_reservation()
    {
        WithConfiguredFonts("Segoe UI", 9, (control, window) =>
        {
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            SetConstructorHeaderBounds(accessor);
            window.Height = 120;
            RunLayout(window);
            AssertParentLayout(control);
            ScrollBar scrollbar = control.GetVisualDescendants().OfType<ScrollBar>()
                .Single(bar => bar.Orientation == Orientation.Vertical);
            scrollbar.Bounds.Width.Should().BeGreaterThan(0);
            double fullWidth = control.Bounds.Width;
            double narrowWidth = accessor.TableLayout.Bounds.Width;
            narrowWidth.Should().Be(fullWidth - scrollbar.Bounds.Width);
            string boundaryText = FindBoundaryText(accessor.CommitMessage, fullWidth, narrowWidth);
            RunLayout(window);
            List<int> notifications = [];
            accessor.CommitMessage.ContentsResized += notifications.Add;
            accessor.CommitMessage.SetXHTMLText(boundaryText);
            RunLayout(window);
            TestContext.Out.WriteLine($"settled boundary length={boundaryText.Length} columns={fullWidth}/{narrowWidth} table={accessor.TableLayout.Bounds} body={accessor.CommitMessage.Bounds} family={accessor.CommitMessage.FontFamily.Name} fontSize={accessor.CommitMessage.FontSize} layoutWidth={GetLayout(accessor.CommitMessage).WidthIncludingTrailingWhitespace} rows={GetLayout(accessor.CommitMessage).TextLines.Count} notifications={string.Join(',', notifications)}");
            AssertParentLayout(control);
            GetLayout(accessor.CommitMessage).TextLines.Should().HaveCount(2);
            notifications.Should().ContainSingle().Which.Should().Be(accessor.CommitMessageHeight);
            control.IsMeasureValid.Should().BeTrue();
            control.IsArrangeValid.Should().BeTrue();
            Rect tableBounds = accessor.TableLayout.Bounds;
            Rect messageBounds = accessor.CommitMessage.Bounds;
            Rect refsBounds = accessor.RevisionInfo.Bounds;
            for (int pass = 0; pass < 5; pass++)
            {
                control.InvalidateMeasure();
                RunLayout(window);
                control.IsMeasureValid.Should().BeTrue("unchanged settled contents must not re-invalidate their own parent");
                control.IsArrangeValid.Should().BeTrue();
                accessor.TableLayout.Bounds.Should().Be(tableBounds);
                accessor.CommitMessage.Bounds.Should().Be(messageBounds);
                accessor.RevisionInfo.Bounds.Should().Be(refsBounds);
                notifications.Should().ContainSingle();
            }
        });
    }

    [AvaloniaTest]
    public void CommitInfo_parent_should_grow_async_linked_refs_after_its_one_line_row_was_arranged()
    {
        WithConfiguredFonts("Segoe UI", 9, (control, window) =>
        {
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            accessor.Header.ShowCommitInfo(CreateRevision(artificial: false), [ObjectId.IndexId]);
            accessor.CommitMessage.SetXHTMLText("commit subject");
            accessor.RevisionInfo.SetXHTMLText("loading refs");
            RunLayout(window);
            AssertParentLayout(control);
            accessor.RevisionInfoHeight.Should().Be((int)accessor.RevisionInfo.LineHeight);
            GetLayout(accessor.RevisionInfo).TextLines.Should().ContainSingle();
            ScrollViewer scroll = control.GetVisualDescendants().OfType<ScrollViewer>().First();

            // The portable resolved header font can make its preferred width exceed
            // the authored table width. Include the actual horizontal scrollbar's
            // viewport reservation rather than assuming one spare text row encloses it.
            double viewportReservation = control.Bounds.Height - scroll.Viewport.Height;
            window.Height = accessor.TableLayout.Bounds.Height + accessor.RevisionInfo.LineHeight + viewportReservation;
            RunLayout(window);
            TestContext.Out.WriteLine($"initial async refs table={accessor.TableLayout.Bounds} owner={control.Bounds} viewport={scroll.Viewport} extent={scroll.Extent} headerPreferred={accessor.Header.DesiredSize} lineHeight={accessor.RevisionInfo.LineHeight} reservation={viewportReservation}");
            scroll.Extent.Height.Should().BeLessThanOrEqualTo(scroll.Viewport.Height);

            const string refs = "Contained in branches:\n<a href='gitext://gotobranch/main'>main</a>\n"
                + "<a href='gitext://gotobranch/feature/parity'>feature/parity</a>\n\nContained in tags:\n"
                + "<a href='gitext://gototag/v1.0'>v1.0</a>\n\nDerives from tag: <a href='gitext://gototag/v1.0'>v1.0</a>";
            const int sourceParagraphCount = 8;
            List<int> notifications = [];
            accessor.RevisionInfo.ContentsResized += notifications.Add;

            // Only the refs change, after their previous finite row has settled. This
            // is the asynchronous Browse path, not a parent's manual measure/cache update.
            Dispatcher.UIThread.Post(() => accessor.RevisionInfo.SetXHTMLText(refs));
            RunLayout(window);
            AssertParentLayout(control);
            GetLayout(accessor.RevisionInfo).TextLines.Should().HaveCount(sourceParagraphCount);
            accessor.RevisionInfoHeight.Should().Be((int)(sourceParagraphCount * accessor.RevisionInfo.LineHeight));
            notifications.Should().ContainSingle().Which.Should().Be(accessor.RevisionInfoHeight);
            if (OperatingSystem.IsWindows())
            {
                accessor.RevisionInfoHeight.Should().Be(120,
                    "the native default9pt ContentsResized rectangle includes all eight15px paragraphs");
            }

            scroll.Extent.Height.Should().BeGreaterThan(scroll.Viewport.Height);
            ScrollBar scrollbar = scroll.GetVisualDescendants().OfType<ScrollBar>()
                .Single(bar => bar.Orientation == Orientation.Vertical);
            scrollbar.IsVisible.Should().BeTrue();
            scrollbar.Bounds.Width.Should().BeGreaterThan(0);
            accessor.TableLayout.Bounds.Width.Should().Be(Math.Max(
                control.Bounds.Width - scrollbar.Bounds.Width, accessor.Header.DesiredSize.Width));
            HyperlinkButton[] links = accessor.RevisionInfo.GetVisualDescendants().OfType<HyperlinkButton>().ToArray();
            links.Should().HaveCount(4, "the last tag link must not disappear from the actual visual tree");
            HyperlinkButton lastLink = links.Last();
            Point? lastLinkOrigin = lastLink.TranslatePoint(default, accessor.RevisionInfo);
            lastLinkOrigin.Should().NotBeNull();
            lastLinkOrigin.GetValueOrDefault().Y.Should().BeGreaterThanOrEqualTo(
                (sourceParagraphCount - 1) * accessor.RevisionInfo.LineHeight);
            (lastLinkOrigin.GetValueOrDefault().Y + lastLink.Bounds.Height)
                .Should().BeLessThanOrEqualTo(accessor.RevisionInfo.Bounds.Height);

            Rect tableBounds = accessor.TableLayout.Bounds;
            for (int pass = 0; pass < 5; pass++)
            {
                control.InvalidateMeasure();
                RunLayout(window);
                accessor.TableLayout.Bounds.Should().Be(tableBounds);
                GetLayout(accessor.RevisionInfo).TextLines.Should().HaveCount(sourceParagraphCount);
                notifications.Should().ContainSingle();
            }

            accessor.RevisionInfo.SetXHTMLText("refs complete");
            RunLayout(window);
            AssertParentLayout(control);
            GetLayout(accessor.RevisionInfo).TextLines.Should().ContainSingle();
            accessor.RevisionInfoHeight.Should().Be((int)accessor.RevisionInfo.LineHeight);
            scroll.Extent.Height.Should().BeLessThanOrEqualTo(scroll.Viewport.Height);
            accessor.TableLayout.Bounds.Width.Should().Be(Math.Max(control.Bounds.Width, accessor.Header.DesiredSize.Width));
        });
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void XhtmlTextBlock_should_report_every_native_contents_row_even_when_its_client_height_is_unchanged(bool message)
    {
        WithConfiguredFonts("Segoe UI", 9, (control, _) =>
        {
            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            XhtmlTextBlock block = message ? accessor.CommitMessage : accessor.RevisionInfo;
            double clientHeight = block.LineHeight;
            block.SetXHTMLText("first\nsecond\nthird\nfourth\nfifth\nsixth\nseventh\neighth");
            block.Measure(new Size(SourceTableWidth, clientHeight + block.Margin.Top + block.Margin.Bottom));
            block.DesiredSize.Height.Should().Be(clientHeight + block.Margin.Top + block.Margin.Bottom);
            block.ContentsHeight.Should().Be((int)(8 * block.LineHeight),
                "RichEdit's ContentsResized rectangle is independent of its existing finite client rectangle");
        });
    }

    [AvaloniaTest]
    public void Browse_capture_should_report_the_actual_source_anchored_body_client_and_outer_height()
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
        AppSettings.ShowSplitViewLayout = true;
        using FormBrowse form = new();
        try
        {
            form.Show();
            CommitInfo.TestAccessor accessor = form.RevisionInfo.GetTestAccessor();
            accessor.TableLayout.IsVisible = true;
            accessor.CommitMessage.SetXHTMLText("body with source cached anchors");
            accessor.RevisionInfo.SetXHTMLText("refs");
            for (int pass = 0; pass < 8; pass++)
            {
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
            }

            XhtmlTextBlock body = accessor.CommitMessage;
            body.IsEffectivelyVisible.Should().BeTrue();
            body.Bounds.Height.Should().Be(body.ContentsHeight + SourceMinimumHeight,
                "the actual source anchor rectangle already includes its minimum-size edge allowance");
            CaptureSurface surface = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height));
            CaptureNode capturedBody = EnumerateNodes(surface.Root).Single(node =>
                node.FieldName == "rtbxCommitMessage" && node.Text == body.GetPlainText());
            capturedBody.BoundsDip.Height.Should().Be((decimal)body.Bounds.Height);
            capturedBody.ClientSizeDip.Height.Should().Be((decimal)body.Bounds.Height,
                "a borderless source client is the real control height, not another synthetic outer pixel");
            capturedBody.BoundsPx.Height.Should().Be((int)Math.Round(body.Bounds.Height));
            capturedBody.ClientSizePx.Height.Should().Be(capturedBody.BoundsPx.Height);
        }
        finally
        {
            form.Close();
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    private static IEnumerable<CaptureNode> EnumerateNodes(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children)
        {
            foreach (CaptureNode descendant in EnumerateNodes(child))
            {
                yield return descendant;
            }
        }
    }

    private static void AssertParentLayout(CommitInfo control)
    {
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();
        TextLayout messageLayout = GetLayout(accessor.CommitMessage);
        TextLayout refsLayout = GetLayout(accessor.RevisionInfo);
        int contentsHeight = (int)Math.Ceiling(messageLayout.Height);
        int refsHeight = (int)Math.Ceiling(refsLayout.Height);
        accessor.CommitMessageHeight.Should().Be(contentsHeight);
        accessor.RevisionInfoHeight.Should().Be(refsHeight);
        accessor.CommitMessage.ContentsHeight.Should().Be(contentsHeight);
        accessor.RevisionInfo.ContentsHeight.Should().Be(refsHeight);
        accessor.TableLayout.RowDefinitions[0].Height.GridUnitType.Should().Be(GridUnitType.Auto);
        accessor.TableLayout.RowDefinitions[1].Height.GridUnitType.Should().Be(GridUnitType.Pixel);
        accessor.TableLayout.RowDefinitions[2].Height.GridUnitType.Should().Be(GridUnitType.Pixel);
        accessor.TableLayout.ColumnDefinitions[0].Width.GridUnitType.Should().Be(GridUnitType.Pixel);
        double headerRow = accessor.Header.Bounds.Height + accessor.Header.Margin.Top + accessor.Header.Margin.Bottom;
        double messageRow = contentsHeight + accessor.CommitMessage.Margin.Top + accessor.CommitMessage.Margin.Bottom;
        double refsRow = refsHeight + accessor.RevisionInfo.Margin.Top + accessor.RevisionInfo.Margin.Bottom;
        accessor.TableLayout.RowDefinitions[0].ActualHeight.Should().Be(headerRow);
        accessor.TableLayout.RowDefinitions[1].ActualHeight.Should().Be(messageRow);
        accessor.TableLayout.RowDefinitions[2].ActualHeight.Should().Be(refsRow);
        accessor.TableLayout.Bounds.Height.Should().Be(headerRow + messageRow + refsRow);
        accessor.CommitMessagePanel.Bounds.Width.Should().Be(accessor.TableLayout.Bounds.Width);
        accessor.CommitMessagePanel.Bounds.Height.Should().Be(messageRow);
        accessor.CommitMessage.Bounds.X.Should().Be(accessor.CommitMessage.Margin.Left);
        accessor.CommitMessage.Bounds.Y.Should().Be(accessor.CommitMessage.Margin.Top);
        accessor.CommitMessage.Bounds.Height.Should().Be(contentsHeight + SourceMinimumHeight);
        double anchorRight = SourceTableWidth - accessor.CommitMessage.Margin.Left - SourceCommitMessageWidth;
        double anchorBottom = accessor.CommitMessage.Margin.Bottom - SourceMinimumHeight;
        (accessor.CommitMessagePanel.Bounds.Width - accessor.CommitMessage.Bounds.Right).Should().Be(anchorRight);
        (accessor.CommitMessagePanel.Bounds.Height - accessor.CommitMessage.Bounds.Bottom).Should().Be(anchorBottom);
        accessor.RevisionInfo.Bounds.Height.Should().Be(refsHeight);
        accessor.RevisionInfo.Bounds.Width.Should().Be(accessor.TableLayout.Bounds.Width - accessor.RevisionInfo.Margin.Left - accessor.RevisionInfo.Margin.Right);
        accessor.RevisionInfo.Bounds.Y.Should().Be(headerRow + messageRow + accessor.RevisionInfo.Margin.Top);
        double.IsNaN(accessor.CommitMessage.Width).Should().BeTrue();
        double.IsNaN(accessor.CommitMessage.Height).Should().BeTrue();
        messageLayout.TextLines.Last().Height.Should().Be(accessor.CommitMessage.LineHeight);
        messageLayout.Height.Should().BeLessThanOrEqualTo(accessor.CommitMessage.Bounds.Height);
        refsLayout.Height.Should().BeLessThanOrEqualTo(accessor.RevisionInfo.Bounds.Height);
    }

    private static void AssertSelection(XhtmlTextBlock block)
    {
        block.SelectionStart.Should().Be(3);
        block.SelectionEnd.Should().Be(11);
        block.GetSelectionPlainText().Should().Be(WrappedParagraph[3..11]);
    }

    private static GitRevision CreateRevision(bool artificial)
        => new(artificial ? ObjectId.WorkTreeId : ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Author = "Author",
            Committer = artificial ? "Author" : "Committer",
            AuthorUnixTime = 1650000000,
            CommitUnixTime = artificial ? 1650000000 : 1650000600,
            ParentIds = [ObjectId.Parse("abcdef1234567890abcdef1234567890abcdef12")],
        };

    private static string FindBoundaryText(XhtmlTextBlock source, double fullWidth, double narrowWidth)
    {
        // Use the actual attached editor and the same column constraints as OnLayout.
        // An unattached clone does not share the source anchors or inline inheritance.
        string originalText = source.GetPlainText();
        try
        {
            for (int length = 1; length < 4096; length++)
            {
                string text = new('M', length);
                source.SetXHTMLText(text);
                source.Measure(new Size(fullWidth, double.PositiveInfinity));
                int wideLines = GetLayout(source).TextLines.Count;
                source.Measure(new Size(narrowWidth, double.PositiveInfinity));
                int narrowLines = GetLayout(source).TextLines.Count;
                if (wideLines == 1 && narrowLines == 2)
                {
                    TestContext.Out.WriteLine($"measured boundary length={length} columns={fullWidth}/{narrowWidth} family={source.FontFamily.Name} fontSize={source.FontSize} layoutWidth={GetLayout(source).WidthIncludingTrailingWhitespace} rows={wideLines}/{narrowLines}");
                    return text;
                }
            }
        }
        finally
        {
            source.SetXHTMLText(originalText);
        }

        throw new AssertionException("The actual resolved font must provide a one-line/two-line wrap boundary between the real viewport widths.");
    }

    private static TextLayout GetLayout(XhtmlTextBlock block)
        => block.TextLayout ?? throw new AssertionException("The source-shaped parent must retain its actual measured TextLayout.");

    private static void RunLayout(Window window)
    {
        // A rendered header/queued avatar update can invalidate its parent after the
        // first pass. Consume real framework work until the owned layout is valid; do
        // not call source handlers or overwrite the parent's cached heights in tests.
        for (int pass = 0; pass < 8; pass++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            if (window.Content is not CommitInfo control)
            {
                throw new AssertionException("The native-shaped parent fixture must own its CommitInfo control.");
            }

            CommitInfo.TestAccessor accessor = control.GetTestAccessor();
            Layoutable[] layout = [window, control, accessor.TableLayout, accessor.Header, accessor.CommitMessage, accessor.RevisionInfo];
            if (layout.All(element => element.IsMeasureValid && element.IsArrangeValid))
            {
                return;
            }
        }

        throw new AssertionException("The actual CommitInfo parent/header/body layout did not settle after eight framework passes.");
    }

    private static void SetConstructorHeaderBounds(CommitInfo.TestAccessor accessor)
    {
        // The native parent probe deliberately covers the constructor header, not a
        // rendered avatar-off Browse header. Match that authored state only here.
        accessor.Header.Width = SourceHeaderWidth;
        accessor.Header.Height = SourceHeaderHeight;
    }

    private static void WithConfiguredFonts(string family, int points, Action<CommitInfo, Window> verify)
    {
        WinFormsShims.Font previousUiFont = AppSettings.Font;
        WinFormsShims.Font previousCommitFont = AppSettings.CommitFont;
        Window? window = null;
        try
        {
            AppSettings.Font = new WinFormsShims.Font(family, points);
            AppSettings.CommitFont = new WinFormsShims.Font(family, points);
            AvaloniaFontSettings.ApplyAppSettings();
            CommitInfo control = new();
            control.GetTestAccessor().TableLayout.IsVisible = true;
            window = new Window { Width = SourceTableWidth, Height = 900, Content = control };
            window.Show();
            RunLayout(window);
            verify(control, window);
        }
        finally
        {
            window?.Close();
            AppSettings.Font = previousUiFont;
            AppSettings.CommitFont = previousCommitFont;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }
}
