using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.ScriptsEngine;

namespace GitExtensionsTests;

[TestFixture]
public sealed class TypographyAndMetricsTests
{
    [AvaloniaTest]
    public void Shared_styles_should_match_compact_WinForms_control_heights()
    {
        TextBox textBox = new();
        ComboBox comboBox = new() { ItemsSource = new[] { "main" }, SelectedIndex = 0 };
        NumericUpDown numeric = new();
        CheckBox checkBox = new() { Content = "Check" };
        RadioButton radioButton = new() { Content = "Radio" };
        Button action = new() { Classes = { "gitextensions-dialog-action" }, Content = "OK" };
        StackPanel content = new()
        {
            Width = 240,
            Children =
            {
                textBox,
                comboBox,
                numeric,
                checkBox,
                radioButton,
                action,
            },
        };
        Window window = new() { SizeToContent = SizeToContent.WidthAndHeight, Content = content };

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            textBox.Bounds.Height.Should().Be(23);
            comboBox.Bounds.Height.Should().Be(23);
            numeric.Bounds.Height.Should().Be(23);
            checkBox.Bounds.Height.Should().Be(19);
            radioButton.Bounds.Height.Should().Be(19);
            action.Bounds.Height.Should().Be(25);
            action.Bounds.Width.Should().BeGreaterThanOrEqualTo(75);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FormFilePrompt_should_preserve_designer_metrics_at_every_render_scale()
    {
        foreach (double scale in new[] { 1d, 1.25d, 1.5d, 2d })
        {
            FormFilePrompt form = new();
            form.SetRenderScaling(scale);
            form.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();

                Grid layout = form.GetVisualDescendants().OfType<Grid>().First();
                TextBlock label = form.FindControl<TextBlock>("lblSelectFiles")!;
                TextBox path = form.FindControl<TextBox>("txtFilePath")!;
                Button browse = form.FindControl<Button>("btnBrowse")!;
                Button ok = form.FindControl<Button>("btnOk")!;

                form.Bounds.Width.Should().BeApproximately(549, 0.8, "the {0:P0} capture should preserve the Designer client width", scale);
                form.Bounds.Height.Should().BeApproximately(78, 0.8, "the {0:P0} capture should preserve the Designer client height", scale);
                layout.Margin.Should().Be(new Thickness(8));
                label.Margin.Should().Be(new Thickness(3));
                path.Margin.Should().Be(new Thickness(3));
                path.Bounds.Height.Should().BeApproximately(21, 0.8);
                browse.Margin.Should().Be(new Thickness(3));
                browse.Bounds.Width.Should().BeApproximately(115, 0.8);
                browse.Bounds.Height.Should().BeApproximately(25, 0.8);
                ok.Margin.Should().Be(new Thickness(3));
                ok.Bounds.Width.Should().BeApproximately(115, 0.8);
                ok.Bounds.Height.Should().BeApproximately(25, 0.8);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void FormRecentReposSettings_should_preserve_source_specific_density()
    {
        FormRecentReposSettings form = new([]);
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            form.Bounds.Size.Should().Be(new Size(684, 361));
            NumericUpDown maximum = form.FindControl<NumericUpDown>("_NO_TRANSLATE_maxRecentRepositories")!;
            NumericUpDown minimumWidth = form.FindControl<NumericUpDown>("comboMinWidthEdit")!;
            CheckBox sortTop = form.FindControl<CheckBox>("sortTopRepos")!;
            RadioButton doNotShorten = form.FindControl<RadioButton>("dontShortenRB")!;
            TextBlock topLabel = form.FindControl<TextBlock>("TopLabel")!;
            TextBlock recentLabel = form.FindControl<TextBlock>("label1")!;

            maximum.Bounds.Size.Should().Be(new Size(61, 23));
            minimumWidth.Bounds.Size.Should().Be(new Size(61, 23));
            maximum.GetVisualDescendants().OfType<TextBox>().Single().Bounds.Width.Should().BeGreaterThan(30);
            RepeatButton[] spinnerButtons = maximum.GetVisualDescendants().OfType<RepeatButton>().ToArray();
            spinnerButtons.Should().HaveCount(2);
            spinnerButtons.Should().OnlyContain(button => button.Bounds.Width == 18);
            spinnerButtons[0].Bounds.Y.Should().BeLessThan(spinnerButtons[1].Bounds.Y);
            sortTop.Bounds.Height.Should().Be(19);
            sortTop.Margin.Should().Be(new Thickness(3));
            doNotShorten.Bounds.Height.Should().Be(19);
            doNotShorten.Margin.Should().Be(new Thickness(3));
            topLabel.FontWeight.Should().Be(FontWeight.Normal);
            recentLabel.FontWeight.Should().Be(FontWeight.Normal);
        }
        finally
        {
            form.Close();
        }
    }
}
