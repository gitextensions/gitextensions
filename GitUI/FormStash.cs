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
            Stashes.DataSource = GitCommands.GitCommands.GetStashes();
            Stashes.DisplayMember = "Name";
            if (Stashes.Items.Count > 0)
                Stashes.SelectedIndex = 0;

            InitializeSoft();
        }

        private void InitializeSoft()
        {
            Stashed.DisplayMember = "FileNameA";
            Stashed.DataSource = GitCommands.GitCommands.GetStashedItems(Stashes.Text);
        }

        private void InitializeTracked()
        {
            var itemStatusList = GitCommands.GitCommands.GetAllChangedFiles();

            Changes.DisplayMember = "Name";
            Changes.DataSource = itemStatusList;
        }

        private void StashedSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Stashed.SelectedItem is Patch)
                ShowPatch((Patch) Stashed.SelectedItem);
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
            View.ViewCurrentChanges(((GitItemStatus) Changes.SelectedItem).Name, false);
        }

        private void StashClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess("stash save").ShowDialog();
            NeedRefresh = true;
            Initialize();
            InitializeTracked();
        }

        private void ClearClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(string.Format("stash drop {0}", Stashes.Text)).ShowDialog();
            NeedRefresh = true;
            Initialize();
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
                StashMessage.Text = ((GitStash) Stashes.SelectedItem).Message;
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
        }

        private void FormStashShown(object sender, EventArgs e)
        {
            RefreshAll();
        }
    }
}