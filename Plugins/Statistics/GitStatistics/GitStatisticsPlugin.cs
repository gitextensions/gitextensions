﻿using GitUIPluginInterfaces;
using ResourceManager;

namespace GitStatistics
{
    public class GitStatisticsPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitStatisticsPlugin()
        {
            SetNameAndDescription("Statistics");
            Translate();
        }

        private readonly StringSetting CodeFiles = new StringSetting("Code files",
                                "*.c;*.cpp;*.cc;*.cxx;*.h;*.hpp;*.hxx;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" +
                                "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb;*.js;*.mk;*.java");

        private readonly StringSetting IgnoreDirectories = new StringSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
        private readonly BoolSetting IgnoreSubmodules = new BoolSetting("Ignore submodules", true);

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
            bool countSubmodule = !IgnoreSubmodules.ValueOrDefault(Settings);
            using (var formGitStatistics =
                new FormGitStatistics(gitUIEventArgs.GitModule, CodeFiles.ValueOrDefault(Settings), countSubmodule)
                    {
                        DirectoriesToIgnore = IgnoreDirectories.ValueOrDefault(Settings)
                    })
            {
                formGitStatistics.DirectoriesToIgnore = formGitStatistics.DirectoriesToIgnore.Replace("/", "\\");

                formGitStatistics.ShowDialog(gitUIEventArgs.OwnerForm);
            }
            return false;
        }

        #endregion
    }
}
