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
        private readonly TranslationString _noRevisionFoundError =
            new TranslationString("No revision found.");

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

        public void SelectCurrentRevisionExecute()
        {
            _formBrowse.ExecuteCommand(GitUI.CommandsDialogs.FormBrowse.Commands.SelectCurrentRevision);
        }

        public void GotoCommitExcecute()
        {
            using (FormGoToCommit formGoToCommit = new FormGoToCommit(UICommands))
            {
                if (formGoToCommit.ShowDialog(_formBrowse) != DialogResult.OK)
                    return;

                string revisionGuid = formGoToCommit.ValidateAndGetSelectedRevision();
                if (!string.IsNullOrEmpty(revisionGuid))
                {
                    RevisionGrid.SetSelectedRevision(new GitRevision(Module, revisionGuid));
                }
                else
                {
                    MessageBox.Show(_formBrowse, _noRevisionFoundError.Text);
                }
            }
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

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoCurrentRevision";
                menuCommand.Text = "Go to current revision";
                menuCommand.Image = global::GitUI.Properties.Resources.IconGotoCurrentRevision;
                if (_formBrowse != null) // null when TranslationApp is called
                {
                    menuCommand.ShortcutKeyDisplayString = _formBrowse.GetShortcutKeys(GitUI.CommandsDialogs.FormBrowse.Commands.SelectCurrentRevision).ToShortcutKeyDisplayString();
                }
                menuCommand.ExecuteAction = SelectCurrentRevisionExecute;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoCommit";
                menuCommand.Text = "Go to commit...";
                menuCommand.Image = global::GitUI.Properties.Resources.IconGotoCommit;
                menuCommand.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.G)));
                menuCommand.ExecuteAction = GotoCommitExcecute;

                resultList.Add(menuCommand);
            }

            return resultList;
        }

        protected override IEnumerable<MenuCommand> GetMenuCommandsForTranslation()
        {
            return GetNavigateMenuCommands();
        }
    }
}
