using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormStash : GitModuleForm
    {
        private readonly TranslationString _currentWorkingDirChanges = new("Current working directory changes");
        private readonly TranslationString _noStashes = new("There are no stashes.");
        private readonly TranslationString _stashUntrackedFilesNotSupportedCaption = new("Stash untracked files");
        private readonly TranslationString _stashUntrackedFilesNotSupported = new("Stash untracked files is not supported in the version of msysgit you are using. Please update msysgit to at least version 1.7.7 to use this option.");
        private readonly TranslationString _stashDropConfirmTitle = new("Drop Stash Confirmation");
        private readonly TranslationString _cannotBeUndone = new("This action cannot be undone.");
        private readonly TranslationString _areYouSure = new("Are you sure you want to drop the stash? This action cannot be undone.");
        private readonly TranslationString _dontShowAgain = new("Don't show me this message again.");

        private readonly AsyncLoader _asyncLoader = new();

        public bool ManageStashes { get; set; }
        private GitStash? _currentWorkingDirStashItem;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormStash()
        {
            InitializeComponent();
            CompleteTheInitialization();
        }

        public FormStash(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            View.ExtraDiffArgumentsChanged += delegate { StashedSelectedIndexChanged(this, EventArgs.Empty); };
            View.TopScrollReached += FileViewer_TopScrollReached;
            View.BottomScrollReached += FileViewer_BottomScrollReached;
            CompleteTheInitialization();
        }

        private void CompleteTheInitialization()
        {
            KeyPreview = true;
            View.EscapePressed += () => DialogResult = DialogResult.Cancel;
            splitContainer1.SplitterDistance = DpiUtil.Scale(280);
            InitializeComplete();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
            {
                var focusedControl = this.FindFocusedControl();

                switch (focusedControl)
                {
                    case ComboBox { DroppedDown: true } comboBox:
                        comboBox.DroppedDown = false;
                        break;
                    case TextBoxBase { SelectionLength: > 0 } textBox:
                        textBox.SelectionLength = 0;
                        break;
                    default:
                        DialogResult = DialogResult.Cancel;
                        break;
                }

                // do not let the modal form react itself on this preview of the Escape key press
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
            {
                // do not let the modal form react itself on this preview of the Escape key press
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }

        private void FormStashFormClosing(object sender, FormClosingEventArgs e)
        {
            AppSettings.StashKeepIndex = StashKeepIndex.Checked;
            AppSettings.IncludeUntrackedFilesInManualStash = chkIncludeUntrackedFiles.Checked;
        }

        private void FormStashLoad(object sender, EventArgs e)
        {
            StashKeepIndex.Checked = AppSettings.StashKeepIndex;
            chkIncludeUntrackedFiles.Checked = AppSettings.IncludeUntrackedFilesInManualStash;

            ResizeStashesWidth();
        }

        private void Initialize()
        {
            var stashedItems = Module.GetStashes().ToList();

            _currentWorkingDirStashItem = new GitStash(-1, _currentWorkingDirChanges.Text);

            stashedItems.Insert(0, _currentWorkingDirStashItem);

            Stashes.Text = "";
            StashMessage.Text = "";
            Stashes.SelectedItem = null;
            Stashes.ComboBox.DisplayMember = nameof(GitStash.Message);
            Stashes.Items.Clear();
            foreach (GitStash stashedItem in stashedItems)
            {
                Stashes.Items.Add(stashedItem);
            }

            if (ManageStashes && Stashes.Items.Count > 1)
            {
                // more than just the default ("Current working directory changes")
                Stashes.SelectedIndex = 1; // -> auto-select first non-default
            }
            else if (Stashes.Items.Count > 0)
            {
                // (no stashes) -> select default ("Current working directory changes")
                Stashes.SelectedIndex = 0;
            }
        }

        private void InitializeSoft()
        {
            GitStash? gitStash = Stashes.SelectedItem as GitStash;

            Stashed.GroupByRevision = false;
            Stashed.ClearDiffs();

            Loading.Visible = true;
            Loading.IsAnimating = true;
            Stashes.Enabled = false;
            refreshToolStripButton.Enabled = false;
            toolStripButton_customMessage.Enabled = false;
            if (gitStash == _currentWorkingDirStashItem)
            {
                toolStripButton_customMessage.Enabled = true;
                _asyncLoader.LoadAsync(() => Module.GetAllChangedFiles(), LoadGitItemStatuses);
                Clear.Enabled = false; // disallow Drop  (of current working directory)
                Apply.Enabled = false; // disallow Apply (of current working directory)
            }
            else if (gitStash is not null)
            {
                _asyncLoader.LoadAsync(() => Module.GetStashDiffFiles(gitStash.Name), LoadGitItemStatuses);
                Clear.Enabled = true; // allow Drop
                Apply.Enabled = true; // allow Apply
            }
        }

        private void FileViewer_TopScrollReached(object sender, EventArgs e)
        {
            Stashed.SelectPreviousVisibleItem();
            View.ScrollToBottom();
        }

        private void FileViewer_BottomScrollReached(object sender, EventArgs e)
        {
            Stashed.SelectNextVisibleItem();
            View.ScrollToTop();
        }

        private void LoadGitItemStatuses(IReadOnlyList<GitItemStatus> gitItemStatuses)
        {
            GitStash gitStash = (GitStash)Stashes.SelectedItem;
            if (gitStash == _currentWorkingDirStashItem)
            {
                // FileStatusList has no interface for both worktree<-index, index<-HEAD at the same time
                // Must be handled when displaying
                var headId = Module.RevParse("HEAD");
                Validates.NotNull(headId);
                var headRev = new GitRevision(headId);
                var indexRev = new GitRevision(ObjectId.IndexId)
                {
                    ParentIds = new[] { headId }
                };
                var workTreeRev = new GitRevision(ObjectId.WorkTreeId)
                {
                    ParentIds = new[] { ObjectId.IndexId }
                };
                var indexItems = gitItemStatuses.Where(item => item.Staged == StagedStatus.Index).ToList();
                var workTreeItems = gitItemStatuses.Where(item => item.Staged != StagedStatus.Index).ToList();
                Stashed.SetStashDiffs(headRev, indexRev, ResourceManager.TranslatedStrings.Index, indexItems, workTreeRev, ResourceManager.TranslatedStrings.Workspace, workTreeItems);
            }
            else
            {
                var firstId = Module.RevParse(gitStash.Name + "^");
                var firstRev = firstId is null ? null : new GitRevision(firstId);

                var selectedId = Module.RevParse(gitStash.Name);
                Validates.NotNull(selectedId);
                var secondRev = new GitRevision(selectedId);
                if (firstId is not null)
                {
                    secondRev.ParentIds = new[] { firstId };
                }

                Stashed.SetDiffs(firstRev, secondRev, gitItemStatuses);
            }

            Loading.Visible = false;
            Loading.IsAnimating = false;
            Stashes.Enabled = true;
            refreshToolStripButton.Enabled = true;
        }

        private void ResizeStashesWidth()
        {
            Stashes.Size = new Size(toolStrip1.Width - 15 - refreshToolStripButton.Width - showToolStripLabel.Width - toolStripButton_customMessage.Width, Stashes.Size.Height);
        }

        private void StashedSelectedIndexChanged(object sender, EventArgs e)
        {
            View.ViewChangesAsync(Stashed.SelectedItem);
            EnablePartialStash();
        }

        private void StashClick(object sender, EventArgs e)
        {
            if (chkIncludeUntrackedFiles.Checked && !GitVersion.Current.StashUntrackedFilesSupported)
            {
                MessageBox.Show(_stashUntrackedFilesNotSupported.Text, _stashUntrackedFilesNotSupportedCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (WaitCursorScope.Enter())
            {
                var msg = toolStripButton_customMessage.Checked ? " " + StashMessage.Text.Trim() : string.Empty;
                UICommands.StashSave(this, chkIncludeUntrackedFiles.Checked, StashKeepIndex.Checked, msg);
                Initialize();
            }
        }

        private void StashSelectedFiles_Click(object sender, EventArgs e)
        {
            if (chkIncludeUntrackedFiles.Checked && !GitVersion.Current.StashUntrackedFilesSupported)
            {
                MessageBox.Show(_stashUntrackedFilesNotSupported.Text, _stashUntrackedFilesNotSupportedCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (WaitCursorScope.Enter())
            {
                var msg = toolStripButton_customMessage.Checked ? " " + StashMessage.Text.Trim() : string.Empty;
                UICommands.StashSave(this, chkIncludeUntrackedFiles.Checked, StashKeepIndex.Checked, msg, Stashed.SelectedItems.Select(i => i.Item.Name).ToList());
                Initialize();
            }
        }

        private void ClearClick(object sender, EventArgs e)
        {
            using (new WaitCursorScope())
            {
                var stashName = GetStashName();
                if (AppSettings.StashConfirmDropShow)
                {
                    using var dialog = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog
                    {
                        OwnerWindowHandle = Handle,
                        Text = _areYouSure.Text,
                        Caption = _stashDropConfirmTitle.Text,
                        InstructionText = _cannotBeUndone.Text,
                        StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No,
                        Icon = TaskDialogStandardIcon.Information,
                        FooterCheckBoxText = _dontShowAgain.Text,
                        FooterIcon = TaskDialogStandardIcon.Information,
                        StartupLocation = Microsoft.WindowsAPICodePack.Dialogs.TaskDialogStartupLocation.CenterOwner,
                    };

                    TaskDialogResult result = dialog.Show();

                    if (result == TaskDialogResult.Yes)
                    {
                        UICommands.StashDrop(this, stashName);
                        Initialize();
                    }

                    if (dialog.FooterCheckBoxChecked == true)
                    {
                        AppSettings.StashConfirmDropShow = false;
                    }
                }
                else
                {
                    UICommands.StashDrop(this, stashName);
                    Initialize();
                }
            }
        }

        private string GetStashName()
        {
            return ((GitStash)Stashes.SelectedItem).Name;
        }

        private void ApplyClick(object sender, EventArgs e)
        {
            UICommands.StashApply(this, GetStashName());
            Initialize();
        }

        private void StashesSelectedIndexChanged(object sender, EventArgs e)
        {
            EnablePartialStash();

            using (WaitCursorScope.Enter())
            {
                InitializeSoft();

                if (Stashes.SelectedItem is not null)
                {
                    StashMessage.Text = ((GitStash)Stashes.SelectedItem).Message;
                }

                if (Stashes.Items.Count == 1)
                {
                    StashMessage.Text = _noStashes.Text;
                }
            }
        }

        private void EnablePartialStash()
        {
            StashSelectedFiles.Enabled = Stashes.SelectedIndex == 0 && Stashed.SelectedItems.Any();
        }

        private void Stashes_DropDown(object sender, EventArgs e)
        {
            Stashes.ResizeDropDownWidth(Stashes.Size.Width, splitContainer1.Width - (2 * showToolStripLabel.Width));
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            using (WaitCursorScope.Enter())
            {
                Initialize();
            }
        }

        private void FormStashShown(object sender, EventArgs e)
        {
            // shown when form is first displayed
            RefreshAll();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeStashesWidth();
        }

        private void FormStash_Resize(object sender, EventArgs e)
        {
            ResizeStashesWidth();
        }

        private void toolStripButton_customMessage_Click(object sender, EventArgs e)
        {
            if (toolStripButton_customMessage.Enabled)
            {
                if (((ToolStripButton)sender).Checked)
                {
                    StashMessage.ReadOnly = false;
                    StashMessage.Focus();
                    StashMessage.SelectAll();
                }
                else
                {
                    StashMessage.ReadOnly = true;
                }
            }
        }

        private void StashMessage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (toolStripButton_customMessage.Enabled)
            {
                if (!toolStripButton_customMessage.Checked)
                {
                    toolStripButton_customMessage.PerformClick();
                }
            }
        }

        private void toolStripButton_customMessage_EnabledChanged(object sender, EventArgs e)
        {
            var button = (ToolStripButton)sender;
            if (!button.Enabled)
            {
                StashMessage.ReadOnly = true;
            }
            else if (button.Checked)
            {
                StashMessage.ReadOnly = false;
            }
        }

        private void View_KeyUp(object sender, KeyEventArgs e)
        {
            // Close Stash form with escape button while pointer focus is in FileViewer(diff view)
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void CherryPickFileChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            View.CherryPickAllChanges();
        }

        private void ContextMenuStripStashedFiles_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cherryPickFileChangesToolStripMenuItem.Enabled = Stashed.SelectedItems.Count() == 1;
        }
    }
}
