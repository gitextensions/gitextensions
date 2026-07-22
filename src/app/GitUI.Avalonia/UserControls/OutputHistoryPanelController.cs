using Avalonia.Controls;
using Avalonia.Threading;
using GitCommands;
using GitUI.Models;

namespace GitUI.UserControls;

internal sealed class OutputHistoryPanelController : OutputHistoryControllerBase
{
    private readonly Grid _parent;
    private readonly GridSplitter _splitter;
    private readonly Border _host;
    private GridLength _visibleHeight = new(150);

    internal OutputHistoryPanelController(
        IOutputHistoryProvider outputHistoryProvider,
        OutputHistoryControl outputHistoryControl,
        Grid parent,
        GridSplitter splitter,
        Border host)
        : base(outputHistoryProvider, outputHistoryControl)
    {
        _parent = parent;
        _splitter = splitter;
        _host = host;
        _host.Child = outputHistoryControl;

        SetVisible(outputHistoryProvider.Enabled && AppSettings.OutputHistoryPanelVisible.Value);
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
            Dispatcher.UIThread.Post(() => _textBox.TextArea.Focus(), DispatcherPriority.Input);
        }

        return true;
    }

    private void SetVisible(bool visible)
    {
        RowDefinitions rows = _parent.RowDefinitions;
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
