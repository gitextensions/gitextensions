using System;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls.RevisionGridClasses;
using GitUIPluginInterfaces;

namespace GitUI.HelperDialogs
{
    public partial class FormChooseCommit : GitModuleForm
    {
        private FormChooseCommit()
            : this(null)
        {
        }

        private FormChooseCommit(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            Translate();
        }

        public FormChooseCommit(GitUICommands commands, string preselectCommit, bool showArtificial = false)
            : this(commands)
        {
            revisionGrid.MultiSelect = false;
            revisionGrid.ShowUncommitedChangesIfPossible = showArtificial && !revisionGrid.Module.IsBareRepository();

            if (!string.IsNullOrEmpty(preselectCommit))
            {
                var guid = Module.RevParse(preselectCommit);
                if (guid != null)
                {
                    revisionGrid.SetInitialRevision(guid.ToString());
                }
            }
        }

        public GitRevision SelectedRevision { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            revisionGrid.Load();
            base.OnLoad(e);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var revisions = revisionGrid.GetSelectedRevisions();

            if (revisions.Count == 1)
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

        private void linkLabelParent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkLabel = (LinkLabel)sender;
            var parentId = (ObjectId)linkLabel.Tag;

            revisionGrid.SetSelectedRevision(parentId.ToString());
        }

        private void revisionGrid_SelectionChanged(object sender, EventArgs e)
        {
            var revisions = revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 1)
            {
                return;
            }

            SelectedRevision = revisions[0];

            flowLayoutPanelParents.Visible = SelectedRevision.HasParent;

            if (!flowLayoutPanelParents.Visible)
            {
                return;
            }

            var parents = SelectedRevision.ParentGuids;

            if (parents == null || parents.Count == 0)
            {
                return;
            }

            linkLabelParent.Tag = parents[0];
            linkLabelParent.Text = GitRevision.ToShortSha(parents[0]);

            if (parents.Count > 1)
            {
                linkLabelParent2.Visible = true;
                linkLabelParent2.Tag = parents[1];
                linkLabelParent2.Text = GitRevision.ToShortSha(parents[1]);
            }
            else
            {
                linkLabelParent2.Visible = false;
            }
        }
    }
}
