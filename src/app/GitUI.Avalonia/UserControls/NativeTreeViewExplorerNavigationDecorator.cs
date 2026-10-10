using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace GitUI.UserControls;

// Add explorer-like navigation to NativeTreeView:
// * Arrow key navigation to highlight without selecting node
// * Space or Enter key to select node
// * Mouse clicking highlighted node selects it
//
// As this decorator sets TreeView.SelectedNode, you should avoid hooking into
// node selection-based events. Instead, hook into AfterSelect on this decorator
// to know when a node has been selected (either by Space/Enter, or mouse click).
public class NativeTreeViewExplorerNavigationDecorator
{
    private readonly TreeView _treeView;
    private DateTime _lastKeyNavigateTime = DateTime.MinValue;
    private readonly Func<DateTime> _getCurrentTime;
    private bool _pointerSelectionRaised;

    public event EventHandler<SelectionChangedEventArgs>? AfterSelect;

    public NativeTreeViewExplorerNavigationDecorator(TreeView treeView, Func<DateTime> getCurrentTime)
    {
        _treeView = treeView;
        _getCurrentTime = getCurrentTime;

        // Avalonia routes preview keys before its TreeView changes the native selection.
        _treeView.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        _treeView.KeyDown += OnKeyDown;
        _treeView.SelectionChanged += OnAfterSelect;
        _treeView.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        _treeView.AddHandler(InputElement.PointerReleasedEvent, OnNodeMouseClick, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    public NativeTreeViewExplorerNavigationDecorator(TreeView treeView)
        : this(treeView, () => DateTime.Now)
    {
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Suppress the "ding" when Enter is pressed
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
        }
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right)
        {
            _lastKeyNavigateTime = _getCurrentTime();
        }
        else if (e.Key is Key.Space or Key.Enter)
        {
            // Force a reselection of the current node
            _lastKeyNavigateTime = DateTime.MinValue;
            RaiseAfterSelect();
            e.Handled = true;
        }
    }

    private void OnAfterSelect(object? sender, SelectionChangedEventArgs e)
    {
        // If arrow key was used to navigate to this node, don't send OnSelected
        int delta = (int)_getCurrentTime().Subtract(_lastKeyNavigateTime).TotalMilliseconds;
        if (delta is (>= 0 and < 500))
        {
            return;
        }

        _pointerSelectionRaised = true;
        AfterSelect?.Invoke(sender, e);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastKeyNavigateTime = DateTime.MinValue;
        _pointerSelectionRaised = false;
    }

    private void OnNodeMouseClick(object? sender, PointerReleasedEventArgs e)
    {
        // If selected node is clicked, make sure to force re-selection. This way, if user
        // navigates to a node by keyboard, then clicks on the same node with the mouse, it
        // will perform the AfterSelect action (i.e. select revision in revision graph).
        if (!_pointerSelectionRaised
            && e.Source is Avalonia.Visual source
            && !source.GetSelfAndVisualAncestors().OfType<ToggleButton>().Any()
            && source.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault() is { } item
            && ReferenceEquals(_treeView.SelectedItem, item))
        {
            RaiseAfterSelect();
        }
    }

    private void RaiseAfterSelect()
    {
        if (_treeView.SelectedItem is { } item)
        {
            AfterSelect?.Invoke(_treeView, new SelectionChangedEventArgs(
                SelectingItemsControl.SelectionChangedEvent, Array.Empty<object>(), new object[] { item }));
        }
    }
}
