using System.Collections.Generic;

namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public partial class FormContributors : GitExtensionsForm
    {
        public FormContributors()
        {
            InitializeComponent();

            Translate();
        }

        public void LoadContributors(IList<string> coders, IList<string> translators, IList<string> designers, IList<string> others)
        {
            Coders.DataSource = coders;
            Translators.DataSource = translators;
            Designers.DataSource = designers;
        }
    }
}
