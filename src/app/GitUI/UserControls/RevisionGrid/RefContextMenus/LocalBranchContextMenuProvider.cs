using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.RefContextMenus;

/// <summary>
///  Provides context menu items for local branch (head) refs.
/// </summary>
internal sealed class LocalBranchContextMenuProvider : Translate, IRefContextMenuProvider
{
    private readonly TranslationString _checkoutBranch = new("Chec&kout this branch");
    private readonly TranslationString _mergeIntoCurrent = new("&Merge into current branch");
    private readonly TranslationString _rebaseOnto = new("&Rebase current branch onto this");
    private readonly TranslationString _renameBranch = new("R&ename this branch");
    private readonly TranslationString _deleteBranch = new("&Delete this branch");
    private readonly TranslationString _pushBranch = new("Pus&h this branch");

    public bool Handles(IGitRef? gitRef, string? stashReflogSelector) => gitRef?.IsHead is true;

    public void Populate(ContextMenuStrip menu, IGitRef? gitRef, string? stashReflogSelector, RefContextMenuContext context)
    {
        if (gitRef is null)
        {
            return;
        }

        bool isCurrentBranch = gitRef.CompleteName == context.CurrentBranchRef;
        bool isAtCurrentHead = gitRef.ObjectId == context.CurrentCheckout;

        if (!context.IsBareRepository && !isCurrentBranch)
        {
            ToolStripMenuItem checkout = new(_checkoutBranch.Text, Images.BranchCheckout);
            checkout.Click += (_, _) => context.UICommands.StartCheckoutBranch(context.ParentForm, gitRef.Name);
            menu.Items.Add(checkout);
        }

        if (!context.IsBareRepository && !isAtCurrentHead)
        {
            string refUnambiguousName = context.GetRefUnambiguousName(gitRef);
            ToolStripMenuItem merge = new(_mergeIntoCurrent.Text, Images.Merge);
            merge.Click += (_, _) => context.UICommands.StartMergeBranchDialog(context.ParentForm, refUnambiguousName);
            menu.Items.Add(merge);

            ToolStripMenuItem rebase = new(_rebaseOnto.Text, Images.Rebase);
            rebase.Click += (_, _) => context.UICommands.StartRebase(context.ParentForm, refUnambiguousName);
            menu.Items.Add(rebase);
        }

        if (menu.Items.Count > 0)
        {
            menu.Items.Add(new ToolStripSeparator());
        }

        ToolStripMenuItem rename = new(_renameBranch.Text, Images.EditFile);
        rename.Click += (_, _) => context.UICommands.StartRenameDialog(context.ParentForm, gitRef.Name);
        menu.Items.Add(rename);

        if (!isCurrentBranch)
        {
            ToolStripMenuItem delete = new(_deleteBranch.Text, Images.BranchDelete);
            delete.Click += (_, _) => context.UICommands.StartDeleteBranchDialog(context.ParentForm, gitRef.Name);
            menu.Items.Add(delete);
        }

        if (!context.IsBareRepository)
        {
            ToolStripMenuItem push = new(_pushBranch.Text, Images.Push);
            push.Click += (_, _) => context.UICommands.StartPushDialog(context.ParentForm, pushOnShow: false, forceWithLease: false, out _, gitRef.Name);
            menu.Items.Add(push);
        }
    }
}
