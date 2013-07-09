using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormRecentReposSettings : GitExtensionsForm
    {
        private readonly int ComboWidth;
        private readonly int FormWidth;

        public FormRecentReposSettings()
        {
            InitializeComponent();
            FormWidth = Width;
            ComboWidth = comboPanel.Width;
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
            splitter.measureFont = MostRecentLB.Font;
            splitter.graphics = MostRecentLB.CreateGraphics();
            try
            {
                splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories, mostRecentRepos, lessRecentRepos);
            }
            finally
            {
                splitter.graphics.Dispose();
            }

            foreach (RecentRepoInfo repo in mostRecentRepos)
                MostRecentLB.Items.Add(repo);

            foreach (RecentRepoInfo repo in lessRecentRepos)
                LessRecentLB.Items.Add(repo);
            
        }

        private void SetComboWidth()
        {
            if (comboMinWidthEdit.Value == 0)
                comboPanel.Width = ComboWidth;
            else
                comboPanel.Width = (int)comboMinWidthEdit.Value + 30;
            this.Width = FormWidth + comboPanel.Width - ComboWidth;
        }

        private void sortMostRecentRepos_CheckedChanged(object sender, EventArgs e)
        {
            RefreshRepos();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SetComboWidth();
            RefreshRepos();
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
            RecentRepoInfo repo;

            e.Cancel = !GetSelectedRepo(sender, out repo);

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
                sender = (sender as ContextMenuStrip).SourceControl;
            else if (sender is ToolStripItem)
                return GetSelectedRepo((sender as ToolStripItem).Owner, out repo);
            else
                sender = null;

            ListBox lb;
            if (sender == MostRecentLB)
                lb = MostRecentLB;
            else if (sender == LessRecentLB)
                lb = LessRecentLB;
            else
                lb = null;

            if (lb != null)
                repo = (RecentRepoInfo)lb.SelectedItem;
            else
                repo = null;

            return repo != null;
        }

        private void anchorToMostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecentRepoInfo repo;

            if (GetSelectedRepo(sender, out repo))
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.MostRecent;
                RefreshRepos();
            }
        }

        private void anchorToLessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecentRepoInfo repo;

            if (GetSelectedRepo(sender, out repo))
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.LessRecent;
                RefreshRepos();
            }
        }

        private void removeAnchorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecentRepoInfo repo;

            if (GetSelectedRepo(sender, out repo))
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.None;
                RefreshRepos();
            }
        }

        private void removeRecentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecentRepoInfo repo;

            if (GetSelectedRepo(sender, out repo))
            {
                Repositories.RepositoryHistory.Repositories.Remove(repo.Repo);
                RefreshRepos();
            }
        }


    }
}
