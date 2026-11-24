using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.CommandsDialogs;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class BrowseTabCaptionPaintTests
{
    [AvaloniaTest]
    [TestCase(false, "#FF000000", false)]
    [TestCase(false, "#FF000000", true)]
    [TestCase(true, "#FFF0F0F0", false)]
    [TestCase(true, "#FFF0F0F0", true)]
    [TestCase(true, "#FFE8D2AC", false)]
    [TestCase(true, "#FFE8D2AC", true)]
    public void Enabled_captions_should_paint_the_live_source_control_text_for_every_tab_and_selection(
        bool dark, string foregroundArgb, bool hover)
    {
        using RenderTargetBitmap icon = new(new PixelSize(16, 16));
        TabItem[] pages = [.. new[] { "Commit", "Diff", "File tree", "GPG", "Output" }
            .Select(caption => new TabItem
            {
                Header = caption,
                Icon = icon,
                Content = new Border(),
                Classes = { "gitextensions-workspace-tab" },
            })];
        FullBleedTabControl tabs = new()
        {
            SelectedIndex = 0,
            Classes = { "gitextensions-workspace-tabs" },
        };
        foreach (TabItem page in pages)
        {
            tabs.Items.Add(page);
        }

        Window window = new()
        {
            Width = 640,
            Height = 240,
            Content = tabs,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
            Classes = { "gitextensions-browse-window" },
        };
        Color foreground = Color.Parse(foregroundArgb);
        window.Resources["GitExtensionsSourceControlTextBrush"] = new SolidColorBrush(foreground);
        try
        {
            window.Show();
            foreach (int selected in Enumerable.Range(0, pages.Length))
            {
                tabs.SelectedIndex = selected;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                for (int index = 0; index < pages.Length; index++)
                {
                    TabItem page = pages[index];
                    if (hover)
                    {
                        Point origin = page.TranslatePoint(default, window)
                            ?? throw new AssertionException("The source-shaped tab must have an actual header position.");
                        window.MouseMove(origin + new Vector(page.Bounds.Width / 2, page.Bounds.Height / 2));
                        Dispatcher.UIThread.RunJobs();
                        page.IsPointerOver.Should().BeTrue();
                    }

                    page.IsEffectivelyEnabled.Should().BeTrue();
                    AccessText caption = page.GetVisualDescendants().OfType<AccessText>().Single();
                    caption.Foreground.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(foreground,
                        "the original enabled {0} caption stays ControlText, including selection={1} and hover={2}",
                        page.Header, selected == index, hover);
                    caption.Opacity.Should().Be(1);
                    page.Opacity.Should().Be(1);
                }
            }

            Color changed = Color.Parse("#FF8AD4C1");
            window.Resources["GitExtensionsSourceControlTextBrush"] = new SolidColorBrush(changed);
            Dispatcher.UIThread.RunJobs();
            foreach (TabItem page in pages)
            {
                page.GetVisualDescendants().OfType<AccessText>().Single().Foreground
                    .Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(changed,
                        "source-system color publication must repaint an already-created tab header");
            }
        }
        finally
        {
            window.Close();
        }
    }
}
