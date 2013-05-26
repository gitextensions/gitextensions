using GitUI.CommandsDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.UserControls
{
    internal class RevisionGridMenuCommands
    {
        RevisionGrid _revisionGrid;

        public RevisionGridMenuCommands(RevisionGrid revisionGrid)
        {
            _revisionGrid = revisionGrid;
        }

        public IEnumerable<MenuCommand> GetNavigateMenuCommands()
        {
            var resultList = new List<MenuCommand>();

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoChildCommit";
                menuCommand.Text = "Go to child commit";
                //// menuCommand.Image = global::GitUI.Properties.Resources.IconGotoCommit;
                //// TODO menuCommand.ShortcutKeyDisplayString = _formBrowse.GetShortcutKeys(GitUI.CommandsDialogs.FormBrowse.Commands.SelectCurrentRevision).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.GoToChild);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoParentCommit";
                menuCommand.Text = "Go to parent commit";
                //// menuCommand.Image = global::GitUI.Properties.Resources.IconGotoCommit;
                //// TODO menuCommand.ShortcutKeyDisplayString = _formBrowse.GetShortcutKeys(GitUI.CommandsDialogs.FormBrowse.Commands.SelectCurrentRevision).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.GoToParent);

                resultList.Add(menuCommand);
            }

            return resultList;
        }
    }
}
