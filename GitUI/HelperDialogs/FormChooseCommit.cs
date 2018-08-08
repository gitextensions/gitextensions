using System;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.HelperDialogs
{
    public partial class FormChooseCommit : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormChooseCommit()
        {
            InitializeComponent();
        }

        private FormChooseCommit([NotNull] GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormChooseCommit([NotNull] GitUICommands commands, [CanBeNull] string preselectCommit, bool showArtificial = false)
            : this(commands)
        {
            revisionGrid.MultiSelect = false;
            revisionGrid.ShowUncommittedChangesIfPossible = showArtificial && !revisionGrid.Module.IsBareRepository();

            if (!string.IsNullOrEmpty(preselectCommit))
            {
                var objectId = Module.RevParse(preselectCommit);
                if (objectId != null)
                {
                    revisionGrid.InitialObjectId = objectId;
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
            revisionGrid.MenuCommands.GotoCommitExecute();
        }

        private void linkLabelParent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkLabel = (LinkLabel)sender;
            var parentId = (ObjectId)linkLabel.Tag;

            revisionGrid.SetSelectedRevision(parentId);
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

            var parents = SelectedRevision.ParentIds;

            if (parents == null || parents.Count == 0)
            {
                return;
            }

            linkLabelParent.Tag = parents[0];
            linkLabelParent.Text = parents[0].ToShortString();

            if (parents.Count > 1)
            {
                linkLabelParent2.Visible = true;
                linkLabelParent2.Tag = parents[1];
                linkLabelParent2.Text = parents[1].ToShortString();
            }
            else
            {
                linkLabelParent2.Visible = false;
            }
        }
    }
}
