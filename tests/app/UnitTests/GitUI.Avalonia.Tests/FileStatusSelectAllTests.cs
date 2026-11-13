using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUIPluginInterfaces;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FileStatusSelectAllTests
{
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void SelectAll_should_expand_nested_folders_and_keep_the_viewport(bool fileTree)
    {
        using AppSettingsTestScope settings = new();
        FileStatusList control = new() { SelectionMode = SelectionMode.Multiple };
        GitItemStatus[] files =
        [
            new("folder/first.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree },
            new("folder/nested/second.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree },
            new("other/third.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree },
        ];
        control.SetDiffs(
        [
            new FileStatusWithDescription(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), "working tree", files),
        ], isFileTreeMode: fileTree);
        Window window = new() { Content = control, Width = 800, Height = 500 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            using var before = window.CaptureRenderedFrame();
            TreeView tree = control.FindControl<TreeView>(fileTree ? "tvFiles" : "tvDiffFiles")!;
            tree.IsVisible.Should().BeTrue();
            TreeViewItem[] roots = [.. tree.GetVisualDescendants().OfType<TreeViewItem>().Where(item => item.Level == 0)];
            roots.Should().NotBeEmpty();
            foreach (TreeViewItem root in roots)
            {
                tree.SelectedItem = root.DataContext;
                control.GetTestAccessor().CollapseAllMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            }

            Dispatcher.UIThread.RunJobs();
            ScrollViewer scroll = tree.GetVisualDescendants().OfType<ScrollViewer>().First();
            var offset = scroll.Offset;
            bool autoScroll = tree.AutoScrollToSelectedItem;
            control.SelectAll();
            Dispatcher.UIThread.RunJobs();
            using var after = window.CaptureRenderedFrame();

            tree.GetVisualDescendants().OfType<TreeViewItem>().Where(item => item.Items.Count > 0)
                .Should().OnlyContain(item => item.IsExpanded);
            control.SelectedGitItems.Should().BeEquivalentTo(files);
            tree.GetVisualDescendants().OfType<TreeViewItem>().Where(item => item.Items.Count == 0 && item.IsEffectivelyVisible)
                .Should().HaveCount(3);
            scroll.Offset.Should().Be(offset);
            tree.AutoScrollToSelectedItem.Should().Be(autoScroll);
        }
        finally
        {
            window.Close();
            (control as IDisposable)?.Dispose();
        }
    }
}
