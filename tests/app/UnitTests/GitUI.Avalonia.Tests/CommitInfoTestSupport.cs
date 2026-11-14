using System.Reflection;
using Avalonia.Controls;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;

namespace GitExtensionsTests;

// parity-scaffolding: inspect native fields without extending the original CommitInfo accessor.
internal static class CommitInfoTestSupport
{
    public static Accessor GetNativeTestSupport(this CommitInfo control) => new(control);

    internal readonly struct Accessor(CommitInfo control)
    {
        public AvatarControl Avatar => Header.GetTestAccessor().Avatar;

        public XhtmlTextBlock CommitMessage => control.GetTestAccessor().CommitMessage;

        public XhtmlTextBlock RevisionInfo => control.GetTestAccessor().RevisionInfo;

        public CommitInfoHeader Header => control.FindControl<CommitInfoHeader>("commitInfoHeader")!;

        public MenuItem AddNoteMenuItem => control.FindControl<MenuItem>("addNoteToolStripMenuItem")!;

        public ContextMenu ContextMenu => Get<ContextMenu>("commitInfoContextMenuStrip");

        public MenuItem CopyLinkMenuItem => control.FindControl<MenuItem>("copyLinkToolStripMenuItem")!;

        public MenuItem ShowLocalBranchesMenuItem => control.FindControl<MenuItem>("showContainedInBranchesToolStripMenuItem")!;

        public MenuItem ShowRemoteBranchesMenuItem => control.FindControl<MenuItem>("showContainedInBranchesRemoteToolStripMenuItem")!;

        public MenuItem ShowRemoteBranchesIfNoLocalMenuItem => control.FindControl<MenuItem>("showContainedInBranchesRemoteIfNoLocalToolStripMenuItem")!;

        public MenuItem ShowTagsMenuItem => control.FindControl<MenuItem>("showContainedInTagsToolStripMenuItem")!;

        public MenuItem ShowAnnotatedTagMessagesMenuItem => control.FindControl<MenuItem>("showMessagesOfAnnotatedTagsToolStripMenuItem")!;

        public MenuItem ShowDerivedTagMenuItem => control.FindControl<MenuItem>("showTagThisCommitDerivesFromMenuItem")!;

        public Grid TableLayout => control.FindControl<Grid>("tableLayout")!;

        public Border CommitMessagePanel => control.FindControl<Border>("pnlCommitMessage")!;

        public int CommitMessageHeight => Get<int>("_commitMessageHeight");

        public int RevisionInfoHeight => Get<int>("_revisionInfoHeight");

        public IDictionary<string, int> GetSortedTags() => control.GetTestAccessor().GetSortedTags();

        private T Get<T>(string name) => (T)typeof(CommitInfo)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;
    }
}
