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
    private StubMessageBoxHost _messageBoxes = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _messageBoxes = new StubMessageBoxHost();
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
        accessor.CompareToMergeBase.Content.Should().Be($"Compare to merge &base ({parent.ToShortString()})");
        Avalonia.Controls.ToolTip.GetTip(accessor.Swap).Should().Be("Swap BASE and Compare commits");
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
}
