using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace ReleaseNotesGenerator
{
    public class ReleaseNotesGeneratorPlugin : GitPluginBase
    {
        public ReleaseNotesGeneratorPlugin()
        {
            Description = "Release Notes Generator";
            Translate();
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var form = new ReleaseNotesGeneratorForm(gitUiCommands))
            {
                if (form.ShowDialog(gitUiCommands.OwnerForm) == DialogResult.OK)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
