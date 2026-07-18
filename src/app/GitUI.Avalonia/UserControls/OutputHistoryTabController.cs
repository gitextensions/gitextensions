using Avalonia.Controls;
using Avalonia.Threading;
using GitUI.Models;
using GitUI.Properties;

namespace GitUI.UserControls;

internal sealed class OutputHistoryTabController : OutputHistoryControllerBase
{
    private TabItem? _tabPage;

    internal OutputHistoryTabController(
        IOutputHistoryProvider outputHistoryProvider,
        OutputHistoryControl outputHistoryControl,
        TabControl parent,
        string tabCaption)
        : base(outputHistoryProvider, outputHistoryControl)
    {
        if (!outputHistoryProvider.Enabled)
        {
            return;
        }

        _tabPage = new TabItem
        {
            Header = tabCaption,
            Name = "OutputHistoryTab",
            Icon = Images.GitCommandLog,
            Content = outputHistoryControl,
        };
        _tabPage.Classes.Add("gitextensions-workspace-tab");
        parent.Items.Add(_tabPage);
    }

    internal override bool FocusAndToggleIfPanel()
    {
        if (_tabPage?.Parent is not TabControl parent)
        {
            return false;
        }

        parent.SelectedItem = _tabPage;
        Dispatcher.UIThread.Post(() => _textBox.Focus(), DispatcherPriority.Input);
        return true;
    }
}
