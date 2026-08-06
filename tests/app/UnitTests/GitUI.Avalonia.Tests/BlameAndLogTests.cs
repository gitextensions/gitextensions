using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
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
