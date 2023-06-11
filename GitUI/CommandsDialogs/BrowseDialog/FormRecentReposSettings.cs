using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using Microsoft;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormRecentReposSettings : GitExtensionsForm
    {
        private const int MinComboWidthAllowed = 30;
        private IList<Repository>? _repositoryHistory;
        private decimal _previousValue;

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
            });
        }

        private void LoadSettings()
        {
            SetShorteningStrategy(AppSettings.ShorteningRecentRepoPathStrategy);
            sortPinnedRepos.Checked = AppSettings.SortPinnedRepos;
            sortAllRecentRepos.Checked = AppSettings.SortAllRecentRepos;
            comboMinWidthEdit.Value = AppSettings.RecentReposComboMinWidth;
            SetNumericUpDownValue(_NO_TRANSLATE_maxRecentRepositories, AppSettings.MaxPinnedRepositories);
            SetNumericUpDownValue(_NO_TRANSLATE_RecentRepositoriesHistorySize, AppSettings.RecentRepositoriesHistorySize);

            _previousValue = comboMinWidthEdit.Value;

            return;

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
            AppSettings.SortPinnedRepos = sortPinnedRepos.Checked;
            AppSettings.SortAllRecentRepos = sortAllRecentRepos.Checked;
            AppSettings.MaxPinnedRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value;
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

            try
            {
                PinnedLB.BeginUpdate();
                AllRecentLB.BeginUpdate();

                PinnedLB.Items.Clear();
                AllRecentLB.Items.Clear();

                List<RecentRepoInfo> pinnedRepos = new();
                List<RecentRepoInfo> allRecentRepos = new();

                RecentRepoSplitter splitter = new()
                {
                    MaxPinnedRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value,
                    ShorteningStrategy = GetShorteningStrategy(),
                    SortAllRecentRepos = sortAllRecentRepos.Checked,
                    SortPinnedRepos = sortPinnedRepos.Checked,
                    RecentReposComboMinWidth = (int)comboMinWidthEdit.Value,
                    MeasureFont = PinnedLB.Font,
                    Graphics = PinnedLB.CreateGraphics()
                };

                try
                {
                    splitter.SplitRecentRepos(_repositoryHistory, pinnedRepos, allRecentRepos);
                }
                finally
                {
                    splitter.Graphics.Dispose();
                }

                foreach (var repo in pinnedRepos)
                {
                    PinnedLB.Items.Add(GetRepositoryListViewItem(repo, repo.Repo.Anchor == Repository.RepositoryAnchor.Pinned));
                }

                foreach (var repo in allRecentRepos)
                {
                    AllRecentLB.Items.Add(GetRepositoryListViewItem(repo, repo.Repo.Anchor == Repository.RepositoryAnchor.AllRecent));
                }

                SetComboWidth();
            }
            finally
            {
                PinnedLB.EndUpdate();
                AllRecentLB.EndUpdate();
            }
        }

        private static ListViewItem GetRepositoryListViewItem(RecentRepoInfo repo, bool anchored)
        {
            ListViewItem item = new(repo.Caption) { Tag = repo, ToolTipText = repo.Repo.Path };

            if (anchored)
            {
                item.Font = new Font(item.Font, FontStyle.Bold);
            }

            if (!Directory.Exists(repo.Repo.Path))
            {
                item.ForeColor = Color.Red;
            }

            return item;
        }

        private void SetComboWidth()
        {
            if (comboMinWidthEdit.Value == 0)
            {
                PinnedLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                AllRecentLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else
            {
                int width = Math.Max(MinComboWidthAllowed, (int)comboMinWidthEdit.Value);
                PinnedLB.Columns[0].Width = width;
                AllRecentLB.Columns[0].Width = width;
            }
        }

        private void sortPinnedRepos_CheckedChanged(object sender, EventArgs e)
        {
            RefreshRepos();
        }

        private void comboMinWidthEdit_ValueChanged(object sender, EventArgs e)
        {
            if (comboMinWidthEdit.Value == _previousValue)
            {
                return;
            }

            if (comboMinWidthEdit.Value < _previousValue)
            {
                if (comboMinWidthEdit.Value < MinComboWidthAllowed)
                {
                    comboMinWidthEdit.Value = 0;
                }
            }
            else
            {
                if (comboMinWidthEdit.Value < MinComboWidthAllowed)
                {
                    comboMinWidthEdit.Value = MinComboWidthAllowed;
                }
            }

            _previousValue = comboMinWidthEdit.Value;
            SetComboWidth();
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
            if (GetSelectedRepos(sender, out List<RecentRepoInfo?> repos))
            {
                e.Cancel = false;
                foreach (RecentRepoInfo repo in repos)
                {
                    anchorToPinnedReposToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.Pinned;
                    anchorToAllRecentReposToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.AllRecent;
                    removeAnchorToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.None;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private bool GetSelectedRepos(object? sender, [NotNullWhen(returnValue: true)] out List<RecentRepoInfo?>? repos)
        {
            if (sender is ContextMenuStrip strip)
            {
                sender = strip.SourceControl;
            }
            else if (sender is ToolStripItem item)
            {
                return GetSelectedRepos(item.Owner, out repos);
            }

            ListView? lb;
            if (sender == PinnedLB)
            {
                lb = PinnedLB;
            }
            else if (sender == AllRecentLB)
            {
                lb = AllRecentLB;
            }
            else
            {
                lb = null;
            }

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

        private void AllRecentLB_DoubleClick(object sender, EventArgs e)
        {
            AnchorToMostRecentRepositories(sender);
        }

        private void PinnedLB_DoubleClick(object sender, EventArgs e)
        {
            AnchorToLessRecentRepositories(sender);
        }

        private void anchorToMostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnchorToMostRecentRepositories(sender);
        }

        private void AnchorToMostRecentRepositories(object sender)
        {
            if (GetSelectedRepos(sender, out List<RecentRepoInfo?> repos))
            {
                foreach (RecentRepoInfo repo in repos)
                {
                    repo.Repo.Anchor = Repository.RepositoryAnchor.Pinned;
                }

                RefreshRepos();
            }
        }

        private void anchorToLessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnchorToLessRecentRepositories(sender);
        }

        private void AnchorToLessRecentRepositories(object sender)
        {
            if (GetSelectedRepos(sender, out List<RecentRepoInfo?> repos))
            {
                foreach (RecentRepoInfo repo in repos)
                {
                    repo.Repo.Anchor = Repository.RepositoryAnchor.AllRecent;
                }

                RefreshRepos();
            }
        }

        private void removeAnchorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedRepos(sender, out List<RecentRepoInfo?> repos))
            {
                foreach (RecentRepoInfo repo in repos)
                {
                    repo.Repo.Anchor = Repository.RepositoryAnchor.None;
                }

                RefreshRepos();
            }
        }

        private void removeRecentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!GetSelectedRepos(sender, out List<RecentRepoInfo?> repos))
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                foreach (RecentRepoInfo repo in repos)
                {
                    _repositoryHistory = await RepositoryHistoryManager.Locals.RemoveRecentAsync(repo.Repo.Path);
                }

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
