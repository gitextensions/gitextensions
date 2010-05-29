using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI
{
    public partial class FormStash : GitExtensionsForm
    {
        public FormStash()
        {
            InitializeComponent();
            View.ExtraDiffArgumentsChanged += new EventHandler<EventArgs>(View_ExtraDiffArgumentsChanged);
        }

        void View_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewCurrentChanges();
        }

        private void FormStash_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("stash");
        }

        private void FormStash_Load(object sender, EventArgs e)
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
            List<GitItemStatus> itemStatusList = GitCommands.GitCommands.GetAllChangedFiles();// GitStatus(false);

            Changes.DisplayMember = "Name";
            Changes.DataSource = itemStatusList;
        }

        private void Stashed_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Stashed.SelectedItem is Patch)
            {
                ShowPatch((Patch)Stashed.SelectedItem);
            }
        }

        private void ShowPatch(Patch patch)
        {
            View.ViewPatch(patch.Text);
        }

        private void Changes_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewCurrentChanges();
        }

        private void ViewCurrentChanges()
        {
            Cursor.Current = Cursors.WaitCursor;
            View.ViewCurrentChanges(((GitItemStatus)Changes.SelectedItem).Name, false);
        }

        public bool NeedRefresh = false;

        private void Stash_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess("stash save");
            NeedRefresh = true;
            Initialize();
            InitializeTracked();

        }

        private void Clear_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess("stash drop " + Stashes.Text);
            NeedRefresh = true;
            Initialize();
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            new FormProcess("stash apply " + Stashes.Text);
            //MessageBox.Show("Stash apply\n" + GitCommands.GitCommands.StashApply(), "Stash");

            MergeConflictHandler.HandleMergeConflicts();

            Initialize();
            InitializeTracked();

        }

        private void Stashes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            InitializeSoft();
            if (Stashes.SelectedItem != null)
            {
                StashMessage.Text = ((GitStash)Stashes.SelectedItem).Message;
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Initialize();
            InitializeTracked();
        }

        private void FormStash_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Initialize();
            InitializeTracked();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
