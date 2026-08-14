using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitExtensions.Plugins.Gource;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.RepoHosting;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommitInfo;
using GitUI.LeftPanel;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.Settings;
using GitUIPluginInterfaces;

namespace WinFormsParityCapture;

internal static class ComponentFactory
{
    public static Control Create(CaptureComponentPlan component, GitUICommands commands)
    {
        Control control = component.TypeName switch
        {
            "GitUI.CommandsDialogs.FormBrowse" => new FormBrowse(commands, new BrowseArguments()),
            "GitUI.CommandsDialogs.FormCommit" => new FormCommit(commands),
            "GitUI.CommandsDialogs.FormStash" => new FormStash(commands),
            "GitUI.CommandsDialogs.FormSettings" => new FormSettings(commands),
            "GitUI.CommandsDialogs.FormDiff" => CreateFormDiff(commands),
            "GitUI.CommandsDialogs.FormCompareToBranch" => new FormCompareToBranch(commands, commands.Module.RevParse("HEAD")),
            "GitUI.CommandsDialogs.FormFormatPatch" => new FormFormatPatch(commands),
            "GitUI.CommandsDialogs.BrowseDialog.FormGoToCommit" => new FormGoToCommit(commands),
            "GitUI.CommandsDialogs.FormCheckoutRevision" => CreateCheckoutRevision(commands),
            "GitUI.CommandsDialogs.RepoHosting.CreatePullRequestForm" =>
                new CreatePullRequestForm(commands, RepositoryHostCaptureFixture.Create(commands), null, null),
            "GitUI.CommandsDialogs.RepoHosting.ForkAndCloneForm" =>
                new ForkAndCloneForm(commands, RepositoryHostCaptureFixture.Create(commands), null),
            "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm" =>
                new ViewPullRequestsForm(commands, RepositoryHostCaptureFixture.Create(commands)),
            "GitUI.CommandsDialogs.SearchControl" => CreateSearchControl(),
            "GitUI.CommandsDialogs.SearchWindow" => CreateSearchWindow(),
            "GitUI.CommitInfo.CommitInfo" => CreateCommitInfo(),
            "GitUI.CommitInfo.CommitInfoHeader" => CreateCommitInfoHeader(),
            "GitUI.LeftPanel.RepoObjectsTree" => CreateRepoObjectsTree(commands),
            "GitUI.UserControls.RevisionGrid.EmptyRepoControl" => new EmptyRepoControl(),

            // parity-scaffolding: Hosts the internal modeless editor-search dialog without changing GitUI visibility.
            "GitUI.FormFindInCommitFilesGitGrep" => CreateWithCommands(component.TypeName, commands),
            "GitUI.CommandsDialogs.SettingsDialog.Pages.ColorsSettingsPage" =>
                CreateSettingsPage(new ColorsSettingsPage(GitUICommands.EmptyServiceProvider)),
            "GitUI.CommandsDialogs.SettingsDialog.Pages.BlameViewerSettingsPage" =>
                CreateSettingsPage(new BlameViewerSettingsPage(commands)),
            "GitUI.CommandsDialogs.SettingsDialog.Pages.CommitDialogSettingsPage" =>
                CreateSettingsPage(new CommitDialogSettingsPage(commands)),
            "GitUI.CommandsDialogs.SettingsDialog.Pages.FormBrowseRepoSettingsPage" =>
                CreateSettingsPage(new FormBrowseRepoSettingsPage(commands)),
            "GitUI.CommandsDialogs.SettingsDialog.Pages.ShellExtensionSettingsPage" =>
                CreateSettingsPage(new ShellExtensionSettingsPage(commands)),
            "GitExtensions.Plugins.Gource.GourceStart" => new GourceStart(string.Empty, null!, string.Empty),
            _ => CreateParameterless(component.TypeName)
        };
        PrepareInitialSize(control);
        foreach ((string fieldName, string text) in component.TextValues)
        {
            if (FindFieldValue(control, fieldName) is not Control target)
            {
                throw new InvalidDataException($"Text seed field '{fieldName}' was not found on {component.TypeName}.");
            }

            target.Text = text;
        }

        return control;
    }

    // parity-scaffolding: Standalone settings pages are normally loaded by FormSettings.
    private static T CreateSettingsPage<T>(T page) where T : SettingsPageBase
    {
        page.LoadSettings();
        return page;
    }

    // parity-scaffolding: Code-only controls have no Designer-owned size when hosted standalone.
    private static void PrepareInitialSize(Control control)
    {
        control.Size = control switch
        {
            WaitSpinner => new Size(48, 48),
            WatermarkComboBox or CaseSensitiveComboBox => new Size(250, 23),
            RepoObjectsTree => new Size(360, 560),
            _ => control.Size
        };
    }

    // parity-scaffolding: Populates the same commit-details state used by the Avalonia capture host.
    private static CommitInfo CreateCommitInfo()
    {
        return new CommitInfo { ShowBranchesAsLinks = true };
    }

    // parity-scaffolding: Populates the standalone header with the tranche's representative revision.
    private static CommitInfoHeader CreateCommitInfoHeader()
    {
        return new CommitInfoHeader();
    }

    // parity-scaffolding: Gives the checkout dialog a deterministic initial revision.
    private static FormCheckoutRevision CreateCheckoutRevision(GitUICommands commands)
    {
        FormCheckoutRevision form = new(commands);
        form.SetRevision("HEAD");
        return form;
    }

    // parity-scaffolding: Closes the open generic capture boundary with representative paths.
    private static SearchControl<string> CreateSearchControl()
        => new(SearchCandidates, _ => { });

    // parity-scaffolding: Closes the open generic capture boundary with representative paths.
    private static SearchWindow<string> CreateSearchWindow()
        => new(SearchCandidates);

    private static IEnumerable<string> SearchCandidates(string value)
        => new[] { "src/App.cs", "src/Commands/Checkout.cs", "tests/SearchTests.cs" }
            .Where(candidate => candidate.Contains(value, StringComparison.OrdinalIgnoreCase));

    // parity-scaffolding: Hosts the original tree under a commands source while its model is initialised.
    private static RepoObjectsTree CreateRepoObjectsTree(GitUICommands commands)
    {
        RepoObjectsTree tree = new();
        CaptureCommandsHost host = new(commands);
        host.Controls.Add(tree);
        CaptureRevisionGridInfo revisionGridInfo = new(commands.Module);
        tree.Initialize(
            aheadBehindDataProvider: null,
            filterRevisionGridBySpaceSeparatedRefs: _ => { },
            refsSource: revisionGridInfo,
            revisionGridInfo);
        tree.RefreshRevisionsLoading(
            commands.Module.GetRefs,
            new Lazy<IReadOnlyCollection<GitRevision>>(() => []),
            forceRefresh: true);
        tree.RefreshRevisionsLoaded();
        TreeView treeMain = (TreeView?)FindFieldValue(tree, "treeMain")
            ?? throw new InvalidOperationException("RepoObjectsTree did not create treeMain.");
        TreeNode selectedNode = treeMain.Nodes[0];
        treeMain.Nodes[0].Expand();
        treeMain.SelectedNode = selectedNode;
        InvokeNonPublic(tree, "SelectNode", selectedNode.Tag!, false, false);
        host.Controls.Remove(tree);
        host.Dispose();
        return tree;
    }

    // parity-scaffolding: Runs control logic only after WinForms has created the capture host handle.
    public static void PrepareAfterHandle(Control control, IGitUICommands commands, CaptureComponentPlan component)
    {
        CaptureCommandsSource source = new(commands);
        switch (control)
        {
            case CommitInfo commitInfo:
                commitInfo.UICommandsSource = source;
                commitInfo.Revision = CreateRevision(commands);
                break;
            case CommitInfoHeader commitInfoHeader:
                commitInfoHeader.UICommandsSource = source;
                commitInfoHeader.ShowCommitInfo(CreateRevision(commands), [commands.Module.RevParse("HEAD~1")]);
                break;
            case RevisionGridControl revisionGrid:
                revisionGrid.PerformRefreshRevisions(forceRefresh: true);
                break;
            case BranchSelector branchSelector:
                branchSelector.UICommandsSource = source;
                branchSelector.Initialize(remote: false, containObjectIds: null);
                break;
            case InteractiveGitActionControl interactiveGitActionControl:
                interactiveGitActionControl.UICommandsSource = source;
                InvokeNonPublic(
                    interactiveGitActionControl,
                    "SetGitAction",
                    InteractiveGitActionControl.GitAction.Rebase,
                    false);
                break;
            case SettingsCheckBox settingsCheckBox:
                settingsCheckBox.Text = "Enable representative setting";
                settingsCheckBox.ToolTipText = "Representative setting information";
                break;
            case WaitSpinner waitSpinner:
                waitSpinner.IsAnimating = false;
                SetNonPublicField(waitSpinner, "_progress", 7);
                waitSpinner.Invalidate();
                break;
            case LoadingControl loadingControl:
                loadingControl.IsAnimating = false;
                WaitSpinner loadingSpinner = (WaitSpinner?)FindFieldValue(loadingControl, "_waitSpinner")
                    ?? throw new InvalidOperationException("LoadingControl did not create its WaitSpinner.");
                SetNonPublicField(loadingSpinner, "_progress", 7);
                loadingSpinner.Invalidate();
                break;
            case WatermarkComboBox watermarkComboBox:
                watermarkComboBox.Watermark = "Filter files using a regular expression...";
                break;
            case CaseSensitiveComboBox caseSensitiveComboBox:
                caseSensitiveComboBox.Items.AddRange(["Main", "main", "release/1.0"]);
                caseSensitiveComboBox.Text = "main";
                break;
            case FileStatusList fileStatusList:
                SeedFileStatusList(fileStatusList, commands);
                break;

            // parity-scaffolding: Seeds the isolated Dashboard history before paired capture.
            case Dashboard dashboard:
                dashboard.UICommandsSource = source;
                Repository repository = new(commands.Module.WorkingDir);
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(repository.Path));
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AssignCategoryAsync(repository, "Development"));
                dashboard.RefreshContent();
                break;
        }

        // parity-scaffolding: Load handlers may replace plan seeds; the shared plan remains authoritative.
        foreach ((string fieldName, string text) in component.TextValues)
        {
            if (FindFieldValue(control, fieldName) is not Control target)
            {
                throw new InvalidDataException($"Text seed field '{fieldName}' was not found on {component.TypeName}.");
            }

            target.Text = text;
        }
    }

    // parity-scaffolding: Async revision loading can settle on a stash or artificial row at
    // different times in isolated workers; every paired state must start from repository HEAD.
    public static void PrepareCaptureState(Control control, IGitUICommands commands)
    {
        if (control is not RevisionGridControl revisionGrid)
        {
            return;
        }

        DataGridView grid = (DataGridView?)FindFieldValue(revisionGrid, "_gridView")
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its real DataGridView.");
        System.Reflection.PropertyInfo loadCompleteProperty = grid.GetType().GetProperty(
            "IsDataLoadComplete",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its load-complete state.");
        System.Reflection.FieldInfo refreshingField = typeof(RevisionGridControl).GetField(
            "_isRefreshingRevisions",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its refresh state.");

        if (revisionGrid.FindForm() is Form form)
        {
            form.Activate();
        }

        System.Reflection.MethodInfo getRevision = typeof(RevisionGridControl).GetMethod(
            "GetRevision",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: [typeof(int)],
            modifiers: null)
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its row lookup.");
        System.Reflection.FieldInfo latestRowField = typeof(RevisionGridControl).GetField(
            "_latestSelectedRowIndex",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its selected-row state.");
        ObjectId head = commands.Module.RevParse("HEAD");
        WaitForStableHeadSelection();

        // parity-scaffolding: Data loading and visible-row graph rendering settle independently;
        // capture only after the product's background renderer publishes its measured width.
        WaitForRevisionGridRender(revisionGrid, grid);
        WaitForStableHeadSelection();

        GitRevision? GetRevision(int index)
            => (GitRevision?)getRevision.Invoke(revisionGrid, [index]);

        void WaitForStableHeadSelection()
        {
            DateTime selectionDeadline = DateTime.UtcNow.AddSeconds(30);
            int stableObservationCount = 0;
            while (stableObservationCount < 10 && DateTime.UtcNow < selectionDeadline)
            {
                Application.DoEvents();
                bool isRefreshing = (bool)refreshingField.GetValue(revisionGrid)!;
                bool isDataLoadComplete = (bool)loadCompleteProperty.GetValue(grid)!;
                int selectedIndex = Enumerable.Range(0, grid.RowCount)
                    .FirstOrDefault(index => GetRevision(index)?.ObjectId == head, -1);
                if (selectedIndex >= 0 && !isRefreshing && isDataLoadComplete)
                {
                    if (!grid.Rows[selectedIndex].Selected || grid.CurrentCell?.RowIndex != selectedIndex)
                    {
                        grid.Focus();
                        grid.ClearSelection();
                        grid.Rows[selectedIndex].Selected = true;
                        grid.CurrentCell = grid.Rows[selectedIndex].Cells[Math.Min(1, grid.ColumnCount - 1)];
                    }

                    latestRowField.SetValue(revisionGrid, selectedIndex);
                    Application.DoEvents();
                    stableObservationCount = IsRevisionGridSelectionReady(
                        (bool)refreshingField.GetValue(revisionGrid)!,
                        (bool)loadCompleteProperty.GetValue(grid)!,
                        GetRevision(selectedIndex)?.ObjectId == head,
                        grid.Rows[selectedIndex].Selected,
                        (int)latestRowField.GetValue(revisionGrid)! == selectedIndex)
                        ? stableObservationCount + 1
                        : 0;
                }
                else
                {
                    stableObservationCount = 0;
                }

                Thread.Sleep(25);
            }

            if (stableObservationCount < 10)
            {
                throw new CaptureStateUnsupportedException(
                    "The original revision grid did not retain a stable repository HEAD selection before capture.");
            }
        }
    }

    internal static bool IsRevisionGridSelectionReady(
        bool isRefreshing,
        bool isDataLoadComplete,
        bool selectedRevisionIsHead,
        bool selectedRowIsSelected,
        bool latestRowMatches)
        => !isRefreshing
            && isDataLoadComplete
            && selectedRevisionIsHead
            && selectedRowIsSelected
            && latestRowMatches;

    // parity-scaffolding: Never accept a menu capture if the original's asynchronous grid
    // replaced HEAD after preparation or if the real opening handlers did not finish.
    public static void VerifyCaptureState(Control control, IGitUICommands commands, CaptureStatePlan state)
    {
        if (control is not RevisionGridControl revisionGrid)
        {
            return;
        }

        System.Reflection.PropertyInfo latestRevisionProperty = typeof(RevisionGridControl).GetProperty(
            "LatestSelectedRevision",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its latest selected revision.");
        GitRevision? latestRevision = (GitRevision?)latestRevisionProperty.GetValue(revisionGrid);
        if (latestRevision?.ObjectId != commands.Module.RevParse("HEAD"))
        {
            throw new CaptureStateNotReadyException("The original revision grid replaced repository HEAD before capture.");
        }

        if (state.TargetField == "mainContextMenu")
        {
            ToolStripMenuItem rebase = RequireMenuItem("rebaseOnToolStripMenuItem");
            ToolStripMenuItem applyStash = RequireMenuItem("applyStashToolStripMenuItem");
            ToolStripMenuItem popStash = RequireMenuItem("popStashToolStripMenuItem");
            ToolStripMenuItem dropStash = RequireMenuItem("dropStashToolStripMenuItem");
            ToolStripMenuItem resetChanges = RequireMenuItem("resetChangesToolStripMenuItem");
            ToolStripMenuItem commit = RequireMenuItem("commitToolStripMenuItem");
            if (!IsRevisionGridHeadContextMenuReady(
                    rebase.Visible,
                    rebase.Enabled,
                    applyStash.Visible,
                    popStash.Visible,
                    dropStash.Visible,
                    resetChanges.Visible,
                    commit.Visible))
            {
                throw new CaptureStateNotReadyException(
                    "The original revision-grid context menu did not finish applying its repository HEAD state "
                    + $"(rebase={rebase.Visible}/{rebase.Enabled}, applyStash={applyStash.Visible}, "
                    + $"popStash={popStash.Visible}, dropStash={dropStash.Visible}, "
                    + $"resetChanges={resetChanges.Visible}, commit={commit.Visible}).");
            }
        }
        else if (state.TargetField == "copyToClipboardToolStripMenuItem")
        {
            ToolStripMenuItem copy = RequireMenuItem("copyToClipboardToolStripMenuItem");
            string messageLabel = ResourceManager.TranslatedStrings.GetMessage(1);
            bool hasMessage = copy.DropDownItems.Cast<ToolStripItem>()
                .Any(item => (item.Text ?? string.Empty).Replace("&", string.Empty, StringComparison.Ordinal)
                    .StartsWith(messageLabel, StringComparison.OrdinalIgnoreCase));
            if (!hasMessage)
            {
                string itemText = string.Join(", ", copy.DropDownItems.Cast<ToolStripItem>()
                    .Select(item => item.Text ?? "<null>"));
                throw new CaptureStateNotReadyException(
                    "The original revision-grid copy menu did not finish loading the selected commit message "
                    + $"(items: {itemText}).");
            }
        }

        ToolStripMenuItem RequireMenuItem(string fieldName)
            => (ToolStripMenuItem?)FindFieldValue(revisionGrid, fieldName)
               ?? throw new CaptureStateUnsupportedException(
                   $"The original revision grid did not expose menu item '{fieldName}'.");
    }

    internal static bool IsRevisionGridHeadContextMenuReady(
        bool rebaseVisible,
        bool rebaseEnabled,
        bool applyStashVisible,
        bool popStashVisible,
        bool dropStashVisible,
        bool resetChangesVisible,
        bool commitVisible)
        => rebaseVisible
            && rebaseEnabled
            && !applyStashVisible
            && !popStashVisible
            && !dropStashVisible
            && !resetChangesVisible
            && !commitVisible;

    private static void WaitForRevisionGridRender(RevisionGridControl revisionGrid, DataGridView grid)
    {
        System.Reflection.PropertyInfo updatingVisibleRowsProperty = grid.GetType().GetProperty(
            "UpdatingVisibleRows",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its visible-row update state.");
        System.Reflection.FieldInfo graphProviderField = typeof(RevisionGridControl).GetField(
            "_revisionGraphColumnProvider",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new CaptureStateUnsupportedException("The original revision grid did not expose its graph renderer.");
        object graphProvider = graphProviderField.GetValue(revisionGrid)
            ?? throw new CaptureStateUnsupportedException("The original revision grid graph renderer was unavailable.");
        System.Reflection.FieldInfo renderedWidthField = graphProvider.GetType().GetField(
            "_columnWidth",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new CaptureStateUnsupportedException("The original revision grid graph renderer did not expose its measured width.");

        grid.Refresh();
        DateTime renderDeadline = DateTime.UtcNow.AddSeconds(30);
        int stableObservationCount = 0;
        int renderedWidth = 0;
        while (stableObservationCount < 3 && DateTime.UtcNow < renderDeadline)
        {
            Application.DoEvents();
            renderedWidth = (int)renderedWidthField.GetValue(graphProvider)!;
            bool updatingVisibleRows = (bool)updatingVisibleRowsProperty.GetValue(grid)!;
            bool rendered = IsRevisionGridRenderReady(
                grid.Columns[0].Visible,
                updatingVisibleRows,
                renderedWidth,
                grid.Columns[0].Width);
            stableObservationCount = rendered
                ? stableObservationCount + 1
                : 0;
            Thread.Sleep(25);
        }

        if (stableObservationCount < 3)
        {
            throw new CaptureStateUnsupportedException(
                $"The original revision grid did not complete visible-row graph rendering before capture "
                + $"(updating={updatingVisibleRowsProperty.GetValue(grid)}, renderedWidth={renderedWidth}, "
                + $"columnWidth={grid.Columns[0].Width}).");
        }
    }

    internal static bool IsRevisionGridRenderReady(
        bool graphVisible,
        bool updatingVisibleRows,
        int renderedWidth,
        int columnWidth)
        => !updatingVisibleRows
            && (!graphVisible || (renderedWidth > 0 && columnWidth == renderedWidth));

    // parity-scaffolding: Cancel the original grid's asynchronous refresh before WinForms disposal joins it.
    public static void CleanupBeforeDispose(Control control)
    {
        RevisionGridControl? revisionGrid = control as RevisionGridControl;
        if (revisionGrid is null
            && control is FormFormatPatch
            && FindFieldValue(control, "RevisionGrid") is RevisionGridControl nestedRevisionGrid)
        {
            revisionGrid = nestedRevisionGrid;
        }

        if (revisionGrid is not null)
        {
            InvokeNonPublic(revisionGrid, "CancelBackgroundTasks");
        }
    }

    // parity-scaffolding: Gives the original standalone list the same repository-backed groups as the twin capture host.
    private static void SeedFileStatusList(FileStatusList fileStatusList, IGitUICommands commands)
    {
        IReadOnlyList<GitItemStatus> changedFiles = commands.Module.GetAllChangedFilesWithSubmodulesStatus(
            excludeIgnoredFiles: true,
            excludeAssumeUnchangedFiles: true,
            excludeSkipWorktreeFiles: true,
            untrackedFiles: UntrackedFilesMode.Default,
            cancellationToken: default);
        int splitIndex = Math.Max(1, changedFiles.Count / 2);
        fileStatusList.GroupByRevision = true;
        fileStatusList.SetStashDiffs(
            CreateRevision(commands),
            new GitRevision(ObjectId.IndexId),
            "Working directory",
            [.. changedFiles.Skip(splitIndex)],
            new GitRevision(ObjectId.WorkTreeId),
            "Diff with parent",
            [.. changedFiles.Take(splitIndex)]);
        fileStatusList.SetFilter("src|CHANGELOG");
    }

    // parity-scaffolding: Gives both diff surfaces the same adjacent representative revisions.
    private static FormDiff CreateFormDiff(GitUICommands commands)
    {
        ObjectId head = commands.Module.RevParse("HEAD");
        ObjectId parent = commands.Module.RevParse("HEAD~1");
        return new FormDiff(commands, parent, head, "HEAD~1", "HEAD");
    }

    // parity-scaffolding: Seeds private original state without adding product-facing capture hooks.
    private static void SetNonPublicField(object target, string fieldName, object value)
    {
        System.Reflection.FieldInfo field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on {target.GetType().FullName}.");
        field.SetValue(target, value);
    }

    // parity-scaffolding: Drives an original private state transition through its own implementation.
    private static void InvokeNonPublic(object target, string methodName, params object[] arguments)
    {
        System.Reflection.MethodInfo method = target.GetType().GetMethod(
            methodName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Method '{methodName}' was not found on {target.GetType().FullName}.");
        method.Invoke(target, arguments);
    }

    // parity-scaffolding: Keeps both commit-details capture surfaces on one deterministic model.
    private static GitRevision CreateRevision(IGitUICommands commands)
    {
        IGitModule module = commands.Module;
        ObjectId objectId = module.GetCurrentCheckout();
        IReadOnlyList<IGitRef> refs = module.GetRefs(RefsFilter.NoFilter);
        long unixTime = new DateTimeOffset(2026, 7, 17, 10, 30, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        return new GitRevision(objectId)
        {
            Author = "Avalonia Contributor",
            AuthorEmail = "avalonia@example.com",
            AuthorUnixTime = unixTime,
            Committer = "Git Extensions Team",
            CommitterEmail = "team@gitextensions.org",
            CommitUnixTime = unixTime,
            Subject = "Establish the Avalonia application shell",
            Body = "Establish the Avalonia application shell\n\nRepresentative content used by the visual parity screenshot harness.",
            ParentIds = [module.RevParse("HEAD~1")],
            Refs = refs.Where(gitRef => gitRef.ObjectId == objectId).ToArray(),
        };
    }

    private static object? FindFieldValue(object owner, string fieldName)
    {
        for (Type? type = owner.GetType(); type is not null; type = type.BaseType)
        {
            System.Reflection.FieldInfo? field = type.GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.DeclaredOnly);
            if (field is not null)
            {
                return field.GetValue(owner);
            }
        }

        if (owner is Control control)
        {
            return FindNamedControl(control, fieldName);
        }

        return null;

        static Control? FindNamedControl(Control control, string fieldName)
        {
            if (control.Name == fieldName)
            {
                return control;
            }

            foreach (Control child in control.Controls)
            {
                if (FindNamedControl(child, fieldName) is Control match)
                {
                    return match;
                }
            }

            return null;
        }
    }

    private static Control CreateParameterless(string typeName)
    {
        Type type = Type.GetType($"{typeName}, GitUI", throwOnError: true)!;
        if (!typeof(Control).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{typeName} is not a Windows Forms control.");
        }

        return (Control?)Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"{typeName} could not be constructed.");
    }

    private static Control CreateWithCommands(string typeName, GitUICommands commands)
    {
        Type type = Type.GetType($"{typeName}, GitUI", throwOnError: true)!;
        return (Control?)Activator.CreateInstance(
            type,
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            args: [commands],
            culture: null)
            ?? throw new InvalidOperationException($"{typeName} could not be constructed.");
    }

    // parity-scaffolding: Adapts the capture worker's commands to GitModuleControl ownership.
    private sealed class CaptureCommandsSource(IGitUICommands commands) : IGitUICommandsSource
    {
        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged
        {
            add { }
            remove { }
        }

        public IGitUICommands UICommands { get; } = commands;
    }

    // parity-scaffolding: Supplies the ancestor contract expected by standalone GitModuleControls.
    private sealed class CaptureCommandsHost(IGitUICommands commands) : Panel, IGitUICommandsSource
    {
        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged
        {
            add { }
            remove { }
        }

        public IGitUICommands UICommands { get; } = commands;
    }

    // parity-scaffolding: Supplies deterministic revision-grid state without constructing FormBrowse.
    private sealed class CaptureRevisionGridInfo(IGitModule module) : ICheckRefs, IRevisionGridInfo
    {
        private readonly IReadOnlyList<IGitRef> _refs = module.GetRefs(RefsFilter.NoFilter);

        public ObjectId CurrentCheckout { get; } = module.GetCurrentCheckout();

        public bool Contains(ObjectId objectId) => _refs.Any(gitRef => gitRef.ObjectId == objectId);

        public GitRevision GetRevision(ObjectId objectId) => new(objectId);

        public GitRevision? GetActualRevision(ObjectId objectId) => GetRevision(objectId);

        public GitRevision GetActualRevision(GitRevision revision) => revision;

        public IReadOnlyList<GitRevision> GetSelectedRevisions() => [GetRevision(CurrentCheckout)];

        public string DescribeRevision(GitRevision revision, int maxLength = 0) => revision.ObjectId.ToString();

        public string GetCurrentBranch() => module.GetSelectedBranch(emptyIfDetached: true);
    }
}
