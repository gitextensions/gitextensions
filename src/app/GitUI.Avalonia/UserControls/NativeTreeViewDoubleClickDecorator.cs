using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace GitUI.UserControls;

// Add double-click related behaviour to NativeTreeView
public class NativeTreeViewDoubleClickDecorator
{
    private readonly TreeView _treeView;

    // Invoked just before an inner tree node is expanded/collapsed. Set CancelEventHandler.Cancel to true
    // to cancel the expand/collapse of the current node.
    public event CancelEventHandler? BeforeDoubleClickExpandCollapse;

    public NativeTreeViewDoubleClickDecorator(TreeView treeView)
    {
        _treeView = treeView;

        // DoubleTapped only bubbles in Avalonia. Unlike the native WinForms tree, TreeView
        // does not expand on this gesture, so the decorator applies expansion after cancellation.
        _treeView.AddHandler(InputElement.DoubleTappedEvent, HandleBeforeDoubleClickExpandCollapse, RoutingStrategies.Bubble);
    }

    private void HandleBeforeDoubleClickExpandCollapse(object? sender, TappedEventArgs e)
    {
        // We only care about double-clicks on the node itself, not the plus/minus part
        if (e.Source is not Avalonia.Visual source
            || source.GetSelfAndVisualAncestors().OfType<ToggleButton>().Any()
            || source.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault() is not { Items.Count: > 0 } item)
        {
            return;
        }

        CancelEventArgs cancelEventArgs = new();
        BeforeDoubleClickExpandCollapse?.Invoke(_treeView, cancelEventArgs);
        if (!cancelEventArgs.Cancel)
        {
            item.IsExpanded = !item.IsExpanded;
        }

        e.Handled = true;
    }
}
