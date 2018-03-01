using GitUIPluginInterfaces;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ResourceManager;

namespace Gource
{
    public class GourcePlugin : GitPluginBase, IGitPluginForRepository
    {
        #region Translation
        private readonly TranslationString _currentDirectoryIsNotValidGit = new TranslationString("The current directory is not a valid git repository.\n\n" +
            "Gource can be only be started from a valid git repository.");
        private readonly TranslationString _resetConfigPath = new TranslationString("Cannot find Gource in the configured path: {0}.\n\n" +
            "Do you want to reset the configured path?");
        private readonly TranslationString _gource = new TranslationString("Gource");
        private readonly TranslationString _doYouWantDownloadGource = new TranslationString("There is no path to Gource configured.\n\n" +
            "Do you want to automatically download Gource?");
        private readonly TranslationString _download = new TranslationString("Download");
        private readonly TranslationString _cannotFindGource = new TranslationString("Cannot find Gource.\n" +
            "Please download Gource and set the path in the plugins settings dialog.");
        private readonly TranslationString _bytesDownloaded = new TranslationString("{0} bytes downloaded.");
        private readonly TranslationString _gourceDownloadedAndUnzipped = new TranslationString("Gource has been downloaded and unzipped.");
        private readonly TranslationString _downloadingFailed = new TranslationString("Downloading failed.\n" +
            "Please download Gource and set the path in the plugins settings dialog.");
        #endregion

        public GourcePlugin()
        {
            SetNameAndDescription("Gource");
            Translate();
        }

        private StringSetting GourcePath = new StringSetting("Path to Gource", "");
        private StringSetting GourceArguments = new StringSetting("Arguments", "--hide filenames --user-image-dir \"$(AVATARS)\"");

        #region IGitPlugin Members

        public override IEnumerable<ISetting> GetSettings()
        {
            // return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return GourcePath;
            yield return GourceArguments;
        }

        public override bool Execute(GitUIBaseEventArgs eventArgs)
        {
            IGitModule gitUiCommands = eventArgs.GitModule;
            var ownerForm = eventArgs.OwnerForm as IWin32Window;
            if (!gitUiCommands.IsValidGitWorkingDir())
            {
                MessageBox.Show(ownerForm, _currentDirectoryIsNotValidGit.Text);
                return false;
            }

            var pathToGource = GourcePath.ValueOrDefault(Settings);

            if (!string.IsNullOrEmpty(pathToGource))
            {
                if (!File.Exists(pathToGource))
                {
                    if (MessageBox.Show(ownerForm,
                            string.Format(_resetConfigPath.Text, pathToGource), _gource.Text, MessageBoxButtons.YesNo) ==
                        DialogResult.Yes)
                    {
                        Settings.SetValue<string>(GourcePath.Name, GourcePath.DefaultValue, s => s);
                        pathToGource = GourcePath.DefaultValue;
                    }
                }
            }

            if (string.IsNullOrEmpty(pathToGource))
            {
                if (MessageBox.Show(ownerForm, _doYouWantDownloadGource.Text, _download.Text,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var gourceUrl = SearchForGourceUrl();

                    if (string.IsNullOrEmpty(gourceUrl))
                    {
                        MessageBox.Show(ownerForm, _cannotFindGource.Text);
                        return false;
                    }
                    var downloadDir = Path.GetTempPath();
                    var fileName = Path.Combine(downloadDir, "gource.zip");
                    var downloadSize = DownloadFile(gourceUrl, fileName);
                    if (downloadSize > 0)
                    {
                        MessageBox.Show(string.Format(_bytesDownloaded.Text, downloadSize));
                        Directory.CreateDirectory(Path.Combine(downloadDir, "gource"));
                        UnZipFiles(fileName, Path.Combine(downloadDir, "gource"), true);

                        var newGourcePath = Path.Combine(downloadDir, "gource\\gource.exe");
                        if (File.Exists(newGourcePath))
                        {
                            MessageBox.Show(ownerForm, _gourceDownloadedAndUnzipped.Text);
                            pathToGource = newGourcePath;
                        }
                    }
                    else
                    {
                        MessageBox.Show(ownerForm, _downloadingFailed.Text);
                    }
                }
            }

            using (var gourceStart = new GourceStart(pathToGource, eventArgs, GourceArguments.ValueOrDefault(Settings)))
            {
                gourceStart.ShowDialog(ownerForm);
                Settings.SetValue<string>(GourceArguments.Name, gourceStart.GourceArguments, s => s);
                Settings.SetValue<string>(GourcePath.Name, gourceStart.PathToGource, s => s);
            }
            return true;
        }

        #endregion IGitPlugin Members

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, bool deleteZipFile)
        {
            try
            {
                var s = new ZipInputStream(File.OpenRead(zipPathAndFile));

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    var directoryName = outputFolder;
                    var fileName = Path.GetFileName(theEntry.Name);
                    // create directory
                    if (directoryName != "")
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    if (fileName == String.Empty || theEntry.Name.IndexOf(".ini") >= 0)
                        continue;

                    var fullPath = Path.Combine(directoryName, theEntry.Name);
                    fullPath = fullPath.Replace("\\ ", "\\");
                    var fullDirPath = Path.GetDirectoryName(fullPath);
                    if (fullDirPath != null && !Directory.Exists(fullDirPath))
                        Directory.CreateDirectory(fullDirPath);
                    var streamWriter = File.Create(fullPath);
                    var data = new byte[2048];
                    while (true)
                    {
                        var size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
                s.Close();
                if (deleteZipFile)
                    File.Delete(zipPathAndFile);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public int DownloadFile(String remoteFilename, String localFilename)
        {
            // Function will return the number of bytes processed
            // to the caller. Initialize to 0 here.
            var bytesProcessed = 0;

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream localStream = null;

            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            try
            {
                var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy };
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
                } while (bytesRead > 0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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
                var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy };
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                webClient.Encoding = Encoding.UTF8;

                var response = webClient.DownloadString(@"https://github.com/acaudwell/Gource/releases/latest");

                // find http://gource.googlecode.com/files/gource-0.26b.win32.zip
                // find http://gource.googlecode.com/files/gource-0.34-rc2.win32.zip
                var regEx = new Regex(@"(?:<a .*href="")(.*gource-.{3,15}win32\.zip)""");

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
                MessageBox.Show(ex.Message, "Exception");
                return string.Empty;
            }
        }
    }
}
