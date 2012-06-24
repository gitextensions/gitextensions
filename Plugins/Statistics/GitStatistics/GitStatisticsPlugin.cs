﻿using System;
using System.IO;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitStatistics
{
    public class GitStatisticsPlugin : IGitPluginForRepository
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
                                "*.c;*.cpp;*.cc;*.h;*.hpp;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" + 
                                "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb");
            Settings.AddSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
            Settings.AddSetting("Ignore submodules (true/false)", "true");
        }

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            if (string.IsNullOrEmpty(gitUiCommands.GitWorkingDir))
                return false;

            using (var formGitStatistics =
                new FormGitStatistics(Settings.GetSetting("Code files"))
                    {
                        DirectoriesToIgnore =
                            Settings.GetSetting("Directories to ignore (EndsWith)")
                    })
            {

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

                formGitStatistics.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            }
            return false;
        }

        #endregion
    }
}