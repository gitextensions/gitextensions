using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormFormatPullRequest : GitExtensionsForm
    {
        public FormFormatPullRequest()
        {
            InitializeComponent();Translate();
        }

        private void FormFormatPullRequest_Load(object sender, EventArgs e)
        {
            RevisionGrid.Load();
            RevisionGrid.MultiSelect = false;
        }

        private void buttonFormatPullRequest_Click(object sender, EventArgs e)
        {
            List<GitRevision> revs = RevisionGrid.GetSelectedRevisions();

            requestText.Text = Settings.Module.FormatPullRequest(revs[0].Guid, pullURL.Text).Replace("\n","\r\n");
        }

    }
}
