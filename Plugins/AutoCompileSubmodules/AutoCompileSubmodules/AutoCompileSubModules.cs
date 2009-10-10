using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitUIPluginInterfaces;

namespace AutoCompileSubmodules
{
    public class AutoCompileSubModules : IGitPlugin
    {
        //Description of the plugin
        public string Description 
        {
            get
            {
                return "Auto compile SubModules";
            }
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

        private string FindMsBuild()
        {
            if (File.Exists(@"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe"))
                return @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe";

            return "";
        }

        public void Register(IGitUICommands gitUICommands)
        {
            //Register settings
            Settings.AddSetting("Enabled (true / false)", "false");
            Settings.AddSetting("Path to msbuild.exe", FindMsBuild());
            Settings.AddSetting("msbuild.exe arguments", "/p:Configuration=Debug");

            //Connect to events
            gitUICommands.PostUpdateSubmodules += new GitUIEventHandler(gitUICommands_PostUpdateSubmodules);
            gitUICommands.PostUpdateSubmodulesRecursive += new GitUIEventHandler(gitUICommands_PostUpdateSubmodulesRecursive);
        }

        void gitUICommands_PostUpdateSubmodulesRecursive(IGitUIEventArgs e)
        {
            if (Settings.GetSetting("Enabled (true / false)").Equals("true", StringComparison.InvariantCultureIgnoreCase))
                Execute(e);
        }


        /// <summary>
        /// Automaticly compile all solution files found in any submodule
        /// </summary>
        void gitUICommands_PostUpdateSubmodules(IGitUIEventArgs e)
        {
            if (Settings.GetSetting("Enabled (true / false)").Equals("true", StringComparison.InvariantCultureIgnoreCase))
                Execute(e);
        }

        public void Execute(IGitUIEventArgs e)
        {
            //Only build when plugin is enabled
            if (!string.IsNullOrEmpty(e.GitWorkingDir))
            {
                string arguments = Settings.GetSetting("msbuild.exe arguments");
                string msbuildpath = Settings.GetSetting("Path to msbuild.exe");

                DirectoryInfo workingDir = new DirectoryInfo(e.GitWorkingDir);
                FileInfo[] solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

                for (int n = solutionFiles.Length -1; n > 0; n--)
                {
                    FileInfo solutionFile = solutionFiles[n];

                    DialogResult result = MessageBox.Show("Do you want to build " + solutionFile.Name + "?\n\n" + SolutionFilesToString(solutionFiles), "Build", MessageBoxButtons.YesNoCancel);

                    if (result == DialogResult.Cancel)
                        return;

                    if (result == DialogResult.Yes)
                    {
                        if (string.IsNullOrEmpty(msbuildpath) || !File.Exists(msbuildpath))
                            MessageBox.Show("Please enter correct MSBuild path in the plugin settings dialog and try again.");
                        else
                            e.GitUICommands.StartCommandLineProcessDialog(msbuildpath, solutionFile.FullName + " " + arguments);
                    }
                }
            }
        }

        private string SolutionFilesToString(FileInfo[] solutionFiles)
        {
            StringBuilder solutionString = new StringBuilder();
            for (int n = solutionFiles.Length - 1; n > 0; n--)
            {
                FileInfo solutionFile = solutionFiles[n]; 
                solutionString.Append(solutionFile.Name);
                solutionString.Append("\n");
            }
            return solutionString.ToString();
        }
    }
}
