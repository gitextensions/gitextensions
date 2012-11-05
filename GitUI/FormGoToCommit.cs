using System;
using GitCommands;
using System.Diagnostics;

namespace GitUI
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        public FormGoToCommit(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
        }

        public string GetRevision()
        {
            return Module.RevParse(commitExpression.Text.Trim());
        }


        private void goButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkGitRevParse_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://www.kernel.org/pub/software/scm/git/docs/git-rev-parse.html");
        }
    }
}
