using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>
/// Applies the WinForms <c>ToolStripDropDownMenu</c> 96-DPI width calculation to Avalonia menu items.
/// </summary>
internal static class WinFormsToolStripMenuSizer
{
    private const double TextRendererOverhang = 7;
    private const double DefaultImageMarginWidth = 25;
    private const double TextPaddingLeft = 8;
    private const double TextPaddingRight = 9;
    private const double ArrowWidth = 10;
    private const double ArrowPaddingRight = 8;
    private const double DropDownLayoutBorder = 1;
    private const string FilterMenuClass = "gitextensions-filter-menu";
    private const string SystemRendererClass = "gitextensions-filter-menu-system-renderer";
    private const string CaptionOnlyClass = "gitextensions-menu-no-gesture";
    private static readonly ConditionalWeakTable<MenuItem, ShortcutDisplay> ShortcutDisplays = new();
    private static readonly ConditionalWeakTable<MenuFlyout, ToolbarDropDownScope> ToolbarDropDowns = new();

    /// <summary>
    ///  Applies the source top ToolStrip's directional item-edge dropdown anchor.
    /// </summary>
    /// <param name="flyout">The retained source dropdown.</param>
    /// <param name="owner">The source item supplying direction and bounds.</param>
    internal static void ApplyToolbarDropDownPlacement(MenuFlyout flyout, TemplatedControl owner)
    {
        // ToolStrip's BelowRight extends right from the item's left edge; BelowLeft
        // mirrors that anchor. Avalonia's popup positioner already mirrors logical
        // anchors for RTL, so choosing a second right anchor would reverse it twice.
        flyout.Placement = PlacementMode.BottomEdgeAlignedLeft;
        flyout.VerticalOffset = -1;
    }

    /// <summary>
    ///  Opts a retained toolbar menu into its source owner's font and native item metrics.
    /// </summary>
    /// <param name="flyout">The retained source dropdown.</param>
    /// <param name="owner">The source item supplying typography and direction.</param>
    internal static void ConfigureToolbarDropDown(MenuFlyout flyout, TemplatedControl owner)
    {
        if (ToolbarDropDowns.TryGetValue(flyout, out ToolbarDropDownScope? current))
        {
            if (ReferenceEquals(current.Owner, owner))
            {
                return;
            }

            ClearToolbarDropDown(flyout);
        }

        ToolbarDropDowns.Add(flyout, new ToolbarDropDownScope(flyout, owner));
    }

    /// <summary>
    ///  Releases only values and subscriptions owned by this toolbar-menu opt-in.
    /// </summary>
    /// <param name="flyout">The retained dropdown leaving its source owner.</param>
    internal static void ClearToolbarDropDown(MenuFlyout flyout)
    {
        if (ToolbarDropDowns.TryGetValue(flyout, out ToolbarDropDownScope? current))
        {
            ToolbarDropDowns.Remove(flyout);
            current.Dispose();
        }
    }

    public static void SetShortcutDisplayString(MenuItem item, string? displayString)
    {
        ShortcutDisplays.Remove(item);
        if (!string.IsNullOrEmpty(displayString))
        {
            ShortcutDisplays.Add(item, new ShortcutDisplay(displayString));
        }
    }

    internal static string? GetShortcutDisplayString(MenuItem item)
        => ShortcutDisplays.TryGetValue(item, out ShortcutDisplay? display) ? display.Value : null;

    public static void Apply(ItemsControl owner)
    {
        Apply(owner, [.. owner.Items.OfType<MenuItem>()], owner is ContextMenu or MenuItem);
    }

    public static void Apply(MenuFlyout flyout, TemplatedControl owner)
    {
        Apply(owner, [.. flyout.Items.OfType<MenuItem>()], false);
    }

    public static double Apply(MenuFlyout flyout, TemplatedControl owner, MenuItem excludedItem)
        => Apply(owner, [.. flyout.Items.OfType<MenuItem>().Where(item => item != excludedItem)], false);

    private static double Apply(TemplatedControl owner, MenuItem[] menuItems, bool hasLayoutBorder)
    {
        if (menuItems.Length == 0)
        {
            return 0;
        }

        // ToolStripDropDownMenu.ShowImageMargin defaults to true. Its layout always reserves
        // one measured tab before the shortcut column, even when an item's shortcut is empty.
        double tabWidth = MeasureText(owner, "\t");
        double maximumTextAndShortcutWidth = menuItems.Max(item =>
            MeasureText(item, GetDisplayText(item.Header))
            + tabWidth
            + GetShortcutWidth(item));
        double itemWidth = Math.Ceiling(DefaultImageMarginWidth
            + TextPaddingLeft
            + maximumTextAndShortcutWidth
            + TextPaddingRight
            + ArrowWidth
            + ArrowPaddingRight)
            - (hasLayoutBorder ? DropDownLayoutBorder : 0);

        foreach (MenuItem item in menuItems)
        {
            item.Width = itemWidth;
            if (ShortcutDisplays.TryGetValue(item, out ShortcutDisplay? display))
            {
                item.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .FirstOrDefault(textBlock => textBlock.Name == "PART_InputGestureText")
                    ?.SetCurrentValue(TextBlock.TextProperty, display.Value);
            }
        }

        return itemWidth;
    }

    private static double MeasureText(TemplatedControl owner, string text)
        => WinFormsTextMeasurer.Measure(owner, text) + TextRendererOverhang;

    private static double GetShortcutWidth(MenuItem item)
    {
        string? shortcut = ShortcutDisplays.TryGetValue(item, out ShortcutDisplay? display)
            ? display.Value
            : item.InputGesture?.ToString();
        return string.IsNullOrEmpty(shortcut) ? 0 : MeasureText(item, shortcut);
    }

    private static string GetDisplayText(object? header)
    {
        string source = header?.ToString() ?? string.Empty;
        if (!source.Contains('_', StringComparison.Ordinal))
        {
            return source;
        }

        Span<char> buffer = source.Length <= 256 ? stackalloc char[source.Length] : new char[source.Length];
        int output = 0;
        bool mnemonicRemoved = false;
        for (int index = 0; index < source.Length; index++)
        {
            if (source[index] != '_')
            {
                buffer[output++] = source[index];
                continue;
            }

            if (index + 1 < source.Length && source[index + 1] == '_')
            {
                buffer[output++] = '_';
                index++;
            }
            else if (mnemonicRemoved)
            {
                buffer[output++] = '_';
            }
            else
            {
                mnemonicRemoved = true;
            }
        }

        return new string(buffer[..output]);
    }

    private sealed class ShortcutDisplay(string value)
    {
        public string Value { get; } = value;
    }

    private sealed class ToolbarDropDownScope : IDisposable
    {
        private readonly MenuFlyout _flyout;
        private readonly TemplatedControl _owner;
        private readonly Dictionary<MenuItem, ItemScope> _items = [];
        private readonly bool _hadPresenterClass;
        private MenuFlyoutPresenter? _presenter;
        private MenuItem? _disabledSelection;
        private IDisposable? _showModeValue;
        private Key? _handledOpenKey;
        private (FontFamily Family, double Size, FontStyle Style, FontWeight Weight, FlowDirection Direction)? _font;
        private bool _updating;
        private bool _queued;
        private bool _disposed;
        private bool _trackingPointer;
        private bool _keyboardOpening;

        public ToolbarDropDownScope(MenuFlyout flyout, TemplatedControl owner)
        {
            _flyout = flyout;
            _owner = owner;
            _hadPresenterClass = flyout.FlyoutPresenterClasses.Contains(FilterMenuClass);
            flyout.FlyoutPresenterClasses.Add(FilterMenuClass);
            ApplyToolbarDropDownPlacement(flyout, owner);
            flyout.Opening += OnOpening;
            flyout.Opened += OnOpened;
            flyout.Closed += OnClosed;
            flyout.Items.CollectionChanged += OnItemsChanged;
            owner.PropertyChanged += OnOwnerChanged;
            owner.AddHandler(InputElement.KeyDownEvent, OnOwnerKeyDown, RoutingStrategies.Tunnel);
            owner.AddHandler(InputElement.KeyUpEvent, OnOwnerKeyUp, RoutingStrategies.Tunnel);
        }

        public TemplatedControl Owner => _owner;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _flyout.Opening -= OnOpening;
            _flyout.Opened -= OnOpened;
            _flyout.Closed -= OnClosed;
            DetachPresenter();
            _flyout.Items.CollectionChanged -= OnItemsChanged;
            _owner.PropertyChanged -= OnOwnerChanged;
            _owner.RemoveHandler(InputElement.KeyDownEvent, OnOwnerKeyDown);
            _owner.RemoveHandler(InputElement.KeyUpEvent, OnOwnerKeyUp);
            _handledOpenKey = null;
            _showModeValue?.Dispose();
            _showModeValue = null;
            ItemScope[] scopes = _items.Values.ToArray();
            _items.Clear();
            foreach (ItemScope scope in scopes)
            {
                scope.Dispose();
            }

            if (!_hadPresenterClass)
            {
                _flyout.FlyoutPresenterClasses.Remove(FilterMenuClass);
            }
        }

        private void OnOpening(object? sender, EventArgs args)
        {
            Apply();
            _showModeValue?.Dispose();
            _showModeValue = null;
            if (_disposed)
            {
                return;
            }

            // ShowDropDown/MouseDown do not select a row. ToolStripDropDownItem's
            // keyboard route selects the first enabled row after opening instead.
            IDisposable? value = _flyout.SetValue(PopupFlyoutBase.ShowModeProperty,
                _keyboardOpening ? FlyoutShowMode.Standard : FlyoutShowMode.Transient, BindingPriority.Template);
            if (_disposed)
            {
                value?.Dispose();
            }
            else
            {
                _showModeValue = value;
            }
        }

        private void OnOpened(object? sender, EventArgs args)
        {
            Apply();
            if (!_disposed && _flyout.Popup.Child is MenuFlyoutPresenter presenter && !ReferenceEquals(_presenter, presenter))
            {
                DetachPresenter();
                _presenter = presenter;
                presenter.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
                presenter.PointerExited += OnPointerExited;
            }
        }

        private void OnClosed(object? sender, EventArgs args)
        {
            if (_flyout.IsOpen)
            {
                return;
            }

            // Native dropdown closure clears its item selection. Hide() does not call
            // MenuFlyoutPresenter.Close(), which normally resets Avalonia's selection.
            if (_presenter is MenuFlyoutPresenter presenter && presenter.SelectedItem is MenuItem selected
                && _items.ContainsKey(selected))
            {
                presenter.SetCurrentValue(SelectingItemsControl.SelectedItemProperty, null);
                if (!_flyout.IsOpen && selected.IsSelected)
                {
                    selected.SetCurrentValue(MenuItem.IsSelectedProperty, false);
                }
            }

            if (!_flyout.IsOpen)
            {
                DetachPresenter();
            }
        }

        private bool OwnsFlyout()
            => !_disposed && _owner.IsEffectivelyEnabled && _owner switch
            {
                SplitButton split => ReferenceEquals(split.Flyout, _flyout),
                DropDownButton dropDown => ReferenceEquals(dropDown.Flyout, _flyout),
                _ => false,
            };

        private void OnOwnerKeyDown(object? sender, KeyEventArgs args)
        {
            if (!OwnsFlyout() || args.KeyModifiers != KeyModifiers.None
                || (args.Key is not (Key.Down or Key.Up)
                    && !(_owner is DropDownButton && (args.Key is Key.Enter or Key.Space))))
            {
                return;
            }

            args.Handled = true;
            _handledOpenKey = args.Key;
            bool wasOpen = _flyout.IsOpen;
            _keyboardOpening = true;
            try
            {
                _flyout.ShowAt(_owner);
                if (wasOpen && OwnsFlyout() && _flyout.IsOpen
                    && _flyout.Popup.Child is MenuFlyoutPresenter presenter && ReferenceEquals(_presenter, presenter))
                {
                    _flyout.Items.OfType<MenuItem>().FirstOrDefault(item => item.IsVisible && item.IsEffectivelyEnabled)
                        ?.Focus(NavigationMethod.Directional);
                }
            }
            finally
            {
                _keyboardOpening = false;
            }
        }

        private void OnOwnerKeyUp(object? sender, KeyEventArgs args)
        {
            if (!OwnsFlyout())
            {
                return;
            }

            if (_handledOpenKey == args.Key)
            {
                _handledOpenKey = null;
                args.Handled = true;
            }
            else if (_owner is SplitButton
                && ((args.Key == Key.F4 && args.KeyModifiers == KeyModifiers.None)
                    || (args.Key == Key.Down && args.KeyModifiers == KeyModifiers.Alt)))
            {
                // The original FormBrowse queued F4/WM_SYSKEY Alt+Down routes do
                // not open these menus; Fluent's split-button KeyUp openers must not add one.
                args.Handled = true;
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs args)
        {
            if (_disposed || _trackingPointer || !_flyout.IsOpen || _presenter is not MenuFlyoutPresenter presenter)
            {
                return;
            }

            _trackingPointer = true;
            try
            {
                MenuItem? disabled = _items.Keys.FirstOrDefault(item => !item.IsEffectivelyEnabled
                    && item.IsVisible && _flyout.Items.Contains(item)
                    && new Rect(item.Bounds.Size).Contains(args.GetPosition(item)));
                if (ReferenceEquals(disabled, _disabledSelection))
                {
                    return;
                }

                ClearDisabledSelection();
                if (!_disposed && _flyout.IsOpen && ReferenceEquals(_presenter, presenter)
                    && disabled is not null && _items.ContainsKey(disabled) && _flyout.Items.Contains(disabled))
                {
                    // ToolStripMenuItem supports disabled hot tracking. Avalonia excludes
                    // disabled rows from pointer hit testing, so select the real row through
                    // its menu's normal selection container without inventing pointer state.
                    _disabledSelection = disabled;
                    presenter.SetCurrentValue(SelectingItemsControl.SelectedItemProperty, disabled);
                    if ((_disposed || !_flyout.IsOpen || !ReferenceEquals(_presenter, presenter)
                        || !_items.ContainsKey(disabled) || !_flyout.Items.Contains(disabled))
                        && ReferenceEquals(presenter.SelectedItem, disabled))
                    {
                        // Selection notifications may retire this scope before the
                        // container finishes publishing its new selected item.
                        presenter.SetCurrentValue(SelectingItemsControl.SelectedItemProperty, null);
                    }
                }
            }
            finally
            {
                _trackingPointer = false;
            }
        }

        private void OnPointerExited(object? sender, PointerEventArgs args) => ClearDisabledSelection();

        private void ClearDisabledSelection()
        {
            MenuItem? disabled = _disabledSelection;
            _disabledSelection = null;
            if (disabled is not null && _presenter is MenuFlyoutPresenter presenter && ReferenceEquals(presenter.SelectedItem, disabled))
            {
                presenter.SetCurrentValue(SelectingItemsControl.SelectedItemProperty, null);
            }

            if (disabled is not null && (!_flyout.IsOpen || !_flyout.Items.Contains(disabled)) && disabled.IsSelected)
            {
                // Removing a self-container or hiding the popup detaches containers
                // before selection is cleared. Retire only the hot-tracked row we owned.
                disabled.SetCurrentValue(MenuItem.IsSelectedProperty, false);
            }
        }

        private void DetachPresenter()
        {
            ClearDisabledSelection();
            if (_presenter is MenuFlyoutPresenter presenter)
            {
                _presenter = null;
                presenter.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
                presenter.PointerExited -= OnPointerExited;
            }
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (_disabledSelection is not null && !_flyout.Items.Contains(_disabledSelection))
            {
                ClearDisabledSelection();
            }

            foreach (MenuItem removed in _items.Keys.Where(item => !_flyout.Items.Contains(item)).ToArray())
            {
                if (_items.Remove(removed, out ItemScope? scope))
                {
                    scope.Dispose();
                }
            }

            QueueApply();
        }

        private void OnItemChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == TemplatedControl.FontFamilyProperty
                || args.Property == TemplatedControl.FontSizeProperty
                || args.Property == TemplatedControl.FontStyleProperty
                || args.Property == TemplatedControl.FontWeightProperty
                || args.Property == MenuItem.HeaderProperty
                || args.Property == MenuItem.InputGestureProperty
                || args.Property == Control.IsVisibleProperty
                || args.Property == Control.MarginProperty
                || args.Property == NativeToolStripDropDownMenuItem.UseSystemVisualStyleProperty)
            {
                QueueApply();
            }
        }

        private void OnOwnerChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == TemplatedControl.FontFamilyProperty
                || args.Property == TemplatedControl.FontSizeProperty
                || args.Property == TemplatedControl.FontStyleProperty
                || args.Property == TemplatedControl.FontWeightProperty
                || args.Property == Control.FlowDirectionProperty)
            {
                QueueApply();
            }
        }

        private void QueueApply()
        {
            if (_disposed || _queued || !_flyout.IsOpen)
            {
                return;
            }

            _queued = true;

            // Inherited fonts can change during style-frame enumeration. Replace
            // disposable values only once the framework has finished that evaluation.
            Dispatcher.UIThread.Post(() =>
            {
                _queued = false;
                if (!_disposed && _flyout.IsOpen)
                {
                    Apply();
                }
            }, DispatcherPriority.Loaded);
        }

        private void Apply()
        {
            if (_disposed || _updating)
            {
                return;
            }

            _updating = true;
            try
            {
                ApplyToolbarDropDownPlacement(_flyout, _owner);
                (FontFamily Family, double Size, FontStyle Style, FontWeight Weight, FlowDirection Direction) font =
                    (_owner.FontFamily, _owner.FontSize, _owner.FontStyle, _owner.FontWeight, _owner.FlowDirection);
                bool fontChanged = _font != font;
                foreach (MenuItem item in _flyout.Items.OfType<MenuItem>().ToArray())
                {
                    if (_disposed)
                    {
                        return;
                    }

                    if (!_flyout.Items.Contains(item))
                    {
                        continue;
                    }

                    if (!_items.TryGetValue(item, out ItemScope? scope))
                    {
                        scope = new(item, OnItemChanged);
                        if (_disposed || !_flyout.Items.Contains(item))
                        {
                            scope.Dispose();
                            continue;
                        }

                        _items.Add(item, scope);
                        scope.ApplyFont(item, font);
                    }
                    else if (fontChanged)
                    {
                        scope.ApplyFont(item, font);
                    }

                    scope.UpdateCaptionClass();
                    scope.UpdateRendererClass();
                }

                if (_disposed)
                {
                    return;
                }

                _font = font;
                NativeToolStripDropDownLayout.Group metrics = new(_owner, _flyout.Items.OfType<Control>().ToArray(), null, null);
                foreach ((MenuItem item, ItemScope scope) in _items.ToArray())
                {
                    if (!_disposed && _flyout.Items.Contains(item))
                    {
                        scope.ApplySize(item, metrics);
                        scope.ApplyShortcut();
                    }
                }
            }
            finally
            {
                _updating = false;
            }
        }

        private sealed class ItemScope : IDisposable
        {
            private readonly List<IDisposable?> _fontValues = [];
            private readonly List<IDisposable?> _sizeValues = [];
            private readonly MenuItem _item;
            private readonly EventHandler<AvaloniaPropertyChangedEventArgs> _onChanged;
            private readonly bool _hadFilterClass;
            private readonly bool _hadCaptionClass;
            private readonly bool _hadSystemRendererClass;
            private IDisposable? _groupValue;
            private IDisposable? _shortcutValue;
            private TextBlock? _shortcutText;
            private string? _shortcutDisplay;
            private (int Width, int Height)? _size;
            private bool _disposed;

            public ItemScope(MenuItem item, EventHandler<AvaloniaPropertyChangedEventArgs> onChanged)
            {
                _item = item;
                _onChanged = onChanged;
                _hadFilterClass = item.Classes.Contains(FilterMenuClass);
                _hadCaptionClass = item.Classes.Contains(CaptionOnlyClass);
                _hadSystemRendererClass = item.Classes.Contains(SystemRendererClass);
                item.Classes.Add(FilterMenuClass);
                item.PropertyChanged += OnPropertyChanged;
                item.TemplateApplied += OnTemplateApplied;
            }

            public void UpdateCaptionClass()
            {
                if (!_disposed && !_hadCaptionClass)
                {
                    _item.Classes.Set(CaptionOnlyClass, _item.InputGesture is null && GetShortcutDisplayString(_item) is null);
                }
            }

            public void UpdateRendererClass()
            {
                if (!_disposed && !_hadSystemRendererClass)
                {
                    _item.Classes.Set(SystemRendererClass, NativeToolStripDropDownMenuItem.GetUseSystemVisualStyle(_item));
                }
            }

            public void ApplyFont(MenuItem item, (FontFamily Family, double Size, FontStyle Style, FontWeight Weight, FlowDirection Direction) font)
            {
                Release(_fontValues);
                SetValue(item, TemplatedControl.FontFamilyProperty, font.Family, _fontValues);
                SetValue(item, TemplatedControl.FontSizeProperty, font.Size, _fontValues);
                SetValue(item, TemplatedControl.FontStyleProperty, font.Style, _fontValues);
                SetValue(item, TemplatedControl.FontWeightProperty, font.Weight, _fontValues);
                SetValue(item, Control.FlowDirectionProperty, font.Direction, _fontValues);
            }

            public void ApplyShortcut()
            {
                if (_disposed)
                {
                    return;
                }

                TextBlock? text = _item.GetVisualDescendants().OfType<TextBlock>()
                    .FirstOrDefault(part => part.Name == "PART_InputGestureText" && ReferenceEquals(part.TemplatedParent, _item));
                string? display = GetShortcutDisplayString(_item);
                if (ReferenceEquals(text, _shortcutText) && display == _shortcutDisplay)
                {
                    return;
                }

                ClearShortcut();
                if (_disposed || text is null || display is null)
                {
                    return;
                }

                // Fluent formats arrow names differently from the source hotkey formatter.
                // Keep the measured source caption on its actual text part and release only
                // this override when the menu leaves the source-shaped adapter.
                IDisposable? value = text.SetValue(TextBlock.TextProperty, display, BindingPriority.Template);
                if (_disposed || !ReferenceEquals(text.TemplatedParent, _item))
                {
                    value?.Dispose();
                }
                else
                {
                    _shortcutValue = value;
                    _shortcutText = text;
                    _shortcutDisplay = display;
                }
            }

            public void ApplySize(MenuItem item, NativeToolStripDropDownLayout.Group metrics)
            {
                IDisposable? previousGroupValue = _groupValue;
                _groupValue = null;
                previousGroupValue?.Dispose();
                if (_disposed)
                {
                    return;
                }

                IDisposable? groupValue = item.SetValue(NativeFilterToolStripDropDownPanel.GroupProperty, metrics, BindingPriority.Template);
                if (_disposed)
                {
                    groupValue?.Dispose();
                    return;
                }

                _groupValue = groupValue;
                item.GetVisualAncestors().OfType<NativeFilterToolStripDropDownPanel>().FirstOrDefault()?.InvalidateMeasure();
                int width = metrics.ItemWidth;
                int height = metrics.ItemHeight;
                if (!_disposed && _size != (width, height))
                {
                    Release(_sizeValues);
                    SetValue(item, MenuItem.WidthProperty, (double)width, _sizeValues);
                    SetValue(item, MenuItem.HeightProperty, (double)height, _sizeValues);
                    _size = (width, height);
                }
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _item.PropertyChanged -= OnPropertyChanged;
                _item.TemplateApplied -= OnTemplateApplied;
                ClearShortcut();
                IDisposable? groupValue = _groupValue;
                _groupValue = null;
                groupValue?.Dispose();
                Release(_fontValues);
                Release(_sizeValues);
                if (!_hadFilterClass)
                {
                    _item.Classes.Remove(FilterMenuClass);
                }

                if (!_hadCaptionClass)
                {
                    _item.Classes.Remove(CaptionOnlyClass);
                }

                if (!_hadSystemRendererClass)
                {
                    _item.Classes.Remove(SystemRendererClass);
                }
            }

            private void ClearShortcut()
            {
                IDisposable? value = _shortcutValue;
                _shortcutValue = null;
                _shortcutText = null;
                _shortcutDisplay = null;
                value?.Dispose();
            }

            private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
            {
                if (args.Property == TemplatedControl.TemplateProperty)
                {
                    ClearShortcut();
                }

                _onChanged(sender, args);
            }

            private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs args)
            {
                // Complete template construction before publishing a value whose callback
                // may replace that template again or retire this scope synchronously.
                Dispatcher.UIThread.Post(ApplyShortcut, DispatcherPriority.Loaded);
            }

            private void SetValue<T>(MenuItem item, StyledProperty<T> property, T value, List<IDisposable?> values)
            {
                if (_disposed)
                {
                    return;
                }

                IDisposable? owned = item.SetValue(property, value, BindingPriority.Template);
                if (_disposed)
                {
                    // A synchronous callback may remove/rebind the row before SetValue
                    // returns. Do not retain a value after its owning scope has retired.
                    owned?.Dispose();
                }
                else
                {
                    values.Add(owned);
                }
            }

            private static void Release(List<IDisposable?> values)
            {
                IDisposable?[] released = values.ToArray();
                values.Clear();
                foreach (IDisposable? value in released)
                {
                    value?.Dispose();
                }
            }
        }
    }
}
