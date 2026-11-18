using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GitUI.Compat;

namespace GitUI.UserControls;

// Add double-click related behaviour to NativeTreeView
public class NativeTreeViewDoubleClickDecorator
{
    private readonly TreeView _treeView;

    private int _mouseClicks = 0;
    private DateTime _lastMouseDown = DateTime.MinValue;

    private readonly Func<DateTime> _getCurrentTime;

    // Invoked just before an inner tree node is expanded/collapsed. Set CancelEventHandler.Cancel to true
    // to cancel the expand/collapse of the current node.
    public event CancelEventHandler? BeforeDoubleClickExpandCollapse;

    public NativeTreeViewDoubleClickDecorator(TreeView treeView, Func<DateTime> getCurrentTime)
    {
        _treeView = treeView;

        // Avalonia exposes expansion after the property changes; repository items supply the cancellable boundary.
        _treeView.AddHandler(NativeTreeViewItem.ExpansionChangingEvent, HandleBeforeDoubleClickExpandCollapse, RoutingStrategies.Bubble);
        _treeView.AddHandler(InputElement.PointerPressedEvent, OnMouseDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        _getCurrentTime = getCurrentTime;
    }

    public NativeTreeViewDoubleClickDecorator(TreeView treeView)
        : this(treeView, () => DateTime.Now)
    {
    }

    private void HandleBeforeDoubleClickExpandCollapse(object? sender, NativeTreeViewItem.ExpansionChangingEventArgs e)
    {
        // Once detected, reset double-clicked state so that if BeforeDoubleClickExpandCollapse does
        // anything that collapses/expands nodes on the tree, this function won't erroneously resend
        // this event.
        bool doubleClicked = IsDoubleClickStateSet();
        ResetDoubleClickState();

        if (doubleClicked)
        {
            CancelEventArgs cancelEventArgs = new();
            BeforeDoubleClickExpandCollapse?.Invoke(sender, cancelEventArgs);
            e.Cancel = cancelEventArgs.Cancel;
        }
    }

    private void OnMouseDown(object? sender, PointerPressedEventArgs e)
    {
        // We only care about double-clicks on the node itself, not the plus/minus part
        if (e.Source is Avalonia.Visual source && source.GetSelfAndVisualAncestors().OfType<ToggleButton>().Any())
        {
            return;
        }

        // Use _mouseClicks and _lastMouseDown to detect double-clicks, and set _lastDoubleClick when detected
        ++_mouseClicks;

        DateTime now = _getCurrentTime();

        if (_mouseClicks == 2)
        {
            _mouseClicks = 0;
        }

        _lastMouseDown = now;
    }

    // Returns true if a double-click occurred very recently
    private bool IsDoubleClickStateSet()
    {
        int delta = (int)DateTime.Now.Subtract(_lastMouseDown).TotalMilliseconds;
        return delta is (>= 0 and < 200);
    }

    private void ResetDoubleClickState()
    {
        _lastMouseDown = DateTime.MinValue;
    }
}
