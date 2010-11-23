using System;
using System.IO;
using GitUIPluginInterfaces;

namespace GitStatistics
{
    public class GitStatisticsPlugin : IGitPlugin
    {
        #region IGitPlugin Members

        public string Description
        {
            get { return "Statistics"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            Settings.AddSetting("Code files",
                                "*.c;*.cpp;*.h;*.hpp;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav");
            Settings.AddSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
            Settings.AddSetting("Ignore submodules (true/false)", "true");
        }

        public void Execute(GitUIBaseEventArgs gitUiCommands)
        {
            if (string.IsNullOrEmpty(gitUiCommands.GitWorkingDir))
                return;

            var formGitStatistics =
                new FormGitStatistics(Settings.GetSetting("Code files"))
                    {
                        DirectoriesToIgnore =
                            Settings.GetSetting("Directories to ignore (EndsWith)")
                    };

            if (Settings.GetSetting("Ignore submodules (true/false)")
                .Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var submodule in gitUiCommands.GitCommands.GetSubmodules())
                {
                    formGitStatistics.DirectoriesToIgnore += ";";
                    formGitStatistics.DirectoriesToIgnore += gitUiCommands.GitWorkingDir + submodule.LocalPath;
                }
            }

            formGitStatistics.DirectoriesToIgnore = formGitStatistics.DirectoriesToIgnore.Replace("/", "\\");
            formGitStatistics.WorkingDir = new DirectoryInfo(gitUiCommands.GitWorkingDir);

            formGitStatistics.ShowDialog();
        }

        #endregion
    }
}