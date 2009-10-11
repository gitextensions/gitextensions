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

        }

        public void Execute(IGitUIEventArgs gitUICommands)
        {
            try
            {
                if (!string.IsNullOrEmpty(gitUICommands.GitWorkingDir))
                {
                    FormGitStatistics formGitStatistics = new FormGitStatistics(gitUICommands);

                    formGitStatistics.CodeFilePattern = Settings.GetSetting("Code files");
                    formGitStatistics.WorkingDir = new DirectoryInfo(gitUICommands.GitWorkingDir);
                        
                    formGitStatistics.ShowDialog();
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
