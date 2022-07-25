﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using Microsoft;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormRecentReposSettings : GitExtensionsForm
    {
        private IList<Repository>? _repositoryHistory;

        public FormRecentReposSettings()
            : base(true)
        {
            InitializeComponent();
            InitializeComplete();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                _repositoryHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                LoadSettings();
                RefreshRepos();
                SetComboWidth();
            });
        }

        private void LoadSettings()
        {
            SetShorteningStrategy(AppSettings.ShorteningRecentRepoPathStrategy);
            sortMostRecentRepos.Checked = AppSettings.SortMostRecentRepos;
            sortAllRecentRepos.Checked = AppSettings.SortAllRecentRepos;
            comboMinWidthEdit.Value = AppSettings.RecentReposComboMinWidth;
            SetNumericUpDownValue(_NO_TRANSLATE_maxRecentRepositories, AppSettings.MaxMostRecentRepositories);
            SetNumericUpDownValue(_NO_TRANSLATE_RecentRepositoriesHistorySize, AppSettings.RecentRepositoriesHistorySize);

            void SetNumericUpDownValue(NumericUpDown control, int value)
            {
                control.Value = Math.Min(Math.Max(control.Minimum, value), control.Maximum);
            }

            void SetShorteningStrategy(ShorteningRecentRepoPathStrategy strategy)
            {
                switch (strategy)
                {
                    case ShorteningRecentRepoPathStrategy.None:
                        dontShortenRB.Checked = true;
                        break;
                    case ShorteningRecentRepoPathStrategy.MostSignDir:
                        mostSigDirRB.Checked = true;
                        break;
                    case ShorteningRecentRepoPathStrategy.MiddleDots:
                        middleDotRB.Checked = true;
                        break;
                    default:
                        throw new Exception("Unhandled shortening strategy: " + strategy);
                }
            }
        }

        private void SaveSettings()
        {
            Validates.NotNull(_repositoryHistory);

            AppSettings.ShorteningRecentRepoPathStrategy = GetShorteningStrategy();
            AppSettings.SortMostRecentRepos = sortMostRecentRepos.Checked;
            AppSettings.SortAllRecentRepos = sortAllRecentRepos.Checked;
            AppSettings.MaxMostRecentRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value;
            AppSettings.RecentReposComboMinWidth = (int)comboMinWidthEdit.Value;
            AppSettings.RecentRepositoriesHistorySize = (int)_NO_TRANSLATE_RecentRepositoriesHistorySize.Value;

            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(_repositoryHistory));
        }

        private ShorteningRecentRepoPathStrategy GetShorteningStrategy()
        {
            if (dontShortenRB.Checked)
            {
                return ShorteningRecentRepoPathStrategy.None;
            }
            else if (mostSigDirRB.Checked)
            {
                return ShorteningRecentRepoPathStrategy.MostSignDir;
            }
            else if (middleDotRB.Checked)
            {
                return ShorteningRecentRepoPathStrategy.MiddleDots;
            }
            else
            {
                throw new Exception("Can not figure shortening strategy");
            }
        }

        private void RefreshRepos()
        {
            Validates.NotNull(_repositoryHistory);

            MostRecentLB.Items.Clear();
            AllRecentLB.Items.Clear();

            List<RecentRepoInfo> mostRecentRepos = new();
            List<RecentRepoInfo> allRecentRepos = new();

            RecentRepoSplitter splitter = new()
            {
                MaxRecentRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value,
                ShorteningStrategy = GetShorteningStrategy(),
                SortAllRecentRepos = sortAllRecentRepos.Checked,
                SortMostRecentRepos = sortMostRecentRepos.Checked,
                RecentReposComboMinWidth = (int)comboMinWidthEdit.Value,
                MeasureFont = MostRecentLB.Font,
                Graphics = MostRecentLB.CreateGraphics()
            };

            try
            {
                splitter.SplitRecentRepos(_repositoryHistory, mostRecentRepos, allRecentRepos);
            }
            finally
            {
                splitter.Graphics.Dispose();
            }

            foreach (var repo in mostRecentRepos)
            {
                MostRecentLB.Items.Add(new ListViewItem(repo.Caption) { Tag = repo, ToolTipText = repo.Caption });
            }

            foreach (var repo in allRecentRepos)
            {
                AllRecentLB.Items.Add(new ListViewItem(repo.Caption) { Tag = repo, ToolTipText = repo.Caption });
            }
        }

        private void SetComboWidth()
        {
            if (comboMinWidthEdit.Value == 0)
            {
                MostRecentLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                AllRecentLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else
            {
                int width = Math.Max(30, (int)comboMinWidthEdit.Value);
                MostRecentLB.Columns[0].Width = width;
                AllRecentLB.Columns[0].Width = width;
            }
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
                AllRecentLB.BeginUpdate();

                SetComboWidth();
                RefreshRepos();
            }
            finally
            {
                MostRecentLB.EndUpdate();
                AllRecentLB.EndUpdate();
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
            if (GetSelectedRepo(sender, out var repo))
            {
                e.Cancel = false;
                anchorToMostToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.MostRecent;
                anchorToLessToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.AllRecent;
                removeAnchorToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.None;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private bool GetSelectedRepo(object? sender, [NotNullWhen(returnValue: true)] out RecentRepoInfo? repo)
        {
            if (sender is ContextMenuStrip strip)
            {
                sender = strip.SourceControl;
            }
            else if (sender is ToolStripItem item)
            {
                return GetSelectedRepo(item.Owner, out repo);
            }
            else
            {
                sender = null;
            }

            ListView? lb;
            if (sender == MostRecentLB)
            {
                lb = MostRecentLB;
            }
            else if (sender == AllRecentLB)
            {
                lb = AllRecentLB;
            }
            else
            {
                lb = null;
            }

            repo = null;
            if (lb?.SelectedItems.Count > 0)
            {
                repo = lb.SelectedItems[0].Tag as RecentRepoInfo;
            }

            return repo is not null;
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
                repo.Repo.Anchor = Repository.RepositoryAnchor.AllRecent;
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
            if (!GetSelectedRepo(sender, out var repo))
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                _repositoryHistory = await RepositoryHistoryManager.Locals.RemoveRecentAsync(repo.Repo.Path);

                await this.SwitchToMainThreadAsync();
                RefreshRepos();
            });
        }

        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var listView = (ListView)sender;

            var rowBounds = e.Bounds;
            int leftMargin = e.Item.GetBounds(ItemBoundsPortion.Label).Left;
            Rectangle bounds = new(leftMargin, rowBounds.Top, rowBounds.Width - leftMargin, rowBounds.Height);

            e.Graphics.FillRectangle(SystemBrushes.Window, bounds);
            TextRenderer.DrawText(e.Graphics, e.Item.Text, listView.Font, bounds, SystemColors.ControlText,
                                  TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter);
        }
    }
}
