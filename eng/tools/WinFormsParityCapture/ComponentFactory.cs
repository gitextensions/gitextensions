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

    // parity-scaffolding: Cancel the original grid's asynchronous refresh before WinForms disposal joins it.
    public static void CleanupBeforeDispose(Control control)
    {
        if (control is FormFormatPatch
            && FindFieldValue(control, "RevisionGrid") is RevisionGridControl revisionGrid)
        {
            InvokeNonPublic(revisionGrid, "CancelBackgroundTasks");
        }
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
