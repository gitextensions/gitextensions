using System.Collections.Specialized;
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
    private readonly Func<bool> _isUpdating;
    private readonly Dictionary<ItemCollection, ItemsControl> _collectionOwners = [];
    private readonly Dictionary<TreeViewItem, ItemsControl> _itemOwners = [];
    private readonly List<TreeViewItem> _caretPath = [];
    private bool _caretHadFocus;
    private string _incrementalSearch = string.Empty;
    private long _lastTextInputTimestamp;

    internal NativeTreeKeyboardAdapter(TreeView tree, Func<long>? getTextInputTimestamp = null, Func<bool>? isUpdating = null)
    {
        _tree = tree;
        _getTextInputTimestamp = getTextInputTimestamp ?? (() => Environment.TickCount64);
        _isUpdating = isUpdating ?? (() => false);

        // Avalonia tunnels through instance handlers in reverse registration order.
        // Install before the explorer decorator so its preview arrow guard runs first.
        tree.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        tree.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        tree.PropertyChanged += OnTreePropertyChanged;
        tree.AddHandler(TreeViewItem.CollapsedEvent, OnItemCollapsed, RoutingStrategies.Tunnel);
        RegisterItems(tree);
        if (tree.SelectedItem is TreeViewItem selected)
        {
            RememberCaret(selected);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || e.KeyModifiers.HasFlag(KeyModifiers.Alt) || e.KeyModifiers.HasFlag(KeyModifiers.Meta))
        {
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (e.Key is Key.Home or Key.End or Key.PageUp or Key.PageDown or Key.Up or Key.Down or Key.Left or Key.Right)
            {
                ScrollWithoutMovingCaret(e.Key);
                e.Handled = true;
            }

            return;
        }

        if (e.Key is not (Key.Home or Key.End or Key.PageUp or Key.PageDown or Key.Up or Key.Down or Key.Left or Key.Right or Key.Back))
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
            case Key.Back:
                target = index < 0 ? null : nodes[index].Parent;
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

        if (e.Property == TreeView.SelectedItemProperty)
        {
            if (_tree.SelectedItem is TreeViewItem selected)
            {
                RememberCaret(selected);
            }
            else if (_caretPath.Count == 0 || IsPresent(_caretPath[0]))
            {
                // Explicitly clearing a live caret is not a node-removal fallback.
                // Avalonia also clears root selection before its Items notification;
                // retain the removed path until that notification can choose a target.
                _caretPath.Clear();
                _caretHadFocus = false;
            }
        }
        else if (e.Property == InputElement.IsKeyboardFocusWithinProperty
            && _caretPath.Count > 0 && IsPresent(_caretPath[0]))
        {
            _caretHadFocus = _tree.IsKeyboardFocusWithin;
        }
    }

    private void RegisterItems(ItemsControl owner)
    {
        if (_collectionOwners.TryAdd(owner.Items, owner))
        {
            owner.Items.CollectionChanged += OnItemsCollectionChanged;
        }

        foreach (TreeViewItem item in owner.Items.OfType<TreeViewItem>())
        {
            _itemOwners[item] = owner;
            RegisterItems(item);
        }
    }

    private void UnregisterItem(TreeViewItem item)
    {
        foreach (TreeViewItem child in item.Items.OfType<TreeViewItem>())
        {
            UnregisterItem(child);
        }

        item.Items.CollectionChanged -= OnItemsCollectionChanged;
        _collectionOwners.Remove(item.Items);
        _itemOwners.Remove(item);
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ItemCollection collection || !_collectionOwners.TryGetValue(collection, out ItemsControl? owner))
        {
            return;
        }

        bool removedCaret = _caretPath.Count > 0 && !IsPresent(_caretPath[0]);
        bool hadFocus = _caretHadFocus;
        TreeViewItem? target = removedCaret
            ? owner.Items.Skip(Math.Max(0, e.OldStartingIndex)).OfType<TreeViewItem>().FirstOrDefault()
                ?? owner.Items.OfType<TreeViewItem>().LastOrDefault()
                ?? _caretPath.Skip(1).FirstOrDefault(IsPresent)
            : null;
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (TreeViewItem oldItem in _itemOwners.Where(pair => ReferenceEquals(pair.Value, owner)).Select(pair => pair.Key).ToArray())
            {
                UnregisterItem(oldItem);
            }
        }
        else if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace)
        {
            foreach (TreeViewItem oldItem in e.OldItems!.OfType<TreeViewItem>())
            {
                UnregisterItem(oldItem);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            RegisterItems(owner);
        }
        else if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace)
        {
            foreach (TreeViewItem newItem in e.NewItems!.OfType<TreeViewItem>())
            {
                _itemOwners[newItem] = owner;
                RegisterItems(newItem);
            }
        }

        // Ref refresh reconstructs containers under the owner's source-shaped
        // selection suppression and restores its saved identity afterwards.
        // Track the new collections above, but do not select a transient fallback.
        if (removedCaret && _isUpdating())
        {
            // An identity absent from the rebuilt tree must not let a later,
            // unrelated collection mutation resurrect the removed caret.
            // The owner's saved selection re-seeds this path when it survives.
            _caretPath.Clear();
            _caretHadFocus = false;
        }
        else if (removedCaret)
        {
            // Native deletion prefers the next sibling, then the previous sibling,
            // then its surviving parent. This caret is not a logical filter flag.
            if (target is not null)
            {
                SelectCaret(target, hadFocus);
            }
            else
            {
                _tree.SelectedItem = null;
                _caretPath.Clear();
                _caretHadFocus = false;
            }
        }
    }

    private void OnItemCollapsed(object? sender, RoutedEventArgs e)
    {
        if (!_isUpdating() && e.Source is TreeViewItem item && _caretPath.Skip(1).Contains(item))
        {
            // The native caret cannot remain in a collapsed descendant. Do not
            // steal keyboard focus when the tree's container was already unfocused.
            SelectCaret(item, _tree.IsKeyboardFocusWithin);
        }
    }

    private void RememberCaret(TreeViewItem selected)
    {
        _caretPath.Clear();
        for (TreeViewItem? item = selected; item is not null;
             item = _itemOwners.TryGetValue(item, out ItemsControl? owner) ? owner as TreeViewItem : null)
        {
            _caretPath.Add(item);
        }

        _caretHadFocus = _tree.IsKeyboardFocusWithin;
    }

    private bool IsPresent(TreeViewItem item)
    {
        ItemsControl current = item;
        while (current is TreeViewItem node)
        {
            if (!_itemOwners.TryGetValue(node, out ItemsControl? owner) || !owner.Items.Contains(node))
            {
                return false;
            }

            current = owner;
        }

        return ReferenceEquals(current, _tree);
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

    private void ScrollWithoutMovingCaret(Key key)
    {
        if (_tree.SelectedItem is not TreeViewItem selected
            || _tree.GetVisualDescendants().OfType<ScrollViewer>()
                .FirstOrDefault(scroll => scroll.FindAncestorOfType<TreeView>() == _tree) is not { } scroll)
        {
            return;
        }

        // Native Ctrl navigation scrolls without selecting, focusing or revealing
        // the unchanged caret. Its horizontal SB_LINE is five pixels at96DPI;
        // vertical pages share one visible row. Use the arranged header, not subtree.
        double rowHeight = GetRowHeight(selected);
        double maximumX = Math.Max(0, scroll.Extent.Width - scroll.Viewport.Width);
        double maximumY = Math.Max(0, scroll.Extent.Height - scroll.Viewport.Height);
        if (key is Key.Left or Key.Right)
        {
            int direction = key == Key.Left ? -1 : 1;
            double next = Math.Clamp(scroll.Offset.X + (direction * NativeTreeScrollAdapter.NativeHorizontalLine), 0, maximumX);
            scroll.Offset = new Vector(next, scroll.Offset.Y);
        }
        else if (rowHeight > 0)
        {
            double next = key switch
            {
                Key.Home => 0,
                Key.End => maximumY,
                Key.PageUp => scroll.Offset.Y - (GetPageStep(selected) * rowHeight),
                Key.PageDown => scroll.Offset.Y + (GetPageStep(selected) * rowHeight),
                Key.Up => scroll.Offset.Y - rowHeight,
                _ => scroll.Offset.Y + rowHeight,
            };
            next = Math.Clamp(next, 0, maximumY);
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

    private void SelectCaret(TreeViewItem target, bool focus = true)
    {
        // Directional focus lets Avalonia paint its keyboard cue. Set the
        // single native caret explicitly, never the consumer's logical flags.
        ScrollViewer? scroll = _tree.GetVisualDescendants().OfType<ScrollViewer>()
            .FirstOrDefault(viewer => viewer.FindAncestorOfType<TreeView>() == _tree);
        double? horizontal = scroll?.Offset.X;
        if (focus)
        {
            target.Focus(NavigationMethod.Directional);
        }

        _tree.SelectedItem = target;
        target.BringIntoView();
        if (scroll is not null && horizontal.HasValue)
        {
            // Native caret navigation reveals its row vertically without resetting
            // horizontal scroll, including Backspace and incremental caption search.
            scroll.Offset = new Vector(Math.Clamp(horizontal.Value, 0,
                Math.Max(0, scroll.Extent.Width - scroll.Viewport.Width)), scroll.Offset.Y);
        }
    }

    private readonly record struct VisibleNode(TreeViewItem Item, TreeViewItem? Parent);
}
