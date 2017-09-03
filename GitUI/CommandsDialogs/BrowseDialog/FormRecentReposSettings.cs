using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using System.IO;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormRecentReposSettings : GitExtensionsForm
    {
        private bool ContainsDeletedRepositories { get; set; }
        public FormRecentReposSettings()
            : base(true)
        {
            InitializeComponent();
            Translate();
            LoadSettings();
            RefreshRepos();
            SetComboWidth();
        }

        private void LoadSettings()
        {
            SetShorteningStrategy(AppSettings.ShorteningRecentRepoPathStrategy);
            sortMostRecentRepos.Checked = AppSettings.SortMostRecentRepos;
            sortLessRecentRepos.Checked = AppSettings.SortLessRecentRepos;
            _NO_TRANSLATE_maxRecentRepositories.Value = AppSettings.MaxMostRecentRepositories;
            comboMinWidthEdit.Value = AppSettings.RecentReposComboMinWidth;
        }

        private void SaveSettings()
        {
            AppSettings.ShorteningRecentRepoPathStrategy = GetShorteningStrategy();
            AppSettings.SortMostRecentRepos = sortMostRecentRepos.Checked;
            AppSettings.SortLessRecentRepos = sortLessRecentRepos.Checked;
            AppSettings.MaxMostRecentRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value;
            AppSettings.RecentReposComboMinWidth = (int)comboMinWidthEdit.Value;
        }

        private string GetShorteningStrategy()
        {
            if (dontShortenRB.Checked)
                return RecentRepoSplitter.ShorteningStrategy_None;
            else if (mostSigDirRB.Checked)
                return RecentRepoSplitter.ShorteningStrategy_MostSignDir;
            else if (middleDotRB.Checked)
                return RecentRepoSplitter.ShorteningStrategy_MiddleDots;
            else
                throw new Exception("Can not figure shortening strategy");
        }

        private void SetShorteningStrategy(string strategy)
        {
            if (RecentRepoSplitter.ShorteningStrategy_None.Equals(strategy))
                dontShortenRB.Checked = true;
            else if (RecentRepoSplitter.ShorteningStrategy_MostSignDir.Equals(strategy))
                mostSigDirRB.Checked = true;
            else if (RecentRepoSplitter.ShorteningStrategy_MiddleDots.Equals(strategy))
                middleDotRB.Checked = true;
            else
                throw new Exception("Unhandled shortening strategy: " + strategy);
        }

        private static Font AnchorFont = new Font(new ListViewItem().Font, FontStyle.Bold);

        private void RefreshRepos()
        {
            MostRecentLB.Items.Clear();
            LessRecentLB.Items.Clear();

            List<RecentRepoInfo> mostRecentRepos = new List<RecentRepoInfo>();
            List<RecentRepoInfo> lessRecentRepos = new List<RecentRepoInfo>();

            RecentRepoSplitter splitter = new RecentRepoSplitter();
            splitter.MaxRecentRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value;
            splitter.ShorteningStrategy = GetShorteningStrategy();
            splitter.SortLessRecentRepos = sortLessRecentRepos.Checked;
            splitter.SortMostRecentRepos = sortMostRecentRepos.Checked;
            splitter.RecentReposComboMinWidth = (int)comboMinWidthEdit.Value;
            splitter.MeasureFont = MostRecentLB.Font;
            splitter.Graphics = MostRecentLB.CreateGraphics();

            try
            {
                splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories, mostRecentRepos, lessRecentRepos);
            }
            finally
            {
                splitter.Graphics.Dispose();
            }

            foreach (RecentRepoInfo repo in mostRecentRepos)
            {
                var item = GetRepositoryListViewItem(repo, repo.Repo.Anchor == Repository.RepositoryAnchor.MostRecent);
                MostRecentLB.Items.Add(item);
            }

            foreach (RecentRepoInfo repo in lessRecentRepos)
            {
                var item = GetRepositoryListViewItem(repo, repo.Repo.Anchor == Repository.RepositoryAnchor.LessRecent);
                LessRecentLB.Items.Add(item);
            }

            SetButtonState();
        }

        private ListViewItem GetRepositoryListViewItem(RecentRepoInfo repo, bool anchored)
        {
            var item = new ListViewItem(repo.Caption) { Tag = repo, ToolTipText = repo.Repo.Path };
            if (anchored)
                item.Font = AnchorFont;
            if (!Directory.Exists(repo.Repo.Path))
            {
                item.ForeColor = Color.Red;
                ContainsDeletedRepositories = true;
            }
            return item;
        }

        private void RemoveDeletedRepositories()
        {
            var repos = Repositories.RepositoryHistory.Repositories;
            for (int i = repos.Count - 1; i >= 0; i--)
            {
                if (!Directory.Exists(repos[i].Path))
                {
                    repos.RemoveAt(i);
                }
            }
            ContainsDeletedRepositories = false;
        }

        private void SetComboWidth()
        {
            if (comboMinWidthEdit.Value == 0)
            {
                MostRecentLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                LessRecentLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else
            {
                int width = Math.Max(30, (int)comboMinWidthEdit.Value);
                MostRecentLB.Columns[0].Width = width;
                LessRecentLB.Columns[0].Width = width;
            }
        }

        private void sortMostRecentRepos_CheckedChanged(object sender, EventArgs e)
        {
            RefreshRepos();
            SetComboWidth();
        }

        private void comboMinWidthEdit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                MostRecentLB.BeginUpdate();
                LessRecentLB.BeginUpdate();

                SetComboWidth();
                RefreshRepos();
            }
            finally
            {
                MostRecentLB.EndUpdate();
                LessRecentLB.EndUpdate();
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            List<RecentRepoInfo> repos;

            e.Cancel = !GetSelectedRepos(sender, out repos);

            if (!e.Cancel)
            {
                foreach (var repo in repos)
                {
                    anchorToMostToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.MostRecent;
                    anchorToLessToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.LessRecent;
                    removeAnchorToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.None;
                }
            }
        }

        private bool GetSelectedRepos(object sender, out List<RecentRepoInfo> repos)
        {
            if (sender is ContextMenuStrip)
                sender = ((ContextMenuStrip)sender).SourceControl;
            else if (sender is ToolStripItem)
                return GetSelectedRepos(((ToolStripItem)sender).Owner, out repos);
            else if (!(sender is ListView))
                sender = null;

            ListView lb;
            if (sender == MostRecentLB)
                lb = MostRecentLB;
            else if (sender == LessRecentLB)
                lb = LessRecentLB;
            else
                lb = null;

            repos = new List<RecentRepoInfo>();
            if (lb?.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in lb.SelectedItems)
                {
                    repos.Add(item.Tag as RecentRepoInfo);
                }
            }
            return repos.Count != 0;
        }

        private void anchorToMostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnchorToMostRecentRepositories(sender);
        }

        private void AnchorToMostRecentRepositories(object sender)
        {
            List<RecentRepoInfo> repos;

            if (GetSelectedRepos(sender, out repos))
            {
                foreach (var repo in repos)
                {
                    repo.Repo.Anchor = Repository.RepositoryAnchor.MostRecent;
                }
                RefreshRepos();
                SetComboWidth();
            }
        }

        private void anchorToLessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnchorToLessRecentRepositories(sender);
        }

        private void AnchorToLessRecentRepositories(object sender)
        {
            List<RecentRepoInfo> repos;

            if (GetSelectedRepos(sender, out repos))
            {
                foreach (var repo in repos)
                {
                    repo.Repo.Anchor = Repository.RepositoryAnchor.LessRecent;
                }
                RefreshRepos();
                SetComboWidth();
            }
        }

        private void removeAnchorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<RecentRepoInfo> repos;

            if (GetSelectedRepos(sender, out repos))
            {
                foreach (var repo in repos)
                {
                    repo.Repo.Anchor = Repository.RepositoryAnchor.None;
                }
                RefreshRepos();
                SetComboWidth();
            }
        }

        private void removeRecentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<RecentRepoInfo> repos;

            if (GetSelectedRepos(sender, out repos))
            {
                foreach (var repo in repos)
                {
                    Repositories.RepositoryHistory.Repositories.Remove(repo.Repo);
                }
                RefreshRepos();
            }
        }

        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var listView = (ListView)sender;

            var rowBounds = e.Bounds;
            int leftMargin = e.Item.GetBounds(ItemBoundsPortion.Label).Left;
            var bounds = new Rectangle(leftMargin, rowBounds.Top, rowBounds.Width - leftMargin, rowBounds.Height);

            e.Graphics.FillRectangle(SystemBrushes.Window, bounds);
            TextRenderer.DrawText(e.Graphics, e.Item.Text, listView.Font, bounds, SystemColors.ControlText,
                                  TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter);
        }

        private void buttonAnchorAllToLessRecentRepositories_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in MostRecentLB.Items)
            {
                item.Selected = true;
            }
            AnchorToLessRecentRepositories(MostRecentLB);
        }

        private void buttonAnchorToLessRecentRepositories_Click(object sender, EventArgs e)
        {
            AnchorToLessRecentRepositories(MostRecentLB);
            if(MostRecentLB.Items.Count != 0)
            {
                MostRecentLB.Items[0].Selected = true;
                MostRecentLB.Select();
            }
        }

        private void buttonAnchorToMostRecentRepositories_Click(object sender, EventArgs e)
        {
            AnchorToMostRecentRepositories(LessRecentLB);
            if (LessRecentLB.Items.Count != 0)
            {
                LessRecentLB.Items[0].Selected = true;
                LessRecentLB.Select();
            }
        }

        private void buttonAnchorAllToMostRecentRepositories_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in LessRecentLB.Items)
            {
                item.Selected = true;
            }
            AnchorToMostRecentRepositories(LessRecentLB);
        }

        private void SetButtonState()
        {
            buttonAnchorAllToLessRecentRepositories.Enabled = (MostRecentLB.Items.Count != 0);
            buttonAnchorAllToMostRecentRepositories.Enabled = (LessRecentLB.Items.Count != 0);

            buttonAnchorToLessRecentRepositories.Enabled = (MostRecentLB.SelectedItems.Count != 0);
            buttonAnchorToMostRecentRepositories.Enabled = (LessRecentLB.SelectedItems.Count != 0);

            buttonRemoveDeletedRepositories.Enabled = ContainsDeletedRepositories;
        }

        private void MostRecentLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetButtonState();
        }

        private void LessRecentLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetButtonState();
        }

        private void buttonRemoveDeletedRepositories_Click(object sender, EventArgs e)
        {
            RemoveDeletedRepositories();
            RefreshRepos();
        }
    }
}
