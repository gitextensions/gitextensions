using Avalonia.Controls;
using GitCommands;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class FormFixHome : GitExtensionsFormBase
{
    private const string PortableHomeDescription =
        "The global config file is located using the HOME environment variable.\nChange the default behaviour only if you experience problems.";

    private readonly TranslationString _gitGlobalConfigNotFound = new(
        "The environment variable HOME does not point to a directory that contains the global git config file:" + Environment.NewLine
        + "\" {0} \"" + Environment.NewLine + Environment.NewLine
        + "Do you want Git Extensions to help locate the correct folder?");
    private readonly TranslationString _gitGlobalConfigNotFoundCaption = new("Global config");
    private readonly TranslationString _gitconfigFoundHome =
        new("Located .gitconfig in %HOME% ({0}). This setting has been chosen automatically.");
    private readonly TranslationString _gitconfigFoundHomedrive =
        new("Located .gitconfig in %HOMEDRIVE%%HOMEPATH% ({0}). This setting has been chosen automatically.");
    private readonly TranslationString _gitconfigFoundUserprofile =
        new("Located .gitconfig in %USERPROFILE% ({0}). This setting has been chosen automatically.");
    private readonly TranslationString _gitconfigFoundPersonalFolder =
        new("Located .gitconfig in personal folder ({0}). This setting has been chosen automatically.");

    private readonly TranslationString _noHomeDirectorySpecified = new("Please enter a HOME directory.");
    private readonly TranslationString _homeNotAccessible = new(
        "The environment variable HOME points to a directory that is not accessible:" + Environment.NewLine + "\"{0}\"");

    // parity-scaffolding: isolate external environment reads when verifying HOME discovery.
    private static IEnvironmentAbstraction _environment = new EnvironmentAbstraction();

    public FormFixHome()
    {
        InitializeComponent();
        WireEvents();
        AcceptButton = ok;
        InitializeComplete();

        if (!OperatingSystem.IsWindows())
        {
            label51.IsVisible = false;
            _NO_TRANSLATE_portableHomeDescription.Text = PortableHomeDescription;
            _NO_TRANSLATE_portableHomeDescription.IsVisible = true;
            userprofileHome.IsVisible = false;
        }
    }

    private static bool HasGlobalGitConfig(string? path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return false;
        }

        // Check default Git config location
        string gitConfigFile = Path.Join(path, ".gitconfig");
        if (CanReadFile(gitConfigFile))
        {
            return true;
        }

        // Check presence of XDG config directory
        string xdgConfigDir = Path.Join(path, ".config");
        if (!Directory.Exists(xdgConfigDir))
        {
            return false;
        }

        // Check whether the XDG_CONFIG_HOME is compatible (unset or matching) with "path" being tested as potential HOME directory
        // and contains a git config file in the according subfolder
        // (refer to https://git-scm.com/docs/git-config#Documentation/git-config.txt-XDGCONFIGHOMEgitconfig)
        // Make issues with casing a "user problem" (case-insensitive equality would depend on file system type)
        string? xdgConfigHome = _environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrEmpty(xdgConfigHome) || xdgConfigHome == xdgConfigDir)
        {
            // Consider alternative Git config file
            string xdgGitConfigFile = Path.Join(xdgConfigDir, "git", "config");
            if (CanReadFile(xdgGitConfigFile))
            {
                return true;
            }
        }

        return false;

        static bool CanReadFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    private static bool IsFixHome()
    {
        try
        {
            string? home = _environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(home) || !Directory.Exists(home))
            {
                return true;
            }

            if (HasGlobalGitConfig(home))
            {
                return false;
            }

            string?[] candidates = [
                _environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User),
                _environment.GetEnvironmentVariable("HOMEDRIVE") + _environment.GetEnvironmentVariable("HOMEPATH"),
                _environment.GetEnvironmentVariable("USERPROFILE"),
                _environment.GetFolderPath(Environment.SpecialFolder.Personal)
            ];

            foreach (string? candidate in candidates)
            {
                if (HasGlobalGitConfig(candidate))
                {
                    return true;
                }
            }

            // No (better) candidates for HOME directory available
            return false;
        }
        catch
        {
            // Exception occurred while checking for home dir.
            // Could be a security issue. Just return true to let the user fix
            // this manually.
            return true;
        }
    }

    private void WireEvents()
    {
        defaultHome.IsCheckedChanged += (_, _) => UpdateOtherHomeState();
        userprofileHome.IsCheckedChanged += (_, _) => UpdateOtherHomeState();
        otherHome.IsCheckedChanged += (_, _) => UpdateOtherHomeState();
        otherHomeBrowse.Click += otherHomeBrowse_Click;
        ok.Click += ok_Click;
    }

    public void ShowIfUserWant()
    {
        if (MessageBoxes.Show(
                string.Format(_gitGlobalConfigNotFound.Text, _environment.GetEnvironmentVariable("HOME")),
                _gitGlobalConfigNotFoundCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Error) == WinFormsShims.DialogResult.Yes)
        {
            ShowDialog(owner: null);
        }
    }

    private bool ApplySettings()
    {
        if (otherHome.IsChecked == true)
        {
            if (string.IsNullOrWhiteSpace(otherHomeDir.Text))
            {
                MessageBoxes.ShowError(this, _noHomeDirectorySpecified.Text);
                return false;
            }

            AppSettings.CustomHomeDir = otherHomeDir.Text;
        }
        else
        {
            AppSettings.CustomHomeDir = string.Empty;
        }

        AppSettings.UserProfileHomeDir = userprofileHome.IsChecked == true;
        EnvironmentConfiguration.SetEnvironmentVariables();
        string home = EnvironmentConfiguration.GetHomeDir();
        if (string.IsNullOrEmpty(home) || !Directory.Exists(home))
        {
            MessageBoxes.ShowError(this, string.Format(_homeNotAccessible.Text, home));
            return false;
        }

        return true;
    }

    public static void CheckHomePath()
    {
        EnvironmentConfiguration.SetEnvironmentVariables();
        if (IsFixHome())
        {
            using FormFixHome form = new();
            form.ShowIfUserWant();
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        LoadSettings();

        defaultHome.Content = string.Format(defaultHome.Content + " ({0})", EnvironmentConfiguration.GetDefaultHomeDir());
        if (userprofileHome.IsVisible)
        {
            userprofileHome.Content = string.Format(userprofileHome.Content + " ({0})", _environment.GetEnvironmentVariable("USERPROFILE"));
        }
    }

    private void LoadSettings()
    {
        if (!string.IsNullOrEmpty(AppSettings.CustomHomeDir))
        {
            defaultHome.IsChecked = userprofileHome.IsChecked = false;
            otherHome.IsChecked = true;
            otherHomeDir.Text = AppSettings.CustomHomeDir;
        }
        else if (AppSettings.UserProfileHomeDir)
        {
            defaultHome.IsChecked = otherHome.IsChecked = false;
            userprofileHome.IsVisible = true;
            userprofileHome.IsChecked = true;
        }
        else
        {
            userprofileHome.IsChecked = otherHome.IsChecked = false;
            defaultHome.IsChecked = true;
        }

        try
        {
            string? userHomeDir = _environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User);
            if (HasGlobalGitConfig(userHomeDir))
            {
                MessageBoxes.Show(this, string.Format(_gitconfigFoundHome.Text, userHomeDir), "Information", WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Information);
                defaultHome.IsChecked = true;
                return;
            }
        }
        catch
        {
            // Exception occurred while checking for home dir.
            // Could be a security issue. Just ignore and let the user choose
            // manually.
        }

        try
        {
            string path = _environment.GetEnvironmentVariable("HOMEDRIVE") +
                       _environment.GetEnvironmentVariable("HOMEPATH");
            if (HasGlobalGitConfig(path))
            {
                MessageBoxes.Show(this, string.Format(_gitconfigFoundHomedrive.Text, path), "Information", WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Information);
                defaultHome.IsChecked = true;
                return;
            }
        }
        catch
        {
            // Exception occurred while checking for home dir.
            // Could be a security issue. Just ignore and let the user choose
            // manually.
        }

        try
        {
            string? path = _environment.GetEnvironmentVariable("USERPROFILE");
            if (HasGlobalGitConfig(path))
            {
                MessageBoxes.Show(this, string.Format(_gitconfigFoundUserprofile.Text, path), "Information", WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Information);
                userprofileHome.IsVisible = true;
                userprofileHome.IsChecked = true;
                return;
            }
        }
        catch
        {
            // Exception occurred while checking for home dir.
            // Could be a security issue. Just ignore and let the user choose
            // manually.
        }

        try
        {
            string path = _environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (HasGlobalGitConfig(path))
            {
                MessageBoxes.Show(this, string.Format(_gitconfigFoundPersonalFolder.Text, _environment.GetFolderPath(Environment.SpecialFolder.Personal)),
                    "Information", WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Information);
                otherHome.IsChecked = true;
                otherHomeDir.Text = _environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }
        catch
        {
            // Exception occurred while checking for home dir.
            // Could be a security issue. Just ignore and let the user choose
            // manually.
        }
    }

    private void UpdateOtherHomeState()
    {
        bool enabled = otherHome.IsChecked == true;
        otherHomeDir.IsEnabled = enabled;
        otherHomeBrowse.IsEnabled = enabled;
    }

    private void ok_Click(object? sender, EventArgs e)
    {
        if (ApplySettings())
        {
            DialogResult = WinFormsShims.DialogResult.OK;
            Close();
        }
    }

    private void otherHomeBrowse_Click(object? sender, EventArgs e)
    {
        WinFormsShims.IWin32Window owner = (TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window)!;
        string? userSelectedPath = OsShellUtil.PickFolder(owner, otherHomeDir.Text);
        if (userSelectedPath is not null)
        {
            otherHomeDir.Text = userSelectedPath;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormFixHome form)
    {
        public RadioButton DefaultHome => form.defaultHome;

        public RadioButton UserProfileHome => form.userprofileHome;

        public RadioButton OtherHome => form.otherHome;

        public TextBox OtherHomeDirectory => form.otherHomeDir;

        public void LoadSettings() => form.LoadSettings();

        public bool ApplySettings() => form.ApplySettings();

        // parity-scaffolding: substitute external environment reads without persistent user-variable writes.
        public static IEnvironmentAbstraction EnvironmentAbstraction
        {
            get => _environment;
            set => _environment = value;
        }

        public static bool HasGlobalGitConfig(string path) => FormFixHome.HasGlobalGitConfig(path);
    }
}
