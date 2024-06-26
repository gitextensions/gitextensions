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
            : base(enablePositionRestore: true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            InitializeComponent();
            InitializeComplete();

            _repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
            LoadSettings();
            RefreshRepos();
        }

        private void LoadSettings()
        {
            SetShorteningStrategy(AppSettings.ShorteningRecentRepoPathStrategy);
            hideTopRepositoriesFromRecentList.Checked = AppSettings.HideTopRepositoriesFromRecentList.Value;
            sortTopRepos.Checked = AppSettings.SortTopRepos;
            sortRecentRepos.Checked = AppSettings.SortRecentRepos;
            comboMinWidthEdit.Value = AppSettings.RecentReposComboMinWidth;
            SetNumericUpDownValue(_NO_TRANSLATE_maxRecentRepositories, AppSettings.MaxTopRepositories);
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
            AppSettings.HideTopRepositoriesFromRecentList.Value = hideTopRepositoriesFromRecentList.Checked;
            AppSettings.SortTopRepos = sortTopRepos.Checked;
            AppSettings.SortRecentRepos = sortRecentRepos.Checked;
            AppSettings.MaxTopRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value;
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
                TopLB.BeginUpdate();
                RecentLB.BeginUpdate();

                TopLB.Items.Clear();
                RecentLB.Items.Clear();

                List<RecentRepoInfo> topRepos = [];
                List<RecentRepoInfo> recentRepos = [];

                RecentRepoSplitter splitter = new()
                {
                    MaxTopRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value,
                    HideTopRepositoriesFromRecentList = hideTopRepositoriesFromRecentList.Checked,
                    ShorteningStrategy = GetShorteningStrategy(),
                    SortRecentRepos = sortRecentRepos.Checked,
                    SortTopRepos = sortTopRepos.Checked,
                    RecentReposComboMinWidth = (int)comboMinWidthEdit.Value,
                    MeasureFont = TopLB.Font,
                };

                splitter.SplitRecentRepos(_repositoryHistory, topRepos, recentRepos);

                foreach (RecentRepoInfo repo in topRepos)
                {
                    TopLB.Items.Add(GetRepositoryListViewItem(repo, repo.Repo.Anchor == Repository.RepositoryAnchor.AnchoredInTop));
                }

                foreach (RecentRepoInfo repo in recentRepos)
                {
                    RecentLB.Items.Add(GetRepositoryListViewItem(repo, repo.Repo.Anchor == Repository.RepositoryAnchor.AnchoredInRecent));
                }

                SetComboWidth();
            }
            finally
            {
                TopLB.EndUpdate();
                RecentLB.EndUpdate();
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
                TopLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                RecentLB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else
            {
                int width = Math.Max(MinComboWidthAllowed, (int)comboMinWidthEdit.Value);
                TopLB.Columns[0].Width = width;
                RecentLB.Columns[0].Width = width;
            }
        }

        private void sortTopRepos_CheckedChanged(object sender, EventArgs e)
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
                    anchorToTopReposToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.AnchoredInTop;
                    anchorToRecentReposToolStripMenuItem.Enabled = repo.Repo.Anchor != Repository.RepositoryAnchor.AnchoredInRecent;
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
            if (sender == TopLB)
            {
                lb = TopLB;
            }
            else if (sender == RecentLB)
            {
                lb = RecentLB;
            }
            else
            {
                lb = null;
            }

            repos = [];
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

        private void TopLB_DoubleClick(object sender, EventArgs e)
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
                    repo.Repo.Anchor = Repository.RepositoryAnchor.AnchoredInTop;
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
                    repo.Repo.Anchor = Repository.RepositoryAnchor.AnchoredInRecent;
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
            ThreadHelper.ThrowIfNotOnUIThread();

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
            });
            RefreshRepos();
        }

        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            ListView listView = (ListView)sender;

            Rectangle rowBounds = e.Bounds;
            int leftMargin = e.Item.GetBounds(ItemBoundsPortion.Label).Left;
            Rectangle bounds = new(leftMargin, rowBounds.Top, rowBounds.Width - leftMargin, rowBounds.Height);

            e.Graphics.FillRectangle(SystemBrushes.Window, bounds);
            TextRenderer.DrawText(e.Graphics, e.Item.Text, listView.Font, bounds, SystemColors.ControlText,
                                  TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter);
        }
    }
}
