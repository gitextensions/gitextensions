using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.Compat;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeToolStripHostedControlTests
{
    [AvaloniaTest]
    [TestCase(9, false)]
    [TestCase(9, true)]
    [TestCase(11, false)]
    [TestCase(11, true)]
    public void Control_hosts_should_keep_menu_fonts_main_preferred_size_and_actual_controls_through_width_cycles(int points, bool rightToLeft)
    {
        using NativeToolStrip strip = new()
        {
            FontSize = points * 96d / 72,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        };
        ToolbarComboBox branches = NewHost("tscboBranchFilter", "retained branch");
        ToolbarComboBox revisions = NewHost("tstxtRevisionFilter", "retained revision");
        Border separator = new() { Name = "toolStripSeparator19", Width = 6 };
        NativeToolStrip.SetItemIsSeparator(separator, true);
        Label branchLabel = NewLabel("toolStripLabel1", "_Branches:");
        Label revisionLabel = NewLabel("tslblRevisionFilter", "_Filter:");
        Control[] original =
        [
            NewSplit("tsbtnAdvancedFilter", string.Empty), NewButton("tsbShowReflog"),
            NewSplit("tssbtnShowBranches", "_All branches"), branchLabel, branches,
            NewButton("tsddbtnBranchFilter"), separator, revisionLabel, revisions,
            NewButton("tsddbtnRevisionFilter"), NewButton("tsmiShowOnlyFirstParent"),
        ];
        strip.Items.AddRange(original);
        Window window = NewWindow(strip, 800);
        try
        {
            window.Show();
            Settle(window);
            Size sourcePreferred = strip.PreferredSize;
            if (OperatingSystem.IsWindows())
            {
                sourcePreferred.Height.Should().Be(points == 9 ? 25 : 27,
                    "the actual native standalone owner probes distinguish the assigned9/11pt font heights");
            }

            branchLabel.Bounds.Width.Should().Be(Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(branchLabel, "Branches:").Width));
            revisionLabel.Bounds.Width.Should().Be(Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(revisionLabel, "Filter:").Width));
            foreach (int width in new[] { 800, 339, 50, 800 })
            {
                strip.Width = width;
                Settle(window);
                strip.Items.Should().Equal(original);
                strip.Items.Should().OnlyContain(item => item.Parent == strip);
                strip.PreferredSize.Should().Be(sourcePreferred,
                    "popup host margins cannot become the main owner's preferred height or overflow boundary");
                ToolbarComboBox[] hosts = [branches, revisions];
                foreach (ToolbarComboBox host in hosts)
                {
                    host.FontFamily.Name.Should().Be("Segoe UI");
                    host.FontSize.Should().Be(12);
                    host.FontStyle.Should().Be(FontStyle.Normal);
                    host.FontWeight.Should().Be(FontWeight.Normal);
                    host.SelectedIndex.Should().Be(0);
                    host.Text.Should().Be(host.Name == branches.Name ? "retained branch" : "retained revision");
                    bool overflow = strip.GetItemPlacement(host) == NativeToolStripItemPlacement.Overflow;
                    host.Margin.Should().Be(overflow ? new Thickness(2) : new Thickness(1, 0, 1, 0));
                    strip.GetCurrentParent(host).Should().BeSameAs(overflow ? strip.OverflowContent : strip);
                    if (!overflow)
                    {
                        host.Bounds.Size.Should().Be(new Size(100, 23));
                        host.Bounds.Y.Should().Be((int)((strip.Bounds.Height - 23) / 2));
                    }
                }

                if (width == 800)
                {
                    strip.HasOverflow.Should().BeFalse();
                    continue;
                }

                strip.HasOverflow.Should().BeTrue();
                strip.ShowOverflow();
                Settle(window);
                strip.IsOverflowOpen.Should().BeTrue();
                Control[] displayed = strip.OverflowItems.Where(item => !NativeToolStrip.GetItemIsSeparator(item)).ToArray();
                strip.OverflowContent.GetVisualChildren().Should().Equal(displayed);
                strip.OverflowContent.Bounds.Width.Should().BeLessThanOrEqualTo(200);
                foreach (ToolbarComboBox host in hosts.Where(host => strip.GetItemPlacement(host) == NativeToolStripItemPlacement.Overflow))
                {
                    host.GetVisualParent().Should().BeSameAs(strip.OverflowContent);
                    host.Bounds.Size.Should().Be(new Size(100, 23));
                    new Rect(strip.OverflowContent.Bounds.Size).Contains(host.Bounds).Should().BeTrue();
                    TextBox editor = GetEditor(host);
                    editor.Focus().Should().BeTrue();
                    editor.IsFocused.Should().BeTrue();
                    editor.SelectionStart = 2;
                    editor.SelectionEnd = 5;
                    editor.SelectedText.Should().Be("tai");
                }

                strip.CloseOverflow();
                Settle(window);
                strip.Items.Should().Equal(original);
                hosts.Should().OnlyContain(host => host.SelectedIndex == 0 && host.Parent == strip);
                strip.PreferredSize.Should().Be(sourcePreferred);
            }

            TextBox regrownEditor = GetEditor(branches);
            regrownEditor.Focus().Should().BeTrue();
            regrownEditor.IsFocused.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Control_host_defaults_should_require_explicit_source_opt_in_and_preserve_authored_values()
    {
        using NativeToolStrip strip = new() { FontSize = 18 };
        ToolbarComboBox host = NewHost("sourceHost", "retained source");
        ToolbarComboBox ordinary = new() { Width = 100, ItemsSource = new[] { "ordinary combo" }, SelectedIndex = 0 };
        using IDisposable? ordinaryCallerFont = ordinary.SetValue(TemplatedControl.FontSizeProperty, 20d, BindingPriority.StyleTrigger);
        host.FontFamily = new FontFamily("Arial");
        host.FontSize = 16;
        host.FontStyle = FontStyle.Italic;
        host.FontWeight = FontWeight.Bold;
        host.Margin = new Thickness(7, 3, 5, 4);
        strip.Items.AddRange([host, ordinary]);
        Window window = NewWindow(strip, 50);
        try
        {
            window.Show();
            Settle(window);
            strip.ShowOverflow();
            Settle(window);
            host.FontFamily.Name.Should().Be("Arial");
            host.FontSize.Should().Be(16);
            host.FontStyle.Should().Be(FontStyle.Italic);
            host.FontWeight.Should().Be(FontWeight.Bold);
            host.Margin.Should().Be(new Thickness(7, 3, 5, 4));
            ordinary.FontSize.Should().Be(18,
                "being an Avalonia ComboBox does not imply the original ToolStripControlHost contract");
            strip.Items.Remove(host);
            Settle(window);
            host.Parent.Should().BeNull();
            host.GetVisualParent().Should().BeNull();
            host.FontSize.Should().Be(16);
            host.Margin.Should().Be(new Thickness(7, 3, 5, 4));
            strip.Dispose();
            ordinary.Parent.Should().BeNull();
            ordinary.FontSize.Should().Be(20);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Control_host_removal_and_disposal_should_release_only_owned_value_scopes()
    {
        using NativeToolStrip strip = new();
        ToolbarComboBox removed = NewHost("removedHost", "retained removed");
        ToolbarComboBox disposed = NewHost("disposedHost", "retained disposed");
        using IDisposable? callerMargin = removed.SetValue(Control.MarginProperty, new Thickness(6), BindingPriority.StyleTrigger);
        using IDisposable? callerFont = removed.SetValue(TemplatedControl.FontSizeProperty, 20d, BindingPriority.StyleTrigger);
        using IDisposable? disposedCallerMargin = disposed.SetValue(Control.MarginProperty, new Thickness(8), BindingPriority.StyleTrigger);
        using IDisposable? disposedCallerFont = disposed.SetValue(TemplatedControl.FontSizeProperty, 22d, BindingPriority.StyleTrigger);
        Thickness disposedMargin = disposed.Margin;
        double disposedFont = disposed.FontSize;
        strip.Items.AddRange([removed, disposed]);
        Window window = NewWindow(strip, 50);
        try
        {
            window.Show();
            Settle(window);
            removed.FontSize.Should().Be(12);
            removed.Margin.Should().Be(new Thickness(2));
            strip.ShowOverflow();
            Settle(window);
            strip.Items.Remove(removed);
            Settle(window);
            removed.Margin.Should().Be(new Thickness(6));
            removed.FontSize.Should().Be(20);
            removed.Parent.Should().BeNull();
            removed.GetVisualParent().Should().BeNull();
            strip.Dispose();
            disposed.Margin.Should().Be(disposedMargin);
            disposed.FontSize.Should().Be(disposedFont);
            disposed.Parent.Should().BeNull();
            disposed.GetVisualParent().Should().BeNull();
        }
        finally
        {
            window.Close();
        }
    }

    private static ToolbarComboBox NewHost(string name, string value)
    {
        ToolbarComboBox combo = new() { Name = name, Width = 100, IsEditable = true, ItemsSource = new[] { value, "other value" }, SelectedIndex = 0 };
        NativeToolStrip.SetItemIsControlHost(combo, true);
        NativeToolStrip.SetItemAutoSize(combo, false);
        NativeToolStrip.SetItemHeight(combo, 23);
        return combo;
    }

    private static Label NewLabel(string name, string caption)
        => new() { Name = name, Content = caption, Padding = new Thickness(0), Margin = new Thickness(0, 1, 0, 2) };

    private static Button NewButton(string name)
        => new() { Name = name, Classes = { "gitextensions-toolbar-button" }, Width = 23, Margin = new Thickness(0, 1, 0, 2) };

    private static IconSplitButton NewSplit(string name, string caption)
        => new()
        {
            Name = name,
            UseNativeToolStripLayout = true,
            Content = caption,
            Margin = new Thickness(0, 1, 0, 2),
            Icon = new DrawingImage { Drawing = new GeometryDrawing { Brush = Brushes.Magenta, Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) } },
        };

    private static TextBox GetEditor(ToolbarComboBox combo)
        => combo.GetVisualDescendants().OfType<TextBox>().Single(editor => editor.Name == "PART_EditableTextBox");

    private static Window NewWindow(NativeToolStrip strip, int width)
    {
        strip.Width = width;
        return new Window { Width = 1200, Height = 240, Content = new Canvas { Children = { strip } } };
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}
