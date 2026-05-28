using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitUI.SettingControlBindings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class DetailedSettingsPage : DistributedSettingsPage
{
    private readonly List<ISettingControlBinding> _controlBindings;
    public DetailedSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();

        _controlBindings = [SettingControlBindingsProvider.CreateControlBinding(DetailedSettings.GetRemoteBranchesDirectlyFromRemote, chkRemotesFromServer),
                            SettingControlBindingsProvider.CreateControlBinding(DetailedSettings.AddMergeLogMessages, addLogMessages),
                            SettingControlBindingsProvider.CreateControlBinding(DetailedSettings.MergeLogMessagesCount, nbMessages)];
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
    }

    protected override void SettingsToPage()
    {
        gbRevisionGraph.Enabled = GetCurrentSettings().SettingLevel == SettingLevel.Global;

        chkMergeGraphLanesHavingCommonParent.Checked = AppSettings.MergeGraphLanesHavingCommonParent.Value;
        chkRenderGraphWithDiagonals.Checked = AppSettings.RenderGraphWithDiagonals.Value;
        chkStraightenGraphDiagonals.Checked = AppSettings.StraightenGraphDiagonals.Value;

        foreach (ISettingControlBinding controlBinding in _controlBindings)
        {
            controlBinding.LoadSetting(GetCurrentSettings());
        }

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.MergeGraphLanesHavingCommonParent.Value = chkMergeGraphLanesHavingCommonParent.Checked;
        AppSettings.RenderGraphWithDiagonals.Value = chkRenderGraphWithDiagonals.Checked;
        AppSettings.StraightenGraphDiagonals.Value = chkStraightenGraphDiagonals.Checked;

        foreach (ISettingControlBinding controlBinding in _controlBindings)
        {
            controlBinding.SaveSetting(GetCurrentSettings());
        }

        base.PageToSettings();
    }
}
