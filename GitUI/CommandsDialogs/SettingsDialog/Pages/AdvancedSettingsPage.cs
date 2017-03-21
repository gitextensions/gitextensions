﻿using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AdvancedSettingsPage : SettingsPageWithHeader
    {
        public AdvancedSettingsPage()
        {
            InitializeComponent();
            Text = "Advanced";
            Translate();

            var autoNormaliseSymbols = new[] {
                new { Key =  "_", Value = "_" },
                new { Key =  "-", Value = "-" },
                new { Key =  "(none)", Value = "" },
            };
            cboAutoNormaliseSymbol.DataSource = autoNormaliseSymbols;
            cboAutoNormaliseSymbol.SelectedIndex = 0;
        }

        protected override void SettingsToPage()
        {
            chkAlwaysShowCheckoutDlg.Checked = AppSettings.AlwaysShowCheckoutBranchDlg;
            chkUseLocalChangesAction.Checked = AppSettings.UseDefaultCheckoutBranchAction;
            chkDontSHowHelpImages.Checked = AppSettings.DontShowHelpImages;
            chkAlwaysShowAdvOpt.Checked = AppSettings.AlwaysShowAdvOpt;
            chkCheckForRCVersions.Checked = AppSettings.CheckForReleaseCandidates;
            chkConsoleEmulator.Checked = AppSettings.UseConsoleEmulatorForCommands;
            chkAutoNormaliseBranchName.Checked = AppSettings.AutoNormaliseBranchName;
            cboAutoNormaliseSymbol.Enabled = chkAutoNormaliseBranchName.Checked;
            cboAutoNormaliseSymbol.SelectedValue = AppSettings.AutoNormaliseSymbol;
        }

        protected override void PageToSettings()
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = chkAlwaysShowCheckoutDlg.Checked;
            AppSettings.UseDefaultCheckoutBranchAction = chkUseLocalChangesAction.Checked;
            AppSettings.DontShowHelpImages = chkDontSHowHelpImages.Checked;
            AppSettings.AlwaysShowAdvOpt = chkAlwaysShowAdvOpt.Checked;
            AppSettings.CheckForReleaseCandidates = chkCheckForRCVersions.Checked;
            AppSettings.UseConsoleEmulatorForCommands = chkConsoleEmulator.Checked;
            AppSettings.AutoNormaliseBranchName = chkAutoNormaliseBranchName.Checked;
            AppSettings.AutoNormaliseSymbol = (string)cboAutoNormaliseSymbol.SelectedValue;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));
        }


        private void chkAutoNormaliseBranchName_CheckedChanged(object sender, System.EventArgs e)
        {
            cboAutoNormaliseSymbol.Enabled = chkAutoNormaliseBranchName.Checked;
        }
    }
}
