using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitUI.UserControls;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class AdvancedSettingsPage : SettingsPageWithHeader
{
    public AdvancedSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();

        var autoNormaliseSymbols = new[]
        {
            new { Key = "_", Value = "_" },
            new { Key = "-", Value = "-" },
            new { Key = "(none)", Value = "" },
        };
        cboAutoNormaliseSymbol.DisplayMember = "Key";
        cboAutoNormaliseSymbol.ValueMember = "Value";
        cboAutoNormaliseSymbol.DataSource = autoNormaliseSymbols;
        cboAutoNormaliseSymbol.SelectedIndex = 0;

        // Populate emulator selector; hide the row entirely when there is nothing to choose between.
        IReadOnlyList<ConsoleControllersFactory.EmulatorInfo> emulators = ConsoleControllersFactory.GetAvailableEmulators();
        if (emulators.Count > 1)
        {
            cboConsoleEmulator.Items.Clear();
            foreach (ConsoleControllersFactory.EmulatorInfo emulator in emulators)
            {
                cboConsoleEmulator.Items.Add(emulator);
            }

            cboConsoleEmulator.DisplayMember = nameof(ConsoleControllersFactory.EmulatorInfo.Label);
            cboConsoleEmulator.ValueMember = nameof(ConsoleControllersFactory.EmulatorInfo.Key);

            lblConsoleEmulatorChoice.Visible = true;
            cboConsoleEmulator.Visible = true;
        }
        else
        {
            lblConsoleEmulatorChoice.Visible = false;
            cboConsoleEmulator.Visible = false;
        }
    }

    protected override void SettingsToPage()
    {
        chkAlwaysShowCheckoutDlg.Checked = AppSettings.AlwaysShowCheckoutBranchDlg;
        chkUseLocalChangesAction.Checked = AppSettings.UseDefaultCheckoutBranchAction;
        chkDontSHowHelpImages.Checked = AppSettings.DontShowHelpImages;
        chkAlwaysShowAdvOpt.Checked = AppSettings.AlwaysShowAdvOpt;
        chkCheckForUpdates.Checked = AppSettings.CheckForUpdates;
        chkCheckForRCVersions.Checked = AppSettings.CheckForReleaseCandidates;
        chkConsoleEmulator.Checked = AppSettings.UseConsoleEmulatorForCommands;
        chkAutoNormaliseBranchName.Checked = AppSettings.AutoNormaliseBranchName;
        cboAutoNormaliseSymbol.Enabled = chkAutoNormaliseBranchName.Checked;
        cboAutoNormaliseSymbol.SelectedValue = AppSettings.AutoNormaliseSymbol;
        chkCommitAndPushForcedWhenAmend.Checked = AppSettings.CommitAndPushForcedWhenAmend;

        // Select the currently configured emulator in the combobox.
        cboConsoleEmulator.SelectedItem = cboConsoleEmulator.Items
            .OfType<ConsoleControllersFactory.EmulatorInfo>()
            .FirstOrDefault(x => string.Equals(x.Key, AppSettings.ConsoleEmulatorName, StringComparison.OrdinalIgnoreCase));

        if (cboConsoleEmulator.SelectedIndex < 0 && cboConsoleEmulator.Items.Count > 0)
        {
            cboConsoleEmulator.SelectedIndex = 0;
        }

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.AlwaysShowCheckoutBranchDlg = chkAlwaysShowCheckoutDlg.Checked;
        AppSettings.UseDefaultCheckoutBranchAction = chkUseLocalChangesAction.Checked;
        AppSettings.DontShowHelpImages = chkDontSHowHelpImages.Checked;
        AppSettings.AlwaysShowAdvOpt = chkAlwaysShowAdvOpt.Checked;
        AppSettings.CheckForUpdates = chkCheckForUpdates.Checked;
        AppSettings.CheckForReleaseCandidates = chkCheckForRCVersions.Checked;
        AppSettings.UseConsoleEmulatorForCommands = chkConsoleEmulator.Checked;
        AppSettings.AutoNormaliseBranchName = chkAutoNormaliseBranchName.Checked;
        AppSettings.AutoNormaliseSymbol = (string)cboAutoNormaliseSymbol.SelectedValue!;
        AppSettings.CommitAndPushForcedWhenAmend = chkCommitAndPushForcedWhenAmend.Checked;

        if (cboConsoleEmulator.SelectedItem is ConsoleControllersFactory.EmulatorInfo selected)
        {
            AppSettings.ConsoleEmulatorName = selected.Key;
        }

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));
    }

    private void chkAutoNormaliseBranchName_CheckedChanged(object sender, EventArgs e)
    {
        cboAutoNormaliseSymbol.Enabled = chkAutoNormaliseBranchName.Checked;
    }
}
