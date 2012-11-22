using System;
using GitCommands;
using System.Diagnostics;
using System.Linq;

namespace GitUI
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        string _selectedRevision = null;
        private AsyncLoader tagsLoader;

        public FormGoToCommit(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            tagsLoader = new AsyncLoader();
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
            _selectedRevision = Module.RevParse(((GitHeaderGuiWrapper)comboBoxTags.SelectedValue).GitHead.CompleteName);
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
            comboBoxTags.Text = Strings.GetLoadingData();
            tagsLoader.Load(
                () => Module.GetTagHeads(GitModule.GetTagHeadsSortOrder.ByCommitDateDescending).Select(g => new GitHeaderGuiWrapper(g)).ToList(),
                list =>
                {
                    comboBoxTags.Text = string.Empty;
                    comboBoxTags.DataSource = list;
                    if (!comboBoxTags.Text.IsNullOrEmpty())
                        comboBoxTags.Select(0, comboBoxTags.Text.Length);
                }
            );
        }
    }

    /// <summary>
    /// to override ToString() for display in combobox
    /// </summary>
    class GitHeaderGuiWrapper
    {
        GitHead _gitHead;
        public GitHeaderGuiWrapper(GitHead gitHead)
        {
            _gitHead = gitHead;
        }

        public override string ToString()
        {
            return _gitHead.LocalName;
        }

        public GitHead GitHead { get { return _gitHead; } }
    }
}
