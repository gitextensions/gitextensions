using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitExtensions.Plugins.ReleaseNotesGenerator.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.ReleaseNotesGenerator
{
    [Export(typeof(IGitPlugin))]
    public class ReleaseNotesGeneratorPlugin : GitPluginBase
    {
        public ReleaseNotesGeneratorPlugin() : base(false)
        {
            SetNameAndDescription("Release Notes Generator");
            Translate();
            Icon = Resources.IconReleaseNotesGenerator;
        }

        public override bool Execute(GitUIEventArgs args)
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
