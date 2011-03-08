using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;
using System.Collections.Generic;
using System.Drawing;

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
            StashedSelectedIndexChanged(null, null);
        }

        private void FormStashFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("stash");
        }

        private void FormStashLoad(object sender, EventArgs e)
        {
            RestorePosition("stash");
            splitContainer2_SplitterMoved(null, null);
        }

        GitStash currentWorkingDirStashItem;

        private void Initialize()
        {
            IList<GitStash> stashedItems = GitCommandHelpers.GetStashes();

            currentWorkingDirStashItem = new GitStash();
            currentWorkingDirStashItem.Name = "Current working dir changes";
            currentWorkingDirStashItem.Message = "Current working dir changes";

            stashedItems.Insert(0, currentWorkingDirStashItem);

            Stashes.Text = "";
            StashMessage.Text = "";
            Stashes.SelectedItem = null;
            Stashes.Items.Clear();
            foreach(GitStash stashedItem in stashedItems)
                Stashes.Items.Add(stashedItem);
            if (Stashes.Items.Count > 1)
                Stashes.SelectedIndex = 1;
            else
            if (Stashes.Items.Count > 0)
                Stashes.SelectedIndex = 0;
        }

        private void InitializeSoft()
        {
            GitStash gitStash = Stashes.SelectedItem as GitStash;

            Stashed.DataSource = null;

            if (gitStash == null)
            {
                Stashed.DataSource = null;
            }else
            if (gitStash == currentWorkingDirStashItem)
            {
                var itemStatusList = GitCommandHelpers.GetAllChangedFiles();

                Stashed.DisplayMember = "Name";
                Stashed.DataSource = itemStatusList;

            }
            else
            {
                Stashed.DisplayMember = "FileNameA";
                Stashed.DataSource = GitCommandHelpers.GetStashedItems(gitStash.Name);
            }
        }

        private void StashedSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Stashed.SelectedItem is Patch)
                ShowPatch((Patch)Stashed.SelectedItem);
            if (Stashed.SelectedItem is GitItemStatus)
                View.ViewCurrentChanges(((GitItemStatus)Stashed.SelectedItem).Name, ((GitItemStatus)Stashed.SelectedItem).OldName, !((GitItemStatus)Stashed.SelectedItem).IsStaged);
            Cursor.Current = Cursors.Default;
        }

        private void ShowPatch(Patch patch)
        {
            View.ViewPatch(patch.Text);
        }


        private void StashClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess("stash save").ShowDialog();
            NeedRefresh = true;
            Initialize();
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
        }

        private void StashesSelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            
            InitializeSoft();

            if (Stashes.SelectedItem != null)
                StashMessage.Text = ((GitStash)Stashes.SelectedItem).Message;

            if (Stashes.Items.Count == 1)
                StashMessage.Text = "There are no stashes.";

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
            Cursor.Current = Cursors.Default;
        }

        private void FormStashShown(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Stashes.Size =  new Size(toolStrip1.Width - 25 - toolStripButton1.Width - toolStripLabel1.Width, Stashes.Size.Height);

        }

        private void FormStash_Resize(object sender, EventArgs e)
        {
            splitContainer2_SplitterMoved(null, null);
        }
    }
}