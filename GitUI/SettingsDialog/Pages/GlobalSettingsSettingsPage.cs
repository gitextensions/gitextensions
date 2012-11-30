using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.SettingsDialog.Pages
{
    public partial class GlobalSettingsSettingsPage : SettingsPageBase
    {
        GitModule _gitModule;

        public GlobalSettingsSettingsPage(GitModule gitModule)
        {
            InitializeComponent();

            _gitModule = gitModule;

            Text = "Global Settings";
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;

            MergetoolPath.Text = _gitModule.GetGlobalSetting(string.Format("mergetool.{0}.path", GlobalMergeTool.Text.Trim()));
            MergeToolCmd.Text = _gitModule.GetGlobalSetting(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text.Trim()));

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(MergeToolCmd.Text))
                MergeToolCmd.Enabled = false;
            else
                MergeToolCmd.Enabled = true;

            MergeToolCmdSuggest_Click(null, null);
        }

        private void MergeToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!Settings.RunningOnWindows())
                return;

            _gitModule.SetGlobalPathSetting(string.Format("mergetool.{0}.path", GlobalMergeTool.Text.Trim()), MergetoolPath.Text.Trim());
            string exeName;
            string exeFile;
            if (!String.IsNullOrEmpty(MergetoolPath.Text))
            {
                exeFile = MergetoolPath.Text;
                exeName = Path.GetFileName(exeFile);
            }
            else
                exeFile = MergeToolsHelper.FindMergeToolFullPath(GlobalMergeTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                MergetoolPath.SelectAll();
                MergetoolPath.SelectedText = "";
                MergeToolCmd.SelectAll();
                MergeToolCmd.SelectedText = "";
                if (sender != null)
                    MessageBox.Show(this, String.Format(CheckSettingsLogic._toolSuggestPath.Text, exeName),
                        CheckSettingsLogic.__mergeToolSuggestCaption.Text);
                return;
            }
            MergetoolPath.SelectAll(); // allow Undo action
            MergetoolPath.SelectedText = exeFile;
            MergeToolCmd.SelectAll();
            MergeToolCmd.SelectedText = MergeToolsHelper.MergeToolcmdSuggest(GlobalMergeTool.Text, exeFile);
        }
    }
}
