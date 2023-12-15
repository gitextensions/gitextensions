using System.ComponentModel.Composition;
using System.Text;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.AutoCompileSubmodules.Properties;
using ResourceManager;

namespace GitExtensions.Plugins.AutoCompileSubmodules
{
    [Export(typeof(IGitPlugin))]
    public class AutoCompileSubModulesPlugin : GitPluginBase, IGitPluginForRepository
    {
        private readonly TranslationString _doYouWantBuild =
            new("Do you want to build {0}?\n\n{1}");
        private readonly TranslationString _enterCorrectMsBuildPath =
            new("Please enter correct MSBuild path in the plugin settings dialog and try again.");

        public AutoCompileSubModulesPlugin() : base(true)
        {
            Id = new Guid("D4D1ACB7-0B6B-4A3C-B0DB-A25056A277D9");
            Name = "Auto compile submodules";
            Translate(AppSettings.CurrentTranslation);
            Icon = Resources.IconAutoCompileSubmodules;
        }

        private readonly BoolSetting _msBuildEnabled = new("Enabled", false);
        private readonly StringSetting _msBuildPath = new("Path to msbuild.exe", FindMsBuild());
        private readonly StringSetting _msBuildArguments = new("msbuild.exe arguments", "/p:Configuration=Debug");

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

            string msbuildPath = _msBuildPath.ValueOrDefault(Settings);

            DirectoryInfo workingDir = new(args.GitModule.WorkingDir);
            FileInfo[] solutionFiles = workingDir.GetFiles("*.sln", SearchOption.AllDirectories);

            for (int n = solutionFiles.Length - 1; n > 0; n--)
            {
                FileInfo solutionFile = solutionFiles[n];

                DialogResult result =
                    MessageBox.Show(args.OwnerForm,
                        string.Format(_doYouWantBuild.Text,
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
                    MessageBox.Show(args.OwnerForm, _enterCorrectMsBuildPath.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            StringBuilder solutionString = new();

            for (int n = solutionFiles.Count - 1; n > 0; n--)
            {
                FileInfo solutionFile = solutionFiles[n];
                solutionString.Append(solutionFile.Name);
                solutionString.Append("\n");
            }

            return solutionString.ToString();
        }
    }
}
