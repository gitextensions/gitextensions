using System.ComponentModel.Composition;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.Gource.Properties;
using GitUI;
using ResourceManager;

namespace GitExtensions.Plugins.Gource;

[Export(typeof(IGitPlugin))]
public class GourcePlugin : GitPluginBase, IGitPluginForRepository
{
    #region Translation
    private readonly TranslationString _currentDirectoryIsNotValidGit = new("The current directory is not a valid git repository.\n\n" +
        "Gource can be only be started from a valid git repository.");
    private readonly TranslationString _resetConfigPath = new("Cannot find Gource in the configured path: {0}.\n\n" +
        "Do you want to reset the configured path?");
    private readonly TranslationString _gource = new("Gource");
    private readonly TranslationString _doYouWantDownloadGource = new("There is no path to Gource configured.\n\n" +
        "Do you want to automatically download Gource?");
    private readonly TranslationString _download = new("Download");
    private readonly TranslationString _cannotFindGource = new("Cannot find Gource.\n" +
        "Please download Gource and set the path in the plugins settings dialog.");
    private readonly TranslationString _bytesDownloaded = new("{0} bytes downloaded.");
    private readonly TranslationString _gourceDownloadedAndUnzipped = new("Gource has been downloaded and unzipped.");
    private readonly TranslationString _downloadingFailed = new("Downloading failed.\n" +
        "Please download Gource and set the path in the plugins settings dialog.");
    #endregion

    private readonly StringSetting _gourcePath = new("Path to Gource", "");
    private readonly StringSetting _gourceArguments = new("Arguments", "--hide filenames --user-image-dir \"$(AVATARS)\"");

    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        UseProxy = true,
        DefaultProxyCredentials = CredentialCache.DefaultCredentials
    });

    public GourcePlugin() : base(true)
    {
        Id = new Guid("F0A6A769-6DCC-4452-9A43-343347015EEC");
        Name = "Gource";
        Translate(AppSettings.CurrentTranslation);
        Icon = Resources.IconGource;
    }

    #region IGitPlugin Members

    public override IEnumerable<ISetting> GetSettings()
    {
        // return all settings or introduce implementation based on reflection on GitPluginBase level
        yield return _gourcePath;
        yield return _gourceArguments;
    }

    public override bool Execute(GitUIEventArgs args)
    {
        if (!args.GitModule.IsValidGitWorkingDir())
        {
            MessageBox.Show(args.OwnerForm, _currentDirectoryIsNotValidGit.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        string pathToGource = _gourcePath.ValueOrDefault(Settings);

        if (!string.IsNullOrEmpty(pathToGource) && !File.Exists(pathToGource))
        {
            DialogResult result = MessageBox.Show(
                args.OwnerForm,
                string.Format(_resetConfigPath.Text, pathToGource), _gource.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Settings.SetValue(_gourcePath.Name, _gourcePath.DefaultValue);
                pathToGource = _gourcePath.DefaultValue;
            }
        }

        if (string.IsNullOrEmpty(pathToGource))
        {
            if (MessageBox.Show(
                    args.OwnerForm, _doYouWantDownloadGource.Text, _download.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string gourceUrl = ThreadHelper.JoinableTaskFactory.Run(SearchForGourceUrlAsync);

                if (string.IsNullOrEmpty(gourceUrl))
                {
                    MessageBox.Show(args.OwnerForm, _cannotFindGource.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                string downloadDir = Path.GetTempPath();
                string fileName = Path.Combine(downloadDir, "gource.zip");
                int downloadSize = ThreadHelper.JoinableTaskFactory.Run(() => DownloadFileAsync(gourceUrl, fileName));
                if (downloadSize > 0)
                {
                    MessageBox.Show(string.Format(_bytesDownloaded.Text, downloadSize), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Directory.CreateDirectory(Path.Combine(downloadDir, "gource"));
                    UnZipFiles(fileName, Path.Combine(downloadDir, "gource"), true);

                    string newGourcePath = Path.Combine(downloadDir, "gource\\gource.exe");
                    if (File.Exists(newGourcePath))
                    {
                        MessageBox.Show(args.OwnerForm, _gourceDownloadedAndUnzipped.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        pathToGource = newGourcePath;
                    }
                }
                else
                {
                    MessageBox.Show(args.OwnerForm, _downloadingFailed.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        using GourceStart gourceStart = new(pathToGource, args, _gourceArguments.ValueOrDefault(Settings));
        gourceStart.ShowDialog(args.OwnerForm);
        Settings.SetValue(_gourceArguments.Name, gourceStart.GourceArguments);
        Settings.SetValue(_gourcePath.Name, gourceStart.PathToGource);

        return false;
    }

    #endregion

    private static void UnZipFiles(string zipPathAndFile, string outputFolder, bool deleteZipFile)
    {
        try
        {
            if (outputFolder != "")
            {
                Directory.CreateDirectory(outputFolder);
            }

            string outputFolderFullPath = Path.GetFullPath(string.IsNullOrEmpty(outputFolder) ? "." : outputFolder);

            using (ZipArchive archive = ZipFile.OpenRead(zipPathAndFile))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name) || entry.FullName.Contains(".ini"))
                    {
                        continue;
                    }

                    string destinationPath = Path.Combine(outputFolder, entry.FullName).Replace("\\ ", "\\");
                    string fullDestinationPath = Path.GetFullPath(destinationPath);
                    if (!fullDestinationPath.StartsWith(outputFolderFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string? fullDirPath = Path.GetDirectoryName(fullDestinationPath);
                    if (fullDirPath is not null && !Directory.Exists(fullDirPath))
                    {
                        Directory.CreateDirectory(fullDirPath);
                    }

                    entry.ExtractToFile(fullDestinationPath, overwrite: true);
                }
            }

            if (deleteZipFile)
            {
                File.Delete(zipPathAndFile);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static async Task<int> DownloadFileAsync(string remoteFilename, string localFilename)
    {
        try
        {
            using Stream remoteStream = await _httpClient.GetStreamAsync(remoteFilename);
            using FileStream localStream = File.Create(localFilename);
            await remoteStream.CopyToAsync(localStream);

            return (int)localStream.Position;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 0;
        }
    }

    private static async Task<string> SearchForGourceUrlAsync()
    {
        // All Gource releases do not have binary releases, use a fallback
        const string latestApiUrl = "https://api.github.com/repos/acaudwell/Gource/releases/latest";
        const string latestBinReleaseUrl = "https://api.github.com/repos/acaudwell/Gource/releases/tags/gource-0.53";
        const string win64ZipSuffix = ".win64.zip";

        try
        {
            string url = await FindWin64AssetAsync(latestApiUrl);
            if (!string.IsNullOrEmpty(url))
            {
                return url;
            }

            return await FindWin64AssetAsync(latestBinReleaseUrl);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }

        async Task<string> FindWin64AssetAsync(string apiUrl)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, apiUrl);
            request.Headers.UserAgent.ParseAdd("GitExtensions");
            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            foreach (JsonElement asset in doc.RootElement.GetProperty("assets").EnumerateArray())
            {
                string name = asset.GetProperty("name").GetString() ?? string.Empty;
                if (name.StartsWith("gource-", StringComparison.OrdinalIgnoreCase)
                    && name.EndsWith(win64ZipSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    return asset.GetProperty("browser_download_url").GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
