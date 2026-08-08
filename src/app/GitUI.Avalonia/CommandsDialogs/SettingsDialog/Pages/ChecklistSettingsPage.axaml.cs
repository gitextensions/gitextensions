using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitCommands.Config;
using GitCommands.DiffMergeTools;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.SettingsDialog.ShellExtension;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class ChecklistSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _wrongGitVersion =
        new("Git found but version {0} is not supported. Upgrade to version {1} or later");
    private readonly TranslationString _notRecommendedGitVersion =
        new("Git found but version {0} is older than recommended. Upgrade to version {1} or later");
    private readonly TranslationString _gitVersionFound = new("Git {0} is found on your computer.");
    private readonly TranslationString _sshClientNotFound = new("SSH client not found: {0}.");
    private readonly TranslationString _otherSshClient = new("Other SSH client configured: {0}.");
    private readonly TranslationString _linuxToolsSshNotFound =
        new("Linux tools (sh) not found. To solve this problem you can set the correct path in settings.");
    private readonly TranslationString _solveGitCommandFailedCaption = new("Locate git");
    private readonly TranslationString _gitCanBeRun = new("Git can be run using: {0}");
    private readonly TranslationString _gitCanBeRunCaption = new("Locate git");
    private readonly TranslationString _solveGitCommandFailed =
        new("The command to run git could not be determined automatically." + Environment.NewLine +
            "Please make sure that Git for Windows is installed or set the correct command manually.");
    private readonly TranslationString _shellExtRegistered = new("Shell extensions registered properly.");
    private readonly TranslationString _shellExtNoInstalled =
        new("Shell extensions are not installed. Run the installer to install the shell extensions.");
    private readonly TranslationString _shellExtNeedsToBeRegistered =
        new("{0} needs to be registered in order to use the shell extensions.");
    private readonly TranslationString _registryKeyGitExtensionsMissing =
        new("Registry entry missing [Software\\GitExtensions\\InstallDir].");
    private readonly TranslationString _registryKeyGitExtensionsFaulty =
        new("Invalid installation directory stored in [Software\\GitExtensions\\InstallDir].");
    private readonly TranslationString _registryKeyGitExtensionsCorrect =
        new("Git Extensions is properly registered.");
    private readonly TranslationString _plinkputtyGenpageantNotFound =
        new("PuTTY is configured as SSH client but cannot find plink.exe, puttygen.exe or pageant.exe.");
    private readonly TranslationString _puttyConfigured = new("SSH client PuTTY is configured properly.");
    private readonly TranslationString _opensshUsed =
        new("Default SSH client, OpenSSH, will be used. (commandline window will appear on pull, push and clone operations)");
    private readonly TranslationString _languageConfigured = new("The configured language is {0}.");
    private readonly TranslationString _noLanguageConfigured =
        new("There is no language configured for Git Extensions.");
    private readonly TranslationString _noEmailSet =
        new("You need to configure a username and an email address.");
    private readonly TranslationString _emailSet = new("A username and an email address are configured.");
    private readonly TranslationString _mergeToolXConfiguredNeedsCmd =
        new("{0} is configured as mergetool, this is a custom mergetool and needs a custom cmd to be configured.");
    private readonly TranslationString _customMergeToolXConfigured =
        new("There is a custom mergetool configured: {0}");
    private readonly TranslationString _mergeToolXConfigured =
        new("There is a mergetool configured: {0}");
    private readonly TranslationString _linuxToolsSshFound =
        new("Linux tools (sh) found on your computer.");
    private readonly TranslationString _gitNotFound =
        new("Git not found. To solve this problem you can set the correct path in settings.");
    private readonly TranslationString _adviceDiffToolConfiguration =
        new("You should configure a diff tool to show file diff in external program.");
    private readonly TranslationString _diffToolXConfigured =
        new("There is a difftool configured: {0}");
    private readonly TranslationString _configureMergeTool =
        new("You need to configure merge tool in order to solve merge conflicts.");
    private readonly TranslationString _noDiffToolConfiguredCaption = new("Difftool");
    private readonly TranslationString _linuxToolsShNotFound =
        new("The path to linux tools (sh) could not be found automatically." + Environment.NewLine +
            "Please make sure there are linux tools installed (through Git for Windows or cygwin) or set the correct path manually.");
    private readonly TranslationString _linuxToolsShNotFoundCaption = new("Locate linux tools");
    private readonly TranslationString _shCanBeRun = new("Command sh can be run using: {0}sh");
    private readonly TranslationString _shCanBeRunCaption = new("Locate linux tools");
    private readonly TranslationString _gcmDetectedCaption =
        new("Obsolete git-credential-winstore.exe detected");
    private readonly TranslationString _puttyFoundAuto =
        new("All paths needed for PuTTY could be automatically found and are set.");

    private const string _putty = "PuTTY";
    private DiffMergeToolConfigurationManager? _diffMergeToolConfigurationManager;

    public ChecklistSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ChecklistSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        Text = "Checklist";

        GitFound.Click += GitFound_Click;
        GitFound_Fix.Click += GitFound_Click;
        UserNameSet.Click += UserNameSet_Click;
        UserNameSet_Fix.Click += UserNameSet_Click;
        MergeTool.Click += MergeToolFix_Click;
        MergeTool_Fix.Click += MergeToolFix_Click;
        DiffTool.Click += DiffToolFix_Click;
        DiffTool_Fix.Click += DiffToolFix_Click;
        ShellExtensionsRegistered.Click += ShellExtensionsRegistered_Click;
        ShellExtensionsRegistered_Fix.Click += ShellExtensionsRegistered_Click;
        GitBinFound.Click += GitBinFound_Click;
        GitBinFound_Fix.Click += GitBinFound_Click;
        GitExtensionsInstall.Click += GitExtensionsInstall_Click;
        GitExtensionsInstall_Fix.Click += GitExtensionsInstall_Click;
        SshConfig.Click += SshConfig_Click;
        SshConfig_Fix.Click += SshConfig_Click;
        translationConfig.Click += translationConfig_Click;
        translationConfig_Fix.Click += translationConfig_Click;
        GcmDetectedFix.Click += GcmDetectedFix_Click;
        Rescan.Click += SaveAndRescan_Click;
        CheckAtStartup.Click += CheckAtStartup_CheckedChanged;
        InitializeComplete();
    }

    public SshSettingsPage? SshSettingsPage { get; set; }

    public override bool IsInstantSavePage => true;

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ChecklistSettingsPage));

    public override void OnPageShown() => CheckSettings();

    public bool CheckSettings()
    {
        _diffMergeToolConfigurationManager = new DiffMergeToolConfigurationManager(
            () => CheckSettingsLogic.CommonLogic.GitConfigSettingsSet.EffectiveSettings);
        ChecklistResult result = Evaluate(CommonLogic);
        Render(GitFound, GitFound_Fix, isVisible: true, result.GitStatus, result.GitMessage);
        Render(UserNameSet, UserNameSet_Fix, isVisible: true, result.Identity, result.IdentityMessage);
        Render(MergeTool, MergeTool_Fix, isVisible: true, result.MergeTool, result.MergeToolMessage);
        Render(DiffTool, DiffTool_Fix, isVisible: true, result.DiffTool, result.DiffToolMessage);
        Render(
            ShellExtensionsRegistered,
            ShellExtensionsRegistered_Fix,
            result.WindowsChecksVisible,
            result.ShellExtensions,
            result.ShellExtensionsMessage);
        Render(GitBinFound, GitBinFound_Fix, result.WindowsChecksVisible, result.GitBin, result.GitBinMessage);
        Render(
            GitExtensionsInstall,
            GitExtensionsInstall_Fix,
            result.WindowsChecksVisible,
            result.InstallRegistration,
            result.InstallRegistrationMessage);
        Render(SshConfig, SshConfig_Fix, result.WindowsChecksVisible, result.Ssh, result.SshMessage);
        Render(
            translationConfig,
            translationConfig_Fix,
            isVisible: true,
            result.Translation,
            result.TranslationMessage);
        Render(
            GcmDetected,
            GcmDetectedFix,
            result.ObsoleteCredentialHelperVisible,
            result.ObsoleteCredentialHelper,
            result.ObsoleteCredentialHelperMessage);

        if (result.IsValid && AppSettings.CheckSettings)
        {
            AppSettings.CheckSettings = false;
        }

        CheckAtStartup.IsChecked = AppSettings.CheckSettings;
        return result.IsValid;
    }

    internal ChecklistResult Evaluate(CommonLogic commonLogic)
    {
        CheckState gitStatus = CheckState.Invalid;
        string gitMessage = _gitNotFound.Text;
        bool gitAvailable = TryCheck(() =>
        {
            if (!CheckSettingsLogic.CanFindGitCmd())
            {
                return false;
            }

            IGitVersion nativeGitVersion = GitVersion.Current;
            IGitVersion usedGitVersion = commonLogic.Module.IsValidGitWorkingDir()
                ? GitVersion.CurrentVersion(commonLogic.Module.GitExecutable)
                : nativeGitVersion;
            string displayedVersion = nativeGitVersion == usedGitVersion
                ? $"{nativeGitVersion}"
                : $"{nativeGitVersion} / WSL {usedGitVersion}";
            if (usedGitVersion < GitVersion.LastSupportedVersion)
            {
                gitMessage = string.Format(
                    _wrongGitVersion.Text,
                    displayedVersion,
                    GitVersion.LastRecommendedVersion);
                return false;
            }

            if (usedGitVersion < GitVersion.LastRecommendedVersion)
            {
                gitStatus = CheckState.NotRecommended;
                gitMessage = string.Format(
                    _notRecommendedGitVersion.Text,
                    displayedVersion,
                    GitVersion.LastRecommendedVersion);
                return false;
            }

            gitStatus = CheckState.Valid;
            gitMessage = string.Format(_gitVersionFound.Text, displayedVersion);
            return true;
        });
        if (!gitAvailable && gitStatus != CheckState.NotRecommended)
        {
            gitStatus = CheckState.Invalid;
        }

        bool identity = TryCheck(() =>
            !string.IsNullOrEmpty(commonLogic.GitConfigSettingsSet.GlobalSettings.GetValue(SettingKeyString.UserName))
            && !string.IsNullOrEmpty(commonLogic.GitConfigSettingsSet.GlobalSettings.GetValue(SettingKeyString.UserEmail)));
        DiffMergeToolConfigurationManager tools = _diffMergeToolConfigurationManager ??=
            new DiffMergeToolConfigurationManager(
                () => commonLogic.GitConfigSettingsSet.EffectiveSettings);
        string? mergeTool = null;
        bool merge = TryCheck(() =>
        {
            mergeTool = tools.ConfiguredMergeTool;
            return !string.IsNullOrWhiteSpace(mergeTool)
                   && !string.IsNullOrWhiteSpace(tools.GetToolCommand(mergeTool, DiffMergeToolType.Merge));
        });
        string? diffTool = null;
        bool diff = TryCheck(() =>
        {
            diffTool = tools.ConfiguredDiffTool;
            return !string.IsNullOrWhiteSpace(diffTool)
                   && !string.IsNullOrWhiteSpace(tools.GetToolCommand(diffTool, DiffMergeToolType.Diff));
        });
        bool editor = TryCheck(() => !string.IsNullOrEmpty(commonLogic.GetGlobalEditor()));
        bool translation = !string.IsNullOrEmpty(AppSettings.Translation);

        bool windows = OperatingSystem.IsWindows();
        (bool Install, string Message) install = windows
            ? TryEvaluate(
                CheckInstallRegistration,
                (false, _registryKeyGitExtensionsFaulty.Text))
            : (true, string.Empty);
        (bool Shell, string Message) shell = windows
            ? TryEvaluate(
                CheckShellExtensions,
                (false, string.Format(
                    _shellExtNeedsToBeRegistered.Text,
                    ShellExtensionManager.GitExtensionsShellEx32Name)))
            : (true, string.Empty);
        bool gitBin = !windows || TryCheck(() =>
            File.Exists(Path.Join(AppSettings.LinuxToolsDir, "sh.exe"))
            || File.Exists(Path.Join(AppSettings.LinuxToolsDir, "sh"))
            || CheckSettingsLogic.CheckIfFileIsInPath("sh.exe")
            || CheckSettingsLogic.CheckIfFileIsInPath("sh"));
        (bool Ssh, string Message) ssh = windows
            ? TryEvaluate(CheckSsh, (false, _plinkputtyGenpageantNotFound.Text))
            : (true, string.Empty);
        string credentialHelper = string.Empty;
        if (windows)
        {
            TryCheck(() =>
            {
                credentialHelper =
                    commonLogic.GitConfigSettingsSet.GlobalSettings.GetValue(SettingKeyString.CredentialHelper)
                    ?? string.Empty;
                return true;
            });
        }

        bool obsoleteCredentialHelperVisible =
            credentialHelper.Contains("git-credential-winstore.exe", StringComparison.OrdinalIgnoreCase);

        return new ChecklistResult(
            gitStatus,
            identity,
            merge,
            diff,
            editor,
            translation,
            windows,
            install.Install,
            shell.Shell,
            gitBin,
            ssh.Ssh,
            !obsoleteCredentialHelperVisible,
            obsoleteCredentialHelperVisible,
            gitMessage,
            identity ? _emailSet.Text : _noEmailSet.Text,
            merge
                ? string.Format(_mergeToolXConfigured.Text, mergeTool)
                : string.IsNullOrWhiteSpace(mergeTool)
                    ? _configureMergeTool.Text
                    : string.Format(_mergeToolXConfiguredNeedsCmd.Text, mergeTool),
            diff
                ? string.Format(_diffToolXConfigured.Text, diffTool)
                : _adviceDiffToolConfiguration.Text,
            translation
                ? string.Format(_languageConfigured.Text, AppSettings.Translation)
                : _noLanguageConfigured.Text,
            install.Message,
            shell.Message,
            gitBin ? _linuxToolsSshFound.Text : _linuxToolsSshNotFound.Text,
            ssh.Message,
            _gcmDetectedCaption.Text);

        (bool Install, string Message) CheckInstallRegistration()
        {
            string? installDirectory = AppSettings.GetInstallDir();
            if (string.IsNullOrEmpty(installDirectory))
            {
                return (false, _registryKeyGitExtensionsMissing.Text);
            }

            if (installDirectory.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                || !Directory.Exists(installDirectory)
                || (!Debugger.IsAttached
                    && !string.Equals(
                        Path.TrimEndingDirectorySeparator(installDirectory),
                        Path.TrimEndingDirectorySeparator(AppSettings.GetGitExtensionsDirectory()!),
                        StringComparison.OrdinalIgnoreCase)))
            {
                return (false, _registryKeyGitExtensionsFaulty.Text);
            }

            return (true, _registryKeyGitExtensionsCorrect.Text);
        }

        (bool Shell, string Message) CheckShellExtensions()
        {
            if (!ShellExtensionManager.FilesExist())
            {
                return (true, _shellExtNoInstalled.Text);
            }

            return ShellExtensionManager.IsRegistered()
                ? (true, _shellExtRegistered.Text)
                : (false, string.Format(
                    _shellExtNeedsToBeRegistered.Text,
                    ShellExtensionManager.GitExtensionsShellEx32Name));
        }

        (bool Ssh, string Message) CheckSsh()
        {
            if (GitSshHelpers.IsPlink)
            {
                bool valid = File.Exists(AppSettings.Plink)
                    && File.Exists(AppSettings.Puttygen)
                    && File.Exists(AppSettings.Pageant);
                return (valid, valid ? _puttyConfigured.Text : _plinkputtyGenpageantNotFound.Text);
            }

            string sshPath = AppSettings.SshPath;
            if (!string.IsNullOrEmpty(sshPath) && !File.Exists(sshPath))
            {
                return (false, string.Format(_sshClientNotFound.Text, sshPath));
            }

            return (true, string.IsNullOrEmpty(sshPath)
                ? _opensshUsed.Text
                : string.Format(_otherSshClient.Text, sshPath));
        }

        bool TryCheck(Func<bool> check)
        {
            try
            {
                return check();
            }
            catch (Exception exception)
            {
                MessageBoxes.Show(
                    TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
                    exception.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
                return false;
            }
        }

        T TryEvaluate<T>(Func<T> check, T fallback)
        {
            try
            {
                return check();
            }
            catch (Exception exception)
            {
                MessageBoxes.Show(
                    TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
                    exception.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
                return fallback;
            }
        }
    }

    protected override void SettingsToPage()
    {
        base.SettingsToPage();
        CheckSettings();
    }

    private static void Render(
        Button status,
        Button repair,
        bool isVisible,
        bool valid,
        string message)
        => Render(
            status,
            repair,
            isVisible,
            valid ? CheckState.Valid : CheckState.Invalid,
            message);

    private static void Render(
        Button status,
        Button repair,
        bool isVisible,
        CheckState state,
        string message)
    {
        status.IsVisible = isVisible;
        repair.IsVisible = isVisible && state != CheckState.Valid;
        status.Content = message;
        status.Foreground = state switch
        {
            CheckState.Valid => Brushes.Green,
            CheckState.NotRecommended => Brushes.Goldenrod,
            _ => Brushes.OrangeRed,
        };
    }

    private void GitFound_Click(object? sender, EventArgs e)
    {
        if (!CheckSettingsLogic.SolveGitCommand())
        {
            MessageBoxes.Show(
                TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
                _solveGitCommandFailed.Text,
                _solveGitCommandFailedCaption.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            PageHost.GotoPage(GitSettingsPage.GetPageReference());
            return;
        }

        MessageBoxes.Show(
            TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
            string.Format(_gitCanBeRun.Text, AppSettings.GitCommandValue),
            _gitCanBeRunCaption.Text,
            WinFormsShims.MessageBoxButtons.OK,
            WinFormsShims.MessageBoxIcon.Information);
        PageHost.GotoPage(GitSettingsPage.GetPageReference());
        SaveAndRescan_Click(sender, e);
    }

    private void UserNameSet_Click(object? sender, EventArgs e)
        => PageHost.GotoPage(GitConfigSettingsPage.GetPageReference());

    private void MergeToolFix_Click(object? sender, EventArgs e)
    {
        string? mergeTool = _diffMergeToolConfigurationManager?.ConfiguredMergeTool;
        if (string.IsNullOrEmpty(mergeTool))
        {
            GotoPageGlobalSettings();
            return;
        }

        SaveAndRescan_Click(this, EventArgs.Empty);
    }

    private void DiffToolFix_Click(object? sender, EventArgs e)
    {
        string? diffTool = _diffMergeToolConfigurationManager?.ConfiguredDiffTool;
        if (string.IsNullOrEmpty(diffTool))
        {
            GotoPageGlobalSettings();
            return;
        }

        SaveAndRescan_Click(this, EventArgs.Empty);
    }

    private void GotoPageGlobalSettings()
        => PageHost.GotoPage(GitConfigSettingsPage.GetPageReference());

    private void ShellExtensionsRegistered_Click(object? sender, EventArgs e)
    {
        ShellExtensionManager.Register();
        CheckSettings();
    }

    private void GitBinFound_Click(object? sender, EventArgs e)
    {
        if (!CheckSettingsLogic.SolveLinuxToolsDir())
        {
            MessageBoxes.Show(
                TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
                _linuxToolsShNotFound.Text,
                _linuxToolsShNotFoundCaption.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            PageHost.GotoPage(GitSettingsPage.GetPageReference());
            return;
        }

        MessageBoxes.Show(
            TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
            string.Format(_shCanBeRun.Text, AppSettings.LinuxToolsDir),
            _shCanBeRunCaption.Text,
            WinFormsShims.MessageBoxButtons.OK,
            WinFormsShims.MessageBoxIcon.Information);
        PageHost.LoadAll(); // apply settings to dialog controls (otherwise the later called SaveAndRescan_Click would overwrite settings again)
        SaveAndRescan_Click(sender, e);
    }

    private void GitExtensionsInstall_Click(object? sender, EventArgs e)
    {
        CheckSettingsLogic.SolveGitExtensionsDir();
        CheckSettings();
    }

    private void SshConfig_Click(object? sender, EventArgs e)
    {
        if (GitSshHelpers.IsPlink)
        {
            if (SshSettingsPage is null)
            {
                return;
            }

            if (SshSettingsPage.AutoFindPuttyPaths())
            {
                MessageBoxes.Show(
                    TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
                    _puttyFoundAuto.Text,
                    _putty,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Information);
            }
            else
            {
                PageHost.GotoPage(SshSettingsPage.GetPageReference());
            }

            return;
        }

        if (SshSettingsPage is not null)
        {
            PageHost.GotoPage(SshSettingsPage.GetPageReference());
        }
    }

    private void translationConfig_Click(object? sender, EventArgs e)
    {
        using FormChooseTranslation frm = new();
        frm.ShowDialog(TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window); // will set Settings.Translation

        PageHost.LoadAll();

        Translator.Translate(this, AppSettings.CurrentTranslation);
        SaveAndRescan_Click(this, EventArgs.Empty);
    }

    private void GcmDetectedFix_Click(object? sender, EventArgs e)
        => OsShellUtil.OpenUrlInDefaultBrowser(
            "https://github.com/gitextensions/gitextensions/wiki/Fix-GitCredentialWinStore-missing");

    private void SaveAndRescan_Click(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            PageHost.SaveAll();
            PageHost.LoadAll();
            CheckSettings();
        }
    }

    private void CheckAtStartup_CheckedChanged(object? sender, EventArgs e)
        => AppSettings.CheckSettings = CheckAtStartup.IsChecked == true;

    internal enum CheckState
    {
        Invalid,
        NotRecommended,
        Valid,
    }

    internal sealed record ChecklistResult(
        CheckState GitStatus,
        bool Identity,
        bool MergeTool,
        bool DiffTool,
        bool Editor,
        bool Translation,
        bool WindowsChecksVisible,
        bool InstallRegistration,
        bool ShellExtensions,
        bool GitBin,
        bool Ssh,
        bool ObsoleteCredentialHelper,
        bool ObsoleteCredentialHelperVisible,
        string GitMessage,
        string IdentityMessage,
        string MergeToolMessage,
        string DiffToolMessage,
        string TranslationMessage,
        string InstallRegistrationMessage,
        string ShellExtensionsMessage,
        string GitBinMessage,
        string SshMessage,
        string ObsoleteCredentialHelperMessage)
    {
        internal bool IsValid
            => GitStatus == CheckState.Valid
               && Identity
               && MergeTool
               && DiffTool
               && Editor
               && Translation
               && InstallRegistration
               && ShellExtensions
               && GitBin
               && Ssh
               && ObsoleteCredentialHelper;
    }
}
