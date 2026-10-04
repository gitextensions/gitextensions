using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitUI.Compat;
using GitUI.UserControls;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class FilterToolBarCaptureTreeTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Tree_should_report_authored_items_and_actual_native_owner_geometry_and_fonts(bool rightToLeft)
    {
        FilterToolBar filters = new() { Width = 800, FontSize = 11 * 96d / 72 };
        NativeToolStrip strip = filters.Strip;
        Window window = new()
        {
            Width = 1000,
            Height = 240,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Content = new Canvas { Children = { filters } },
        };
        try
        {
            window.Show();
            Settle();
            foreach (int width in new[] { 800, 50, 800 })
            {
                filters.Width = width;
                Settle();
                if (strip.HasOverflow)
                {
                    strip.ShowOverflow();
                    Settle();
                }

                CaptureNode root = new AvaloniaControlTreeReader(filters, renderScale: 1)
                    .ReadPrimary(filters, new PixelSize((int)filters.Bounds.Width, (int)filters.Bounds.Height)).Root;
                root.Children.Select(node => node.FieldName).Should().Equal(strip.Items.Select(item => item.Name));
                root.Font!.SizeDip.Should().Be(decimal.Round((decimal)strip.FontSize, 4));
                foreach (Control item in strip.Items.Where(item => !NativeToolStrip.GetItemIsSeparator(item)))
                {
                    CaptureNode node = root.Children.Single(child => child.FieldName == item.Name);
                    // Separate popup roots have no primary-root transform. The reader
                    // reports their actually arranged current-parent coordinates instead.
                    Point origin = item.TranslatePoint(default, filters) ?? item.Bounds.Position;
                    if (strip.GetItemPlacement(item) == NativeToolStripItemPlacement.Overflow)
                    {
                        item.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
                    }

                    node.BoundsDip.X.Should().Be(decimal.Round((decimal)origin.X, 4));
                    node.BoundsDip.Y.Should().Be(decimal.Round((decimal)origin.Y, 4));
                    node.BoundsDip.Width.Should().Be(decimal.Round((decimal)item.Bounds.Width, 4));
                    node.BoundsDip.Height.Should().Be(decimal.Round((decimal)item.Bounds.Height, 4));
                    node.Margin.Dip.Left.Should().Be(decimal.Round((decimal)item.Margin.Left, 4));
                    node.Margin.Dip.Top.Should().Be(decimal.Round((decimal)item.Margin.Top, 4));
                    node.Font!.SizeDip.Should().Be(decimal.Round((decimal)((Avalonia.Controls.Primitives.TemplatedControl)item).FontSize, 4));
                    node.Children.Should().NotContain(child => child.Type.Contains(nameof(NativeToolStripSplitButtonFrame), StringComparison.Ordinal));
                }

                CaptureNode separator = root.Children.Single(node => node.FieldName == "toolStripSeparator19");
                separator.Children.Should().NotContain(child => child.Type.Contains(nameof(NativeToolStripSeparatorChrome), StringComparison.Ordinal));
                strip.CloseOverflow();
                Settle();
            }
        }
        finally
        {
            window.Close();
            strip.Dispose();
        }

        void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
