using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.CommandsDialogs;
using SkiaSharp;
using Point = Avalonia.Point;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class BrowseTabOutlineTests
{
    [AvaloniaTest]
    [TestCase("#FFE5E5E5", "#FFF9F9F9")]
    [TestCase("#FF424242", "#FF202020")]
    [TestCase("#FF416D35", "#FFF6F2DE")]
    public void Selected_first_header_should_paint_its_left_outline_inside_the_owner_then_join_the_content_frame(
        string outlineArgb, string selectedArgb)
    {
        using TabFixture fixture = new(outlineArgb, selectedArgb);
        Border selectedOutline = fixture.GetOutline(0);
        Point headerOrigin = fixture.GetOrigin(selectedOutline);
        headerOrigin.X.Should().Be(0,
            "the native selected first tab expands to the page frame's left edge");
        fixture.Pages[0].ClipToBounds.Should().BeFalse(
            "native selected headers paint outside their fixed slot without moving adjacent captions");
        fixture.Tabs.ClipToBounds.Should().BeTrue(
            "only the header slot clip is released; the complete source pane remains clipped");
        using SKBitmap frame = fixture.Capture();
        SKColor outline = ToSkColor(Color.Parse(outlineArgb));
        int headerEnd = (int)Math.Floor(headerOrigin.Y + selectedOutline.Bounds.Height);
        for (int y = (int)headerOrigin.Y + 1; y < headerEnd; y++)
        {
            frame.GetPixel(0, y).Should().Be(outline,
                "the selected native tab retains its one-pixel left outline at owner row {0}", y);
        }

        Border pageFrame = fixture.Tabs.GetVisualDescendants().OfType<Border>()
            .Single(border => border.Classes.Contains("gitextensions-workspace-page-frame"));
        Point pageOrigin = fixture.GetOrigin(pageFrame);
        pageOrigin.X.Should().Be(headerOrigin.X);
        frame.GetPixel(0, (int)pageOrigin.Y + 2).Should().Be(outline);

        fixture.Tabs.SelectedIndex = 1;
        fixture.Layout();
        Border unselectedOutline = fixture.GetOutline(0);
        Point unselectedOrigin = fixture.GetOrigin(unselectedOutline);
        unselectedOrigin.X.Should().Be(2,
            "the source reserves the initial unselected tab's inset without moving other tabs");
        using SKBitmap unselected = fixture.Capture();
        unselected.GetPixel((int)unselectedOrigin.X, (int)unselectedOrigin.Y + 3).Should().Be(outline);
        unselected.GetPixel(0, (int)unselectedOrigin.Y + 3).Should().Be(SKColors.White);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Header_selection_should_leave_neighbor_slots_caption_x_and_selected_content_allocation_fixed(bool translated)
    {
        using TabFixture fixture = new("#FFE5E5E5", "#FFF9F9F9", translated);
        Rect[] allocations = fixture.Pages.Select(page => page.Bounds).ToArray();
        Point[] captions = fixture.Pages.Select(page => fixture.GetOrigin(
            page.GetVisualDescendants().OfType<AccessText>().Single())).ToArray();
        Point[] icons = fixture.Pages.Select(page => fixture.GetOrigin(
            page.GetVisualDescendants().OfType<Image>().Single())).ToArray();
        ContentPresenter content = fixture.Tabs.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "PART_SelectedContentHost");
        Rect contentBounds = content.Bounds;
        Point contentOrigin = fixture.GetOrigin(content);
        double totalHeight = fixture.Tabs.Bounds.Height;

        foreach (int selectedIndex in new[] { 1, 2, 0, 2, 1, 0 })
        {
            fixture.Tabs.SelectedIndex = selectedIndex;
            fixture.Layout();

            fixture.Pages.Select(page => page.Bounds).Should().Equal(allocations);
            fixture.Tabs.Bounds.Height.Should().Be(totalHeight);
            content.Bounds.Should().Be(contentBounds);
            fixture.GetOrigin(content).Should().Be(contentOrigin);
            fixture.SelectedBodies[selectedIndex].Bounds.Height.Should().Be(contentBounds.Height);
            for (int index = 0; index < fixture.Pages.Length; index++)
            {
                TabItem page = fixture.Pages[index];
                Point caption = fixture.GetOrigin(page.GetVisualDescendants().OfType<AccessText>().Single());
                Point icon = fixture.GetOrigin(page.GetVisualDescendants().OfType<Image>().Single());
                caption.X.Should().Be(captions[index].X);
                icon.X.Should().Be(icons[index].X);
                Point surface = fixture.GetOrigin(fixture.GetOutline(index));
                double rise = surface.Y - fixture.GetOrigin(page).Y;
                rise.Should().Be(index == selectedIndex ? 0 : 2,
                    "only the selected native outline, icon and caption rise within a fixed tab slot");
                if (index > 0 && index != selectedIndex)
                {
                    caption.Should().Be(captions[index]);
                    icon.Should().Be(icons[index]);
                }
            }
        }
    }

    private static SKColor ToSkColor(Color color) => new(color.R, color.G, color.B, color.A);

    private sealed class TabFixture : IDisposable
    {
        private readonly RenderTargetBitmap _icon = new(new PixelSize(16, 16));

        public TabFixture(string outlineArgb, string selectedArgb, bool translated = false)
        {
            SelectedBodies = [new Border(), new Border(), new Border()];
            Pages =
            [
                CreatePage(translated ? "Übertragungen" : "Commit", SelectedBodies[0]),
                CreatePage(translated ? "Unterschiede" : "Diff", SelectedBodies[1]),
                CreatePage(translated ? "Dateibaum" : "File tree", SelectedBodies[2]),
            ];
            Tabs = new FullBleedTabControl
            {
                SelectedIndex = 0,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
            Tabs.Classes.Add("gitextensions-workspace-tabs");
            foreach (TabItem page in Pages)
            {
                Tabs.Items.Add(page);
            }

            Window = new Window { Width = 640, Height = 240, Content = Tabs, Background = Brushes.White };
            Window.Classes.Add("gitextensions-browse-window");
            Window.Resources["GitExtensionsUiFontSize"] = 12d;
            Window.Resources["GitExtensionsWindowBackgroundBrush"] = Brushes.White;
            Window.Resources["GitExtensionsNativeTabBorderBrush"] = new SolidColorBrush(Color.Parse(outlineArgb));
            Window.Resources["GitExtensionsBrowseTabSelectedBackgroundBrush"] = new SolidColorBrush(Color.Parse(selectedArgb));
            Window.Show();
            Layout();
        }

        public Window Window { get; }

        public FullBleedTabControl Tabs { get; }

        public TabItem[] Pages { get; }

        public Border[] SelectedBodies { get; }

        public Border GetOutline(int index)
            => Pages[index].GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_LayoutRoot");

        public Point GetOrigin(Visual visual)
            => visual.TranslatePoint(default, Window)
                ?? throw new AssertionException("The tab fixture must retain its actual styled visual tree.");

        public void Layout()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public SKBitmap Capture()
        {
            using WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual tab fixture must render a frame.");
            using MemoryStream stream = new();
            frame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            return SKBitmap.Decode(stream)
                ?? throw new AssertionException("The actual tab frame must decode to pixel data.");
        }

        public void Dispose()
        {
            Window.Close();
            _icon.Dispose();
        }

        private TabItem CreatePage(string caption, Border content)
        {
            TabItem page = new() { Header = caption, Icon = _icon, Content = content };
            page.Classes.Add("gitextensions-workspace-tab");
            return page;
        }
    }
}
