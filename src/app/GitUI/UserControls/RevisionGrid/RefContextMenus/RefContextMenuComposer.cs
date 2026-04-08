using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.RefContextMenus;

/// <summary>
///  Composes a <see cref="ContextMenuStrip"/> for a right-clicked ref label by delegating
///  to the first matching <see cref="IRefContextMenuProvider"/>.
/// </summary>
internal sealed class RefContextMenuComposer : Translate
{
    private readonly TranslationString _copyName = new("Cop&y name to clipboard");

    private readonly IReadOnlyList<IRefContextMenuProvider> _providers;

    public RefContextMenuComposer(IReadOnlyList<IRefContextMenuProvider> providers)
    {
        _providers = providers;
    }

    /// <summary>
    ///  Builds a <see cref="ContextMenuStrip"/> for the given ref or stash selector.
    ///  Returns <see langword="null"/> when no provider produced any items.
    /// </summary>
    public ContextMenuStrip? Build(IGitRef? gitRef, string? stashReflogSelector, RefContextMenuContext context)
    {
        IRefContextMenuProvider? provider = null;
        foreach (IRefContextMenuProvider p in _providers)
        {
            if (p.Handles(gitRef, stashReflogSelector))
            {
                provider = p;
                break;
            }
        }

        if (provider is null)
        {
            return null;
        }

        ContextMenuStrip menu = new();
        provider.Populate(menu, gitRef, stashReflogSelector, context);

        if (menu.Items.Count == 0)
        {
            menu.Dispose();
            return null;
        }

        string copyText = gitRef?.Name ?? stashReflogSelector ?? "";

        menu.Items.Add(new ToolStripSeparator());
        ToolStripMenuItem copy = new(_copyName.Text, Images.CopyToClipboard);
        copy.Click += (_, _) => ClipboardUtil.TrySetText(copyText);
        menu.Items.Add(copy);

        return menu;
    }
}
