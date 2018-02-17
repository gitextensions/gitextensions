﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormRecentReposSettings : GitExtensionsForm
    {
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
            comboMinWidthEdit.Value = AppSettings.RecentReposComboMinWidth;
            SetNumericUpDownValue(_NO_TRANSLATE_maxRecentRepositories, AppSettings.MaxMostRecentRepositories);
            SetNumericUpDownValue(_NO_TRANSLATE_RecentRepositoriesHistorySize, AppSettings.RecentRepositoriesHistorySize);
        }

        private void SaveSettings()
        {
            AppSettings.ShorteningRecentRepoPathStrategy = GetShorteningStrategy();
            AppSettings.SortMostRecentRepos = sortMostRecentRepos.Checked;
            AppSettings.SortLessRecentRepos = sortLessRecentRepos.Checked;
            AppSettings.MaxMostRecentRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value;
            AppSettings.RecentReposComboMinWidth = (int)comboMinWidthEdit.Value;

            var mustResizeRepositriesHistory = AppSettings.RecentRepositoriesHistorySize != (int)_NO_TRANSLATE_RecentRepositoriesHistorySize.Value;
            AppSettings.RecentRepositoriesHistorySize = (int)_NO_TRANSLATE_RecentRepositoriesHistorySize.Value;
            if (mustResizeRepositriesHistory)
            {
                Repositories.RepositoryHistory.MaxCount = AppSettings.RecentRepositoriesHistorySize;
            }
        }

        private string GetShorteningStrategy()
        {
            if (dontShortenRB.Checked)
                return RecentRepoSplitter.ShorteningStrategy_None;
            if (mostSigDirRB.Checked)
                return RecentRepoSplitter.ShorteningStrategy_MostSignDir;
            if (middleDotRB.Checked)
                return RecentRepoSplitter.ShorteningStrategy_MiddleDots;
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

        private void RefreshRepos()
        {
            MostRecentLB.Items.Clear();
            LessRecentLB.Items.Clear();

            List<RecentRepoInfo> mostRecentRepos = new List<RecentRepoInfo>();
            List<RecentRepoInfo> lessRecentRepos = new List<RecentRepoInfo>();

            var splitter = new RecentRepoSplitter
            {
                MaxRecentRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value,
                ShorteningStrategy = GetShorteningStrategy(),
                SortLessRecentRepos = sortLessRecentRepos.Checked,
                SortMostRecentRepos = sortMostRecentRepos.Checked,
                RecentReposComboMinWidth = (int)comboMinWidthEdit.Value,
                MeasureFont = MostRecentLB.Font,
                Graphics = MostRecentLB.CreateGraphics()
            };

            try
            {
                splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories, mostRecentRepos, lessRecentRepos);
            }
            finally
            {
                splitter.Graphics.Dispose();
            }

            foreach (RecentRepoInfo repo in mostRecentRepos)
                MostRecentLB.Items.Add(new ListViewItem(repo.Caption) { Tag = repo, ToolTipText = repo.Caption });

            foreach (RecentRepoInfo repo in lessRecentRepos)
                LessRecentLB.Items.Add(new ListViewItem(repo.Caption) { Tag = repo, ToolTipText = repo.Caption });
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

        private static void SetNumericUpDownValue(NumericUpDown control, int value)
        {
            control.Value = Math.Min(Math.Max(control.Minimum, value), control.Maximum);
        }

        private void sortMostRecentRepos_CheckedChanged(object sender, EventArgs e)
        {
            RefreshRepos();
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
            e.Cancel = !GetSelectedRepo(sender, out var repo);

            if (!e.Cancel)
            {
                anchorToMostToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.MostRecent;
                anchorToLessToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.LessRecent;
                removeAnchorToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.None;
            }
        }

        private bool GetSelectedRepo(object sender, out RecentRepoInfo repo)
        {
            if (sender is ContextMenuStrip)
                sender = ((ContextMenuStrip)sender).SourceControl;
            else if (sender is ToolStripItem)
                return GetSelectedRepo(((ToolStripItem)sender).Owner, out repo);
            else
                sender = null;

            ListView lb;
            if (sender == MostRecentLB)
                lb = MostRecentLB;
            else if (sender == LessRecentLB)
                lb = LessRecentLB;
            else
                lb = null;

            repo = null;
            if (lb?.SelectedItems.Count > 0)
            {
                repo = lb.SelectedItems[0].Tag as RecentRepoInfo;
            }
            return repo != null;
        }

        private void anchorToMostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedRepo(sender, out var repo))
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.MostRecent;
                RefreshRepos();
            }
        }

        private void anchorToLessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedRepo(sender, out var repo))
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.LessRecent;
                RefreshRepos();
            }
        }

        private void removeAnchorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedRepo(sender, out var repo))
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.None;
                RefreshRepos();
            }
        }

        private void removeRecentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedRepo(sender, out var repo))
            {
                Repositories.RepositoryHistory.Repositories.Remove(repo.Repo);
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
    }
}
