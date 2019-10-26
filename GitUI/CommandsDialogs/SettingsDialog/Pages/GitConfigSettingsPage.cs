using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.DiffMergeTools;
using GitCommands.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitConfigSettingsPage : ConfigFileSettingsPage
    {
        private readonly TranslationString _selectFile = new TranslationString("Select file");
        private readonly GitConfigSettingsPageController _controller;
        private DiffMergeToolConfigurationManager _diffMergeToolConfigurationManager;

        public GitConfigSettingsPage()
        {
            InitializeComponent();
            Text = "Config";

            _NO_TRANSLATE_GlobalMergeTool.Items.AddRange(RegisteredDiffMergeTools.All(DiffMergeToolType.Merge).ToArray());
            _NO_TRANSLATE_GlobalDiffTool.Items.AddRange(RegisteredDiffMergeTools.All(DiffMergeToolType.Diff).ToArray());

            InitializeComplete();

            _controller = new GitConfigSettingsPageController();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            _diffMergeToolConfigurationManager = new DiffMergeToolConfigurationManager(() => CurrentSettings);

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
        }

        protected override void SettingsToPage()
        {
            var mergeTool = _diffMergeToolConfigurationManager.ConfiguredMergeTool;
            var diffTool = _diffMergeToolConfigurationManager.ConfiguredDiffTool;

            Global_FilesEncoding.Text = CurrentSettings.FilesEncoding?.EncodingName ?? "";

            GlobalUserName.Text = CurrentSettings.GetValue(SettingKeyString.UserName);
            GlobalUserEmail.Text = CurrentSettings.GetValue(SettingKeyString.UserEmail);
            GlobalEditor.Text = CurrentSettings.GetValue("core.editor");
            CommitTemplatePath.Text = CurrentSettings.GetValue("commit.template");

            _NO_TRANSLATE_GlobalMergeTool.Text = mergeTool;
            MergetoolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(mergeTool, DiffMergeToolType.Merge);
            MergeToolCmd.Text = _diffMergeToolConfigurationManager.GetToolCommand(mergeTool, DiffMergeToolType.Merge);

            _NO_TRANSLATE_GlobalDiffTool.Text = diffTool;
            DifftoolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(diffTool, DiffMergeToolType.Diff);
            DifftoolCmd.Text = _diffMergeToolConfigurationManager.GetToolCommand(diffTool, DiffMergeToolType.Diff);

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

            // TODO: why use GUI???
            var diffTool = _NO_TRANSLATE_GlobalDiffTool.Text;
            if (!string.IsNullOrWhiteSpace(diffTool))
            {
                _diffMergeToolConfigurationManager.ConfigureDiffMergeTool(diffTool, DiffMergeToolType.Diff, DifftoolPath.Text, DifftoolCmd.Text);
            }
            else
            {
                _diffMergeToolConfigurationManager.UnsetCurrentTool(DiffMergeToolType.Diff);
            }

            // TODO: merge.guitool???
            var mergeTool = _NO_TRANSLATE_GlobalMergeTool.Text;
            if (!string.IsNullOrWhiteSpace(mergeTool))
            {
                _diffMergeToolConfigurationManager.ConfigureDiffMergeTool(mergeTool, DiffMergeToolType.Merge, MergetoolPath.Text, MergeToolCmd.Text);
            }
            else
            {
                _diffMergeToolConfigurationManager.UnsetCurrentTool(DiffMergeToolType.Merge);
            }

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

        private string BrowseDiffMergeTool(string toolName, string path)
        {
            DiffMergeToolConfiguration diffMergeToolConfig = default;
            if (_NO_TRANSLATE_GlobalDiffTool.SelectedIndex > -1)
            {
                diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, null);
            }

            string initialDirectory = _controller.GetInitialDirectory(path, diffMergeToolConfig.Path);

            var filter = !string.IsNullOrWhiteSpace(diffMergeToolConfig.ExeFileName)
                ? $"{toolName}|{diffMergeToolConfig.ExeFileName}"
                : "*.exe;*.cmd;*.bat|*.exe;*.cmd;*.bat";

            using (var dialog = new OpenFileDialog
            {
                Filter = filter,
                InitialDirectory = initialDirectory,
                Title = _selectFile.Text
            })
            {
                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : path;
            }
        }

        private void SuggestDiffToolCommand()
        {
            var toolName = _NO_TRANSLATE_GlobalDiffTool.Text;
            if (string.IsNullOrWhiteSpace(toolName))
            {
                DifftoolCmd.Text = string.Empty;
                return;
            }

            var diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, DifftoolPath.Text);
            DifftoolCmd.Text = diffMergeToolConfig.FullDiffCommand;
        }

        private void SuggestMergeToolCommand()
        {
            var toolName = _NO_TRANSLATE_GlobalMergeTool.Text;
            if (string.IsNullOrWhiteSpace(toolName))
            {
                MergeToolCmd.Text = string.Empty;
                return;
            }

            var diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, MergetoolPath.Text);
            MergeToolCmd.Text = diffMergeToolConfig.FullMergeCommand;
        }

        private void MergeToolCmdSuggest_Click(object sender, EventArgs e)
            => SuggestMergeToolCommand();

        private void DiffToolCmdSuggest_Click(object sender, EventArgs e)
            => SuggestDiffToolCommand();

        private void DiffMergeToolPath_LostFocus(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            if (sender == DifftoolPath)
            {
                DifftoolPath.Text = DifftoolPath.Text.ToPosixPath();
                return;
            }

            if (sender == MergetoolPath)
            {
                MergetoolPath.Text = MergetoolPath.Text.ToPosixPath();
                return;
            }
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            MergetoolPath.Text = BrowseDiffMergeTool(_NO_TRANSLATE_GlobalMergeTool.Text, MergetoolPath.Text).ToPosixPath();
            SuggestMergeToolCommand();
        }

        private void GlobalDiffTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            var toolName = _NO_TRANSLATE_GlobalDiffTool.Text;

            DifftoolPath.Enabled =
                BrowseDiffTool.Enabled =
                    DifftoolCmd.Enabled =
                        DiffToolCmdSuggest.Enabled = !string.IsNullOrEmpty(toolName);

            string toolPath;
            if (string.IsNullOrWhiteSpace(toolName) || string.IsNullOrWhiteSpace(toolPath = _diffMergeToolConfigurationManager.GetToolPath(toolName, DiffMergeToolType.Diff)))
            {
                DifftoolPath.Text =
                    DifftoolCmd.Text = string.Empty;
                return;
            }

            DifftoolPath.Text = toolPath;
            DifftoolCmd.Text = _diffMergeToolConfigurationManager.GetToolCommand(toolName, DiffMergeToolType.Diff);
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            var toolName = _NO_TRANSLATE_GlobalMergeTool.Text;

            MergetoolPath.Enabled =
                BrowseMergeTool.Enabled =
                    MergeToolCmd.Enabled =
                        MergeToolCmdSuggest.Enabled = !string.IsNullOrEmpty(toolName);

            string toolPath;
            if (string.IsNullOrWhiteSpace(toolName) || string.IsNullOrWhiteSpace(toolPath = _diffMergeToolConfigurationManager.GetToolPath(toolName, DiffMergeToolType.Merge)))
            {
                MergetoolPath.Text =
                    MergeToolCmd.Text = string.Empty;
                return;
            }

            MergetoolPath.Text = toolPath;
            MergeToolCmd.Text = _diffMergeToolConfigurationManager.GetToolCommand(toolName, DiffMergeToolType.Merge);
        }

        private void BrowseDiffTool_Click(object sender, EventArgs e)
        {
            DifftoolPath.Text = BrowseDiffMergeTool(_NO_TRANSLATE_GlobalDiffTool.Text, DifftoolPath.Text).ToPosixPath();
            SuggestDiffToolCommand();
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
