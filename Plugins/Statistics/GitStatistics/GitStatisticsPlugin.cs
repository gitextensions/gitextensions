using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;
using System.IO;

namespace GitStatistics
{
    public class GitStatisticsPlugin : IGitPlugin
    {
        #region IGitPlugin Members

        public string Description
        {
            get { return "Statistics"; }
        }

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
            Settings.AddSetting("Code files", "*.c;*.cpp;*.h;*.hpp;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php");
            Settings.AddSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
            Settings.AddSetting("Ignore submodules (true/false)", "true");

        }

        public void Execute(IGitUIEventArgs gitUICommands)
        {
            if (!string.IsNullOrEmpty(gitUICommands.GitWorkingDir))
            {
                FormGitStatistics formGitStatistics = new FormGitStatistics(gitUICommands);

                formGitStatistics.CodeFilePattern = Settings.GetSetting("Code files");
                formGitStatistics.DirectoriesToIgnore = Settings.GetSetting("Directories to ignore (EndsWith)");

                if (Settings.GetSetting("Ignore submodules (true/false)").Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (IGitSubmodule submodule in gitUICommands.GitCommands.GetSubmodules())
                    {
                        formGitStatistics.DirectoriesToIgnore += ";";
                        formGitStatistics.DirectoriesToIgnore += gitUICommands.GitWorkingDir + submodule.LocalPath;
                    }
                }

                formGitStatistics.DirectoriesToIgnore = formGitStatistics.DirectoriesToIgnore.Replace("/", "\\");
                formGitStatistics.WorkingDir = new DirectoryInfo(gitUICommands.GitWorkingDir);
                    
                formGitStatistics.ShowDialog();
            }
        }

        #endregion
    }
}
