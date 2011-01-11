using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.RepoHosting
{
    public partial class CreatePullRequestForm : Form
    {
        IGitHostingPlugin _repoHost;

        public CreatePullRequestForm(IGitHostingPlugin repoHost)
        {
            InitializeComponent();
        }

        private void CreatePullRequestForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            _fetchers = _repoHost.GetPullRequestTargetsForCurrentWorkingDirRepo();

            _selectedOwner.Items.Clear();
            foreach (var fetcher in _fetchers)
                _selectedOwner.Items.Add(fetcher);

            if (_selectedOwner.Items.Count > 0)
            {
                _selectedOwner.SelectedIndex = 0;
                _selectedOwner_SelectedIndexChanged(null, null);
            }
        }
    }
}
