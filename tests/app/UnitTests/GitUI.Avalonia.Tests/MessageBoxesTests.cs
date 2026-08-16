using GitCommands.Settings;
using GitUI;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
public sealed class MessageBoxesTests
{
    [TearDown]
    public void TearDown()
    {
        MessageBoxes.TestAccessor.TaskDialogPresenter = TaskDialog.ShowDialog;
    }

    [Test]
    public void ConfirmSuppressible_should_confirm_without_showing_a_dialog_when_suppressed()
    {
        TestSetting dontConfirm = new() { Value = true };

        bool confirmed = MessageBoxes.ConfirmSuppressible(owner: null, "message", "caption", dontConfirm);

        confirmed.Should().BeTrue();
        dontConfirm.Value.Should().BeTrue();
    }

    [Test]
    public void ConfirmSuppressible_should_persist_checked_suppression_after_confirmation()
    {
        TestSetting dontConfirm = new();
        MessageBoxes.TestAccessor.TaskDialogPresenter = (_, page) =>
        {
            page.Verification!.Checked = true;
            return TaskDialogButton.Yes;
        };

        bool confirmed = MessageBoxes.ConfirmSuppressible(owner: null, "message", "caption", dontConfirm);

        confirmed.Should().BeTrue();
        dontConfirm.Value.Should().BeTrue();
    }

    private sealed class TestSetting : ISetting<bool>
    {
        public string Name => "DontConfirm";

        public bool Default => false;

        public bool Value { get; set; }

        public string FullPath => Name;
    }
}
