using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Properties;
using GitUI.UserControls;
using GitUI.UserControls.Settings;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class SmallControlParityTests
{
    [AvaloniaTest]
    public void BranchSelector_should_preserve_named_fields_and_local_default()
    {
        BranchSelector control = new();
        BranchSelector.TestAccessor accessor = control.GetTestAccessor();

        accessor.LocalBranch.IsChecked.Should().BeTrue();
        accessor.Remotebranch.IsChecked.Should().NotBeTrue();
        accessor.Branches.IsEditable.Should().BeTrue();
        control.IsRemoteBranchChecked.Should().BeFalse();
        control.SelectedBranchName.Should().BeEmpty();
        accessor.LocalBranch.MinWidth.Should().Be(93);
        accessor.Remotebranch.MinWidth.Should().Be(112);
        accessor.Remotebranch.Margin.Left.Should().Be(2);
        accessor.Remotebranch.Padding.Left.Should().Be(6);
    }

    [AvaloniaTest]
    public void BranchSelector_should_preserve_96_dpi_Designer_geometry_in_dips()
    {
        BranchSelector control = new();
        Window window = new() { Width = 325, Height = 54, Content = control };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            control.Bounds.Width.Should().BeApproximately(325, 1);
            control.Bounds.Height.Should().BeApproximately(54, 1);
            control.GetTestAccessor().Branches.MinWidth.Should().Be(214);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void BranchSelector_should_reload_the_selected_branch_source_in_both_directions()
    {
        IGitRef localBranch = Substitute.For<IGitRef>();
        localBranch.Name.Returns("main");
        IGitRef remoteBranch = Substitute.For<IGitRef>();
        remoteBranch.Name.Returns("origin/main");
        IGitModule module = Substitute.For<IGitModule>();
        module.GetRefs(RefsFilter.Heads).Returns([localBranch]);
        module.GetRefs(RefsFilter.Remotes).Returns([remoteBranch]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        BranchSelector control = new() { UICommandsSource = source };
        BranchSelector.TestAccessor accessor = control.GetTestAccessor();

        control.Initialize(remote: false, containObjectIds: null);
        accessor.Branches.ItemsSource.Should().BeEquivalentTo(new[] { "main" });

        accessor.Remotebranch.IsChecked = true;
        accessor.Branches.ItemsSource.Should().BeEquivalentTo(new[] { "origin/main" });

        accessor.LocalBranch.IsChecked = true;
        accessor.Branches.ItemsSource.Should().BeEquivalentTo(new[] { "main" });
    }

    [AvaloniaTest]
    [TestCase(InteractiveGitActionControl.GitAction.Rebase, false)]
    [TestCase(InteractiveGitActionControl.GitAction.Rebase, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Merge, false)]
    [TestCase(InteractiveGitActionControl.GitAction.Merge, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Patch, false)]
    [TestCase(InteractiveGitActionControl.GitAction.Patch, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Bisect, false)]
    [TestCase(InteractiveGitActionControl.GitAction.None, false)]
    [TestCase(InteractiveGitActionControl.GitAction.None, true)]
    public void InteractiveGitActionControl_should_match_original_state_matrix(
        InteractiveGitActionControl.GitAction action,
        bool conflicts)
    {
        InteractiveGitActionControl control = new();
        InteractiveGitActionControl.TestAccessor accessor = control.GetTestAccessor();

        accessor.SetGitAction(action, conflicts);

        accessor.Action.Should().Be(action);
        accessor.HasConflicts.Should().Be(conflicts);
        if (accessor.Visible)
        {
            accessor.HasIconClass("gitextensions-icon-16").Should().BeTrue();
            accessor.Icon.Should().BeSameAs(conflicts ? Images.SolveMerge : Images.Information);
            accessor.TextLabel.Text.Should().Be(ExpectedMessage(action, conflicts));
        }

        switch (action)
        {
            case InteractiveGitActionControl.GitAction.None:
                accessor.Visible.Should().Be(conflicts);
                accessor.Controls.Contains(accessor.ResolveButton).Should().Be(conflicts);
                accessor.Controls.Count.Should().Be(conflicts ? 1 : 0);
                break;
            case InteractiveGitActionControl.GitAction.Bisect:
                accessor.Visible.Should().BeTrue();
                accessor.Controls.Should().ContainSingle().Which.Should().BeSameAs(accessor.MoreButton);
                break;
            case InteractiveGitActionControl.GitAction.Rebase:
            case InteractiveGitActionControl.GitAction.Patch:
                accessor.Visible.Should().BeTrue();
                accessor.Controls.Contains(accessor.ResolveButton).Should().Be(conflicts);
                accessor.Controls.Contains(accessor.ContinueButton).Should().Be(!conflicts);
                accessor.Controls.Contains(accessor.AbortButton).Should().BeTrue();
                accessor.Controls.Contains(accessor.MoreButton).Should().BeTrue();
                break;
            case InteractiveGitActionControl.GitAction.Merge:
                accessor.Visible.Should().BeTrue();
                accessor.Controls.Contains(accessor.ResolveButton).Should().Be(conflicts);
                accessor.Controls.Contains(accessor.ContinueButton).Should().Be(!conflicts);
                accessor.Controls.Contains(accessor.AbortButton).Should().BeTrue();
                accessor.Controls.Contains(accessor.MoreButton).Should().BeFalse();
                break;
        }

        static string ExpectedMessage(InteractiveGitActionControl.GitAction action, bool conflicts)
        {
            if (action == InteractiveGitActionControl.GitAction.None)
            {
                return "There are unresolved merge conflicts.";
            }

            string actionName = action.ToString();
            return conflicts
                ? $"{actionName} is currently in progress with merge conflicts."
                : $"{actionName} is currently in progress.";
        }
    }

    [AvaloniaTest]
    public void SettingsCheckBox_should_forward_text_checked_state_and_tooltip()
    {
        SettingsCheckBox control = new();
        SettingsCheckBox.TestAccessor accessor = control.GetTestAccessor();
        int checkedChanges = 0;
        control.CheckedChanged += (_, _) => checkedChanges++;

        control.Text = "Enable feature";
        control.ToolTipText = "More information";
        control.Checked = true;

        control.Text.Should().Be("Enable feature");
        control.Checked.Should().BeTrue();
        checkedChanges.Should().Be(1);
        accessor.PictureBox.IsVisible.Should().BeTrue();
        accessor.PictureBox.Classes.Should().Contain("gitextensions-icon-16");
        accessor.PictureBox.Source.Should().BeSameAs(Images.Information);
        ToolTip.GetTip(accessor.CheckBox).Should().Be("More information");
        ToolTip.GetTip(accessor.PictureBox).Should().Be("More information");
    }

    [AvaloniaTest]
    public void WatermarkComboBox_should_leave_text_unchanged_while_exposing_watermark()
    {
        WatermarkComboBox control = new() { Watermark = "Find...", Text = "needle" };
        Window window = new() { Content = control };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            control.PlaceholderText.Should().Be("Find...");
            control.Text.Should().Be("needle");
            control.GetTestAccessor().BaseText.Should().Be("needle");
            control.IsWatermarkVisible.Should().BeFalse();
            control.GetVisualDescendants().OfType<TextBox>()
                .Should().ContainSingle(textBox => textBox.Name == "PART_EditableTextBox")
                .Which.Text.Should().Be("needle");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void CaseSensitiveComboBox_should_select_only_an_exact_case_match()
    {
        CaseSensitiveComboBox control = new() { ItemsSource = new[] { "Main", "main" } };
        Window window = new() { Content = control };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            control.Text = "main";
            control.NotifyAutoCompleteForTest();

            control.SelectedIndex.Should().Be(1);
            control.Text.Should().Be("main");

            control.Text = "Main";
            control.NotifyAutoCompleteForTest();

            control.SelectedIndex.Should().Be(0);
            control.Text.Should().Be("Main");
            control.GetVisualDescendants().OfType<TextBox>()
                .Should().ContainSingle(textBox => textBox.Name == "PART_EditableTextBox");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task WaitSpinner_should_expose_the_original_animation_boundary()
    {
        WaitSpinner control = new() { Width = 48, Height = 48 };
        Window window = new() { Content = control };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            control.IsAnimating.Should().BeTrue();

            int initialProgress = control.ProgressForTest;
            await WaitUntilAsync(() => control.ProgressForTest != initialProgress);

            control.SetProgressForCapture(7);
            control.IsAnimating = false;
            control.IsAnimating.Should().BeFalse();
            int stoppedProgress = control.ProgressForTest;
            await Task.Delay(80);
            Dispatcher.UIThread.RunJobs();
            control.ProgressForTest.Should().Be(stoppedProgress);
        }
        finally
        {
            window.Close();
        }
    }

    [Test]
    public void EnterEventArgs_should_report_the_input_origin()
    {
        new EnterEventArgs(byMouse: true).ByMouse.Should().BeTrue();
        new EnterEventArgs(byMouse: false).ByMouse.Should().BeFalse();
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(2);
        while (!predicate())
        {
            if (DateTime.UtcNow >= deadline)
            {
                Assert.Fail("Timed out waiting for the spinner animation timer.");
            }

            await Task.Delay(20);
            Dispatcher.UIThread.RunJobs();
        }
    }
}
