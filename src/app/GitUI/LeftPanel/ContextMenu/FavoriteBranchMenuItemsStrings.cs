﻿using GitCommands;
using GitExtensions.Extensibility.Translations;
using ResourceManager;

namespace GitUI.LeftPanel.ContextMenu;

internal class FavoriteBranchMenuItemsStrings : Translate
{
    public FavoriteBranchMenuItemsStrings()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
    }

    public void ApplyTo(MenuItemsStrings strings)
    {
        new BranchMenuItemsStrings().ApplyTo(strings);
    }
}
