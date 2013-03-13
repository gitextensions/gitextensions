using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class BehaviourSettingsPage : SettingsPageBase
    {
        public BehaviourSettingsPage()
        {
            InitializeComponent();
            Text = "Behaviour";
            //NOW Check how Translate() works
            Translate();    
        }

        protected override void OnLoadSettings()
        {
            chkCommitKeepSelection.Checked = Settings.CommitForm_KeepSelectionOnFilesWhenStageUnstage;
        }

        public override void SaveSettings()
        {
            Settings.CommitForm_KeepSelectionOnFilesWhenStageUnstage = chkCommitKeepSelection.Checked;            
        }

    }
}
