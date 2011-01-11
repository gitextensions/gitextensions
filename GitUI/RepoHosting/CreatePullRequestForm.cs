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
            var pullRequestApis = _repoHost.GetPullRequestTargetsForCurrentWorkingDirRepo().Where(r => !r.IsProbablyOwnedByMe);


            
            _pullReqTargetsCB.Items.Clear();
            foreach (var pra in pullRequestApis)
                _pullReqTargetsCB.Items.Add(pra);

            if (_selectedOwner)

            if (_selectedOwner.Items.Count > 0)
            {
                _selectedOwner.SelectedIndex = 0;
                _selectedOwner_SelectedIndexChanged(null, null);
            }
        }
    }
}
