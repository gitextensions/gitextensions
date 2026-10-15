using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using SourceControls = GitUI.Compat.WinFormsControls;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class MainWindowToolStripLayoutTests
{
    [SetUp]
    public void SetUp() => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(1800, 9, false)]
    [TestCase(923, 9, false)]
    [TestCase(600, 9, false)]
    [TestCase(1800, 11, false)]
    [TestCase(1800, 14, false)]
    [TestCase(1800, 9, true)]
    [TestCase(923, 11, true)]
    [TestCase(600, 14, true)]
    public void ToolStripPanel_should_keep_preferred_widths_and_shrink_the_rightmost_owners_before_main(int width, int points, bool explicitOwnerFont)
    {
        using NativeToolStrip main = new();
        using NativeToolStrip filters = new();
        using NativeToolStrip scripts = new() { Name = "ToolStripScripts", GripVisible = false };
        SourceSizedControl fixedMain = new(new Size(900, 22)) { Margin = new Thickness(0, 1, 0, 2) };
        NativeToolStrip.SetItemAutoSize(fixedMain, false);
        main.Items.Add(fixedMain);
        IconSplitButton branch = NewBranch("Avalonia_Port");
        main.Items.Add(branch);
        SourceSizedControl fixedFilter = new(new Size(500, 22)) { Margin = new Thickness(0, 1, 0, 2) };
        NativeToolStrip.SetItemAutoSize(fixedFilter, false);
        filters.Items.Add(fixedFilter);
        if (explicitOwnerFont)
        {
            main.FontSize = points * 96d / 72;
            filters.FontSize = points * 96d / 72;
        }

        Grid mainHost = new() { Name = "toolStripMainHost", Children = { main } };
        Grid filterHost = new() { Name = "toolStripFiltersHost", Children = { filters } };
        SourceControls.ToolStripPanel top = new() { Name = "_topPanel", Children = { mainHost, filterHost, scripts } };
        Border body = new() { Name = "_contentPanel" };
        SourceControls.ToolStripContainer container = new() { Children = { top, body } };
        Window window = new() { Width = width, Height = 180, FontSize = points * 96d / 72, Content = container };
        try
        {
            window.Show();
            Settle(window);
            NativeToolStrip[] owners = [main, filters, scripts];
            double[] expectedWidths = owners.Select(owner => owner.PreferredSize.Width).ToArray();
            double excess = expectedWidths.Sum() - (width - 11);
            for (int i = expectedWidths.Length - 1; i >= 0 && excess > 0; i--)
            {
                double freed = Math.Min(excess, Math.Max(0, expectedWidths[i] - 50));
                expectedWidths[i] -= freed;
                excess -= freed;
            }

            owners.Select(owner => owner.Bounds.Width).Should().Equal(expectedWidths);
            scripts.PreferredSize.Should().Be(new Size(102, 25));
            branch.FontSize.Should().Be(explicitOwnerFont ? points * 96d / 72 : 12);
            mainHost.Bounds.X.Should().Be(7);
            filterHost.Bounds.X.Should().Be(mainHost.Bounds.Right);
            scripts.Bounds.X.Should().Be(filterHost.Bounds.Right);
            scripts.Bounds.Right.Should().BeLessThanOrEqualTo(width - 4);
            top.Bounds.Height.Should().Be(owners.Max(owner => owner.PreferredSize.Height));
            body.Bounds.Y.Should().Be(top.Bounds.Height);
            body.Bounds.Height.Should().Be(container.Bounds.Height - top.Bounds.Height);
            if (width == 1800)
            {
                filterHost.Bounds.Width.Should().Be(filters.PreferredSize.Width, "the source AutoSize owner does not stretch into unused row space");
                scripts.Bounds.Width.Should().Be(scripts.PreferredSize.Width);
                scripts.Bounds.Right.Should().BeLessThan(width - 4);
            }
            else
            {
                scripts.Bounds.Width.Should().Be(50);
                filterHost.Bounds.Width.Should().Be(50);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    [TestCase(14)]
    public void Browse_should_follow_actual_filter_and_script_owner_heights_and_reclaim_hidden_owners(int points)
    {
        using FormBrowse form = new() { Width = 1800, Height = 600, FontSize = points * 96d / 72 };
        form.Show();
        try
        {
            Settle(form);
            NativeToolStrip main = form.FindControl<NativeToolStrip>("ToolStripMain")!;
            FilterToolBar filters = form.FindControl<FilterToolBar>("ToolStripFilters")!;
            NativeToolStrip scripts = form.FindControl<NativeToolStrip>("ToolStripScripts")!;
            SourceControls.ToolStripPanel top = form.FindControl<SourceControls.ToolStripPanel>("_topPanel")!;
            Grid body = form.FindControl<Grid>("_contentPanel")!;
            main.FontSize.Should().Be(12);
            filters.Strip.FontSize.Should().Be(12);
            scripts.FontSize.Should().Be(12);
            top.Bounds.Height.Should().Be(new[] { main.PreferredSize.Height, filters.Strip.PreferredSize.Height, scripts.PreferredSize.Height }.Max());
            body.Bounds.Y.Should().Be(top.Bounds.Height);
            form.toolStripFiltersHost.Bounds.Height.Should().Be(filters.Strip.PreferredSize.Height);
            scripts.GripVisible.Should().BeFalse();
            filters.IsVisible = false;
            Settle(form);
            scripts.Bounds.X.Should().Be(form.toolStripMainHost.Bounds.Right);
            main.IsVisible = false;
            Settle(form);
            scripts.Bounds.X.Should().Be(7);
            scripts.IsVisible = false;
            Settle(form);
            top.Bounds.Height.Should().Be(0);
            body.Bounds.Y.Should().Be(0);
            filters.IsVisible = true;
            filters.FontSize = points * 96d / 72;
            Settle(form);
            form.toolStripFiltersHost.Bounds.X.Should().Be(7);
            top.Bounds.Height.Should().Be(filters.Strip.PreferredSize.Height);
            body.Bounds.Y.Should().Be(filters.Strip.PreferredSize.Height);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("Avalonia_Port", "Avalonia_Port", 112)]
    [TestCase("feature&&docs", "feature&docs", 0)]
    [TestCase("Translated &branch_name", "Translated branch_name", 0)]
    public void Branch_caption_should_preserve_original_literal_and_access_key_notation(string source, string displayed, int nativeWidth)
    {
        using NativeToolStrip strip = new();
        IconSplitButton branch = NewBranch(source);
        strip.Items.Add(branch);
        Window window = new() { Width = 400, Height = 80, Content = strip };
        try
        {
            window.Show();
            Settle(window);
            AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(branch.Content as string ?? string.Empty).Should().Be(displayed);
            (branch.Content as string).Should().Be(AvaloniaTranslationUtils.ToAvaloniaMnemonics(source));
            Size caption = WinFormsTextMeasurer.MeasureTextRenderer(branch, OperatingSystem.IsWindows() ? source : displayed);
            branch.Bounds.Width.Should().Be(Math.Ceiling(caption.Width) + 32);
            if (OperatingSystem.IsWindows() && nativeWidth != 0)
            {
                branch.Bounds.Width.Should().Be(nativeWidth, "the native probe independently measures the literal underscore caption");
            }

            TextBlock paintedCaption = branch.GetVisualDescendants().OfType<NativeToolStripSplitContent>().Single().Children.OfType<TextBlock>().Single();
            paintedCaption.Text.Should().Be(branch.Content as string);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Hidden_grip_should_remove_reserve_and_real_paint_without_affecting_default_visible_grip(bool rightToLeft, bool professional)
    {
        using NativeToolStrip strip = new()
        {
            Width = 40,
            Height = 25,
            Background = Brushes.Magenta,
            UseSystemVisualStyle = !professional,
            GripDarkBrush = Brushes.Black,
            GripLightBrush = Brushes.White,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        };
        SourceSizedControl item = new(new Size(23, 22)) { Margin = new Thickness(0, 1, 0, 2) };
        strip.Items.Add(item);
        Window window = new() { Width = 60, Height = 50, Content = new Canvas { Children = { strip } } };
        try
        {
            window.Show();
            Settle(window);
            strip.GripVisible.Should().BeTrue();
            double visiblePreferred = strip.PreferredSize.Width;
            item.Bounds.X.Should().Be(rightToLeft ? 12 : 5);
            strip.GripVisible = false;
            Settle(window);
            strip.PreferredSize.Width.Should().Be(visiblePreferred - 5);
            item.Bounds.X.Should().Be(rightToLeft ? 17 : 0);
            using WriteableBitmap frame = window.CaptureRenderedFrame() ?? throw new AssertionException("The actual hidden-grip frame is unavailable.");
            for (int y = 0; y < 25; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    ReadPixel(frame, rightToLeft ? 35 + x : x, y).Should().Be(Colors.Magenta);
                }
            }

            strip.GripVisible = true;
            Settle(window);
            strip.PreferredSize.Width.Should().Be(visiblePreferred);
            item.Bounds.X.Should().Be(rightToLeft ? 12 : 5);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("Review_branch", true, false)]
    [TestCase("Review&&docs", true, false)]
    [TestCase("Review_branch", false, false)]
    [TestCase("", true, true)]
    public void Scripts_should_remeasure_empty_to_nonempty_to_empty_and_keep_click_and_explicit_size_contracts(string caption, bool hasImage, bool imageOnly)
    {
        using NativeToolStrip strip = new() { GripVisible = false };
        Window window = new() { Width = 400, Height = 80, Content = strip };
        try
        {
            window.Show();
            Settle(window);
            strip.PreferredSize.Should().Be(new Size(102, 25));
            IconButton script = new()
            {
                Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(caption),
                Icon = hasImage ? NewImage() : null,
                Margin = new Thickness(0, 1, 0, 2),
                Tag = "userscript",
            };
            if (imageOnly)
            {
                script.Classes.Add("gitextensions-icon-only");
            }

            NativeToolStrip.SetItemUseNativeButtonSize(script, true);
            int clicks = 0;
            script.Click += (_, _) => clicks++;
            strip.Items.Add(script);
            strip.GripVisible = true;
            Settle(window);
            string displayed = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(script.Content as string ?? string.Empty);
            Size text = caption.Length == 0 ? default
                : WinFormsTextMeasurer.MeasureTextRenderer(script, OperatingSystem.IsWindows() ? caption : displayed);
            double preferredWidth = Math.Max(23, Math.Ceiling(text.Width) + (hasImage ? 16 : 0) + 4);
            strip.PreferredSize.Width.Should().Be(preferredWidth + 7);
            script.Bounds.Width.Should().Be(preferredWidth);
            script.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            clicks.Should().Be(1);
            script.Width = 80;
            script.Height = 28;
            NativeToolStrip.SetItemAutoSize(script, false);
            NativeToolStrip.SetItemHeight(script, 28);
            Settle(window);
            script.Bounds.Size.Should().Be(new Size(80, 28));
            strip.PreferredSize.Height.Should().Be(31);
            strip.Items.Remove(script);
            strip.GripVisible = false;
            Settle(window);
            strip.Items.Should().BeEmpty();
            strip.PreferredSize.Should().Be(new Size(102, 25));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9, false)]
    [TestCase(11, false)]
    [TestCase(14, false)]
    [TestCase(9, true)]
    [TestCase(11, true)]
    [TestCase(14, true)]
    public void Closed_overflow_host_margins_should_feed_source_preferred_size_and_reclaim_on_regrowth(int points, bool rightToLeft)
    {
        using NativeToolStrip strip = new() { FontSize = points * 96d / 72 };
        ToolbarComboBox[] hosts = [new() { Width = 100 }, new() { Width = 100 }];
        foreach (ToolbarComboBox host in hosts)
        {
            NativeToolStrip.SetItemIsControlHost(host, true);
            NativeToolStrip.SetItemAutoSize(host, false);
            NativeToolStrip.SetItemHeight(host, 23);
            strip.Items.Add(host);
        }

        Window window = new()
        {
            Width = 800,
            Height = 120,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Content = strip,
        };
        try
        {
            window.Show();
            Settle(window);
            strip.PreferredSize.Should().Be(new Size(211, 25));
            hosts.Should().OnlyContain(host => host.Margin == new Thickness(1, 0, 1, 0));
            strip.Width = 50;
            Settle(window);
            strip.IsOverflowOpen.Should().BeFalse();
            hosts.Should().OnlyContain(host => strip.GetItemPlacement(host) == NativeToolStripItemPlacement.Overflow);
            hosts.Should().OnlyContain(host => host.Margin == new Thickness(2));
            strip.PreferredSize.Should().Be(new Size(215, 27),
                "source GetPreferredSizeHorizontal includes current overflow margins even when the popup is closed");
            hosts.Should().OnlyContain(host => host.FontSize == 12,
                "native ComboBox control fonts stay at the default menu font independently of the owner font");
            strip.Width = double.NaN;
            Settle(window);
            strip.PreferredSize.Should().Be(new Size(211, 25));
            hosts.Should().OnlyContain(host => host.Margin == new Thickness(1, 0, 1, 0));
            strip.HasOverflow.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    private static IconSplitButton NewBranch(string source) => new()
    {
        UseNativeToolStripLayout = true,
        Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(source),
        Margin = new Thickness(0, 1, 0, 2),
        Icon = NewImage(),
    };

    private static DrawingImage NewImage() => new()
    {
        Drawing = new GeometryDrawing { Brush = Brushes.Magenta, Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) },
    };

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static Color ReadPixel(WriteableBitmap bitmap, int x, int y)
    {
        using ILockedFramebuffer buffer = bitmap.Lock();
        byte[] pixel = new byte[4];
        Marshal.Copy(IntPtr.Add(buffer.Address, (y * buffer.RowBytes) + (x * 4)), pixel, 0, pixel.Length);
        return buffer.Format == PixelFormat.Bgra8888
            ? Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0])
            : Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2]);
    }

    private sealed class SourceSizedControl(Size preferred) : Control
    {
        protected override bool BypassFlowDirectionPolicies => true;

        protected override Size MeasureOverride(Size availableSize) => preferred;
    }
}
