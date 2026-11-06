using Avalonia.Controls;
using Avalonia.Threading;
using GitCommands;
using GitUI.Models;

namespace GitUI.UserControls;

internal sealed class OutputHistoryPanelController : OutputHistoryControllerBase
{
    private readonly Grid _horizontalSplitContainer;
    private readonly OutputHistoryControl _outputHistoryControl;
    private readonly Action _showVerticalSplitContainer1;
    private readonly Grid _verticalSplitContainer1;
    private readonly Grid _verticalSplitContainer2;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(25) };
    private readonly GridSplitter _splitter;
    private readonly Border _host;
    private readonly Control _mainPanel;
    private GridLength _visibleHeight = new(150);

    internal OutputHistoryPanelController(
        IOutputHistoryProvider outputHistoryProvider,
        OutputHistoryControl outputHistoryControl,
        Grid parent,
        GridSplitter splitter,
        Border host,
        bool visible)
        : base(outputHistoryProvider, outputHistoryControl)
    {
        // Avalonia represents the source's synchronized split containers as one shared row grid.
        _horizontalSplitContainer = parent;
        _verticalSplitContainer1 = parent;
        _verticalSplitContainer2 = parent;
        _outputHistoryControl = outputHistoryControl;
        _showVerticalSplitContainer1 = () => SetVisible(visible: true);
        _splitter = splitter;
        _host = host;
        _mainPanel = parent.Children.Single(child => Grid.GetRow(child) == 0);
        _host.Child = _outputHistoryControl;

        _timer.Tick += SetSizeByVerticalSplitContainer1;
        _verticalSplitContainer1.SizeChanged += SetSizeByVerticalSplitContainer1Deferred;
        _outputHistoryControl.DetachedFromVisualTree += (_, _) =>
        {
            _timer.Stop();
            _timer.Tick -= SetSizeByVerticalSplitContainer1;
            _verticalSplitContainer1.SizeChanged -= SetSizeByVerticalSplitContainer1Deferred;
        };

        SetVisible(outputHistoryProvider.Enabled && visible);
    }

    internal override bool FocusAndToggleIfPanel()
    {
        if (!_outputHistoryProvider.Enabled)
        {
            return false;
        }

        bool show = !AppSettings.OutputHistoryPanelVisible;
        AppSettings.OutputHistoryPanelVisible.Value = show;
        SetVisible(show);
        if (show)
        {
            _showVerticalSplitContainer1();
            Dispatcher.UIThread.Post(() => _textBox.TextArea.Focus(), DispatcherPriority.Input);
        }

        return true;
    }

    internal double SplitterDistance
    {
        get => _host.IsVisible && _verticalSplitContainer1.RowDefinitions[2].ActualHeight > 0
            ? _verticalSplitContainer1.RowDefinitions[2].ActualHeight
            : _visibleHeight.Value;
        set
        {
            _visibleHeight = new GridLength(value);
            ApplyLayout();
        }
    }

    internal double SplitterSize => _verticalSplitContainer1.Bounds.Height;
    internal IReadOnlyList<Control> VisibilityControls => [_mainPanel, _host];

    internal bool Panel1Collapsed
    {
        get => !_mainPanel.IsVisible;
        set
        {
            if (value)
            {
                SetVisible(true);
            }

            _mainPanel.IsVisible = !value;
            ApplyLayout();
        }
    }

    internal bool Panel2Collapsed
    {
        get => !_host.IsVisible;
        set
        {
            if (value)
            {
                _mainPanel.IsVisible = true;
            }

            SetVisible(!value);
        }
    }

    private void SetSizeByVerticalSplitContainer1Deferred(object? sender, EventArgs eventArgs)
    {
        _timer.Stop();
        _timer.Start();
    }

    private void SetSizeByVerticalSplitContainer1(object? sender, EventArgs eventArgs)
    {
        _timer.Stop();
        SetSizeIgnoringEvents((int)Math.Round(SplitterDistance));
    }

    private void SetSizeByVerticalSplitContainer2(object? sender, EventArgs eventArgs)
        => SetSizeIgnoringEvents((int)Math.Round(_verticalSplitContainer2.RowDefinitions[2].ActualHeight));

    private void SetSizeIgnoringEvents(int height)
    {
        _verticalSplitContainer1.SizeChanged -= SetSizeByVerticalSplitContainer1Deferred;
        try
        {
            SetSize(height);
        }
        finally
        {
            _verticalSplitContainer1.SizeChanged += SetSizeByVerticalSplitContainer1Deferred;
        }
    }

    private void SetSize(int height)
    {
        bool visible = _host.IsVisible && _verticalSplitContainer1.IsVisible;
        if (visible)
        {
            double maximumHeight = Math.Max(0, _horizontalSplitContainer.Bounds.Height - _splitter.Bounds.Height - 25);
            SplitterDistance = Math.Clamp(height, 0, maximumHeight);
        }

        SetVisible(visible);
    }

    private void SetVisible(bool visible)
    {
        RowDefinitions rows = _verticalSplitContainer1.RowDefinitions;
        if (!visible && _host.IsVisible && _mainPanel.IsVisible && rows[2].Height.IsAbsolute && rows[2].Height.Value > 0)
        {
            _visibleHeight = rows[2].Height;
        }

        _host.IsVisible = visible;
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        bool mainVisible = _mainPanel.IsVisible;
        bool outputVisible = _host.IsVisible;
        RowDefinitions rows = _verticalSplitContainer1.RowDefinitions;
        rows[0].MinHeight = mainVisible ? 25 : 0;
        rows[0].Height = mainVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        rows[1].Height = mainVisible && outputVisible ? new GridLength(6) : new GridLength(0);
        rows[2].Height = outputVisible ? mainVisible ? _visibleHeight : new GridLength(1, GridUnitType.Star) : new GridLength(0);
        _splitter.IsVisible = mainVisible && outputVisible;
    }
}
