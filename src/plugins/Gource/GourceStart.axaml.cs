using System.Diagnostics;
using Avalonia.Platform.Storage;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.Avatars;
using GitUI.Compat;

namespace GitExtensions.Plugins.Gource;

public partial class GourceStart : ResourceManager.GitExtensionsFormBase
{
    public GourceStart()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public GourceStart(string pathToGource, GitUIEventArgs gitUIArgs, string gourceArguments)
    {
        InitializeComponent();
        button1.Click += Button1Click;
        GourceBrowse.Click += GourceBrowseClick;
        WorkingDirBrowse.Click += WorkingDirBrowseClick;
        linkLabel1.Click += linkLabel1_LinkClicked;
        linkLabel2.Click += linkLabel2_LinkClicked;
        InitializeComplete();

        // To accommodate the translation app
        if (gitUIArgs is null)
        {
            return;
        }

        PathToGource = pathToGource;
        GitUIArgs = gitUIArgs;
        GitWorkingDir = gitUIArgs.GitModule.WorkingDir;
        GourceArguments = gourceArguments;

        WorkingDir.Text = GitWorkingDir;
        GourcePath.Text = pathToGource;
        Arguments.Text = GourceArguments;
    }

    private GitUIEventArgs GitUIArgs { get; } = null!;

    public string PathToGource { get; set; } = "";

    public string? GitWorkingDir { get; set; }

    public string GourceArguments { get; set; } = "";

    private void RunRealCmdDetached(string cmd, string arguments)
        => RunRealCmdDetached(cmd, arguments, startInfo => Process.Start(startInfo));

    private void RunRealCmdDetached(
        string cmd,
        string arguments,
        Func<ProcessStartInfo, Process?> startProcess)
    {
        try
        {
            // Cross-platform constraint: pass the executable and arguments separately so no shell parses either value.
            startProcess(CreateProcessStartInfo(cmd, arguments, WorkingDir.Text ?? string.Empty));
        }
        catch (Exception e)
        {
            MessageBoxes.ShowError(this, e.Message);
        }
    }

    internal static ProcessStartInfo CreateProcessStartInfo(string cmd, string arguments, string workingDirectory)
        => new()
        {
            FileName = cmd,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };

    private void Button1Click(object? sender, EventArgs e)
        => StartGource(
            FlatpakEnvironment.IsFlatpak(),
            startInfo => Process.Start(startInfo),
            AvatarService.DefaultProvider);

    private void StartGource(
        bool isFlatpak,
        Func<ProcessStartInfo, Process?> startProcess,
        IAvatarProvider avatarProvider)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (!IsLaunchAvailable(isFlatpak))
        {
            // Cross-platform constraint: host executables are visible but not executable in Flatpak.
            MessageBoxes.FailedToRunShell(
                this,
                "Gource",
                new PlatformNotSupportedException("Gource is not available in this Flatpak installation."));
            return;
        }

        if (!File.Exists(GourcePath.Text))
        {
            MessageBoxes.ShowError(this, "Cannot find Gource.\nPlease download Gource and set the correct path.");
            return;
        }

        GourceArguments = Arguments.Text ?? string.Empty;
        string gourceAvatarsDir = GourceArguments.Contains("$(AVATARS)")
            ? ThreadHelper.JoinableTaskFactory.Run(() => LoadAvatarsAsync(avatarProvider))
            : "";
        string arguments = GourceArguments.Replace("$(AVATARS)", gourceAvatarsDir);
        PathToGource = GourcePath.Text;
        GitWorkingDir = WorkingDir.Text;

        RunRealCmdDetached(GourcePath.Text, arguments, startProcess);
        Close();
    }

    internal static bool IsLaunchAvailable(bool isFlatpak)
        => !isFlatpak;

    private async Task<string> LoadAvatarsAsync()
        => await LoadAvatarsAsync(AvatarService.DefaultProvider);

    private async Task<string> LoadAvatarsAsync(IAvatarProvider avatarProvider)
    {
        string gourceAvatarsDir = Path.Join(Path.GetTempPath(), "GitAvatars");

        Directory.CreateDirectory(gourceAvatarsDir);

        foreach (string file in Directory.GetFiles(gourceAvatarsDir))
        {
            File.Delete(file);
        }

        GitArgumentBuilder args = new("log") { "--pretty=format:\"%aE|%aN\"" };
        string[] lines = GitUIArgs.GitModule.GitExecutable.GetOutput(args).Split('\n');

        IEnumerable<(string email, string name)> authors = lines.Select(
            line =>
            {
                string[] bits = line.Split('|');
                return (email: bits[0], name: bits[1]);
            })
            .Where(t => !string.IsNullOrWhiteSpace(t.email) && !string.IsNullOrWhiteSpace(t.name))
            .GroupBy(t => t.name)
            .Select(g => (g.First().email, name: g.Key));

        await Task.WhenAll(authors.Select(DownloadImageAsync));

        return gourceAvatarsDir;

        async Task DownloadImageAsync((string email, string name) author)
        {
            try
            {
                byte[]? image = await avatarProvider.GetAvatarAsync(author.email, author.name, imageSize: 90);
                string filename = author.name + ".png";

                if (image is null || filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return;
                }

                string filePath = Path.Join(gourceAvatarsDir, filename);
                await File.WriteAllBytesAsync(filePath, image);
            }
            catch
            {
                // Do nothing
            }
        }
    }

    private void GourceBrowseClick(object? sender, EventArgs e)
    {
        string? selectedPath = DispatcherPump.Wait(PickGourceAsync);
        ApplyGourceSelection(selectedPath);

        async Task<string?> PickGourceAsync()
        {
            FilePickerOpenOptions options = await CreateGourcePickerOptionsAsync(StorageProvider);

            if (!await PortalPickerGuard.IsAvailableAsync())
            {
                return null;
            }

            IReadOnlyList<IStorageFile> files = await PortalPickerGuard.OpenFilePickerAsync(StorageProvider, options);
            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        }
    }

    private void WorkingDirBrowseClick(object? sender, EventArgs e)
    {
        string? selectedPath = DispatcherPump.Wait(PickWorkingDirectoryAsync);
        ApplyWorkingDirectorySelection(selectedPath);

        async Task<string?> PickWorkingDirectoryAsync()
        {
            FolderPickerOpenOptions options = await CreateWorkingDirectoryPickerOptionsAsync(StorageProvider);

            if (!await PortalPickerGuard.IsAvailableAsync())
            {
                return null;
            }

            IReadOnlyList<IStorageFolder> folders = await PortalPickerGuard.OpenFolderPickerAsync(StorageProvider, options);
            return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
        }
    }

    private async Task<FilePickerOpenOptions> CreateGourcePickerOptionsAsync(IStorageProvider storageProvider)
    {
        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Gource")
                {
                    Patterns = OperatingSystem.IsWindows() ? ["gource.exe"] : ["gource"],
                },
            ],
        };

        if (!string.IsNullOrWhiteSpace(GourcePath.Text))
        {
            options.SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(
                Path.GetDirectoryName(GourcePath.Text) ?? string.Empty);
        }

        return options;
    }

    private async Task<FolderPickerOpenOptions> CreateWorkingDirectoryPickerOptionsAsync(IStorageProvider storageProvider)
    {
        FolderPickerOpenOptions options = new() { AllowMultiple = false };
        if (!string.IsNullOrWhiteSpace(WorkingDir.Text))
        {
            options.SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(WorkingDir.Text);
        }

        return options;
    }

    private void ApplyGourceSelection(string? selectedPath)
    {
        if (!string.IsNullOrEmpty(selectedPath))
        {
            GourcePath.Text = selectedPath;
        }
    }

    private void ApplyWorkingDirectorySelection(string? selectedPath)
    {
        if (!string.IsNullOrEmpty(selectedPath))
        {
            WorkingDir.Text = selectedPath;
        }
    }

    private void linkLabel1_LinkClicked(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/acaudwell/Gource/");
    }

    private void linkLabel2_LinkClicked(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/acaudwell/Gource#readme");
    }

    // parity-scaffolding: Exposes the original dialog actions to focused functional tests.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(GourceStart form)
    {
        internal Avalonia.Controls.TextBox Arguments => form.Arguments;
        internal Avalonia.Controls.Button Button1 => form.button1;
        internal Avalonia.Controls.TextBox GourcePath => form.GourcePath;
        internal Avalonia.Controls.TextBox WorkingDir => form.WorkingDir;
        internal Task<FilePickerOpenOptions> CreateGourcePickerOptionsAsync(IStorageProvider storageProvider)
            => form.CreateGourcePickerOptionsAsync(storageProvider);
        internal Task<FolderPickerOpenOptions> CreateWorkingDirectoryPickerOptionsAsync(IStorageProvider storageProvider)
            => form.CreateWorkingDirectoryPickerOptionsAsync(storageProvider);
        internal void ApplyGourceSelection(string? selectedPath) => form.ApplyGourceSelection(selectedPath);
        internal void ApplyWorkingDirectorySelection(string? selectedPath) => form.ApplyWorkingDirectorySelection(selectedPath);
        internal Task<string> LoadAvatarsAsync(IAvatarProvider avatarProvider) => form.LoadAvatarsAsync(avatarProvider);
        internal void OpenProjectLink() => form.linkLabel1_LinkClicked(null, EventArgs.Empty);
        internal void OpenCommandLineLink() => form.linkLabel2_LinkClicked(null, EventArgs.Empty);
        internal void StartGource(
            bool isFlatpak,
            Func<ProcessStartInfo, Process?> startProcess,
            IAvatarProvider avatarProvider)
            => form.StartGource(isFlatpak, startProcess, avatarProvider);
    }
}
