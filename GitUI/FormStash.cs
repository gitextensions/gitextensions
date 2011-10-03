using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using PatchApply;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormStash : GitExtensionsForm
    {
        TranslationString currentWorkingDirChanges = new TranslationString("Current working dir changes");
        TranslationString noStashes = new TranslationString("There are no stashes.");        

        public bool NeedRefresh;
        private readonly SynchronizationContext _syncContext;

        public FormStash()
        {
            _syncContext = SynchronizationContext.Current;

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
            Settings.StashKeepIndex = this.StashKeepIndex.Checked;
            SavePosition("stash");
        }

        private void FormStashLoad(object sender, EventArgs e)
        {
            this.StashKeepIndex.Checked = Settings.StashKeepIndex;
            RestorePosition("stash");
            splitContainer2_SplitterMoved(null, null);
        }

        GitStash currentWorkingDirStashItem;

        private void Initialize()
        {
            IList<GitStash> stashedItems = GitCommandHelpers.GetStashes();

            currentWorkingDirStashItem = new GitStash();
            currentWorkingDirStashItem.Name = currentWorkingDirChanges.Text;
            currentWorkingDirStashItem.Message = currentWorkingDirChanges.Text;

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

            Stashed.GitItemStatuses = null;

            Loading.Visible = true;
            Stashes.Enabled = false;
            toolStripButton1.Enabled = false;
            toolStripButton_customMessage.Enabled = false;
            if (gitStash == null)
            {
                Stashed.GitItemStatuses = null;
            }else
            if (gitStash == currentWorkingDirStashItem)
            {
                this.toolStripButton_customMessage.Enabled = true;
                ThreadPool.QueueUserWorkItem(
                o =>
                {
                    IList<GitItemStatus> gitItemStatuses = GitCommandHelpers.GetAllChangedFiles();
                    _syncContext.Post(state1 => LoadGitItemStatuses(gitItemStatuses), null);
                });
            }
            else
            {
                ThreadPool.QueueUserWorkItem(
                o =>
                {
                    IList<GitItemStatus> gitItemStatuses = GitCommandHelpers.GetDiffFiles(gitStash.Name, gitStash.Name + "^", true);
                    _syncContext.Post(state1 => LoadGitItemStatuses(gitItemStatuses), null);
                });
            }
        }

        private void LoadGitItemStatuses(IList<GitItemStatus> gitItemStatuses)
        {
            Stashed.GitItemStatuses = gitItemStatuses;
            Loading.Visible = false;
            Stashes.Enabled = true;
            toolStripButton1.Enabled = true;
        }

        private void StashedSelectedIndexChanged(object sender, EventArgs e)
        {
            GitStash gitStash = Stashes.SelectedItem as GitStash;
            GitItemStatus stashedItem = Stashed.SelectedItem;
            Cursor.Current = Cursors.WaitCursor;

            if (stashedItem != null && 
                gitStash == currentWorkingDirStashItem) //current working dir
            {
                View.ViewCurrentChanges(stashedItem.Name, stashedItem.OldName, !stashedItem.IsStaged);
            }
            else
            if (stashedItem != null)
            {
                string extraDiffArguments = View.GetExtraDiffArguments();
                View.ViewPatch(() =>
                {
                    PatchApply.Patch patch = GitCommandHelpers.GetSingleDiff(gitStash.Name, gitStash.Name + "^", stashedItem.Name, stashedItem.OldName, extraDiffArguments);
                    if (patch == null)
                        return String.Empty;
                    return patch.Text;
                });

            }
            else
                View.ViewText(string.Empty, string.Empty);
            Cursor.Current = Cursors.Default;
        }

        private void ShowPatch(Patch patch)
        {
            View.ViewPatch(patch.Text);
        }


        private void StashClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string Arguments = "";
            string Msg = "";

            if (StashKeepIndex.Checked){ Arguments += " --keep-index"; }
            if (toolStripButton_customMessage.Checked) { Msg = " " + StashMessage.Text.Trim(); }

            new FormProcess(String.Format("stash save{0}{1}", Arguments, Msg), "StashSave").ShowDialog();
            NeedRefresh = true;
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ClearClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(string.Format("stash drop {0}", Stashes.Text), "StashDrop").ShowDialog();
            NeedRefresh = true;
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ApplyClick(object sender, EventArgs e)
        {
            new FormProcess(string.Format("stash apply {0}", Stashes.Text), "StashApply").ShowDialog();

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
                StashMessage.Text = noStashes.Text;

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
            Stashes.Size =  new Size(Math.Min(200, toolStrip1.Width - 25 - toolStripButton1.Width - toolStripLabel1.Width - toolStripButton_customMessage.Width), Stashes.Size.Height);
        }

        private void FormStash_Resize(object sender, EventArgs e)
        {
            splitContainer2_SplitterMoved(null, null);
            this.StashKeepIndex.Location = new Point(this.Stash.Location.X + 5, this.Stash.Location.Y - 21);
        }

        private void toolStripButton_customMessage_Click(object sender, EventArgs e)
        {
            if (toolStripButton_customMessage.Enabled)
            {
                if (((ToolStripButton)sender).Checked == true)
                {
                    this.StashMessage.ReadOnly = false;
                    this.StashMessage.Focus();
                    this.StashMessage.SelectAll();
                }
                else
                {
                    this.StashMessage.ReadOnly = true;
                }
            }
        }

        private void StashMessage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.toolStripButton_customMessage.Enabled)
            {
                if (!this.toolStripButton_customMessage.Checked)
                    this.toolStripButton_customMessage.PerformClick();
            }
        }

        private void toolStripButton_customMessage_EnabledChanged(object sender, EventArgs e)
        {
            if (!((ToolStripButton)sender).Enabled)
            {
                StashMessage.ReadOnly = true;
            }
            else if (((ToolStripButton)sender).Enabled && ((ToolStripButton)sender).Checked)
            {
                StashMessage.ReadOnly = false;
            }
        }

    }
}