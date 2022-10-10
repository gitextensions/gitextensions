﻿using System.ComponentModel.Composition;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using GitExtensions.Plugins.Gource.Properties;
using GitUIPluginInterfaces;
using ICSharpCode.SharpZipLib.Zip;
using ResourceManager;

namespace GitExtensions.Plugins.Gource
{
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

        public GourcePlugin() : base(true)
        {
            Id = new Guid("F0A6A769-6DCC-4452-9A43-343347015EEC");
            Name = "Gource";
            Translate();
            Icon = Resources.IconGource;
        }

        private readonly StringSetting _gourcePath = new("Path to Gource", "");
        private readonly StringSetting _gourceArguments = new("Arguments", "--hide filenames --user-image-dir \"$(AVATARS)\"");

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

            var pathToGource = _gourcePath.ValueOrDefault(Settings);

            if (!File.Exists(pathToGource))
            {
                var result = MessageBox.Show(
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
                    var gourceUrl = SearchForGourceUrl();

                    if (string.IsNullOrEmpty(gourceUrl))
                    {
                        MessageBox.Show(args.OwnerForm, _cannotFindGource.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    var downloadDir = Path.GetTempPath();
                    var fileName = Path.Combine(downloadDir, "gource.zip");
                    var downloadSize = DownloadFile(gourceUrl, fileName);
                    if (downloadSize > 0)
                    {
                        MessageBox.Show(string.Format(_bytesDownloaded.Text, downloadSize), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Directory.CreateDirectory(Path.Combine(downloadDir, "gource"));
                        UnZipFiles(fileName, Path.Combine(downloadDir, "gource"), true);

                        var newGourcePath = Path.Combine(downloadDir, "gource\\gource.exe");
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

                using (var zipFileStream = File.OpenRead(zipPathAndFile))
                using (ZipInputStream zipInputStream = new(zipFileStream))
                {
                    while (true)
                    {
                        var entry = zipInputStream.GetNextEntry();

                        if (entry is null)
                        {
                            break;
                        }

                        var fileName = Path.GetFileName(entry.Name);

                        if (fileName == string.Empty || entry.Name.Contains(".ini"))
                        {
                            continue;
                        }

                        var fullPath = Path.Combine(outputFolder, entry.Name).Replace("\\ ", "\\");
                        var fullDirPath = Path.GetDirectoryName(fullPath);
                        if (fullDirPath is not null && !Directory.Exists(fullDirPath))
                        {
                            Directory.CreateDirectory(fullDirPath);
                        }

                        using var fileStream = File.Create(fullPath);
                        zipInputStream.CopyTo(fileStream);
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

        private static int DownloadFile(string remoteFilename, string localFilename)
        {
            // Function will return the number of bytes processed
            // to the caller. Initialize to 0 here.
            var bytesProcessed = 0;

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream? localStream = null;

            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            try
            {
#pragma warning disable SYSLIB0014 // 'WebClient' is obsolete
                using WebClient webClient = new() { Proxy = WebRequest.DefaultWebProxy };
#pragma warning restore SYSLIB0014 // 'WebClient' is obsolete
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

                // Once the WebResponse object has been retrieved,
                // get the stream object associated with the response's data
                var remoteStream = webClient.OpenRead(remoteFilename);

                // Create the local file
                localStream = File.Create(localFilename);

                // Allocate a 1k buffer
                var buffer = new byte[1024];
                int bytesRead;

                // Simple do/while loop to read from stream until
                // no bytes are returned
                do
                {
                    // Read data (up to 1k) from the stream
                    bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    // Write the data to the local file
                    localStream.Write(buffer, 0, bytesRead);

                    // Increment total bytes processed
                    bytesProcessed += bytesRead;
                }
                while (bytesRead > 0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Close the response and streams objects here
                // to make sure they're closed even if an exception
                // is thrown at some point
                localStream?.Close();
            }

            // Return total bytes processed to caller.
            return bytesProcessed;
        }

        private static string SearchForGourceUrl()
        {
            try
            {
#pragma warning disable SYSLIB0014 // 'WebClient' is obsolete
                using WebClient webClient = new() { Proxy = WebRequest.DefaultWebProxy };
#pragma warning restore SYSLIB0014 // 'WebClient' is obsolete
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                webClient.Encoding = Encoding.UTF8;

                var response = webClient.DownloadString(@"https://github.com/acaudwell/Gource/releases/latest");

                // find http://gource.googlecode.com/files/gource-0.26b.win32.zip
                // find http://gource.googlecode.com/files/gource-0.34-rc2.win32.zip
                Regex regEx = new(@"(?:<a .*href="")(.*gource-.{3,15}win32\.zip)""");

                var matches = regEx.Matches(response);

                foreach (Match match in matches)
                {
                    return "https://github.com" + match.Groups[1].Value;
                }

                response = webClient.DownloadString(@"https://github.com/acaudwell/Gource/releases/tag/gource-0.42");

                regEx = new Regex(@"(?:<a .*href="")(.*gource-.{3,15}win32\.zip)""");

                matches = regEx.Matches(response);

                foreach (Match match in matches)
                {
                    return "https://github.com" + match.Groups[1].Value;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }
    }
}
