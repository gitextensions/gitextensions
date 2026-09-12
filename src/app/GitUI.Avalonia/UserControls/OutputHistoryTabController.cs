using Avalonia.Controls;
using Avalonia.Threading;
using GitUI.Models;
using GitUI.Properties;
using TabPage = GitUI.Compat.WinFormsControls.TabPage;

namespace GitUI.UserControls;

internal sealed class OutputHistoryTabController : OutputHistoryControllerBase
{
    private TabPage? _tabPage;

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

        _tabPage = new TabPage
        {
            Header = tabCaption,
            Name = "OutputHistoryTab",
            Icon = Images.GitCommandLog,
            Content = outputHistoryControl,
            TabIndex = 4,
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
        Dispatcher.UIThread.Post(() => _textBox.TextArea.Focus(), DispatcherPriority.Input);
        return true;
    }
}
