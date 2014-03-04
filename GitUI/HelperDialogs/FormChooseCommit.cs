using System;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager;
using GitUI.UserControls.RevisionGridClasses;

namespace GitUI.HelperDialogs
{
    public partial class FormChooseCommit : GitModuleForm
    {
        private FormChooseCommit()
            : this(null)
        { }

        private FormChooseCommit(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();        
        }

        public FormChooseCommit(GitUICommands aCommands, string preselectCommit)
            : this(aCommands)
        {
            revisionGrid.MultiSelect = false;

            if (!String.IsNullOrEmpty(preselectCommit))
            {
                string guid = Module.RevParse(preselectCommit);
                if (!String.IsNullOrEmpty(guid))
                {
                    revisionGrid.SetInitialRevision(new GitRevision(Module, guid));
                }
            }

        }

        public GitCommands.GitRevision SelectedRevision { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            revisionGrid.Load();
            base.OnLoad(e);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var revisions = revisionGrid.GetSelectedRevisions();
            if (1 == revisions.Count)
            {
                SelectedRevision = revisions[0];
                DialogResult = DialogResult.OK;

                Close();
            }
        }

        private void revisionGrid_DoubleClickRevision(object sender, DoubleClickRevisionEventArgs e)
        {
            if (e.Revision != null)
            {
                SelectedRevision = e.Revision;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonGotoCommit_Click(object sender, EventArgs e)
        {
            revisionGrid.MenuCommands.GotoCommitExcecute();
        }
    }
}
