using GitUI.Models;
using GitUI.Properties;

namespace GitUI.UserControls;

internal partial class OutputHistoryTabController : OutputHistoryControllerBase
{
    private TabPage? _tabPage;

    internal OutputHistoryTabController(IOutputHistoryProvider outputHistoryProvider,
                                        OutputHistoryControl outputHistoryControl,
                                        TabControl parent,
                                        string tabCaption)
        : base(outputHistoryProvider, outputHistoryControl)
    {
        if (!outputHistoryProvider.Enabled)
        {
            return;
        }

        _tabPage = new()
        {
            Text = tabCaption,
            Name = "OutputHistoryTab",
        };
        _tabPage.Controls.Add(outputHistoryControl);
        outputHistoryControl.Dock = DockStyle.Fill;
        parent.TabPages.Add(_tabPage);
        _tabPage.ImageKey = nameof(Images.GitCommandLog);
    }

    internal override bool FocusAndToggleIfPanel()
    {
        if (_tabPage is null)
        {
            return false;
        }

        ((TabControl)_tabPage.Parent).SelectedTab = _tabPage;
        _tabPage.Controls[0].Focus();
        return true;
    }
}
