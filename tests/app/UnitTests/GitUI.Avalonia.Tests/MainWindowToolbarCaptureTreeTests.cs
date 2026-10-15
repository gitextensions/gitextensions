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
