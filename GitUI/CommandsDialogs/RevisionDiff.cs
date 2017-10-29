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

        private RevisionGrid _revisionGrid;
        private RevisionFileTree _revisionFileTree;
        private string _oldRevision;
        private GitItemStatus _oldDiffItem;


        public RevisionDiff()
        {
            InitializeComponent();
            Translate();

            DiffFiles.FilterVisible = true;
            DiffFiles.DescribeRevision = DescribeRevision;
            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.DiffFont;

            GotFocus += (s, e) => DiffFiles.Focus();
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
        }


        private string DescribeRevision(string sha1)
        {
            var revision = _revisionGrid.GetRevision(sha1);
            if (revision == null)
            {
                return sha1.ShortenTo(8);
            }

            return _revisionGrid.DescribeRevision(revision);
        }

        private static int GetNextIdx(int curIdx, int maxIdx, bool searchBackward)
        {
            if (searchBackward)
            {
                if (curIdx == 0)
                {
                    curIdx = maxIdx;
                }
                else
                {
                    curIdx--;
                }
            }
            else
            {
                if (curIdx == maxIdx)
                {
                    curIdx = 0;
                }
                else
                {
                    curIdx++;
                }
            }
            return curIdx;
        }

        private Tuple<int, string> GetNextPatchFile(bool searchBackward)
        {
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count == 0)
                return null;
            int idx = DiffFiles.SelectedIndex;
            if (idx == -1)
                return new Tuple<int, string>(idx, null);

            idx = GetNextIdx(idx, DiffFiles.GitItemStatuses.Count() - 1, searchBackward);
            DiffFiles.SetSelectedIndex(idx, notify: false);
            return new Tuple<int, string>(idx, GetSelectedPatch(revisions, DiffFiles.SelectedItem));
        }

        private string GetSelectedPatch(IList<GitRevision> revisions, GitItemStatus file)
        {
            string firstRevision = revisions.Count > 0 ? revisions[0].Guid : null;
            string secondRevision = revisions.Count == 2 ? revisions[1].Guid : null;
            return DiffText.GetSelectedPatch(firstRevision, secondRevision, file);
        }

        private void ResetSelectedItemsTo(string revision, bool actsAsChild)
        {
            var selectedItems = DiffFiles.SelectedItems;
            IEnumerable<GitItemStatus> itemsToCheckout;
            if (actsAsChild)
            {
                var deletedItems = selectedItems.Where(item => item.IsDeleted);
                Module.RemoveFiles(deletedItems.Select(item => item.Name), false);
                itemsToCheckout = selectedItems.Where(item => !item.IsDeleted);
            }
            else //acts as parent
            {
                //if file is new to the parent, it has to be removed
                var addedItems = selectedItems.Where(item => item.IsNew);
                Module.RemoveFiles(addedItems.Select(item => item.Name), false);
                itemsToCheckout = selectedItems.Where(item => !item.IsNew);
            }

            Module.CheckoutFiles(itemsToCheckout.Select(item => item.Name), revision, false);
        }

        private void ShowSelectedFileDiff()
        {
            if (DiffFiles.SelectedItem == null)
            {
                DiffText.ViewPatch("");
                return;
            }

            var items = _revisionGrid.GetSelectedRevisions();
            if (items.Count() == 1)
            {
                items.Add(new GitRevision(Module, DiffFiles.SelectedItemParent));

                if (!string.IsNullOrWhiteSpace(DiffFiles.SelectedItemParent)
                    && DiffFiles.SelectedItemParent == DiffFiles.CombinedDiff.Text)
                {
                    var diffOfConflict = Module.GetCombinedDiffContent(items.First(), DiffFiles.SelectedItem.Name,
                        DiffText.GetExtraDiffArguments(), DiffText.Encoding);

                    if (string.IsNullOrWhiteSpace(diffOfConflict))
                    {
                        diffOfConflict = Strings.GetUninterestingDiffOmitted();
                    }

                    DiffText.ViewPatch(diffOfConflict);
                    return;
                }
            }
            DiffText.ViewChanges(items, DiffFiles.SelectedItem, String.Empty);
        }


        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
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
            ShowSelectedFileDiff();
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // switch to view (and fills the first level of file tree data model if not already done)
            (FindForm() as FormBrowse)?.ExecuteCommand(FormBrowse.Commands.FocusFileTree);
            _revisionFileTree.ExpandToFile(DiffFiles.SelectedItems.First().Name);
        }

        private void DiffContextMenu_Opening(object sender, CancelEventArgs e)
        {
            bool artificialRevSelected;

            IList<GitRevision> selectedRevisions = _revisionGrid.GetSelectedRevisions();

            if (selectedRevisions.Count == 0)
                artificialRevSelected = false;
            else
                artificialRevSelected = selectedRevisions[0].IsArtificial();
            if (selectedRevisions.Count > 1)
                artificialRevSelected = artificialRevSelected || selectedRevisions[selectedRevisions.Count - 1].IsArtificial();

            // disable items that need exactly one selected item
            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;
            var isCombinedDiff = isExactlyOneItemSelected &&
                DiffFiles.CombinedDiff.Text == DiffFiles.SelectedItemParent;
            var isAnyCombinedDiff = DiffFiles.SelectedItemParents.Any(item => item == DiffFiles.CombinedDiff.Text);
            var enabled = isExactlyOneItemSelected && !isCombinedDiff;
            openWithDifftoolToolStripMenuItem.Enabled = !isAnyCombinedDiff;
            saveAsToolStripMenuItem1.Enabled = enabled;
            cherryPickSelectedDiffFileToolStripMenuItem.Enabled = enabled;
            diffShowInFileTreeToolStripMenuItem.Enabled = isExactlyOneItemSelected;
            fileHistoryDiffToolstripMenuItem.Enabled = isExactlyOneItemSelected;
            blameToolStripMenuItem.Enabled = isExactlyOneItemSelected;
            resetFileToToolStripMenuItem.Enabled = !isCombinedDiff;

            this.diffCommitSubmoduleChanges.Visible =
                this.diffResetSubmoduleChanges.Visible =
                this.diffStashSubmoduleChangesToolStripMenuItem.Visible =
                this.diffUpdateSubmoduleMenuItem.Visible =
                this.diffSubmoduleSummaryMenuItem.Visible =
                isExactlyOneItemSelected && DiffFiles.SelectedItem.IsSubmodule && selectedRevisions[0].Guid == GitRevision.UnstagedGuid;
            this.diffUpdateSubmoduleMenuItem.Visible = false; //TBD

            // openContainingFolderToolStripMenuItem.Enabled or not
            {
                openContainingFolderToolStripMenuItem.Enabled = false;

                foreach (var item in DiffFiles.SelectedItems)
                {
                    string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(Module, item);
                    if (FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                    {
                        openContainingFolderToolStripMenuItem.Enabled = true;
                        break;
                    }
                }
            }
        }

        //
        // diff context menu
        //
        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                    UICommands.StartFileHistoryDialog(this, item.Name, null, false, true);
                else
                    UICommands.StartFileHistoryDialog(this, item.Name, revisions[0], true, true);
            }
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

                if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                    UICommands.StartFileHistoryDialog(this, item.Name);
                else
                    UICommands.StartFileHistoryDialog(this, item.Name, revisions[0], false);
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
            bool artificialRevSelected = false;
            bool enableDiffDropDown = true;
            bool showParentItems = false;

            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count > 0)
            {
                artificialRevSelected = revisions[0].IsArtificial();

                if (revisions.Count == 2)
                {
                    artificialRevSelected = artificialRevSelected || revisions[revisions.Count - 1].IsArtificial();
                    showParentItems = true;
                }
                else
                    enableDiffDropDown = revisions.Count == 1;
            }

            aBToolStripMenuItem.Enabled = enableDiffDropDown;
            bLocalToolStripMenuItem.Enabled = enableDiffDropDown;
            aLocalToolStripMenuItem.Enabled = enableDiffDropDown;
            parentOfALocalToolStripMenuItem.Enabled = enableDiffDropDown;
            parentOfBLocalToolStripMenuItem.Enabled = enableDiffDropDown;

            parentOfALocalToolStripMenuItem.Visible = showParentItems;
            parentOfBLocalToolStripMenuItem.Visible = showParentItems;

            if (!enableDiffDropDown)
                return;
            //enable *<->Local items only when local file exists
            foreach (var item in DiffFiles.SelectedItems)
            {
                string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(Module, item);
                if (File.Exists(filePath))
                {
                    bLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    aLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    parentOfALocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    parentOfBLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    return;
                }
            }
        }

        private void resetFileToFirstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 2 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            ResetSelectedItemsTo(revisions[1].Guid, false);
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

            ResetSelectedItemsTo(revisions[0].GetParentGuid, false);
        }

        private void resetFileToSecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 2 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            ResetSelectedItemsTo(revisions[0].Guid, true);
        }

        private void resetFileToSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count != 1 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            ResetSelectedItemsTo(revisions[0].Guid, true);
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
                    GitRevision parentRev = _revisionGrid.GetRevision(revisions[0].GetParentGuid);
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

        /// <summary>
        private void diffCommitSubmoduleChanges_Click(object sender, EventArgs e)
        {
            GitUICommands submodulCommands = new GitUICommands(Module.WorkingDir + DiffFiles.SelectedItem.Name.EnsureTrailingPathSeparator());
            submodulCommands.StartCommitDialog(this, false);
            //TBD RefreshRevisions();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var unStagedFiles = DiffFiles.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitModule module = Module.GetSubmodule(item.Name);

                // Reset all changes.
                module.ResetHard("");

                // Also delete new files, if requested.
                if (resetType == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    var unstagedFiles = module.GetUnstagedFiles();
                    foreach (var file in unstagedFiles.Where(file => file.IsNew))
                    {
                        try
                        {
                            string path = Path.Combine(module.WorkingDir, file.Name);
                            if (File.Exists(path))
                                File.Delete(path);
                            else
                                Directory.Delete(path, true);
                        }
                        catch (System.IO.IOException) { }
                        catch (System.UnauthorizedAccessException) { }
                    }
                }
            }

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
