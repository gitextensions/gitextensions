using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class DiffPatchDialogTests
{
    private StubFolderPicker _folderPicker = null!;
    private StubMessageBoxHost _messageBoxes = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _folderPicker = new StubFolderPicker();
        _messageBoxes = new StubMessageBoxHost();
        WinFormsShims.ShimHost.FolderPicker = _folderPicker;
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;
    }

    [AvaloniaTest]
    public void Diff_and_patch_views_should_construct_through_their_designer_boundaries()
    {
        FormDiff diff = new();
        FormCompareToBranch compareToBranch = new();
        FormFormatPatch formatPatch = new();

        diff.FindControl<FileStatusList>("DiffFiles").Should().NotBeNull();
        diff.FindControl<GitUI.Editor.FileViewer>("DiffText").Should().NotBeNull();
        compareToBranch.FindControl<GitUI.UserControls.BranchSelector>("branchSelector").Should().NotBeNull();
        compareToBranch.FindControl<Button>("btnCompare").Should().NotBeNull();
        formatPatch.FindControl<RevisionGridControl>("RevisionGrid").Should().NotBeNull();
        formatPatch.FindControl<TextBox>("OutputPath").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void Diff_and_patch_views_should_render_their_complete_layouts()
    {
        Window[] forms = [new FormDiff(), new FormCompareToBranch(), new FormFormatPatch()];

        foreach (Window form in forms)
        {
            form.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                form.CaptureRenderedFrame().Should().NotBeNull();
                form.GetVisualDescendants().OfType<Button>().Should().Contain(button => button.Bounds.Width > 0);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void Diff_and_patch_views_should_preserve_source_and_runtime_dimensions_at_96_dpi()
    {
        FormDiff diff = new();
        FormCompareToBranch compareToBranch = new();
        FormFormatPatch formatPatch = new();

        diff.Width.Should().Be(1042);
        diff.Height.Should().Be(685);
        diff.FindControl<Button>("btnSwap")!.Width.Should().Be(22);
        diff.FindControl<Button>("btnAnotherFirstBranch")!.Height.Should().Be(22);

        compareToBranch.Width.Should().Be(434);
        compareToBranch.Height.Should().Be(110);
        // The Designer's 60x23 minimum grows through WinForms AutoSize to 66x25 with the runtime font.
        compareToBranch.FindControl<Button>("btnCompare")!.Width.Should().Be(66);
        compareToBranch.FindControl<Button>("btnCompare")!.Height.Should().Be(25);
        compareToBranch.FindControl<Button>("btnCompare")!.Padding.Left.Should().Be(0);

        // The original FormFormatPatch Designer is authored at 120 DPI, so WinForms AutoScale resolves integer 96-DPI dimensions.
        formatPatch.Width.Should().Be(824);
        formatPatch.Height.Should().Be(532);
        formatPatch.MinWidth.Should().Be(446);
        formatPatch.MinHeight.Should().Be(316);
        formatPatch.FindControl<Grid>("tableLayoutPanelForm")!.Margin.Left.Should().Be(3);
        formatPatch.FindControl<Grid>("tableLayoutPanelForm")!.RowSpacing.Should().Be(3);
        formatPatch.FindControl<Grid>("tableLayoutPanelSaveTo")!.ColumnSpacing.Should().Be(3);
        formatPatch.FindControl<Label>("lblPatches")!.Padding.Top.Should().Be(5);
        formatPatch.FindControl<TextBox>("OutputPath")!.Height.Should().Be(23);
        formatPatch.FindControl<Button>("Browse")!.Width.Should().Be(64);
        formatPatch.FindControl<Button>("Browse")!.Height.Should().Be(25);
        formatPatch.FindControl<Button>("FormatPatch")!.Width.Should().Be(140);
        formatPatch.FindControl<Button>("FormatPatch")!.Height.Should().Be(25);
        formatPatch.FindControl<TextBlock>("CurrentBranch")!.Margin.Left.Should().Be(6);
    }

    [AvaloniaTest]
    public void Compare_to_branch_should_preserve_the_original_runtime_auto_layout()
    {
        FormCompareToBranch form = new();
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            GitUI.UserControls.BranchSelector branchSelector = form.FindControl<GitUI.UserControls.BranchSelector>("branchSelector")!;
            Button compare = form.FindControl<Button>("btnCompare")!;

            branchSelector.Bounds.X.Should().Be(8);
            branchSelector.Bounds.Y.Should().Be(8);
            branchSelector.Bounds.Width.Should().Be(322);
            branchSelector.Bounds.Height.Should().Be(58);
            compare.Bounds.X.Should().Be(352);
            compare.Bounds.Y.Should().Be(78);
            compare.Bounds.Width.Should().Be(66);
            compare.Bounds.Height.Should().Be(25);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Diff_should_preserve_the_original_runtime_auto_layout()
    {
        FormDiff form = new();
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Grid settings = form.FindControl<Grid>("settingsLayoutPanel")!;
            GroupBox firstGroup = form.FindControl<GroupBox>("firstCommitGroup")!;
            StackPanel options = form.FindControl<StackPanel>("diffOptionsPanel")!;
            Grid split = form.FindControl<Grid>("splitContainer1")!;
            FileStatusList files = form.FindControl<FileStatusList>("DiffFiles")!;

            settings.Bounds.X.Should().Be(3);
            settings.Bounds.Y.Should().Be(3);
            settings.Bounds.Height.Should().Be(106);
            firstGroup.Bounds.Height.Should().Be(50);
            options.Bounds.X.Should().Be(3);
            options.Bounds.Y.Should().Be(53);
            options.Bounds.Width.Should().Be(412);
            options.Bounds.Height.Should().Be(25);
            split.Bounds.X.Should().Be(3);
            split.Bounds.Y.Should().Be(115);
            split.Bounds.Height.Should().Be(602);
            files.Bounds.Width.Should().Be(345);
            files.Bounds.Height.Should().Be(602);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Diff_designer_instance_should_ignore_runtime_only_merge_base_changes()
    {
        FormDiff form = new();
        FormDiff.TestAccessor accessor = form.GetTestAccessor();

        Action toggle = () => accessor.CompareToMergeBase.IsChecked = true;

        toggle.Should().NotThrow();
    }

    [AvaloniaTest]
    public void Compare_to_branch_should_accept_the_selected_branch()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormCompareToBranch form = new(commands, ObjectId.Parse("2222222222222222222222222222222222222222"));
        FormCompareToBranch.TestAccessor accessor = form.GetTestAccessor();
        accessor.BranchSelector.GetTestAccessor().Branches.Text = "feature/parity";

        accessor.Compare.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        form.BranchName.Should().Be("feature/parity");
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
    }

    [AvaloniaTest]
    public void Compare_to_branch_should_reject_an_empty_selection()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormCompareToBranch form = new(commands, ObjectId.Parse("2222222222222222222222222222222222222222"));
        FormCompareToBranch.TestAccessor accessor = form.GetTestAccessor();
        accessor.BranchSelector.GetTestAccessor().Branches.Text = "   ";

        accessor.Compare.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        form.BranchName.Should().BeNull();
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.None);
    }

    [AvaloniaTest]
    public void Diff_should_resolve_and_offer_the_original_merge_base()
    {
        ObjectId parent = ObjectId.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        ObjectId head = ObjectId.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetCurrentCheckout().Returns(head);
        module.GetMergeBase(parent, head).Returns(parent);

        FormDiff form = new(commands, parent, head, "HEAD~1", "HEAD");
        FormDiff.TestAccessor accessor = form.GetTestAccessor();

        accessor.CompareToMergeBase.IsEnabled.Should().BeTrue();
        accessor.CompareToMergeBase.Content.Should().Be($"Compare to merge _base ({parent.ToShortString()})");
        Avalonia.Controls.ToolTip.GetTip(form.FindControl<Button>("btnAnotherFirstBranch")!).Should().Be("Select another branch");
        Avalonia.Controls.ToolTip.GetTip(form.FindControl<Button>("btnAnotherSecondBranch")!).Should().Be("Select another branch");
        Avalonia.Controls.ToolTip.GetTip(form.FindControl<Button>("btnAnotherFirstCommit")!).Should().Be("Select another commit");
        Avalonia.Controls.ToolTip.GetTip(form.FindControl<Button>("btnAnotherSecondCommit")!).Should().Be("Select another commit");
        Avalonia.Controls.ToolTip.GetTip(accessor.Swap).Should().Be("Swap BASE and Compare commits");
    }

    [AvaloniaTest]
    public void Diff_should_disable_merge_base_when_the_revisions_are_identical()
    {
        ObjectId head = ObjectId.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetCurrentCheckout().Returns(head);

        FormDiff form = new(commands, head, head, "HEAD", "HEAD");
        FormDiff.TestAccessor accessor = form.GetTestAccessor();

        accessor.CompareToMergeBase.IsEnabled.Should().BeFalse();
        module.DidNotReceiveWithAnyArgs().GetMergeBase(default, default);
    }

    [AvaloniaTest]
    public void Diff_should_use_the_merge_base_for_directory_diff_when_selected()
    {
        ObjectId first = ObjectId.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        ObjectId second = ObjectId.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        ObjectId mergeBase = ObjectId.Parse("cccccccccccccccccccccccccccccccccccccccc");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetCurrentCheckout().Returns(second);
        module.GetMergeBase(first, second).Returns(mergeBase);
        FormDiff form = new(commands, first, second, "main~1", "main");
        FormDiff.TestAccessor accessor = form.GetTestAccessor();

        accessor.CompareToMergeBase.IsChecked = true;
        accessor.CompareDirectories.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        module.Received(1).OpenWithDifftoolDirDiff(mergeBase.ToString(), second.ToString(), customTool: null);
    }

    [AvaloniaTest]
    public void Diff_should_disable_directory_diff_from_the_worktree()
    {
        ObjectId head = ObjectId.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetCurrentCheckout().Returns(head);
        FormDiff form = new(commands, ObjectId.WorkTreeId, head, "Working directory", "HEAD");
        FormDiff.TestAccessor accessor = form.GetTestAccessor();

        Invoke(form, "PopulateDiffFiles");

        accessor.CompareDirectories.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Diff_should_swap_the_original_revision_order_and_directory_diff_arguments()
    {
        ObjectId parent = ObjectId.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        ObjectId head = ObjectId.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        module.GetCurrentCheckout().Returns(head);
        module.GetMergeBase(parent, head).Returns(parent);
        FormDiff form = new(commands, parent, head, "HEAD~1", "HEAD");
        FormDiff.TestAccessor accessor = form.GetTestAccessor();

        accessor.Swap.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        accessor.CompareDirectories.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        accessor.FirstCommit.Text.Should().Be("HEAD");
        accessor.SecondCommit.Text.Should().Be("HEAD~1");
        module.Received(1).OpenWithDifftoolDirDiff(head.ToString(), parent.ToString(), customTool: null);
    }

    [AvaloniaTest]
    public void Format_patch_should_create_the_selected_single_revision_range()
    {
        ObjectId parent = ObjectId.Parse("1111111111111111111111111111111111111111");
        ObjectId head = ObjectId.Parse("2222222222222222222222222222222222222222");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        string outputPath = Path.GetTempPath();
        module.FormatPatch(parent.ToString(), head.ToString(), outputPath).Returns("0001-parity.patch");
        GitRevision revision = new(head) { ParentIds = [parent], Subject = "Port format patch" };
        FormFormatPatch form = new(commands);
        FormFormatPatch.TestAccessor accessor = form.GetTestAccessor();
        accessor.OutputPath.Text = outputPath;
        accessor.RevisionGrid.GetTestAccessor().SetRevisions([revision]);
        accessor.RevisionGrid.GetTestAccessor().Revisions.SelectedItem = revision;

        Invoke(form, "FormatPatch_Click", form, EventArgs.Empty);

        module.Received(1).FormatPatch(parent.ToString(), head.ToString(), outputPath);
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().Be("0001-parity.patch");
    }

    [AvaloniaTest]
    public void Format_patch_should_reject_an_empty_output_path()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        FormFormatPatch form = new(commands);

        Invoke(form, "FormatPatch_Click", form, EventArgs.Empty);

        module.DidNotReceiveWithAnyArgs().FormatPatch(default!, default!, default!);
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().Be("You need to enter an output path.");
    }

    [AvaloniaTest]
    public void Format_patch_should_use_the_selected_folder_and_preserve_a_cancelled_selection()
    {
        (IGitUICommands commands, _) = CreateCommands();
        FormFormatPatch form = new(commands);
        FormFormatPatch.TestAccessor accessor = form.GetTestAccessor();
        accessor.OutputPath.Text = "existing";

        _folderPicker.Result = null;
        accessor.Browse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        accessor.OutputPath.Text.Should().Be("existing");

        _folderPicker.Result = Path.GetTempPath();
        accessor.Browse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        accessor.OutputPath.Text.Should().Be(Path.GetTempPath());
        _folderPicker.Owners.Should().OnlyContain(owner => ReferenceEquals(owner, form));
    }

    [AvaloniaTest]
    public void Format_patch_should_reject_an_empty_revision_selection()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        FormFormatPatch form = new(commands);
        form.GetTestAccessor().OutputPath.Text = Path.GetTempPath();

        Invoke(form, "FormatPatch_Click", form, EventArgs.Empty);

        module.DidNotReceiveWithAnyArgs().FormatPatch(default!, default!, default!);
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().Be("You need to select at least one revision");
    }

    [AvaloniaTest]
    public void Format_patch_should_report_an_empty_git_result_without_closing()
    {
        ObjectId head = ObjectId.Parse("2222222222222222222222222222222222222222");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        string outputPath = Path.GetTempPath();
        module.FormatPatch(string.Empty, head.ToString(), outputPath).Returns(string.Empty);
        GitRevision revision = new(head) { ParentIds = [] };
        FormFormatPatch form = new(commands);
        FormFormatPatch.TestAccessor accessor = form.GetTestAccessor();
        accessor.OutputPath.Text = outputPath;
        accessor.RevisionGrid.GetTestAccessor().SetRevisions([revision]);
        accessor.RevisionGrid.GetTestAccessor().Revisions.SelectedItem = revision;

        Invoke(form, "FormatPatch_Click", form, EventArgs.Empty);

        module.Received(1).FormatPatch(string.Empty, head.ToString(), outputPath);
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().Be("Unable to create patch file(s)");
    }

    [AvaloniaTest]
    public void Format_patch_should_preserve_the_original_two_revision_range()
    {
        ObjectId oldestParent = ObjectId.Parse("1111111111111111111111111111111111111111");
        ObjectId oldest = ObjectId.Parse("2222222222222222222222222222222222222222");
        ObjectId newestParent = ObjectId.Parse("3333333333333333333333333333333333333333");
        ObjectId newest = ObjectId.Parse("4444444444444444444444444444444444444444");
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        string outputPath = Path.GetTempPath();
        module.FormatPatch(newestParent.ToString(), oldest.ToString(), outputPath).Returns("two-revision.patch");
        GitRevision oldestRevision = new(oldest) { ParentIds = [oldestParent] };
        GitRevision newestRevision = new(newest) { ParentIds = [newestParent] };
        FormFormatPatch form = new(commands);
        FormFormatPatch.TestAccessor accessor = form.GetTestAccessor();
        accessor.OutputPath.Text = outputPath;
        accessor.RevisionGrid.GetTestAccessor().SetRevisions([oldestRevision, newestRevision]);
        accessor.RevisionGrid.GetTestAccessor().Revisions.SelectedItems!.Add(oldestRevision);
        accessor.RevisionGrid.GetTestAccessor().Revisions.SelectedItems!.Add(newestRevision);

        Invoke(form, "FormatPatch_Click", form, EventArgs.Empty);

        module.Received(1).FormatPatch(newestParent.ToString(), oldest.ToString(), outputPath);
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().Be("two-revision.patch");
    }

    [AvaloniaTest]
    public void Format_patch_should_create_each_selected_revision_when_more_than_two_are_selected()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        string outputPath = Path.GetTempPath();
        GitRevision[] revisions =
        [
            CreateRevision('2', '1'),
            CreateRevision('4', '3'),
            CreateRevision('6', '5'),
        ];
        module.FormatPatch(Arg.Any<string>(), Arg.Any<string>(), outputPath, Arg.Any<int>()).Returns("patch");
        FormFormatPatch form = new(commands);
        FormFormatPatch.TestAccessor accessor = form.GetTestAccessor();
        accessor.OutputPath.Text = outputPath;
        accessor.RevisionGrid.GetTestAccessor().SetRevisions(revisions);
        foreach (GitRevision revision in revisions)
        {
            accessor.RevisionGrid.GetTestAccessor().Revisions.SelectedItems!.Add(revision);
        }

        Invoke(form, "FormatPatch_Click", form, EventArgs.Empty);

        module.Received(1).FormatPatch(new string('5', 40), new string('6', 40), outputPath, 1);
        module.Received(1).FormatPatch(new string('3', 40), new string('4', 40), outputPath, 2);
        module.Received(1).FormatPatch(new string('1', 40), new string('2', 40), outputPath, 3);
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().Be("patchpatchpatch");
    }

    private static GitRevision CreateRevision(char objectId, char parentId)
        => new(ObjectId.Parse(new string(objectId, 40)))
        {
            ParentIds = [ObjectId.Parse(new string(parentId, 40))],
        };

    private static void Invoke(object target, string methodName, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"{target.GetType().Name}.{methodName} was not found.");
        method.Invoke(target, arguments);
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.GetRefs(Arg.Any<RefsFilter>()).Returns([]);
        module.GetSelectedBranch(Arg.Any<bool>()).Returns("main");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);
    }

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
            return WinFormsShims.DialogResult.OK;
        }
    }

    private sealed class StubFolderPicker : WinFormsShims.IFolderPicker
    {
        public List<WinFormsShims.IWin32Window?> Owners { get; } = [];
        public string? Result { get; set; }

        public string? PickFolder(WinFormsShims.IWin32Window? owner, string? selectedPath)
        {
            Owners.Add(owner);
            return Result;
        }
    }
}
