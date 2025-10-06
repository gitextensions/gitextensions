#nullable enable

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Config;
using GitCommands.DiffMergeTools;
using GitCommands.Settings;
using GitCommands.Utils;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Settings;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigSettingsPage : GitConfigBaseSettingsPage
{
    private readonly TranslationString _selectFile = new("Select file");
    private readonly GitConfigSettingsPageController _controller;
    private DiffMergeToolConfigurationManager? _diffMergeToolConfigurationManager;

    public GitConfigSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
        InitializeComponent();

        txtDiffToolPath.Enabled =
            btnDiffToolBrowse.Enabled =
                txtDiffToolCommand.Enabled =
                    btnDiffToolCommandSuggest.Enabled = false;

        txtMergeToolPath.Enabled =
            btnMergeToolBrowse.Enabled =
                txtMergeToolCommand.Enabled =
                    btnMergeToolCommandSuggest.Enabled = false;

        _NO_TRANSLATE_cboMergeTool.Items.AddRange(RegisteredDiffMergeTools.All(DiffMergeToolType.Merge).ToArray());
        _NO_TRANSLATE_cboDiffTool.Items.AddRange(RegisteredDiffMergeTools.All(DiffMergeToolType.Diff).ToArray());

        InitializeComplete();

        _controller = new GitConfigSettingsPageController();
    }

    private string? AdaptCommandIfWsl(string? command)
    {
        if (string.IsNullOrEmpty(command) || !PathUtil.IsWslPath(Module?.WorkingDir))
        {
            return command;
        }

        // Replace "D:" with "/mnt/d"
        int colonIndex = command.IndexOf(':');
        if (colonIndex == (command[0] == '"' ? 2 : 1))
        {
            int windowsDriveIndex = colonIndex - 1;
            command = $"{command[..windowsDriveIndex]}/mnt/{char.ToLower(command[windowsDriveIndex])}{command[(colonIndex + 1)..]}";
        }

        return Regex.Replace(command, @"\$(LOCAL|REMOTE|BASE|MERGED)", @"$(wslpath -aw $&)").ToPosixPath();
    }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);

        _diffMergeToolConfigurationManager = new DiffMergeToolConfigurationManager(() => CurrentSettings);

        CommonLogic.FillEncodings(Global_FilesEncoding);

        const string gitCredentialHelperPrefix = "git-credential-";
        string[] linuxCredentialHelpers = ["oauth"];
        if (EnvUtils.RunningOnWindows())
        {
            cbxCredentialHelper.Items.AddRange(PathUtil.IsWslPath(Module?.WorkingDir)
                ? [.. FindGitCredentialHelpers().Select(path => path.ToWslPath().Replace(" ", @"\ ")), .. linuxCredentialHelpers]
                : [.. FindGitCredentialHelpers().Select(GetCredentialHelperName)]);
        }
        else
        {
            cbxCredentialHelper.Items.AddRange(linuxCredentialHelpers);
        }

        cbxCredentialHelper.Items.AddRange(["store", "cache"]);

        GlobalEditor.Items.AddRange([.. EditorHelper.GetEditors().Select(AdaptCommandIfWsl).WhereNotNull()]);

        return;

        static IEnumerable<string> FindGitCredentialHelpers()
        {
            try
            {
                string? gitDir = Path.GetDirectoryName(AppSettings.GitCommand);
                if (gitDir?.EndsWith("bin") is true)
                {
                    gitDir = Path.GetDirectoryName(gitDir);
                }

                return !Directory.Exists(gitDir)
                    ? []
                    : Directory.GetFiles(gitDir, $"{gitCredentialHelperPrefix}*.exe", SearchOption.AllDirectories)
                        .Where(path => !path.Contains("git-credential-helper-selector"));
            }
            catch (Exception exception)
            {
                Trace.Write(exception);
                return [];
            }
        }

        static string GetCredentialHelperName(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            return name.StartsWith(gitCredentialHelperPrefix) ? name[gitCredentialHelperPrefix.Length..] : path;
        }
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
        cbxCredentialHelper.Enabled = canFindGitCmd;
        GlobalEditor.Enabled = canFindGitCmd;
        txtCommitTemplatePath.Enabled = canFindGitCmd;
        _NO_TRANSLATE_cboMergeTool.Enabled = canFindGitCmd;
        txtMergeToolPath.Enabled = canFindGitCmd;
        txtMergeToolCommand.Enabled = canFindGitCmd;
        InvalidGitPathGlobal.Visible = !canFindGitCmd;

        if (ReadOnly)
        {
            // Unselect has no effect in SettingsToPage because ComboBox asynchronously lives its own life
            ComboBox[] comboBoxes = [cbxCredentialHelper, GlobalEditor];
            foreach (ComboBox comboBox in comboBoxes)
            {
                comboBox.Select(start: comboBox.Text.Length, length: 0);
            }
        }
    }

    protected override void SettingsToPage()
    {
        Validates.NotNull(_diffMergeToolConfigurationManager);
        Validates.NotNull(CurrentSettings);

        string? mergeTool = _diffMergeToolConfigurationManager.ConfiguredMergeTool;
        string? diffTool = _diffMergeToolConfigurationManager.ConfiguredDiffTool;

        Global_FilesEncoding.SelectedItem = new GitEncodingSettingsGetter(CurrentSettings).FilesEncoding;

        GlobalUserName.Text = CurrentSettings.GetValue(SettingKeyString.UserName);
        GlobalUserEmail.Text = CurrentSettings.GetValue(SettingKeyString.UserEmail);
        GlobalEditor.Text = CurrentSettings.GetValue("core.editor");
        txtCommitTemplatePath.Text = CurrentSettings.GetValue("commit.template");

        // Hide credential helper because EffectiveGitConfigSettings can only return the last value
        GitConfigSettings? gitConfigSettings = TryGetGitConfigSettings(CurrentSettings);
        bool showCredentialHelper = gitConfigSettings is not null;
        lblCredentialHelper.Visible = showCredentialHelper;
        cbxCredentialHelper.Visible = showCredentialHelper;
        cbxCredentialHelper.Enabled = showCredentialHelper;
        if (gitConfigSettings is not null)
        {
            IReadOnlyList<string> values = gitConfigSettings.GetValues(SettingKeyString.CredentialHelper);
            cbxCredentialHelper.Enabled = values.Count <= 1;
            cbxCredentialHelper.Text = string.Join(", ", values);
        }

        _NO_TRANSLATE_cboMergeTool.Text = mergeTool;
        txtMergeToolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(mergeTool, DiffMergeToolType.Merge);
        txtMergeToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(mergeTool, DiffMergeToolType.Merge);

        _NO_TRANSLATE_cboDiffTool.Text = diffTool;
        txtDiffToolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(diffTool, DiffMergeToolType.Diff);
        txtDiffToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(diffTool, DiffMergeToolType.Diff);

        AutoCRLFType? autocrlf = ((ISettingsValueGetter)CurrentSettings).GetValue<AutoCRLFType>("core.autocrlf");

        globalAutoCrlfFalse.Checked = autocrlf is AutoCRLFType.@false;
        globalAutoCrlfInput.Checked = autocrlf is AutoCRLFType.input;
        globalAutoCrlfTrue.Checked = autocrlf is AutoCRLFType.@true;
        globalAutoCrlfNotSet.Checked = autocrlf is null;

        base.SettingsToPage();

        return;

        static GitConfigSettings? TryGetGitConfigSettings(SettingsSource settingsSource)
        {
            return settingsSource is SettingsSource<IPersistentConfigValueStore> persistentSettingsSource
                ? persistentSettingsSource.ConfigValueStore as GitConfigSettings
                : settingsSource is SettingsSource<IConfigValueStore> otherGenericSettingsSource
                    ? otherGenericSettingsSource.ConfigValueStore as GitConfigSettings
                    : null;
        }
    }

    /// <summary>
    /// silently does not save some settings if Git is not configured correctly
    /// (user notification is done elsewhere).
    /// </summary>
    protected override void PageToSettings()
    {
        Validates.NotNull(CurrentSettings);

        new GitEncodingSettingsSetter(CurrentSettings).FilesEncoding = (Encoding?)Global_FilesEncoding.SelectedItem;

        base.PageToSettings();

        if (!CheckSettingsLogic.CanFindGitCmd())
        {
            return;
        }

        CurrentSettings.SetValue(SettingKeyString.UserName, GlobalUserName.Text);
        CurrentSettings.SetValue(SettingKeyString.UserEmail, GlobalUserEmail.Text);
        CurrentSettings.SetValue("commit.template", txtCommitTemplatePath.Text);
        CurrentSettings.SetValue("core.editor", GlobalEditor.Text.ConvertPathToGitSetting());
        if (cbxCredentialHelper.Enabled)
        {
            CurrentSettings.SetValue(SettingKeyString.CredentialHelper, cbxCredentialHelper.Text);
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);

        // TODO: why use GUI???
        string diffTool = _NO_TRANSLATE_cboDiffTool.Text;
        if (!string.IsNullOrWhiteSpace(diffTool))
        {
            _diffMergeToolConfigurationManager.ConfigureDiffMergeTool(diffTool, DiffMergeToolType.Diff, txtDiffToolPath.Text, txtDiffToolCommand.Text);
        }
        else
        {
            _diffMergeToolConfigurationManager.UnsetCurrentTool(DiffMergeToolType.Diff);
        }

        // TODO: merge.guitool???
        string mergeTool = _NO_TRANSLATE_cboMergeTool.Text;
        if (!string.IsNullOrWhiteSpace(mergeTool))
        {
            _diffMergeToolConfigurationManager.ConfigureDiffMergeTool(mergeTool, DiffMergeToolType.Merge, txtMergeToolPath.Text, txtMergeToolCommand.Text);
        }
        else
        {
            _diffMergeToolConfigurationManager.UnsetCurrentTool(DiffMergeToolType.Merge);
        }

        AutoCRLFType? autoCRLFType =
            globalAutoCrlfFalse.Checked ? AutoCRLFType.@false
            : globalAutoCrlfInput.Checked ? AutoCRLFType.input
            : globalAutoCrlfTrue.Checked ? AutoCRLFType.@true
            : null;
        CurrentSettings.SetValue("core.autocrlf", autoCRLFType?.ToString());
    }

    private string BrowseDiffMergeTool(string toolName, string path, DiffMergeToolType toolType)
    {
        Validates.NotNull(_diffMergeToolConfigurationManager);

        DiffMergeToolConfiguration diffMergeToolConfig = default;
        int index = toolType == DiffMergeToolType.Diff ? _NO_TRANSLATE_cboDiffTool.SelectedIndex : _NO_TRANSLATE_cboMergeTool.SelectedIndex;
        if (index > -1)
        {
            diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, null);
        }

        string initialDirectory = _controller.GetInitialDirectory(path, diffMergeToolConfig.Path);

        string filter = !string.IsNullOrWhiteSpace(diffMergeToolConfig.ExeFileName)
            ? $"{toolName}|{diffMergeToolConfig.ExeFileName}"
            : "*.exe;*.cmd;*.bat|*.exe;*.cmd;*.bat";

        using OpenFileDialog dialog = new()
        {
            Filter = filter,
            InitialDirectory = initialDirectory,
            Title = _selectFile.Text
        };
        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : path;
    }

    private void SuggestDiffToolCommand()
    {
        string toolName = _NO_TRANSLATE_cboDiffTool.Text;
        if (string.IsNullOrWhiteSpace(toolName))
        {
            txtDiffToolCommand.Text = string.Empty;
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);
        DiffMergeToolConfiguration diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, txtDiffToolPath.Text);
        txtDiffToolCommand.Text = AdaptCommandIfWsl(diffMergeToolConfig.FullDiffCommand);
    }

    private void SuggestMergeToolCommand()
    {
        string toolName = _NO_TRANSLATE_cboMergeTool.Text;
        if (string.IsNullOrWhiteSpace(toolName))
        {
            txtMergeToolCommand.Text = string.Empty;
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);
        DiffMergeToolConfiguration diffMergeToolConfig = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, txtMergeToolPath.Text);
        txtMergeToolCommand.Text = AdaptCommandIfWsl(diffMergeToolConfig.FullMergeCommand);
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

    private void txtMergeToolPath_TextChanged(object sender, EventArgs e)
    {
        if (!txtMergeToolPath.Focused || sender != txtMergeToolPath)
        {
            return;
        }

        // we only want to suggest command for known built-in tool types
        string toolName = _NO_TRANSLATE_cboMergeTool.Text;
        if (!string.IsNullOrWhiteSpace(toolName) && RegisteredDiffMergeTools.All(DiffMergeToolType.Merge).Contains(toolName))
        {
            SuggestMergeToolCommand();
        }
    }

    private void txtDiffToolPath_TextChanged(object sender, EventArgs e)
    {
        if (!txtDiffToolPath.Focused || sender != txtDiffToolPath)
        {
            return;
        }

        // we only want to suggest command for known built-in tool types
        string toolName = _NO_TRANSLATE_cboDiffTool.Text;
        if (!string.IsNullOrWhiteSpace(toolName) && RegisteredDiffMergeTools.All(DiffMergeToolType.Diff).Contains(toolName))
        {
            SuggestDiffToolCommand();
        }
    }

    private void btnMergeToolBrowse_Click(object sender, EventArgs e)
    {
        txtMergeToolPath.Text = BrowseDiffMergeTool(_NO_TRANSLATE_cboMergeTool.Text, txtMergeToolPath.Text, DiffMergeToolType.Merge).ToPosixPath();
        SuggestMergeToolCommand();
    }

    private void cboDiffTool_TextChanged(object sender, EventArgs e)
    {
        string toolName = _NO_TRANSLATE_cboDiffTool.Text;

        txtDiffToolPath.Enabled =
            btnDiffToolBrowse.Enabled =
                txtDiffToolCommand.Enabled =
                    btnDiffToolCommandSuggest.Enabled = !string.IsNullOrEmpty(toolName);

        if (IsLoadingSettings)
        {
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);

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
        string toolName = _NO_TRANSLATE_cboMergeTool.Text;

        txtMergeToolPath.Enabled =
            btnMergeToolBrowse.Enabled =
                txtMergeToolCommand.Enabled =
                    btnMergeToolCommandSuggest.Enabled = !string.IsNullOrEmpty(toolName);

        if (IsLoadingSettings)
        {
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);

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
        txtDiffToolPath.Text = BrowseDiffMergeTool(_NO_TRANSLATE_cboDiffTool.Text, txtDiffToolPath.Text, DiffMergeToolType.Diff).ToPosixPath();
        SuggestDiffToolCommand();
    }

    private void btnCommitTemplateBrowse_Click(object sender, EventArgs e)
    {
        txtCommitTemplatePath.Text = CommonLogic.SelectFile(".", "*.txt (*.txt)|*.txt", txtCommitTemplatePath.Text);
    }

    private void ConfigureEncoding_Click(object sender, EventArgs e)
    {
        using FormAvailableEncodings encodingDlg = new();
        if (encodingDlg.ShowDialog() == DialogResult.OK)
        {
            Global_FilesEncoding.Items.Clear();
            CommonLogic.FillEncodings(Global_FilesEncoding);
        }
    }
}
