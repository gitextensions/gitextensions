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
    {
        try
        {
            // Cross-platform constraint: pass the executable and arguments separately so no shell parses either value.
            Process.Start(CreateProcessStartInfo(cmd, arguments, WorkingDir.Text ?? string.Empty));
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
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (!File.Exists(GourcePath.Text))
        {
            MessageBoxes.ShowError(this, "Cannot find Gource.\nPlease download Gource and set the correct path.");
            return;
        }

        GourceArguments = Arguments.Text ?? string.Empty;
        string gourceAvatarsDir = GourceArguments.Contains("$(AVATARS)")
            ? ThreadHelper.JoinableTaskFactory.Run(LoadAvatarsAsync)
            : "";
        string arguments = GourceArguments.Replace("$(AVATARS)", gourceAvatarsDir);
        PathToGource = GourcePath.Text;
        GitWorkingDir = WorkingDir.Text;

        RunRealCmdDetached(GourcePath.Text, arguments);
        Close();
    }

    private async Task<string> LoadAvatarsAsync()
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
                byte[]? image = await AvatarService.DefaultProvider.GetAvatarAsync(author.email, author.name, imageSize: 90);
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
        if (!string.IsNullOrEmpty(selectedPath))
        {
            GourcePath.Text = selectedPath;
        }

        async Task<string?> PickGourceAsync()
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
                options.SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(
                    Path.GetDirectoryName(GourcePath.Text) ?? string.Empty);
            }

            IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(options);
            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        }
    }

    private void WorkingDirBrowseClick(object? sender, EventArgs e)
    {
        string? selectedPath = DispatcherPump.Wait(PickWorkingDirectoryAsync);
        if (!string.IsNullOrEmpty(selectedPath))
        {
            WorkingDir.Text = selectedPath;
        }

        async Task<string?> PickWorkingDirectoryAsync()
        {
            FolderPickerOpenOptions options = new() { AllowMultiple = false };
            if (!string.IsNullOrWhiteSpace(WorkingDir.Text))
            {
                options.SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(WorkingDir.Text);
            }

            IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(options);
            return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
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
}
