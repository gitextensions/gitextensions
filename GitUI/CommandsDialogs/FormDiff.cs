using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.HelperDialogs;
using GitUI.Theming;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormDiff : GitModuleForm
    {
        private string _baseCommitDisplayStr;
        private string _headCommitDisplayStr;
        private GitRevision _baseRevision;
        private GitRevision _headRevision;
        private readonly GitRevision _mergeBase;
        private readonly IGitRevisionTester _revisionTester;
        private readonly IFileStatusListContextMenuController _revisionDiffContextMenuController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;
        private readonly bool _firstParentIsValid;

        private readonly ToolTip _toolTipControl = new ToolTip();

        private readonly TranslationString _anotherBranchTooltip = new TranslationString("Select another branch");
        private readonly TranslationString _anotherCommitTooltip = new TranslationString("Select another commit");
        private readonly TranslationString _btnSwapTooltip = new TranslationString("Swap BASE and Compare commits");
        private readonly TranslationString _ckCompareToMergeBase = new TranslationString("Compare to merge &base");

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormDiff()
        {
            InitializeComponent();
        }

        public FormDiff(
            GitUICommands commands, bool firstParentIsValid,
            ObjectId baseId, ObjectId headId,
            string baseCommitDisplayStr, string headCommitDisplayStr)
            : base(commands)
        {
            _baseCommitDisplayStr = baseCommitDisplayStr;
            _headCommitDisplayStr = headCommitDisplayStr;
            _firstParentIsValid = firstParentIsValid;

            InitializeComponent();

            btnSwap.AdaptImageLightness();

            InitializeComplete();

            _toolTipControl.SetToolTip(btnAnotherBaseBranch, _anotherBranchTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherHeadBranch, _anotherBranchTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherBaseCommit, _anotherCommitTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherHeadCommit, _anotherCommitTooltip.Text);
            _toolTipControl.SetToolTip(btnSwap, _btnSwapTooltip.Text);

            _baseRevision = new GitRevision(baseId);
            _headRevision = new GitRevision(headId);

            ObjectId mergeBase;
            if (_baseRevision.ObjectId.IsArtificial || _headRevision.ObjectId.IsArtificial)
            {
                mergeBase = null;
            }
            else
            {
                mergeBase = Module.GetMergeBase(_baseRevision.ObjectId, _headRevision.ObjectId);
            }

            _mergeBase = mergeBase != null ? new GitRevision(mergeBase) : null;
            ckCompareToMergeBase.Text = $"{_ckCompareToMergeBase} ({_mergeBase?.ObjectId.ToShortString()})";
            ckCompareToMergeBase.Enabled = _mergeBase != null;

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
            _revisionTester = new GitRevisionTester(_fullPathResolver);
            _revisionDiffContextMenuController = new FileStatusListContextMenuController();

            lblBaseCommit.BackColor = AppColor.DiffRemoved.GetThemeColor();
            lblHeadCommit.BackColor = AppColor.DiffAdded.GetThemeColor();

            DiffFiles.ContextMenuStrip = DiffContextMenu;
            DiffFiles.SelectedIndexChanged += delegate { ShowSelectedFileDiff(); };
            DiffText.ExtraDiffArgumentsChanged += delegate { ShowSelectedFileDiff(); };
            DiffText.TopScrollReached += FileViewer_TopScrollReached;
            DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
            Load += delegate { PopulateDiffFiles(); };
        }

        private void FileViewer_TopScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectPreviousVisibleItem();
            DiffText.ScrollToBottom();
        }

        private void FileViewer_BottomScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectNextVisibleItem();
            DiffText.ScrollToTop();
        }

        private void PopulateDiffFiles()
        {
            lblBaseCommit.Text = _baseCommitDisplayStr;
            lblHeadCommit.Text = _headCommitDisplayStr;

            DiffFiles.SetDiffs(ckCompareToMergeBase.Checked
                ? new[] { _headRevision, _mergeBase }
                : new[] { _headRevision, _baseRevision });

            // Bug in git-for-windows: Comparing working directory to any branch, fails, due to -R
            // I.e., git difftool --gui --no-prompt --dir-diff -R HEAD fails, but
            // git difftool --gui --no-prompt --dir-diff HEAD succeeds
            // Thus, we disable comparing "from" working directory.
            var enableDifftoolDirDiff = _baseRevision?.ObjectId != ObjectId.WorkTreeId;
            btnCompareDirectoriesWithDiffTool.Enabled = enableDifftoolDirDiff;
        }

        private void ShowSelectedFileDiff()
        {
            DiffText.ViewChangesAsync(DiffFiles.SelectedItem);
        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            var orgBaseRev = _baseRevision;
            _baseRevision = _headRevision;
            _headRevision = orgBaseRev;

            var orgBaseStr = _baseCommitDisplayStr;
            _baseCommitDisplayStr = _headCommitDisplayStr;
            _headCommitDisplayStr = orgBaseStr;
            PopulateDiffFiles();
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedGitItem == null)
            {
                return;
            }

            var diffKind = GetDiffKind();

            foreach (var item in DiffFiles.SelectedItems)
            {
                if (item.FirstRevision?.ObjectId == ObjectId.CombinedDiffId)
                {
                    continue;
                }

                var revs = new[] { item.SecondRevision, item.FirstRevision };
                UICommands.OpenWithDifftool(this, revs, item.Item.Name, item.Item.OldName, diffKind, item.Item.IsTracked);
            }

            RevisionDiffKind GetDiffKind()
            {
                if (Equals(sender, firstToLocalToolStripMenuItem))
                {
                    return RevisionDiffKind.DiffALocal;
                }
                else if (sender == selectedToLocalToolStripMenuItem)
                {
                    return RevisionDiffKind.DiffBLocal;
                }
                else if (sender == firstParentToLocalToolStripMenuItem)
                {
                    return RevisionDiffKind.DiffAParentLocal;
                }
                else if (sender == selectedParentToLocalToolStripMenuItem)
                {
                    return RevisionDiffKind.DiffBParentLocal;
                }
                else
                {
                    Debug.Assert(sender == firstToSelectedToolStripMenuItem, "Not implemented DiffWithRevisionKind: " + sender);
                    return RevisionDiffKind.DiffAB;
                }
            }
        }

        private void copyFilenameToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormBrowse.CopyFullPathToClipboard(DiffFiles, Module);
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void fileHistoryDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedGitItem;

            if (item.IsTracked)
            {
                UICommands.StartFileHistoryDialog(this, item.Name, _baseRevision);
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedGitItem;

            if (item.IsTracked)
            {
                UICommands.StartFileHistoryDialog(this, item.Name, _baseRevision, true, true);
            }
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
                Owner = this
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }

            if (selectedItem != null)
            {
                DiffFiles.SelectedGitItem = selectedItem;
            }
        }

        private void ckCompareToMergeBase_CheckedChanged(object sender, EventArgs e)
        {
            PopulateDiffFiles();
        }

        private void btnCompareDirectoriesWithDiffTool_Clicked(object sender, EventArgs e)
        {
            Module.OpenWithDifftoolDirDiff((ckCompareToMergeBase.Checked ? _mergeBase : _baseRevision).Guid, _headRevision.Guid);
        }

        private void btnPickAnotherBranch_Click(object sender, EventArgs e)
        {
            PickAnotherBranch(_baseRevision, ref _baseCommitDisplayStr, ref _baseRevision);
        }

        private void btnAnotherCommit_Click(object sender, EventArgs e)
        {
            PickAnotherCommit(_baseRevision, ref _baseCommitDisplayStr, ref _baseRevision);
        }

        private void btnAnotherHeadBranch_Click(object sender, EventArgs e)
        {
            PickAnotherBranch(_headRevision, ref _headCommitDisplayStr, ref _headRevision);
        }

        private void btnAnotherHeadCommit_Click(object sender, EventArgs e)
        {
            PickAnotherCommit(_headRevision, ref _headCommitDisplayStr, ref _headRevision);
        }

        private void diffContextToolStripMenuItem_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool isAnyTracked = DiffFiles.SelectedItems.Any(item => item.Item.IsTracked);
            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;

            openWithDifftoolToolStripMenuItem.Enabled = isAnyTracked;
            fileHistoryDiffToolstripMenuItem.Enabled = isAnyTracked && isExactlyOneItemSelected;
            blameToolStripMenuItem.Enabled = fileHistoryDiffToolstripMenuItem.Enabled && !DiffFiles.SelectedGitItem.IsSubmodule;
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            var parentIds = DiffFiles.SelectedItems.FirstIds().ToList();
            bool firstIsParent = _revisionTester.AllFirstAreParentsToSelected(parentIds, _headRevision);
            bool localExists = _revisionTester.AnyLocalFileExists(DiffFiles.SelectedItems.Select(i => i.Item));

            bool allAreNew = DiffFiles.SelectedItems.All(i => i.Item.IsNew);
            bool allAreDeleted = DiffFiles.SelectedItems.All(i => i.Item.IsDeleted);

            return new ContextMenuDiffToolInfo(
                _headRevision,
                parentIds,
                allAreNew: allAreNew,
                allAreDeleted: allAreDeleted,
                firstIsParent: firstIsParent,
                firstParentsValid: _firstParentIsValid,
                localExists: localExists);
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();

            firstToSelectedToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo);
            firstToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo);
            selectedToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo);
            firstParentToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo);
            selectedParentToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo);
            firstParentToLocalToolStripMenuItem.Visible = _revisionDiffContextMenuController.ShouldDisplayMenuFirstParentToLocal(selectionInfo);
            selectedParentToLocalToolStripMenuItem.Visible = _revisionDiffContextMenuController.ShouldDisplayMenuSelectedParentToLocal(selectionInfo);
        }

        private void PickAnotherBranch(GitRevision preSelectCommit, ref string displayStr, [CanBeNull] ref GitRevision revision)
        {
            using (var form = new FormCompareToBranch(UICommands, preSelectCommit.ObjectId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    displayStr = form.BranchName;
                    var objectId = Module.RevParse(form.BranchName);
                    revision = objectId == null ? null : new GitRevision(objectId);
                    PopulateDiffFiles();
                }
            }
        }

        private void PickAnotherCommit(GitRevision preSelect, ref string displayStr, ref GitRevision revision)
        {
            using (var form = new FormChooseCommit(UICommands, preselectCommit: preSelect.Guid, showArtificial: true))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    revision = form.SelectedRevision;
                    displayStr = form.SelectedRevision.Subject;
                    PopulateDiffFiles();
                }
            }
        }
    }
}
