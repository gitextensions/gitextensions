using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using GitStatistics.Properties;

namespace GitStatistics
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginConfigurable,
        IGitPluginExecutable
    {
        private const string _defaultCodeFiles =
            "*.c;*.cpp;*.cc;*.cxx;*.h;*.hpp;*.hxx;*.inl;*.idl;*.asm;*.inc;*.cs;*.xsd;*.wsdl;*.xml;*.htm;*.html;*.css;" +
            "*.vbs;*.vb;*.sql;*.aspx;*.asp;*.php;*.nav;*.pas;*.py;*.rb;*.js;*.jsm;*.ts;*.mk;*.java";

        private readonly StringSetting _codeFiles = new StringSetting("Code files", Strings.CodeFiles, _defaultCodeFiles);
        private readonly StringSetting _ignoreDirectories = new StringSetting("Directories to ignore (EndsWith)", Strings.IgnoreDirectories, @"\Debug;\Release;\obj;\bin;\lib");
        private readonly BoolSetting _ignoreSubmodules = new BoolSetting("Ignore submodules", Strings.IgnoreSubmodules, defaultValue: true);

        public string Name => "Statistics";

        public string Description => Strings.Description;

        public Image Icon => Images.IconGitStatistics;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public IEnumerable<ISetting> GetSettings()
        {
            yield return _codeFiles;
            yield return _ignoreDirectories;
            yield return _ignoreSubmodules;
        }

        public bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(args.GitModule.WorkingDir))
            {
                return false;
            }

            var countSubmodule = !_ignoreSubmodules.ValueOrDefault(SettingsContainer.GetSettingsSource());

            var formStatistics = new FormGitStatistics(args.GitModule, _codeFiles.ValueOrDefault(SettingsContainer.GetSettingsSource()), countSubmodule)
            {
                DirectoriesToIgnore = _ignoreDirectories.ValueOrDefault(SettingsContainer.GetSettingsSource()).Replace("/", "\\")
            };

            using (formStatistics)
            {
                formStatistics.ShowDialog(args.OwnerForm);
            }

            return false;
        }
    }
}
