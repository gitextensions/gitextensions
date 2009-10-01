using System;
using System.Collections.Generic;
using System.Text;
using GitUI;
using System.Windows.Forms;
using System.IO;

namespace AutoCompileSubmodules
{
    public class AutoCompileSubModules : IGitPlugin
    {
        //Description of the submodule
        public string Description 
        {
            get
            {
                return "Auto compile SubModules";
            }
        }

        //Store settings to use later
        private IGitPluginSettingsContainer Settings;

        private string FindMsBuild()
        {
            if (File.Exists(@"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe"))
                return @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe";

            return "";
        }

        public void Register(GitUICommands gitUICommands, IGitPluginSettingsContainer settings)
        {
            //Register settings
            settings.AddSetting("MsBuild path", FindMsBuild());
            settings.AddSetting("MsBuild arguments", "/p:Configuration=Debug");
            Settings = settings;

            //Connect to events
            gitUICommands.PostUpdateSubmodules += new GitUIEventHandler(gitUICommands_PostUpdateSubmodules);
        }


        /// <summary>
        /// Automaticly compile all solution files found in any submodule
        /// </summary>
        void gitUICommands_PostUpdateSubmodules(GitUIEventArgs e)
        {
            Execute(e);
        }

        public void Execute(GitUIEventArgs e)
        {
            DirectoryInfo workingDir = new DirectoryInfo(e.GitWorkingDir);
            DirectoryInfo[] repositories = workingDir.GetDirectories(".git", SearchOption.AllDirectories);

            string arguments = Settings.GetSetting("MsBuild arguments");
            string msbuildpath = Settings.GetSetting("MsBuild path");

            //Loop through all .git directories
            foreach (DirectoryInfo repositoryDirectory in repositories)
            {
                //When the parent dir is not equal to the current working dir, it is a submodule
                if (repositoryDirectory.Parent != workingDir)
                {

                    FileInfo[] solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

                    foreach (FileInfo solutionFile in solutionFiles)
                    {
                        if (MessageBox.Show("Do you want to build " + solutionFile.Name + "?", "Build", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            if (string.IsNullOrEmpty(msbuildpath) || !File.Exists(msbuildpath))
                                MessageBox.Show("Please enter correct MSBuild path and try again.");
                            else
                                e.GitUICommands.StartCommandLineProcessDialog(msbuildpath, solutionFile.FullName + " " + arguments);
                        }
                    }
                }
            }
        }
    }
}
