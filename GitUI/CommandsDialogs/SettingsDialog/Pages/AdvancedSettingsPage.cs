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
        }

        protected override void SettingsToPage()
        {
            chkAlwaysShowCheckoutDlg.Checked = AppSettings.AlwaysShowCheckoutBranchDlg;
            chkUseLocalChangesAction.Checked = AppSettings.UseDefaultCheckoutBranchAction;
            chkDontSHowHelpImages.Checked = AppSettings.DontShowHelpImages;
            chkAlwaysShowAdvOpt.Checked = AppSettings.AlwaysShowAdvOpt;
            chkCheckForRCVersions.Checked = AppSettings.CheckForReleaseCandidates;
            chkRememberIgnoreWhiteSpacePreference.Checked = AppSettings.RememberIgnoreWhiteSpacePreference;
            chkConsoleEmulator.Checked = AppSettings.UseConsoleEmulatorForCommands;
        }

        protected override void PageToSettings()
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = chkAlwaysShowCheckoutDlg.Checked;
            AppSettings.UseDefaultCheckoutBranchAction = chkUseLocalChangesAction.Checked;
            AppSettings.DontShowHelpImages = chkDontSHowHelpImages.Checked;
            AppSettings.AlwaysShowAdvOpt = chkAlwaysShowAdvOpt.Checked;
            AppSettings.CheckForReleaseCandidates = chkCheckForRCVersions.Checked;
            AppSettings.RememberIgnoreWhiteSpacePreference = chkRememberIgnoreWhiteSpacePreference.Checked;
            AppSettings.UseConsoleEmulatorForCommands = chkConsoleEmulator.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));
        }        
    }
}
