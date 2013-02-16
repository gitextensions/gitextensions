namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public partial class FormContributors : GitExtensionsForm
    {
        public FormContributors()
        {
            InitializeComponent();

            Translate();
        }

        public void LoadContributors(string coders, string translators, string designers, string others)
        {
            codersLabel.Text = coders;
            translatorsLabel.Text = translators;
            designersLabel.Text = designers;
        }
    }
}
