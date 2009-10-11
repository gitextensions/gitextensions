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
        }

        private void FormStash_Load(object sender, EventArgs e)
        {


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
            /*
            List<GitItemStatus> untrackedItemStatus = new List<GitItemStatus>();
            List<GitItemStatus> trackedItemStatus = new List<GitItemStatus>();
            foreach (GitItemStatus itemStatus in itemStatusList)
            {
                if (itemStatus.IsTracked == false)
                    untrackedItemStatus.Add(itemStatus);
                else
                trackedItemStatus.Add(itemStatus);
            }*/

            Changes.DisplayMember = "Name";
            //Changes.DataSource = trackedItemStatus;
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
            /*
            string syntax = "XML";
            if ((patch.FileNameB.LastIndexOf('.') > 0))
            {
                string extension = patch.FileNameB.Substring(patch.FileNameB.LastIndexOf('.') + 1).ToUpper();

                switch (extension)
                {
                    case "BAS":
                    case "VBS":
                    case "VB":
                        syntax = "VBNET";
                        break;
                    case "CS":
                        syntax = "C#";
                        break;
                    case "CMD":
                    case "BAT":
                        syntax = "BAT";
                        break;
                    case "C":
                    case "RC":
                    case "IDL":
                    case "H":
                    case "CPP":
                        syntax = "C#";
                        break;
                    default:
                        break;
                }
            }
            View.SetHighlighting(syntax);

            View.Text = patch.Text;
            View.Refresh();*/
            View.ViewPatch(patch.Text);
        }

        private void Changes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            View.ViewPatch(GitCommands.GitCommands.GetCurrentChanges(((GitItemStatus)Changes.SelectedItem).Name, false));
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
