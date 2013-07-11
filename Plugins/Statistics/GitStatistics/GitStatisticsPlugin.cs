using System;
using System.IO;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitStatistics
{
    public class GitStatisticsPlugin : GitPluginBase, IGitPluginForRepository
    {
        #region IGitPlugin Members

        public override string Description
        {
            get { return "Statistics"; }
        }

        protected override void RegisterSettings()
        {
            base.RegisterSettings();
            Settings.AddSetting("Code files",
                                "*.c;*.cpp;*.cc;*.h;*.hpp;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" + 
                                "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb");
            Settings.AddSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
            Settings.AddSetting("Ignore submodules (true/false)", "true");
        }

        public override bool Execute(GitUIBaseEventArgs gitUIEventArgs)
        {
            if (string.IsNullOrEmpty(gitUIEventArgs.GitModule.GitWorkingDir))
                return false;

            using (var formGitStatistics =
                new FormGitStatistics(gitUIEventArgs.GitModule, Settings.GetSetting("Code files"))
                    {
                        DirectoriesToIgnore =
                            Settings.GetSetting("Directories to ignore (EndsWith)")
                    })
            {

                if (Settings.GetSetting("Ignore submodules (true/false)")
                    .Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var submodule in gitUIEventArgs.GitModule.GetSubmodulesInfo())
                    {
                        formGitStatistics.DirectoriesToIgnore += ";";
                        formGitStatistics.DirectoriesToIgnore += Path.Combine(gitUIEventArgs.GitModule.GitWorkingDir, submodule.LocalPath);
                    }
                }

                formGitStatistics.DirectoriesToIgnore = formGitStatistics.DirectoriesToIgnore.Replace("/", "\\");
                formGitStatistics.WorkingDir = new DirectoryInfo(gitUIEventArgs.GitModule.GitWorkingDir);

                formGitStatistics.ShowDialog(gitUIEventArgs.OwnerForm as IWin32Window);
            }
            return false;
        }

        #endregion
    }
}