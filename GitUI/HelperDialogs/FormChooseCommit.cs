﻿using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.HelperDialogs
{
    public partial class FormChooseCommit : GitModuleForm
    {
        private FormChooseCommit(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormChooseCommit(GitUICommands commands, string? preselectCommit, bool showArtificial = false, bool showCurrentBranchOnly = false)
            : this(commands)
        {
            revisionGrid.MultiSelect = false;
            revisionGrid.ShowUncommittedChangesIfPossible = showArtificial;
            if (showCurrentBranchOnly)
            {
                revisionGrid.ShowCurrentBranchOnly();
            }

            if (!string.IsNullOrEmpty(preselectCommit))
            {
                ObjectId objectId = Module.RevParse(preselectCommit);
                if (objectId is not null)
                {
                    revisionGrid.SelectedId = objectId;
                }
            }
        }

        public GitRevision? SelectedRevision { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            revisionGrid.Load();
            base.OnLoad(e);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            IReadOnlyList<GitRevision> revisions = revisionGrid.GetSelectedRevisions();

            if (revisions.Count == 1)
            {
                SelectedRevision = revisions[0];
                DialogResult = DialogResult.OK;

                Close();
            }
        }

        private void revisionGrid_DoubleClickRevision(object sender, DoubleClickRevisionEventArgs e)
        {
            if (e.Revision is not null)
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
            LinkLabel linkLabel = (LinkLabel)sender;
            ObjectId parentId = (ObjectId)linkLabel.Tag;

            if (!revisionGrid.SetSelectedRevision(parentId))
            {
                MessageBoxes.RevisionFilteredInGrid(this, parentId);
            }
        }

        private void revisionGrid_SelectionChanged(object sender, EventArgs e)
        {
            IReadOnlyList<GitRevision> revisions = revisionGrid.GetSelectedRevisions();

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

            IReadOnlyList<ObjectId> parents = SelectedRevision.ParentIds;

            if (parents?.Count is not > 0)
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
