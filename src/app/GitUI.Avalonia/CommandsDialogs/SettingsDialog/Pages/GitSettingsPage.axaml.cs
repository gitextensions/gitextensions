using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class GitSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _envIsSetToString = new("{0} is set to: {1}");
    private readonly TranslationString _envIsNotSetString = new("{0} is not set.");
    private readonly TranslationString _portableGitCommand = new("Command used to run git");
    private readonly TranslationString _portableGlobalConfigPath = new(
        "By default, the global config file is located in the location stored in the environment variable $HOME.\n(This can be overridden by setting $GIT_CONFIG_GLOBAL.)");
    private readonly TranslationString _selectGitExecutable = new("Select Git executable");

    public GitSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public GitSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
        ConfigurePlatformControls();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(GitSettingsPage));

    public override void OnPageShown()
    {
        GitPath.Text = AppSettings.GitCommandValue;
        LinuxToolsDir.Text = AppSettings.LinuxToolsDir;
    }

    protected override void SettingsToPage()
    {
        EnvironmentConfiguration.SetEnvironmentVariables();
        const string globalConfigEnvironmentName = "GIT_CONFIG_GLOBAL";
        string displayedEnvironmentName = globalConfigEnvironmentName;
        string? environmentValue = EnvironmentConfiguration.GetEnvironmentVariable(globalConfigEnvironmentName);
        string additionalText = string.Empty;
        if (environmentValue is null)
        {
            additionalText = $"    ({string.Format(_envIsNotSetString.Text, FormatEnvironmentName(globalConfigEnvironmentName))})";
            environmentValue = EnvironmentConfiguration.GetHomeDir();
            displayedEnvironmentName = "HOME";
        }

        homeIsSetToLabel.Text = string.Format(
            _envIsSetToString.Text,
            FormatEnvironmentName(displayedEnvironmentName),
            environmentValue) + additionalText;
        GitPath.Text = AppSettings.GitCommandValue;
        LinuxToolsDir.Text = AppSettings.LinuxToolsDir;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.GitCommandValue = GitPath.Text ?? string.Empty;
        if (OperatingSystem.IsWindows())
        {
            AppSettings.LinuxToolsDir = LinuxToolsDir.Text ?? string.Empty;
        }

        base.PageToSettings();
    }

    private void WireEvents()
    {
        BrowseGitPath.Click += (_, _) => this.InvokeAndForget(BrowseGitPathAsync);
        BrowseLinuxToolsDir.Click += BrowseLinuxToolsDir_Click;
        GitPath.TextChanged += GitPath_TextChanged;
        LinuxToolsDir.TextChanged += LinuxToolsDir_TextChanged;
        downloadGitForWindows.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(
            "https://github.com/gitextensions/gitextensions/wiki/Application-Dependencies#git");
        ChangeHomeButton.Click += ChangeHomeButton_Click;
    }

    private void ConfigurePlatformControls()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        label50.IsVisible = false;
        lblGitCommand.IsVisible = false;
        _NO_TRANSLATE_lblPortableGitCommand.Text = _portableGitCommand.Text;
        _NO_TRANSLATE_lblPortableGitCommand.IsVisible = true;
        lblShPath.IsVisible = false;
        LinuxToolsDir.IsVisible = false;
        BrowseLinuxToolsDir.IsVisible = false;
        downloadGitForWindows.IsVisible = false;
        lblGlobalConfigPath.IsVisible = false;
        _NO_TRANSLATE_lblPortableGlobalConfigPath.Text = _portableGlobalConfigPath.Text;
        _NO_TRANSLATE_lblPortableGlobalConfigPath.IsVisible = true;
    }

    private async Task BrowseGitPathAsync()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = _selectGitExecutable.Text,
            FileTypeFilter =
            [
                new FilePickerFileType("Git")
                {
                    Patterns = OperatingSystem.IsWindows() ? ["git.exe", "git.cmd"] : ["git"],
                },
                FilePickerFileTypes.All,
            ],
        };
        string? currentDirectory = Path.GetDirectoryName(GitPath.Text);
        if (!string.IsNullOrEmpty(currentDirectory))
        {
            options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(currentDirectory);
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            GitPath.Text = path;
        }
    }

    private void BrowseLinuxToolsDir_Click(object? sender, EventArgs e)
    {
        CheckSettingsLogic.SolveLinuxToolsDir(LinuxToolsDir.Text?.Trim());
        WinFormsShims.IWin32Window owner = (TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window)!;
        string? userSelectedPath = OsShellUtil.PickFolder(owner, AppSettings.LinuxToolsDir);
        if (userSelectedPath is not null)
        {
            LinuxToolsDir.Text = userSelectedPath;
        }
    }

    private void GitPath_TextChanged(object? sender, EventArgs e)
    {
        if (!IsLoadingSettings)
        {
            CheckSettingsLogic.SolveGitCommand(GitPath.Text?.Trim());
        }
    }

    private void LinuxToolsDir_TextChanged(object? sender, EventArgs e)
    {
        if (!IsLoadingSettings && OperatingSystem.IsWindows())
        {
            CheckSettingsLogic.SolveLinuxToolsDir(LinuxToolsDir.Text?.Trim());
        }
    }

    private void ChangeHomeButton_Click(object? sender, EventArgs e)
    {
        PageHost.SaveAll();
        using FormFixHome form = new();
        form.ShowDialog((TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window)!);
        PageHost.LoadAll();
    }

    private static string FormatEnvironmentName(string name)
        => OperatingSystem.IsWindows() ? $"%{name}%" : $"${name}";

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(GitSettingsPage page)
    {
        public TextBox GitPath => page.GitPath;

        public TextBox LinuxToolsDir => page.LinuxToolsDir;

        public Control LinuxToolsLabel => page.lblShPath;

        public Control DownloadGit => page.downloadGitForWindows;

        public TextBlock EnvironmentStatus => page.homeIsSetToLabel;
    }
}
