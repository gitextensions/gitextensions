using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace ReleaseNotesGenerator
{
    public class ReleaseNotesGenerator : GitPluginBase
    {
        #region Translation
        private readonly TranslationString _pluginDescription = new TranslationString("Release Notes Generator");
        #endregion

        public override string Description
        {
            get { return _pluginDescription.Text; }
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
