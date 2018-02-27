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
            commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            committerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            authorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            diffContainsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            hashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            //
            // commitToolStripMenuItem1
            //
            commitToolStripMenuItem.Checked = true;
            commitToolStripMenuItem.CheckOnClick = true;
            commitToolStripMenuItem.Name = "commitToolStripMenuItem1";
            commitToolStripMenuItem.Text = "Commit";
            //
            // committerToolStripMenuItem
            //
            committerToolStripMenuItem.CheckOnClick = true;
            committerToolStripMenuItem.Name = "committerToolStripMenuItem";
            committerToolStripMenuItem.Text = "Committer";
            //
            // authorToolStripMenuItem
            //
            authorToolStripMenuItem.CheckOnClick = true;
            authorToolStripMenuItem.Name = "authorToolStripMenuItem";
            authorToolStripMenuItem.Text = "Author";
            //
            // diffContainsToolStripMenuItem
            //
            diffContainsToolStripMenuItem.CheckOnClick = true;
            diffContainsToolStripMenuItem.Name = "diffContainsToolStripMenuItem";
            diffContainsToolStripMenuItem.Text = "Diff contains (SLOW)";
            this.diffContainsToolStripMenuItem.Click += this.diffContainsToolStripMenuItem_Click;
            //
            // hashToolStripMenuItem
            //
            hashToolStripMenuItem.CheckOnClick = true;
            hashToolStripMenuItem.Name = "hashToolStripMenuItem";
            hashToolStripMenuItem.Size = new System.Drawing.Size(216, 24);
            hashToolStripMenuItem.Text = "Hash";
        }

        public FilterRevisionsHelper(ToolStripTextBox textBox, ToolStripDropDownButton dropDownButton, RevisionGrid revisionGrid, ToolStripLabel label, ToolStripButton showFirstParentButton, Form form)
            : this()
        {
            _NO_TRANSLATE_dropDownButton = dropDownButton;
            _NO_TRANSLATE_textBox = textBox;
            _NO_TRANSLATE_revisionGrid = revisionGrid;
            _NO_TRANSLATE_label = label;
            _NO_TRANSLATE_showFirstParentButton = showFirstParentButton;
            _NO_TRANSLATE_form = form;

            _NO_TRANSLATE_dropDownButton.DropDownItems.AddRange(new ToolStripItem[] {
                commitToolStripMenuItem,
                committerToolStripMenuItem,
                authorToolStripMenuItem,
                diffContainsToolStripMenuItem });

            _NO_TRANSLATE_showFirstParentButton.Checked = AppSettings.ShowFirstParent;

            this._NO_TRANSLATE_label.Click += this.ToolStripLabelClick;
            this._NO_TRANSLATE_textBox.Leave += this.ToolStripTextBoxFilterLeave;
            this._NO_TRANSLATE_textBox.KeyPress += this.ToolStripTextBoxFilterKeyPress;
            this._NO_TRANSLATE_showFirstParentButton.Click += this.ToolStripShowFirstParentButtonClick;
            _NO_TRANSLATE_revisionGrid.ShowFirstParentsToggled += this.RevisionGridShowFirstParentsToggled;
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
            _NO_TRANSLATE_revisionGrid.ShowFirstParent_ToolStripMenuItemClick(sender, e);
        }

        private void RevisionGridShowFirstParentsToggled(object sender, EventArgs e)
        {
            _NO_TRANSLATE_showFirstParentButton.Checked = AppSettings.ShowFirstParent;
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

        public void SetLimit(int limit)
        {
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