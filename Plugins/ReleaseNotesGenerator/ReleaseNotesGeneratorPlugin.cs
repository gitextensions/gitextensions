using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using ReleaseNotesGenerator.Properties;
using ResourceManager;

namespace ReleaseNotesGenerator
{
    [Export(typeof(IGitPlugin))]
    public class ReleaseNotesGeneratorPlugin : GitPluginBase
    {
        public ReleaseNotesGeneratorPlugin()
        {
            SetNameAndDescription("Release Notes Generator");
            Translate();
            Icon = Resources.IconReleaseNotesGenerator;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using (var form = new ReleaseNotesGeneratorForm(args))
            {
                if (form.ShowDialog(args.OwnerForm) == DialogResult.OK)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
