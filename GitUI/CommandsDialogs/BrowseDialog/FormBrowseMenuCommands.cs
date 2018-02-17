﻿using System.Collections.Generic;
using System.Linq;
using GitUI.CommandsDialogs.BrowseDialog;

namespace GitUI.CommandsDialogs
{
    internal class FormBrowseMenuCommands : MenuCommandsBase
    {
        private readonly FormBrowse _formBrowse;
        private GitUICommands UICommands => _formBrowse.UICommands;

        // must be created only once because of translation
        private IEnumerable<MenuCommand> _navigateMenuCommands;

        public FormBrowseMenuCommands(FormBrowse formBrowse)
        {
            TranslationCategoryName = "FormBrowse";
            Translate();

            _formBrowse = formBrowse;
        }

        public IEnumerable<MenuCommand> GetNavigateMenuCommands()
        {
            if (_navigateMenuCommands == null)
            {
                _navigateMenuCommands = CreateNavigateMenuCommands();
            }

            return _navigateMenuCommands;
        }

        private static IEnumerable<MenuCommand> CreateNavigateMenuCommands()
        {
            // no additional MenuCommands that are not defined in the RevisionGrid
            return Enumerable.Empty<MenuCommand>();
        }

        protected override IEnumerable<MenuCommand> GetMenuCommandsForTranslation()
        {
            return GetNavigateMenuCommands();
        }
    }
}
