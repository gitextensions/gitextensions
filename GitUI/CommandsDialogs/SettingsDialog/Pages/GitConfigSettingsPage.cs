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

            _NO_TRANSLATE_cboMergeTool.Items.AddRange(RegisteredDiffMergeTools.All(DiffMergeToolType.Merge).ToArray());
            _NO_TRANSLATE_cboDiffTool.Items.AddRange(RegisteredDiffMergeTools.All(DiffMergeToolType.Diff).ToArray());

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
            txtCommitTemplatePath.Enabled = canFindGitCmd;
            _NO_TRANSLATE_cboMergeTool.Enabled = canFindGitCmd;
            txtMergeToolPath.Enabled = canFindGitCmd;
            txtMergeToolCommand.Enabled = canFindGitCmd;
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
            txtCommitTemplatePath.Text = CurrentSettings.GetValue("commit.template");

            _NO_TRANSLATE_cboMergeTool.Text = mergeTool;
            txtMergeToolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(mergeTool, DiffMergeToolType.Merge);
            txtMergeToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(mergeTool, DiffMergeToolType.Merge);

            _NO_TRANSLATE_cboDiffTool.Text = diffTool;
            txtDiffToolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(diffTool, DiffMergeToolType.Diff);
            txtDiffToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(diffTool, DiffMergeToolType.Diff);

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
            CurrentSettings.SetValue("commit.template", txtCommitTemplatePath.Text);
            CurrentSettings.SetPathValue("core.editor", GlobalEditor.Text);

            // TODO: why use GUI???
            var diffTool = _NO_TRANSLATE_cboDiffTool.Text;
            if (!string.IsNullOrWhiteSpace(diffTool))
            {
                _diffMergeToolConfigurationManager.ConfigureDiffMergeTool(diffTool, DiffMergeToolType.Diff, txtDiffToolPath.Text, txtDiffToolCommand.Text);
            }
            else
            {
                _diffMergeToolConfigurationManager.UnsetCurrentTool(DiffMergeToolType.Diff);
            }

            // TODO: merge.guitool???
            var mergeTool = _NO_TRANSLATE_cboMergeTool.Text;
            if (!string.IsNullOrWhiteSpace(mergeTool))
            {
                _diffMergeToolConfigurationManager.ConfigureDiffMergeTool(mergeTool, DiffMergeToolType.Merge, txtMergeToolPath.Text, txtMergeToolCommand.Text);
            }
            else
            {
                _diffMergeToolConfigurationManager.UnsetCurrentTool(DiffMergeToolType.Merge);
            }

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
            if (_NO_TRANSLATE_cboDiffTool.SelectedIndex > -1)
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
            var toolName = _NO_TRANSLATE_cboDiffTool.Text;
            if (string.IsNullOrWhiteSpace(toolName))
            {
                txtDiffToolCommand.Text = string.Empty;
                return;
            }

            var diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, txtDiffToolPath.Text);
            txtDiffToolCommand.Text = diffMergeToolConfig.FullDiffCommand;
        }

        private void SuggestMergeToolCommand()
        {
            var toolName = _NO_TRANSLATE_cboMergeTool.Text;
            if (string.IsNullOrWhiteSpace(toolName))
            {
                txtMergeToolCommand.Text = string.Empty;
                return;
            }

            var diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, txtMergeToolPath.Text);
            txtMergeToolCommand.Text = diffMergeToolConfig.FullMergeCommand;
        }

        private void btnMergeToolCommandSuggest_Click(object sender, EventArgs e)
            => SuggestMergeToolCommand();

        private void btnDiffToolCommandSuggest_Click(object sender, EventArgs e)
            => SuggestDiffToolCommand();

        private void txtDiffMergeToolPath_LostFocus(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            if (sender == txtDiffToolPath)
            {
                txtDiffToolPath.Text = txtDiffToolPath.Text.ToPosixPath();
                return;
            }

            if (sender == txtMergeToolPath)
            {
                txtMergeToolPath.Text = txtMergeToolPath.Text.ToPosixPath();
                return;
            }
        }

        private void btnMergeToolBrowse_Click(object sender, EventArgs e)
        {
            txtMergeToolPath.Text = BrowseDiffMergeTool(_NO_TRANSLATE_cboMergeTool.Text, txtMergeToolPath.Text).ToPosixPath();
            SuggestMergeToolCommand();
        }

        private void cboDiffTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            var toolName = _NO_TRANSLATE_cboDiffTool.Text;

            txtDiffToolPath.Enabled =
                btnDiffToolBrowse.Enabled =
                    txtDiffToolCommand.Enabled =
                        btnDiffToolCommandSuggest.Enabled = !string.IsNullOrEmpty(toolName);

            string toolPath;
            if (string.IsNullOrWhiteSpace(toolName) || string.IsNullOrWhiteSpace(toolPath = _diffMergeToolConfigurationManager.GetToolPath(toolName, DiffMergeToolType.Diff)))
            {
                txtDiffToolPath.Text =
                    txtDiffToolCommand.Text = string.Empty;
                return;
            }

            txtDiffToolPath.Text = toolPath;
            txtDiffToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(toolName, DiffMergeToolType.Diff);
        }

        private void cboMergeTool_TextChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            var toolName = _NO_TRANSLATE_cboMergeTool.Text;

            txtMergeToolPath.Enabled =
                btnMergeToolBrowse.Enabled =
                    txtMergeToolCommand.Enabled =
                        btnMergeToolCommandSuggest.Enabled = !string.IsNullOrEmpty(toolName);

            string toolPath;
            if (string.IsNullOrWhiteSpace(toolName) || string.IsNullOrWhiteSpace(toolPath = _diffMergeToolConfigurationManager.GetToolPath(toolName, DiffMergeToolType.Merge)))
            {
                txtMergeToolPath.Text =
                    txtMergeToolCommand.Text = string.Empty;
                return;
            }

            txtMergeToolPath.Text = toolPath;
            txtMergeToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(toolName, DiffMergeToolType.Merge);
        }

        private void btnDiffToolBrowse_Click(object sender, EventArgs e)
        {
            txtDiffToolPath.Text = BrowseDiffMergeTool(_NO_TRANSLATE_cboDiffTool.Text, txtDiffToolPath.Text).ToPosixPath();
            SuggestDiffToolCommand();
        }

        private void btnCommitTemplateBrowse_Click(object sender, EventArgs e)
        {
            txtCommitTemplatePath.Text = CommonLogic.SelectFile(".", "*.txt (*.txt)|*.txt", txtCommitTemplatePath.Text);
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
