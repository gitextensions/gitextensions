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

        bool show = !AppSettings.OutputHistoryPanelVisible.Value;
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
            if (_host.IsVisible)
            {
                _verticalSplitContainer1.RowDefinitions[2].Height = _visibleHeight;
            }
        }
    }

    internal double SplitterSize => _splitter.Bounds.Height;

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
            double maximumHeight = Math.Max(0, _horizontalSplitContainer.Bounds.Height - SplitterSize);
            SplitterDistance = Math.Clamp(height, 0, maximumHeight);
        }

        SetVisible(visible);
    }

    private void SetVisible(bool visible)
    {
        RowDefinitions rows = _verticalSplitContainer1.RowDefinitions;
        if (!visible && rows[2].Height.Value > 0)
        {
            _visibleHeight = rows[2].Height;
        }

        rows[1].Height = visible ? new GridLength(6) : new GridLength(0);
        rows[2].Height = visible ? _visibleHeight : new GridLength(0);
        _splitter.IsVisible = visible;
        _host.IsVisible = visible;
    }
}
