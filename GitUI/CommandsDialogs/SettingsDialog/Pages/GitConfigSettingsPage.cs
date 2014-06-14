﻿using System;
using System.IO;
using System.Windows.Forms;
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
            Text = "Git Config";
            Translate();
        }

        protected override void Init(ISettingsPageHost aPageHost)
        {
            base.Init(aPageHost);

            CommonLogic.FillEncodings(Global_FilesEncoding);

            GlobalEditor.Items.AddRange(EditorHelper.GetEditors());
        }


        protected override string GetCommaSeparatedKeywordList()
        {
            return "path,user,name,email,merge,tool,diff,line ending,encoding,commit template";
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GitConfigSettingsPage));
        }

        public override void OnPageShown()
        {
            {
                bool canFindGitCmd = CheckSettingsLogic.CanFindGitCmd();

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

        protected override void SettingsToPage()
        {
            CommonLogic.EncodingToCombo(CurrentSettings.FilesEncoding, Global_FilesEncoding);

            GlobalUserName.Text = CurrentSettings.GetValue("user.name");
            GlobalUserEmail.Text = CurrentSettings.GetValue("user.email");
            GlobalEditor.Text = CurrentSettings.GetValue("core.editor");
            GlobalMergeTool.Text = CurrentSettings.GetValue("merge.tool");
            CommitTemplatePath.Text = CurrentSettings.GetValue("commit.template");

            MergetoolPath.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text));
            MergeToolCmd.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text));

            GlobalDiffTool.Text = CheckSettingsLogic.GetDiffToolFromConfig(CurrentSettings);

            DifftoolPath.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.path", GlobalDiffTool.Text));
            DifftoolCmd.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.cmd", GlobalDiffTool.Text));

            GlobalKeepMergeBackup.SetNullableChecked(CurrentSettings.mergetool.keepBackup.Value);

            globalAutoCrlfFalse.Checked = CurrentSettings.core.autocrlf.Value == AutoCRLFType.False;
            globalAutoCrlfInput.Checked = CurrentSettings.core.autocrlf.Value == AutoCRLFType.Input;
            globalAutoCrlfTrue.Checked = CurrentSettings.core.autocrlf.Value == AutoCRLFType.True;
        }

        /// <summary>
        /// silently does not save some settings if Git is not configured correctly
        /// (user notification is done elsewhere)
        /// </summary>
        protected override void PageToSettings()
        {
            CurrentSettings.FilesEncoding = CommonLogic.ComboToEncoding(Global_FilesEncoding);

            if (CheckSettingsLogic.CanFindGitCmd())
            {
                CurrentSettings.SetValue("user.name", GlobalUserName.Text);
                CurrentSettings.SetValue("user.email", GlobalUserEmail.Text);
                CurrentSettings.SetValue("commit.template", CommitTemplatePath.Text);
                CurrentSettings.SetPathValue("core.editor", GlobalEditor.Text);

                CheckSettingsLogic.SetDiffToolToConfig(CurrentSettings, GlobalDiffTool.Text);

                CurrentSettings.SetPathValue(string.Format("difftool.{0}.path", GlobalDiffTool.Text), DifftoolPath.Text);
                CurrentSettings.SetPathValue(string.Format("difftool.{0}.cmd", GlobalDiffTool.Text), DifftoolCmd.Text);

                CurrentSettings.SetValue("merge.tool", GlobalMergeTool.Text);

                CurrentSettings.SetPathValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text), MergetoolPath.Text);
                CurrentSettings.SetPathValue(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text), MergeToolCmd.Text);

                CurrentSettings.mergetool.keepBackup.Value = GlobalKeepMergeBackup.GetNullableChecked();

                if (globalAutoCrlfFalse.Checked) CurrentSettings.core.autocrlf.Value = AutoCRLFType.False;
                if (globalAutoCrlfInput.Checked) CurrentSettings.core.autocrlf.Value = AutoCRLFType.Input;
                if (globalAutoCrlfTrue.Checked) CurrentSettings.core.autocrlf.Value = AutoCRLFType.True;
            }
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
                return;

            MergetoolPath.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text.Trim()));
            MergeToolCmd.Text = CurrentSettings.GetValue(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text.Trim()));

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

            CurrentSettings.SetPathValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text.Trim()), MergetoolPath.Text.Trim());
            string exeName;
            string exeFile;
            if (!String.IsNullOrEmpty(MergetoolPath.Text))
            {
                exeFile = MergetoolPath.Text;
                exeName = Path.GetFileName(exeFile);
            }
            else
                exeFile = MergeToolsHelper.FindMergeToolFullPath(ConfigFileSettingsSet, GlobalMergeTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                MergetoolPath.SelectAll();
                MergetoolPath.SelectedText = "";
                MergeToolCmd.SelectAll();
                MergeToolCmd.SelectedText = "";
                if (sender != null)
                    MessageBox.Show(this, String.Format(_toolSuggestPathText.Text, exeName),
                        _mergeToolSuggestCaption.Text);
                return;
            }
            MergetoolPath.SelectAll(); // allow Undo action
            MergetoolPath.SelectedText = exeFile;
            MergeToolCmd.SelectAll();
            MergeToolCmd.SelectedText = MergeToolsHelper.MergeToolcmdSuggest(GlobalMergeTool.Text, exeFile);
        }

        private void ResolveDiffToolPath()
        {
            string kdiff3Path = MergeToolsHelper.FindPathForKDiff(CurrentSettings.GetValue("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3Path))
                return;

            kdiff3Path = MergeToolsHelper.FindFileInFolders("kdiff3.exe", MergetoolPath.Text);
            if (string.IsNullOrEmpty(kdiff3Path))
                return;

            DifftoolPath.Text = kdiff3Path;
        }

        private void DiffToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!EnvUtils.RunningOnWindows())
                return;

            CurrentSettings.SetPathValue(string.Format("difftool.{0}.path", GlobalMergeTool.Text.Trim()), MergetoolPath.Text.Trim());
            string exeName;
            string exeFile;
            if (!String.IsNullOrEmpty(DifftoolPath.Text))
            {
                exeFile = DifftoolPath.Text;
                exeName = Path.GetFileName(exeFile);
            }
            else
                exeFile = MergeToolsHelper.FindDiffToolFullPath(ConfigFileSettingsSet, GlobalDiffTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                DifftoolPath.SelectAll();
                DifftoolPath.SelectedText = "";
                DifftoolCmd.SelectAll();
                DifftoolCmd.SelectedText = "";
                if (sender != null)
                    MessageBox.Show(this, String.Format(_toolSuggestPathText.Text, exeName),
                        _diffToolSuggestCaption.Text);
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
            if (IsLoadingSettings)
                return;

            string diffTool = GlobalDiffTool.Text.Trim();
            DifftoolPath.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.path", diffTool));
            DifftoolCmd.Text = CurrentSettings.GetValue(string.Format("difftool.{0}.cmd", diffTool));

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
