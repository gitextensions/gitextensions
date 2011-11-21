using System;
using System.Windows.Forms;
using GitCommands.Statistics;

namespace GitUI.Statistics
{
    public partial class FormCommitCount : GitExtensionsForm
    {
        public FormCommitCount()
        {
            InitializeComponent();
#if !__MonoCS__ // animated GIFs are not supported in Mono/Linux
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;
#endif
            Translate();
        }

        private void FormCommitCountFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("commit-count");
        }

        private void FormCommitCountLoad(object sender, EventArgs e)
        {
            RestorePosition("commit-count");
            Loading.Visible = true;

            foreach (var keyValuePair in CommitCounter.GroupAllCommitsByContributor().Item1)
            {
                CommitCount.Text += string.Format("{1,6} - {0}\r\n", keyValuePair.Key, keyValuePair.Value);
            }

            Loading.Visible = false;
        }
    }
}