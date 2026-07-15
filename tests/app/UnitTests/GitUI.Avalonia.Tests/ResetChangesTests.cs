using Avalonia.Headless.NUnit;
using GitUI.CommandsDialogs;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ResetChangesTests
{
    [AvaloniaTest]
    public void FormResetChanges_should_construct()
    {
        using FormResetChanges form = new();
        form.SelectedAction.Should().Be(FormResetChanges.ActionEnum.Cancel, "closing the dialog must not reset anything");
    }

    [AvaloniaTest]
    public void FormResetChanges_should_force_deleting_new_files_when_only_new_files_are_selected()
    {
        using FormResetChanges form = new(hasExistingFiles: false, hasNewFiles: true);

        form.cbDeleteNewFilesAndDirectories.IsChecked.Should().BeTrue();
        form.cbDeleteNewFilesAndDirectories.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormResetChanges_should_forbid_deleting_new_files_when_none_are_selected()
    {
        using FormResetChanges form = new(hasExistingFiles: true, hasNewFiles: false);

        form.cbDeleteNewFilesAndDirectories.IsChecked.Should().BeFalse();
        form.cbDeleteNewFilesAndDirectories.IsEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormResetChanges_should_offer_deleting_new_files_for_a_mixed_selection()
    {
        using FormResetChanges form = new(hasExistingFiles: true, hasNewFiles: true);

        form.cbDeleteNewFilesAndDirectories.IsEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormResetChanges_should_show_the_confirmation_message()
    {
        using FormResetChanges form = new(hasExistingFiles: true, hasNewFiles: true, confirmationMessage: "Reset 3 files?");

        form.txtMessage.Text.Should().Be("Reset 3 files?");
    }

    [AvaloniaTest]
    [TestCase(true, FormResetChanges.ActionEnum.ResetAndDelete)]
    [TestCase(false, FormResetChanges.ActionEnum.Reset)]
    public void FormResetChanges_reset_should_report_whether_new_files_are_deleted(bool deleteNewFiles, FormResetChanges.ActionEnum expected)
    {
        using FormResetChanges form = new(hasExistingFiles: true, hasNewFiles: true);
        form.cbDeleteNewFilesAndDirectories.IsChecked = deleteNewFiles;

        RaiseClick(form.btnReset);

        form.SelectedAction.Should().Be(expected);
    }

    [AvaloniaTest]
    public void FormResetChanges_cancel_should_report_cancel()
    {
        using FormResetChanges form = new(hasExistingFiles: true, hasNewFiles: true);
        form.cbDeleteNewFilesAndDirectories.IsChecked = true;

        RaiseClick(form.btnCancel);

        form.SelectedAction.Should().Be(FormResetChanges.ActionEnum.Cancel);
    }

    private static void RaiseClick(Avalonia.Controls.Button button)
        => button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
}
