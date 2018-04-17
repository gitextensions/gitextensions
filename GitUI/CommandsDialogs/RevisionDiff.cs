using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.UserControls.RevisionGridClasses;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionDiff : GitModuleControl
    {
        private readonly TranslationString _saveFileFilterCurrentFormat = new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new TranslationString("All files");
        private readonly TranslationString _deleteSelectedFilesCaption = new TranslationString("Delete");
        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want to delete the selected file(s)?");
        private readonly TranslationString _deleteFailed = new TranslationString("Delete file failed");
        private readonly TranslationString _multipleDescription = new TranslationString("<multiple>");
        private readonly TranslationString _selectedRevision = new TranslationString("Selected");
        private readonly TranslationString _firstRevision = new TranslationString("First");

        private RevisionGrid _revisionGrid;
        private RevisionFileTree _revisionFileTree;
        private string _oldRevision;
        private GitItemStatus _oldDiffItem;
        private IRevisionDiffController _revisionDiffController;
        private readonly IFileStatusListContextMenuController _revisionDiffContextMenuController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;
        private readonly IGitRevisionTester _gitRevisionTester;

        public RevisionDiff()
        {
            InitializeComponent();
            DiffFiles.AlwaysRevisionGroups = true;
            Translate();
            HotkeysEnabled = true;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
            _gitRevisionTester = new GitRevisionTester(_fullPathResolver);
            _revisionDiffContextMenuController = new FileStatusListContextMenuController();
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
            if (Visible)
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
            DeleteSelectedFiles = 0
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

        public void DisplayDiffTab()
        {
            DiffText.SaveCurrentScrollPos();
            var revisions = _revisionGrid.GetSelectedRevisions();
            DiffFiles.SetDiffs(revisions);
            if (_oldDiffItem != null && DiffFiles.Revision?.Guid == _oldRevision)
            {
                DiffFiles.SelectedItem = _oldDiffItem;
                _oldDiffItem = null;
                _oldRevision = null;
            }
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
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            diffDeleteFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.DeleteSelectedFiles).ToShortcutKeyDisplayString();
            DiffText.ReloadHotkeys();
        }

        protected override void OnRuntimeLoad()
        {
            _revisionDiffController = new RevisionDiffController(_gitRevisionTester);

            DiffFiles.FilterVisible = true;
            DiffFiles.DescribeRevision = DescribeRevision;
            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.DiffFont;
            ReloadHotkeys();

            GotFocus += (s, e1) => DiffFiles.Focus();

            base.OnRuntimeLoad();
        }

        private string DescribeRevision(string sha1)
        {
            return DescribeRevision(sha1, 0);
        }

        private string DescribeRevision(string sha1, int maxLength)
        {
            if (sha1.IsNullOrEmpty())
            {
                // No parent at all, present as working directory
                return Strings.GetCurrentUnstagedChanges();
            }

            var revision = _revisionGrid.GetRevision(sha1);
            if (revision == null)
            {
                return sha1.ShortenTo(8);
            }

            return _revisionGrid.DescribeRevision(revision, maxLength);
        }

        /// <summary>
        /// Provide a description for the first selected or parent to the "primary" selected last
        /// </summary>
        /// <returns>A description of the selected parent</returns>
        private string DescribeSelectedParentRevision(bool showUnstagedAndCombined)
        {
            var parents = DiffFiles.SelectedItemParents
                .Where(i => showUnstagedAndCombined ||
                    !(i.Guid.IsNullOrWhiteSpace() || i.Guid == GitRevision.UnstagedGuid || i.Guid == DiffFiles.CombinedDiffGuid))
                .Distinct()
                .Count();
            if (parents == 0)
            {
                return null;
            }
            else if (parents == 1)
            {
                return DescribeRevision(DiffFiles.SelectedItemParent?.Guid, 50);
            }
            else
            {
                return _multipleDescription.Text;
            }
        }

        private bool GetNextPatchFile(bool searchBackward, bool loop, out int fileIndex, out Task loadFileContent)
        {
            fileIndex = -1;
            loadFileContent = Task.CompletedTask;
            if (DiffFiles.Revision == null)
            {
                return false;
            }

            int idx = DiffFiles.SelectedIndex;
            if (idx == -1)
            {
                return false;
            }

            fileIndex = DiffFiles.GetNextIndex(searchBackward, loop);
            if (fileIndex == idx)
            {
                if (!loop)
                {
                    return false;
                }
            }
            else
            {
                DiffFiles.SetSelectedIndex(fileIndex, notify: false);
            }

            loadFileContent = ShowSelectedFileDiffAsync();
            return true;
        }

        private ContextMenuSelectionInfo GetSelectionInfo()
        {
            // First (A) is parent if one revision selected or if parent, then selected
            bool firstIsParent = _gitRevisionTester.AllFirstAreParentsToSelected(DiffFiles.SelectedItemParents, DiffFiles.Revision);

            // Combined diff is a display only diff, no manipulations
            bool isAnyCombinedDiff = DiffFiles.SelectedItemParents.Any(item => item.Guid == DiffFiles.CombinedDiffGuid);
            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;
            bool isAnyItemSelected = DiffFiles.SelectedItems.Any();

            // No changes to files in bare repos
            bool isBareRepository = Module.IsBareRepository();
            bool isAnyTracked = DiffFiles.SelectedItems.Any(item => item.IsTracked);
            bool isAnySubmodule = DiffFiles.SelectedItems.Any(item => item.IsSubmodule);
            bool singleFileExists = isExactlyOneItemSelected && File.Exists(_fullPathResolver.Resolve(DiffFiles.SelectedItem.Name));

            var selectionInfo = new ContextMenuSelectionInfo(DiffFiles.Revision,
                firstIsParent: firstIsParent,
                isAnyCombinedDiff: isAnyCombinedDiff,
                isSingleGitItemSelected: isExactlyOneItemSelected,
                isAnyItemSelected: isAnyItemSelected,
                isBareRepository: isBareRepository,
                singleFileExists: singleFileExists,
                isAnyTracked: isAnyTracked,
                isAnySubmodule: isAnySubmodule);
            return selectionInfo;
        }

        private void ResetSelectedItemsTo(bool actsAsChild)
        {
            if (!DiffFiles.SelectedItems.Any())
            {
                return;
            }

            var selectedItems = DiffFiles.SelectedItems;
            if (actsAsChild)
            {
                // selected, all are the same
                string revision = DiffFiles.Revision.Guid;
                var deletedItems = selectedItems.Where(item => item.IsDeleted);
                Module.RemoveFiles(deletedItems.Select(item => item.Name), false);

                var itemsToCheckout = selectedItems.Where(item => !item.IsDeleted);
                Module.CheckoutFiles(itemsToCheckout.Select(item => item.Name), revision, false);
            }
            else
            {
                // acts as parent
                // if file is new to the parent, it has to be removed
                var addedItems = selectedItems.Where(item => item.IsNew);
                Module.RemoveFiles(addedItems.Select(item => item.Name), false);

                foreach (var parent in DiffFiles.SelectedItemParents)
                {
                    var itemsToCheckout = DiffFiles.SelectedItemsWithParent.Where(item => !item.Item.IsNew && item.ParentRevision.Guid == parent.Guid);
                    Module.CheckoutFiles(itemsToCheckout.Select(item => item.Item.Name), parent.Guid, false);
                }
            }

            RefreshArtificial();
        }

        private async Task ShowSelectedFileDiffAsync()
        {
            if (DiffFiles.SelectedItem == null || DiffFiles.Revision == null)
            {
                DiffText.ViewPatch("");
                return;
            }

            if (DiffFiles.SelectedItemParent?.Guid == DiffFiles.CombinedDiffGuid)
            {
                var diffOfConflict = Module.GetCombinedDiffContent(DiffFiles.Revision, DiffFiles.SelectedItem.Name,
                    DiffText.GetExtraDiffArguments(), DiffText.Encoding);

                if (string.IsNullOrWhiteSpace(diffOfConflict))
                {
                    diffOfConflict = Strings.GetUninterestingDiffOmitted();
                }

                DiffText.ViewPatch(diffOfConflict);
                return;
            }

            await DiffText.ViewChangesAsync(DiffFiles.SelectedItemParent?.Guid, DiffFiles.Revision?.Guid, DiffFiles.SelectedItem, string.Empty);
        }

        private async void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await ShowSelectedFileDiffAsync();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
            {
                return;
            }

            if (AppSettings.OpenSubmoduleDiffInSeparateWindow && DiffFiles.SelectedItem.IsSubmodule)
            {
                var submoduleName = DiffFiles.SelectedItem.Name;

                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        var status = await DiffFiles.SelectedItem.GetSubmoduleStatusAsync().ConfigureAwait(false);

                        var process = new Process
                        {
                            StartInfo =
                            {
                                FileName = Application.ExecutablePath,
                                Arguments = "browse -commit=" + status.Commit,
                                WorkingDirectory = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator())
                            }
                        };

                        process.Start();
                    });
            }
            else
            {
                UICommands.StartFileHistoryDialog(this, DiffFiles.SelectedItem.Name, DiffFiles.Revision);
            }
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses == null || !DiffFiles.GitItemStatuses.Any())
            {
                DiffText.ViewPatch(string.Empty);
            }
        }

        private async void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            try
            {
                await ShowSelectedFileDiffAsync();
            }
            catch (OperationCanceledException)
            {
            }
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
            resetFileToToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowResetFileMenus(selectionInfo);

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
                UICommands.StartFileHistoryDialog(this, item.Name, DiffFiles.Revision, true, true);
            }
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            // IsStaged is set by default, so that cannot be trusted, must be limited when selecting
            var files = DiffFiles.SelectedItemsWithParent
                .Where(it => it.ParentRevision.Guid == GitRevision.IndexGuid)
                .Select(it => it.Item)
                .ToList();

            Module.StageFiles(files, out _);
            RefreshArtificial();
        }

        private void UnstageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var files = new List<GitItemStatus>();
            foreach (var item in DiffFiles.SelectedItems.Where(i => i.IsStaged))
            {
                if (!item.IsNew)
                {
                    Module.UnstageFileToRemove(item.Name);

                    if (item.IsRenamed)
                    {
                        Module.UnstageFileToRemove(item.OldName);
                    }
                }
                else
                {
                    files.Add(item);
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

            IEnumerable<GitItemStatus> FindDiffFilesMatches(string name)
            {
                var predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);
                return candidates.Where(item => predicate(item.Name) || predicate(item.OldName));
            }

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
                UICommands.StartFileHistoryDialog(this, item.Name, DiffFiles.Revision);
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null || DiffFiles.Revision == null)
            {
                return;
            }

            RevisionDiffKind diffKind;

            if (sender == firstToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffALocal;
            }
            else if (sender == selectedToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffBLocal;
            }
            else if (sender == firstParentToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffAParentLocal;
            }
            else if (sender == selectedParentToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffBParentLocal;
            }
            else
            {
                diffKind = RevisionDiffKind.DiffAB;
            }

            foreach (var itemWithParent in DiffFiles.SelectedItemsWithParent)
            {
                var revs = new[] { DiffFiles.Revision, itemWithParent.ParentRevision };
                UICommands.OpenWithDifftool(this, revs, itemWithParent.Item.Name, itemWithParent.Item.OldName, diffKind, itemWithParent.Item.IsTracked);
            }
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            bool firstIsParent = _gitRevisionTester.AllFirstAreParentsToSelected(DiffFiles.SelectedItemParents, DiffFiles.Revision);
            bool localExists = _gitRevisionTester.AnyLocalFileExists(DiffFiles.SelectedItemsWithParent.Select(i => i.Item));

            IEnumerable<string> selectedItemParentRevs = DiffFiles.SelectedItemParents.Select(i => i.Guid);
            bool allAreNew = DiffFiles.SelectedItemsWithParent.All(i => i.Item.IsNew);
            bool allAreDeleted = DiffFiles.SelectedItemsWithParent.All(i => i.Item.IsDeleted);

            var selectionInfo = new ContextMenuDiffToolInfo(DiffFiles.Revision, selectedItemParentRevs,
                allAreNew: allAreNew,
                allAreDeleted: allAreDeleted,
                firstIsParent: firstIsParent,
                firstParentsValid: _revisionGrid.IsFirstParentValid(),
                localExists: localExists);
            return selectionInfo;
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();

            if (DiffFiles.SelectedItemsWithParent.Any())
            {
                selectedDiffCaptionMenuItem.Text = _selectedRevision + ": (" + _revisionGrid.DescribeRevision(DiffFiles.Revision, 50) + ")";
                selectedDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(selectedDiffCaptionMenuItem, DiffContextMenu);

                firstDiffCaptionMenuItem.Text = _firstRevision + ":";
                var parentDesc = DescribeSelectedParentRevision(true);
                if (parentDesc.IsNotNullOrWhitespace())
                {
                    firstDiffCaptionMenuItem.Text += " (" + parentDesc + ")";
                }

                firstDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(firstDiffCaptionMenuItem, DiffContextMenu);
            }
            else
            {
                firstDiffCaptionMenuItem.Visible = false;
                selectedDiffCaptionMenuItem.Visible = false;
            }

            firstToSelectedToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo);
            firstToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo);
            selectedToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo);
            firstParentToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo);
            selectedParentToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo);
            firstParentToLocalToolStripMenuItem.Visible = _revisionDiffContextMenuController.ShouldDisplayMenuFirstParentToLocal(selectionInfo);
            selectedParentToLocalToolStripMenuItem.Visible = _revisionDiffContextMenuController.ShouldDisplayMenuSelectedParentToLocal(selectionInfo);
        }

        private void resetFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetSelectedItemsTo(sender == resetFileToSelectedToolStripMenuItem);
        }

        private void resetFileToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (DiffFiles.Revision == null)
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
                resetFileToParentToolStripMenuItem.Visible = false;
                return;
            }

            if (DiffFiles.Revision.Guid == GitRevision.UnstagedGuid)
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToSelectedToolStripMenuItem.Visible = true;
                resetFileToSelectedToolStripMenuItem.Text = _selectedRevision + " (" + _revisionGrid.DescribeRevision(DiffFiles.Revision, 50) + ")";
            }

            var parentDesc = DescribeSelectedParentRevision(false);
            if (parentDesc.IsNullOrWhiteSpace())
            {
                resetFileToParentToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToParentToolStripMenuItem.Visible = true;
                resetFileToParentToolStripMenuItem.Text = _firstRevision + " (" + parentDesc + ")";
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (DiffFiles.Revision == null || DiffFiles.SelectedItem == null)
            {
                return;
            }

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
                    Module.SaveBlobAs(fileDialog.FileName, string.Format("{0}:\"{1}\"", DiffFiles.Revision?.Guid, item.Name));
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
                            {
                                Module.UnstageFileToRemove(item.OldName);
                            }
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
                submodulCommands.StartCommitDialog(this);
            }

            RefreshArtificial();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return;
            }

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
                            {
                                File.Delete(path);
                            }
                            else
                            {
                                Directory.Delete(path, true);
                            }
                        }
                        catch (IOException)
                        {
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                    }
                }
            }

            RefreshArtificial();
        }

        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct();

            FormProcess.ShowDialog(FindForm() as FormBrowse, GitCommandHelpers.SubmoduleUpdateCmd(submodules));
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

            using (var frm = new FormEdit(summary))
            {
                frm.ShowDialog(this);
            }
        }
    }
}
