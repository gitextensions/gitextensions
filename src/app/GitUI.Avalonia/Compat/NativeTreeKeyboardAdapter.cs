using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>
///  Preserves the native TREEVIEW caret navigation omitted by Avalonia's tree control.
/// </summary>
internal sealed class NativeTreeKeyboardAdapter
{
    private const int IncrementalSearchIdleMilliseconds = 1000;
    private readonly TreeView _tree;
    private readonly Func<long> _getTextInputTimestamp;
    private string _incrementalSearch = string.Empty;
    private long _lastTextInputTimestamp;

    internal NativeTreeKeyboardAdapter(TreeView tree, Func<long>? getTextInputTimestamp = null)
    {
        _tree = tree;
        _getTextInputTimestamp = getTextInputTimestamp ?? (() => Environment.TickCount64);

        // Avalonia tunnels through instance handlers in reverse registration order.
        // Install before the explorer decorator so its preview arrow guard runs first.
        tree.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        tree.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        tree.PropertyChanged += OnTreePropertyChanged;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || e.KeyModifiers.HasFlag(KeyModifiers.Alt) || e.KeyModifiers.HasFlag(KeyModifiers.Meta))
        {
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (e.Key is Key.Up or Key.Down)
            {
                ScrollOneRow(e.Key == Key.Up ? -1 : 1);
                e.Handled = true;
            }

            return;
        }

        if (e.Key is not (Key.Home or Key.End or Key.PageUp or Key.PageDown or Key.Up or Key.Down or Key.Left or Key.Right))
        {
            return;
        }

        _incrementalSearch = string.Empty;
        List<VisibleNode> nodes = [];
        AddVisibleNodes(_tree, null, nodes);
        if (nodes.Count == 0)
        {
            e.Handled = true;
            return;
        }

        int index = nodes.FindIndex(node => ReferenceEquals(node.Item, _tree.SelectedItem));
        TreeViewItem? target;
        switch (e.Key)
        {
            case Key.Home:
                target = nodes[0].Item;
                break;
            case Key.End:
                target = nodes[^1].Item;
                break;
            case Key.Up:
                target = nodes[Math.Max(0, index - 1)].Item;
                break;
            case Key.Down:
                target = nodes[Math.Min(nodes.Count - 1, index + 1)].Item;
                break;
            case Key.PageUp:
            case Key.PageDown:
                int direction = e.Key == Key.PageUp ? -1 : 1;
                int pageStep = GetPageStep(index < 0 ? nodes[0].Item : nodes[index].Item);
                target = nodes[Math.Clamp(Math.Max(0, index) + (direction * pageStep), 0, nodes.Count - 1)].Item;
                break;
            case Key.Left:
                target = index < 0 ? nodes[0].Item : GetLeftTarget(nodes[index]);
                break;
            default:
                target = index < 0 ? nodes[0].Item : GetRightTarget(nodes[index].Item);
                break;
        }

        if (target is not null)
        {
            SelectCaret(target);
        }

        e.Handled = true;
    }

    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Handled || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        List<VisibleNode> nodes = [];
        AddVisibleNodes(_tree, null, nodes);
        foreach (char character in e.Text.Where(character => !char.IsControl(character)))
        {
            // Real queued native input preserves the prefix just below one
            // second and resets it just above. Avalonia exposes TextInput, not
            // Win32 queue timestamps; use its monotonic event-arrival boundary.
            // Exact native threshold/preference/IME behavior remains unverified.
            long timestamp = _getTextInputTimestamp();
            long elapsed = timestamp - _lastTextInputTimestamp;
            if (elapsed < 0 || elapsed >= IncrementalSearchIdleMilliseconds)
            {
                _incrementalSearch = string.Empty;
            }

            _lastTextInputTimestamp = timestamp;
            _incrementalSearch += character;
            bool repeated = _incrementalSearch.All(value => value == _incrementalSearch[0]);
            string prefix = repeated ? _incrementalSearch[..1] : _incrementalSearch;
            int current = nodes.FindIndex(node => ReferenceEquals(node.Item, _tree.SelectedItem));
            int start = repeated ? current + 1 : Math.Max(0, current);
            for (int offset = 0; offset < nodes.Count; offset++)
            {
                TreeViewItem item = nodes[(start + offset) % nodes.Count].Item;
                if (GetCaption(item).StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    SelectCaret(item);
                    break;
                }
            }
        }

        e.Handled = true;
    }

    private void OnTreePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == InputElement.IsKeyboardFocusWithinProperty && !_tree.IsKeyboardFocusWithin)
        {
            _incrementalSearch = string.Empty;
        }
    }

    private static string GetCaption(TreeViewItem item)
        => item.Header switch
        {
            string caption => caption,
            TextBlock text => text.Text ?? string.Empty,
            Panel panel => panel.Children.OfType<TextBlock>().FirstOrDefault()?.Text ?? string.Empty,
            _ => string.Empty,
        };

    private static void AddVisibleNodes(ItemsControl parent, TreeViewItem? parentItem, List<VisibleNode> nodes)
    {
        foreach (TreeViewItem item in parent.Items.OfType<TreeViewItem>().Where(item => item.IsVisible))
        {
            nodes.Add(new VisibleNode(item, parentItem));
            if (item.IsExpanded)
            {
                AddVisibleNodes(item, item, nodes);
            }
        }
    }

    private static TreeViewItem? GetLeftTarget(VisibleNode node)
    {
        if (node.Item.IsExpanded && node.Item.Items.Count > 0)
        {
            node.Item.IsExpanded = false;
            return null;
        }

        return node.Parent;
    }

    private static TreeViewItem? GetRightTarget(TreeViewItem item)
    {
        TreeViewItem? child = item.Items.OfType<TreeViewItem>().FirstOrDefault(child => child.IsVisible);
        if (child is null)
        {
            return null;
        }

        if (!item.IsExpanded)
        {
            item.IsExpanded = true;
            return null;
        }

        return child;
    }

    private void ScrollOneRow(int direction)
    {
        if (_tree.SelectedItem is not TreeViewItem selected
            || _tree.GetVisualDescendants().OfType<ScrollViewer>()
                .FirstOrDefault(scroll => scroll.FindAncestorOfType<TreeView>() == _tree) is not { } scroll)
        {
            return;
        }

        // Native Ctrl+Up/Down changes the vertical scroll position by one item,
        // without selecting, focusing, or bringing the unchanged caret into view.
        // Use the arranged header rather than the expanded item's subtree height.
        double rowHeight = GetRowHeight(selected);
        if (rowHeight > 0)
        {
            double next = Math.Clamp(scroll.Offset.Y + (direction * rowHeight), 0,
                Math.Max(0, scroll.Extent.Height - scroll.Viewport.Height));
            scroll.Offset = new Vector(scroll.Offset.X, next);
        }
    }

    private int GetPageStep(TreeViewItem selected)
    {
        double rowHeight = GetRowHeight(selected);
        ScrollViewer? scroll = _tree.GetVisualDescendants().OfType<ScrollViewer>()
            .FirstOrDefault(scroll => scroll.FindAncestorOfType<TreeView>() == _tree);

        // Native TVM_GETVISIBLECOUNT excludes a partial final item. Page keys
        // advance that count minus one, keeping a row shared between pages.
        return scroll is not null && rowHeight > 0
            ? Math.Max(1, (int)Math.Floor(scroll.Viewport.Height / rowHeight) - 1)
            : 1;
    }

    private static double GetRowHeight(TreeViewItem selected)
        => selected.GetVisualDescendants().OfType<Control>()
            .FirstOrDefault(control => control.Name == "PART_LayoutRoot"
                && control.FindAncestorOfType<TreeViewItem>() == selected)?.Bounds.Height ?? 0;

    private void SelectCaret(TreeViewItem target)
    {
        // Directional focus lets Avalonia paint its keyboard cue. Set the
        // single native caret explicitly, never the consumer's logical flags.
        target.Focus(NavigationMethod.Directional);
        _tree.SelectedItem = target;
        target.BringIntoView();
    }

    private readonly record struct VisibleNode(TreeViewItem Item, TreeViewItem? Parent);
}
