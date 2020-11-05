using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using ReleaseNotesGenerator.Properties;

namespace ReleaseNotesGenerator
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginExecutable
    {
        public string Name => "Release Notes Generator";

        public string Description => Strings.Description;

        public Image Icon => Images.IconReleaseNotesGenerator;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public bool Execute(GitUIEventArgs args)
        {
            using var form = new ReleaseNotesGeneratorForm(args);

            if (form.ShowDialog(args.OwnerForm) == DialogResult.OK)
            {
                return true;
            }

            return false;
        }
    }
}
