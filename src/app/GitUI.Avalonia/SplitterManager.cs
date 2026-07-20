using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;

namespace GitUI;

/// <summary>Persists Avalonia splitter distances using the WinForms manager's setting shape.</summary>
public sealed class SplitterManager
{
    private readonly List<SplitterData> _splitters = [];
    private readonly SettingsSource _settings;

    public SplitterManager(SettingsSource settings)
    {
        _settings = settings;
    }

    public void AddSplitter(Grid splitter, string settingName, int? defaultDistance = null)
        => AddSplitter(new GridSplitterTarget(splitter), settingName, defaultDistance);

    internal void AddSplitter(IPersistedSplitter splitter, string settingName, int? defaultDistance = null)
        => _splitters.Add(new SplitterData(splitter, settingName, defaultDistance));

    public void RestoreSplitters()
    {
        foreach (SplitterData splitter in _splitters)
        {
            splitter.RestoreFromSettings(_settings);
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
        private readonly int? _defaultDistance;
        private readonly string _settingName;
        private readonly IPersistedSplitter _splitter;

        public SplitterData(IPersistedSplitter splitter, string settingName, int? defaultDistance)
        {
            _splitter = splitter;
            _settingName = settingName;
            _defaultDistance = defaultDistance;
        }

        internal string SizeSettingsKey => _settingName + "_Size";
        internal string DistanceSettingsKey => _settingName + "_Distance";

        public void RestoreFromSettings(SettingsSource settings)
        {
            int? distance = settings.GetInt(DistanceSettingsKey) ?? _defaultDistance;
            if (distance is > 0)
            {
                _splitter.SplitterDistance = distance.Value;
            }
        }

        public void SaveToSettings(SettingsSource settings)
        {
            double distance = _splitter.SplitterDistance;
            if (distance <= 0)
            {
                return;
            }

            double size = _splitter.SplitterSize;
            if (size > 0)
            {
                settings.SetInt(SizeSettingsKey, Convert.ToInt32(size));
            }

            settings.SetInt(DistanceSettingsKey, Convert.ToInt32(distance));
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly SplitterManager _manager;

        public TestAccessor(SplitterManager manager)
        {
            _manager = manager;
        }

        public IReadOnlyList<SplitterData> Splitters => _manager._splitters;
    }

    private sealed class GridSplitterTarget : IPersistedSplitter
    {
        private readonly DefinitionBase _leadingDefinition;
        private readonly GridResizeDirection _resizeDirection;
        private readonly Grid _splitter;

        public GridSplitterTarget(Grid splitter)
        {
            _splitter = splitter;
            GridSplitter gridSplitter = splitter.Children.OfType<GridSplitter>().Single();
            _resizeDirection = gridSplitter.ResizeDirection;
            if (_resizeDirection == GridResizeDirection.Auto)
            {
                _resizeDirection = Grid.GetColumn(gridSplitter) > 0
                    ? GridResizeDirection.Columns
                    : GridResizeDirection.Rows;
            }

            int splitterIndex = _resizeDirection == GridResizeDirection.Columns
                ? Grid.GetColumn(gridSplitter)
                : Grid.GetRow(gridSplitter);
            if (splitterIndex <= 0)
            {
                throw new InvalidOperationException("A persisted GridSplitter must follow its leading pane.");
            }

            _leadingDefinition = _resizeDirection == GridResizeDirection.Columns
                ? splitter.ColumnDefinitions[splitterIndex - 1]
                : splitter.RowDefinitions[splitterIndex - 1];
        }

        public double SplitterSize => _resizeDirection == GridResizeDirection.Columns
            ? _splitter.Bounds.Width
            : _splitter.Bounds.Height;

        public double SplitterDistance
        {
            get => _resizeDirection == GridResizeDirection.Columns
                ? ((ColumnDefinition)_leadingDefinition).ActualWidth
                : ((RowDefinition)_leadingDefinition).ActualHeight;
            set
            {
                if (value <= 0)
                {
                    return;
                }

                if (_resizeDirection == GridResizeDirection.Columns)
                {
                    ((ColumnDefinition)_leadingDefinition).Width = new GridLength(value);
                }
                else
                {
                    ((RowDefinition)_leadingDefinition).Height = new GridLength(value);
                }
            }
        }
    }
}

internal interface IPersistedSplitter
{
    double SplitterSize { get; }
    double SplitterDistance { get; set; }
}
