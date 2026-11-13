using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class MainWindowNativePaletteTests
{
    [AvaloniaTest]
    [TestCase("light", false)]
    [TestCase("dark", false)]
    [TestCase("custom", false)]
    [TestCase("light", true)]
    [TestCase("dark", true)]
    [TestCase("custom", true)]
    public void Native_toolbar_should_inherit_the_source_item_foreground_instead_of_Fluent_text(string palette, bool dropDown)
    {
        using PaletteFixture fixture = new(palette, dropDown);
        fixture.ColorOf(fixture.Owner.Foreground).Should().Be(fixture.Expected);
        fixture.Owner.Focus();
        fixture.Settle();
        fixture.ColorOf(fixture.Owner.Foreground).Should().Be(fixture.Expected,
            "the original enabled ToolStrip item's ambient ForeColor does not change with keyboard focus");

        if (fixture.Owner is IconSplitButton)
        {
            TextBlock caption = fixture.Owner.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Text == "Branch");
            fixture.ColorOf(caption.Foreground).Should().Be(fixture.Expected,
                "the actual native caption must consume the same foreground as the branch dropdown inherits");
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Native_toolbar_should_follow_live_palette_replacement_without_overriding_authored_foreground(bool dropDown)
    {
        using PaletteFixture fixture = new("custom", dropDown);
        Color updated = Color.Parse("#FFD4B890");
        fixture.Window.Resources["GitExtensionsSourceControlTextBrush"] = new SolidColorBrush(updated);
        fixture.Settle();
        fixture.ColorOf(fixture.Owner.Foreground).Should().Be(updated);

        Color authored = Color.Parse("#FFB6D8FA");
        fixture.Owner.Foreground = new SolidColorBrush(authored);
        fixture.Window.Resources["GitExtensionsSourceControlTextBrush"] = Brushes.Red;
        fixture.Settle();
        fixture.ColorOf(fixture.Owner.Foreground).Should().Be(authored,
            "the source's explicitly assigned ForeColor takes precedence over its ambient default");

        fixture.Owner.ClearValue(TemplatedControl.ForegroundProperty);
        fixture.Settle();
        fixture.ColorOf(fixture.Owner.Foreground).Should().Be(Colors.Red);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Source_toolbar_foreground_should_not_change_unopted_or_disabled_framework_controls(bool dropDown)
    {
        using PaletteFixture fixture = new("custom", dropDown);
        TemplatedControl ordinary = dropDown ? new DropDownButton() : new SplitButton();
        fixture.Panel.Children.Add(ordinary);
        fixture.Owner.IsEnabled = false;
        fixture.Settle();
        Color ordinaryBefore = fixture.ColorOf(ordinary.Foreground);
        Color disabledBefore = fixture.ColorOf(fixture.Owner.Foreground);
        fixture.Window.Resources["GitExtensionsSourceControlTextBrush"] = Brushes.Red;
        fixture.Settle();
        fixture.ColorOf(ordinary.Foreground).Should().Be(ordinaryBefore);
        fixture.ColorOf(fixture.Owner.Foreground).Should().Be(disabledBefore,
            "the enabled-source-role correction must not silently change unmeasured disabled paint");

        fixture.Owner.IsEnabled = true;
        fixture.Settle();
        fixture.ColorOf(fixture.Owner.Foreground).Should().Be(Colors.Red);
    }

    private sealed class PaletteFixture : IDisposable
    {
        public PaletteFixture(string palette, bool dropDown)
        {
            Owner = dropDown
                ? new IconDropDownButton { UseNativeToolStripLayout = true }
                : new IconSplitButton { UseNativeToolStripLayout = true, Content = "Branch" };
            Owner.Classes.Add("gitextensions-toolbar-button");
            Panel = new StackPanel { Children = { Owner } };
            Window = new Window
            {
                Width = 600,
                Height = 200,
                RequestedThemeVariant = palette == "light" ? ThemeVariant.Light : ThemeVariant.Dark,
                Content = Panel,
            };
            Expected = palette switch
            {
                "light" => Colors.Black,
                "dark" => Color.Parse("#FFF0F0F0"),
                "custom" => Color.Parse("#FFC2E3F4"),
                _ => throw new ArgumentOutOfRangeException(nameof(palette)),
            };
            if (palette == "custom")
            {
                Window.Resources["GitExtensionsSourceControlTextBrush"] = new SolidColorBrush(Expected);
            }

            Window.Show();
            Settle();
        }

        public Window Window { get; }

        public StackPanel Panel { get; }

        public TemplatedControl Owner { get; }

        public Color Expected { get; }

        public Color ColorOf(IBrush? brush) => ((ISolidColorBrush)(brush
            ?? throw new InvalidOperationException("The actual foreground is unavailable."))).Color;

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose() => Window.Close();
    }
}
