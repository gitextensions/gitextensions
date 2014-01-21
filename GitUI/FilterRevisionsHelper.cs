using System;
using System.Windows.Forms;

namespace GitUI
{
    public class FilterRevisionsHelper 
    {
        private ToolStripTextBox _NO_TRANSLATE_toolStripTextBoxFilter;
        private ToolStripDropDownButton _NO_TRANSLATE_toolStripDropDownButton1;
        private RevisionGrid _NO_TRANSLATE_RevisionGrid;
        private ToolStripLabel _NO_TRANSLATE_toolStripLabel2;

        private ToolStripMenuItem commitToolStripMenuItem1;
        private ToolStripMenuItem committerToolStripMenuItem;
        private ToolStripMenuItem authorToolStripMenuItem;
        private ToolStripMenuItem diffContainsToolStripMenuItem;
        private ToolStripMenuItem hashToolStripMenuItem;

        private Form _NO_TRANSLATE_form;

        public FilterRevisionsHelper()
        {
            this.commitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.committerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffContainsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            // 
            // commitToolStripMenuItem1
            // 
            this.commitToolStripMenuItem1.Checked = true;
            this.commitToolStripMenuItem1.CheckOnClick = true;
            this.commitToolStripMenuItem1.Name = "commitToolStripMenuItem1";
            this.commitToolStripMenuItem1.Text = "Commit";
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

        public FilterRevisionsHelper(ToolStripTextBox toolStripTextBoxFilter, ToolStripDropDownButton toolStripDropDownButton1, RevisionGrid RevisionGrid, ToolStripLabel toolStripLabel2, Form form)
            : this()
        {
            this._NO_TRANSLATE_toolStripDropDownButton1 = toolStripDropDownButton1;
            this._NO_TRANSLATE_toolStripTextBoxFilter = toolStripTextBoxFilter;
            this._NO_TRANSLATE_RevisionGrid = RevisionGrid;
            this._NO_TRANSLATE_toolStripLabel2 = toolStripLabel2;
            this._NO_TRANSLATE_form = form;

            this._NO_TRANSLATE_toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] {
                this.commitToolStripMenuItem1,
                this.committerToolStripMenuItem,
                this.authorToolStripMenuItem,
                this.diffContainsToolStripMenuItem});

            this._NO_TRANSLATE_toolStripLabel2.Click += this.ToolStripLabel2Click;
            this._NO_TRANSLATE_toolStripTextBoxFilter.Leave += this.ToolStripTextBoxFilterLeave;
            this._NO_TRANSLATE_toolStripTextBoxFilter.KeyPress += this.ToolStripTextBoxFilterKeyPress;
        }

        public void SetFilter(string filter)
        {
            _NO_TRANSLATE_toolStripTextBoxFilter.Text = filter;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string revListArgs;
            string inMemMessageFilter;
            string inMemCommitterFilter;
            string inMemAuthorFilter;
            var filterParams = new bool[4];
            filterParams[0] = commitToolStripMenuItem1.Checked;
            filterParams[1] = committerToolStripMenuItem.Checked;
            filterParams[2] = authorToolStripMenuItem.Checked;
            filterParams[3] = diffContainsToolStripMenuItem.Checked;
            try
            {
                _NO_TRANSLATE_RevisionGrid.FormatQuickFilter(_NO_TRANSLATE_toolStripTextBoxFilter.Text,
                                               filterParams,
                                               out revListArgs,
                                               out inMemMessageFilter,
                                               out inMemCommitterFilter,
                                               out inMemAuthorFilter);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(_NO_TRANSLATE_form, ex.Message, "Filter error");
                _NO_TRANSLATE_toolStripTextBoxFilter.Text = "";
                return;
            }

            if ((_NO_TRANSLATE_RevisionGrid.Filter == revListArgs) &&
                (_NO_TRANSLATE_RevisionGrid.InMemMessageFilter == inMemMessageFilter) &&
                (_NO_TRANSLATE_RevisionGrid.InMemCommitterFilter == inMemCommitterFilter) &&
                (_NO_TRANSLATE_RevisionGrid.InMemAuthorFilter == inMemAuthorFilter) &&
                (_NO_TRANSLATE_RevisionGrid.InMemFilterIgnoreCase))
                return;
            _NO_TRANSLATE_RevisionGrid.Filter = revListArgs;
            _NO_TRANSLATE_RevisionGrid.InMemMessageFilter = inMemMessageFilter;
            _NO_TRANSLATE_RevisionGrid.InMemCommitterFilter = inMemCommitterFilter;
            _NO_TRANSLATE_RevisionGrid.InMemAuthorFilter = inMemAuthorFilter;
            _NO_TRANSLATE_RevisionGrid.InMemFilterIgnoreCase = true;
            _NO_TRANSLATE_RevisionGrid.Visible = true;
            _NO_TRANSLATE_RevisionGrid.ForceRefreshRevisions();
        }

        private void ToolStripTextBoxFilterLeave(object sender, EventArgs e)
        {
            ToolStripLabel2Click(sender, e);
        }

        private void ToolStripTextBoxFilterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                ToolStripLabel2Click(null, null);
        }

        private void ToolStripLabel2Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void diffContainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (diffContainsToolStripMenuItem.Checked)
            {
                commitToolStripMenuItem1.Checked = false;
                committerToolStripMenuItem.Checked = false;
                authorToolStripMenuItem.Checked = false;
                hashToolStripMenuItem.Checked = false;
            }
            else
                commitToolStripMenuItem1.Checked = true;
        }

        public void SetLimit(int limit) {
            _NO_TRANSLATE_RevisionGrid.SetLimit(limit);
        }
    }
}