using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormFileHistory : GitModuleForm
    {
        private const string FormBrowseName = "FormBrowse";

        private readonly TranslationString _buildReportTabCaption = new("Build Report");
        private readonly AsyncLoader _asyncLoader = new();
        private readonly ICommitDataManager _commitDataManager;
        private readonly FormBrowseMenus _formBrowseMenus;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly FormFileHistoryController _controller = new();
        private readonly CancellationTokenSequence _customDiffToolsSequence = new();
        private readonly CancellationTokenSequence _viewChangesSequence = new();

        private BuildReportTabPageExtension? _buildReportTabPageExtension;

        private string FileName { get; set; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormFileHistory()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        /// <summary>
        /// Open FileHistory form.
        /// </summary>
        /// <param name="commands">commands in the current form.</param>
        /// <param name="fileName">name in repo of file to view.</param>
        /// <param name="revision">initial selected commit.</param>
        /// <param name="filterByRevision">add filter.</param>
        /// <param name="showBlame">show blame initially instead of diff view.</param>
        public FormFileHistory(GitUICommands commands, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false)
            : base(commands)
        {
            InitializeComponent();
            ConfigureTabControl();

            ToolStripFilters.Bind(() => Module, RevisionGrid);

            Color toolForeColor = SystemColors.WindowText;
            Color toolBackColor = Color.Transparent;
            BackColor = SystemColors.Window;
            ForeColor = toolForeColor;
            ToolStripFilters.BackColor = toolBackColor;
            ToolStripFilters.ForeColor = toolForeColor;
            ToolStripFilters.InitToolStripStyles(toolForeColor, toolBackColor);

            _formBrowseMenus = new FormBrowseMenus(FileHistoryContextMenu);
            _formBrowseMenus.ResetMenuCommandSets();
            _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, RevisionGrid.MenuCommands.NavigateMenuCommands);
            _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, RevisionGrid.MenuCommands.ViewMenuCommands);
            _formBrowseMenus.InsertRevisionGridMainMenuItems(toolStripSeparator4);

            _commitDataManager = new CommitDataManager(() => Module);
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);

            CommitDiff.EscapePressed += Close;
            View.EscapePressed += Close;
            Diff.EscapePressed += Close;
            Blame.EscapePressed += Close;

            copyToClipboardToolStripMenuItem.SetRevisionFunc(() => RevisionGrid.GetSelectedRevisions());

            InitializeComplete();

            Blame.ConfigureRepositoryHostPlugin(PluginRegistry.TryGetGitHosterForModule(Module));

            RevisionGrid.SelectedId = revision?.ObjectId;
            RevisionGrid.ShowBuildServerInfo = true;
            RevisionGrid.FilePathByObjectId = new();

            FileName = fileName;
            SetTitle();

            Diff.ExtraDiffArgumentsChanged += (sender, e) => UpdateSelectedFileViewers();

            var isSubmodule = GitModule.IsValidGitWorkingDir(_fullPathResolver.Resolve(FileName));

            if (isSubmodule)
            {
                tabControl1.RemoveIfExists(BlameTab);
            }

            RevisionGrid.SelectionChanged += FileChangesSelectionChanged;
            RevisionGrid.DisableContextMenu();

            bool blameTabExists = tabControl1.Contains(BlameTab);

            UpdateFollowHistoryMenuItems();

            showFullHistoryToolStripMenuItem.Checked = AppSettings.FullHistoryInFileHistory;
            simplifyMergesToolStripMenuItem.Checked = AppSettings.SimplifyMergesInFileHistory;
            simplifyMergesToolStripMenuItem.Enabled = AppSettings.FullHistoryInFileHistory;

            loadHistoryOnShowToolStripMenuItem.Checked = AppSettings.LoadFileHistoryOnShow;
            loadBlameOnShowToolStripMenuItem.Checked = AppSettings.LoadBlameOnShow && blameTabExists;
            saveAsToolStripMenuItem.Visible = !isSubmodule;

            toolStripBlameOptions.Visible = blameTabExists;
            if (blameTabExists)
            {
                ignoreWhitespaceToolStripMenuItem.Checked = AppSettings.IgnoreWhitespaceOnBlame;
                detectMoveAndCopyInAllFilesToolStripMenuItem.Checked = AppSettings.DetectCopyInAllOnBlame;
                detectMoveAndCopyInThisFileToolStripMenuItem.Checked = AppSettings.DetectCopyInFileOnBlame;
                displayAuthorFirstToolStripMenuItem.Checked = AppSettings.BlameDisplayAuthorFirst;
                showAuthorAvatarToolStripMenuItem.Checked = AppSettings.BlameShowAuthorAvatar;
                showAuthorToolStripMenuItem.Checked = AppSettings.BlameShowAuthor;
                showAuthorDateToolStripMenuItem.Checked = AppSettings.BlameShowAuthorDate;
                showAuthorTimeToolStripMenuItem.Checked = AppSettings.BlameShowAuthorTime;
                showLineNumbersToolStripMenuItem.Checked = AppSettings.BlameShowLineNumbers;
                showOriginalFilePathToolStripMenuItem.Checked = AppSettings.BlameShowOriginalFilePath;
            }

            if (filterByRevision && revision?.ObjectId is not null)
            {
                ToolStripFilters.SetRevisionFilter(revision.Guid);
            }

            tabControl1.SelectedTab = blameTabExists && showBlame ? BlameTab : DiffTab;

            return;

            void ConfigureTabControl()
            {
                tabControl1.ImageList = new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit,
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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _asyncLoader.Dispose();
                _customDiffToolsSequence.Dispose();
                _viewChangesSequence.Dispose();

                // if the form was instantiated by the translation app, all of the following would be null
                _formBrowseMenus?.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

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
                RevisionGrid.Visible = false;
            }

            LoadCustomDifftools();
        }

        public void LoadCustomDifftools()
        {
            List<CustomDiffMergeTool> menus = new()
            {
                new(openWithDifftoolToolStripMenuItem, OpenWithDifftoolToolStripMenuItem_Click),
                new(diffToolRemoteLocalStripMenuItem, diffToolRemoteLocalStripMenuItem_Click),
            };

            new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: true, cancellationToken: _customDiffToolsSequence.Next());
        }

        private void LoadFileHistory()
        {
            RevisionGrid.Visible = true;

            if (string.IsNullOrEmpty(FileName))
            {
                return;
            }

            // Replace windows path separator to Linux path separator.
            // This is needed to keep the file history working when started from file tree in
            // browse dialog.
            FileName = FileName.ToPosixPath();

            RevisionGrid.SetAndApplyPathFilter(FileName);
            RevisionGrid.Load();
        }

        private string? GetFileNameForRevision(GitRevision rev)
        {
            if (RevisionGrid.FilePathByObjectId is null)
            {
                return null;
            }

            ObjectId objectId = rev.IsArtificial ? RevisionGrid.CurrentCheckout : rev.ObjectId;

            return RevisionGrid.FilePathByObjectId.TryGetValue(objectId, out string? path) ? path : null;
        }

        private void FileChangesSelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectedFileViewers();
        }

        private void SetTitle(string? alternativeFileName = null)
        {
            var str = new StringBuilder()
                .Append("File History - ")
                .Append(FileName);

            if (!string.IsNullOrEmpty(alternativeFileName) && alternativeFileName != FileName)
            {
                str.Append(" (").Append(alternativeFileName).Append(')');
            }

            str.Append(" - ").Append(PathUtil.GetDisplayPath(Module.WorkingDir));

            Text = str.ToString();
        }

        private void UpdateSelectedFileViewers(bool force = false)
        {
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();

            if (selectedRevisions.Count == 0)
            {
                return;
            }

            GitRevision revision = selectedRevisions[0];
            var children = RevisionGrid.GetRevisionChildren(revision.ObjectId);
            string fileName = GetFileNameForRevision(revision) ?? FileName;

            SetTitle(fileName);

            TabPage preferredTab = null;
            if (revision.IsArtificial)
            {
                CommitInfoTabPage.Parent = null;
                preferredTab = DiffTab;
            }
            else
            {
                if (CommitInfoTabPage.Parent is null)
                {
                    tabControl1.TabPages.Insert(0, CommitInfoTabPage);
                }
            }

            if (fileName.EndsWith("/"))
            {
                // Note that artificial commits for object type tree (folder) will be handled here too,
                // i.e. no tab at all is visible
                DiffTab.Parent = null;
                preferredTab = CommitInfoTabPage;
            }
            else
            {
                if (DiffTab.Parent is null)
                {
                    int index = tabControl1.TabPages.IndexOf(CommitInfoTabPage);
                    Debug.Assert(index != -1, "TabControl should contain commit info tab page");
                    tabControl1.TabPages.Insert(index + 1, DiffTab);
                }
            }

            if (revision.IsArtificial || fileName.EndsWith("/"))
            {
                BlameTab.Parent = null;
                ViewTab.Parent = null;
            }
            else
            {
                if (ViewTab.Parent is null)
                {
                    int index = tabControl1.TabPages.IndexOf(DiffTab);
                    Debug.Assert(index != -1, "TabControl should contain diff tab page");
                    tabControl1.TabPages.Insert(index + 1, ViewTab);
                }

                if (BlameTab.Parent is null)
                {
                    int index = tabControl1.TabPages.IndexOf(ViewTab);
                    Debug.Assert(index != -1, "TabControl should contain view tab page");
                    tabControl1.TabPages.Insert(index + 1, BlameTab);
                }
            }

            if (tabControl1.SelectedTab?.Parent is null && preferredTab is not null)
            {
                tabControl1.SelectedTab = preferredTab;
            }

            if (tabControl1.SelectedTab == BlameTab)
            {
                _ = Blame.LoadBlameAsync(revision, children, fileName, RevisionGrid, BlameTab, Diff.Encoding, force: force, cancellationToken: _viewChangesSequence.Next());
            }
            else if (tabControl1.SelectedTab == ViewTab)
            {
                Validates.NotNull(fileName);
                View.Encoding = Diff.Encoding;
                GitItemStatus file = new(name: fileName)
                {
                    IsTracked = true,
                    IsSubmodule = GitModule.IsValidGitWorkingDir(_fullPathResolver.Resolve(fileName))
                };
                _ = View.ViewGitItemAsync(file, revision.ObjectId);
            }
            else if (tabControl1.SelectedTab == DiffTab)
            {
                Validates.NotNull(fileName);
                GitItemStatus file = new(name: fileName)
                {
                    IsTracked = true,
                    IsSubmodule = GitModule.IsValidGitWorkingDir(_fullPathResolver.Resolve(fileName))
                };
                var revisions = RevisionGrid.GetSelectedRevisions();
                FileStatusItem item = new(firstRev: revisions.Skip(1).LastOrDefault(), secondRev: revisions.FirstOrDefault(), file);
                _ = Diff.ViewChangesAsync(item, defaultText: TranslatedStrings.NoChanges,
                    cancellationToken: _viewChangesSequence.Next());
            }
            else if (tabControl1.SelectedTab == CommitInfoTabPage)
            {
                CommitDiff.SetRevision(revision.ObjectId, fileName);
            }

            _buildReportTabPageExtension ??= new BuildReportTabPageExtension(() => Module, tabControl1, _buildReportTabCaption.Text);

            _buildReportTabPageExtension.FillBuildReport(selectedRevisions.Count == 1 ? revision : null);
        }

        private void TabControl1SelectedIndexChanged(object sender, EventArgs e)
        {
            FileChangesSelectionChanged(sender, e);
        }

        private void FileChangesDoubleClick(object sender, EventArgs e)
        {
            RevisionGrid.ViewSelectedRevisions();
        }

        private void OpenWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFilesWithDiffTool(RevisionDiffKind.DiffAB, sender);
        }

        private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, object sender)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default mergetool
                item.HideDropDown();
                item.Owner.Hide();
            }

            var toolName = item?.Tag as string;
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            var orgFileName = selectedRevisions.Count != 0
                ? GetFileNameForRevision(selectedRevisions[0])
                : null;

            UICommands.OpenWithDifftool(this, selectedRevisions, FileName, orgFileName, diffKind, true, customTool: toolName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRows = RevisionGrid.GetSelectedRevisions();

            if (selectedRows.Count > 0)
            {
                string? orgFileName = GetFileNameForRevision(selectedRows[0]) ?? FileName;

                string? fullName = _fullPathResolver.Resolve(orgFileName);
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return;
                }

                fullName = fullName.ToNativePath();
                using SaveFileDialog fileDialog = new()
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = Path.GetExtension(fullName),
                    AddExtension = true
                };
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

        private void simplifyMergesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleSimplifyMergesFlag();
        }

        private void ToggleSimplifyMergesFlag()
        {
            AppSettings.SimplifyMergesInFileHistory = !AppSettings.SimplifyMergesInFileHistory;
            simplifyMergesToolStripMenuItem.Checked = AppSettings.SimplifyMergesInFileHistory;

            if (AppSettings.FullHistoryInFileHistory)
            {
                LoadFileHistory();
            }
        }

        private void ToggleFullHistoryFlag()
        {
            AppSettings.FullHistoryInFileHistory = !AppSettings.FullHistoryInFileHistory;
            showFullHistoryToolStripMenuItem.Checked = AppSettings.FullHistoryInFileHistory;

            simplifyMergesToolStripMenuItem.Enabled = AppSettings.FullHistoryInFileHistory;

            LoadFileHistory();
        }

        private void cherryPickThisCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                UICommands.StartCherryPickDialog(this, selectedRevisions[0]);
            }
        }

        private void revertCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                UICommands.StartRevertCommitDialog(this, selectedRevisions[0]);
            }
        }

        private void FileHistoryContextMenuOpening(object sender, CancelEventArgs e)
        {
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();

            diffToolRemoteLocalStripMenuItem.Enabled =
                selectedRevisions.Count == 1 && selectedRevisions[0].ObjectId != ObjectId.WorkTreeId &&
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
            OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal, sender);
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

        private void Blame_CommandClick(object sender, ResourceManager.CommandEventArgs e)
        {
            if (e.Command == "gotocommit")
            {
                Validates.NotNull(e.Data);
                if (Module.TryResolvePartialCommitId(e.Data, out var objectId))
                {
                    if (!RevisionGrid.SetSelectedRevision(objectId))
                    {
                        MessageBoxes.RevisionFilteredInGrid(this, objectId);
                    }
               }
            }
            else if (e.Command == "gotobranch" || e.Command == "gototag")
            {
                Validates.NotNull(e.Data);
                CommitData? commit = _commitDataManager.GetCommitData(e.Data, out _);
                if (commit is not null)
                {
                    if (!RevisionGrid.SetSelectedRevision(commit.ObjectId))
                    {
                        MessageBoxes.RevisionFilteredInGrid(this, commit.ObjectId);
                    }
                }
            }
            else if (e.Command == "navigatebackward")
            {
                RevisionGrid.NavigateBackward();
            }
            else if (e.Command == "navigateforward")
            {
                RevisionGrid.NavigateForward();
            }
        }

        private void followFileHistoryRenamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.FollowRenamesInFileHistoryExactOnly = !AppSettings.FollowRenamesInFileHistoryExactOnly;
            UpdateFollowHistoryMenuItems();
            LoadFileHistory();
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
            TranslationUtils.AddTranslationItemsFromFields(FormBrowseName, ToolStripFilters, translation);
        }

        public override void TranslateItems(ITranslation translation)
        {
            base.TranslateItems(translation);
            TranslationUtils.TranslateItemsFromFields(FormBrowseName, ToolStripFilters, translation);
        }

        #endregion

        private void displayAuthorFirstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.BlameDisplayAuthorFirst = !AppSettings.BlameDisplayAuthorFirst;
            displayAuthorFirstToolStripMenuItem.Checked = AppSettings.BlameDisplayAuthorFirst;
            UpdateSelectedFileViewers(true);
        }

        private void showAuthorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.BlameShowAuthor = !AppSettings.BlameShowAuthor;
            showAuthorToolStripMenuItem.Checked = AppSettings.BlameShowAuthor;

            if (!AppSettings.BlameShowAuthor)
            {
                showAuthorDateToolStripMenuItem.Checked = true;
                AppSettings.BlameShowAuthorDate = true;
            }

            UpdateSelectedFileViewers(true);
        }

        private void showAuthorDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.BlameShowAuthorDate = !AppSettings.BlameShowAuthorDate;
            showAuthorDateToolStripMenuItem.Checked = AppSettings.BlameShowAuthorDate;

            showAuthorTimeToolStripMenuItem.Enabled = AppSettings.BlameShowAuthorDate;

            if (!AppSettings.BlameShowAuthorDate)
            {
                showAuthorToolStripMenuItem.Checked = true;
                AppSettings.BlameShowAuthor = true;
            }

            UpdateSelectedFileViewers(true);
        }

        private void showAuthorTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.BlameShowAuthorTime = !AppSettings.BlameShowAuthorTime;
            showAuthorTimeToolStripMenuItem.Checked = AppSettings.BlameShowAuthorTime;
            UpdateSelectedFileViewers(true);
        }

        private void showLineNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.BlameShowLineNumbers = !AppSettings.BlameShowLineNumbers;
            showLineNumbersToolStripMenuItem.Checked = AppSettings.BlameShowLineNumbers;
            Blame.UpdateShowLineNumbers();
            UpdateSelectedFileViewers(true);
        }

        private void showOriginalFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.BlameShowOriginalFilePath = !AppSettings.BlameShowOriginalFilePath;
            showOriginalFilePathToolStripMenuItem.Checked = AppSettings.BlameShowOriginalFilePath;
            UpdateSelectedFileViewers(true);
        }

        private void showAuthorAvatarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.BlameShowAuthorAvatar = !AppSettings.BlameShowAuthorAvatar;
            showAuthorAvatarToolStripMenuItem.Checked = AppSettings.BlameShowAuthorAvatar;
            UpdateSelectedFileViewers(true);
        }

        private void GitcommandLogToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormGitCommandLog.ShowOrActivate(this);
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormFileHistory _form;

            public TestAccessor(FormFileHistory form)
            {
                _form = form;
            }

            public RevisionGridControl RevisionGrid => _form.RevisionGrid;
        }
    }
}
