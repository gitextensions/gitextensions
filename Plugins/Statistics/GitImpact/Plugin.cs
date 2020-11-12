using System.ComponentModel.Composition;
using System.Drawing;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using GitImpact.Properties;

namespace GitImpact
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginExecutable
    {
        public string Name => "Impact Graph";

        public string Description => Strings.Description;

        public Image Icon => Images.IconGitImpact;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(args.GitModule.WorkingDir))
            {
                return false;
            }

            using var form = new FormImpact(args.GitModule);

            form.ShowDialog(args.OwnerForm);

            return false;
        }
    }
}
