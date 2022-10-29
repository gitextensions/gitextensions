using GitUI.Models;
using GitUI.Properties;

namespace GitUI.UserControls;

public partial class OutputHistoryTabController : OutputHistoryControllerBase
{
    private TabPage? _tabPage;

    public OutputHistoryTabController(IOutputHistoryModel outputHistoryModel,
                                      OutputHistoryControl outputHistoryControl,
                                      TabControl parent,
                                      string tabCaption)
        : base(outputHistoryModel, outputHistoryControl)
    {
        if (!outputHistoryModel.Enabled)
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

    public override bool ToggleControl()
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
