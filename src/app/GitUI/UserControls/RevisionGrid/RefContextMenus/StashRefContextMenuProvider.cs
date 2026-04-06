using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.RefContextMenus;

/// <summary>
///  Provides context menu items for stash refs (identified via <see cref="IGitRef.IsStash"/>).
/// </summary>
internal sealed class StashRefContextMenuProvider : Translate, IRefContextMenuProvider
{
    private readonly TranslationString _applyStash = new("&Apply stash");
    private readonly TranslationString _popStash = new("P&op stash");
    private readonly TranslationString _dropStash = new("Dr&op stash...");

    public bool Handles(IGitRef? gitRef, string? stashReflogSelector) => gitRef?.IsStash is true;

    public void Populate(ContextMenuStrip menu, IGitRef? gitRef, string? stashReflogSelector, RefContextMenuContext context)
    {
        ToolStripMenuItem apply = new(_applyStash.Text, Images.Stash);
        apply.Click += (_, _) =>
        {
            context.UICommands.StashApply(menu, context.GetLatestSelectedRevision()?.ObjectId.ToString() ?? "");
            context.PerformRefreshRevisions();
        };
        menu.Items.Add(apply);

        ToolStripMenuItem pop = new(_popStash.Text, Images.Stash);
        pop.Click += (_, _) =>
        {
            string? stashName = context.GetLatestSelectedRevision()?.ReflogSelector;
            if (!string.IsNullOrEmpty(stashName))
            {
                context.UICommands.StashPop(menu, stashName);
                context.PerformRefreshRevisions();
            }
        };
        menu.Items.Add(pop);

        ToolStripMenuItem drop = new(_dropStash.Text, Images.Stash);
        drop.Click += (_, _) => context.DropStash(drop, EventArgs.Empty);
        menu.Items.Add(drop);
    }
}
