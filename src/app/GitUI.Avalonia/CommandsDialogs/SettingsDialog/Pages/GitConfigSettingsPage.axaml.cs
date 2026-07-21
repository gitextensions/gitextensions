using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Config;
using GitCommands.DiffMergeTools;
using GitCommands.Settings;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Settings;
using GitUI.Properties;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigSettingsPage : GitConfigBaseSettingsPage
{
    private readonly TranslationString _selectFile = new("Select file");
    private readonly GitConfigSettingsPageController _controller;
    private DiffMergeToolConfigurationManager? _diffMergeToolConfigurationManager;

    [GeneratedRegex(@"\$(?:LOCAL|REMOTE|BASE|MERGED)", RegexOptions.ExplicitCapture)]
    private static partial Regex WslRebaseRegex { get; }

    public GitConfigSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public GitConfigSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        _controller = new GitConfigSettingsPageController();
        pictureBox1.Source = Images.StatusBadgeError;

        _NO_TRANSLATE_cboMergeTool.ItemsSource = RegisteredDiffMergeTools.All(DiffMergeToolType.Merge).ToArray();
        _NO_TRANSLATE_cboDiffTool.ItemsSource = RegisteredDiffMergeTools.All(DiffMergeToolType.Diff).ToArray();
        ConfigureToolControls(_NO_TRANSLATE_cboMergeTool, txtMergeToolPath, btnMergeToolBrowse, txtMergeToolCommand, btnMergeToolCommandSuggest);
        ConfigureToolControls(_NO_TRANSLATE_cboDiffTool, txtDiffToolPath, btnDiffToolBrowse, txtDiffToolCommand, btnDiffToolCommandSuggest);

        WireEvents();
        InitializeComplete();
    }

    protected override void Init(ISettingsPageHost pageHost)
    {
        base.Init(pageHost);
        if (CurrentSettings is null)
        {
            CommonLogic.FillEncodings(Global_FilesEncoding);
            return;
        }

        _diffMergeToolConfigurationManager = new DiffMergeToolConfigurationManager(() => CurrentSettings);

        CommonLogic.FillEncodings(Global_FilesEncoding);
        cbxCredentialHelper.ItemsSource = GetCredentialHelpers();
        GlobalEditor.ItemsSource = EditorHelper.GetEditors()
            .Select(AdaptCommandIfWsl)
            .WhereNotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(GitConfigSettingsPage));

    public override void OnPageShown()
    {
        if (CurrentSettings is null)
        {
            return;
        }

        bool canFindGitCmd = CheckSettingsLogic.CanFindGitCmd();
        GlobalUserName.IsEnabled = canFindGitCmd;
        GlobalUserEmail.IsEnabled = canFindGitCmd;
        GlobalEditor.IsEnabled = canFindGitCmd;
        txtCommitTemplatePath.IsEnabled = canFindGitCmd;
        _NO_TRANSLATE_cboMergeTool.IsEnabled = canFindGitCmd;
        _NO_TRANSLATE_cboDiffTool.IsEnabled = canFindGitCmd;
        InvalidGitPathGlobal.IsVisible = !canFindGitCmd;

        ConfigureToolControls(_NO_TRANSLATE_cboMergeTool, txtMergeToolPath, btnMergeToolBrowse, txtMergeToolCommand, btnMergeToolCommandSuggest);
        ConfigureToolControls(_NO_TRANSLATE_cboDiffTool, txtDiffToolPath, btnDiffToolBrowse, txtDiffToolCommand, btnDiffToolCommandSuggest);
    }

    protected override void SettingsToPage()
    {
        if (CurrentSettings is null)
        {
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);

        string? mergeTool = _diffMergeToolConfigurationManager.ConfiguredMergeTool;
        string? diffTool = _diffMergeToolConfigurationManager.ConfiguredDiffTool;

        Global_FilesEncoding.SelectedItem = new GitEncodingSettingsGetter(CurrentSettings).FilesEncoding;
        GlobalUserName.Text = CurrentSettings.GetValue(SettingKeyString.UserName);
        GlobalUserEmail.Text = CurrentSettings.GetValue(SettingKeyString.UserEmail);
        GlobalEditor.Text = CurrentSettings.GetValue("core.editor");
        txtCommitTemplatePath.Text = CurrentSettings.GetValue("commit.template");

        GitConfigSettings? gitConfigSettings = TryGetGitConfigSettings(CurrentSettings);
        bool showCredentialHelper = gitConfigSettings is not null;
        lblCredentialHelper.IsVisible = showCredentialHelper;
        cbxCredentialHelper.IsVisible = showCredentialHelper;
        if (gitConfigSettings is not null)
        {
            IReadOnlyList<string> values = gitConfigSettings.GetValues(SettingKeyString.CredentialHelper);
            cbxCredentialHelper.IsEnabled = values.Count <= 1;
            cbxCredentialHelper.Text = string.Join(", ", values);
        }

        _NO_TRANSLATE_cboMergeTool.Text = mergeTool;
        txtMergeToolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(mergeTool, DiffMergeToolType.Merge);
        txtMergeToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(mergeTool, DiffMergeToolType.Merge);

        _NO_TRANSLATE_cboDiffTool.Text = diffTool;
        txtDiffToolPath.Text = _diffMergeToolConfigurationManager.GetToolPath(diffTool, DiffMergeToolType.Diff);
        txtDiffToolCommand.Text = _diffMergeToolConfigurationManager.GetToolCommand(diffTool, DiffMergeToolType.Diff);

        AutoCRLFType? autocrlf = ((ISettingsValueGetter)CurrentSettings).GetValue<AutoCRLFType>("core.autocrlf");
        globalAutoCrlfFalse.IsChecked = autocrlf is AutoCRLFType.@false;
        globalAutoCrlfInput.IsChecked = autocrlf is AutoCRLFType.input;
        globalAutoCrlfTrue.IsChecked = autocrlf is AutoCRLFType.@true;
        globalAutoCrlfNotSet.IsChecked = autocrlf is null;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        if (CurrentSettings is null)
        {
            return;
        }

        new GitEncodingSettingsSetter(CurrentSettings).FilesEncoding = Global_FilesEncoding.SelectedItem as Encoding;

        base.PageToSettings();
        if (!CheckSettingsLogic.CanFindGitCmd())
        {
            return;
        }

        CurrentSettings.SetValue(SettingKeyString.UserName, GlobalUserName.Text);
        CurrentSettings.SetValue(SettingKeyString.UserEmail, GlobalUserEmail.Text);
        CurrentSettings.SetValue("commit.template", txtCommitTemplatePath.Text);
        CurrentSettings.SetValue("core.editor", GlobalEditor.Text.ConvertPathToGitSetting());
        if (cbxCredentialHelper.IsEnabled)
        {
            CurrentSettings.SetValue(SettingKeyString.CredentialHelper, cbxCredentialHelper.Text);
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);
        SaveTool(DiffMergeToolType.Diff, _NO_TRANSLATE_cboDiffTool.Text, txtDiffToolPath.Text, txtDiffToolCommand.Text);
        SaveTool(DiffMergeToolType.Merge, _NO_TRANSLATE_cboMergeTool.Text, txtMergeToolPath.Text, txtMergeToolCommand.Text);

        AutoCRLFType? autoCrlf = globalAutoCrlfFalse.IsChecked == true ? AutoCRLFType.@false
            : globalAutoCrlfInput.IsChecked == true ? AutoCRLFType.input
            : globalAutoCrlfTrue.IsChecked == true ? AutoCRLFType.@true
            : null;
        CurrentSettings.SetValue("core.autocrlf", autoCrlf?.ToString());
    }

    private void WireEvents()
    {
        btnMergeToolCommandSuggest.Click += (_, _) => SuggestMergeToolCommand();
        btnDiffToolCommandSuggest.Click += (_, _) => SuggestDiffToolCommand();
        btnMergeToolBrowse.Click += (_, _) => this.InvokeAndForget(() => BrowseToolAsync(DiffMergeToolType.Merge));
        btnDiffToolBrowse.Click += (_, _) => this.InvokeAndForget(() => BrowseToolAsync(DiffMergeToolType.Diff));
        btnCommitTemplateBrowse.Click += (_, _) => this.InvokeAndForget(BrowseCommitTemplateAsync);
        ConfigureEncoding.Click += ConfigureEncoding_Click;
        txtMergeToolPath.LostFocus += (_, _) => txtMergeToolPath.Text = txtMergeToolPath.Text.ToPosixPath();
        txtDiffToolPath.LostFocus += (_, _) => txtDiffToolPath.Text = txtDiffToolPath.Text.ToPosixPath();
        txtMergeToolPath.TextChanged += txtMergeToolPath_TextChanged;
        txtDiffToolPath.TextChanged += txtDiffToolPath_TextChanged;
        _NO_TRANSLATE_cboMergeTool.PropertyChanged += cboMergeTool_PropertyChanged;
        _NO_TRANSLATE_cboDiffTool.PropertyChanged += cboDiffTool_PropertyChanged;
    }

    private string? AdaptCommandIfWsl(string? command)
    {
        if (string.IsNullOrEmpty(command) || !PathUtil.IsWslPath(Module?.WorkingDir))
        {
            return command;
        }

        int colonIndex = command.IndexOf(':');
        if (colonIndex == (command[0] == '"' ? 2 : 1))
        {
            int windowsDriveIndex = colonIndex - 1;
            command = $"{command[..windowsDriveIndex]}/mnt/{char.ToLower(command[windowsDriveIndex])}{command[(colonIndex + 1)..]}";
        }

        return WslRebaseRegex.Replace(command, @"$(wslpath -aw $&)").ToPosixPath();
    }

    private IReadOnlyList<string> GetCredentialHelpers()
    {
        const string gitCredentialHelperPrefix = "git-credential-";
        List<string> helpers = [];
        if (OperatingSystem.IsWindows())
        {
            helpers.AddRange(PathUtil.IsWslPath(Module?.WorkingDir)
                ? FindGitCredentialHelpers().Select(path => path.ToWslPath().Replace(" ", @"\ "))
                : FindGitCredentialHelpers().Select(GetCredentialHelperName));
        }

        helpers.AddRange(["oauth", "store", "cache"]);
        return helpers.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        static IEnumerable<string> FindGitCredentialHelpers()
        {
            try
            {
                string? gitDir = Path.GetDirectoryName(AppSettings.GitCommand);
                if (gitDir?.EndsWith("bin", StringComparison.OrdinalIgnoreCase) is true)
                {
                    gitDir = Path.GetDirectoryName(gitDir);
                }

                return !Directory.Exists(gitDir)
                    ? []
                    : Directory.GetFiles(gitDir, $"{gitCredentialHelperPrefix}*.exe", SearchOption.AllDirectories)
                        .Where(path => !path.Contains("git-credential-helper-selector", StringComparison.OrdinalIgnoreCase));
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
            return name.StartsWith(gitCredentialHelperPrefix, StringComparison.OrdinalIgnoreCase)
                ? name[gitCredentialHelperPrefix.Length..]
                : path;
        }
    }

    private void SaveTool(DiffMergeToolType toolType, string? toolName, string? path, string? command)
    {
        Validates.NotNull(_diffMergeToolConfigurationManager);
        if (string.IsNullOrWhiteSpace(toolName))
        {
            _diffMergeToolConfigurationManager.UnsetCurrentTool(toolType);
            return;
        }

        _diffMergeToolConfigurationManager.ConfigureDiffMergeTool(toolName, toolType, path ?? string.Empty, command ?? string.Empty);
    }

    private void SuggestDiffToolCommand()
    {
        string? toolName = _NO_TRANSLATE_cboDiffTool.Text;
        if (string.IsNullOrWhiteSpace(toolName))
        {
            txtDiffToolCommand.Text = string.Empty;
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);
        DiffMergeToolConfiguration config = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, txtDiffToolPath.Text);
        txtDiffToolCommand.Text = AdaptCommandIfWsl(config.FullDiffCommand);
    }

    private void SuggestMergeToolCommand()
    {
        string? toolName = _NO_TRANSLATE_cboMergeTool.Text;
        if (string.IsNullOrWhiteSpace(toolName))
        {
            txtMergeToolCommand.Text = string.Empty;
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);
        DiffMergeToolConfiguration config = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, txtMergeToolPath.Text);
        txtMergeToolCommand.Text = AdaptCommandIfWsl(config.FullMergeCommand);
    }

    private async Task BrowseToolAsync(DiffMergeToolType toolType)
    {
        ComboBox toolCombo = toolType == DiffMergeToolType.Diff ? _NO_TRANSLATE_cboDiffTool : _NO_TRANSLATE_cboMergeTool;
        TextBox pathTextBox = toolType == DiffMergeToolType.Diff ? txtDiffToolPath : txtMergeToolPath;
        string? toolName = toolCombo.Text;
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);
        DiffMergeToolConfiguration config = _diffMergeToolConfigurationManager.LoadDiffMergeToolConfig(toolName, null);
        string initialDirectory = _controller.GetInitialDirectory(pathTextBox.Text ?? string.Empty, config.Path);
        string? selectedPath = await SelectFileAsync(initialDirectory, config.ExeFileName);
        if (selectedPath is null)
        {
            return;
        }

        pathTextBox.Text = selectedPath.ToPosixPath();
        if (toolType == DiffMergeToolType.Diff)
        {
            SuggestDiffToolCommand();
        }
        else
        {
            SuggestMergeToolCommand();
        }
    }

    private async Task BrowseCommitTemplateAsync()
    {
        string initialDirectory = Path.GetDirectoryName(txtCommitTemplatePath.Text) ?? Module?.WorkingDir ?? ".";
        string? selectedPath = await SelectFileAsync(initialDirectory, "*.txt");
        if (selectedPath is not null)
        {
            txtCommitTemplatePath.Text = selectedPath;
        }
    }

    private async Task<string?> SelectFileAsync(string initialDirectory, string pattern)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return null;
        }

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = _selectFile.Text,
            FileTypeFilter =
            [
                new FilePickerFileType(pattern) { Patterns = [pattern] },
                FilePickerFileTypes.All,
            ],
        };
        if (Directory.Exists(initialDirectory))
        {
            options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        return files.FirstOrDefault()?.TryGetLocalPath();
    }

    private void ConfigureEncoding_Click(object? sender, EventArgs e)
    {
        using FormAvailableEncodings dialog = new();
        if (dialog.ShowDialog(TopLevel.GetTopLevel(this) as GitExtensions.Shims.WinForms.IWin32Window)
            == GitExtensions.Shims.WinForms.DialogResult.OK)
        {
            CommonLogic.FillEncodings(Global_FilesEncoding);
        }
    }

    private void txtMergeToolPath_TextChanged(object? sender, EventArgs e)
    {
        if (!IsLoadingSettings && txtMergeToolPath.IsFocused && IsRegisteredTool(_NO_TRANSLATE_cboMergeTool.Text, DiffMergeToolType.Merge))
        {
            SuggestMergeToolCommand();
        }
    }

    private void txtDiffToolPath_TextChanged(object? sender, EventArgs e)
    {
        if (!IsLoadingSettings && txtDiffToolPath.IsFocused && IsRegisteredTool(_NO_TRANSLATE_cboDiffTool.Text, DiffMergeToolType.Diff))
        {
            SuggestDiffToolCommand();
        }
    }

    private void cboMergeTool_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ComboBox.TextProperty)
        {
            ToolChanged(DiffMergeToolType.Merge);
        }
    }

    private void cboDiffTool_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ComboBox.TextProperty)
        {
            ToolChanged(DiffMergeToolType.Diff);
        }
    }

    private void ToolChanged(DiffMergeToolType toolType)
    {
        ComboBox combo = toolType == DiffMergeToolType.Diff ? _NO_TRANSLATE_cboDiffTool : _NO_TRANSLATE_cboMergeTool;
        TextBox path = toolType == DiffMergeToolType.Diff ? txtDiffToolPath : txtMergeToolPath;
        TextBox command = toolType == DiffMergeToolType.Diff ? txtDiffToolCommand : txtMergeToolCommand;
        Button browse = toolType == DiffMergeToolType.Diff ? btnDiffToolBrowse : btnMergeToolBrowse;
        Button suggest = toolType == DiffMergeToolType.Diff ? btnDiffToolCommandSuggest : btnMergeToolCommandSuggest;
        ConfigureToolControls(combo, path, browse, command, suggest);

        if (IsLoadingSettings || string.IsNullOrWhiteSpace(combo.Text))
        {
            if (!IsLoadingSettings)
            {
                path.Text = string.Empty;
                command.Text = string.Empty;
            }

            return;
        }

        Validates.NotNull(_diffMergeToolConfigurationManager);
        string toolPath = _diffMergeToolConfigurationManager.GetToolPath(combo.Text, toolType);
        if (string.IsNullOrWhiteSpace(toolPath))
        {
            path.Text = string.Empty;
            command.Text = string.Empty;
            return;
        }

        path.Text = toolPath;
        command.Text = _diffMergeToolConfigurationManager.GetToolCommand(combo.Text, toolType);
    }

    private static void ConfigureToolControls(ComboBox combo, TextBox path, Button browse, TextBox command, Button suggest)
    {
        bool enabled = combo.IsEnabled && !string.IsNullOrWhiteSpace(combo.Text);
        path.IsEnabled = enabled;
        browse.IsEnabled = enabled;
        command.IsEnabled = enabled;
        suggest.IsEnabled = enabled;
    }

    private static bool IsRegisteredTool(string? toolName, DiffMergeToolType toolType)
        => !string.IsNullOrWhiteSpace(toolName) && RegisteredDiffMergeTools.All(toolType).Contains(toolName);

    private static GitConfigSettings? TryGetGitConfigSettings(SettingsSource settingsSource)
        => settingsSource is SettingsSource<IPersistentConfigValueStore> persistentSettingsSource
            ? persistentSettingsSource.ConfigValueStore as GitConfigSettings
            : settingsSource is SettingsSource<IConfigValueStore> otherGenericSettingsSource
                ? otherGenericSettingsSource.ConfigValueStore as GitConfigSettings
                : null;

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(GitConfigSettingsPage page)
    {
        public TextBox UserName => page.GlobalUserName;

        public TextBox UserEmail => page.GlobalUserEmail;

        public ComboBox Editor => page.GlobalEditor;

        public ComboBox CredentialHelper => page.cbxCredentialHelper;

        public ComboBox MergeTool => page._NO_TRANSLATE_cboMergeTool;

        public ComboBox DiffTool => page._NO_TRANSLATE_cboDiffTool;

        public IReadOnlyList<RadioButton> AutoCrlfOptions =>
            [page.globalAutoCrlfTrue, page.globalAutoCrlfInput, page.globalAutoCrlfFalse, page.globalAutoCrlfNotSet];
    }
}
