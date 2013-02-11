using System;
using GitCommands;
using System.Diagnostics;
using System.Linq;

namespace GitUI
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        string _selectedRevision;
        private readonly AsyncLoader _tagsLoader;
        private readonly AsyncLoader _branchesLoader;

        public FormGoToCommit(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            _tagsLoader = new AsyncLoader();
            _branchesLoader = new AsyncLoader();
        }

        public string GetRevision()
        {
            return _selectedRevision;
        }

        private void commitExpression_TextChanged(object sender, EventArgs e)
        {
            _selectedRevision = Module.RevParse(commitExpression.Text.Trim());
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
            _tagsLoader.Load(
                () => Module.GetTagHeads(GitModule.GetTagHeadsSortOrder.ByCommitDateDescending).Select(g => new GitHeaderGuiWrapper(g)).ToList(),
                list =>
                {
                    comboBoxTags.Text = string.Empty;
                    comboBoxTags.DataSource = list;
                    if (!comboBoxTags.Text.IsNullOrEmpty())
                    {
                        comboBoxTags.Select(0, comboBoxTags.Text.Length);
                    }
                }
            );
        }

        private void comboBoxTags_TextChanged(object sender, EventArgs e)
        {
            // TODO: try to get GitHead and then CompleteName
            _selectedRevision = Module.RevParse(comboBoxTags.Text);
        }

        private void comboBoxTags_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxTags.SelectedValue == null)
            {
                return;
            }

            // does not work when using autocomplete, for that we have the _TextChanged method
            _selectedRevision = Module.RevParse(((GitHeaderGuiWrapper)comboBoxTags.SelectedValue).GitHead.CompleteName);
            Go();
        }

        private void comboBoxBranches_Enter(object sender, EventArgs e)
        {
            comboBoxBranches.Text = Strings.GetLoadingData();
            _branchesLoader.Load(
                () => Module.GetHeads(false).Select(g => new GitHeaderGuiWrapper(g)).ToList(),
                list =>
                {
                    comboBoxBranches.Text = string.Empty;
                    comboBoxBranches.DataSource = list;
                    if (!comboBoxBranches.Text.IsNullOrEmpty())
                    {
                        comboBoxBranches.Select(0, comboBoxBranches.Text.Length);
                    }
                }
            );
        }

        private void comboBoxBranches_TextChanged(object sender, EventArgs e)
        {
            // TODO: try to get GitHead and then CompleteName
            _selectedRevision = Module.RevParse(comboBoxBranches.Text);
        }

        private void comboBoxBranches_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxBranches.SelectedValue == null)
            {
                return;
            }

            _selectedRevision = Module.RevParse(((GitHeaderGuiWrapper)comboBoxBranches.SelectedValue).GitHead.CompleteName);
            Go();
        }
    }

    /// <summary>
    /// to override ToString() for display in combobox
    /// </summary>
    class GitHeaderGuiWrapper
    {
        readonly GitRef _gitHead;
        public GitHeaderGuiWrapper(GitRef gitHead)
        {
            _gitHead = gitHead;
        }

        public override string ToString()
        {
            return _gitHead.LocalName;
        }

        public GitRef GitHead { get { return _gitHead; } }
    }
}
