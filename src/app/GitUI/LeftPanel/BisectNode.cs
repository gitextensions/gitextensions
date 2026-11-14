using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Bisect) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
internal sealed class BisectNode : BaseRevisionNode
{
    public BisectNode(Tree tree, IGitRef gitRef, bool visible)
        : base(tree, gitRef.CompleteName.RemovePrefix(GitRefName.RefsBisectPrefix), visible)
    {
        ObjectId = gitRef.ObjectId;
        CompleteName = gitRef.CompleteName;
        IsGood = gitRef.IsBisectGood;
        IsBad = gitRef.IsBisectBad;
        DisplayName = GetDisplayName(FullPath, gitRef.ObjectId);
    }

    public string CompleteName { get; }
    public string DisplayName { get; }
    public bool IsGood { get; }
    public bool IsBad { get; }

    /// <summary>
    ///  Shortens the commit hash in the ref name, e.g. "good-0123456789abcdef..." to "good-01234567".
    /// </summary>
    internal static string GetDisplayName(string name, ObjectId objectId)
    {
        string hash = objectId.ToString();
        return name.EndsWith($"-{hash}", StringComparison.Ordinal)
            ? $"{name[..^hash.Length]}{objectId.ToShortString()}"
            : name;
    }

    internal override void OnSelected()
    {
        if (Tree.IgnoreSelectionChangedEvent)
        {
            return;
        }

        base.OnSelected();
        SelectRevision();
    }

    internal override void OnDoubleClick()
    {
        SelectRevision();
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();

        TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
            !Visible ? nameof(Images.EyeClosed)
            : IsBad ? nameof(Images.BisectBad)
            : IsGood ? nameof(Images.BisectGood)
            : nameof(Images.BisectSkip);
        TreeViewNode.ToolTipText = CompleteName;
    }

    protected override string DisplayText()
    {
        return DisplayName;
    }
}
