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
using GitUI.Hotkey;
using ResourceManager;
using System.Threading.Tasks;

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
        private IRevisionDiffController _revisionDiffController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;

        public RevisionDiff()
        {
            InitializeComponent();
            Translate();
            this.HotkeysEnabled = true;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
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
            RefreshArtificial();
        }

        public void RefreshArtificial()
        {
            if (this.Visible)
            {
                var revisions = _revisionGrid.GetSelectedRevisions();

                if (revisions.Count > 0 && revisions[0].IsArtificial)
                {
                    DiffFiles.SetDiffs(revisions);
                }
            }
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "BrowseDiff";

        internal enum Commands
        {
            DeleteSelectedFiles,
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.DeleteSelectedFiles: return DeleteSelectedFiles();
                default: return base.ExecuteCommand(cmd);
            }
        }

        internal Keys GetShortcutKeys(Commands cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        #endregion

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
                    return _diffParentWithSelection.Text;

                case 2: // diff "first clicked revision" --> "second clicked revision"
                    return _diffTwoSelected.Text;
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
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            this.diffDeleteFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.DeleteSelectedFiles).ToShortcutKeyDisplayString();
            DiffText.ReloadHotkeys();
        }


        protected override void OnRuntimeLoad(EventArgs e)
        {
            _revisionDiffController = new RevisionDiffController();

            DiffFiles.FilterVisible = true;
            DiffFiles.DescribeRevision = DescribeRevision;
            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.DiffFont;
            ReloadHotkeys();

            GotFocus += (s, e1) => DiffFiles.Focus();

            base.OnRuntimeLoad(e);
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

        private bool GetNextPatchFile(bool searchBackward, bool loop, out int fileIndex, out Task loadFileContent)
        {
            fileIndex = -1;
            loadFileContent = Task.FromResult<string>(null);
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count == 0)
                return false;

            int idx = DiffFiles.SelectedIndex;
            if (idx == -1)
                return false;

            fileIndex = DiffFiles.GetNextIndex(searchBackward, loop);
            if (fileIndex == idx)
            {
                if (!loop)
                    return false;
            }
            else
            {
                DiffFiles.SetSelectedIndex(fileIndex, notify: false);
            }
            loadFileContent = ShowSelectedFileDiff();
            return true;
        }

        private ContextMenuSelectionInfo GetSelectionInfo()
        {
            IList<GitRevision> selectedRevisions = _revisionGrid.GetSelectedRevisions();

            bool isAnyCombinedDiff = DiffFiles.SelectedItemParents.Any(item => item.Guid == DiffFiles.CombinedDiff.Text);
            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;
            bool isAnyItemSelected = DiffFiles.SelectedItems.Count() > 0;
            var isCombinedDiff = isExactlyOneItemSelected && DiffFiles.CombinedDiff.Text == DiffFiles.SelectedItemParent?.Guid;
            var selectedItemStatus = DiffFiles.SelectedItem;
            bool isBareRepository = Module.IsBareRepository();
            bool singleFileExists = isExactlyOneItemSelected && File.Exists(_fullPathResolver.Resolve(DiffFiles.SelectedItem.Name));

            var selectionInfo = new ContextMenuSelectionInfo(selectedRevisions, selectedItemStatus, isAnyCombinedDiff, isExactlyOneItemSelected, isCombinedDiff, isAnyItemSelected, isBareRepository, singleFileExists);
            return selectionInfo;
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
            else // acts as parent
            {
                // if file is new to the parent, it has to be removed
                var addedItems = selectedItems.Where(item => item.IsNew);
                Module.RemoveFiles(addedItems.Select(item => item.Name), false);
                itemsToCheckout = selectedItems.Where(item => !item.IsNew);
            }

            Module.CheckoutFiles(itemsToCheckout.Select(item => item.Name), revision, false);
            RefreshArtificial();
        }

        private async Task ShowSelectedFileDiff()
        {
            var items = _revisionGrid.GetSelectedRevisions();
            if (DiffFiles.SelectedItem == null || items.Count() == 0)
            {
                DiffText.ViewPatch("");
                return;
            }

            if (items.Count() == 1)
            {
                items.Add(DiffFiles.SelectedItemParent);

                if (!string.IsNullOrWhiteSpace(DiffFiles.SelectedItemParent?.Guid)
                    && DiffFiles.SelectedItemParent?.Guid == DiffFiles.CombinedDiff.Text)
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
            await DiffText.ViewChanges(items, DiffFiles.SelectedItem, String.Empty);
        }


        private async void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await ShowSelectedFileDiff();
            }
            catch (OperationCanceledException)
            { }
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
                        process.StartInfo.WorkingDirectory = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator());
                        process.Start();
                    });
            }
            else
            {
                UICommands.StartFileHistoryDialog(this, DiffFiles.SelectedItem.Name, DiffFiles.Revision, false);
            }
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses == null || !DiffFiles.GitItemStatuses.Any())
                DiffText.ViewPatch(String.Empty);
        }

        private async void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            try
            {
                await ShowSelectedFileDiff();
            }
            catch (OperationCanceledException)
            { }
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // switch to view (and fills the first level of file tree data model if not already done)
            (FindForm() as FormBrowse)?.ExecuteCommand(FormBrowse.Commands.FocusFileTree);
            _revisionFileTree.ExpandToFile(DiffFiles.SelectedItems.First().Name);
        }

        private void DiffContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var selectionInfo = GetSelectionInfo();

            // Many options have no meaning for artificial commits or submodules
            // Hide the obviously no action options when single selected, handle them in actions if multi select

            openWithDifftoolToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
            saveAsToolStripMenuItem1.Visible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
            copyFilenameToClipboardToolStripMenuItem1.Enabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);

            stageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuStage(selectionInfo);
            unstageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuUnstage(selectionInfo);

            cherryPickSelectedDiffFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);
            // Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
            diffShowInFileTreeToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            fileHistoryDiffToolstripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
            blameToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBlame(selectionInfo);
            resetFileToToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuResetFile(selectionInfo);

            diffEditFileToolStripMenuItem.Visible =
               diffDeleteFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditFile(selectionInfo);

            diffCommitSubmoduleChanges.Visible =
                diffResetSubmoduleChanges.Visible =
                diffStashSubmoduleChangesToolStripMenuItem.Visible =
                diffUpdateSubmoduleMenuItem.Visible =
                diffSubmoduleSummaryMenuItem.Visible =
                diffUpdateSubmoduleMenuItem.Visible = _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

            diffToolStripSeparator13.Visible = _revisionDiffController.ShouldShowMenuEditFile(selectionInfo) || _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

            // openContainingFolderToolStripMenuItem.Enabled or not
            {
                openContainingFolderToolStripMenuItem.Enabled = false;

                foreach (var item in DiffFiles.SelectedItems)
                {
                    string filePath = _fullPathResolver.Resolve(item.Name);
                    if (FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                    {
                        openContainingFolderToolStripMenuItem.Enabled = true;
                        break;
                    }
                }
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                GitRevision revision = _revisionGrid.GetSelectedRevisions().FirstOrDefault();
                UICommands.StartFileHistoryDialog(this, item.Name, revision, true, true);
            }
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var files = new List<GitItemStatus>();
            foreach (var item in DiffFiles.SelectedItems)
            {
                files.Add(item);
            }
            bool err;
            Module.StageFiles(files, out err);
            RefreshArtificial();
        }

        private void UnstageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var files = new List<GitItemStatus>();
            foreach (var item in DiffFiles.SelectedItems)
            {
                if (item.IsStaged)
                {
                    if (!item.IsNew)
                    {
                        Module.UnstageFileToRemove(item.Name);

                        if (item.IsRenamed)
                            Module.UnstageFileToRemove(item.OldName);
                    }
                    else
                    {
                        files.Add(item);
                    }
                }
            }

            Module.UnstageFiles(files);
            RefreshArtificial();
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
                var predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);
                return candidates.Where(item => predicate(item.Name) || predicate(item.OldName)).ToList();
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
                UICommands.StartFileHistoryDialog(this, item.Name, DiffFiles.Revision, false);
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

            GitUI.RevisionDiffKind diffKind;

            if (sender == aLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffALocal;
            else if (sender == bLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffBLocal;
            else if (sender == parentOfALocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffAParentLocal;
            else if (sender == parentOfBLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffBParentLocal;
            else
            {
                diffKind = GitUI.RevisionDiffKind.DiffAB;
            }

            foreach (var itemWithParent in DiffFiles.SelectedItemsWithParent)
            {
                _revisionGrid.OpenWithDifftool(itemWithParent.Item.Name, itemWithParent.Item.OldName, diffKind, itemWithParent.Item.IsTracked);
            }
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            IList<GitRevision> selectedRevisions = _revisionGrid.GetSelectedRevisions();
            // Should be blocked in the GUI but not an error to show to the user
            Debug.Assert(selectedRevisions.Count == 1 || selectedRevisions.Count == 2,
                "Unexpectedly number of revisions for difftool" + selectedRevisions.Count);
            if (selectedRevisions.Count < 1 || selectedRevisions.Count > 2)
            {
                return null;
            }

            bool aIsLocal = selectedRevisions.Count == 2 && selectedRevisions[1].Guid == GitRevision.UnstagedGuid;
            bool bIsLocal = selectedRevisions[0].Guid == GitRevision.UnstagedGuid;
            bool multipleRevisionsSelected = selectedRevisions.Count == 2;

            bool localExists = false;
            bool bIsNormal = false; // B is assumed to be new or deleted (check from DiffFiles)

            // enable *<->Local items only when (any) local file exists
            foreach (var item in DiffFiles.SelectedItems)
            {
                bIsNormal = bIsNormal || !(item.IsNew || item.IsDeleted);
                string filePath = _fullPathResolver.Resolve(item.Name);
                if (File.Exists(filePath) || Directory.Exists(filePath))
                {
                    localExists = true;
                    if (localExists && bIsNormal)
                        break;
                }
            }

            var selectionInfo = new ContextMenuDiffToolInfo(aIsLocal, bIsLocal, bIsNormal, localExists, multipleRevisionsSelected);
            return selectionInfo;
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();

            aBToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuAB(selectionInfo);
            aLocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuALocal(selectionInfo);
            bLocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBLocal(selectionInfo);
            parentOfALocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuAParentLocal(selectionInfo);
            parentOfBLocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBParentLocal(selectionInfo);
            parentOfALocalToolStripMenuItem.Visible = parentOfALocalToolStripMenuItem.Enabled || _revisionDiffController.ShouldShowMenuAParent(selectionInfo);
            parentOfBLocalToolStripMenuItem.Visible = parentOfBLocalToolStripMenuItem.Enabled || _revisionDiffController.ShouldShowMenuBParent(selectionInfo);
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

            ResetSelectedItemsTo(revisions[0].FirstParentGuid, false);
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
                if (revisions[0].Guid == GitRevision.UnstagedGuid)
                {
                    resetFileToSelectedToolStripMenuItem.Visible = false;
                }
                else
                {
                    resetFileToSelectedToolStripMenuItem.Visible = true;
                    TranslateItem(resetFileToSelectedToolStripMenuItem.Name, resetFileToSelectedToolStripMenuItem);
                    resetFileToSelectedToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(revisions[0], 50) + ")";
                }
                if (revisions[0].HasParent)
                {
                    resetFileToParentToolStripMenuItem.Visible = true;
                    TranslateItem(resetFileToParentToolStripMenuItem.Name, resetFileToParentToolStripMenuItem);
                    GitRevision parentRev = _revisionGrid.GetRevision(revisions[0].FirstParentGuid);
                    if (parentRev != null)
                    {
                        resetFileToParentToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(parentRev, 50) + ")";
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
                if (revisions[1].Guid == GitRevision.UnstagedGuid)
                {
                    resetFileToFirstToolStripMenuItem.Visible = false;
                }
                else
                {
                    resetFileToFirstToolStripMenuItem.Visible = true;
                    TranslateItem(resetFileToFirstToolStripMenuItem.Name, resetFileToFirstToolStripMenuItem);
                    resetFileToFirstToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(revisions[1], 50) + ")";
                }

                if (revisions[0].Guid == GitRevision.UnstagedGuid)
                {
                    resetFileToSecondToolStripMenuItem.Visible = false;
                }
                else
                {
                    resetFileToSecondToolStripMenuItem.Visible = true;
                    TranslateItem(resetFileToSecondToolStripMenuItem.Name, resetFileToSecondToolStripMenuItem);
                    resetFileToSecondToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(revisions[0], 50) + ")";
                }
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

            var fullName = _fullPathResolver.Resolve(item.Name);
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

        private bool DeleteSelectedFiles()
        {
            try
            {
                if (DiffFiles.SelectedItem == null || DiffFiles.Revision == null || !DiffFiles.Revision.IsArtificial ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                {
                    return false;
                }

                var selectedItems = DiffFiles.SelectedItems;
                if (DiffFiles.Revision?.Guid == GitRevision.IndexGuid)
                {
                    var files = new List<GitItemStatus>();
                    var stagedItems = selectedItems.Where(item => item.IsStaged);
                    foreach (var item in stagedItems)
                    {
                        if (!item.IsNew)
                        {
                            Module.UnstageFileToRemove(item.Name);

                            if (item.IsRenamed)
                                Module.UnstageFileToRemove(item.OldName);
                        }
                        else
                        {
                            files.Add(item);
                        }
                    }
                    Module.UnstageFiles(files);
                }
                DiffFiles.StoreNextIndexToSelect();
                var items = DiffFiles.SelectedItems.Where(item => !item.IsSubmodule);
                foreach (var item in items)
                {
                    File.Delete(_fullPathResolver.Resolve(item.Name));
                }
                RefreshArtificial();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message);
                return false;
            }
            return true;
        }

        private void diffDeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedFiles();
        }

        private void diffEditFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = DiffFiles.SelectedItem;
            var fileName = _fullPathResolver.Resolve(item.Name);

            UICommands.StartFileEditorDialog(fileName);
            RefreshArtificial();
        }

        private void diffCommitSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                GitUICommands submodulCommands = new GitUICommands(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
                submodulCommands.StartCommitDialog(this, false);
            }
            RefreshArtificial();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
                return;

            foreach (var name in submodules)
            {
                GitModule module = Module.GetSubmodule(name);

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
                            string path = _fullPathResolver.Resolve(file.Name);
                            if (File.Exists(path))
                                File.Delete(path);
                            else
                                Directory.Delete(path, true);
                        }
                        catch (IOException) { }
                        catch (UnauthorizedAccessException) { }
                    }
                }
            }
            RefreshArtificial();
        }

        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct();

            FormProcess.ShowDialog((FindForm() as FormBrowse), GitCommandHelpers.SubmoduleUpdateCmd(submodules));
            RefreshArtificial();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                GitUICommands uiCmds = new GitUICommands(Module.GetSubmodule(name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }
            RefreshArtificial();
        }

        private void diffSubmoduleSummaryMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            string summary = "";
            foreach (var name in submodules)
            {
                summary += Module.GetSubmoduleSummary(name);
            }
            using (var frm = new FormEdit(summary)) frm.ShowDialog(this);
        }
    }
}
