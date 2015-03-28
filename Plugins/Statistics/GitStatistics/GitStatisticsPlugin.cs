using System;
using System.IO;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitStatistics
{
    public class GitStatisticsPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitStatisticsPlugin()
        {
            Description = "Statistics";
            Translate();
        }

        StringSetting CodeFiles = new StringSetting("Code files",
                                "*.c;*.cpp;*.cc;*.h;*.hpp;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" +
                                "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb;*.js");
        StringSetting IgnoreDirectories = new StringSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
        BoolSetting IgnoreSubmodules = new BoolSetting("Ignore submodules", true);

        #region IGitPlugin Members

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return CodeFiles;
            yield return IgnoreDirectories;
            yield return IgnoreSubmodules;
        }

        public override bool Execute(GitUIBaseEventArgs gitUIEventArgs)
        {
            if (string.IsNullOrEmpty(gitUIEventArgs.GitModule.WorkingDir))
                return false;

            using (var formGitStatistics =
                new FormGitStatistics(gitUIEventArgs.GitModule, CodeFiles[Settings])
                    {
                        DirectoriesToIgnore = IgnoreDirectories[Settings]
                    })
            {

                if (IgnoreSubmodules[Settings].Value)
                {
                    foreach (var submodule in gitUIEventArgs.GitModule.GetSubmodulesInfo())
                    {
                        formGitStatistics.DirectoriesToIgnore += ";";
                        formGitStatistics.DirectoriesToIgnore += Path.Combine(gitUIEventArgs.GitModule.WorkingDir, submodule.LocalPath);
                    }
                }

                formGitStatistics.DirectoriesToIgnore = formGitStatistics.DirectoriesToIgnore.Replace("/", "\\");
                formGitStatistics.WorkingDir = new DirectoryInfo(gitUIEventArgs.GitModule.WorkingDir);

                formGitStatistics.ShowDialog(gitUIEventArgs.OwnerForm);
            }
            return false;
        }

        #endregion
    }
}
