using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitExtensions.Plugins.GitStatistics.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.GitStatistics
{
    [Export(typeof(IGitPlugin))]
    public class GitStatisticsPlugin : GitPluginBase, IGitPluginForRepository
    {
        private const string _defaultCodeFiles =
            "*.c;*.cpp;*.cc;*.cxx;*.h;*.hpp;*.hxx;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" +
            "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb;*.js;*.jsm;*.ts;*.mk;*.java";

        private readonly StringSetting _codeFiles = new("Code files", _defaultCodeFiles);
        private readonly StringSetting _ignoreDirectories = new("Directories to ignore (EndsWith)", @"\Debug;\Release;\obj;\bin;\lib");
        private readonly BoolSetting _ignoreSubmodules = new("Ignore submodules", defaultValue: true);

        public GitStatisticsPlugin() : base(true)
        {
            Id = new Guid("17D1507D-C00D-4A10-AB75-DECB2EA5FCBF");
            Name = "Statistics";
            Translate();
            Icon = Resources.IconGitStatistics;
        }

        public override IEnumerable<ISetting> GetSettings()
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

            var countSubmodule = !_ignoreSubmodules.ValueOrDefault(Settings);

            FormGitStatistics formStatistics = new(args.GitModule, _codeFiles.ValueOrDefault(Settings), countSubmodule)
            {
                DirectoriesToIgnore = _ignoreDirectories.ValueOrDefault(Settings).Replace("/", "\\")
            };

            using (formStatistics)
            {
                formStatistics.ShowDialog(args.OwnerForm);
            }

            return false;
        }
    }
}
