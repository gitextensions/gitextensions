using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    internal sealed class RevisionDiffController
    {
        private RevisionGrid _revisionGrid;
        private FileStatusList _diffFiles;
        private Editor.FileViewer _diffText;

        internal RevisionDiffController(RevisionGrid revisionGrid, FileStatusList diffFiles, Editor.FileViewer diffText)
        {
            _revisionGrid = revisionGrid;
            _diffFiles = diffFiles;
            _diffText = diffText;
        }

        internal Tuple<int, string> GetNextPatchFile(bool searchBackward)
        {
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count == 0)
                return null;
            int idx = _diffFiles.SelectedIndex;
            if (idx == -1)
                return new Tuple<int, string>(idx, null);

            idx = GetNextIdx(idx, _diffFiles.GitItemStatuses.Count() - 1, searchBackward);
            _diffFiles.SetSelectedIndex(idx, notify: false);
            return new Tuple<int, string>(idx, GetSelectedPatch(revisions, _diffFiles.SelectedItem));
        }

        internal string DescribeRevision(string sha1)
        {
            var revision = _revisionGrid.GetRevision(sha1);
            if (revision == null)
            {
                return sha1.ShortenTo(8);
            }

            return _revisionGrid.DescribeRevision(revision);
        }

        internal static int GetNextIdx(int curIdx, int maxIdx, bool searchBackward)
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

        internal string GetSelectedPatch(IList<GitRevision> revisions, GitItemStatus file)
        {
            string firstRevision = revisions.Count > 0 ? revisions[0].Guid : null;
            string secondRevision = revisions.Count == 2 ? revisions[1].Guid : null;
            return _diffText.GetSelectedPatch(firstRevision, secondRevision, file);
        }

        internal void ShowSelectedFileDiff()
        {
            if (_diffFiles.SelectedItem == null)
            {
                _diffText.ViewPatch("");
                return;
            }

            var items = _revisionGrid.GetSelectedRevisions();
            if (items.Count() == 1)
            {
                items.Add(new GitRevision(_revisionGrid.Module, _diffFiles.SelectedItemParent));

                if (!string.IsNullOrWhiteSpace(_diffFiles.SelectedItemParent)
                    && _diffFiles.SelectedItemParent == _diffFiles.CombinedDiff.Text)
                {
                    var diffOfConflict = _revisionGrid.Module.GetCombinedDiffContent(items.First(), _diffFiles.SelectedItem.Name,
                        _diffText.GetExtraDiffArguments(), _diffText.Encoding);

                    if (string.IsNullOrWhiteSpace(diffOfConflict))
                    {
                        diffOfConflict = Strings.GetUninterestingDiffOmitted();
                    }

                    _diffText.ViewPatch(diffOfConflict);
                    return;
                }
            }
            _diffText.ViewChanges(items, _diffFiles.SelectedItem, String.Empty);
        }

        internal void DiffContextMenu_Opening(out bool openWithDifftool, out bool saveAs, 
            out bool stage, out bool unstage, out bool cherryPick, out bool diffShowInFileTree, 
            out bool fileHistory, out bool blame, out bool resetFileTo, out bool diffEditFile, 
            out bool diffDeleteFile, out bool submodule, out bool openContainingFolder)
        {
            bool artificialRevSelected;

            IList<GitRevision> selectedRevisions = _revisionGrid.GetSelectedRevisions();

            if (selectedRevisions.Count == 0)
                artificialRevSelected = false;
            else
                artificialRevSelected = selectedRevisions[0].IsArtificial();
            if (selectedRevisions.Count > 1)
                artificialRevSelected = artificialRevSelected || selectedRevisions[selectedRevisions.Count - 1].IsArtificial();

            //Many options have no meaning for artificial commits or submodules
            //Hide the obviously no action options when single selected, handle them in actions if multi select

            // disable items that need exactly one selected item
            bool isExactlyOneItemSelected = _diffFiles.SelectedItems.Count() == 1;
            var isCombinedDiff = isExactlyOneItemSelected &&
                _diffFiles.CombinedDiff.Text == _diffFiles.SelectedItemParent;
            var isAnyCombinedDiff = _diffFiles.SelectedItemParents.Any(item => item == _diffFiles.CombinedDiff.Text);

            //Options for toolstripmenu

            openWithDifftool = !isAnyCombinedDiff;
            saveAs = !isCombinedDiff && isExactlyOneItemSelected && !_diffFiles.SelectedItem.IsSubmodule;

            stage =
                selectedRevisions.Count() >= 1 && selectedRevisions[0].Guid == GitRevision.UnstagedGuid ||
                selectedRevisions.Count() >= 2 && selectedRevisions[1].Guid == GitRevision.UnstagedGuid;
            unstage =
                selectedRevisions.Count() >= 1 && selectedRevisions[0].Guid == GitRevision.IndexGuid ||
                selectedRevisions.Count() >= 2 && selectedRevisions[1].Guid == GitRevision.IndexGuid;

            cherryPick = !isCombinedDiff && isExactlyOneItemSelected &&
                !(_diffFiles.SelectedItem.IsSubmodule || selectedRevisions[0].Guid == GitRevision.UnstagedGuid ||
                (_diffFiles.SelectedItem.IsNew || _diffFiles.SelectedItem.IsDeleted) && selectedRevisions[0].Guid == GitRevision.IndexGuid);
            //Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
            diffShowInFileTree = isExactlyOneItemSelected && !selectedRevisions[0].IsArtificial();
            fileHistory = isExactlyOneItemSelected && !(_diffFiles.SelectedItem.IsNew && selectedRevisions[0].IsArtificial());
            blame = isExactlyOneItemSelected && !(_diffFiles.SelectedItem.IsSubmodule || selectedRevisions[0].IsArtificial());
            resetFileTo = !isCombinedDiff &&
                !(isExactlyOneItemSelected &&
                (_diffFiles.SelectedItem.IsSubmodule || _diffFiles.SelectedItem.IsNew) && selectedRevisions[0].Guid == GitRevision.UnstagedGuid);

            diffEditFile = diffDeleteFile = 
                isExactlyOneItemSelected && !_diffFiles.SelectedItem.IsSubmodule && selectedRevisions[0].IsArtificial();

            submodule = isExactlyOneItemSelected && _diffFiles.SelectedItem.IsSubmodule && selectedRevisions[0].Guid == GitRevision.UnstagedGuid;

            // openContainingFolderToolStripMenuItem.Enabled or not
            {
                openContainingFolder = false;

                foreach (var item in _diffFiles.SelectedItems)
                {
                    string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(_revisionGrid.Module, item);
                    if (FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                    {
                        openContainingFolder = true;
                        break;
                    }
                }
            }
        }

        internal void ShowEnableDiffDropDown(out bool enableDiffDropDown, out bool showParentItems, out bool localExists)
        {
            bool artificialRevSelected = false;
            enableDiffDropDown = true;
            showParentItems = false;
        

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

            localExists = false;
            if (!enableDiffDropDown)
                return;

            foreach (var item in _diffFiles.SelectedItems)
            {
                string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(_revisionGrid.Module, item);
                if (File.Exists(filePath))
                {
                    localExists = !artificialRevSelected;
                    return;
                }
            }
        }

        internal void ResetSelectedItemsTo(string revision, bool actsAsChild)
        {
            var selectedItems = _diffFiles.SelectedItems;
            IEnumerable<GitItemStatus> itemsToCheckout;
            if (actsAsChild)
            {
                var deletedItems = selectedItems.Where(item => item.IsDeleted);
                _revisionGrid.Module.RemoveFiles(deletedItems.Select(item => item.Name), false);
                itemsToCheckout = selectedItems.Where(item => !item.IsDeleted);
            }
            else //acts as parent
            {
                //if file is new to the parent, it has to be removed
                var addedItems = selectedItems.Where(item => item.IsNew);
                _revisionGrid.Module.RemoveFiles(addedItems.Select(item => item.Name), false);
                itemsToCheckout = selectedItems.Where(item => !item.IsNew);
            }

            _revisionGrid.Module.CheckoutFiles(itemsToCheckout.Select(item => item.Name), revision, false);
        }

        internal void ResetSubmoduleChanges(FormResetChanges.ActionEnum resetType)
        {
            var unStagedFiles = _diffFiles.SelectedItems.ToList();
            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitModule module = _revisionGrid.Module.GetSubmodule(item.Name);

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
        }

        internal void StageFiles()
        {
            var files = new List<GitItemStatus>();
            foreach (var item in _diffFiles.SelectedItems)
            {
                files.Add(item);
            }
            bool err;
            _revisionGrid.Module.StageFiles(files, out err);
        }

        internal void UnstageFiles()
        {
            var files = new List<GitItemStatus>();
            foreach (var item in _diffFiles.SelectedItems)
            {
                if (item.IsStaged)
                {
                    if (!item.IsNew)
                    {
                        _revisionGrid.Module.UnstageFileToRemove(item.Name);

                        if (item.IsRenamed)
                            _revisionGrid.Module.UnstageFileToRemove(item.OldName);
                    }
                    else
                    {
                        files.Add(item);
                    }
                }
            }

            _revisionGrid.Module.UnstageFiles(files);
        }

        internal bool CheckDeleteFiles()
        {
            return _diffFiles.SelectedItem != null && _diffFiles.Revision.IsArtificial();
        }

        internal bool DeleteSelectedFiles()
        {
            var selectedItems = _diffFiles.SelectedItems;
            if (_diffFiles.Revision.Guid == GitRevision.IndexGuid)
            {
                var files = new List<GitItemStatus>();
                var stagedItems = selectedItems.Where(item => item.IsStaged);
                foreach (var item in stagedItems)
                {
                    if (!item.IsNew)
                    {
                        _revisionGrid.Module.UnstageFileToRemove(item.Name);

                        if (item.IsRenamed)
                            _revisionGrid.Module.UnstageFileToRemove(item.OldName);
                    }
                    else
                    {
                        files.Add(item);
                    }
                }
                _revisionGrid.Module.UnstageFiles(files);
            }
            _diffFiles.StoreNextIndexToSelect();
            var items = _diffFiles.SelectedItems.Where(item => !item.IsSubmodule);
            foreach (var item in items)
            {
                File.Delete(Path.Combine(_revisionGrid.Module.WorkingDir, item.Name));
            }

            return true;
        }
    }
}
