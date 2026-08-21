using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Logging;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.Blame;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.Menus;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class BlameAndLogTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormBlame_should_construct_and_host_the_blame_control()
    {
        FormBlame form = new();

        form.GetTestAccessor().BlameControl.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormBlame_should_emit_its_original_title_translation_key()
    {
        FormBlame form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBlame), "$this", "Text", "File History");
    }

    [AvaloniaTest]
    public void FormBlame_should_fill_the_96_dpi_designer_client_without_a_synthetic_inset()
    {
        FormBlame form = new() { Width = 784, Height = 762 };
        form.Show();
        Dispatcher.UIThread.RunJobs();

        BlameControl blame = form.GetTestAccessor().BlameControl;
        blame.Bounds.Should().Be(new Rect(0, 0, 784, 762));
        blame.TabIndex.Should().Be(0);

        form.Close();
    }

    [AvaloniaTest]
    public void FormLog_should_construct_and_host_the_grid_files_and_viewer()
    {
        FormLog form = new();
        FormLog.TestAccessor accessor = form.GetTestAccessor();

        accessor.RevisionGrid.Should().NotBeNull();
        accessor.DiffFiles.Should().NotBeNull();
        accessor.DiffViewer.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormLog_should_emit_its_title_under_the_original_FormDiff_category()
    {
        FormLog form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        // The WinForms form's Designer Name is "FormDiff", so its only string lives there.
        translation.Received(1).AddTranslationItem("FormDiff", "$this", "Text", "Diff");
        translation.DidNotReceive().AddTranslationItem(nameof(FormLog), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [AvaloniaTest]
    public void FormLog_should_construct_with_ui_commands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormLog form = new(commands);

        form.GetTestAccessor().RevisionGrid.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormLog_should_preserve_the_96_dpi_designer_split_and_tab_order()
    {
        FormLog form = new() { Width = 750, Height = 529 };

        FormLog.TestAccessor accessor = form.GetTestAccessor();
        Grid root = (Grid)form.Content!;
        root.RowDefinitions[0].Height.Should().Be(new GridLength(205));
        root.RowDefinitions[1].Height.Should().Be(new GridLength(4));
        root.ColumnDefinitions.Should().BeEmpty();
        Grid lower = (Grid)accessor.DiffFiles.Parent!;
        lower.ColumnDefinitions[0].Width.Should().Be(new GridLength(188));
        lower.ColumnDefinitions[1].Width.Should().Be(new GridLength(4));
        root.Children.OfType<GridSplitter>().Should().ContainSingle();
        lower.Children.OfType<GridSplitter>().Should().ContainSingle();
        accessor.RevisionGrid.TabIndex.Should().Be(1);
        accessor.DiffFiles.TabIndex.Should().Be(0);
        accessor.DiffViewer.TabIndex.Should().Be(1);

        form.Close();
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_construct_with_two_tabs_and_wrap_enabled()
    {
        FormGitCommandLog form = new();
        FormGitCommandLog.TestAccessor accessor = form.GetTestAccessor();

        accessor.TabControl.Items.Should().HaveCount(2);
        accessor.LogItems.Should().NotBeNull();
        accessor.CommandCacheItems.Should().NotBeNull();
        accessor.WordWrap.IsChecked.Should().BeTrue();
        accessor.LogOutput.IsReadOnly.Should().BeTrue();
        accessor.CommandCacheOutput.IsReadOnly.Should().BeTrue();

        form.Close();
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_preserve_the_96_dpi_designer_layout_theme_and_context_surfaces()
    {
        FormGitCommandLog form = new();
        FormGitCommandLog.TestAccessor accessor = form.GetTestAccessor();
        Grid logSplit = form.FindControl<Grid>("splitContainer2")!;
        Grid cacheSplit = form.FindControl<Grid>("splitContainer1")!;
        TabItem logTab = form.FindControl<TabItem>("tabPageCommandLog")!;
        TabItem cacheTab = form.FindControl<TabItem>("tabPageCommandCache")!;

        form.Name.Should().Be(nameof(FormGitCommandLog));
        form.Width.Should().Be(659);
        form.Height.Should().Be(470);
        accessor.TabControl.Margin.Should().Be(new Thickness(0, 3, 0, 0));
        accessor.TabControl.TabIndex.Should().Be(1);
        logSplit.RowDefinitions.Select(row => row.Height).Should().Equal(
            GridLength.Star,
            new GridLength(4),
            new GridLength(150));
        cacheSplit.RowDefinitions.Select(row => row.Height).Should().Equal(
            new GridLength(183, GridUnitType.Star),
            new GridLength(4),
            new GridLength(231, GridUnitType.Star));
        logSplit.Margin.Should().Be(new Thickness(1, 0, 1, 1));
        cacheSplit.Margin.Should().Be(new Thickness(1, 0, 1, 1));
        logSplit.Children.OfType<GridSplitter>().Single().MinHeight.Should().Be(4);
        cacheSplit.Children.OfType<GridSplitter>().Single().MinHeight.Should().Be(4);
        logTab.Classes.Should().Contain("gitextensions-dialog-tab");
        cacheTab.Classes.Should().Contain("gitextensions-dialog-tab");
        accessor.LogItems.BorderThickness.Should().Be(default(Thickness));
        accessor.CommandCacheItems.BorderThickness.Should().Be(default(Thickness));
        accessor.LogItems.Focusable.Should().BeTrue();
        accessor.CommandCacheItems.Focusable.Should().BeTrue();
        accessor.LogItems.FontSize.Should().Be(12);
        accessor.CommandCacheItems.FontSize.Should().Be(12);
        accessor.LogOutput.FontSize.Should().Be(12);
        accessor.CommandCacheOutput.FontSize.Should().Be(12);
        string genericMonospace = OperatingSystem.IsWindows() ? "Courier New" : "DejaVu Sans Mono";
        accessor.LogItems.FontFamily.Name.Should().Be(genericMonospace);
        accessor.CommandCacheItems.FontFamily.Should().Be(accessor.LogItems.FontFamily);
        accessor.LogOutput.FontFamily.Should().Be(accessor.LogItems.FontFamily);
        accessor.CommandCacheOutput.FontFamily.Should().Be(accessor.LogItems.FontFamily);
        logTab.Background.Should().NotBeNull();
        cacheTab.Background.Should().Be(logTab.Background);
        accessor.LogOutput.Background.Should().NotBeNull();
        accessor.CommandCacheOutput.Background.Should().Be(accessor.LogOutput.Background);
        logTab.ContextMenu.Should().NotBeNull();
        cacheTab.ContextMenu.Should().NotBeNull();
        accessor.AlwaysOnTop.TabIndex.Should().Be(2);
        accessor.AlwaysOnTop.Width.Should().Be(101);
        accessor.AlwaysOnTop.Height.Should().Be(19);
        accessor.WordWrap.TabIndex.Should().Be(3);
        accessor.WordWrap.Width.Should().Be(84);
        accessor.WordWrap.Height.Should().Be(19);
        accessor.CaptureCallStacks.TabIndex.Should().Be(4);
        accessor.CaptureCallStacks.Width.Should().Be(124);
        accessor.CaptureCallStacks.Height.Should().Be(19);

        form.Close();
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_toggle_word_wrap_on_both_outputs()
    {
        FormGitCommandLog form = new();
        FormGitCommandLog.TestAccessor accessor = form.GetTestAccessor();

        accessor.WordWrap.IsChecked = false;

        accessor.LogOutput.TextWrapping.Should().Be(TextWrapping.NoWrap);
        accessor.CommandCacheOutput.TextWrapping.Should().Be(TextWrapping.NoWrap);

        form.Close();
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_toggle_always_on_top()
    {
        FormGitCommandLog form = new();
        FormGitCommandLog.TestAccessor accessor = form.GetTestAccessor();

        accessor.AlwaysOnTop.IsChecked = true;

        form.Topmost.Should().BeTrue();
        accessor.AlwaysOnTop.IsChecked.Should().BeTrue();

        form.Close();
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_keep_the_last_selection_when_it_was_at_the_end()
    {
        ListBox log = new();

        FormGitCommandLog.TestAccessor.Refresh(log, ["a", "b", "c"]);
        log.SelectedIndex.Should().Be(2);

        FormGitCommandLog.TestAccessor.Refresh(log, ["a", "b", "c", "d"]);
        log.SelectedIndex.Should().Be(3);
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_restore_a_prior_middle_selection()
    {
        ListBox log = new();

        FormGitCommandLog.TestAccessor.Refresh(log, ["a", "b", "c"]);
        log.SelectedIndex = 0;

        FormGitCommandLog.TestAccessor.Refresh(log, ["x", "y", "z"]);

        log.SelectedIndex.Should().Be(0);
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_ignore_bubbled_list_selection_when_switching_to_the_cache_tab()
    {
        CommandLog.Clear();
        GitModule.GitCommandCache.Clear();
        GitModule.GitCommandCache.Add("status --porcelain=v2", "1 .M src/App.cs", string.Empty);
        ProcessOperation operation = CommandLog.LogProcessStart("git", "status --short");
        operation.LogProcessEnd(0);
        FormGitCommandLog form = new();
        form.Show();
        Dispatcher.UIThread.RunJobs();
        FormGitCommandLog.TestAccessor accessor = form.GetTestAccessor();

        Action selectCacheTab = () => accessor.TabControl.SelectedIndex = 1;

        selectCacheTab.Should().NotThrow();
        Dispatcher.UIThread.RunJobs();
        accessor.CommandCacheItems.ItemCount.Should().Be(1);
        accessor.CommandCacheItems.SelectedIndex.Should().Be(0);

        form.Close();
        CommandLog.Clear();
        GitModule.GitCommandCache.Clear();
    }

    [AvaloniaTest]
    public void FormGitCommandLog_should_emit_its_original_translation_keys()
    {
        FormGitCommandLog form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "$this", "Text", "Git Command Log");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "tabPageCommandLog", "Text", "Command log");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "tabPageCommandCache", "Text", "Command cache");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "chkAlwaysOnTop", "Text", "Always on top");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "chkWordWrap", "Text", "Word wrap");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "chkCaptureCallStacks", "Text", "Capture call stacks");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "mnuSaveToFile", "Text", "&Save to file");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "mnuCopyCommandLine", "Text", "&Copy full command line");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "mnuClear", "Text", "C&lear");
        translation.Received(1).AddTranslationItem(nameof(FormGitCommandLog), "tsmiClearCache", "Text", "C&lear");

        form.Close();
    }

    [AvaloniaTest]
    public void ToolsMenu_should_expose_the_git_command_log_item_with_its_accelerator()
    {
        ToolsToolStripMenuItem menu = new();
        MenuItem item = menu.GetTestAccessor().GitCommandLogMenuItem;

        item.Should().NotBeNull();
        item.Header.Should().Be("Git _command log");
        item.InputGesture.Should().Be(new KeyGesture(Key.F12));
    }

    [AvaloniaTest]
    public void ToolsMenu_should_emit_the_git_command_log_key_under_FormBrowse()
    {
        ToolsToolStripMenuItem menu = new();
        ITranslation translation = Substitute.For<ITranslation>();

        ((ITranslate)menu).AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem("FormBrowse", "gitcommandLogToolStripMenuItem", "Text", "Git &command log");
    }
}
