using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        /// <summary>
        /// this will be used when Go() is called
        /// </summary>
        private string _selectedRevision;

        // these two are used to prepare for _selectedRevision
        private IGitRef _selectedTag;
        private IGitRef _selectedBranch;

        private readonly AsyncLoader _tagsLoader = new AsyncLoader();
        private readonly AsyncLoader _branchesLoader = new AsyncLoader();

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormGoToCommit()
        {
            InitializeComponent();
        }

        public FormGoToCommit(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        private void FormGoToCommit_Load(object sender, EventArgs e)
        {
            LoadTagsAsync();
            LoadBranchesAsync();
            SetCommitExpressionFromClipboard();
        }

        /// <summary>
        /// returns null if revision does not exist (could not be revparsed)
        /// </summary>
        [CanBeNull]
        public ObjectId ValidateAndGetSelectedRevision()
        {
            return Module.RevParse(_selectedRevision);
        }

        private void commitExpression_TextChanged(object sender, EventArgs e)
        {
            SetSelectedRevisionByFocusedControl();
        }

        private void Go()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            Go();
        }

        private void linkGitRevParse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://git-scm.com/docs/git-rev-parse#_specifying_revisions");
        }

        private void LoadTagsAsync()
        {
            comboBoxTags.Text = Strings.LoadingData;
            ThreadHelper.JoinableTaskFactory.RunAsync(() =>
            {
                return _tagsLoader.LoadAsync(
                    () => Module.GetTagRefs(GitModule.GetTagRefsSortOrder.ByCommitDateDescending).ToList(),
                    list =>
                    {
                        comboBoxTags.Text = string.Empty;
                        comboBoxTags.DataSource = list;
                        comboBoxTags.DisplayMember = nameof(IGitRef.LocalName);
                        SetSelectedRevisionByFocusedControl();
                    });
            });
        }

        private void LoadBranchesAsync()
        {
            comboBoxBranches.Text = Strings.LoadingData;
            ThreadHelper.JoinableTaskFactory.RunAsync(() =>
            {
                return _branchesLoader.LoadAsync(
                    () => Module.GetRefs(false).ToList(),
                    list =>
                    {
                        comboBoxBranches.Text = string.Empty;
                        comboBoxBranches.DataSource = list;
                        comboBoxBranches.DisplayMember = nameof(IGitRef.LocalName);
                        SetSelectedRevisionByFocusedControl();
                    });
            });
        }

        private static IReadOnlyList<IGitRef> DataSourceToGitRefs(ComboBox cb)
        {
            return (IReadOnlyList<IGitRef>)cb.DataSource;
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
            if (textboxCommitExpression.Focused)
            {
                _selectedRevision = textboxCommitExpression.Text.Trim();
            }
            else if (comboBoxTags.Focused)
            {
                _selectedRevision = _selectedTag != null ? _selectedTag.Guid : "";
            }
            else if (comboBoxBranches.Focused)
            {
                _selectedRevision = _selectedBranch != null ? _selectedBranch.Guid : "";
            }
        }

        private void comboBoxTags_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxTags.DataSource == null)
            {
                return;
            }

            _selectedTag = DataSourceToGitRefs(comboBoxTags).FirstOrDefault(a => a.LocalName == comboBoxTags.Text);
            SetSelectedRevisionByFocusedControl();
        }

        private void comboBoxBranches_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxBranches.DataSource == null)
            {
                return;
            }

            _selectedBranch = DataSourceToGitRefs(comboBoxBranches).FirstOrDefault(a => a.LocalName == comboBoxBranches.Text);
            SetSelectedRevisionByFocusedControl();
        }

        private void comboBoxTags_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxTags.SelectedValue == null)
            {
                return;
            }

            _selectedTag = (IGitRef)comboBoxTags.SelectedValue;
            SetSelectedRevisionByFocusedControl();
            Go();
        }

        private void comboBoxBranches_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxBranches.SelectedValue == null)
            {
                return;
            }

            _selectedBranch = (IGitRef)comboBoxBranches.SelectedValue;
            SetSelectedRevisionByFocusedControl();
            Go();
        }

        private void comboBoxTags_KeyUp(object sender, KeyEventArgs e)
        {
            GoIfEnterKey(sender, e);
        }

        private void comboBoxBranches_KeyUp(object sender, KeyEventArgs e)
        {
            GoIfEnterKey(sender, e);
        }

        private void GoIfEnterKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Go();
            }
        }

        private void SetCommitExpressionFromClipboard()
        {
            string text = Clipboard.GetText().Trim();
            if (text.IsNullOrEmpty())
            {
                return;
            }

            var guid = Module.RevParse(text);
            if (guid != null)
            {
                textboxCommitExpression.Text = text;
                textboxCommitExpression.SelectAll();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tagsLoader.Dispose();
                _branchesLoader.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
