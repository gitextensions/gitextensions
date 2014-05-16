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
        private BoolSetting MsBuildEnabled = new BoolSetting("Enabled", false);
        private StringSetting MsBuildPath = new StringSetting("Path to msbuild.exe", FindMsBuild());
        private StringSetting MsBuildArguments = new StringSetting("msbuild.exe arguments", "/p:Configuration=Debug");
        
        private const string DefaultMsBuildPath = @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe";
        private static string FindMsBuild()
        {
            return File.Exists(DefaultMsBuildPath) ? DefaultMsBuildPath : "";
        }

        #region IGitPlugin Members

        /// <summary>
        ///   Gets the plugin description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Auto compile SubModules"; }
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return MsBuildEnabled;
            yield return MsBuildPath;
            yield return MsBuildArguments;
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
            if (string.IsNullOrEmpty(e.GitModule.WorkingDir))
                return false;

            var msbuildpath = MsBuildPath[Settings];

            var workingDir = new DirectoryInfo(e.GitModule.WorkingDir);
            var solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

            for (var n = solutionFiles.Length - 1; n > 0; n--)
            {
                var solutionFile = solutionFiles[n];

                var result =
                    MessageBox.Show(e.OwnerForm,
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
                    MessageBox.Show(e.OwnerForm, "Please enter correct MSBuild path in the plugin settings dialog and try again.");
                else
                    e.GitUICommands.StartCommandLineProcessDialog(e.OwnerForm, msbuildpath, solutionFile.FullName + " " + MsBuildArguments[Settings]);
            }
            return false;
        }

        #endregion

        /// <summary>
        ///   Automaticly compile all solution files found in any submodule
        /// </summary>
        private void GitUiCommandsPostUpdateSubmodules(object sender, GitUIPostActionEventArgs e)
        {
            if (e.ActionDone && MsBuildEnabled[Settings].Value)
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