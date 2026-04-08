using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.RefContextMenus;

/// <summary>
///  Provides context menu items for remote branch refs.
/// </summary>
internal sealed class RemoteBranchContextMenuProvider : Translate, IRefContextMenuProvider
{
    private readonly TranslationString _checkoutBranch = new("&Checkout this branch");
    private readonly TranslationString _mergeIntoCurrent = new("&Merge into current branch");
    private readonly TranslationString _rebaseOnto = new("&Rebase current branch onto this");
    private readonly TranslationString _deleteBranch = new("&Delete this branch");

    public bool Handles(IGitRef? gitRef, string? stashReflogSelector) => gitRef?.IsRemote is true;

    public void Populate(ContextMenuStrip menu, IGitRef? gitRef, string? stashReflogSelector, RefContextMenuContext context)
    {
        if (gitRef is null)
        {
            return;
        }

        bool isAtCurrentHead = gitRef.ObjectId == context.CurrentCheckout;

        if (!context.IsBareRepository)
        {
            ToolStripMenuItem checkout = new(_checkoutBranch.Text, Images.BranchCheckout);
            checkout.Click += (_, _) => context.UICommands.StartCheckoutRemoteBranch(context.ParentForm, gitRef.Name);
            menu.Items.Add(checkout);

            if (!isAtCurrentHead)
            {
                string refUnambiguousName = context.GetRefUnambiguousName(gitRef);
                ToolStripMenuItem merge = new(_mergeIntoCurrent.Text, Images.Merge);
                merge.Click += (_, _) => context.UICommands.StartMergeBranchDialog(context.ParentForm, refUnambiguousName);
                menu.Items.Add(merge);

                ToolStripMenuItem rebase = new(_rebaseOnto.Text, Images.Rebase);
                rebase.Click += (_, _) => context.UICommands.StartRebase(context.ParentForm, refUnambiguousName);
                menu.Items.Add(rebase);
            }

            menu.Items.Add(new ToolStripSeparator());
        }

        ToolStripMenuItem delete = new(_deleteBranch.Text, Images.BranchDelete);
        delete.Click += (_, _) => context.UICommands.StartDeleteRemoteBranchDialog(context.ParentForm, gitRef.Name);
        menu.Items.Add(delete);
    }
}
