using System.Diagnostics;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.Menus;
using GitUI.HelperDialogs;
using GitUI.Theming;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormDiff : GitModuleForm
    {
        private string? _firstCommitDisplayStr;
        private string? _secondCommitDisplayStr;
        private GitRevision? _firstRevision;
        private GitRevision? _secondRevision;
        private readonly GitRevision? _mergeBase;
        private Lazy<ObjectId?> _currentHead = null;

        private readonly IGitRevisionTester _revisionTester;
        private readonly IFileStatusListContextMenuController _revisionDiffContextMenuController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;
        private readonly CancellationTokenSequence _viewChangesSequence = new();

        private readonly ToolTip _toolTipControl = new();

        private readonly TranslationString _anotherBranchTooltip = new("Select another branch");
        private readonly TranslationString _anotherCommitTooltip = new("Select another commit");
        private readonly TranslationString _btnSwapTooltip = new("Swap BASE and Compare commits");
        private readonly TranslationString _ckCompareToMergeBase = new("Compare to merge &base");

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormDiff()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormDiff(
            GitUICommands commands,
            ObjectId firstId,
            ObjectId secondId,
            string firstCommitDisplayStr, string secondCommitDisplayStr)
            : base(commands)
        {
            _firstCommitDisplayStr = firstCommitDisplayStr;
            _secondCommitDisplayStr = secondCommitDisplayStr;

            InitializeComponent();

            btnSwap.AdaptImageLightness();

            InitializeComplete();

            _toolTipControl.SetToolTip(btnAnotherFirstBranch, _anotherBranchTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherSecondBranch, _anotherBranchTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherFirstCommit, _anotherCommitTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherSecondCommit, _anotherCommitTooltip.Text);
            _toolTipControl.SetToolTip(btnSwap, _btnSwapTooltip.Text);

            _firstRevision = new GitRevision(firstId);
            _secondRevision = new GitRevision(secondId);

            // _mergeBase is not changed if first/second is changed
            // similar, _currentHead is not updated if changed in Browse
            _currentHead = new(() => Module.GetCurrentCheckout());
            ObjectId? firstMergeId = firstId.IsArtificial ? _currentHead.Value : firstId;
            ObjectId? secondMergeId = secondId.IsArtificial ? _currentHead.Value : secondId;
            if (firstMergeId is null || secondMergeId is null || firstMergeId == secondMergeId)
            {
                _mergeBase = null;
            }
            else
            {
                ObjectId mergeBase = Module.GetMergeBase(firstMergeId, secondMergeId);
                _mergeBase = mergeBase is not null ? new GitRevision(mergeBase) : null;
            }

            ckCompareToMergeBase.Text = $"{_ckCompareToMergeBase} ({_mergeBase?.ObjectId.ToShortString()})";
            ckCompareToMergeBase.Enabled = _mergeBase is not null;

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
            _revisionTester = new GitRevisionTester(_fullPathResolver);
            _revisionDiffContextMenuController = new FileStatusListContextMenuController();

            lblFirstCommit.BackColor = AppColor.DiffRemoved.GetThemeColor();
            lblSecondCommit.BackColor = AppColor.DiffAdded.GetThemeColor();

            DiffFiles.ContextMenuStrip = DiffContextMenu;
            DiffFiles.SelectedIndexChanged += delegate { ShowSelectedFileDiff(); };
            DiffText.ExtraDiffArgumentsChanged += delegate { ShowSelectedFileDiff(); };
            DiffText.TopScrollReached += FileViewer_TopScrollReached;
            DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
            Load += delegate { PopulateDiffFiles(); };

            copyPathsToolStripMenuItem.Initialize(() => UICommands, () => DiffFiles.SelectedItems.Select(fsi => fsi.Item.Name));
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewChangesSequence.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
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
            lblFirstCommit.Text = _firstCommitDisplayStr;
            lblSecondCommit.Text = _secondCommitDisplayStr;

            // Bug in git-for-windows: Comparing working directory to any branch, fails, due to -R
            // I.e., git difftool --gui --no-prompt --dir-diff -R HEAD fails, but
            // git difftool --gui --no-prompt --dir-diff HEAD succeeds
            // Thus, we disable comparing "from" working directory.
            var enableDifftoolDirDiff = _firstRevision?.ObjectId != ObjectId.WorkTreeId;
            btnCompareDirectoriesWithDiffTool.Enabled = enableDifftoolDirDiff;

            Validates.NotNull(_secondRevision);
            GitRevision[] revisions;
            if (ckCompareToMergeBase.Checked)
            {
                Validates.NotNull(_mergeBase);
                revisions = new[] { _secondRevision, _mergeBase };
            }
            else
            {
                Validates.NotNull(_firstRevision);
                if (_mergeBase is null)
                {
                    revisions = new[] { _secondRevision, _firstRevision };
                }
                else
                {
                    revisions = new[] { _secondRevision, _mergeBase, _firstRevision };
                }
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await DiffFiles.SetDiffsAsync(revisions, _currentHead.Value, _viewChangesSequence.Next());
            }).FileAndForget();
        }

        private void ShowSelectedFileDiff()
        {
            _ = DiffText.ViewChangesAsync(DiffFiles.SelectedItem,
                cancellationToken: _viewChangesSequence.Next());
        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            var orgFirstRev = _firstRevision;
            _firstRevision = _secondRevision;
            _secondRevision = orgFirstRev;

            var orgFirstStr = _firstCommitDisplayStr;
            _firstCommitDisplayStr = _secondCommitDisplayStr;
            _secondCommitDisplayStr = orgFirstStr;
            PopulateDiffFiles();
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedGitItem is null)
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
                else
                {
                    Debug.Assert(sender == firstToSelectedToolStripMenuItem, "Not implemented DiffWithRevisionKind: " + sender);
                    return RevisionDiffKind.DiffAB;
                }
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void fileHistoryDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus? item = DiffFiles.SelectedGitItem;
            Validates.NotNull(item);

            if (item.IsTracked)
            {
                UICommands.StartFileHistoryDialog(this, item.Name, _firstRevision);
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus? item = DiffFiles.SelectedGitItem;
            Validates.NotNull(item);

            if (item.IsTracked)
            {
                UICommands.StartFileHistoryDialog(this, item.Name, _firstRevision, true, true);
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

            GitItemStatus? selectedItem;
            using (var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = this
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }

            if (selectedItem is not null)
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
            GitRevision? firstRevision = ckCompareToMergeBase.Checked ? _mergeBase : _firstRevision;
            Validates.NotNull(firstRevision);
            Validates.NotNull(_secondRevision);
            Module.OpenWithDifftoolDirDiff(firstRevision.Guid, _secondRevision.Guid);
        }

        private void btnPickAnotherFirstBranch_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_firstRevision);
            PickAnotherBranch(_firstRevision, ref _firstCommitDisplayStr, ref _firstRevision);
        }

        private void btnAnotherFirstCommit_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_firstRevision);
            PickAnotherCommit(_firstRevision, ref _firstCommitDisplayStr, ref _firstRevision);
        }

        private void btnAnotherSecondBranch_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_secondRevision);
            PickAnotherBranch(_secondRevision, ref _secondCommitDisplayStr, ref _secondRevision);
        }

        private void btnAnotherSecondCommit_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_secondRevision);
            PickAnotherCommit(_secondRevision, ref _secondCommitDisplayStr, ref _secondRevision);
        }

        private void diffContextToolStripMenuItem_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool isAnyTracked = DiffFiles.SelectedItems.Any(item => item.Item.IsTracked);
            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;

            openWithDifftoolToolStripMenuItem.Enabled = isAnyTracked;
            fileHistoryDiffToolstripMenuItem.Enabled = isAnyTracked && isExactlyOneItemSelected;
            Validates.NotNull(DiffFiles.SelectedGitItem);
            blameToolStripMenuItem.Enabled = fileHistoryDiffToolstripMenuItem.Enabled && !DiffFiles.SelectedGitItem.IsSubmodule;
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            var parentIds = DiffFiles.SelectedItems.FirstIds().ToList();
            bool firstIsParent = _revisionTester.AllFirstAreParentsToSelected(parentIds, _secondRevision);
            bool localExists = _revisionTester.AnyLocalFileExists(DiffFiles.SelectedItems.Select(i => i.Item));

            bool allAreNew = DiffFiles.SelectedItems.All(i => i.Item.IsNew);
            bool allAreDeleted = DiffFiles.SelectedItems.All(i => i.Item.IsDeleted);

            return new ContextMenuDiffToolInfo(
                _secondRevision,
                parentIds,
                allAreNew: allAreNew,
                allAreDeleted: allAreDeleted,
                firstIsParent: firstIsParent,
                localExists: localExists);
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();

            firstToSelectedToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo);
            firstToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo);
            selectedToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo);
        }

        private void PickAnotherBranch(GitRevision preSelectCommit, ref string? displayStr, ref GitRevision? revision)
        {
            using FormCompareToBranch form = new(UICommands, preSelectCommit.ObjectId);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                displayStr = form.BranchName;
                var objectId = Module.RevParse(form.BranchName);
                revision = objectId is null ? null : new GitRevision(objectId);
                PopulateDiffFiles();
            }
        }

        private void PickAnotherCommit(GitRevision preSelect, ref string? displayStr, ref GitRevision? revision)
        {
            using FormChooseCommit form = new(UICommands, preselectCommit: preSelect.Guid, showArtificial: true);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                revision = form.SelectedRevision;
                displayStr = form.SelectedRevision?.Subject;
                PopulateDiffFiles();
            }
        }
    }
}
