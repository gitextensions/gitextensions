using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

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

        ////protected override void RegisterSettings()
        ////{
        ////    base.RegisterSettings();
        ////}

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var form = new ReleaseNotesGeneratorForm(Settings, gitUiCommands))
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
