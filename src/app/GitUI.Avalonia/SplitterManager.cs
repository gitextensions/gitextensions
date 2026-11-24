using Avalonia.Controls;
using Avalonia.Controls.Documents;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI;

namespace GitUI;

public sealed class SplitterManager
{
    private readonly List<SplitterData> _splitters = [];
    private readonly SettingsSource _settings;

    public SplitterManager(SettingsSource settings)
    {
        _settings = settings;
    }

    public void AddSplitter(Grid splitter, string settingName, int? defaultDistance = null)
        => AddSplitter(splitter, settingName, FixedPanel.None, defaultDistance);

    internal void AddSplitter(Grid splitter, string settingName, FixedPanel fixedPanel, int? defaultDistance = null)
    {
        GridSplitterTarget target = new(splitter, fixedPanel);
        SplitterData data = AddSplitter(target, settingName, defaultDistance);
        splitter.SizeChanged += (_, _) =>
        {
            if (data.IsRestorePending && target.SplitterSize > 0)
            {
                data.RestoreFromSettings(_settings);
            }
        };
    }

    internal void AddSplitter(
        string settingName,
        Func<double> getDistance,
        Action<double> setDistance,
        Func<double> getSize,
        int? defaultDistance = null,
        FixedPanel fixedPanel = FixedPanel.None,
        int panel1MinSize = 25,
        int panel2MinSize = 25,
        int splitterWidth = 6,
        Func<bool>? getPanel1Collapsed = null,
        Action<bool>? setPanel1Collapsed = null,
        Func<bool>? getPanel2Collapsed = null,
        Action<bool>? setPanel2Collapsed = null,
        Func<FixedPanel>? getFixedPanel = null,
        Func<double, double>? fromLegacyDistance = null,
        IReadOnlyList<Control>? visibilityControls = null)
    {
        DelegatingSplitterTarget target = new(getDistance, setDistance, getSize, fixedPanel,
            panel1MinSize, panel2MinSize, splitterWidth,
            getPanel1Collapsed, setPanel1Collapsed, getPanel2Collapsed, setPanel2Collapsed, getFixedPanel, fromLegacyDistance);
        target.ObserveVisibility(visibilityControls ?? []);
        AddSplitter(target, settingName, defaultDistance);
    }

    internal SplitterData AddSplitter(IPersistedSplitter splitter, string settingName, int? defaultDistance = null)
    {
        // Avalonia distances are DIPs; persist their logical DPI, not the desktop's physical scale.
        SplitterData data = new(splitter, settingName, 96, defaultDistance);
        _splitters.Add(data);
        return data;
    }

    public void RestoreSplitters()
    {
        foreach (SplitterData splitter in _splitters)
        {
            splitter.RestoreFromSettings(_settings);
        }
    }

    /// <summary>
    ///  Restores the distances which have been stored for a larger window than the one the
    ///  splitters had while <see cref="RestoreSplitters"/> ran, and which had to be skipped
    ///  therefore. To be called once the form has reached its final size.
    /// </summary>
    public void RestorePendingSplitters()
    {
        foreach (SplitterData splitter in _splitters)
        {
            if (splitter.IsRestorePending)
            {
                splitter.RestoreFromSettings(_settings);
            }
        }
    }

    public void SaveSplitters()
    {
        foreach (SplitterData splitter in _splitters)
        {
            splitter.SaveToSettings(_settings);
        }
    }

    internal sealed class SplitterData
    {
        private readonly IPersistedSplitter _splitter;
        private readonly string _settingName;
        private readonly int _dpi;
        private readonly int? _defaultDistance;

        public SplitterData(IPersistedSplitter splitter, string settingName, int dpi, int? defaultDistance)
        {
            _splitter = splitter;
            _settingName = settingName;
            _dpi = dpi;
            _defaultDistance = defaultDistance;
        }

        private int SplitterSize => Convert.ToInt32(_splitter.SplitterSize);
        internal string SizeSettingsKey => _settingName + "_Size";
        internal string DpiSettingsKey => _settingName + "_Dpi";
        internal string DistanceSettingsKey => _settingName + "_Distance";
        internal string FontSizeSettingsKey => _settingName + "_FontSize";
        internal string Panel1CollapsedSettingsKey => _settingName + "_Panel1Collapsed";
        internal string Panel2CollapsedSettingsKey => _settingName + "_Panel2Collapsed";

        /// <summary>
        ///  Gets whether the stored distance did not fit the size the splitter had while it was
        ///  restored, so that it has been kept for <see cref="RestorePendingSplitters"/>.
        /// </summary>
        internal bool IsRestorePending { get; private set; }

        public void RestoreFromSettings(SettingsSource settings)
        {
            IsRestorePending = false;

            int prevDpi = settings.GetInt(DpiSettingsKey) ?? _dpi;
            int? prevSizeSetting = settings.GetInt(SizeSettingsKey);
            int prevSize = prevSizeSetting ?? 0;
            int? prevDistanceSetting = settings.GetInt(DistanceSettingsKey);
            int prevDistance = prevDistanceSetting ?? 0;

            bool legacyNative = settings.GetInt(DpiSettingsKey) is null && prevSize <= _splitter.SplitterWidth && prevDistance > 0;
            if ((prevSize > 0 && prevDistance > 0) || legacyNative)
            {
                FixedPanel fixedPanel = _splitter.FixedPanel;
                int splitterWidth = _splitter.SplitterWidth;
                if (legacyNative)
                {
                    SetSplitterDistance((float)_splitter.FromLegacyDistance(prevDistance));
                }
                else if (SplitterSize == prevSize && _dpi == prevDpi)
                {
                    SetSplitterDistance(fixedPanel == FixedPanel.Panel2 ? prevDistance + splitterWidth : prevDistance);
                }
                else
                {
                    switch (fixedPanel)
                    {
                        case FixedPanel.None:
                            // At this point, the property "SplitterSize" has its current value,
                            // the final size after opening the window may still be unknown. The calculation below
                            // determines the resulting splitter distance by the ratio of both sides of the splitter.
                            SetSplitterDistance((float)SplitterSize * prevDistance / prevSize);
                            break;
                        case FixedPanel.Panel1:
                            SetSplitterDistance(DpiUtil.Scale(prevDistance, prevDpi));
                            break;
                        case FixedPanel.Panel2:
                            int panel2PrevSize = DpiUtil.Scale(prevSize, prevDpi) - DpiUtil.Scale(prevDistance, prevDpi);
                            const int paddingOffset = 0; // Avalonia has no FormCommit.ctor+WorkaroundPaddingIncreaseBug padding increase
                            SetSplitterDistance(SplitterSize - panel2PrevSize - paddingOffset + splitterWidth);
                            break;
                    }
                }
            }
            else if (_defaultDistance.HasValue && !prevSizeSetting.HasValue && !prevDistanceSetting.HasValue)
            {
                SetSplitterDistance(_defaultDistance.Value);
            }

            // Keep the collapsed states as in the designer if not stored yet
            _splitter.Panel1Collapsed = settings.GetBool(Panel1CollapsedSettingsKey, defaultValue: _splitter.Panel1Collapsed);
            _splitter.Panel2Collapsed = settings.GetBool(Panel2CollapsedSettingsKey, defaultValue: _splitter.Panel2Collapsed);
        }

        public void SaveToSettings(SettingsSource settings)
        {
            // An unrealized Avalonia grid has no Designer bounds; do not replace a valid saved layout with zeroes.
            if (SplitterSize <= 0 || _splitter.SplitterDistance <= 0)
            {
                return;
            }

            settings.SetInt(DpiSettingsKey, _dpi);
            settings.SetInt(SizeSettingsKey, SplitterSize);
            settings.SetInt(DistanceSettingsKey, Convert.ToInt32(_splitter.SplitterDistance));
            settings.SetFloat(FontSizeSettingsKey, _splitter.FontSize);
            settings.SetBool(Panel1CollapsedSettingsKey, _splitter.Panel1Collapsed);
            settings.SetBool(Panel2CollapsedSettingsKey, _splitter.Panel2Collapsed);
        }

        private void SetSplitterDistance(float distance)
        {
            try
            {
                int intDistance = Convert.ToInt32(distance);

                if (IsValidSplitterDistance(intDistance))
                {
                    _splitter.SplitterDistance = intDistance;
                }
                else
                {
                    // The window is still smaller than the one the distance has been stored for,
                    // because its stored bounds and state are applied only after the form has been
                    // constructed. Keep the distance for RestorePendingSplitters() rather than
                    // dropping it, which used to reset the layout of an enlarged window.
                    IsRestorePending = true;

                    if (_defaultDistance.HasValue && IsValidSplitterDistance(_defaultDistance.Value))
                    {
                        _splitter.SplitterDistance = _defaultDistance.Value;
                    }
                }
            }
            catch
            {
                // The attempt to set even the default value has failed.
            }

            bool IsValidSplitterDistance(int d)
            {
                return d > _splitter.Panel1MinSize &&
                       d < SplitterSize - _splitter.Panel2MinSize;
            }
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly SplitterManager _manager;

        public TestAccessor(SplitterManager manager)
        {
            _manager = manager;
        }

        public List<SplitterData> Splitters => _manager._splitters;
    }

    private sealed class GridSplitterTarget : IPersistedSplitter
    {
        private readonly Grid _splitter;
        private readonly GridSplitter _handle;
        private readonly bool _columns;
        private readonly int _index;
        private readonly int _splitterWidth;
        private double _distance;
        private double _referenceSize;
        private double _referenceDistance;
        private GridLength _leading;
        private GridLength _trailing;
        private bool _panel1Collapsed;
        private bool _panel2Collapsed;
        private readonly Dictionary<Control, bool> _visibility = [];

        public GridSplitterTarget(Grid splitter, FixedPanel fixedPanel)
        {
            _splitter = splitter;
            FixedPanel = fixedPanel;
            _handle = splitter.Children.OfType<GridSplitter>().Single();
            _columns = _handle.ResizeDirection == GridResizeDirection.Columns
                || (_handle.ResizeDirection == GridResizeDirection.Auto && Grid.GetColumn(_handle) > 0);
            _index = _columns ? Grid.GetColumn(_handle) : Grid.GetRow(_handle);
            if (_index <= 0)
            {
                throw new InvalidOperationException("A persisted GridSplitter must follow its leading pane.");
            }

            _leading = GetLength(_index - 1);
            _trailing = GetLength(_index + 1);
            _splitterWidth = Convert.ToInt32(GetLength(_index).Value);
            _distance = _leading.IsAbsolute ? _leading.Value : 0;
            _splitter.SizeChanged += (_, _) => Resize();
            _handle.DragCompleted += (_, _) =>
            {
                _referenceSize = SplitterSize;
                _referenceDistance = SplitterDistance;
            };
            if (_columns)
            {
                splitter.ColumnDefinitions[_index - 1].MinWidth = Math.Max(25, splitter.ColumnDefinitions[_index - 1].MinWidth);
                splitter.ColumnDefinitions[_index + 1].MinWidth = Math.Max(25, splitter.ColumnDefinitions[_index + 1].MinWidth);
            }
            else
            {
                splitter.RowDefinitions[_index - 1].MinHeight = Math.Max(25, splitter.RowDefinitions[_index - 1].MinHeight);
                splitter.RowDefinitions[_index + 1].MinHeight = Math.Max(25, splitter.RowDefinitions[_index + 1].MinHeight);
            }
        }

        public double SplitterSize => _columns ? _splitter.Bounds.Width : _splitter.Bounds.Height;
        public FixedPanel FixedPanel { get; }
        public int SplitterWidth => _splitterWidth;
        public int Panel1MinSize => Convert.ToInt32(_columns
            ? _splitter.ColumnDefinitions[_index - 1].MinWidth : _splitter.RowDefinitions[_index - 1].MinHeight);
        public int Panel2MinSize => Convert.ToInt32(_columns
            ? _splitter.ColumnDefinitions[_index + 1].MinWidth : _splitter.RowDefinitions[_index + 1].MinHeight);
        public float FontSize => (float)(_splitter.GetValue(TextElement.FontSizeProperty) * 72 / 96);

        public double SplitterDistance
        {
            get => _panel1Collapsed || _panel2Collapsed
                ? _distance
                : _columns ? _splitter.ColumnDefinitions[_index - 1].ActualWidth : _splitter.RowDefinitions[_index - 1].ActualHeight;
            set
            {
                _distance = value;
                _referenceSize = SplitterSize;
                _referenceDistance = value;
                double trailing = Math.Max(0, SplitterSize - SplitterWidth - value);
                _leading = FixedPanel == FixedPanel.Panel2 ? new GridLength(1, GridUnitType.Star) : new GridLength(value);
                _trailing = FixedPanel == FixedPanel.Panel2 ? new GridLength(trailing) : new GridLength(1, GridUnitType.Star);
                ApplyLayout();
            }
        }

        public bool Panel1Collapsed
        {
            get => _panel1Collapsed;
            set => SetCollapsed(value, panel1: true);
        }

        public bool Panel2Collapsed
        {
            get => _panel2Collapsed;
            set => SetCollapsed(value, panel1: false);
        }

        private void Resize()
        {
            if (FixedPanel != FixedPanel.None || _panel1Collapsed || _panel2Collapsed || SplitterSize <= 0)
            {
                return;
            }

            if (_referenceSize <= 0)
            {
                _referenceSize = SplitterSize;
                _referenceDistance = SplitterDistance;
                return;
            }

            _distance = Math.Clamp(Math.Floor(_referenceDistance * SplitterSize / _referenceSize), Panel1MinSize, Math.Max(Panel1MinSize, SplitterSize - Panel2MinSize - SplitterWidth));
            _leading = new GridLength(_distance);
            ApplyLayout();
        }

        private GridLength GetLength(int index) => _columns ? _splitter.ColumnDefinitions[index].Width : _splitter.RowDefinitions[index].Height;

        private void SetLength(int index, GridLength length)
        {
            if (_columns)
            {
                _splitter.ColumnDefinitions[index].Width = length;
            }
            else
            {
                _splitter.RowDefinitions[index].Height = length;
            }
        }

        private void SetCollapsed(bool value, bool panel1)
        {
            if ((panel1 ? _panel1Collapsed : _panel2Collapsed) == value)
            {
                return;
            }

            if (value && (panel1 ? _panel2Collapsed : _panel1Collapsed))
            {
                SetCollapsed(false, !panel1);
            }

            if (!_panel1Collapsed && !_panel2Collapsed)
            {
                _distance = SplitterDistance;
                _leading = GetLength(_index - 1);
                _trailing = GetLength(_index + 1);
            }

            if (panel1)
            {
                _panel1Collapsed = value;
            }
            else
            {
                _panel2Collapsed = value;
            }

            int panelIndex = panel1 ? _index - 1 : _index + 1;
            foreach (Control child in _splitter.Children.Where(child => (_columns ? Grid.GetColumn(child) : Grid.GetRow(child)) == panelIndex))
            {
                if (value)
                {
                    _visibility[child] = child.IsVisible;
                    child.IsVisible = false;
                }
                else if (_visibility.Remove(child, out bool visible))
                {
                    child.IsVisible = visible;
                }
            }

            ApplyLayout();
            Resize();
        }

        private void ApplyLayout()
        {
            if (_columns)
            {
                _splitter.ColumnDefinitions[_index - 1].MinWidth = _panel1Collapsed ? 0 : 25;
                _splitter.ColumnDefinitions[_index + 1].MinWidth = _panel2Collapsed ? 0 : 25;
            }
            else
            {
                _splitter.RowDefinitions[_index - 1].MinHeight = _panel1Collapsed ? 0 : 25;
                _splitter.RowDefinitions[_index + 1].MinHeight = _panel2Collapsed ? 0 : 25;
            }

            SetLength(_index - 1, _panel1Collapsed ? new GridLength(0) : _panel2Collapsed ? new GridLength(1, GridUnitType.Star) : _leading);
            SetLength(_index + 1, _panel2Collapsed ? new GridLength(0) : _panel1Collapsed ? new GridLength(1, GridUnitType.Star) : _trailing);
            bool visible = !_panel1Collapsed && !_panel2Collapsed;
            SetLength(_index, new GridLength(visible ? _splitterWidth : 0));
            _handle.IsVisible = visible;
        }
    }

    private sealed class DelegatingSplitterTarget(
        Func<double> getDistance,
        Action<double> setDistance,
        Func<double> getSize,
        FixedPanel fixedPanel,
        int panel1MinSize,
        int panel2MinSize,
        int splitterWidth,
        Func<bool>? getPanel1Collapsed,
        Action<bool>? setPanel1Collapsed,
        Func<bool>? getPanel2Collapsed,
        Action<bool>? setPanel2Collapsed,
        Func<FixedPanel>? getFixedPanel,
        Func<double, double>? fromLegacyDistance) : IPersistedSplitter
    {
        private double? _collapsedDistance;
        private bool _wasCollapsed;
        private bool IsCollapsed => Panel1Collapsed || Panel2Collapsed;

        public double SplitterSize => getSize();
        public FixedPanel FixedPanel => getFixedPanel?.Invoke() ?? fixedPanel;
        public int Panel1MinSize => panel1MinSize;
        public int Panel2MinSize => panel2MinSize;
        public int SplitterWidth => splitterWidth;
        public float FontSize => GitCommands.AppSettings.Font.Size;
        public double FromLegacyDistance(double distance) => fromLegacyDistance?.Invoke(distance) ?? distance;

        public double SplitterDistance
        {
            get
            {
                if (!IsCollapsed)
                {
                    _collapsedDistance = null;
                    return getDistance();
                }

                if (_collapsedDistance is null && SplitterSize > 0)
                {
                    _collapsedDistance = getDistance();
                }

                return _collapsedDistance ?? getDistance();
            }
            set
            {
                _collapsedDistance = IsCollapsed ? value : null;
                setDistance(value);
            }
        }

        public bool Panel1Collapsed
        {
            get => getPanel1Collapsed?.Invoke() ?? false;
            set => SetCollapsed(panel1: true, value);
        }

        public bool Panel2Collapsed
        {
            get => getPanel2Collapsed?.Invoke() ?? false;
            set => SetCollapsed(panel1: false, value);
        }

        public void ObserveVisibility(IReadOnlyList<Control> controls)
        {
            _wasCollapsed = IsCollapsed;
            foreach (Control control in controls)
            {
                control.PropertyChanged += (_, e) =>
                {
                    if (e.Property != Control.IsVisibleProperty)
                    {
                        return;
                    }

                    bool collapsed = IsCollapsed;
                    if (collapsed && !_wasCollapsed && SplitterSize > 0)
                    {
                        _collapsedDistance = getDistance();
                    }
                    else if (!collapsed)
                    {
                        _collapsedDistance = null;
                    }

                    _wasCollapsed = collapsed;
                };
            }
        }

        private void SetCollapsed(bool panel1, bool value)
        {
            if (_collapsedDistance is null && SplitterSize > 0 && (IsCollapsed || value))
            {
                _collapsedDistance = getDistance();
            }

            if (panel1)
            {
                setPanel1Collapsed?.Invoke(value);
            }
            else
            {
                setPanel2Collapsed?.Invoke(value);
            }

            if (!IsCollapsed)
            {
                _collapsedDistance = null;
            }

            _wasCollapsed = IsCollapsed;
        }
    }
}

internal enum FixedPanel
{
    None,
    Panel1,
    Panel2
}

internal interface IPersistedSplitter
{
    double SplitterSize { get; }
    double SplitterDistance { get; set; }
    FixedPanel FixedPanel { get; }
    int Panel1MinSize { get; }
    int Panel2MinSize { get; }
    int SplitterWidth { get; }
    float FontSize { get; }
    bool Panel1Collapsed { get; set; }
    bool Panel2Collapsed { get; set; }

    // Older native callback settings stored the handle size without DPI; convert their distance at the control boundary.
    double FromLegacyDistance(double distance) => distance;
}
