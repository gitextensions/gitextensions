using System.Windows.Forms;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed partial class EmptyRepoControl : GitModuleControl
    {
        private readonly TranslationString _repoHasNoCommits = new TranslationString("This repository does not yet contain any commits.");

        /// <summary>For VS designer.</summary>
        public EmptyRepoControl()
            : this(false)
        {
        }

        public EmptyRepoControl(bool isBareRepository)
        {
            InitializeComponent();
            InitializeComplete();

            lblEmptyRepository.Text = _repoHasNoCommits.Text;

            if (isBareRepository)
            {
                btnEditGitIgnore.Visible = false;
                btnOpenCommitForm.Visible = false;
            }
            else
            {
                btnEditGitIgnore.Click += (_, e) => UICommands.StartEditGitIgnoreDialog(this, localExcludes: false);
                btnOpenCommitForm.Click += (_, e) => UICommands.StartCommitDialog(this);
            }

            Dock = DockStyle.Fill;
        }
    }
}
