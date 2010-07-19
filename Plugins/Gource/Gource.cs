using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using ICSharpCode.SharpZipLib.Zip;

namespace Gource
{
    public class Gource : IGitPlugin
    {
        #region IGitPlugin Members

        public string Description
        {
            get { return "gource"; }
        }

        //Store settings to use later
        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            //Register settings
            Settings.AddSetting("Path to \"gource\"", "");
            Settings.AddSetting("Arguments", "--hide filenames");
        }

        public void Execute(IGitUIEventArgs gitUiCommands)
        {
            if (!gitUiCommands.IsValidGitWorkingDir(gitUiCommands.GitWorkingDir))
            {
                MessageBox.Show("The current directory is not a valid git repository." + Environment.NewLine +
                                Environment.NewLine + "Gource can be only be started from a valid git repository.");
                return;
            }

            var pathToGource = Settings.GetSetting("Path to \"gource\"");

            if (!string.IsNullOrEmpty(pathToGource))
            {
                if (!File.Exists(pathToGource))
                {
                    if (
                        MessageBox.Show(
                            "Cannot find \"gource\" in the configured path: " + pathToGource +
                            ".\n\n.Do you want to reset the configured path?", "Gource", MessageBoxButtons.YesNo) ==
                        DialogResult.Yes)
                    {
                        Settings.SetSetting("Path to \"gource\"", "");
                        pathToGource = Settings.GetSetting("Path to \"gource\"");
                    }
                }
            }

            if (string.IsNullOrEmpty(pathToGource))
            {
                if (
                    MessageBox.Show(
                        "There is no path to \"gource\" configured.\n\nDo you want to automaticly download \"gource\"?",
                        "Download", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var gourceUrl = SearchForGourceUrl();

                    if (string.IsNullOrEmpty(gourceUrl))
                    {
                        MessageBox.Show(
                            "Cannot find \"gource\".\nPlease download \"gource\" and set the path in the plugins settings dialog.");
                        return;
                    }
                    var downloadDir = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                    var fileName = downloadDir + "\\gource.zip";
                    var downloadSize = DownloadFile(gourceUrl, fileName);
                    if (downloadSize > 0)
                    {
                        MessageBox.Show(downloadSize + " bytes downloaded.");
                        Directory.CreateDirectory(downloadDir + "\\gource");
                        UnZipFiles(fileName, downloadDir + "\\gource", true);

                        var newGourcePath = downloadDir + "\\gource\\gource.exe";
                        if (File.Exists(newGourcePath))
                        {
                            Settings.SetSetting("Path to \"gource\"", newGourcePath);
                            MessageBox.Show("\"gource\" has been downloaded and unzipped.");
                            pathToGource = newGourcePath;
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Downloading failed.\nPlease download \"gource\" and set the path in the plugins settings dialog.");
                    }
                }
            }

            var gourceStart = new GourceStart(pathToGource, gitUiCommands.GitWorkingDir,
                                              Settings.GetSetting("Arguments"));
            gourceStart.ShowDialog();

            Settings.SetSetting("Arguments", gourceStart.GourceArguments);
            Settings.SetSetting("Path to \"gource\"", gourceStart.PathToGource);
        }

        #endregion

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

                    var fullPath = directoryName + "\\" + theEntry.Name;
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
                var webClient = new WebClient {Proxy = WebRequest.DefaultWebProxy};
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
                if (localStream != null) localStream.Close();
            }

            // Return total bytes processed to caller.
            return bytesProcessed;
        }


        private static string SearchForGourceUrl()
        {
            try
            {
                var webClient = new WebClient {Proxy = WebRequest.DefaultWebProxy};
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                webClient.Encoding = Encoding.UTF8;

                var response = webClient.DownloadString(@"http://code.google.com/p/gource/");

                //find http://gource.googlecode.com/files/gource-0.26b.win32.zip
                var regEx = new Regex(@"gource-.*\.win32\.zip");


                var matches = regEx.Matches(response);

                foreach (Match match in matches)
                {
                    return "http://gource.googlecode.com/files/" + match.Value;
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