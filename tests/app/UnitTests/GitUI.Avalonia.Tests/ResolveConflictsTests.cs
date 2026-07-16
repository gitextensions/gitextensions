using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ResolveConflictsTests
{
    private StubMessageBoxHost _messageBoxes = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        // The "no mergetool configured" warning (and similar) must not block the test run.
        _messageBoxes = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;
    }

    [AvaloniaTest]
    public void FormResolveConflicts_should_construct()
    {
        (IGitUICommands commands, _) = CreateCommands();

        FormResolveConflicts form = new(commands);

        form.ConflictedFiles.Should().NotBeNull();
        form.merge.IsDefault.Should().BeTrue("Merge is the dialog's accept button");
        form.ContextChooseLocal.Should().NotBeNull("the context menu items keep their WinForms names");
        form.customMergetool.IsVisible.Should().BeFalse("custom merge tools are not ported yet");
        form.fileHistoryToolStripMenuItem.IsVisible.Should().BeTrue("the file history shell is ported");
        form.progressBar.IsVisible.Should().BeFalse();
        form.FindControl<GitUI.UserControls.GotoUserManualControl>("gotoUserManualControl1").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void Load_should_list_the_conflicts_and_describe_the_selected_one()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetConflictsAsync(Arg.Any<string?>()).Returns(
        [
            CreateConflict("a.txt", hasBase: true, hasLocal: true, hasRemote: true),
            CreateConflict("b.txt", hasBase: true, hasLocal: false, hasRemote: true),
        ]);

        FormResolveConflicts form = new(commands);
        form.Show();

        form.ConflictedFiles.ItemCount.Should().Be(2);
        form.ConflictedFiles.SelectedIndex.Should().Be(0, "the first conflict is preselected like in WinForms");
        form.conflictDescription.Text.Should().Contain("changed both locally");
        form.localFileName.Text.Should().Be("a.txt");
        form.baseFileName.Text.Should().Be("a.txt");
        form.remoteFileName.Text.Should().Be("a.txt");
        form.labelLocalCurrent.Text.Should().Be("Local/current\u00A0(ours)", "a merge, not a rebase, is in progress");
        form.labelRemoteIncoming.Text.Should().Be("Remote/incoming\u00A0(theirs)");
    }

    [AvaloniaTest]
    public void Selecting_the_deleted_side_should_update_the_description()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetConflictsAsync(Arg.Any<string?>()).Returns(
        [
            CreateConflict("a.txt", hasBase: true, hasLocal: true, hasRemote: true),
            CreateConflict("b.txt", hasBase: true, hasLocal: false, hasRemote: true),
        ]);

        FormResolveConflicts form = new(commands);
        form.Show();

        form.ConflictedFiles.SelectedIndex = 1;

        form.conflictDescription.Text.Should().Contain("deleted locally");
        form.localFileName.Text.Should().Be("deleted");
        form.remoteFileName.Text.Should().Be("b.txt");
    }

    [AvaloniaTest]
    public void Selecting_multiple_conflicts_should_reduce_the_available_commands()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetConflictsAsync(Arg.Any<string?>()).Returns(
        [
            CreateConflict("a.txt", hasBase: true, hasLocal: true, hasRemote: true),
            CreateConflict("b.txt", hasBase: true, hasLocal: false, hasRemote: true),
        ]);

        FormResolveConflicts form = new(commands);
        form.Show();

        form.ContextOpenLocalWith.IsEnabled.Should().BeTrue("a single conflict is selected");

        form.ConflictedFiles.SelectAll();

        form.ContextOpenLocalWith.IsEnabled.Should().BeFalse("open/save commands need a single selection");
        form.ContextChooseLocal.IsEnabled.Should().BeTrue("choosing a side works for many files at once");
    }

    [AvaloniaTest]
    public void File_history_menu_item_should_open_the_selected_conflicts_history()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetConflictsAsync(Arg.Any<string?>()).Returns(
        [
            CreateConflict("a.txt", hasBase: true, hasLocal: true, hasRemote: true),
        ]);

        FormResolveConflicts form = new(commands);
        form.Show();
        try
        {
            form.fileHistoryToolStripMenuItem.IsVisible.Should().BeTrue();
            form.fileHistoryToolStripMenuItem.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.MenuItem.ClickEvent));

            commands.Received(1).StartFileHistoryDialog(form, "a.txt");
        }
        finally
        {
            form.Close();
        }
    }

    [Test]
    public void HandleMergeConflicts_should_open_the_resolve_dialog_for_a_conflicted_merge()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.InTheMiddleOfConflictedMerge().Returns(true);

        bool original = AppSettings.DontConfirmResolveConflicts;
        try
        {
            // Answer the "solve conflicts now?" question without showing it.
            AppSettings.DontConfirmResolveConflicts = true;

            bool result = MergeConflictHandler.HandleMergeConflicts(commands, owner: null, offerCommit: false, offerUpdateSubmodules: false);

            result.Should().BeTrue();
            commands.Received(1).StartResolveConflictsDialog(null, false);
        }
        finally
        {
            AppSettings.DontConfirmResolveConflicts = original;
        }
    }

    [Test]
    public void HandleMergeConflicts_should_do_nothing_for_a_clean_working_tree()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.InTheMiddleOfConflictedMerge().Returns(false);

        bool result = MergeConflictHandler.HandleMergeConflicts(commands, owner: null, offerCommit: false, offerUpdateSubmodules: false);

        result.Should().BeFalse();
        commands.DidNotReceive().StartResolveConflictsDialog(Arg.Any<WinFormsShims.IWin32Window?>(), Arg.Any<bool>());
        commands.DidNotReceive().UpdateSubmodules(Arg.Any<WinFormsShims.IWin32Window?>());
    }

    [Test]
    public void HandleMergeConflicts_should_offer_updating_submodules_when_asked_to()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.InTheMiddleOfConflictedMerge().Returns(false);

        MergeConflictHandler.HandleMergeConflicts(commands, owner: null, offerCommit: false, offerUpdateSubmodules: true);

        commands.Received(1).UpdateSubmodules(null);
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.InTheMiddleOfConflictedMerge().Returns(true);
        module.InTheMiddleOfRebase().Returns(false);
        module.InTheMiddleOfPatch().Returns(false);
        module.IsSubmodule(Arg.Any<string>()).Returns(false);
        module.GetConflictsAsync(Arg.Any<string?>()).Returns([]);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);
    }

    private static ConflictData CreateConflict(string filename, bool hasBase, bool hasLocal, bool hasRemote)
    {
        // A missing side has no file name, like the "git ls-files -u" parser produces.
        return new ConflictData(
            new ConflictedFileData(ObjectId.WorkTreeId, hasBase ? filename : null!),
            new ConflictedFileData(ObjectId.WorkTreeId, hasLocal ? filename : null!),
            new ConflictedFileData(ObjectId.WorkTreeId, hasRemote ? filename : null!));
    }

    /// <summary>Answers every message box with its default-affirmative result.</summary>
    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return buttons switch
            {
                WinFormsShims.MessageBoxButtons.YesNo or WinFormsShims.MessageBoxButtons.YesNoCancel => WinFormsShims.DialogResult.Yes,
                _ => WinFormsShims.DialogResult.OK,
            };
        }
    }
}
