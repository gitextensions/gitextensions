using System;
using GitCommands;
using System.Diagnostics;
using System.Linq;

namespace GitUI
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        string _selectedRevision = null;

        public FormGoToCommit(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
        }

        public string GetRevision()
        {
            return _selectedRevision;
        }

        private void commitExpression_TextChanged(object sender, EventArgs e)
        {
            _selectedRevision = Module.RevParse(commitExpression.Text.Trim());
        }

        private void comboBoxTags_TextChanged(object sender, EventArgs e)
        {
            _selectedRevision = Module.RevParse(comboBoxTags.Text);
        }

        private void comboBoxTags_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // does not work when using autocomplete, therefore we have the _TextChanged method
            GitHead selected = (GitHead)comboBoxTags.SelectedValue;
            if (selected == null)
                return;

            _selectedRevision = Module.RevParse(selected.CompleteName);
            Go();
        }

        private void Go()
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            Go();
        }

        private void linkGitRevParse_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://www.kernel.org/pub/software/scm/git/docs/git-rev-parse.html");
        }

        private void comboBoxTags_Enter(object sender, EventArgs e)
        {
            comboBoxTags.DataSource = this.Module.GetTagHeads(GitModule.GetTagHeadsSortOrder.ByCommitDateDescending).ToList();
        }
    }
}
