using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

// Node representing a submodule
internal sealed class SubmoduleNode : Node
{
    public SubmoduleInfo Info { get; set; }
    public bool IsCurrent { get; }
    public IReadOnlyList<GitItemStatus>? GitStatus { get; }
    public string LocalPath { get; }
    public string SuperPath { get; }
    public string SubmoduleName { get; }
    public string BranchText { get; }

    public SubmoduleNode(
        Tree tree,
        NodeBase parent,
        SubmoduleInfo submoduleInfo,
        bool isCurrent,
        IReadOnlyList<GitItemStatus>? gitStatus,
        string localPath,
        string superPath)
        : base(tree, parent, GetSubmoduleName(submoduleInfo), GetSubmoduleItemImage(submoduleInfo.Detailed), isCurrent)
    {
        Info = submoduleInfo;
        IsCurrent = isCurrent;
        GitStatus = gitStatus;
        LocalPath = localPath;
        SuperPath = superPath;

        // Extract submodule name and branch
        // e.g. Info.Text = "Externals/conemu-inside [no branch]"
        // Note that the branch portion won't be there if the user hasn't yet init'd + updated the submodule.
        string[] pathAndBranch = Info.Text.Split(Delimiters.Space, 2);
        Trace.Assert(pathAndBranch.Length >= 1);
        SubmoduleName = pathAndBranch[0].SubstringAfterLast('/').SubstringAfterLast('\\'); // Remove path
        BranchText = pathAndBranch.Length == 2 ? " " + pathAndBranch[1] : string.Empty;
        RefreshDetails();
    }

    public override string SearchText => Info.Text;

    public void RefreshDetails()
        => ApplyStatus();

    protected override string DisplayText()
        => SubmoduleName + BranchText + Info.Detailed?.AddedAndRemovedText;

    protected override string NodeName()
        => SubmoduleName;

    public override void ApplyStyle()
    {
        base.ApplyStyle();
        ApplyStatus(); // Note that status is applied also after the tree is created, when status is applied
    }

    protected override WinFormsShims.FontStyle GetFontStyle()
        => base.GetFontStyle() | (IsCurrent ? WinFormsShims.FontStyle.Bold : WinFormsShims.FontStyle.Regular);

    private void ApplyStatus()
    {
        SetHeader(DisplayText(), GetSubmoduleItemImage(Info.Detailed), IsCurrent);
        ToolTip.SetTip(TreeViewNode, DisplayText());
    }

    internal async Task SetStatusToolTipAsync(CancellationToken token)
    {
        string toolTip = await Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            if (Info.Detailed?.RawStatus is not null)
            {
                return SubmoduleResources.GetSubmoduleStatusText(
                    new GitModule(UICommands.GetRequiredService<IGitExecutorProvider>(), Info.Path),
                    Info.Detailed.RawStatus,
                    moduleIsParent: false,
                    limitOutput: true);
            }

            if (GitStatus is not null)
            {
                ArtificialCommitChangeCount changeCount = new();
                changeCount.Update(GitStatus);
                return changeCount.GetSummary();
            }

            return SubmoduleResources.GetSubmoduleText(
                new GitModule(UICommands.GetRequiredService<IGitExecutorProvider>(), "."),
                Info.Path,
                hash: string.Empty);
        }, token);

        await Dispatcher.UIThread.InvokeAsync(() => ToolTip.SetTip(TreeViewNode, toolTip));
    }

    public void Open()
    {
        if (!Directory.Exists(Info.Path))
        {
            MessageBoxes.SubmoduleDirectoryDoesNotExist(owner: null, Info.Path, Info.Text);
            return;
        }

        if (Info.Detailed?.RawStatus is not null)
        {
            Tree.OwnerControl.OpenRepository(Info.Path, ObjectId.WorkTreeId, Info.Detailed.RawStatus.OldCommit);
            return;
        }

        Tree.OwnerControl.OpenRepository(Info.Path);
    }

    public void LaunchGitExtensions()
    {
        if (!Directory.Exists(Info.Path))
        {
            MessageBoxes.SubmoduleDirectoryDoesNotExist(owner: null, Info.Path, Info.Text);
            return;
        }

        ObjectId selected;
        ObjectId first;
        if (IsCurrent)
        {
            // Get the current (most likely) selections from the grid
            IReadOnlyList<GitRevision> revisions = UICommands.BrowseRepo?.GetSelectedRevisions() ?? [];
            selected = revisions.Count > 0 ? revisions[0].ObjectId : default;
            first = revisions.Count > 1 ? revisions[^1].ObjectId : default;
        }
        else
        {
            // Try select a "diff" from the expected commit to worktree for a submodule
            selected = ObjectId.WorkTreeId;
            first = Info.Detailed?.RawStatus?.OldCommit ?? default;
        }

        GitUICommands.LaunchBrowse(workingDir: Info.Path.EnsureTrailingPathSeparator(), selected, first);
    }

    internal override void OnSelected()
    {
        if (Tree.IgnoreSelectionChangedEvent)
        {
            return;
        }

        base.OnSelected();
    }

    internal override void OnDoubleClick()
    {
        if (IsCurrent)
        {
            // For the current module the module is already open, so launch a new instance
            LaunchGitExtensions();
        }
        else
        {
            Open();
        }
    }

    private static string GetSubmoduleName(SubmoduleInfo info)
    {
        string path = info.Text.Split(Delimiters.Space, 2)[0];
        return path.SubstringAfterLast('/').SubstringAfterLast('\\');
    }

    private static IImage GetSubmoduleItemImage(DetailedSubmoduleInfo? details)
    {
        return (details?.Status, details?.IsDirty) switch
        {
            (SubmoduleStatus.FastForward, true) => Images.SubmoduleRevisionUpDirty,
            (SubmoduleStatus.FastForward, false) => Images.SubmoduleRevisionUp,
            (SubmoduleStatus.Rewind, true) => Images.SubmoduleRevisionDownDirty,
            (SubmoduleStatus.Rewind, false) => Images.SubmoduleRevisionDown,
            (SubmoduleStatus.NewerTime, true) => Images.SubmoduleRevisionSemiUpDirty,
            (SubmoduleStatus.NewerTime, false) => Images.SubmoduleRevisionSemiUp,
            (SubmoduleStatus.OlderTime, true) => Images.SubmoduleRevisionSemiDownDirty,
            (SubmoduleStatus.OlderTime, false) => Images.SubmoduleRevisionSemiDown,
            (_, true) => Images.SubmoduleDirty,
            (_, false) => Images.FileStatusModified,
            _ => Images.FolderSubmodule,
        };
    }
}
