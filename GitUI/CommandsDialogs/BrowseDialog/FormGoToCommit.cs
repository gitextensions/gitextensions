using System;
using System.Diagnostics;
using System.Linq;
using GitCommands;

namespace GitUI.CommandsDialogs.BrowseDialog
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
            LoadTagsAsync();
            LoadBranchesAsync();
        }

        public string GetRevision()
        {
            return _selectedRevision;
        }

        private void commitExpression_TextChanged(object sender, EventArgs e)
        {
            SetSelectedRevisionByFocusedControl();
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
            Process.Start(@"https://www.kernel.org/pub/software/scm/git/docs/git-rev-parse.html#_specifying_revisions");
        }

        private void LoadTagsAsync()
        {
            comboBoxTags.Text = Strings.GetLoadingData();
            _tagsLoader.Load(
                () => Module.GetTagRefs(GitModule.GetTagRefsSortOrder.ByCommitDateDescending).ToList(),
                list =>
                {
                    comboBoxTags.Text = string.Empty;
                    comboBoxTags.DataSource = list; //.Select(a => a.LocalName).ToArray(); // TODO: why is the "LocalName" way like in LoadBranchesAsync not working here?
                    comboBoxTags.DisplayMember = "LocalName";
                    if (!comboBoxTags.Text.IsNullOrEmpty())
                    {
                        comboBoxTags.Select(0, comboBoxTags.Text.Length);
                    }
                    SetSelectedRevisionByFocusedControl();
                }
            );
        }

        private void LoadBranchesAsync()
        {
            comboBoxBranches.Text = Strings.GetLoadingData();
            _branchesLoader.Load(
                () => Module.GetRefs(false).ToList(),
                list =>
                {
                    comboBoxBranches.Text = string.Empty;
                    comboBoxBranches.DataSource = list;
                    comboBoxBranches.DisplayMember = "LocalName";
                    if (!comboBoxBranches.Text.IsNullOrEmpty())
                    {
                        comboBoxBranches.Select(0, comboBoxBranches.Text.Length);
                    }
                    SetSelectedRevisionByFocusedControl();
                }
            );
        }

        private void comboBoxTags_Enter(object sender, EventArgs e)
        {
            SetSelectedRevisionByFocusedControl();
        }

        private void comboBoxBranches_Enter(object sender, EventArgs e)
        {
            SetSelectedRevisionByFocusedControl();
        }

        private void SetSelectedRevisionByFocusedControl()
        {
            if (commitExpression.Focused)
            {
                _selectedRevision = Module.RevParse(commitExpression.Text.Trim());
            }
            else if (comboBoxTags.Focused)
            {
                // TODO: try to get GitRef and then CompleteName
                _selectedRevision = Module.RevParse(comboBoxTags.Text);
            }
            else if (comboBoxBranches.Focused)
            {
                _selectedRevision = Module.RevParse(comboBoxBranches.Text);
            }
        }

        private void comboBoxTags_TextChanged(object sender, EventArgs e)
        {
            SetSelectedRevisionByFocusedControl();
        }

        private void comboBoxBranches_TextChanged(object sender, EventArgs e)
        {
            SetSelectedRevisionByFocusedControl();
        }

        private void comboBoxTags_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxTags.SelectedValue == null)
            {
                return;
            }

            // does not work when using autocomplete, for that we have the _TextChanged method
            _selectedRevision = Module.RevParse(((GitRef)comboBoxTags.SelectedValue).CompleteName);
            Go();
        }

        private void comboBoxBranches_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxBranches.SelectedValue == null)
            {
                return;
            }

            _selectedRevision = Module.RevParse(((GitRef)comboBoxBranches.SelectedValue).CompleteName);
            Go();
        }

        private void comboBoxTags_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            GoIfEnterKey(sender, e);
        }

        private void comboBoxBranches_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            GoIfEnterKey(sender, e);
        }

        private void GoIfEnterKey(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                Go();
            }
        }
    }
}
