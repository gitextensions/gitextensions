using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.IO;
using GitCommands.Config;
using ResourceManager.Translation;

namespace GitUI.SettingsDialog.Pages
{
    public partial class GlobalSettingsSettingsPage : SettingsPageBase
    {
        private readonly TranslationString __diffToolSuggestCaption = new TranslationString("Suggest difftool cmd");

        CommonLogic _commonLogic;
        GitModule _gitModule;

        public GlobalSettingsSettingsPage(CommonLogic commonLogic, GitModule gitModule)
        {
            InitializeComponent();

            _commonLogic = commonLogic;
            _gitModule = gitModule;

            Text = "Global Settings";
        }

        protected override void OnLoadSettings()
        {
            
        }

        public override void SaveSettings()
        {
            ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();

            _commonLogic.EncodingToCombo(_gitModule.GetFilesEncoding(false), Global_FilesEncoding);

            GlobalUserName.Text = globalConfig.GetValue("user.name");
            GlobalUserEmail.Text = globalConfig.GetValue("user.email");
            GlobalEditor.Text = globalConfig.GetPathValue("core.editor");
            GlobalMergeTool.Text = globalConfig.GetValue("merge.tool");
            CommitTemplatePath.Text = globalConfig.GetValue("commit.template");

            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                MergetoolPath.Text = globalConfig.GetPathValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text));
            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                MergeToolCmd.Text = globalConfig.GetPathValue(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text));

            GlobalDiffTool.Text = CheckSettingsLogic.GetGlobalDiffToolFromConfig();

            if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                DifftoolPath.Text = globalConfig.GetPathValue(string.Format("difftool.{0}.path", GlobalDiffTool.Text));
            if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                DifftoolCmd.Text = globalConfig.GetPathValue(string.Format("difftool.{0}.cmd", GlobalDiffTool.Text));

            CommonLogic.SetCheckboxFromString(GlobalKeepMergeBackup, globalConfig.GetValue("mergetool.keepBackup"));

            string globalAutocrlf = string.Empty;
            if (globalConfig.HasValue("core.autocrlf"))
            {
                globalAutocrlf = globalConfig.GetValue("core.autocrlf").ToLower();
            }
            else if (!string.IsNullOrEmpty(Settings.GitBinDir))
            {
                try
                {
                    //the following lines only work for msysgit (i think). MSysgit has a config file
                    //in the etc directory which is checked after the local and global config. In
                    //practice this is only used to core.autocrlf. If there are more cases, we might
                    //need to consider a better solution.
                    var configFile =
                        new ConfigFile(Path.GetDirectoryName(Settings.GitBinDir).Replace("bin", "etc\\gitconfig"), false);
                    globalAutocrlf = configFile.GetValue("core.autocrlf").ToLower();
                }
                catch
                {
                }
            }

            globalAutoCrlfFalse.Checked = globalAutocrlf == "false";
            globalAutoCrlfInput.Checked = globalAutocrlf == "input";
            globalAutoCrlfTrue.Checked = globalAutocrlf == "true";
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

        private void ResolveDiffToolPath()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(_gitModule.GetGlobalSetting("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return;

            kdiff3path = MergeToolsHelper.FindFileInFolders("kdiff3.exe", MergetoolPath.Text);
            if (string.IsNullOrEmpty(kdiff3path))
                return;

            DifftoolPath.Text = kdiff3path;
        }

        private void DiffToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!Settings.RunningOnWindows())
                return;

            _gitModule.SetGlobalPathSetting(string.Format("difftool.{0}.path", GlobalMergeTool.Text.Trim()), MergetoolPath.Text.Trim());
            string exeName;
            string exeFile;
            if (!String.IsNullOrEmpty(DifftoolPath.Text))
            {
                exeFile = DifftoolPath.Text;
                exeName = Path.GetFileName(exeFile);
            }
            else
                exeFile = MergeToolsHelper.FindDiffToolFullPath(GlobalDiffTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                DifftoolPath.SelectAll();
                DifftoolPath.SelectedText = "";
                DifftoolCmd.SelectAll();
                DifftoolCmd.SelectedText = "";
                if (sender != null)
                    MessageBox.Show(this, String.Format(CheckSettingsLogic._toolSuggestPath.Text, exeName),
                        __diffToolSuggestCaption.Text);
                return;
            }
            DifftoolPath.SelectAll(); // allow Undo action
            DifftoolPath.SelectedText = exeFile;
            DifftoolCmd.SelectAll();
            DifftoolCmd.SelectedText = MergeToolsHelper.DiffToolCmdSuggest(GlobalDiffTool.Text, exeFile);
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            string mergeTool = GlobalMergeTool.Text.ToLowerInvariant();
            string exeFile = MergeToolsHelper.GetMergeToolExeFile(mergeTool);

            if (exeFile != null)
                MergetoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} ({1})|{1}", GlobalMergeTool.Text, exeFile), MergetoolPath.Text);
            else
                MergetoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} (*.exe)|*.exe", GlobalMergeTool.Text), MergetoolPath.Text);
        }

        private void GlobalDiffTool_TextChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            string diffTool = GlobalDiffTool.Text.Trim();
            DifftoolPath.Text = _gitModule.GetGlobalSetting(string.Format("difftool.{0}.path", diffTool));
            DifftoolCmd.Text = _gitModule.GetGlobalSetting(string.Format("difftool.{0}.cmd", diffTool));

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                ResolveDiffToolPath();

            DiffToolCmdSuggest_Click(null, null);
        }

        private void BrowseDiffTool_Click(object sender, EventArgs e)
        {
            string diffTool = GlobalDiffTool.Text.ToLowerInvariant();
            string exeFile = MergeToolsHelper.GetDiffToolExeFile(diffTool);

            if (exeFile != null)
                DifftoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} ({1})|{1}", GlobalDiffTool.Text, exeFile), DifftoolPath.Text);
            else
                DifftoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} (*.exe)|*.exe", GlobalDiffTool.Text), DifftoolPath.Text);
        }

        private void BrowseCommitTemplate_Click(object sender, EventArgs e)
        {
            CommitTemplatePath.Text = CommonLogic.SelectFile(".", "*.txt (*.txt)|*.txt", CommitTemplatePath.Text);
        }
    }
}
