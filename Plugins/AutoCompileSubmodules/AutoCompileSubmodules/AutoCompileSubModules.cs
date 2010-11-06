using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace AutoCompileSubmodules
{
    public class AutoCompileSubModules : IGitPlugin
    {
        private const string MsBuildPath = @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe";

        #region IGitPlugin Members

        /// <summary>
        ///   Gets the plugin description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return "Auto compile SubModules"; }
        }

        // Store settings to use later
        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            // Register settings
            Settings.AddSetting("Enabled (true / false)", "false");
            Settings.AddSetting("Path to msbuild.exe", FindMsBuild());
            Settings.AddSetting("msbuild.exe arguments", "/p:Configuration=Debug");

            // Connect to events
            gitUiCommands.PostUpdateSubmodules += GitUiCommandsPostUpdateSubmodules;
            gitUiCommands.PostUpdateSubmodulesRecursive += GitUiCommandsPostUpdateSubmodulesRecursive;
        }

        public void Execute(GitUIBaseEventArgs e)
        {
            // Only build when plugin is enabled
            if (string.IsNullOrEmpty(e.GitWorkingDir))
                return;

            var arguments = Settings.GetSetting("msbuild.exe arguments");
            var msbuildpath = Settings.GetSetting("Path to msbuild.exe");

            var workingDir = new DirectoryInfo(e.GitWorkingDir);
            var solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

            for (var n = solutionFiles.Length - 1; n > 0; n--)
            {
                var solutionFile = solutionFiles[n];

                var result =
                    MessageBox.Show(
                        string.Format("Do you want to build {0}?\n\n{1}",
                                      solutionFile.Name, 
                                      SolutionFilesToString(solutionFiles)),
                        "Build", 
                        MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Cancel)
                    return;

                if (result != DialogResult.Yes)
                    continue;

                if (string.IsNullOrEmpty(msbuildpath) || !File.Exists(msbuildpath))
                    MessageBox.Show("Please enter correct MSBuild path in the plugin settings dialog and try again.");
                else
                    e.GitUICommands.StartCommandLineProcessDialog(msbuildpath, solutionFile.FullName + " " + arguments);
            }
        }

        #endregion

        private static string FindMsBuild()
        {
            return File.Exists(MsBuildPath) ? MsBuildPath : "";
        }

        private void GitUiCommandsPostUpdateSubmodulesRecursive(object sender, GitUIBaseEventArgs e)
        {
            if (Settings.GetSetting("Enabled (true / false)")
                .Equals("true", StringComparison.InvariantCultureIgnoreCase))
                Execute(e);
        }


        /// <summary>
        ///   Automaticly compile all solution files found in any submodule
        /// </summary>
        private void GitUiCommandsPostUpdateSubmodules(object sender, GitUIBaseEventArgs e)
        {
            if (Settings.GetSetting("Enabled (true / false)")
                .Equals("true", StringComparison.InvariantCultureIgnoreCase))
                Execute(e);
        }

        private static string SolutionFilesToString(IList<FileInfo> solutionFiles)
        {
            var solutionString = new StringBuilder();
            for (var n = solutionFiles.Count - 1; n > 0; n--)
            {
                var solutionFile = solutionFiles[n];
                solutionString.Append(solutionFile.Name);
                solutionString.Append("\n");
            }
            return solutionString.ToString();
        }
    }
}