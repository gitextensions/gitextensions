using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AutoCompileSubmodules.Properties;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Events;
using GitExtensions.Extensibility.Settings;

namespace AutoCompileSubmodules
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginConfigurable,
        IGitPluginExecutable,
        IPostUpdateSubmodulesHandler
    {
        private readonly BoolSetting _msBuildEnabled = new BoolSetting("Enabled", Strings.MsBuildEnabled, false);
        private readonly StringSetting _msBuildPath = new StringSetting("Path to msbuild.exe", Strings.MsBuildPath, FindMsBuild());
        private readonly StringSetting _msBuildArguments = new StringSetting("msbuild.exe arguments", Strings.MsBuildArguments, "/p:Configuration=Debug");

        private const string DefaultMsBuildPath = @"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe";

        public string Name => "Auto compile SubModules";

        public string Description => Strings.Description;

        public Image Icon => Images.IconAutoCompileSubmodules;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public IEnumerable<ISetting> GetSettings()
        {
            yield return _msBuildEnabled;
            yield return _msBuildPath;
            yield return _msBuildArguments;
        }

        public bool Execute(GitUIEventArgs args)
        {
            // Only build when plugin is enabled
            if (string.IsNullOrEmpty(args.GitModule.WorkingDir))
            {
                return false;
            }

            var msbuildPath = _msBuildPath.ValueOrDefault(SettingsContainer.GetSettingsSource());

            var workingDir = new DirectoryInfo(args.GitModule.WorkingDir);
            var solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

            for (var n = solutionFiles.Length - 1; n > 0; n--)
            {
                var solutionFile = solutionFiles[n];

                var result =
                    MessageBox.Show(args.OwnerForm,
                        string.Format(Strings.DoYouWantBuild,
                                      solutionFile.Name,
                                      SolutionFilesToString(solutionFiles)),
                        "Build",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

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
                    MessageBox.Show(args.OwnerForm, Strings.EnterCorrectMsBuildPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    args.GitUICommands.StartCommandLineProcessDialog(args.OwnerForm, msbuildPath, solutionFile.FullName + " " + _msBuildArguments.ValueOrDefault(SettingsContainer.GetSettingsSource()));
                }
            }

            return false;
        }

        /// <summary>
        ///   Automatically compile all solution files found in any submodule
        /// </summary>
        public void OnPostUpdateSubmodules(GitUIPostActionEventArgs e)
        {
            if (e.ActionDone && _msBuildEnabled.ValueOrDefault(SettingsContainer.GetSettingsSource()))
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

        private static string FindMsBuild()
        {
            return File.Exists(DefaultMsBuildPath) ? DefaultMsBuildPath : string.Empty;
        }
    }
}
