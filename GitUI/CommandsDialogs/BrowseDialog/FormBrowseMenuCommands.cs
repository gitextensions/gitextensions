using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUI.Hotkey;

namespace GitUI.CommandsDialogs
{
    class FormBrowseMenuCommands : MenuCommandsBase
    {
        RevisionGrid RevisionGrid;
        FormBrowse _formBrowse;
        GitUICommands UICommands { get { return _formBrowse.UICommands; } }
        GitModule Module { get { return UICommands.Module; } }

        // must be created only once because of translation
        IEnumerable<MenuCommand> _navigateMenuCommands;

        public FormBrowseMenuCommands(FormBrowse formBrowse, RevisionGrid revisionGrid)
        {
            TranslationCategoryName = "FormBrowse";
            Translate();

            _formBrowse = formBrowse;
            RevisionGrid = revisionGrid;
        }

        public IEnumerable<MenuCommand> GetNavigateMenuCommands()
        {
            if (_navigateMenuCommands == null)
            {
                _navigateMenuCommands = CreateNavigateMenuCommands();
            }

            return _navigateMenuCommands;
        }

        private IEnumerable<MenuCommand> CreateNavigateMenuCommands()
        {
            var resultList = new List<MenuCommand>();

            // no additional MenuCommands that are not defined in the RevisionGrid

            return resultList;
        }

        protected override IEnumerable<MenuCommand> GetMenuCommandsForTranslation()
        {
            return GetNavigateMenuCommands();
        }
    }
}
