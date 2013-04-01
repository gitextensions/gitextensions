using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace AutoCompileSubmodules
{
    public class AutoCompileSubModules : GitPluginBase, IGitPluginForRepository
    {
        private const string MsBuildPath = @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe";

        #region IGitPlugin Members

        /// <summary>
        ///   Gets the plugin description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Auto compile SubModules"; }
        }

        protected override void RegisterSettings()
        {
            base.RegisterSettings();
            Settings.AddSetting("Enabled (true / false)", "false");
            Settings.AddSetting("Path to msbuild.exe", FindMsBuild());
            Settings.AddSetting("msbuild.exe arguments", "/p:Configuration=Debug");
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            // Connect to events
            gitUiCommands.PostUpdateSubmodules += GitUiCommandsPostUpdateSubmodules;
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            // Connect to events
            gitUiCommands.PostUpdateSubmodules -= GitUiCommandsPostUpdateSubmodules;
        }

        public override bool Execute(GitUIBaseEventArgs e)
        {
            // Only build when plugin is enabled
            if (string.IsNullOrEmpty(e.GitModule.GitWorkingDir))
                return false;

            var arguments = Settings.GetSetting("msbuild.exe arguments");
            var msbuildpath = Settings.GetSetting("Path to msbuild.exe");

            var workingDir = new DirectoryInfo(e.GitModule.GitWorkingDir);
            var solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

            for (var n = solutionFiles.Length - 1; n > 0; n--)
            {
                var solutionFile = solutionFiles[n];

                var result =
                    MessageBox.Show(e.OwnerForm as IWin32Window,
                        string.Format("Do you want to build {0}?\n\n{1}",
                                      solutionFile.Name, 
                                      SolutionFilesToString(solutionFiles)),
                        "Build", 
                        MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Cancel)
                    return false;

                if (result != DialogResult.Yes)
                    continue;

                if (string.IsNullOrEmpty(msbuildpath) || !File.Exists(msbuildpath))
                    MessageBox.Show(e.OwnerForm as IWin32Window, "Please enter correct MSBuild path in the plugin settings dialog and try again.");
                else
                    e.GitUICommands.StartCommandLineProcessDialog(e.OwnerForm as IWin32Window, msbuildpath, solutionFile.FullName + " " + arguments);
            }
            return false;
        }

        #endregion

        private static string FindMsBuild()
        {
            return File.Exists(MsBuildPath) ? MsBuildPath : "";
        }

        /// <summary>
        ///   Automaticly compile all solution files found in any submodule
        /// </summary>
        private void GitUiCommandsPostUpdateSubmodules(object sender, GitUIPostActionEventArgs e)
        {
            if (e.ActionDone && Settings.GetSetting("Enabled (true / false)")
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