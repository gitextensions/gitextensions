﻿using GitCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace GitUI.SettingsDialog.Pages
{
    public partial class AdvancedSettingsPage : SettingsPageBase
    {
        public AdvancedSettingsPage()
        {
            InitializeComponent();
            Text = "Advanced";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            chkAlwaysShowCheckoutDlg.Checked = Settings.AlwaysShowCheckoutBranchDlg;
            chkUseLocalChangesAction.Checked = Settings.UseDefaultCheckoutBranchAction;
            chkDontSHowHelpImages.Checked = Settings.DontShowHelpImages;
        }

        public override void SaveSettings()
        {
            Settings.AlwaysShowCheckoutBranchDlg = chkAlwaysShowCheckoutDlg.Checked;
            Settings.UseDefaultCheckoutBranchAction = chkUseLocalChangesAction.Checked;
            Settings.DontShowHelpImages = chkDontSHowHelpImages.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));
        }        
    }
}
