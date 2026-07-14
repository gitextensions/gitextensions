using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

// TODO(avalonia-port): reduced shell for milestones M1.0-M1.3 — revision grid, file list,
// and patch viewer. The left panel, commit info, menus, and toolbars of the real FormBrowse
// twin arrive in later milestones.
public partial class FormBrowse : Window
{
    private readonly GitModule? _module;

    public FormBrowse()
    {
        InitializeComponent();
    }

    public FormBrowse(IServiceProvider serviceProvider, GitModule module)
        : this()
    {
        _module = module;

        bool isValidWorkingDir = module.IsValidGitWorkingDir();
        string branchName = isValidWorkingDir ? module.GetSelectedBranch() : string.Empty;

        IAppTitleGenerator appTitleGenerator = serviceProvider.GetRequiredService<IAppTitleGenerator>();
        Title = appTitleGenerator.Generate(module.WorkingDir, isValidWorkingDir, branchName);

        RevisionGrid.SelectionChanged += RevisionGrid_SelectionChanged;
        fileStatusList.SelectedIndexChanged += FileStatusList_SelectedIndexChanged;

        if (isValidWorkingDir)
        {
            lblRepoPath.Text = $"{module.WorkingDir}  —  {branchName}";
            lblStatus.Text = $"git: {GitVersion.Current}";
            RevisionGrid.ReloadRevisions(module);
        }
        else
        {
            lblRepoPath.Text = "No git repository";
            lblStatus.Text = "Start the app inside a repository or pass one on the command line: GitExtensions.Avalonia browse <path>";
        }
    }

    private void RevisionGrid_SelectionChanged(object? sender, EventArgs e)
    {
        fileStatusList.Clear();
        fileViewer.ViewPatch(string.Empty);

        if (_module is not GitModule module
            || RevisionGrid.SelectedRevision is not GitRevision revision
            || !revision.HasParent)
        {
            return;
        }

        ObjectId parentId = revision.FirstParentId;
        ThreadHelper.FileAndForget(async () =>
        {
            IReadOnlyList<GitItemStatus> diffs = module.GetDiffFilesWithSubmodulesStatus(
                parentId,
                revision.ObjectId,
                parentToSecond: parentId,
                excludeSkipWorktreeFiles: false,
                UntrackedFilesMode.Default,
                CancellationToken.None);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (ReferenceEquals(RevisionGrid.SelectedRevision, revision))
            {
                fileStatusList.SetDiffs(diffs);
            }
        });
    }

    private void FileStatusList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_module is not GitModule module
            || RevisionGrid.SelectedRevision is not GitRevision revision
            || !revision.HasParent
            || fileStatusList.SelectedItem is not GitItemStatus item)
        {
            return;
        }

        ObjectId parentId = revision.FirstParentId;
        ThreadHelper.FileAndForget(async () =>
        {
            (Patch? patch, string? errorMessage) = await module.GetSingleDiffAsync(
                parentId,
                revision.ObjectId,
                item.Name,
                item.OldName,
                extraDiffArguments: "",
                module.FilesEncoding,
                cacheResult: true,
                isTracked: item.IsTracked,
                useGitColoring: false,
                GitCommandConfiguration.Default,
                CancellationToken.None);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (ReferenceEquals(fileStatusList.SelectedItem, item))
            {
                fileViewer.ViewPatch(patch?.Text ?? errorMessage);
            }
        });
    }
}
