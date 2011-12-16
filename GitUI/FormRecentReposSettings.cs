using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;

namespace GitUI
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
            SetShorteningStrategy(Settings.ShorteningRecentRepoPathStrategy);
            sortMostRecentRepos.Checked = Settings.SortMostRecentRepos;
            sortLessRecentRepos.Checked = Settings.SortLessRecentRepos;
            _NO_TRANSLATE_maxRecentRepositories.Value = Settings.MaxMostRecentRepositories;
            comboMinWidthEdit.Value = Settings.RecentReposComboMinWidth;

        }

        private void SaveSettings()
        {
            Settings.ShorteningRecentRepoPathStrategy = GetShorteningStrategy();
            Settings.SortMostRecentRepos = sortMostRecentRepos.Checked;
            Settings.SortLessRecentRepos = sortLessRecentRepos.Checked;
            Settings.MaxMostRecentRepositories = (int)_NO_TRANSLATE_maxRecentRepositories.Value;
            Settings.RecentReposComboMinWidth = (int)comboMinWidthEdit.Value;
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
                comboPanel.Width = (int)comboMinWidthEdit.Value;
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


    }
}
