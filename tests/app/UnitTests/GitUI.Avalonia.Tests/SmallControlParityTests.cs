using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI;
using GitUI.UserControls;
using GitUI.UserControls.Settings;

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

        control.Text = "main";
        control.NotifyAutoCompleteForTest();

        control.SelectedIndex.Should().Be(1);
        control.Text.Should().Be("main");
    }

    [AvaloniaTest]
    public void WaitSpinner_should_expose_the_original_animation_boundary()
    {
        WaitSpinner control = new() { Width = 48, Height = 48 };
        Window window = new() { Content = control };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            control.IsAnimating.Should().BeTrue();

            control.SetProgressForCapture(7);
            control.IsAnimating = false;
            control.IsAnimating.Should().BeFalse();
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
}
