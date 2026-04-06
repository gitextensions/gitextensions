using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.RefContextMenus;

/// <summary>
///  Provides context menu items for tag refs.
/// </summary>
internal sealed class TagContextMenuProvider : Translate, IRefContextMenuProvider
{
    private readonly TranslationString _mergeIntoCurrent = new("&Merge into current branch");
    private readonly TranslationString _deleteTag = new("&Delete this tag");

    public bool Handles(IGitRef? gitRef, string? stashReflogSelector) => gitRef?.IsTag is true;

    public void Populate(ContextMenuStrip menu, IGitRef? gitRef, string? stashReflogSelector, RefContextMenuContext context)
    {
        if (gitRef is null)
        {
            return;
        }

        bool isAtCurrentHead = gitRef.ObjectId == context.CurrentCheckout;

        if (!context.IsBareRepository && !isAtCurrentHead)
        {
            string refUnambiguousName = context.GetRefUnambiguousName(gitRef);
            ToolStripMenuItem merge = new(_mergeIntoCurrent.Text, Images.Merge);
            merge.Click += (_, _) => context.UICommands.StartMergeBranchDialog(context.ParentForm, refUnambiguousName);
            menu.Items.Add(merge);

            menu.Items.Add(new ToolStripSeparator());
        }

        ToolStripMenuItem delete = new(_deleteTag.Text, Images.TagDelete);
        delete.Click += (_, _) => context.UICommands.StartDeleteTagDialog(context.ParentForm, gitRef.Name);
        menu.Items.Add(delete);
    }
}
