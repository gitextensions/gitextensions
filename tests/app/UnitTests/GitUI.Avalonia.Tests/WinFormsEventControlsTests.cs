using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using AwesomeAssertions;
using TextChangedComboBox = GitUI.Compat.WinFormsEvents.ComboBox;
using ValidatingTextBox = GitUI.Compat.WinFormsEvents.TextBox;

namespace GitUI.Avalonia.Tests;

[TestFixture]
public sealed class WinFormsEventControlsTests
{
    [AvaloniaTest]
    public void WinForms_validating_text_box_should_cancel_the_real_pre_focus_transition()
    {
        ValidatingTextBox textBox = new();
        Button nextButton = new();
        bool cancel = true;
        int validationCount = 0;
        textBox.Validating += (_, e) =>
        {
            validationCount++;
            e.Cancel = cancel;
        };
        Window window = new()
        {
            Content = new StackPanel
            {
                Children =
                {
                    textBox,
                    nextButton,
                },
            },
        };
        try
        {
            window.Show();
            textBox.Focus();
            Dispatcher.UIThread.RunJobs();

            nextButton.Focus();
            Dispatcher.UIThread.RunJobs();

            validationCount.Should().Be(1);
            textBox.IsKeyboardFocusWithin.Should().BeTrue();
            nextButton.IsKeyboardFocusWithin.Should().BeFalse();

            cancel = false;
            nextButton.Focus();
            Dispatcher.UIThread.RunJobs();

            validationCount.Should().Be(2);
            nextButton.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void WinForms_text_changed_combo_box_should_raise_only_for_effective_text_changes()
    {
        TextChangedComboBox comboBox = new() { IsEditable = true };
        List<string?> values = [];
        comboBox.TextChanged += (_, _) => values.Add(comboBox.Text);

        comboBox.Text = "upstream";
        comboBox.Text = "upstream";
        comboBox.Text = "parent";

        values.Should().Equal("upstream", "parent");
    }
}
