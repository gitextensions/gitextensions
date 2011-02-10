using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI
{
    public partial class FormStash : GitExtensionsForm
    {
        public bool NeedRefresh;

        public FormStash()
        {
            InitializeComponent();
            Translate();
            View.ExtraDiffArgumentsChanged += ViewExtraDiffArgumentsChanged;
        }

        private void ViewExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewCurrentChanges();
        }

        private void FormStashFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("stash");
        }

        private void FormStashLoad(object sender, EventArgs e)
        {
            RestorePosition("stash");
        }

        private void Initialize()
        {
            Stashes.Text = "";
            StashMessage.Text = "";
            Stashes.SelectedItem = null;
            Stashes.DataSource = GitCommandHelpers.GetStashes();
            Stashes.DisplayMember = "Name";
            if (Stashes.Items.Count > 0)
                Stashes.SelectedIndex = 0;

            InitializeSoft();
        }

        private void InitializeSoft()
        {
            Stashed.DisplayMember = "FileNameA";
            Stashed.DataSource = GitCommandHelpers.GetStashedItems(Stashes.Text);
        }

        private void InitializeTracked()
        {
            var itemStatusList = GitCommandHelpers.GetAllChangedFiles();

            Changes.DisplayMember = "Name";
            Changes.DataSource = itemStatusList;
        }

        private void StashedSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Stashed.SelectedItem is Patch)
                ShowPatch((Patch)Stashed.SelectedItem);
            Cursor.Current = Cursors.Default;
        }

        private void ShowPatch(Patch patch)
        {
            View.ViewPatch(patch.Text);
        }

        private void ChangesSelectedIndexChanged(object sender, EventArgs e)
        {
            ViewCurrentChanges();
        }

        private void ViewCurrentChanges()
        {
            Cursor.Current = Cursors.WaitCursor;
            View.ViewCurrentChanges(((GitItemStatus)Changes.SelectedItem).Name, ((GitItemStatus)Changes.SelectedItem).OldName, false);
            Cursor.Current = Cursors.Default;
        }

        private void StashClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess("stash save").ShowDialog();
            NeedRefresh = true;
            Initialize();
            InitializeTracked();
            Cursor.Current = Cursors.Default;
        }

        private void ClearClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(string.Format("stash drop {0}", Stashes.Text)).ShowDialog();
            NeedRefresh = true;
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ApplyClick(object sender, EventArgs e)
        {
            new FormProcess(string.Format("stash apply {0}", Stashes.Text)).ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();

            Initialize();
            InitializeTracked();
        }

        private void StashesSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            InitializeSoft();
            if (Stashes.SelectedItem != null)
                StashMessage.Text = ((GitStash)Stashes.SelectedItem).Message;
            Cursor.Current = Cursors.Default;
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            Cursor.Current = Cursors.WaitCursor;
            Initialize();
            InitializeTracked();
            Cursor.Current = Cursors.Default;
        }

        private void FormStashShown(object sender, EventArgs e)
        {
            RefreshAll();
        }
    }
}