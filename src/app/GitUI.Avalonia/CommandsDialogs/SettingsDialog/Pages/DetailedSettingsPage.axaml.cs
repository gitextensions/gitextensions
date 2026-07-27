using System.Globalization;
using Avalonia.Controls;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class DetailedSettingsPage : DistributedSettingsPage
{
    public DetailedSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public DetailedSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(DetailedSettingsPage));

    protected override void SettingsToPage()
    {
        SettingsSource currentSettings = GetCurrentSettings();
        gbRevisionGraph.IsEnabled = currentSettings.SettingLevel == SettingLevel.Global;

        chkMergeGraphLanesHavingCommonParent.IsChecked = AppSettings.MergeGraphLanesHavingCommonParent.Value;
        chkRenderGraphWithDiagonals.IsChecked = AppSettings.RenderGraphWithDiagonals.Value;
        chkStraightenGraphDiagonals.IsChecked = AppSettings.StraightenGraphDiagonals.Value;
        chkRemotesFromServer.IsChecked = DetailedSettings.GetRemoteBranchesDirectlyFromRemote.ValueOrDefault(currentSettings);
        addLogMessages.IsChecked = DetailedSettings.AddMergeLogMessages.ValueOrDefault(currentSettings);
        nbMessages.Text = DetailedSettings.MergeLogMessagesCount.ValueOrDefault(currentSettings).ToString(CultureInfo.InvariantCulture);

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        SettingsSource currentSettings = GetCurrentSettings();
        AppSettings.MergeGraphLanesHavingCommonParent.Value = chkMergeGraphLanesHavingCommonParent.IsChecked == true;
        AppSettings.RenderGraphWithDiagonals.Value = chkRenderGraphWithDiagonals.IsChecked == true;
        AppSettings.StraightenGraphDiagonals.Value = chkStraightenGraphDiagonals.IsChecked == true;
        DetailedSettings.GetRemoteBranchesDirectlyFromRemote[currentSettings] = chkRemotesFromServer.IsChecked;
        DetailedSettings.AddMergeLogMessages[currentSettings] = addLogMessages.IsChecked;
        if (int.TryParse(nbMessages.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int numberOfMessages))
        {
            DetailedSettings.MergeLogMessagesCount[currentSettings] = numberOfMessages;
        }

        base.PageToSettings();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(DetailedSettingsPage page)
    {
        public CheckBox MergeGraphLanesHavingCommonParent => page.chkMergeGraphLanesHavingCommonParent;

        public CheckBox RenderGraphWithDiagonals => page.chkRenderGraphWithDiagonals;

        public CheckBox StraightenGraphDiagonals => page.chkStraightenGraphDiagonals;

        public CheckBox RemotesFromServer => page.chkRemotesFromServer;

        public CheckBox AddLogMessages => page.addLogMessages;

        public TextBox NumberOfMessages => page.nbMessages;

        public void SaveSettings() => page.PageToSettings();
    }
}
