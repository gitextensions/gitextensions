using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
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
public sealed class MainWindowToolStripParentLayoutTests
{
    [SetUp]
    public void SetUp() => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(9, false)]
    [TestCase(11, false)]
    [TestCase(14, false)]
    [TestCase(9, true)]
    [TestCase(11, true)]
    [TestCase(14, true)]
    public void Browse_wrapper_should_propagate_native_preferred_size_through_template_and_row_during_closed_overflow(int points, bool explicitOwnerFont)
    {
        const int SourceToolStripDefaultHeight = 25;
        using FormBrowse form = new() { Width = 1800, Height = 573, FontSize = points * 96d / 72 };
        FilterToolBar filters = form.FindControl<FilterToolBar>("ToolStripFilters")
            ?? throw new InvalidOperationException("The actual Browse filter wrapper must exist.");
        NativeToolStrip main = form.FindControl<NativeToolStrip>("ToolStripMain")
            ?? throw new InvalidOperationException("The actual Browse main strip must exist.");
        NativeToolStrip scripts = form.FindControl<NativeToolStrip>("ToolStripScripts")
            ?? throw new InvalidOperationException("The actual Browse scripts strip must exist.");
        SourceControls.ToolStripPanel row = form.FindControl<SourceControls.ToolStripPanel>("_topPanel")
            ?? throw new InvalidOperationException("The actual Browse toolbar row must exist.");
        Grid body = form.FindControl<Grid>("_contentPanel")
            ?? throw new InvalidOperationException("The actual Browse content panel must exist.");
        if (explicitOwnerFont)
        {
            filters.FontSize = points * 96d / 72;
        }

        try
        {
            form.Show();
            bool sawOverflow = false;
            bool sawMain = false;
            foreach (int width in new[] { 1800, 923, 600, 1277, 1800, 923 })
            {
                form.Width = width;
                Settle(form);
                filters.Strip.IsOverflowOpen.Should().BeFalse();
                sawOverflow |= filters.Strip.HasOverflow;
                sawMain |= !filters.Strip.HasOverflow;
                double preferredHeight = Math.Max(SourceToolStripDefaultHeight,
                    filters.Strip.Items.Where(item => item.IsVisible && NativeToolStrip.GetItemOverflow(item) != NativeToolStripItemOverflow.Always)
                        .Select(item => filters.Strip.GetItemPreferredSize(item).Height + item.Margin.Top + item.Margin.Bottom)
                        .DefaultIfEmpty().Max());
                filters.Strip.PreferredSize.Height.Should().Be(preferredHeight);
                form.toolStripFiltersHost.Bounds.Height.Should().Be(preferredHeight);
                filters.Bounds.Height.Should().Be(preferredHeight);
                filters.Strip.Bounds.Height.Should().Be(preferredHeight);
                row.Bounds.Height.Should().Be(new[] { main.PreferredSize.Height, preferredHeight, scripts.PreferredSize.Height }.Max());
                body.Bounds.Y.Should().Be(row.Bounds.Height);
                Size preferred = filters.Strip.PreferredSize;
                Rect bounds = form.toolStripFiltersHost.Bounds;
                Settle(form);
                filters.Strip.PreferredSize.Should().Be(preferred);
                form.toolStripFiltersHost.Bounds.Should().Be(bounds);
                filters.Strip.IsMeasureValid.Should().BeTrue();
                filters.Strip.IsArrangeValid.Should().BeTrue();
                row.IsMeasureValid.Should().BeTrue();
                row.IsArrangeValid.Should().BeTrue();
            }

            sawOverflow.Should().BeTrue();
            sawMain.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}
