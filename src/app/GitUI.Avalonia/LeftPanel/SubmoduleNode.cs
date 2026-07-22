using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel;

// Node representing a submodule
internal sealed class SubmoduleNode : Node
{
    public SubmoduleInfo Info { get; }
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

        string[] pathAndBranch = Info.Text.Split(Delimiters.Space, 2);
        Trace.Assert(pathAndBranch.Length >= 1);
        SubmoduleName = pathAndBranch[0].SubstringAfterLast('/').SubstringAfterLast('\\');
        BranchText = pathAndBranch.Length == 2 ? " " + pathAndBranch[1] : string.Empty;
        RefreshDetails();
    }

    public override string SearchText => Info.Text;

    public void RefreshDetails()
    {
        string displayText = SubmoduleName + BranchText + Info.Detailed?.AddedAndRemovedText;
        SetHeader(displayText, GetSubmoduleItemImage(Info.Detailed), IsCurrent);
        ToolTip.SetTip(TreeViewNode, displayText);
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

        GitUICommands.LaunchBrowse(Info.Path.EnsureTrailingPathSeparator());
    }

    internal override void OnDoubleClick()
    {
        if (IsCurrent)
        {
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
