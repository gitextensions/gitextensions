using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class MainWindowToolbarCaptureTreeTests
{
    [AvaloniaTest]
    [TestCase(false, "feature_branch")]
    [TestCase(true, "feature_branch")]
    [TestCase(false, "feature__branch")]
    [TestCase(true, "feature__branch")]
    [TestCase(false, "R&D_branch")]
    [TestCase(true, "R&D_branch")]
    public void Tree_should_record_source_branch_caption_and_actual_filter_item_foreground_not_arrow_color(bool dark, string caption)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = new()
        {
            Width = 1800,
            Height = 600,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
        };
        IconSplitButton branch = form.FindControl<IconSplitButton>("branchSelect")!;
        branch.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(caption);
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            CaptureNode tree = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize(1800, 600)).Root;
            CaptureNode[] nodes = Enumerate(tree).ToArray();
            nodes.Single(node => node.FieldName == "branchSelect").Text.Should().Be(caption,
                "AccessText escapes are framework storage, not part of the original ToolStripItem.Text");
            FilterToolBar filters = form.FindControl<FilterToolBar>("ToolStripFilters")!;
            foreach (string field in new[] { "tsddbtnBranchFilter", "tsddbtnRevisionFilter", "tslblRevisionFilter", "tsmiShowOnlyFirstParent" })
            {
                Control item = filters.FindControl<Control>(field)!;
                Color actual = ((ISolidColorBrush)((Avalonia.Controls.Primitives.TemplatedControl)item).Foreground!).Color;
                CaptureNode node = nodes.Single(node => node.FieldName == field);
                node.Colors.Foreground.Should().Be($"#{actual.A:X2}{actual.R:X2}{actual.G:X2}{actual.B:X2}");
                node.Colors.Background.Should().Be("#00FFFFFF",
                    "the original ToolStripItem's raw background is transparent, independent of its owner's painted surface");
                node.Colors.Border.Should().BeNull();
                if (dark)
                {
                    actual.Should().Be(Color.Parse("#FFF0F0F0"),
                        "the item's ambient foreground differs from the dropdown arrow's SystemColors.ControlText");
                }
            }

            CaptureNode separator = nodes.Single(node => node.FieldName == "toolStripSeparator19");
            separator.Colors.Background.Should().Be("#00FFFFFF",
                "the source separator's raw BackColor is transparent, not its owner's painted background");
            separator.Colors.DisabledBackground.Should().Be("#00FFFFFF");
            Border actualSeparator = filters.FindControl<Border>("toolStripSeparator19")!;
            foreach (Color authored in new[] { Colors.CornflowerBlue, Color.Parse("#00FFFFFF") })
            {
                actualSeparator.Background = new SolidColorBrush(authored);
                CaptureNode authoredSeparator = Enumerate(new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize(1800, 600)).Root).Single(node => node.FieldName == "toolStripSeparator19");
                authoredSeparator.Colors.Background.Should().Be($"#{authored.A:X2}{authored.R:X2}{authored.G:X2}{authored.B:X2}");
                authoredSeparator.Colors.DisabledBackground.Should().Be(authoredSeparator.Colors.Background);
            }

            actualSeparator.ClearValue(Border.BackgroundProperty);
            Enumerate(new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize(1800, 600)).Root).Single(node => node.FieldName == "toolStripSeparator19")
                .Colors.Background.Should().Be("#00FFFFFF", "clearing a local separator background restores its native transparent role");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9, 600)]
    [TestCase(9, 1800)]
    [TestCase(11, 600)]
    [TestCase(11, 1800)]
    [TestCase(14, 600)]
    [TestCase(14, 1800)]
    public void Tree_should_record_the_live_toolbar_panel_height_instead_of_a_fixed_projection(int points, int width)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = new() { Width = width, Height = 600 };
        foreach (Control control in new[] { form.ToolStripMain, (Control)form.FindControl<FilterToolBar>("ToolStripFilters")! })
        {
            Avalonia.Controls.Documents.TextElement.SetFontSize(control, points * 96d / 72);
        }

        form.Show();
        try
        {
            foreach (bool visible in new[] { true, false, true })
            {
                form.ToolStripMain.IsVisible = visible;
                form.FindControl<FilterToolBar>("ToolStripFilters")!.IsVisible = visible;
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                Control panel = form.FindControl<Control>("_topPanel")!;
                Control content = form.FindControl<Control>("_contentPanel")!;
                CaptureNode root = new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize(width, 600)).Root;
                CaptureNode topPanel = Enumerate(root).Single(node => node.Type == "System.Windows.Forms.ToolStripPanel"
                    && node.Children.Any(child => child.FieldName == "ToolStripMain"));
                decimal actualHeight = decimal.Round((decimal)panel.Bounds.Height, 4);
                topPanel.BoundsDip.Height.Should().Be(actualHeight);
                topPanel.ClientSizeDip.Height.Should().Be(actualHeight);
                topPanel.BoundsPx.Height.Should().Be((int)Math.Round(panel.Bounds.Height));
                content.Bounds.Y.Should().Be(panel.Bounds.Height,
                    "the actual source-shaped container places its content immediately below its AutoSize toolbar row");
            }
        }
        finally
        {
            form.Close();
        }
    }

    private static IEnumerable<CaptureNode> Enumerate(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children)
        {
            foreach (CaptureNode descendant in Enumerate(child))
            {
                yield return descendant;
            }
        }
    }
}
