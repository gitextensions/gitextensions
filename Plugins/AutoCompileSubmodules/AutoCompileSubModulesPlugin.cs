using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AutoCompileSubmodules.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace AutoCompileSubmodules
{
    [Export(typeof(IGitPlugin))]
    public class AutoCompileSubModulesPlugin : GitPluginBase, IGitPluginForRepository
    {
        private readonly TranslationString _doYouWantBuild =
            new TranslationString("Do you want to build {0}?\n\n{1}");
        private readonly TranslationString _enterCorrectMsBuildPath =
            new TranslationString("Please enter correct MSBuild path in the plugin settings dialog and try again.");

        public AutoCompileSubModulesPlugin()
        {
            SetNameAndDescription("Auto compile SubModules");
            Translate();
            Icon = Resources.IconAutoCompileSubmodules;
        }

        private readonly BoolSetting _msBuildEnabled = new BoolSetting("Enabled", false);
        private readonly StringSetting _msBuildPath = new StringSetting("Path to msbuild.exe", FindMsBuild());
        private readonly StringSetting _msBuildArguments = new StringSetting("msbuild.exe arguments", "/p:Configuration=Debug");

        private const string DefaultMsBuildPath = @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe";

        private static string FindMsBuild()
        {
            return File.Exists(DefaultMsBuildPath) ? DefaultMsBuildPath : "";
        }

        #region IGitPlugin Members

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return _msBuildEnabled;
            yield return _msBuildPath;
            yield return _msBuildArguments;
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

        public override bool Execute(GitUIEventArgs args)
        {
            // Only build when plugin is enabled
            if (string.IsNullOrEmpty(args.GitModule.WorkingDir))
            {
                return false;
            }

            var msbuildPath = _msBuildPath.ValueOrDefault(Settings);

            var workingDir = new DirectoryInfo(args.GitModule.WorkingDir);
            var solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

            for (var n = solutionFiles.Length - 1; n > 0; n--)
            {
                var solutionFile = solutionFiles[n];

                var result =
                    MessageBox.Show(args.OwnerForm,
                        string.Format(_doYouWantBuild.Text,
                                      solutionFile.Name,
                                      SolutionFilesToString(solutionFiles)),
                        "Build",
                        MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Cancel)
                {
                    return false;
                }

                if (result != DialogResult.Yes)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(msbuildPath) || !File.Exists(msbuildPath))
                {
                    MessageBox.Show(args.OwnerForm, _enterCorrectMsBuildPath.Text);
                }
                else
                {
                    args.GitUICommands.StartCommandLineProcessDialog(args.OwnerForm, msbuildPath, solutionFile.FullName + " " + _msBuildArguments.ValueOrDefault(Settings));
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        ///   Automatically compile all solution files found in any submodule
        /// </summary>
        private void GitUiCommandsPostUpdateSubmodules(object sender, GitUIPostActionEventArgs e)
        {
            if (e.ActionDone && _msBuildEnabled.ValueOrDefault(Settings))
            {
                Execute(e);
            }
        }

        private static string SolutionFilesToString(IReadOnlyList<FileInfo> solutionFiles)
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