using Avalonia.Controls;
using GitCommands;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class FormFixHome : GitExtensionsFormBase
{
    private readonly TranslationString _gitGlobalConfigNotFound = new(
        "The environment variable HOME does not point to a directory that contains the global git config file:" + Environment.NewLine
        + "\" {0} \"" + Environment.NewLine + Environment.NewLine
        + "Do you want Git Extensions to help locate the correct folder?");
    private readonly TranslationString _gitGlobalConfigNotFoundCaption = new("Global config");
    private readonly TranslationString _noHomeDirectorySpecified = new("Please enter a HOME directory.");
    private readonly TranslationString _homeNotAccessible = new(
        "The environment variable HOME points to a directory that is not accessible:" + Environment.NewLine + "\"{0}\"");
    private readonly TranslationString _portableHomeDescription = new(
        "The global config file is located using the HOME environment variable.\nChange the default behaviour only if you experience problems.");

    public FormFixHome()
    {
        InitializeComponent();
        WireEvents();
        AcceptButton = ok;
        InitializeComplete();

        if (!OperatingSystem.IsWindows())
        {
            label51.IsVisible = false;
            _NO_TRANSLATE_portableHomeDescription.Text = _portableHomeDescription.Text;
            _NO_TRANSLATE_portableHomeDescription.IsVisible = true;
            userprofileHome.IsVisible = false;
        }
    }

    public void ShowIfUserWant()
    {
        if (MessageBoxes.Show(
                string.Format(_gitGlobalConfigNotFound.Text, Environment.GetEnvironmentVariable("HOME")),
                _gitGlobalConfigNotFoundCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Error) == WinFormsShims.DialogResult.Yes)
        {
            ShowDialog(owner: null);
        }
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

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        LoadSettings();

        defaultHome.Content = $"{defaultHome.Content} ({EnvironmentConfiguration.GetDefaultHomeDir()})";
        if (OperatingSystem.IsWindows())
        {
            userprofileHome.Content = $"{userprofileHome.Content} ({Environment.GetEnvironmentVariable("USERPROFILE")})";
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

    private void LoadSettings()
    {
        if (!string.IsNullOrEmpty(AppSettings.CustomHomeDir))
        {
            otherHome.IsChecked = true;
            otherHomeDir.Text = AppSettings.CustomHomeDir;
        }
        else if (OperatingSystem.IsWindows() && AppSettings.UserProfileHomeDir)
        {
            userprofileHome.IsChecked = true;
        }
        else
        {
            defaultHome.IsChecked = true;
        }

        UpdateOtherHomeState();
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

        AppSettings.UserProfileHomeDir = OperatingSystem.IsWindows() && userprofileHome.IsChecked == true;
        EnvironmentConfiguration.SetEnvironmentVariables();
        string home = EnvironmentConfiguration.GetHomeDir();
        if (string.IsNullOrEmpty(home) || !Directory.Exists(home))
        {
            MessageBoxes.ShowError(this, string.Format(_homeNotAccessible.Text, home));
            return false;
        }

        return true;
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

    private void UpdateOtherHomeState()
    {
        bool enabled = otherHome.IsChecked == true;
        otherHomeDir.IsEnabled = enabled;
        otherHomeBrowse.IsEnabled = enabled;
    }

    private static bool HasGlobalGitConfig(string? path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return false;
        }

        if (CanReadFile(Path.Join(path, ".gitconfig")))
        {
            return true;
        }

        string xdgConfigDirectory = Path.Join(path, ".config");
        if (!Directory.Exists(xdgConfigDirectory))
        {
            return false;
        }

        string? xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        return (string.IsNullOrEmpty(xdgConfigHome) || xdgConfigHome == xdgConfigDirectory)
            && CanReadFile(Path.Join(xdgConfigDirectory, "git", "config"));

        static bool CanReadFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return false;
                }

                File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Dispose();
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
            string? home = Environment.GetEnvironmentVariable("HOME");
            return string.IsNullOrEmpty(home) || !Directory.Exists(home) || !HasGlobalGitConfig(home);
        }
        catch
        {
            return true;
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

        public static bool HasGlobalGitConfig(string path) => FormFixHome.HasGlobalGitConfig(path);
    }
}
