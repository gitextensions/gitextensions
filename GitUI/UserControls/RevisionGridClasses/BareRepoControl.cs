using ResourceManager;

namespace GitUI.UserControls.RevisionGridClasses
{
    public partial class BareRepoControl : GitModuleControl
    {
        private readonly TranslationString _bareRepositoriesAreNotSupported = new TranslationString("Git Extensions does not support viewing bare repositories.");

        public BareRepoControl()
        {
            InitializeComponent();
            Translate();

            label.Text = _bareRepositoriesAreNotSupported.Text;
        }
    }
}
