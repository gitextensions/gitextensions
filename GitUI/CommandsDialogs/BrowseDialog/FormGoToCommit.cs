﻿using System;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using System.Collections.Generic;
using System.Windows.Forms;
using ResourceManager;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        /// <summary>
        /// this will be used when Go() is called
        /// </summary>
        string _selectedRevision;

        // these two are used to prepare for _selectedRevision
        IGitRef _selectedTag;
        IGitRef _selectedBranch;

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

        private void FormGoToCommit_Load(object sender, EventArgs e)
        {
            LoadTagsAsync();
            LoadBranchesAsync();
            SetCommitExpressionFromClipboard();
        }

        /// <summary>
        /// might return an empty or invalid revision
        /// </summary>
        /// <returns></returns>
        public string GetSelectedRevision()
        {
            return _selectedRevision;
        }

        /// <summary>
        /// returns null if revision does not exist (could not be revparsed)
        /// </summary>
        /// <returns></returns>
        public string ValidateAndGetSelectedRevision()
        {
            string guid = Module.RevParse(_selectedRevision);
            if (!string.IsNullOrEmpty(guid))
            {
                return guid;
            }

            return null;
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
                    GitRefsToDataSource(comboBoxTags, list);
                    comboBoxTags.DisplayMember = "LocalName";
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
                    GitRefsToDataSource(comboBoxBranches, list);
                    comboBoxBranches.DisplayMember = "LocalName";
                    SetSelectedRevisionByFocusedControl();
                }
            );
        }

        private static void GitRefsToDataSource(ComboBox cb, IList<IGitRef> refs)
        {
            cb.DataSource = refs;
        }

        private static IList<IGitRef> DataSourceToGitRefs(ComboBox cb)
        {
            return (IList<IGitRef>)cb.DataSource;
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
                if (_selectedTag != null)
                {
                    _selectedRevision = _selectedTag.Guid;
                }
                else
                {
                    _selectedRevision = "";
                }
            }
            else if (comboBoxBranches.Focused)
            {
                if (_selectedBranch != null)
                {
                    _selectedRevision = _selectedBranch.Guid;
                }
                else
                {
                    _selectedRevision = "";
                }
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

        private void SetCommitExpressionFromClipboard()
        {
            string text = Clipboard.GetText().Trim();
            if (text.IsNullOrEmpty())
            {
                return;
            }

            string guid = Module.RevParse(text);
            if (!string.IsNullOrEmpty(guid))
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
                _tagsLoader.Cancel();
                _tagsLoader.Dispose();
                _branchesLoader.Cancel();
                _branchesLoader.Dispose();

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
