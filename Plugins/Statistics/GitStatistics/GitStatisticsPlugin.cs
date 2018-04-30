using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitStatistics
{
    [Export(typeof(IGitPlugin))]
    public class GitStatisticsPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitStatisticsPlugin()
        {
            SetNameAndDescription("Statistics");
            Translate();
        }

        private readonly StringSetting _codeFiles = new StringSetting("Code files",
                                "*.c;*.cpp;*.cc;*.cxx;*.h;*.hpp;*.hxx;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" +
                                "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb;*.js;*.mk;*.java");
        private readonly StringSetting _ignoreDirectories = new StringSetting("Directories to ignore (EndsWith)", "\\Debug;\\Release;\\obj;\\bin;\\lib");
        private readonly BoolSetting _ignoreSubmodules = new BoolSetting("Ignore submodules", true);

        #region IGitPlugin Members

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return _codeFiles;
            yield return _ignoreDirectories;
            yield return _ignoreSubmodules;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(args.GitModule.WorkingDir))
            {
                return false;
            }

            bool countSubmodule = !_ignoreSubmodules.ValueOrDefault(Settings);
            using (var formGitStatistics =
                new FormGitStatistics(args.GitModule, _codeFiles.ValueOrDefault(Settings), countSubmodule)
                {
                    DirectoriesToIgnore = _ignoreDirectories.ValueOrDefault(Settings)
                })
            {
                formGitStatistics.DirectoriesToIgnore = formGitStatistics.DirectoriesToIgnore.Replace("/", "\\");

                formGitStatistics.ShowDialog(args.OwnerForm);
            }

            return false;
        }

        #endregion
    }
}
