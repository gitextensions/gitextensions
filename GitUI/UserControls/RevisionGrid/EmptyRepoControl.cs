using System.Windows.Forms;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed partial class EmptyRepoControl : GitModuleControl
    {
        private readonly TranslationString _repoHasNoCommits = new TranslationString("This repository does not yet contain any commits.");

        public EmptyRepoControl()
        {
            InitializeComponent();
            InitializeComplete();

            lblEmptyRepository.Text = _repoHasNoCommits.Text;

            btnEditGitIgnore.Click += (_, e) => UICommands.StartEditGitIgnoreDialog(this, localExcludes: false);
            btnOpenCommitForm.Click += (_, e) => UICommands.StartCommitDialog(this);

            Dock = DockStyle.Fill;
        }
    }
}
