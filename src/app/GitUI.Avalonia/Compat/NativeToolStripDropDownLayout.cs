using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Applies the source dropdown's image, text, arrow and hosted-control layout to an opted-in menu.
/// </summary>
internal sealed class NativeToolStripDropDownLayout : IDisposable
{
    private static readonly AttachedProperty<Group?> GroupProperty =
        AvaloniaProperty.RegisterAttached<NativeToolStripDropDownLayout, Control, Group?>("Group", inherits: true);

    private readonly TemplatedControl _owner;
    private readonly MenuFlyout _menu;
    private readonly MenuItem _filterHost;
    private readonly TextBox _filter;
    private readonly Dictionary<MenuItem, ItemScope> _scopes = [];
    private readonly List<IDisposable?> _defaultFontValues;
    private readonly List<Group> _groups = [];
    private bool _updating;
    private bool _queued;
    private bool _disposed;

    public NativeToolStripDropDownLayout(TemplatedControl owner, MenuFlyout menu, MenuItem filterHost, TextBox filter)
    {
        _owner = owner;
        _menu = menu;
        _filterHost = filterHost;
        _filter = filter;

        // ToolStrip has its own default menu font, not the enclosing Form's ambient font.
        // A real native-strip owner or an authored local/conditional font still takes precedence.
        FontFamily defaultFamily = OperatingSystem.IsWindows() ? new FontFamily("Segoe UI") : FontManager.Current.DefaultFontFamily;
        _defaultFontValues = SetFont(owner, defaultFamily, 12, FontStyle.Normal, FontWeight.Normal);
        _defaultFontValues.AddRange(SetFont(filter, defaultFamily, 12, FontStyle.Normal, FontWeight.Normal));
        owner.PropertyChanged += OnPropertyChanged;
        filter.PropertyChanged += OnPropertyChanged;
    }

    internal Group Root => _groups[0];

    internal static Group? GetGroup(Control control) => control.GetValue(GroupProperty);

    public void Apply(bool setFilterWidth)
    {
        if (_disposed || _updating)
        {
            return;
        }

        _updating = true;
        try
        {
            HashSet<MenuItem> current = [.. EnumerateItems(_menu.Items.OfType<MenuItem>())];
            foreach (MenuItem item in _scopes.Keys.Where(item => !current.Contains(item)).ToArray())
            {
                Release(item);
            }

            _groups.Clear();
            AddGroup(_owner, _menu.Items.OfType<Control>().ToArray(), root: true);
            if (setFilterWidth)
            {
                // WorkingDirectoryToolStripSplitButton.FillDropDown sets the actual hosted
                // TextBox width to the widest source row minus DpiUtil.Scale(60).
                _filter.Width = Math.Max(0, Root.ItemWidth - 60);
            }

            double filterHeight = WinFormsGraphicsTextMeasurer.GetFontHeight(_filter) + 4 + 3;
            _filter.Height = filterHeight;
            _filterHost.Height = filterHeight + _filter.Margin.Top + _filter.Margin.Bottom;
            _filterHost.Width = _filter.Width + _filter.Margin.Left + _filter.Margin.Right;
            foreach (Group group in _groups)
            {
                foreach (Control control in group.Items)
                {
                    control.SetValue(GroupProperty, group);
                    if (control is MenuItem item && item != _filterHost)
                    {
                        ItemScope scope = _scopes[item];
                        DisposeValues(scope.SizeValues);
                        scope.SizeValues =
                        [
                            item.SetValue(MenuItem.WidthProperty, (double)group.ItemWidth, BindingPriority.Template),
                            item.SetValue(MenuItem.HeightProperty, (double)group.ItemHeight, BindingPriority.Template),
                        ];
                    }

                    control.InvalidateMeasure();
                    control.GetVisualAncestors().OfType<NativeToolStripDropDownPanel>().FirstOrDefault()?.InvalidateMeasure();
                }
            }
        }
        finally
        {
            _updating = false;
        }
    }

    public void Clear()
    {
        _updating = true;
        try
        {
            foreach (MenuItem item in _scopes.Keys.ToArray())
            {
                Release(item);
            }

            foreach (Group group in _groups)
            {
                foreach (Control item in group.Items)
                {
                    item.ClearValue(GroupProperty);
                }
            }

            _groups.Clear();
        }
        finally
        {
            _updating = false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _owner.PropertyChanged -= OnPropertyChanged;
        _filter.PropertyChanged -= OnPropertyChanged;
        Clear();
        DisposeValues(_defaultFontValues);
    }

    internal static Size MeasureSourceText(TemplatedControl owner, string text)
    {
        if (OperatingSystem.IsWindows())
        {
            return WinFormsTextMeasurer.MeasureTextRenderer(owner, text);
        }

        // The native padded route parses '&' prefixes. The portable measurer's
        // FormattedText fallback is literal, so normalize only its measurement input.
        string displayed = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(AvaloniaTranslationUtils.ToAvaloniaMnemonics(text));
        if (text.Length > 0 && displayed.Length == 0)
        {
            Size height = WinFormsTextMeasurer.MeasureSize(owner, "0");
            double overhang = height.Height / 6;
            return new Size(Math.Ceiling(overhang) + Math.Ceiling(overhang * 1.5), height.Height);
        }

        return WinFormsTextMeasurer.MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight,
            owner.FontSize, displayed, singleLine: true, useTextRendererPadding: true);
    }

    private void AddGroup(TemplatedControl owner, Control[] items, bool root)
    {
        foreach (MenuItem item in items.OfType<MenuItem>())
        {
            if (!_scopes.TryGetValue(item, out ItemScope? scope))
            {
                scope = new ItemScope();
                _scopes.Add(item, scope);
                item.PropertyChanged += OnPropertyChanged;
            }

            DisposeValues(scope.FontValues);
            scope.FontValues = SetFont(item, owner.FontFamily, owner.FontSize, owner.FontStyle, owner.FontWeight);
            scope.FontValues.Add(item.SetValue(Control.FlowDirectionProperty, owner.FlowDirection, BindingPriority.Template));
        }

        Group group = new(owner, items, root ? _filterHost : null, root ? _filter : null);
        _groups.Add(group);
        foreach (MenuItem item in items.OfType<MenuItem>().Where(item => item.Items.Count > 0))
        {
            AddGroup(item, item.Items.OfType<Control>().ToArray(), root: false);
        }
    }

    private void Release(MenuItem item)
    {
        item.PropertyChanged -= OnPropertyChanged;
        ItemScope scope = _scopes[item];
        DisposeValues(scope.FontValues);
        DisposeValues(scope.SizeValues);
        item.ClearValue(GroupProperty);
        _scopes.Remove(item);
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (_updating || _disposed || _queued)
        {
            return;
        }

        if (args.Property != TemplatedControl.FontFamilyProperty
            && args.Property != TemplatedControl.FontSizeProperty
            && args.Property != TemplatedControl.FontStyleProperty
            && args.Property != TemplatedControl.FontWeightProperty
            && args.Property != Control.FlowDirectionProperty
            && args.Property != MenuItem.HeaderProperty
            && args.Property != MenuItem.InputGestureProperty
            && args.Property != Visual.IsVisibleProperty
            && args.Property != Layoutable.MarginProperty)
        {
            return;
        }

        _queued = true;

        // Font changes can arrive inside Avalonia's style-frame enumeration. Replacing
        // owned value scopes there is unsafe; process them after that enumeration returns.
        Dispatcher.UIThread.Post(() =>
        {
            _queued = false;
            if (!_disposed && _menu.Items.Count > 1)
            {
                Apply(setFilterWidth: false);
            }
        }, DispatcherPriority.Loaded);
    }

    private static List<IDisposable?> SetFont(TemplatedControl item, FontFamily family, double size, FontStyle style, FontWeight weight)
        =>
        [
            item.SetValue(TemplatedControl.FontFamilyProperty, family, BindingPriority.Template),
            item.SetValue(TemplatedControl.FontSizeProperty, size, BindingPriority.Template),
            item.SetValue(TemplatedControl.FontStyleProperty, style, BindingPriority.Template),
            item.SetValue(TemplatedControl.FontWeightProperty, weight, BindingPriority.Template),
        ];

    private static void DisposeValues(IEnumerable<IDisposable?> values)
    {
        foreach (IDisposable? value in values)
        {
            value?.Dispose();
        }
    }

    private static IEnumerable<MenuItem> EnumerateItems(IEnumerable<MenuItem> items)
    {
        foreach (MenuItem item in items)
        {
            yield return item;
            foreach (MenuItem child in EnumerateItems(item.Items.OfType<MenuItem>()))
            {
                yield return child;
            }
        }
    }

    private sealed class ItemScope
    {
        public List<IDisposable?> FontValues { get; set; } = [];

        public List<IDisposable?> SizeValues { get; set; } = [];
    }

    internal sealed class Group
    {
        private readonly Dictionary<MenuItem, Thickness> _itemTextPadding = [];
        private readonly Dictionary<MenuItem, int> _shortcutHeights = [];

        public Group(TemplatedControl owner, Control[] items, MenuItem? filterHost, TextBox? filter)
        {
            Items = items;
            FilterHost = filterHost;
            Filter = filter;
            RightToLeft = owner.FlowDirection == FlowDirection.RightToLeft;
            int textWidth = 0;
            int textHeight = 0;
            int tabWidth = (int)Math.Ceiling(MeasureSourceText(owner, "\t").Width);
            foreach (MenuItem item in items.OfType<MenuItem>().Where(item => item != filterHost))
            {
                Size caption = MeasureSourceText(item, ToSourceText(item.Header as string ?? string.Empty));
                string shortcutText = GetShortcut(item);
                Size shortcut = MeasureSourceText(item, shortcutText);
                textWidth = Math.Max(textWidth, (int)Math.Ceiling(caption.Width + tabWidth + shortcut.Width));
                textHeight = Math.Max(textHeight, (int)Math.Ceiling(Math.Max(caption.Height, shortcut.Height)));
                _itemTextPadding.Add(item, GetFontPadding(item));
                _shortcutHeights.Add(item, (int)Math.Ceiling(shortcut.Height));
            }

            // ToolStripDropDownMenu also considers non-menu-item Bounds.Width. In this
            // consumer that is the retained hosted input, including its previous fill width.
            if (filter is not null)
            {
                textWidth = Math.Max(textWidth, (int)Math.Ceiling(filter.Width));
            }

            // The source has a 16px default check/image, image padding 2, text padding
            // (8,1,9,1), a 10px arrow with 8 trailing pixels, and a 25px image gutter.
            int height = Math.Max(textHeight + 2, 16 + 4);
            int textRectangleHeight = height - 2;
            height += height % 2;
            ItemWidth = 25 + 8 + textWidth + 9 + 10 + 8;
            ItemHeight = height + 2;
            int textX = 25 + 8;
            if (RightToLeft)
            {
                textX = ItemWidth - (textX + textWidth);
            }

            TextRectangle = new Rect(textX, ((height - textRectangleHeight) / 2) + (textRectangleHeight % 2), textWidth, textRectangleHeight);
            ImageRectangle = new Rect(RightToLeft ? ItemWidth - 5 - 16 : 5, (ItemHeight - 16) / 2, 16, 16);
            ArrowRectangle = new Rect(RightToLeft ? 8 : ItemWidth - 8 - 10, (ItemHeight - height) / 2, 10, height);
            Padding = RightToLeft ? new Thickness(1, 2, 25 + 9, 2) : new Thickness(25 + 8, 2, 1, 2);
            TextPadding = GetFontPadding(owner);
        }

        public Control[] Items { get; }

        public MenuItem? FilterHost { get; }

        public TextBox? Filter { get; }

        public bool RightToLeft { get; }

        public int ItemWidth { get; }

        public int ItemHeight { get; }

        public Rect TextRectangle { get; }

        public Rect ImageRectangle { get; }

        public Rect ArrowRectangle { get; }

        public Thickness Padding { get; }

        public Thickness TextPadding { get; }

        public int PopupWidth => ItemWidth + 1;

        public Thickness GetTextPadding(MenuItem item) => _itemTextPadding[item];

        public Thickness GetShortcutPadding(MenuItem item)
        {
            Thickness padding = GetTextPadding(item);
            return new Thickness(padding.Left, Math.Max(0, ((int)TextRectangle.Height - _shortcutHeights[item]) / 2), padding.Right, 0);
        }

        internal static string GetShortcut(MenuItem item)
            => WinFormsToolStripMenuSizer.GetShortcutDisplayString(item) ?? item.InputGesture?.ToString() ?? string.Empty;

        private static Thickness GetFontPadding(TemplatedControl owner)
        {
            TextBlock fontMetrics = new()
            {
                FontFamily = owner.FontFamily,
                FontSize = owner.FontSize,
                FontStyle = owner.FontStyle,
                FontWeight = owner.FontWeight,
            };
            return WinFormsTextMeasurer.GetTextRendererPadding(fontMetrics);
        }

        private static string ToSourceText(string text)
        {
            StringBuilder source = new(text.Length);
            for (int index = 0; index < text.Length; index++)
            {
                char character = text[index];
                if (character == '_')
                {
                    if (index + 1 < text.Length && text[index + 1] == '_')
                    {
                        source.Append('_');
                        index++;
                    }
                    else
                    {
                        source.Append('&');
                    }
                }
                else
                {
                    source.Append(character);
                    if (character == '&')
                    {
                        source.Append('&');
                    }
                }
            }

            return source.ToString();
        }
    }
}
