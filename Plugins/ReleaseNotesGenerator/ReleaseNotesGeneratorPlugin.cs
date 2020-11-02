using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using ReleaseNotesGenerator.Properties;
using ResourceManager;

namespace ReleaseNotesGenerator
{
    [Export(typeof(IGitPlugin))]
    public class ReleaseNotesGeneratorPlugin : GitPluginBase,
        IGitPluginExecutable
    {
        public ReleaseNotesGeneratorPlugin()
        {
            SetNameAndDescription("Release Notes Generator");
            Translate();
            Icon = Resources.IconReleaseNotesGenerator;
        }

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
