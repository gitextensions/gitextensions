using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionDiff : GitModuleControl
    {
        private readonly TranslationString _saveFileFilterCurrentFormat = new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new TranslationString("All files");
        private readonly TranslationString _diffNoSelection = new TranslationString("Diff (no selection)");
        private readonly TranslationString _diffParentWithSelection = new TranslationString("Diff (A: parent --> B: selection)");
        private readonly TranslationString _diffTwoSelected = new TranslationString("Diff (A: first --> B: second)");
        private readonly TranslationString _diffNotSupported = new TranslationString("Diff (not supported)");
        private readonly TranslationString _deleteSelectedFilesCaption = new TranslationString("Delete");
        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want delete the selected file(s)?");
        private readonly TranslationString _deleteFailed = new TranslationString("Delete file failed");

        private RevisionGrid _revisionGrid;
        private RevisionFileTree _revisionFileTree;
        private string _oldRevision;
        private GitItemStatus _oldDiffItem;
        private RevisionDiffController _revisionDiffController;


        public RevisionDiff()
        {
            InitializeComponent();
            Translate();

            DiffFiles.FilterVisible = true;
            DiffText.Font = AppSettings.DiffFont;

            GotFocus += (s, e) => DiffFiles.Focus();
            Load += (s, e) =>
            {
                _revisionDiffController = new RevisionDiffController(_revisionGrid, DiffFiles, DiffText);
                DiffFiles.DescribeRevision = _revisionDiffController.DescribeRevision;
                DiffText.SetFileLoader(_revisionDiffController.GetNextPatchFile);
            };
        }

        public void ForceRefreshRevisions()
        {
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count != 0)
            {
                _oldRevision = revisions[0].Guid;
                _oldDiffItem = DiffFiles.SelectedItem;
            }
            else
            {
                _oldRevision = null;
                _oldDiffItem = null;
            }
        }

        public string GetTabText()
        {
            var revisions = _revisionGrid.GetSelectedRevisions();

            DiffText.SaveCurrentScrollPos();
            DiffFiles.SetDiffs(revisions);
            if (_oldDiffItem != null && revisions.Count > 0 && revisions[0].Guid == _oldRevision)
            {
                DiffFiles.SelectedItem = _oldDiffItem;
                _oldDiffItem = null;
                _oldRevision = null;
            }

            switch (revisions.Count)
            {
                case 0:
                    return _diffNoSelection.Text;

                case 1: // diff "parent" --> "selected revision"
                    var revision = revisions[0];
                    if (revision != null && revision.HasParent)
                        return _diffParentWithSelection.Text;
                    break;

                case 2: // diff "first clicked revision" --> "second clicked revision"
                    bool artificialRevSelected = revisions[0].IsArtificial() || revisions[1].IsArtificial();
                    if (!artificialRevSelected)
                        return _diffTwoSelected.Text;
                    break;

            }
            return _diffNotSupported.Text;
        }

        public void Bind(RevisionGrid revisionGrid, RevisionFileTree revisionFileTree)
        {
            _revisionGrid = revisionGrid;
            _revisionFileTree = revisionFileTree;
        }

        public void InitSplitterManager(SplitterManager splitterManager)
        {
            splitterManager.AddSplitter(DiffSplitContainer, "DiffSplitContainer");
        }

        public void ReloadHotkeys()
        {
            DiffText.ReloadHotkeys();
            //TBD Shortcut key should be implemented but HotKeyManager is inaccessible in FormBrowse
        }


        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            _revisionDiffController.ShowSelectedFileDiff();
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            if (AppSettings.OpenSubmoduleDiffInSeparateWindow && DiffFiles.SelectedItem.IsSubmodule)
            {
                var submoduleName = DiffFiles.SelectedItem.Name;
                DiffFiles.SelectedItem.SubmoduleStatus.ContinueWith(
                    (t) =>
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = Application.ExecutablePath;
                        process.StartInfo.Arguments = "browse -commit=" + t.Result.Commit;
                        process.StartInfo.WorkingDirectory = Path.Combine(Module.WorkingDir, submoduleName.EnsureTrailingPathSeparator());
                        process.Start();
                    });
            }
            else
            {

                UICommands.StartFileHistoryDialog(this, (DiffFiles.SelectedItem).Name);
            }
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses == null || !DiffFiles.GitItemStatuses.Any())
                DiffText.ViewPatch(String.Empty);
        }

        private void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            _revisionDiffController.ShowSelectedFileDiff();
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // switch to view (and fills the first level of file tree data model if not already done)
            (FindForm() as FormBrowse)?.ExecuteCommand(FormBrowse.Commands.FocusFileTree);
            _revisionFileTree.ExpandToFile(DiffFiles.SelectedItems.First().Name);
        }

        //
        // diff context menu
        //
        private void DiffContextMenu_Opening(object sender, CancelEventArgs e)
        {
            bool openWithDifftool, saveAs,
            stage, unstage, cherryPick, diffShowInFileTree,
            fileHistory, blame, resetFileTo, diffEditFile,
            diffDeleteFile, submodule, openContainingFolder;

            _revisionDiffController.DiffContextMenu_Opening(out openWithDifftool, out saveAs,
            out stage, out unstage, out cherryPick, out diffShowInFileTree,
            out fileHistory, out blame, out resetFileTo, out diffEditFile,
            out diffDeleteFile, out submodule, out openContainingFolder);

            openWithDifftoolToolStripMenuItem.Enabled = openWithDifftool;
            saveAsToolStripMenuItem1.Visible = saveAs;

            stageFileToolStripMenuItem.Visible = stage;
            unstageFileToolStripMenuItem.Visible = unstage;

            cherryPickSelectedDiffFileToolStripMenuItem.Visible = cherryPick;
            //Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
            diffShowInFileTreeToolStripMenuItem.Visible = diffShowInFileTree;
            fileHistoryDiffToolstripMenuItem.Enabled = fileHistory;
            blameToolStripMenuItem.Enabled = blame;
            resetFileToToolStripMenuItem.Enabled = resetFileTo;
            this.diffEditFileToolStripMenuItem.Visible = diffEditFile;
            this.diffDeleteFileToolStripMenuItem.Visible = diffDeleteFile;

            this.diffCommitSubmoduleChanges.Visible =
                this.diffResetSubmoduleChanges.Visible =
                this.diffStashSubmoduleChangesToolStripMenuItem.Visible =
                this.diffUpdateSubmoduleMenuItem.Visible =
                this.diffSubmoduleSummaryMenuItem.Visible = submodule;
            this.diffUpdateSubmoduleMenuItem.Visible = false; //TBD, requires coordination to FormBrowse and Controller

            this.diffToolStripSeparator13.Visible = diffEditFile || diffDeleteFile || submodule;

            openContainingFolderToolStripMenuItem.Enabled = openContainingFolder;
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
                GitRevision rev = (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid)) ?
                    null : revisions[0];

                UICommands.StartFileHistoryDialog(this, item.Name, rev, true, true);
            }
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            _revisionDiffController.StageFiles();
            //TBD RefreshRevisions();
        }

        private void UnstageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            _revisionDiffController.UnstageFiles();
            //TBD RefreshRevisions();
        }

        private void cherryPickSelectedDiffFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiffText.CherryPickAllChanges();
        }

        private void copyFilenameToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormBrowse.CopyFullPathToClipboard(DiffFiles, Module);
        }

        private void findInDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var candidates = DiffFiles.GitItemStatuses;

            Func<string, IList<GitItemStatus>> FindDiffFilesMatches = (string name) =>
            {

                string nameAsLower = name.ToLower();

                return candidates.Where(item =>
                {
                    return item.Name != null && item.Name.ToLower().Contains(nameAsLower)
                        || item.OldName != null && item.OldName.ToLower().Contains(nameAsLower);
                }
                    ).ToList();
            };

            GitItemStatus selectedItem;
            using (var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = FindForm()
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }
            if (selectedItem != null)
            {
                DiffFiles.SelectedItem = selectedItem;
            }
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
                GitRevision rev = (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid)) ?
                    null : revisions[0];

                UICommands.StartFileHistoryDialog(this, item.Name, rev, false);
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            GitUIExtensions.DiffWithRevisionKind diffKind;

            if (sender == aLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffALocal;
            else if (sender == bLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffBLocal;
            else if (sender == parentOfALocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffAParentLocal;
            else if (sender == parentOfBLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffBParentLocal;
            else
            {
                Debug.Assert(sender == aBToolStripMenuItem, "Not implemented DiffWithRevisionKind: " + sender);
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffAB;
            }

            foreach (var itemWithParent in DiffFiles.SelectedItemsWithParent)
            {
                GitItemStatus selectedItem = itemWithParent.Item;
                string parentGuid = _revisionGrid.GetSelectedRevisions().Count() == 1 ? itemWithParent.ParentGuid : null;

                _revisionGrid.OpenWithDifftool(selectedItem.Name, selectedItem.OldName, diffKind, parentGuid);
            }
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            bool localExists;
            bool enableDiffDropDown;
            bool showParentItems;

            _revisionDiffController.ShowEnableDiffDropDown(out enableDiffDropDown, out showParentItems, out localExists);

            aBToolStripMenuItem.Enabled = enableDiffDropDown;

            //enable *<->Local items only when local file exists
            bLocalToolStripMenuItem.Enabled =
                aLocalToolStripMenuItem.Enabled =
                parentOfALocalToolStripMenuItem.Enabled =
                parentOfBLocalToolStripMenuItem.Enabled = localExists;

            parentOfALocalToolStripMenuItem.Visible = 
                parentOfBLocalToolStripMenuItem.Visible = showParentItems;

            return;
        }

        private void resetFileToFirstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 2 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            _revisionDiffController.ResetSelectedItemsTo(revisions[1].Guid, false);
        }

        private void resetFileToParentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 1 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            if (!revisions[0].HasParent)
            {
                throw new ApplicationException("This menu should be disabled for revisions that don't have a parent.");
            }

            _revisionDiffController.ResetSelectedItemsTo(revisions[0].FirstParentGuid, false);
        }

        private void resetFileToSecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 2 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            _revisionDiffController.ResetSelectedItemsTo(revisions[0].Guid, true);
        }

        private void resetFileToSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 1 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            _revisionDiffController.ResetSelectedItemsTo(revisions[0].Guid, true);
        }

        private void resetFileToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
            int selectedRevsCount = revisions.Count;

            if (selectedRevsCount == 1)
            {
                resetFileToSelectedToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToSelectedToolStripMenuItem.Name, resetFileToSelectedToolStripMenuItem);
                resetFileToSelectedToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(revisions[0]).ShortenTo(50) + ")";

                if (revisions[0].HasParent)
                {
                    resetFileToParentToolStripMenuItem.Visible = true;
                    TranslateItem(resetFileToParentToolStripMenuItem.Name, resetFileToParentToolStripMenuItem);
                    GitRevision parentRev = _revisionGrid.GetRevision(revisions[0].FirstParentGuid);
                    if (parentRev != null)
                    {
                        resetFileToParentToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(parentRev).ShortenTo(50) + ")";
                    }
                }
                else
                {
                    resetFileToParentToolStripMenuItem.Visible = false;
                }
            }
            else
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
                resetFileToParentToolStripMenuItem.Visible = false;
            }

            if (selectedRevsCount == 2)
            {
                resetFileToFirstToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToFirstToolStripMenuItem.Name, resetFileToFirstToolStripMenuItem);
                resetFileToFirstToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(revisions[1]).ShortenTo(50) + ")";

                resetFileToSecondToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToSecondToolStripMenuItem.Name, resetFileToSecondToolStripMenuItem);
                resetFileToSecondToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(revisions[0]).ShortenTo(50) + ")";
            }
            else
            {
                resetFileToFirstToolStripMenuItem.Visible = false;
                resetFileToSecondToolStripMenuItem.Visible = false;
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0)
                return;

            if (DiffFiles.SelectedItem == null)
                return;

            GitItemStatus item = DiffFiles.SelectedItem;

            var fullName = Path.Combine(Module.WorkingDir, item.Name);
            using (var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = GitCommandHelpers.GetFileExtension(fullName),
                    AddExtension = true
                })
            {
                fileDialog.Filter =
                    _saveFileFilterCurrentFormat.Text + " (*." +
                    fileDialog.DefaultExt + ")|*." +
                    fileDialog.DefaultExt +
                    "|" + _saveFileFilterAllFiles.Text + " (*.*)|*.*";

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, string.Format("{0}:\"{1}\"", revisions[0].Guid, item.Name));
                }
            }
        }
        
        private bool DeleteSelectedDiffFiles()
        {
            if (DiffFiles.Focused)
            {
                return DeleteSelectedFiles();
            }
            return false;
        }

        public bool DeleteSelectedFiles()
        {
            bool res;
            try
            {
                if (!_revisionDiffController.CheckDeleteFiles() ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                {
                    return false;
                }
                res = _revisionDiffController.DeleteSelectedFiles();
                //TBD RefreshRevisions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message);
                return false;
            }

            return res;
        }

        private void diffDeleteFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            DeleteSelectedFiles();
        }

        private void diffEditFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = DiffFiles.SelectedItem;
            var fileName = Path.Combine(Module.WorkingDir, item.Name);

            UICommands.StartFileEditorDialog(fileName);
            //TBD RefreshRevisions();
        }

        private void diffCommitSubmoduleChanges_Click(object sender, EventArgs e)
        {
            GitUICommands submodulCommands = new GitUICommands(Module.WorkingDir + DiffFiles.SelectedItem.Name.EnsureTrailingPathSeparator());
            submodulCommands.StartCommitDialog(this, false);
            //TBD RefreshRevisions();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
                return;

            _revisionDiffController.ResetSubmoduleChanges(resetType);
            //TBD RefreshRevisions();
        }

        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var unStagedFiles = DiffFiles.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                //TBD FormProcess.ShowDialog(this, GitCommandHelpers.SubmoduleUpdateCmd(item.Name));
            }

            //TBD RefreshRevisions();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var unStagedFiles = DiffFiles.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitUICommands uiCmds = new GitUICommands(Module.GetSubmodule(item.Name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            //TBD RefreshRevisions();
        }

        private void diffSubmoduleSummaryMenuItem_Click(object sender, EventArgs e)
        {
            string summary = Module.GetSubmoduleSummary(DiffFiles.SelectedItem.Name);
            using (var frm = new FormEdit(summary)) frm.ShowDialog(this);
        }
    }
}
