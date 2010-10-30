using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCommit : GitExtensionsForm
    {
        private readonly TranslationString _alsoDeleteUntrackedFiles =
            new TranslationString("Do you also want to delete the new files that are in the selection?" +
                                  Environment.NewLine + Environment.NewLine + "Choose 'No' to keep all new files.");

        private readonly TranslationString _alsoDeleteUntrackedFilesCaption = new TranslationString("Delete");

        private readonly TranslationString _amendCommit =
            new TranslationString("You are about to rewrite history." + Environment.NewLine +
                                  "Only use amend if the commit is not published yet!" + Environment.NewLine +
                                  Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _amendCommitCaption = new TranslationString("Amend commit");

        private readonly TranslationString _closeDialogAfterCommitTooltip =
            new TranslationString(
                "When checked the commit dialog is closed after each commit.\nOtherwise the dialog will only close when there are no modified files left.");

        private readonly TranslationString _deleteFailed = new TranslationString("Delete file failed");

        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want delete the selected file(s)?");

        private readonly TranslationString _deleteSelectedFilesCaption = new TranslationString("Delete");

        private readonly TranslationString _deleteUntrackedFiles =
            new TranslationString("Are you sure you want to delete all untracked files?");

        private readonly TranslationString _deleteUntrackedFilesCaption =
            new TranslationString("Delete untracked files.");

        private readonly TranslationString _enterCommitMessage = new TranslationString("Please enter commit message");
        private readonly TranslationString _enterCommitMessageCaption = new TranslationString("Commit message");

        private readonly TranslationString _enterCommitMessageHint = new TranslationString("Enter commit message");
        private readonly GitCommandsInstance _gitGetUnstagedCommand = new GitCommandsInstance();

        private readonly TranslationString _mergeConflicts =
            new TranslationString("There are unresolved mergeconflicts, solve mergeconflicts before committing.");

        private readonly TranslationString _mergeConflictsCaption = new TranslationString("Merge conflicts");

        private readonly TranslationString _noFilesStaged =
            new TranslationString("There are no files staged for this commit. Are you sure you want to commit?");

        private readonly TranslationString _noStagedChanges = new TranslationString("There are no staged changes");
        private readonly TranslationString _noUnstagedChanges = new TranslationString("There are no unstaged changes");

        private readonly TranslationString _notOnBranch =
            new TranslationString("You are not working on a branch." + Environment.NewLine +
                                  "This commit will be unreferenced when switching to another brach and can be lost." +
                                  Environment.NewLine + "" + Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _notOnBranchCaption = new TranslationString("Not on a branch.");

        private readonly TranslationString _onlyStageChunkOfSingleFileError =
            new TranslationString("You can only use this option when selecting a single file");

        private readonly TranslationString _resetChanges =
            new TranslationString("Are you sure you want to reset the changes to the selected files?");

        private readonly TranslationString _resetChangesCaption = new TranslationString("Reset changes");

        private readonly TranslationString _resetSelectedChanges =
            new TranslationString("Are you sure you want to reset all selected files?");

        private readonly TranslationString _resetStageChunkOfFileCaption = new TranslationString("Unstage chunk of file");
        private readonly TranslationString _stageChunkOfFileCaption = new TranslationString("Stage chunk of file");
        private readonly TranslationString _stageDetails = new TranslationString("Stage Details");
        private readonly TranslationString _stageFiles = new TranslationString("Stage {0} files");

        private readonly SynchronizationContext _syncContext;
        public bool NeedRefresh;
        private GitItemStatus _currentItem;
        private bool _currentItemStaged;

        public FormCommit()
        {
            _syncContext = SynchronizationContext.Current;

            InitializeComponent();
            Translate();

            SolveMergeconflicts.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);

            SelectedDiff.ExtraDiffArgumentsChanged += SelectedDiffExtraDiffArgumentsChanged;

            CloseCommitDialogTooltip.SetToolTip(CloseDialogAfterCommit, _closeDialogAfterCommitTooltip.Text);

            CloseDialogAfterCommit.Checked = Settings.CloseCommitDialogAfterCommit;

            Unstaged.SetNoFilesText(_noUnstagedChanges.Text);
            Staged.SetNoFilesText(_noStagedChanges.Text);
            Message.SetEmptyMessage(_enterCommitMessageHint.Text);

            Unstaged.SelectedIndexChanged += UntrackedSelectionChanged;
            Staged.SelectedIndexChanged += TrackedSelectionChanged;
        }


        /// <summary>
        ///   Releases unmanaged resources and performs other cleanup operations before the
        ///   <see cref = "FormCommit" /> is reclaimed by garbage collection.
        /// </summary>
        ~FormCommit()
        {
            _gitGetUnstagedCommand.Kill();
        }

        private void Initialize()
        {
            UnstageFiles.Enabled = false;
            AddFiles.Enabled = false;
            filesListedToCommitToolStripMenuItem.Enabled = false;
            workingToolStripMenuItem.Enabled = false;

            Cursor.Current = Cursors.WaitCursor;

            // Load unstaged files
            _gitGetUnstagedCommand.Exited += GitCommandsExited;
            var allChangedFilesCmd =
                GitCommandHelpers.GetAllChangedFilesCmd(
                    !showIgnoredFilesToolStripMenuItem.Checked,
                    showUntrackedFilesToolStripMenuItem.Checked);
            _gitGetUnstagedCommand.CmdStartProcess(Settings.GitCommand, allChangedFilesCmd);
            Loading.Visible = true;
            AddFiles.Enabled = false;

            InitializedStagedAsync();

            Commit.Focus();
            AcceptButton = Commit;
            Cursor.Current = Cursors.Default;
        }

        private void InitializedStaged()
        {
            Cursor.Current = Cursors.WaitCursor;
            Staged.GitItemStatuses = null;
            SolveMergeconflicts.Visible = GitCommandHelpers.InTheMiddleOfConflictedMerge();
            Staged.GitItemStatuses = GitCommandHelpers.GetStagedFiles();
            Cursor.Current = Cursors.Default;
        }

        private void InitializedStagedAsync()
        {
            Cursor.Current = Cursors.WaitCursor;

            ThreadPool.QueueUserWorkItem(
                o =>
                {
                    var inTheMiddleOfConflictedMerge =
                        GitCommandHelpers.InTheMiddleOfConflictedMerge();
                    var stagedFiles = GitCommandHelpers.GetStagedFiles();

                    _syncContext.Post(
                        state1 =>
                        {
                            Staged.GitItemStatuses = null;
                            SolveMergeconflicts.Visible = inTheMiddleOfConflictedMerge;
                            Staged.GitItemStatuses = stagedFiles;
                        }, null);
                });
            Cursor.Current = Cursors.Default;
        }


        /// <summary>
        ///   Loads the unstaged output.
        ///   This method is passed in to the SetTextCallBack delegate
        ///   to set the Text property of textBox1.
        /// </summary>
        private void LoadUnstagedOutput()
        {
            Unstaged.GitItemStatuses =
                GitCommandHelpers.GetAllChangedFilesFromString(_gitGetUnstagedCommand.Output.ToString());
            Loading.Visible = false;
            AddFiles.Enabled = true;

            UnstageFiles.Enabled = true;
            AddFiles.Enabled = true;
            filesListedToCommitToolStripMenuItem.Enabled = true;
            workingToolStripMenuItem.Enabled = true;
        }

        protected void ShowChanges(GitItemStatus item, bool staged)
        {
            _currentItem = item;
            _currentItemStaged = staged;

            if (item == null)
                return;

            if (item.Name.EndsWith(".png"))
            {
                SelectedDiff.ViewFile(item.Name);
            }
            else if (item.IsTracked)
            {
                SelectedDiff.ViewCurrentChanges(item.Name, staged);
            }
            else
            {
                SelectedDiff.ViewFile(item.Name);
            }
        }

        private void TrackedSelectionChanged(object sender, EventArgs e)
        {
            if (Staged.SelectedItems.Count == 0)
                return;

            Unstaged.SelectedItem = null;
            ShowChanges(Staged.SelectedItems[0], true);
        }

        private void UntrackedSelectionChanged(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count == 0)
                return;

            Staged.SelectedItem = null;
            ShowChanges(Unstaged.SelectedItems[0], false);
        }

        private void CommitClick(object sender, EventArgs e)
        {
            CheckForStagedAndCommit(false, false);
        }

        private void CheckForStagedAndCommit(bool amend, bool push)
        {
            if (Staged.GitItemStatuses.Count == 0)
            {
                if (MessageBox.Show(_noFilesStaged.Text, _noStagedChanges.Text, MessageBoxButtons.YesNo) ==
                    DialogResult.No)
                    return;
            }

            DoCommit(amend, push);
        }

        private void DoCommit(bool amend, bool push)
        {
            if (GitCommandHelpers.InTheMiddleOfConflictedMerge())
            {
                MessageBox.Show(_mergeConflicts.Text, _mergeConflictsCaption.Text);
                return;
            }
            if (Message.Text.Length < 3)
            {
                MessageBox.Show(_enterCommitMessage.Text, _enterCommitMessageCaption.Text);
                return;
            }

            if (GitCommandHelpers.GetSelectedBranch().Equals("(no branch)", StringComparison.OrdinalIgnoreCase) &&
                MessageBox.Show(_notOnBranch.Text, _notOnBranchCaption.Text, MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            try
            {
                SetCommitMessageFromTextBox(Message.Text);

                var form = new FormProcess(GitCommandHelpers.CommitCmd(amend));
                form.ShowDialog();

                NeedRefresh = true;

                if (form.ErrorOccurred())
                    return;

                Message.Text = string.Empty;

                if (push)
                {
                    GitUICommands.Instance.StartPushDialog(true);
                }

                if (CloseDialogAfterCommit.Checked)
                {
                    Close();
                    return;
                }

                foreach (var gitItemStatus in Unstaged.GitItemStatuses)
                {
                    if (gitItemStatus.IsTracked)
                    {
                        InitializedStaged();
                        return;
                    }
                }

                Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Exception: {0}", e.Message));
            }
        }

        private void ScanClick(object sender, EventArgs e)
        {
            Scan.Enabled = false;
            Initialize();
            Scan.Enabled = true;
        }

        private void StageClick(object sender, EventArgs e)
        {
            Stage(Unstaged.SelectedItems);
        }

        private void Stage(ICollection<GitItemStatus> gitItemStatusses)
        {
            UnstageFiles.Enabled = false;
            AddFiles.Enabled = false;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                progressBar.Visible = true;
                progressBar.Maximum = gitItemStatusses.Count * 2;
                progressBar.Value = 0;

                var files = new List<GitItemStatus>();

                foreach (var gitItemStatus in gitItemStatusses)
                {
                    progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                    files.Add(gitItemStatus);
                }

                if (Settings.ShowErrorsWhenStagingFiles)
                {
                    FormStatus.ProcessStart processStart =
                        form =>
                        {
                            form.AddOutput(string.Format(_stageFiles.Text,
                                                         files.Count));
                            var output = GitCommandHelpers.StageFiles(files);
                            form.AddOutput(output);
                            form.Done(string.IsNullOrEmpty(output));
                        };
                    var process = new FormStatus(processStart, null) { Text = _stageDetails.Text };
                    process.ShowDialogOnError();
                }
                else
                {
                    GitCommandHelpers.StageFiles(files);
                }

                InitializedStaged();
                var stagedFiles = (List<GitItemStatus>)Staged.GitItemStatuses;
                var unStagedFiles = (List<GitItemStatus>)Unstaged.GitItemStatuses;
                Unstaged.GitItemStatuses = null;

                unStagedFiles.RemoveAll(item => stagedFiles.Exists(i => i.Name == item.Name) && files.Exists(i => i.Name == item.Name));

                Unstaged.GitItemStatuses = unStagedFiles;

                progressBar.Value = progressBar.Maximum;

                progressBar.Visible = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            UnstageFiles.Enabled = true;
            AddFiles.Enabled = true;

            Commit.Enabled = true;
            Amend.Enabled = true;
            AcceptButton = Commit;
            Commit.Focus();
            Cursor.Current = Cursors.Default;
        }

        private void UnstageFilesClick(object sender, EventArgs e)
        {
            UnstageFiles.Enabled = false;
            AddFiles.Enabled = false;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (Staged.GitItemStatuses.Count > 10 && Staged.SelectedItems.Count == Staged.GitItemStatuses.Count)
                {
                    Loading.Visible = true;
                    GitCommandHelpers.ResetMixed("HEAD");
                    Initialize();
                }
                else
                {
                    progressBar.Visible = true;
                    progressBar.Maximum = Staged.SelectedItems.Count * 2;
                    progressBar.Value = 0;

                    var files = new List<GitItemStatus>();
                    var allFiles = new List<GitItemStatus>();

                    foreach (var item in Staged.SelectedItems)
                    {
                        progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                        if (!item.IsNew)
                        {
                            progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                            GitCommandHelpers.UnstageFileToRemove(item.Name);
                        }
                        else
                        {
                            files.Add(item);
                        }
                        allFiles.Add(item);
                    }

                    GitCommandHelpers.UnstageFiles(files);

                    InitializedStaged();
                    var stagedFiles = (List<GitItemStatus>)Staged.GitItemStatuses;
                    var unStagedFiles = (List<GitItemStatus>)Unstaged.GitItemStatuses;
                    Unstaged.GitItemStatuses = null;
                    foreach (var item in allFiles)
                    {
                        var item1 = item;
                        if (stagedFiles.Exists(i => i.Name == item1.Name))
                            continue;

                        var item2 = item;
                        if (unStagedFiles.Exists(i => i.Name == item2.Name))
                            continue;

                        if (item.IsNew && !item.IsChanged && !item.IsDeleted)
                            item.IsTracked = false;
                        else
                            item.IsTracked = true;

                        unStagedFiles.Add(item);
                    }
                    Staged.GitItemStatuses = stagedFiles;
                    Unstaged.GitItemStatuses = unStagedFiles;

                    progressBar.Value = progressBar.Maximum;
                }
                progressBar.Visible = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            UnstageFiles.Enabled = true;
            AddFiles.Enabled = true;
            Cursor.Current = Cursors.Default;
        }


        private void ResetSoftClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItem == null ||
                MessageBox.Show(_resetChanges.Text, _resetChangesCaption.Text, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

            var deleteNewFiles = false;
            var askToDeleteNewFiles = true;
            var output = new StringBuilder();
            foreach (var item in Unstaged.SelectedItems)
            {
                if (item.IsNew)
                {
                    if (!deleteNewFiles && askToDeleteNewFiles)
                    {
                        var result = MessageBox.Show(_alsoDeleteUntrackedFiles.Text,
                                                     _alsoDeleteUntrackedFilesCaption.Text, MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                            deleteNewFiles = true;

                        askToDeleteNewFiles = false;
                    }

                    if (deleteNewFiles)
                        File.Delete(Settings.WorkingDir + item.Name);
                }
                else
                {
                    output.Append(GitCommandHelpers.ResetFile(item.Name));
                }
            }

            if (!string.IsNullOrEmpty(output.ToString()))
                MessageBox.Show(output.ToString(), "Reset changes");

            Initialize();
        }

        private void DeleteFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                SelectedDiff.ViewText("", "");
                if (Unstaged.SelectedItem == null ||
                    MessageBox.Show(_deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                    return;

                foreach (var item in Unstaged.SelectedItems)
                    File.Delete(Settings.WorkingDir + item.Name);

                Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_deleteFailed.Text + Environment.NewLine + ex.Message);
            }
        }

        private void SolveMergeConflictsClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog())
                Initialize();
        }

        private void DeleteSelectedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(_deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

            try
            {
                foreach (var gitItemStatus in Unstaged.SelectedItems)
                    File.Delete(Settings.WorkingDir + gitItemStatus.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_deleteFailed.Text + Environment.NewLine + ex);
            }
            Initialize();
        }

        private void ResetSelectedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(_resetSelectedChanges.Text, _resetChangesCaption.Text, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

            foreach (var gitItemStatus in Unstaged.SelectedItems)
            {
                GitCommandHelpers.ResetFile(gitItemStatus.Name);
            }
            Initialize();
        }

        private void ResetAlltrackedChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetClick(null, null);
        }

        private void EditGitIgnoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartEditGitIgnoreDialog();
            Initialize();
        }

        private void StageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            Stage(Unstaged.GitItemStatuses);
        }

        private void UnstageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommandHelpers.ResetMixed("HEAD");
            Initialize();
        }

        private void FormCommitShown(object sender, EventArgs e)
        {
            Initialize();

            var message = GitCommandHelpers.GetMergeMessage();

            if (string.IsNullOrEmpty(message) && File.Exists(GitCommands.Commit.GetCommitMessagePath()))
                message = File.ReadAllText(GitCommands.Commit.GetCommitMessagePath(), Settings.Encoding);
            if (!string.IsNullOrEmpty(message))
                Message.Text = message;

            ThreadPool.QueueUserWorkItem(
                o =>
                {
                    var text =
                        string.Format("Commit to {0} ({1})", GitCommandHelpers.GetSelectedBranch(),
                                      Settings.WorkingDir);

                    _syncContext.Post(state1 => Text = text, null);
                });
        }

        public static void SetCommitMessageFromTextBox(string commitMessageText)
        {
            //Save last commit message in settings. This way it can be used in multiple repositories.
            Settings.LastCommitMessage = commitMessageText;

            var path = Settings.WorkingDirGitDir() + Settings.PathSeparator.ToString() + "COMMITMESSAGE";

            //Commit messages are UTF-8 by default unless otherwise in the config file.
            //The git manual states:
            //  git commit and git commit-tree issues a warning if the commit log message 
            //  given to it does not look like a valid UTF-8 string, unless you 
            //  explicitly say your project uses a legacy encoding. The way to say 
            //  this is to have i18n.commitencoding in .git/config file, like this:...
            Encoding encoding;
            string encodingString;
            encodingString = GitCommandHelpers.GetLocalConfig().GetValue("i18n.commitencoding");
            if (string.IsNullOrEmpty(encodingString))
                encodingString = GitCommandHelpers.GetGlobalConfig().GetValue("i18n.commitencoding");

            if (!string.IsNullOrEmpty(encodingString))
            {
                try
                {
                    encoding = Encoding.GetEncoding(encodingString);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + "Unsupported encoding set in git config file: " + encodingString + Environment.NewLine + "Please check the setting i18n.commitencoding in your local and/or global config files. Commit aborted.");
                    return;
                }
            }
            else
            {
                encoding = new UTF8Encoding(false);
            }

            using (var textWriter = new StreamWriter(path, false, encoding))
            {
                var lineNumber = 0;
                foreach (var line in commitMessageText.Split('\n'))
                {
                    if (lineNumber == 1 && !String.IsNullOrEmpty(line))
                        textWriter.WriteLine();

                    textWriter.WriteLine(line);
                    lineNumber++;
                }
            }
        }

        private void FormCommitFormClosing(object sender, FormClosingEventArgs e)
        {
            GitCommands.Commit.SetCommitMessage(Message.Text);
            SavePosition("commit");
        }

        private void DeleteAllUntrackedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                _deleteUntrackedFiles.Text,
                _deleteUntrackedFilesCaption.Text,
                MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;
            new FormProcess("clean -f").ShowDialog();
            Initialize();
        }

        private void StageChunkOfFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count != 1)
            {
                MessageBox.Show(_onlyStageChunkOfSingleFileError.Text, _stageChunkOfFileCaption.Text);
                return;
            }

            foreach (var gitItemStatus in Unstaged.SelectedItems)
            {
                GitCommandHelpers.RunRealCmd(Settings.GitCommand,
                                                   string.Format("add -p \"{0}\"", gitItemStatus.Name));
                Initialize();
            }
        }

        private void ShowIgnoredFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            showIgnoredFilesToolStripMenuItem.Checked = !showIgnoredFilesToolStripMenuItem.Checked;
            ScanClick(null, null);
        }

        private void CommitMessageToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            commitMessageToolStripMenuItem.DropDownItems.Clear();
            AddCommitMessageToMenu(Settings.LastCommitMessage);

            string localLastCommitMessage = GitCommandHelpers.GetPreviousCommitMessage(0);
            if (!localLastCommitMessage.Trim().Equals(Settings.LastCommitMessage.Trim()))
                AddCommitMessageToMenu(localLastCommitMessage);
            AddCommitMessageToMenu(GitCommandHelpers.GetPreviousCommitMessage(1));
            AddCommitMessageToMenu(GitCommandHelpers.GetPreviousCommitMessage(2));
            AddCommitMessageToMenu(GitCommandHelpers.GetPreviousCommitMessage(3));
        }

        private void AddCommitMessageToMenu(string commitMessage)
        {
            if (string.IsNullOrEmpty(commitMessage))
                return;

            var toolStripItem =
                new ToolStripMenuItem
                {
                    Tag = commitMessage,
                    Text =
                        commitMessage.Substring(0,
                                                Math.Min(Math.Min(50, commitMessage.Length),
                                                         commitMessage.Contains("\n") ? commitMessage.IndexOf('\n') : 99)) +
                        "..."
                };

            commitMessageToolStripMenuItem.DropDownItems.Add(toolStripItem);
        }

        private void CommitMessageToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Message.Text = (string)e.ClickedItem.Tag;
        }

        private void AddFileTogitignoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count == 0)
                return;

            SelectedDiff.ViewText("", "");
            var item = Unstaged.SelectedItem;
            new FormAddToGitIgnore(item.Name).ShowDialog();
            Initialize();
        }

        private void SelectedDiffExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowChanges(_currentItem, _currentItemStaged);
        }

        private void RescanChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ScanClick(null, null);
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count == 0)
                return;

            var item = Unstaged.SelectedItem;
            var fileName = item.Name;

            Process.Start((Settings.WorkingDir + fileName).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
        }

        private void OpenWithToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count == 0)
                return;

            var item = Unstaged.SelectedItem;
            var fileName = item.Name;

            OpenWith.OpenAs(Settings.WorkingDir + fileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
        }

        private void CloseDialogAfterCommitCheckedChanged(object sender, EventArgs e)
        {
            Settings.CloseCommitDialogAfterCommit = CloseDialogAfterCommit.Checked;
        }

        private void FilenameToClipboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count == 0)
                return;

            StringBuilder fileNames = new StringBuilder();
            foreach (var item in Unstaged.SelectedItems)
            {
                //Only use appendline when multiple items are selected.
                //This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                    fileNames.AppendLine();

                fileNames.Append((Settings.WorkingDir + item.Name).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
            }
            Clipboard.SetText(fileNames.ToString());
        }

        private void OpenWithDifftoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count == 0)
                return;

            var item = Unstaged.SelectedItem;
            var fileName = item.Name;

            var cmdOutput = GitCommandHelpers.OpenWithDifftool(fileName);

            if (!string.IsNullOrEmpty(cmdOutput))
                MessageBox.Show(cmdOutput);
        }


        private void ResetPartOfFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count != 1)
            {
                MessageBox.Show(_onlyStageChunkOfSingleFileError.Text, _resetStageChunkOfFileCaption.Text);
                return;
            }

            foreach (var gitItemStatus in Unstaged.SelectedItems)
            {
                GitCommandHelpers.RunRealCmd
                    (Settings.GitCommand,
                     string.Format("checkout -p \"{0}\"", gitItemStatus.Name));
                Initialize();
            }
        }

        private void FormCommitLoad(object sender, EventArgs e)
        {
            RestorePosition("commit");
        }

        private void GitCommandsExited(object sender, EventArgs e)
        {
            _syncContext.Post(_ => LoadUnstagedOutput(), null);
        }

        private void ResetClick(object sender, EventArgs e)
        {
            if (!Abort.AbortCurrentAction())
                return;

            Initialize();
            NeedRefresh = true;
        }

        private void AmendClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Message.Text))
            {
                Message.Text = GitCommandHelpers.GetPreviousCommitMessage(0);
                return;
            }

            if (MessageBox.Show(_amendCommit.Text, _amendCommitCaption.Text, MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
                DoCommit(true, false);
        }

        private void CancelClick(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowUntrackedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            showUntrackedFilesToolStripMenuItem.Checked = !showUntrackedFilesToolStripMenuItem.Checked;
            ScanClick(null, null);
        }

        private void editFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = Unstaged.SelectedItem;
            var fileName = Settings.WorkingDir + item.Name;

            new FormEditor(fileName).ShowDialog();

            UntrackedSelectionChanged(null, null);
        }

        private void CommitAndPush_Click(object sender, EventArgs e)
        {
            CheckForStagedAndCommit(false, true);
        }
    }
}