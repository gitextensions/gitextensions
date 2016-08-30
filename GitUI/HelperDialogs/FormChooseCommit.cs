﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<string, string> _parents;

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

        private void linkLabelParent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            revisionGrid.SetSelectedRevision(new GitRevision(revisionGrid.Module, _parents[((LinkLabel)sender).Text]));
        }

        private void revisionGrid_SelectionChanged(object sender, EventArgs e)
        {
            var revisions = revisionGrid.GetSelectedRevisions();
            if (1 != revisions.Count)
            {
                return;
            }
            SelectedRevision = revisions[0];

            flowLayoutPanelParents.Visible = SelectedRevision.ParentGuids.Length != 0;

            if(!flowLayoutPanelParents.Visible)
                return;
            _parents = SelectedRevision.ParentGuids.ToDictionary(p=> p.Substring(0, 10), p=> p);
            linkLabelParent.Text = _parents.Keys.ElementAt(0);

            linkLabelParent2.Visible = _parents.Count > 1;
            if (linkLabelParent2.Visible)
            {
                linkLabelParent2.Text = _parents.Keys.ElementAt(1);
            }
        }
    }
}
