using System.Windows.Forms;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public partial class BareRepoControl : GitModuleControl
    {
        private readonly TranslationString _bareRepositoriesAreNotSupported = new TranslationString("Git Extensions does not support viewing bare repositories.");

        public BareRepoControl()
        {
            InitializeComponent();
            InitializeComplete();

            label.Text = _bareRepositoriesAreNotSupported.Text;
            Dock = DockStyle.Fill;
        }
    }
}
