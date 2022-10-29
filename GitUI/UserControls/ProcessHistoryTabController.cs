using GitUI.Models;
using GitUI.Properties;

namespace GitUI.UserControls;

public partial class ProcessHistoryTabController : ProcessHistoryControllerBase
{
    private TabPage? _tabPage;

    public ProcessHistoryTabController(IProcessHistoryModel processHistoryModel,
                                       ProcessHistoryControl processHistoryControl,
                                       TabControl parent,
                                       string tabCaption)
        : base(processHistoryModel, processHistoryControl)
    {
        if (!processHistoryModel.Enabled)
        {
            return;
        }

        _tabPage = new()
        {
            Text = tabCaption,
            Name = "ProcessHistoryTab",
        };
        _tabPage.Controls.Add(processHistoryControl);
        processHistoryControl.Dock = DockStyle.Fill;
        parent.TabPages.Add(_tabPage);
        _tabPage.ImageKey = nameof(Images.GitCommandLog);
    }

    public override void ToggleControl()
    {
        if (_tabPage is null)
        {
            return;
        }

        ((TabControl)_tabPage.Parent).SelectedTab = _tabPage;
        _tabPage.Controls[0].Focus();
    }
}
