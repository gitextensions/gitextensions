using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class DetailedSettingsPage : DistributedSettingsPage
{
    public DetailedSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
    }

    protected override void SettingsToPage()
    {
        chkMergeGraphLanesHavingCommonParent.Checked = AppSettings.MergeGraphLanesHavingCommonParent.Value;
        chkRenderGraphWithDiagonals.Checked = AppSettings.RenderGraphWithDiagonals.Value;
        chkStraightenGraphDiagonals.Checked = AppSettings.StraightenGraphDiagonals.Value;

        IDetailedSettings detailedSettings = GetCurrentSettings()
            .Detailed();

        chkRemotesFromServer.Checked = detailedSettings.GetRemoteBranchesDirectlyFromRemote;
        addLogMessages.Checked = detailedSettings.AddMergeLogMessages;
        nbMessages.Text = detailedSettings.MergeLogMessagesCount.ToString();

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.MergeGraphLanesHavingCommonParent.Value = chkMergeGraphLanesHavingCommonParent.Checked;
        AppSettings.RenderGraphWithDiagonals.Value = chkRenderGraphWithDiagonals.Checked;
        AppSettings.StraightenGraphDiagonals.Value = chkStraightenGraphDiagonals.Checked;

        IDetailedSettings detailedSettings = GetCurrentSettings()
            .Detailed();

        detailedSettings.GetRemoteBranchesDirectlyFromRemote = chkRemotesFromServer.Checked;
        detailedSettings.AddMergeLogMessages = addLogMessages.Checked;

        if (int.TryParse(nbMessages.Text, out int messagesCount))
        {
            detailedSettings.MergeLogMessagesCount = messagesCount;
        }

        base.PageToSettings();
    }
}
