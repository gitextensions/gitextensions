using System;
using System.IO;
using System.Windows.Forms;
using GitCommands.Config;
using GitCommands.Settings;
using GitCommands.Utils;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitConfigSettingsPage : ConfigFileSettingsPage
    {
        private readonly TranslationString _toolSuggestPathText =
            new TranslationString("Please enter the path to {0} and press suggest.");

        private readonly TranslationString _diffToolSuggestCaption = new TranslationString("Suggest difftool cmd");
        private readonly TranslationString _mergeToolSuggestCaption = new TranslationString("Suggest mergetool cmd");

        public GitConfigSettingsPage()
        {
            InitializeComponent();
            Text = "Config";
            Translate();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CommonLogic.FillEncodings(Global_FilesEncoding);

            GlobalEditor.Items.AddRange(EditorHelper.GetEditors());
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GitConfigSettingsPage));
        }

        public override void OnPageShown()
        {
            bool canFindGitCmd = CheckSettingsLogic.CanFindGitCmd();

            GlobalUserName.Enabled = canFindGitCmd;
            GlobalUserEmail.Enabled = canFindGitCmd;
            GlobalEditor.Enabled = canFindGitCmd;
            CommitTemplatePath.Enabled = canFindGitCmd;
            _NO_TRANSLATE_GlobalMergeTool.Enabled = canFindGitCmd;
            MergetoolPath.Enabled = canFindGitCmd;
            MergeToolCmd.Enabled = canFindGitCmd;
            GlobalKeepMergeBackup.Enabled = canFindGitCmd;
            InvalidGitPathGlobal.Visible = !canFindGitCmd;

            var isKdiff3 = _NO_TRANSLATE_GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase);

            MergeToolCmd.Enabled = !isKdiff3 || !string.IsNullOrEmpty(MergeToolCmd.Text);
        }

        protected override void SettingsToPage()
        {
            CommonLogic.EncodingToCombo(CurrentSettings.FilesEncoding, Global_FilesEncoding);

            GlobalUserName.Text = CurrentSettings.GetValue(SettingKeyString.UserName);
            GlobalUserEmail.Text = CurrentSettings.GetValue(SettingKeyString.UserEmail);
            GlobalEditor.Text = CurrentSettings.GetValue("core.editor");
            _NO_TRANSLATE_GlobalMergeTool.Text = CurrentSettings.GetValue("merge.tool");
            CommitTemplatePath.Text = CurrentSettings.GetValue("commit.template");

            MergetoolPath.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.path", _NO_TRANSLATE_GlobalMergeTool.Text));
            MergeToolCmd.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.cmd", _NO_TRANSLATE_GlobalMergeTool.Text));

            _NO_TRANSLATE_GlobalDiffTool.Text = CheckSettingsLogic.GetDiffToolFromConfig(CurrentSettings);

            DifftoolPath.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.path", _NO_TRANSLATE_GlobalDiffTool.Text));
            DifftoolCmd.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.cmd", _NO_TRANSLATE_GlobalDiffTool.Text));

            GlobalKeepMergeBackup.SetNullableChecked(CurrentSettings.mergetool.keepBackup.Value);

            globalAutoCrlfFalse.Checked = CurrentSettings.core.autocrlf.Value == AutoCRLFType.@false;
            globalAutoCrlfInput.Checked = CurrentSettings.core.autocrlf.Value == AutoCRLFType.input;
            globalAutoCrlfTrue.Checked = CurrentSettings.core.autocrlf.Value == AutoCRLFType.@true;
            globalAutoCrlfNotSet.Checked = !CurrentSettings.core.autocrlf.Value.HasValue;
        }

        /// <summary>
        /// silently does not save some settings if Git is not configured correctly
        /// (user notification is done elsewhere)
        /// </summary>
        protected override void PageToSettings()
        {
            CurrentSettings.FilesEncoding = CommonLogic.ComboToEncoding(Global_FilesEncoding);

            if (!CheckSettingsLogic.CanFindGitCmd())
            {
                return;
            }

            CurrentSettings.SetValue(SettingKeyString.UserName, GlobalUserName.Text);
            CurrentSettings.SetValue(SettingKeyString.UserEmail, GlobalUserEmail.Text);
            CurrentSettings.SetValue("commit.template", CommitTemplatePath.Text);
            CurrentSettings.SetPathValue("core.editor", GlobalEditor.Text);

            CheckSettingsLogic.SetDiffToolToConfig(CurrentSettings, _NO_TRANSLATE_GlobalDiffTool.Text);

            CurrentSettings.SetPathValue(string.Format("difftool.{0}.path", _NO_TRANSLATE_GlobalDiffTool.Text), DifftoolPath.Text);
            CurrentSettings.SetPathValue(string.Format("difftool.{0}.cmd", _NO_TRANSLATE_GlobalDiffTool.Text), DifftoolCmd.Text);

            CurrentSettings.SetValue("merge.tool", _NO_TRANSLATE_GlobalMergeTool.Text);

            CurrentSettings.SetPathValue(string.Format("mergetool.{0}.path", _NO_TRANSLATE_GlobalMergeTool.Text), MergetoolPath.Text);
            CurrentSettings.SetPathValue(string.Format("mergetool.{0}.cmd", _NO_TRANSLATE_GlobalMergeTool.Text), MergeToolCmd.Text);

            CurrentSettings.mergetool.keepBackup.Value = GlobalKeepMergeBackup.GetNullableChecked();

            if (globalAutoCrlfFalse.Checked)
            {
                CurrentSettings.core.autocrlf.Value = AutoCRLFType.@false;
            }

            if (globalAutoCrlfInput.Checked)
            {
                CurrentSettings.core.autocrlf.Value = AutoCRLFType.input;
            }

            if (globalAutoCrlfTrue.Checked)
            {
                CurrentSettings.core.autocrlf.Value = AutoCRLFType.@true;
            }

            if (globalAutoCrlfNotSet.Checked)
            {
                CurrentSettings.core.autocrlf.Value = null;
            }
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            MergetoolPath.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.path", _NO_TRANSLATE_GlobalMergeTool.Text.Trim()));
            MergeToolCmd.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.cmd", _NO_TRANSLATE_GlobalMergeTool.Text.Trim()));

            var isKdiff3 = _NO_TRANSLATE_GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase);

            MergeToolCmd.Enabled = !isKdiff3 || !string.IsNullOrEmpty(MergeToolCmd.Text);

            MergeToolCmdSuggest_Click(null, null);
        }

        private void MergeToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return;
            }

            CurrentSettings.SetPathValue(string.Format("mergetool.{0}.path", _NO_TRANSLATE_GlobalMergeTool.Text.Trim()), MergetoolPath.Text.Trim());
            string exeName;
            string exeFile;
            if (!string.IsNullOrEmpty(MergetoolPath.Text))
            {
                exeFile = MergetoolPath.Text;
                exeName = Path.GetFileName(exeFile);
            }
            else
            {
                exeFile = MergeToolsHelper.FindMergeToolFullPath(ConfigFileSettingsSet, _NO_TRANSLATE_GlobalMergeTool.Text, out exeName);
            }

            if (string.IsNullOrEmpty(exeFile))
            {
                MergetoolPath.SelectAll();
                MergetoolPath.SelectedText = "";
                MergeToolCmd.SelectAll();
                MergeToolCmd.SelectedText = "";
                if (sender != null)
                {
                    MessageBox.Show(this, string.Format(_toolSuggestPathText.Text, exeName),
                        _mergeToolSuggestCaption.Text);
                }

                return;
            }

            MergetoolPath.SelectAll(); // allow Undo action
            MergetoolPath.SelectedText = exeFile;
            MergeToolCmd.SelectAll();
            MergeToolCmd.SelectedText = MergeToolsHelper.MergeToolcmdSuggest(_NO_TRANSLATE_GlobalMergeTool.Text, exeFile);
        }

        private void ResolveDiffToolPath()
        {
            string kdiff3Path = MergeToolsHelper.FindPathForKDiff(CurrentSettings.GetValue("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3Path))
            {
                return;
            }

            kdiff3Path = MergeToolsHelper.FindFileInFolders("kdiff3.exe", MergetoolPath.Text);
            if (string.IsNullOrEmpty(kdiff3Path))
            {
                return;
            }

            DifftoolPath.Text = kdiff3Path;
        }

        private void DiffToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return;
            }

            CurrentSettings.SetPathValue(string.Format("difftool.{0}.path", _NO_TRANSLATE_GlobalDiffTool.Text.Trim()), DifftoolPath.Text.Trim());
            string exeName;
            string exeFile;
            if (!string.IsNullOrEmpty(DifftoolPath.Text))
            {
                exeFile = DifftoolPath.Text;
                exeName = Path.GetFileName(exeFile);
            }
            else
            {
                exeFile = MergeToolsHelper.FindDiffToolFullPath(ConfigFileSettingsSet, _NO_TRANSLATE_GlobalDiffTool.Text, out exeName);
            }

            if (string.IsNullOrEmpty(exeFile))
            {
                DifftoolPath.SelectAll();
                DifftoolPath.SelectedText = "";
                DifftoolCmd.SelectAll();
                DifftoolCmd.SelectedText = "";
                if (sender != null)
                {
                    MessageBox.Show(this, string.Format(_toolSuggestPathText.Text, exeName),
                        _diffToolSuggestCaption.Text);
                }

                return;
            }

            DifftoolPath.SelectAll(); // allow Undo action
            DifftoolPath.SelectedText = exeFile;
            DifftoolCmd.SelectAll();
            DifftoolCmd.SelectedText = MergeToolsHelper.DiffToolCmdSuggest(_NO_TRANSLATE_GlobalDiffTool.Text, exeFile);
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            string mergeTool = _NO_TRANSLATE_GlobalMergeTool.Text.ToLowerInvariant();
            string exeFile = MergeToolsHelper.GetMergeToolExeFile(mergeTool);

            var filter = exeFile != null
                ? string.Format("{0} ({1})|{1}", _NO_TRANSLATE_GlobalMergeTool.Text, exeFile)
                : string.Format("{0} (*.exe)|*.exe", _NO_TRANSLATE_GlobalMergeTool.Text);

            MergetoolPath.Text = CommonLogic.SelectFile(".", filter, MergetoolPath.Text);
        }

        private void GlobalDiffTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            string diffTool = _NO_TRANSLATE_GlobalDiffTool.Text.Trim();
            DifftoolPath.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.path", diffTool));
            DifftoolCmd.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.cmd", diffTool));

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                ResolveDiffToolPath();
            }

            DiffToolCmdSuggest_Click(null, null);
        }

        private void BrowseDiffTool_Click(object sender, EventArgs e)
        {
            string diffTool = _NO_TRANSLATE_GlobalDiffTool.Text.ToLowerInvariant();
            string exeFile = MergeToolsHelper.GetDiffToolExeFile(diffTool);

            var filter = exeFile != null
                ? string.Format("{0} ({1})|{1}", _NO_TRANSLATE_GlobalDiffTool.Text, exeFile)
                : string.Format("{0} (*.exe)|*.exe", _NO_TRANSLATE_GlobalDiffTool.Text);

            DifftoolPath.Text = CommonLogic.SelectFile(".", filter, DifftoolPath.Text);
        }

        private void BrowseCommitTemplate_Click(object sender, EventArgs e)
        {
            CommitTemplatePath.Text = CommonLogic.SelectFile(".", "*.txt (*.txt)|*.txt", CommitTemplatePath.Text);
        }

        private void ConfigureEncoding_Click(object sender, EventArgs e)
        {
            using (var encodingDlg = new FormAvailableEncodings())
            {
                if (encodingDlg.ShowDialog() == DialogResult.OK)
                {
                    Global_FilesEncoding.Items.Clear();
                    CommonLogic.FillEncodings(Global_FilesEncoding);
                }
            }
        }
    }
}
