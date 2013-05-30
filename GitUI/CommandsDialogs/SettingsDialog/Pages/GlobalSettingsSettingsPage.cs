using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Utils;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GlobalSettingsSettingsPage : SettingsPageBase
    {
        private readonly TranslationString __diffToolSuggestCaption = new TranslationString("Suggest difftool cmd");

        readonly CommonLogic _commonLogic;
        readonly CheckSettingsLogic _checkSettingsLogic;
        readonly GitModule _gitModule;

        private GlobalSettingsSettingsPage()
        {
            InitializeComponent();
            Text = "Global settings";
            Translate();
        }

        public GlobalSettingsSettingsPage(CommonLogic commonLogic, CheckSettingsLogic checkSettingsLogic, GitModule gitModule)
            : this()
        {
            _commonLogic = commonLogic;
            _checkSettingsLogic = checkSettingsLogic;
            _gitModule = gitModule;

            _commonLogic.FillEncodings(Global_FilesEncoding);

            string npp = MergeToolsHelper.FindFileInFolders("notepad++.exe", "Notepad++");
            if (string.IsNullOrEmpty(npp))
                npp = "notepad++";
            else
                npp = "\"" + npp + "\"";

            GlobalEditor.Items.AddRange(new Object[] { "\"" + Settings.GetGitExtensionsFullPath() + "\" fileeditor", "vi", "notepad", npp + " -multiInst -nosession" });
        }

        protected override string GetCommaSeparatedKeywordList()
        {
            return "path,user,name,email,merge,tool,diff,line ending,encoding,commit template";
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GlobalSettingsSettingsPage));
        }

        public override void OnPageShown()
        {
            {
                bool canFindGitCmd = _checkSettingsLogic.CanFindGitCmd();

                GlobalUserName.Enabled = canFindGitCmd;
                GlobalUserEmail.Enabled = canFindGitCmd;
                GlobalEditor.Enabled = canFindGitCmd;
                CommitTemplatePath.Enabled = canFindGitCmd;
                GlobalMergeTool.Enabled = canFindGitCmd;
                MergetoolPath.Enabled = canFindGitCmd;
                MergeToolCmd.Enabled = canFindGitCmd;
                GlobalKeepMergeBackup.Enabled = canFindGitCmd;
                InvalidGitPathGlobal.Visible = !canFindGitCmd;
            }

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase)
                && string.IsNullOrEmpty(MergeToolCmd.Text))
            {
                MergeToolCmd.Enabled = false;
            }
            else
            {
                MergeToolCmd.Enabled = true;
            }
        }

        protected override void OnLoadSettings()
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

        /// <summary>
        /// silently does not save some settings if Git is not configured correctly
        /// (user notification is done elsewhere)
        /// </summary>
        public override void SaveSettings()
        {
            _gitModule.SetFilesEncoding(false, _commonLogic.ComboToEncoding(Global_FilesEncoding));

            if (_checkSettingsLogic.CanFindGitCmd())
            {
                ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();

                if (string.IsNullOrEmpty(GlobalUserName.Text) ||
                    !GlobalUserName.Text.Equals(globalConfig.GetValue("user.name")))
                    globalConfig.SetValue("user.name", GlobalUserName.Text);
                if (string.IsNullOrEmpty(GlobalUserEmail.Text) ||
                    !GlobalUserEmail.Text.Equals(globalConfig.GetValue("user.email")))
                    globalConfig.SetValue("user.email", GlobalUserEmail.Text);
                if (string.IsNullOrEmpty(CommitTemplatePath.Text) ||
                    !CommitTemplatePath.Text.Equals(globalConfig.GetValue("commit.template")))
                    globalConfig.SetValue("commit.template", CommitTemplatePath.Text);
                globalConfig.SetPathValue("core.editor", GlobalEditor.Text);

                CheckSettingsLogic.SetGlobalDiffToolToConfig(globalConfig, GlobalDiffTool.Text);

                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    globalConfig.SetPathValue(string.Format("difftool.{0}.path", GlobalDiffTool.Text), DifftoolPath.Text);
                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    globalConfig.SetPathValue(string.Format("difftool.{0}.cmd", GlobalDiffTool.Text), DifftoolCmd.Text);

                globalConfig.SetValue("merge.tool", GlobalMergeTool.Text);

                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    globalConfig.SetPathValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text), MergetoolPath.Text);
                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    globalConfig.SetPathValue(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text), MergeToolCmd.Text);

                if (GlobalKeepMergeBackup.CheckState == CheckState.Checked)
                    globalConfig.SetValue("mergetool.keepBackup", "true");
                else if (GlobalKeepMergeBackup.CheckState == CheckState.Unchecked)
                    globalConfig.SetValue("mergetool.keepBackup", "false");

                if (globalAutoCrlfFalse.Checked) globalConfig.SetValue("core.autocrlf", "false");
                if (globalAutoCrlfInput.Checked) globalConfig.SetValue("core.autocrlf", "input");
                if (globalAutoCrlfTrue.Checked) globalConfig.SetValue("core.autocrlf", "true");

                CommonLogic.SetEncoding(_gitModule.GetFilesEncoding(false), globalConfig, "i18n.filesEncoding");
                globalConfig.Save();
            }
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
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
            if (!EnvUtils.RunningOnWindows())
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
                    MessageBox.Show(this, String.Format(_checkSettingsLogic.ToolSuggestPathText.Text, exeName),
                        _checkSettingsLogic.MergeToolSuggestCaption.Text);
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
            if (!EnvUtils.RunningOnWindows())
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
                    MessageBox.Show(this, String.Format(_checkSettingsLogic.ToolSuggestPathText.Text, exeName),
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
                MergetoolPath.Text = _commonLogic.SelectFile(".", string.Format("{0} ({1})|{1}", GlobalMergeTool.Text, exeFile), MergetoolPath.Text);
            else
                MergetoolPath.Text = _commonLogic.SelectFile(".", string.Format("{0} (*.exe)|*.exe", GlobalMergeTool.Text), MergetoolPath.Text);
        }

        private void GlobalDiffTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
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
                DifftoolPath.Text = _commonLogic.SelectFile(".", string.Format("{0} ({1})|{1}", GlobalDiffTool.Text, exeFile), DifftoolPath.Text);
            else
                DifftoolPath.Text = _commonLogic.SelectFile(".", string.Format("{0} (*.exe)|*.exe", GlobalDiffTool.Text), DifftoolPath.Text);
        }

        private void BrowseCommitTemplate_Click(object sender, EventArgs e)
        {
            CommitTemplatePath.Text = _commonLogic.SelectFile(".", "*.txt (*.txt)|*.txt", CommitTemplatePath.Text);
        }
    }
}
