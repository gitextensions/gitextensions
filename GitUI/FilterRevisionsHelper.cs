using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class FilterRevisionsHelper : IDisposable
    {
        private ToolStripTextBox _NO_TRANSLATE_textBox;
        private ToolStripDropDownButton _NO_TRANSLATE_dropDownButton;
        private RevisionGrid _NO_TRANSLATE_revisionGrid;
        private ToolStripLabel _NO_TRANSLATE_label;
        private ToolStripButton _NO_TRANSLATE_showFirstParentButton;

        private ToolStripMenuItem commitToolStripMenuItem;
        private ToolStripMenuItem committerToolStripMenuItem;
        private ToolStripMenuItem authorToolStripMenuItem;
        private ToolStripMenuItem diffContainsToolStripMenuItem;
        private ToolStripMenuItem hashToolStripMenuItem;

        private Form _NO_TRANSLATE_form;

        public FilterRevisionsHelper()
        {
            this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.committerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffContainsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            //
            // commitToolStripMenuItem1
            //
            this.commitToolStripMenuItem.Checked = true;
            this.commitToolStripMenuItem.CheckOnClick = true;
            this.commitToolStripMenuItem.Name = "commitToolStripMenuItem1";
            this.commitToolStripMenuItem.Text = "Commit";
            //
            // committerToolStripMenuItem
            //
            this.committerToolStripMenuItem.CheckOnClick = true;
            this.committerToolStripMenuItem.Name = "committerToolStripMenuItem";
            this.committerToolStripMenuItem.Text = "Committer";
            //
            // authorToolStripMenuItem
            //
            this.authorToolStripMenuItem.CheckOnClick = true;
            this.authorToolStripMenuItem.Name = "authorToolStripMenuItem";
            this.authorToolStripMenuItem.Text = "Author";
            //
            // diffContainsToolStripMenuItem
            //
            this.diffContainsToolStripMenuItem.CheckOnClick = true;
            this.diffContainsToolStripMenuItem.Name = "diffContainsToolStripMenuItem";
            this.diffContainsToolStripMenuItem.Text = "Diff contains (SLOW)";
            this.diffContainsToolStripMenuItem.Click += this.diffContainsToolStripMenuItem_Click;
            //
            // hashToolStripMenuItem
            //
            this.hashToolStripMenuItem.CheckOnClick = true;
            this.hashToolStripMenuItem.Name = "hashToolStripMenuItem";
            this.hashToolStripMenuItem.Size = new System.Drawing.Size(216, 24);
            this.hashToolStripMenuItem.Text = "Hash";
        }

        public FilterRevisionsHelper(ToolStripTextBox textBox, ToolStripDropDownButton dropDownButton, RevisionGrid revisionGrid, ToolStripLabel label, ToolStripButton showFirstParentButton, Form form)
            : this()
        {
            this._NO_TRANSLATE_dropDownButton = dropDownButton;
            this._NO_TRANSLATE_textBox = textBox;
            this._NO_TRANSLATE_revisionGrid = revisionGrid;
            this._NO_TRANSLATE_label = label;
            this._NO_TRANSLATE_showFirstParentButton = showFirstParentButton;
            this._NO_TRANSLATE_form = form;

            this._NO_TRANSLATE_dropDownButton.DropDownItems.AddRange(new ToolStripItem[] {
                this.commitToolStripMenuItem,
                this.committerToolStripMenuItem,
                this.authorToolStripMenuItem,
                this.diffContainsToolStripMenuItem });

            this._NO_TRANSLATE_showFirstParentButton.Checked = AppSettings.ShowFirstParent;

            this._NO_TRANSLATE_label.Click += this.ToolStripLabelClick;
            this._NO_TRANSLATE_textBox.Leave += this.ToolStripTextBoxFilterLeave;
            this._NO_TRANSLATE_textBox.KeyPress += this.ToolStripTextBoxFilterKeyPress;
            this._NO_TRANSLATE_showFirstParentButton.Click += this.ToolStripShowFirstParentButtonClick;
            this._NO_TRANSLATE_revisionGrid.ShowFirstParentsToggled += this.RevisionGridShowFirstParentsToggled;
        }

        public void SetFilter(string filter)
        {
            _NO_TRANSLATE_textBox.Text = filter;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string revListArgs;
            string inMemMessageFilter;
            string inMemCommitterFilter;
            string inMemAuthorFilter;
            var filterParams = new bool[4];
            filterParams[0] = commitToolStripMenuItem.Checked;
            filterParams[1] = committerToolStripMenuItem.Checked;
            filterParams[2] = authorToolStripMenuItem.Checked;
            filterParams[3] = diffContainsToolStripMenuItem.Checked;
            try
            {
                _NO_TRANSLATE_revisionGrid.FormatQuickFilter(_NO_TRANSLATE_textBox.Text,
                                               filterParams,
                                               out revListArgs,
                                               out inMemMessageFilter,
                                               out inMemCommitterFilter,
                                               out inMemAuthorFilter);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(_NO_TRANSLATE_form, ex.Message, "Filter error");
                _NO_TRANSLATE_textBox.Text = "";
                return;
            }

            if ((_NO_TRANSLATE_revisionGrid.QuickRevisionFilter == revListArgs) &&
                (_NO_TRANSLATE_revisionGrid.InMemMessageFilter == inMemMessageFilter) &&
                (_NO_TRANSLATE_revisionGrid.InMemCommitterFilter == inMemCommitterFilter) &&
                (_NO_TRANSLATE_revisionGrid.InMemAuthorFilter == inMemAuthorFilter) &&
                (_NO_TRANSLATE_revisionGrid.InMemFilterIgnoreCase))
                return;
            _NO_TRANSLATE_revisionGrid.QuickRevisionFilter = revListArgs;
            _NO_TRANSLATE_revisionGrid.InMemMessageFilter = inMemMessageFilter;
            _NO_TRANSLATE_revisionGrid.InMemCommitterFilter = inMemCommitterFilter;
            _NO_TRANSLATE_revisionGrid.InMemAuthorFilter = inMemAuthorFilter;
            _NO_TRANSLATE_revisionGrid.InMemFilterIgnoreCase = true;
            _NO_TRANSLATE_revisionGrid.Visible = true;
            _NO_TRANSLATE_revisionGrid.ForceRefreshRevisions();
        }

        private void ToolStripTextBoxFilterLeave(object sender, EventArgs e)
        {
            ToolStripLabelClick(sender, e);
        }

        private void ToolStripTextBoxFilterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                ToolStripLabelClick(null, null);
        }

        private void ToolStripLabelClick(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ToolStripShowFirstParentButtonClick(object sender, EventArgs e)
        {
            this._NO_TRANSLATE_revisionGrid.ShowFirstParent_ToolStripMenuItemClick(sender, e);
        }

        private void RevisionGridShowFirstParentsToggled(object sender, EventArgs e)
        {
            this._NO_TRANSLATE_showFirstParentButton.Checked = AppSettings.ShowFirstParent;
        }

        private void diffContainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (diffContainsToolStripMenuItem.Checked)
            {
                commitToolStripMenuItem.Checked = false;
                committerToolStripMenuItem.Checked = false;
                authorToolStripMenuItem.Checked = false;
                hashToolStripMenuItem.Checked = false;
            }
            else
                commitToolStripMenuItem.Checked = true;
        }

        public void SetLimit(int limit) {
            _NO_TRANSLATE_revisionGrid.SetLimit(limit);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                commitToolStripMenuItem.Dispose();
                committerToolStripMenuItem.Dispose();
                authorToolStripMenuItem.Dispose();
                diffContainsToolStripMenuItem.Dispose();
                hashToolStripMenuItem.Dispose();
            }
        }
    }
}