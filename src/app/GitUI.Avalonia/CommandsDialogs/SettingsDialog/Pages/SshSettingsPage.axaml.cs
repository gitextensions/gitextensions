using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using Microsoft.Win32;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class SshSettingsPage : SettingsPageWithHeader
{
    public SshSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        Text = "SSH";

        OpenSSH.IsCheckedChanged += (_, _) => EnableSshOptions();
        Putty.IsCheckedChanged += (_, _) =>
        {
            if (Putty.IsChecked == true)
            {
                AutoFindPuttyPaths();
            }

            EnableSshOptions();
        };
        Other.IsCheckedChanged += (_, _) => EnableSshOptions();
        OtherSshBrowse.Click += (_, _) => ThreadHelper.FileAndForget(
            () => BrowseExecutableAsync(OtherSsh, "Select SSH client", ["*.exe", "*"]));
        PlinkBrowse.Click += (_, _) => ThreadHelper.FileAndForget(
            () => BrowseExecutableAsync(
                PlinkPath,
                "Select plink",
                ["plink.exe", "tortoisegitplink.exe", "tortoiseplink.exe"]));
        PuttygenBrowse.Click += (_, _) => ThreadHelper.FileAndForget(
            () => BrowseExecutableAsync(PuttygenPath, "Select puttygen", ["puttygen.exe"]));
        PageantBrowse.Click += (_, _) => ThreadHelper.FileAndForget(
            () => BrowseExecutableAsync(PageantPath, "Select pageant", ["pageant.exe"]));
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(SshSettingsPage));

    protected override void SettingsToPage()
    {
        PlinkPath.Text = AppSettings.Plink;
        PuttygenPath.Text = AppSettings.Puttygen;
        PageantPath.Text = AppSettings.Pageant;
        AutostartPageant.IsChecked = AppSettings.AutoStartPageant;

        string sshPath = AppSettings.SshPath;
        if (string.IsNullOrEmpty(sshPath))
        {
            OpenSSH.IsChecked = true;
        }
        else if (OperatingSystem.IsWindows() && GitSshHelpers.IsPlink)
        {
            Putty.IsChecked = true;
        }
        else
        {
            OtherSsh.Text = sshPath;
            Other.IsChecked = true;
        }

        EnableSshOptions();
        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.Plink = PlinkPath.Text ?? string.Empty;
        AppSettings.Puttygen = PuttygenPath.Text ?? string.Empty;
        AppSettings.Pageant = PageantPath.Text ?? string.Empty;
        AppSettings.AutoStartPageant = AutostartPageant.IsChecked == true;

        string path = OpenSSH.IsChecked == true
            ? string.Empty
            : Putty.IsChecked == true
                ? PlinkPath.Text ?? string.Empty
                : OtherSsh.Text ?? string.Empty;
        GitSshHelpers.SetGitSshEnvironmentVariable(path);
        AppSettings.SshPath = path;
        base.PageToSettings();
    }

    public bool AutoFindPuttyPaths()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        return GetPuttyLocations().Any(AutoFindPuttyPathsInDir);
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> GetPuttyLocations()
    {
        string? configured = Environment.GetEnvironmentVariable("GITEXT_PUTTY");
        if (!string.IsNullOrEmpty(configured))
        {
            yield return configured;
        }

        string? programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
        string? programFilesX86 = Environment.Is64BitOperatingSystem
            ? Environment.GetEnvironmentVariable("ProgramFiles(x86)")
            : null;
        if (!string.IsNullOrEmpty(programFiles))
        {
            yield return Path.Join(programFiles, "PuTTY");
            yield return Path.Join(programFiles, "TortoiseGit", "bin");
            yield return Path.Join(programFiles, "TortoiseSvn", "bin");
        }

        if (!string.IsNullOrEmpty(programFilesX86))
        {
            yield return Path.Join(programFilesX86, "PuTTY");
            yield return Path.Join(programFilesX86, "TortoiseGit", "bin");
            yield return Path.Join(programFilesX86, "TortoiseSvn", "bin");
        }

        string? registryLocation = GitUI.CommandsDialogs.SettingsDialog.CommonLogic.GetRegistryValue(
            Registry.LocalMachine,
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\PuTTY_is1",
            "InstallLocation");
        if (!string.IsNullOrEmpty(registryLocation))
        {
            yield return registryLocation;
        }
    }

    private bool AutoFindPuttyPathsInDir(string installDirectory)
    {
        if (string.IsNullOrWhiteSpace(installDirectory))
        {
            return false;
        }

        if (!File.Exists(PlinkPath.Text))
        {
            string plink = Path.Join(installDirectory, "plink.exe");
            string tortoisePlink = Path.Join(installDirectory, "TortoisePlink.exe");
            PlinkPath.Text = File.Exists(plink) ? plink : File.Exists(tortoisePlink) ? tortoisePlink : PlinkPath.Text;
        }

        SetPathIfFound(PuttygenPath, Path.Join(installDirectory, "puttygen.exe"));
        SetPathIfFound(PageantPath, Path.Join(installDirectory, "pageant.exe"));
        return File.Exists(PlinkPath.Text)
            && File.Exists(PuttygenPath.Text)
            && File.Exists(PageantPath.Text);

        static void SetPathIfFound(TextBox textBox, string candidate)
        {
            if (!File.Exists(textBox.Text) && File.Exists(candidate))
            {
                textBox.Text = candidate;
            }
        }
    }

    private async Task BrowseExecutableAsync(TextBox target, string title, IReadOnlyList<string> patterns)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = title,
            FileTypeFilter =
            [
                new FilePickerFileType("Executable") { Patterns = patterns },
                FilePickerFileTypes.All,
            ],
        };
        string? currentDirectory = Path.GetDirectoryName(target.Text);
        if (!string.IsNullOrEmpty(currentDirectory))
        {
            options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(currentDirectory);
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            target.Text = path;
        }
    }

    private void EnableSshOptions()
    {
        bool useOther = Other.IsChecked == true;
        bool usePutty = Putty.IsChecked == true && OperatingSystem.IsWindows();
        Putty.IsVisible = OperatingSystem.IsWindows();
        groupBox2.IsVisible = usePutty;
        OtherSsh.IsEnabled = useOther;
        OtherSshBrowse.IsEnabled = useOther;
        PlinkPath.IsEnabled = usePutty;
        PuttygenPath.IsEnabled = usePutty;
        PageantPath.IsEnabled = usePutty;
        PlinkBrowse.IsEnabled = usePutty;
        PuttygenBrowse.IsEnabled = usePutty;
        PageantBrowse.IsEnabled = usePutty;
        AutostartPageant.IsEnabled = usePutty;
    }
}
