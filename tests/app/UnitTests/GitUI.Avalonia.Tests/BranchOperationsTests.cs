using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class BranchOperationsTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_should_construct_with_the_original_controls()
    {
        FormDeleteRemoteBranch form = new();
        FormDeleteRemoteBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Delete.Should().NotBeNull();
        accessor.DeleteRemote.Should().NotBeNull();
        accessor.DeleteLocalTrackingBranch.Should().NotBeNull();
        accessor.Branches.Should().NotBeNull();

        // Delete is disabled until the remote checkbox is set, mirroring the original.
        accessor.Delete.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_should_emit_its_translation_keys()
    {
        FormDeleteRemoteBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "$this", "Text", "Delete branch");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "Delete", "Text", "&Delete");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "labelSelectBranches", "Text", "Select &branches");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "DeleteRemote", "Text", "Delete branch(es) from &remote repository");
        translation.Received(1).AddTranslationItem(nameof(FormDeleteRemoteBranch), "DeleteLocalTrackingBranch", "Text", "Delete &local tracking branch (if available)");
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_delete_button_follows_the_remote_checkbox()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-delete-remote"));
        module.GetMergedRemoteBranches().Returns([]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormDeleteRemoteBranch form = new(commands, string.Empty);
        FormDeleteRemoteBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Delete.IsEnabled.Should().BeFalse();

        accessor.DeleteRemote.IsChecked = true;
        accessor.Delete.IsEnabled.Should().BeTrue();

        accessor.DeleteRemote.IsChecked = false;
        accessor.Delete.IsEnabled.Should().BeFalse();

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_should_show_local_tracking_candidates_for_the_selected_remote()
    {
        IGitRef remote = Substitute.For<IGitRef>();
        remote.Name.Returns("origin/feature/delete-me");
        remote.IsRemote.Returns(true);
        IGitRef local = Substitute.For<IGitRef>();
        local.LocalName.Returns("tracking-delete");
        local.IsTrackingRemote(remote).Returns(true);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetMergedRemoteBranches().Returns([]);
        module.GetRefs(RefsFilter.Remotes).Returns([remote]);
        module.GetRefs(RefsFilter.Heads).Returns([local]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormDeleteRemoteBranch form = new(commands, remote.Name);
        FormDeleteRemoteBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Load();

        accessor.DeleteLocalTrackingBranch.IsEnabled.Should().BeTrue();
        accessor.TrackingCandidateText.Should().Be(
            "Local tracking branche(s) candidate to deletion:" + Environment.NewLine + " - tracking-delete");
    }

    [AvaloniaTest]
    public void FormDeleteRemoteBranch_should_use_the_native_96_dpi_runtime_layout()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GetMergedRemoteBranches().Returns([]);
        module.GetRefs(RefsFilter.Remotes).Returns([]);
        module.GetRefs(RefsFilter.Heads).Returns([]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormDeleteRemoteBranch form = new(commands, string.Empty)
        {
            Height = 167,
            SizeToContent = SizeToContent.Manual
        };

        try
        {
            form.Show();
            form.FindControl<TextBlock>("_NO_TRANSLATE_labelLocalTrackingBranches")!.Text =
                "Local tracking branche(s) candidate to deletion:\n - tracking-delete";
            Dispatcher.UIThread.RunJobs();

            Control mainPanel = form.FindControl<Grid>("MainPanel")!;
            Control controlsPanel = form.FindControl<Grid>("ControlsPanel")!;
            Control table = form.FindControl<Grid>("tlpnlMain")!;
            BranchComboBox branchComboBox = form.FindControl<BranchComboBox>("Branches")!;

            AssertBounds(mainPanel, 0, 0, 403, 126);
            AssertBounds(controlsPanel, 0, 126, 403, 41);
            AssertBoundsRelativeTo(table, mainPanel, 9, 9, 385, 108);
            AssertBoundsRelativeTo(form.FindControl<Label>("labelSelectBranches")!, table, 3, 0, 89, 28);
            AssertBoundsRelativeTo(branchComboBox, table, 95, 0, 290, 28);
            AssertBoundsRelativeTo(branchComboBox.FindControl<ComboBox>("branches")!, branchComboBox, 0, 3, 263, 23);
            AssertBoundsRelativeTo(branchComboBox.FindControl<Button>("selectMultipleBranchesButton")!, branchComboBox, 267, 1, 23, 25);
            AssertBoundsRelativeTo(form.FindControl<CheckBox>("DeleteRemote")!, table, 98, 31, 284, 19);
            AssertBoundsRelativeTo(form.FindControl<CheckBox>("DeleteLocalTrackingBranch")!, table, 98, 56, 284, 19);
            TextBlock trackingBranches = form.FindControl<TextBlock>("_NO_TRANSLATE_labelLocalTrackingBranches")!;
            Point trackingOrigin = trackingBranches.TranslatePoint(default, table)
                ?? throw new InvalidOperationException("Could not translate the tracking-branch label into tlpnlMain.");
            trackingOrigin.Should().Be(new Point(98, 78));
            trackingBranches.Bounds.Width.Should().Be(258);
            trackingBranches.Bounds.Height.Should().BeGreaterThanOrEqualTo(30);
            (trackingBranches.Bounds.Height % 15).Should().Be(0);
            // Cross-platform constraint: fallback fonts may wrap this fixed-width caption to a
            // third line; Windows must retain the captured two-line WinForms height.
            if (OperatingSystem.IsWindows())
            {
                trackingBranches.Bounds.Height.Should().Be(30);
            }

            AssertBoundsRelativeTo(form.FindControl<Button>("Delete")!, controlsPanel, 315, 8, 75, 25);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_construct_with_the_original_controls()
    {
        FormResetAnotherBranch form = new();
        FormResetAnotherBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Branches.Should().NotBeNull();
        accessor.Ok.Should().NotBeNull();
        accessor.Cancel.Should().NotBeNull();
        accessor.ForceReset.Should().NotBeNull();
        accessor.CheckoutBranch.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_emit_its_translation_keys()
    {
        FormResetAnotherBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "$this", "Text", "Reset branch");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "BranchInfo", "Text", "Reset local &branch:");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "Ok", "Text", "OK");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "Cancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "ForceReset", "Text", "&Force reset for a non-fast-forward reset");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "cbxCheckoutBranch", "Text", "Chec&kout branch after reset");
        translation.Received(1).AddTranslationItem(nameof(FormResetAnotherBranch), "_localRefInvalid", "Text", "The entered value '{0}' is not the name of an existing local branch.");
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_load_the_branches_and_revision_summary()
    {
        GitRevision revision = new(ObjectId.Random());
        IGitModule module = Substitute.For<IGitModule>();
        module.GetSelectedBranch().Returns("main");
        module.GetRefs(RefsFilter.Heads).Returns([]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormResetAnotherBranch form = FormResetAnotherBranch.Create(commands, revision);
        FormResetAnotherBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Load();

        accessor.SummaryRevision.Should().BeSameAs(revision);
        accessor.Ok.IsEnabled.Should().BeFalse();

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_allow_a_forced_reset_of_an_existing_local_branch()
    {
        GitRevision revision = new(ObjectId.Random());
        IGitRef branch = Substitute.For<IGitRef>();
        branch.IsHead.Returns(true);
        branch.Name.Returns("feature/reset-target");
        branch.LocalName.Returns("feature/reset-target");
        branch.ObjectId.Returns(ObjectId.Random());
        IGitModule module = Substitute.For<IGitModule>();
        module.GetSelectedBranch().Returns("main");
        module.GetRefs(RefsFilter.Heads).Returns([branch]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormResetAnotherBranch form = FormResetAnotherBranch.Create(commands, revision);
        FormResetAnotherBranch.TestAccessor accessor = form.GetTestAccessor();

        accessor.Load();
        accessor.Branches.Text = branch.Name;
        accessor.ForceReset.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        accessor.Ok.IsEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormResetAnotherBranch_should_use_the_native_96_dpi_runtime_layout()
    {
        GitRevision revision = new(ObjectId.Random());
        IGitModule module = Substitute.For<IGitModule>();
        module.GetSelectedBranch().Returns("main");
        module.GetRefs(RefsFilter.Heads).Returns([]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormResetAnotherBranch form = FormResetAnotherBranch.Create(commands, revision);

        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            Grid table = form.FindControl<Grid>("tableLayoutPanel1")!;
            Control warning = form.FindControl<Grid>("tlpnlWarning")!;
            Control summary = form.FindControl<CommitSummaryUserControl>("commitSummaryUserControl")!;
            Control buttons = form.FindControl<Grid>("flowLayoutPanel1")!;

            AssertBounds(table, 8, 8, 529, 331);
            AssertBoundsRelativeTo(warning, table, 5, 5, 519, 30);
            AssertBoundsRelativeTo(form.FindControl<Control>("pictureBox1")!, warning, 3, 3, 16, 24);
            AssertBoundsRelativeTo(form.FindControl<TextBlock>("lblResetBranchWarning")!, warning, 25, 0, 491, 30);
            AssertBoundsRelativeTo(form.FindControl<Label>("BranchInfo")!, table, 6, 47, 517, 15);
            AssertBoundsRelativeTo(form.FindControl<ComboBox>("Branches")!, table, 11, 65, 507, 23);
            AssertBoundsRelativeTo(summary, table, 4, 92, 521, 150);
            AssertBoundsRelativeTo(form.FindControl<CheckBox>("cbxCheckoutBranch")!, table, 6, 246, 517, 19);
            AssertBoundsRelativeTo(form.FindControl<CheckBox>("ForceReset")!, table, 6, 271, 517, 19);
            AssertBoundsRelativeTo(buttons, table, 5, 295, 519, 31);
            AssertBoundsRelativeTo(form.FindControl<Button>("Ok")!, buttons, 328, 3, 91, 25);
            AssertBoundsRelativeTo(form.FindControl<Button>("Cancel")!, buttons, 425, 3, 91, 25);

            AssertBoundsRelativeTo(summary.FindControl<Grid>("tableLayoutPanel1")!, summary, 11, 27, 499, 112);
            AssertBoundsRelativeTo(summary.FindControl<TextBlock>("labelMessage")!, summary, 13, 27, 495, 15);
            AssertBoundsRelativeTo(summary.FindControl<TextBlock>("labelAuthor")!, summary, 99, 55, 409, 21);
            AssertBoundsRelativeTo(summary.FindControl<TextBlock>("labelBranches")!, summary, 99, 97, 409, 21);
            AssertBoundsRelativeTo(summary.FindControl<TextBlock>("labelTags")!, summary, 99, 118, 409, 21);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Revision_grid_should_emit_the_reset_another_branch_translation_key()
    {
        RevisionGridControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(RevisionGridControl),
            "resetAnotherBranchToHereToolStripMenuItem",
            "Text",
            "Reset an&other branch to here...");
    }

    private static void AssertBounds(Control control, double x, double y, double width, double height)
        => control.Bounds.Should().Be(new Rect(x, y, width, height));

    private static void AssertBoundsRelativeTo(
        Control control,
        Visual relativeTo,
        double x,
        double y,
        double width,
        double height)
    {
        Point origin = control.TranslatePoint(default, relativeTo)
            ?? throw new InvalidOperationException($"Could not translate {control.Name} into {relativeTo}.");
        new Rect(origin, control.Bounds.Size).Should().Be(new Rect(x, y, width, height));
    }
}
