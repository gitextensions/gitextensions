using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.RefContextMenus;

/// <summary>
///  Provides context menu items for stash entries identified by a reflog selector string
///  (e.g. right-clicking a stash label that is not an <see cref="IGitRef"/>).
/// </summary>
internal sealed class StashSelectorContextMenuProvider : Translate, IRefContextMenuProvider
{
    private readonly TranslationString _applyStash = new("Appl&y stash");
    private readonly TranslationString _popStash = new("Pop &stash");
    private readonly TranslationString _dropStash = new("&Drop stash...");

    public bool Handles(IGitRef? gitRef, string? stashReflogSelector) => gitRef is null && stashReflogSelector is not null;

    public void Populate(ContextMenuStrip menu, IGitRef? gitRef, string? stashReflogSelector, RefContextMenuContext context)
    {
        GitRevision? revision = context.GetLatestSelectedRevision();
        if (revision is null || (!revision.IsStash && !revision.IsAutostash))
        {
            return;
        }

        ToolStripMenuItem apply = new(_applyStash.Text, Images.Stash);
        apply.Click += (_, _) =>
        {
            context.UICommands.StashApply(menu, revision.ObjectId.ToString());
            context.PerformRefreshRevisions();
        };
        menu.Items.Add(apply);

        if (revision.IsStash)
        {
            ToolStripMenuItem pop = new(_popStash.Text, Images.Stash);
            pop.Click += (_, _) =>
            {
                context.UICommands.StashPop(menu, stashReflogSelector!);
                context.PerformRefreshRevisions();
            };
            menu.Items.Add(pop);

            ToolStripMenuItem drop = new(_dropStash.Text, Images.Stash);
            drop.Click += (_, _) => context.DropStash(drop, EventArgs.Empty);
            menu.Items.Add(drop);
        }
    }
}
