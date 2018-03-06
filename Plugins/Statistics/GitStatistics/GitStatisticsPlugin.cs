using GitUIPluginInterfaces;
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

        StringSetting _CodeFiles = new StringSetting("Code files",
                                "*.c;*.cpp;*.cc;*.cxx;*.h;*.hpp;*.hxx;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" +
                                "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb;*.js;*.mk;*.java");
        StringSetting _IgnoreDirectories = new StringSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
        BoolSetting _IgnoreSubmodules = new BoolSetting("Ignore submodules", true);

        #region IGitPlugin Members

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return _CodeFiles;
            yield return _IgnoreDirectories;
            yield return _IgnoreSubmodules;
        }

        public override bool Execute(GitUIBaseEventArgs gitUIEventArgs)
        {
            if (string.IsNullOrEmpty(gitUIEventArgs.GitModule.WorkingDir))
            {
                return false;
            }

            bool countSubmodule = !_IgnoreSubmodules.ValueOrDefault(Settings);
            using (var formGitStatistics =
                new FormGitStatistics(gitUIEventArgs.GitModule, _CodeFiles.ValueOrDefault(Settings), countSubmodule)
                {
                    DirectoriesToIgnore = _IgnoreDirectories.ValueOrDefault(Settings)
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
