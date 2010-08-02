using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class GitLogForm : GitExtensionsForm
    {
        public GitLogForm()
        {
            InitializeComponent();
            Translate();
        }

        private void GitLogFormFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("log");
        }

        private void GitLogFormLoad(object sender, EventArgs e)
        {
            RestorePosition("log");
            Log.Text = Settings.GitLog.ToString();
        }
    }
}