using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    public void XhtmlTextBlock_should_apply_the_source_RichEdit_content_overhang_per_instance()
    {
        XhtmlTextBlock block = new();
        block.SetTabStops([80, 81]);
        block.SetXHTMLText("Author:\tName");
        double baseline = block.MinWidth;

        block.NativeContentOverhang = 1;
        block.SetXHTMLText("Author:\tName");

        block.MinWidth.Should().Be(baseline + 1);
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
            links[0].TranslatePoint(new Point(0, 0), block)!.Value.X.Should().BeApproximately(96, 1);
            links[1].TranslatePoint(new Point(0, 0), block)!.Value.X.Should().BeApproximately(96, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void CommitInfoHeader_should_show_the_complete_hash_within_its_native_content_width()
    {
        const string hash = "a37bf89fffa0b681b81ac532fc2188f0d54c7a90";
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
            TextLayout hashLayout = new(
                hash,
                new Typeface(block.FontFamily, block.FontStyle, block.FontWeight),
                block.FontSize,
                foreground: null,
                letterSpacing: block.LetterSpacing);

            (96 + hashLayout.WidthIncludingTrailingWhitespace).Should().BeLessThanOrEqualTo(block.Bounds.Width);
            block.Bounds.Width.Should().Be(block.MinWidth);
        }
        finally
        {
            window.Close();
        }
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
}
