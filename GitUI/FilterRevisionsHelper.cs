using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    internal sealed class FilterRevisionsHelper : IDisposable
    {
        private readonly ToolStripTextBox _NO_TRANSLATE_textBox;
        private readonly RevisionGridControl _NO_TRANSLATE_revisionGrid;
        private readonly ToolStripButton _NO_TRANSLATE_showFirstParentButton;

        private readonly ToolStripMenuItem _commitFilterToolStripMenuItem;
        private readonly ToolStripMenuItem _committerToolStripMenuItem;
        private readonly ToolStripMenuItem _authorToolStripMenuItem;
        private readonly ToolStripMenuItem _diffContainsToolStripMenuItem;
        private readonly ToolStripMenuItem _hashToolStripMenuItem;

        private readonly Form _NO_TRANSLATE_form;

        public FilterRevisionsHelper(ToolStripTextBox textBox, ToolStripDropDownButton dropDownButton, RevisionGridControl revisionGrid, ToolStripLabel label, ToolStripButton showFirstParentButton, Form form)
        {
            _commitFilterToolStripMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "commitToolStripMenuItem1",
                Text = "Commit message and hash"
            };

            _committerToolStripMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = "committerToolStripMenuItem",
                Text = "Committer"
            };

            _authorToolStripMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = "authorToolStripMenuItem",
                Text = "Author"
            };

            _diffContainsToolStripMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = "diffContainsToolStripMenuItem",
                Text = "Diff contains (SLOW)"
            };
            _diffContainsToolStripMenuItem.Click += (sender, e) =>
            {
                if (_diffContainsToolStripMenuItem.Checked)
                {
                    _commitFilterToolStripMenuItem.Checked = false;
                    _committerToolStripMenuItem.Checked = false;
                    _authorToolStripMenuItem.Checked = false;
                    _hashToolStripMenuItem.Checked = false;
                }
                else
                {
                    _commitFilterToolStripMenuItem.Checked = true;
                }
            };

            _hashToolStripMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = "hashToolStripMenuItem",
                Size = new System.Drawing.Size(216, 24),
                Text = "Hash"
            };

            _NO_TRANSLATE_textBox = textBox;
            _NO_TRANSLATE_revisionGrid = revisionGrid;
            _NO_TRANSLATE_showFirstParentButton = showFirstParentButton;
            _NO_TRANSLATE_form = form;

            dropDownButton.DropDownItems.AddRange(new ToolStripItem[]
            {
                _commitFilterToolStripMenuItem,
                _committerToolStripMenuItem,
                _authorToolStripMenuItem,
                _diffContainsToolStripMenuItem
            });

            _NO_TRANSLATE_showFirstParentButton.Checked = AppSettings.ShowFirstParent;

            label.Click += delegate { ApplyFilter(); };
            _NO_TRANSLATE_textBox.Leave += delegate { ApplyFilter(); };
            _NO_TRANSLATE_textBox.KeyUp += (sender, e) =>
            {
                if (e.KeyValue == (char)Keys.Enter)
                {
                    ApplyFilter();
                }
            };
            _NO_TRANSLATE_showFirstParentButton.Click += delegate { _NO_TRANSLATE_revisionGrid.ShowFirstParent(); };
            _NO_TRANSLATE_revisionGrid.ShowFirstParentsToggled += delegate { _NO_TRANSLATE_showFirstParentButton.Checked = AppSettings.ShowFirstParent; };
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

            try
            {
                _NO_TRANSLATE_revisionGrid.FormatQuickFilter(
                    _NO_TRANSLATE_textBox.Text,
                    _commitFilterToolStripMenuItem.Checked,
                    _committerToolStripMenuItem.Checked,
                    _authorToolStripMenuItem.Checked,
                    _diffContainsToolStripMenuItem.Checked,
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
                _NO_TRANSLATE_revisionGrid.InMemFilterIgnoreCase)
            {
                return;
            }

            _NO_TRANSLATE_revisionGrid.QuickRevisionFilter = revListArgs;
            _NO_TRANSLATE_revisionGrid.InMemMessageFilter = inMemMessageFilter;
            _NO_TRANSLATE_revisionGrid.InMemCommitterFilter = inMemCommitterFilter;
            _NO_TRANSLATE_revisionGrid.InMemAuthorFilter = inMemAuthorFilter;
            _NO_TRANSLATE_revisionGrid.InMemFilterIgnoreCase = true;
            _NO_TRANSLATE_revisionGrid.Visible = true;
            _NO_TRANSLATE_revisionGrid.ForceRefreshRevisions();
        }

        public void Dispose()
        {
            _commitFilterToolStripMenuItem.Dispose();
            _committerToolStripMenuItem.Dispose();
            _authorToolStripMenuItem.Dispose();
            _diffContainsToolStripMenuItem.Dispose();
            _hashToolStripMenuItem.Dispose();
        }
    }
}