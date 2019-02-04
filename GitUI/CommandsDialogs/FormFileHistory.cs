using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Properties;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormFileHistory : GitModuleForm
    {
        private const string FormBrowseName = "FormBrowse";

        private readonly TranslationString _buildReportTabCaption = new TranslationString("Build Report");
        private readonly AsyncLoader _asyncLoader = new AsyncLoader();
        private readonly ICommitDataManager _commitDataManager;
        private readonly FilterRevisionsHelper _filterRevisionsHelper;
        private readonly FilterBranchHelper _filterBranchHelper;
        private readonly FormBrowseMenus _formBrowseMenus;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly FormFileHistoryController _controller = new FormFileHistoryController();

        private BuildReportTabPageExtension _buildReportTabPageExtension;

        private string FileName { get; set; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormFileHistory()
        {
            InitializeComponent();
        }

        private FormFileHistory([NotNull] GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            ConfigureTabControl();

            _filterBranchHelper = new FilterBranchHelper(toolStripBranchFilterComboBox, toolStripBranchFilterDropDownButton, FileChanges);
            _filterRevisionsHelper = new FilterRevisionsHelper(toolStripRevisionFilterTextBox, toolStripRevisionFilterDropDownButton, FileChanges, toolStripRevisionFilterLabel, ShowFirstParent, form: this);

            _formBrowseMenus = new FormBrowseMenus(FileHistoryContextMenu);
            _formBrowseMenus.ResetMenuCommandSets();
            _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, FileChanges.MenuCommands.NavigateMenuCommands);
            _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, FileChanges.MenuCommands.ViewMenuCommands);
            _formBrowseMenus.InsertAdditionalMainMenuItems(toolStripSeparator4);

            _commitDataManager = new CommitDataManager(() => Module);
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);

            CommitDiff.EscapePressed += Close;
            View.EscapePressed += Close;
            Diff.EscapePressed += Close;
            Blame.EscapePressed += Close;

            copyToClipboardToolStripMenuItem.SetRevisionFunc(() => FileChanges.GetSelectedRevisions());

            InitializeComplete();

            return;

            void ConfigureTabControl()
            {
                tabControl1.ImageList = new ImageList
                {
                    ColorDepth = ColorDepth.Depth8Bit,
                    ImageSize = DpiUtil.Scale(new Size(16, 16)),
                    Images =
                    {
                        Images.CommitSummary,
                        Images.Diff,
                        Images.ViewFile,
                        Images.Blame
                    }
                };
                tabControl1.TabPages[0].ImageIndex = 0;
                tabControl1.TabPages[1].ImageIndex = 1;
                tabControl1.TabPages[2].ImageIndex = 2;
                tabControl1.TabPages[3].ImageIndex = 3;
            }
        }

        public FormFileHistory(GitUICommands commands, string fileName, GitRevision revision = null, bool filterByRevision = false)
            : this(commands)
        {
            FileChanges.InitialObjectId = revision?.ObjectId;
            FileChanges.ShowBuildServerInfo = true;

            FileName = fileName;
            SetTitle();

            Diff.ExtraDiffArgumentsChanged += (sender, e) => UpdateSelectedFileViewers();

            var isSubmodule = GitModule.IsValidGitWorkingDir(_fullPathResolver.Resolve(FileName));

            if (isSubmodule)
            {
                tabControl1.RemoveIfExists(BlameTab);
            }

            FileChanges.SelectionChanged += FileChangesSelectionChanged;
            FileChanges.DisableContextMenu();

            bool blameTabExists = tabControl1.Contains(BlameTab);

            UpdateFollowHistoryMenuItems();

            fullHistoryToolStripMenuItem.Checked = AppSettings.FullHistoryInFileHistory;
            showFullHistoryToolStripMenuItem.Checked = AppSettings.FullHistoryInFileHistory;
            simplifyMergesToolStripMenuItem.Checked = AppSettings.SimplifyMergesInFileHistory;
            simplifyMergesToolStripMenuItem.Enabled = AppSettings.FullHistoryInFileHistory;
            simplifyMergesContextMenuItem.Checked = AppSettings.SimplifyMergesInFileHistory;
            simplifyMergesContextMenuItem.Enabled = AppSettings.FullHistoryInFileHistory;

            loadHistoryOnShowToolStripMenuItem.Checked = AppSettings.LoadFileHistoryOnShow;
            loadBlameOnShowToolStripMenuItem.Checked = AppSettings.LoadBlameOnShow && blameTabExists;
            saveAsToolStripMenuItem.Visible = !isSubmodule;

            toolStripBlameOptions.Visible = blameTabExists;
            if (blameTabExists)
            {
                ignoreWhitespaceToolStripMenuItem.Checked = AppSettings.IgnoreWhitespaceOnBlame;
                detectMoveAndCopyInAllFilesToolStripMenuItem.Checked = AppSettings.DetectCopyInAllOnBlame;
                detectMoveAndCopyInThisFileToolStripMenuItem.Checked = AppSettings.DetectCopyInFileOnBlame;
            }

            if (filterByRevision && revision?.Guid != null)
            {
                _filterBranchHelper.SetBranchFilter(revision.Guid, false);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _asyncLoader.Dispose();

                // if the form was instantiated by the translation app, all of the following would be null
                _filterRevisionsHelper?.Dispose();
                _filterBranchHelper?.Dispose();
                _formBrowseMenus?.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        public void SelectBlameTab() => tabControl1.SelectedTab = BlameTab;
        public void SelectDiffTab() => tabControl1.SelectedTab = DiffTab;

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            bool autoLoad = (tabControl1.SelectedTab == BlameTab && AppSettings.LoadBlameOnShow) || AppSettings.LoadFileHistoryOnShow;

            if (autoLoad)
            {
                LoadFileHistory();
            }
            else
            {
                FileChanges.Visible = false;
            }
        }

        private void LoadFileHistory()
        {
            FileChanges.Visible = true;

            if (string.IsNullOrEmpty(FileName))
            {
                return;
            }

            _asyncLoader.LoadAsync(
                () => BuildFilter(),
                filter =>
                {
                    FileChanges.SetFilters(filter);
                    FileChanges.Load();
                });

            return;

            (string revision, string path) BuildFilter()
            {
                var fileName = FileName;

                // we will need this later to look up proper casing for the file
                var fullFilePath = _fullPathResolver.Resolve(fileName);

                if (_controller.TryGetExactPath(fullFilePath, out fileName))
                {
                    fileName = fileName.Substring(Module.WorkingDir.Length);
                }

                // Replace windows path separator to Linux path separator.
                // This is needed to keep the file history working when started from file tree in
                // browse dialog.
                FileName = fileName.ToPosixPath();

                var res = (revision: (string)null, path: $" \"{fileName}\"");

                if (AppSettings.FollowRenamesInFileHistory && !Directory.Exists(fullFilePath))
                {
                    // git log --follow is not working as expected (see  http://kerneltrap.org/mailarchive/git/2009/1/30/4856404/thread)
                    //
                    // But we can take a more complicated path to get reasonable results:
                    //  1. use git log --follow to get all previous filenames of the file we are interested in
                    //  2. use git log "list of files names" to get the history graph
                    //
                    // note: This implementation is quite a quick hack (by someone who does not speak C# fluently).
                    //

                    var args = new GitArgumentBuilder("log")
                    {
                        "--format=\"%n\"",
                        "--name-only",
                        GitCommandHelpers.FindRenamesAndCopiesOpts(),
                        "--",
                        fileName.Quote()
                    };

                    var listOfFileNames = new StringBuilder(fileName.Quote());

                    // keep a set of the file names already seen
                    var setOfFileNames = new HashSet<string> { fileName };

                    var lines = Module.GitExecutable.GetOutputLines(args, outputEncoding: GitModule.LosslessEncoding);

                    foreach (var line in lines.Select(GitModule.ReEncodeFileNameFromLossless))
                    {
                        if (!string.IsNullOrEmpty(line) && setOfFileNames.Add(line))
                        {
                            listOfFileNames.Append(" \"");
                            listOfFileNames.Append(line);
                            listOfFileNames.Append('\"');
                        }
                    }

                    // here we need --name-only to get the previous filenames in the revision graph
                    res.path = listOfFileNames.ToString();
                    res.revision += " --name-only --parents" + GitCommandHelpers.FindRenamesAndCopiesOpts();
                }
                else if (AppSettings.FollowRenamesInFileHistory)
                {
                    // history of a directory
                    // --parents doesn't work with --follow enabled, but needed to graph a filtered log
                    res.revision = " " + GitCommandHelpers.FindRenamesOpt() + " --follow --parents";
                }
                else
                {
                    // rename following disabled
                    res.revision = " --parents";
                }

                if (AppSettings.FullHistoryInFileHistory)
                {
                    res.revision = string.Concat(" --full-history ", AppSettings.SimplifyMergesInFileHistory ? "--simplify-merges " : string.Empty, res.revision);
                }

                return res;
            }
        }

        private void FileChangesSelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectedFileViewers();
        }

        private void SetTitle([CanBeNull] string alternativeFileName = null)
        {
            var str = new StringBuilder()
                .Append("File History - ")
                .Append(FileName);

            if (!alternativeFileName.IsNullOrEmpty() && alternativeFileName != FileName)
            {
                str.Append(" (").Append(alternativeFileName).Append(')');
            }

            str.Append(" - ").Append(PathUtil.GetDisplayPath(Module.WorkingDir));

            Text = str.ToString();
        }

        private void UpdateSelectedFileViewers(bool force = false)
        {
            var selectedRevisions = FileChanges.GetSelectedRevisions();

            if (selectedRevisions.Count == 0)
            {
                return;
            }

            GitRevision revision = selectedRevisions[0];
            var children = FileChanges.GetRevisionChildren(revision.ObjectId);

            var fileName = revision.Name;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = FileName;
            }

            SetTitle(fileName);

            if (revision.IsArtificial)
            {
                tabControl1.SelectedTab = DiffTab;

                CommitInfoTabPage.Parent = null;
                BlameTab.Parent = null;
                ViewTab.Parent = null;
            }
            else
            {
                if (CommitInfoTabPage.Parent == null)
                {
                    tabControl1.TabPages.Insert(0, CommitInfoTabPage);
                }

                if (ViewTab.Parent == null)
                {
                    var index = tabControl1.TabPages.IndexOf(DiffTab);
                    Debug.Assert(index != -1, "TabControl should contain diff tab page");
                    tabControl1.TabPages.Insert(index + 1, ViewTab);
                }

                if (BlameTab.Parent == null)
                {
                    var index = tabControl1.TabPages.IndexOf(ViewTab);
                    Debug.Assert(index != -1, "TabControl should contain view tab page");
                    tabControl1.TabPages.Insert(index + 1, BlameTab);
                }
            }

            if (tabControl1.SelectedTab == BlameTab)
            {
                Blame.LoadBlame(revision, children, fileName, FileChanges, BlameTab, Diff.Encoding, force: force);
            }
            else if (tabControl1.SelectedTab == ViewTab)
            {
                View.Encoding = Diff.Encoding;
                View.ViewGitItemRevisionAsync(fileName, revision.ObjectId);
            }
            else if (tabControl1.SelectedTab == DiffTab)
            {
                var file = new GitItemStatus
                {
                    IsTracked = true,
                    Name = fileName,
                    IsSubmodule = GitModule.IsValidGitWorkingDir(_fullPathResolver.Resolve(fileName))
                };
                Diff.ViewChangesAsync(FileChanges.GetSelectedRevisions(), file, "You need to select at least one revision to view diff.");
            }
            else if (tabControl1.SelectedTab == CommitInfoTabPage)
            {
                CommitDiff.SetRevision(revision.ObjectId, fileName);
            }

            if (_buildReportTabPageExtension == null)
            {
                _buildReportTabPageExtension = new BuildReportTabPageExtension(() => Module, tabControl1, _buildReportTabCaption.Text);
            }

            _buildReportTabPageExtension.FillBuildReport(selectedRevisions.Count == 1 ? revision : null);
        }

        private void TabControl1SelectedIndexChanged(object sender, EventArgs e)
        {
            FileChangesSelectionChanged(sender, e);
        }

        private void FileChangesDoubleClick(object sender, EventArgs e)
        {
            FileChanges.ViewSelectedRevisions();
        }

        private void OpenWithDifftoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRevisions = FileChanges.GetSelectedRevisions();

            var orgFileName = selectedRevisions.Count != 0
                ? selectedRevisions[0].Name
                : null;

            UICommands.OpenWithDifftool(this, selectedRevisions, FileName, orgFileName, RevisionDiffKind.DiffAB, true);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRows = FileChanges.GetSelectedRevisions();

            if (selectedRows.Count > 0)
            {
                string orgFileName = selectedRows[0].Name;

                if (string.IsNullOrEmpty(orgFileName))
                {
                    orgFileName = FileName;
                }

                string fullName = _fullPathResolver.Resolve(orgFileName.ToNativePath());

                using (var fileDialog = new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = PathUtil.GetFileExtension(fullName),
                    AddExtension = true
                })
                {
                    fileDialog.Filter =
                        "Current format (*." +
                        fileDialog.DefaultExt + ")|*." +
                        fileDialog.DefaultExt +
                        "|All files (*.*)|*.*";
                    if (fileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Module.SaveBlobAs(fileDialog.FileName, selectedRows[0].Guid + ":\"" + orgFileName + "\"");
                    }
                }
            }
        }

        private void followFileHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.FollowRenamesInFileHistory = !AppSettings.FollowRenamesInFileHistory;
            UpdateFollowHistoryMenuItems();

            LoadFileHistory();
        }

        private void UpdateFollowHistoryMenuItems()
        {
            followFileHistoryToolStripMenuItem.Checked = AppSettings.FollowRenamesInFileHistory;
            followFileHistoryRenamesToolStripMenuItem.Enabled = AppSettings.FollowRenamesInFileHistory;
            followFileHistoryRenamesToolStripMenuItem.Checked = AppSettings.FollowRenamesInFileHistoryExactOnly;
        }

        private void showFullHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFullHistoryFlag();
        }

        private void fullHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFullHistoryFlag();
        }

        private void simplifyMergesContextMenuItem_Click(object sender, EventArgs e)
        {
            ToggleSimplifyMergesFlag();
        }

        private void simplifyMergesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleSimplifyMergesFlag();
        }

        private void ToggleSimplifyMergesFlag()
        {
            AppSettings.SimplifyMergesInFileHistory = !AppSettings.SimplifyMergesInFileHistory;
            simplifyMergesToolStripMenuItem.Checked = AppSettings.SimplifyMergesInFileHistory;
            simplifyMergesContextMenuItem.Checked = AppSettings.SimplifyMergesInFileHistory;

            if (AppSettings.FullHistoryInFileHistory)
            {
                LoadFileHistory();
            }
        }

        private void ToggleFullHistoryFlag()
        {
            AppSettings.FullHistoryInFileHistory = !AppSettings.FullHistoryInFileHistory;
            fullHistoryToolStripMenuItem.Checked = AppSettings.FullHistoryInFileHistory;
            showFullHistoryToolStripMenuItem.Checked = AppSettings.FullHistoryInFileHistory;

            simplifyMergesContextMenuItem.Enabled = AppSettings.FullHistoryInFileHistory;
            simplifyMergesToolStripMenuItem.Enabled = AppSettings.FullHistoryInFileHistory;

            LoadFileHistory();
        }

        private void cherryPickThisCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRevisions = FileChanges.GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                UICommands.StartCherryPickDialog(this, selectedRevisions[0]);
            }
        }

        private void revertCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRevisions = FileChanges.GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                UICommands.StartRevertCommitDialog(this, selectedRevisions[0]);
            }
        }

        private void FileHistoryContextMenuOpening(object sender, CancelEventArgs e)
        {
            var selectedRevisions = FileChanges.GetSelectedRevisions();

            diffToolRemoteLocalStripMenuItem.Enabled =
                selectedRevisions.Count == 1 && selectedRevisions[0].Guid != GitRevision.WorkTreeGuid &&
                File.Exists(_fullPathResolver.Resolve(FileName));
            openWithDifftoolToolStripMenuItem.Enabled =
                selectedRevisions.Count >= 1 && selectedRevisions.Count <= 2;
            manipulateCommitToolStripMenuItem.Enabled =
                selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial;
            saveAsToolStripMenuItem.Enabled = selectedRevisions.Count == 1;
            copyToClipboardToolStripMenuItem.Enabled =
                selectedRevisions.Count >= 1 && !selectedRevisions[0].IsArtificial;
        }

        private void diffToolRemoteLocalStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.OpenWithDifftool(this, FileChanges.GetSelectedRevisions(), FileName, string.Empty, RevisionDiffKind.DiffBLocal, true);
        }

        private void toolStripSplitLoad_ButtonClick(object sender, EventArgs e)
        {
            LoadFileHistory();
        }

        private void loadHistoryOnShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.LoadFileHistoryOnShow = !AppSettings.LoadFileHistoryOnShow;
            loadHistoryOnShowToolStripMenuItem.Checked = AppSettings.LoadFileHistoryOnShow;
        }

        private void loadBlameOnShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.LoadBlameOnShow = !AppSettings.LoadBlameOnShow;
            loadBlameOnShowToolStripMenuItem.Checked = AppSettings.LoadBlameOnShow;
        }

        private void Blame_CommandClick(object sender, CommitInfo.CommandEventArgs e)
        {
            if (e.Command == "gotocommit")
            {
                if (Module.TryResolvePartialCommitId(e.Data, out var objectId))
                {
                    FileChanges.SetSelectedRevision(objectId);
                }
            }
            else if (e.Command == "gotobranch" || e.Command == "gototag")
            {
                CommitData commit = _commitDataManager.GetCommitData(e.Data, out _);
                if (commit != null)
                {
                    FileChanges.SetSelectedRevision(new GitRevision(commit.ObjectId));
                }
            }
            else if (e.Command == "navigatebackward")
            {
                FileChanges.NavigateBackward();
            }
            else if (e.Command == "navigateforward")
            {
                FileChanges.NavigateForward();
            }
        }

        private void followFileHistoryRenamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.FollowRenamesInFileHistoryExactOnly = !AppSettings.FollowRenamesInFileHistoryExactOnly;
            UpdateFollowHistoryMenuItems();
            LoadFileHistory();
        }

        private void toolStripBranchFilterComboBox_Click(object sender, EventArgs e)
        {
            toolStripBranchFilterComboBox.DroppedDown = true;
        }

        private void ignoreWhitespaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.IgnoreWhitespaceOnBlame = !AppSettings.IgnoreWhitespaceOnBlame;
            ignoreWhitespaceToolStripMenuItem.Checked = AppSettings.IgnoreWhitespaceOnBlame;
            UpdateSelectedFileViewers(true);
        }

        private void detectMoveAndCopyInAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.DetectCopyInAllOnBlame = !AppSettings.DetectCopyInAllOnBlame;
            detectMoveAndCopyInAllFilesToolStripMenuItem.Checked = AppSettings.DetectCopyInAllOnBlame;
            UpdateSelectedFileViewers(true);
        }

        private void detectMoveAndCopyInThisFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.DetectCopyInFileOnBlame = !AppSettings.DetectCopyInFileOnBlame;
            detectMoveAndCopyInThisFileToolStripMenuItem.Checked = AppSettings.DetectCopyInFileOnBlame;
            UpdateSelectedFileViewers(true);
        }

        #region Translation

        public override void AddTranslationItems(ITranslation translation)
        {
            base.AddTranslationItems(translation);
            TranslationUtils.AddTranslationItemsFromFields(FormBrowseName, _filterRevisionsHelper, translation);
            TranslationUtils.AddTranslationItemsFromFields(FormBrowseName, _filterBranchHelper, translation);
        }

        public override void TranslateItems(ITranslation translation)
        {
            base.TranslateItems(translation);
            TranslationUtils.TranslateItemsFromFields(FormBrowseName, _filterRevisionsHelper, translation);
            TranslationUtils.TranslateItemsFromFields(FormBrowseName, _filterBranchHelper, translation);
        }

        #endregion
    }
}