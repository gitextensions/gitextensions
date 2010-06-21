using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

namespace Gource
{
    public class Gource : IGitPlugin
    {
        public string Description
        {
            get { return "gource"; }
        }

        //Store settings to use later
        private IGitPluginSettingsContainer settings;
        public IGitPluginSettingsContainer Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        public void Register(IGitUICommands gitUICommands)
        {
            //Register settings
            Settings.AddSetting("Path to \"gource\"", "");
            Settings.AddSetting("Arguments", "--hide filenames");
        }

        public void Execute(IGitUIEventArgs gitUICommands)
        {
            if (!gitUICommands.IsValidGitWorkingDir(gitUICommands.GitWorkingDir))
            {
                MessageBox.Show("The current directory is not a valid git repository." + Environment.NewLine + Environment.NewLine + "Gource can be only be started from a valid git repository.");
                return;
            }

            string pathToGource = Settings.GetSetting("Path to \"gource\"");

            if (!string.IsNullOrEmpty(pathToGource))
            {
                if (!File.Exists(pathToGource))
                {
                    if (MessageBox.Show("Cannot find \"gource\" in the configured path: " + pathToGource + ".\n\n.Do you want to reset the configured path?", "Gource", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Settings.SetSetting("Path to \"gource\"", "");
                        pathToGource = Settings.GetSetting("Path to \"gource\"");
                    }
                    else
                    {
                    }
                }
            }

            if (string.IsNullOrEmpty(pathToGource))
            {
                if (MessageBox.Show("There is no path to \"gource\" configured.\n\nDo you want to automaticly download \"gource\"?", "Download", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string gourceUrl = SearchForGourceUrl();

                    if (string.IsNullOrEmpty(gourceUrl))
                    {
                        MessageBox.Show("Cannot find \"gource\".\nPlease download \"gource\" and set the path in the plugins settings dialog.");
                        return;
                    }
                    string downloadDir = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                    string fileName = downloadDir + "\\gource.zip";
                    int downloadSize = DownloadFile(gourceUrl, fileName);
                    if (downloadSize > 0)
                    {
                        MessageBox.Show(downloadSize.ToString() + " bytes downloaded.");
                        Directory.CreateDirectory(downloadDir + "\\gource");
                        UnZipFiles(fileName, downloadDir + "\\gource", true);

                        string newGourcePath = downloadDir + "\\gource\\gource.exe";
                        if (File.Exists(newGourcePath))
                        {
                            Settings.SetSetting("Path to \"gource\"", newGourcePath);
                            MessageBox.Show("\"gource\" has been downloaded and unzipped.");
                            pathToGource = newGourcePath;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Downloading failed.\nPlease download \"gource\" and set the path in the plugins settings dialog.");
                    }

                }
            }

            GourceStart gourceStart = new GourceStart(pathToGource, gitUICommands.GitWorkingDir, Settings.GetSetting("Arguments"));
            gourceStart.ShowDialog();

            Settings.SetSetting("Arguments", gourceStart.GourceArguments);
            Settings.SetSetting("Path to \"gource\"", gourceStart.PathToGource);
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, bool deleteZipFile)
        {
            try
            {
                ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));

                ZipEntry theEntry;
                string tmpEntry = String.Empty;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = outputFolder;
                    string fileName = Path.GetFileName(theEntry.Name);
                    // create directory 
                    if (directoryName != "")
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    if (fileName != String.Empty)
                    {
                        if (theEntry.Name.IndexOf(".ini") < 0)
                        {
                            string fullPath = directoryName + "\\" + theEntry.Name;
                            fullPath = fullPath.Replace("\\ ", "\\");
                            string fullDirPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                            FileStream streamWriter = File.Create(fullPath);
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
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
                    }
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


        public int DownloadFile(String remoteFilename,
                                       String localFilename)
        {
            // Function will return the number of bytes processed
            // to the caller. Initialize to 0 here.
            int bytesProcessed = 0;

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream localStream = null;

            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            try
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = WebRequest.DefaultWebProxy;
                webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                // Once the WebResponse object has been retrieved,
                // get the stream object associated with the response's data
                Stream remoteStream = webClient.OpenRead(remoteFilename);

                // Create the local file
                localStream = File.Create(localFilename);

                // Allocate a 1k buffer
                byte[] buffer = new byte[1024];
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


        private string SearchForGourceUrl()
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = WebRequest.DefaultWebProxy;
                webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                webClient.Encoding = System.Text.Encoding.UTF8;

                string response = webClient.DownloadString(@"http://code.google.com/p/gource/");

                //find http://gource.googlecode.com/files/gource-0.26b.win32.zip
                Regex regEx = new Regex(@"gource-.*\.win32\.zip");


                MatchCollection matches = regEx.Matches(response);

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
