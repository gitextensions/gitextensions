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
    public partial class FormStash : Form
    {
        public FormStash()
        {
            InitializeComponent();
        }

        private void FormStash_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            List<GitItemStatus> itemStatusList = GitCommands.GitCommands.GitStatus();

            List<GitItemStatus> untrackedItemStatus = new List<GitItemStatus>();
            List<GitItemStatus> trackedItemStatus = new List<GitItemStatus>();
            foreach (GitItemStatus itemStatus in itemStatusList)
            {
                if (itemStatus.IsTracked == false)
                    untrackedItemStatus.Add(itemStatus);
                else
                    trackedItemStatus.Add(itemStatus);
            }

            Changes.DisplayMember = "Name";
            Changes.DataSource = trackedItemStatus;

            Stashed.DisplayMember = "FileNameA";
            Stashed.DataSource = GitCommands.GitCommands.GetStashedItems();
        }

        private void Stashed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Stashed.SelectedItem is Patch)
            {
                ShowPatch((Patch)Stashed.SelectedItem);
            }
        }

        private void ShowPatch(Patch patch)
        {
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
            View.Refresh();
        }

        private void Changes_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditorOptions.SetSyntax(View, ((GitItemStatus)Changes.SelectedItem).Name);
            View.Text = GitCommands.GitCommands.GetCurrentChanges(((GitItemStatus)Changes.SelectedItem).Name, false);
            View.Refresh();
        }

        public bool NeedRefresh = false;

        private void Stash_Click(object sender, EventArgs e)
        {
            if (GitCommands.GitCommands.GetStashedItems().Count > 0)
            {
                MessageBox.Show("There are allready stashed items.\nStashing now will overwrite current stash, aborting.", "Error");
                return;
            }            

            MessageBox.Show("Stash changes\n" + GitCommands.GitCommands.Stash(), "Stash");
            NeedRefresh = true;
            Initialize();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Stash cleared\n" + GitCommands.GitCommands.StashClear(), "Stash");
            NeedRefresh = true;
            Initialize();
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Stash apply\n" + GitCommands.GitCommands.StashApply(), "Stash");

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There are unresolved mergeconflicts, run mergetool now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.exe", "mergetool");
                    if (MessageBox.Show("When all mergeconflicts are resolved, you can commit.\nDo you want to commit now?", "Commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        new FormCommit().ShowDialog();
                    }
                }
            }
            Initialize();
        }
    }
}
