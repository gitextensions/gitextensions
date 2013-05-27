using GitUI.CommandsDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUI.Hotkey;
using System.Windows.Forms;
using System.ComponentModel;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal class RevisionGridMenuCommands : INotifyPropertyChanged
    {
        RevisionGrid _revisionGrid;

        // must both be created only once
        IEnumerable<MenuCommand> _navigateMenuCommands;
        IEnumerable<MenuCommand> _viewMenuCommands;

        public RevisionGridMenuCommands(RevisionGrid revisionGrid)
        {
            _revisionGrid = revisionGrid;
            _navigateMenuCommands = CreateNavigateMenuCommands();
            _viewMenuCommands = CreateViewMenuCommands();
        }

        #region UserProperties

        bool _showRemoteBranches = true;

        public bool ShowRemoteBranches { 
            get { return _showRemoteBranches; }
            set { _showRemoteBranches = value; _revisionGrid.InvalidateRevisions(); OnPropertyChanged(); } 
        }

        #endregion

        public IEnumerable<MenuCommand> GetNavigateMenuCommands()
        {
            return _navigateMenuCommands;
        }

        private IEnumerable<MenuCommand> CreateNavigateMenuCommands()
        {
            var resultList = new List<MenuCommand>();

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoChildCommit";
                menuCommand.Text = "Go to child commit";
                menuCommand.ShortcutKeyDisplayString = _revisionGrid.GetShortcutKeys(GitUI.RevisionGrid.Commands.GoToChild).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.GoToChild);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoParentCommit";
                menuCommand.Text = "Go to parent commit";
                menuCommand.ShortcutKeyDisplayString = _revisionGrid.GetShortcutKeys(GitUI.RevisionGrid.Commands.GoToParent).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.GoToParent);

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "QuickSearch";
                menuCommand.Text = "Quick search";
                menuCommand.ExecuteAction = () => MessageBox.Show("Start typing in revision grid to start quick search.");

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "PrevQuickSearch";
                menuCommand.Text = "Quick search previous";
                menuCommand.ShortcutKeyDisplayString = _revisionGrid.GetShortcutKeys(GitUI.RevisionGrid.Commands.PrevQuickSearch).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.PrevQuickSearch);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "NextQuickSearch";
                menuCommand.Text = "Quick search next";
                menuCommand.ShortcutKeyDisplayString = _revisionGrid.GetShortcutKeys(GitUI.RevisionGrid.Commands.NextQuickSearch).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.NextQuickSearch);

                resultList.Add(menuCommand);
            }

            return resultList;
        }

        private IEnumerable<MenuCommand> CreateViewMenuCommands()
        {
            var resultList = new List<MenuCommand>();

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowRemoteBranches";
                menuCommand.Text = "Show remote branches";
                menuCommand.ExecuteAction = () => ShowRemoteBranches = !ShowRemoteBranches;
                menuCommand.IsCheckedFunc = () => ShowRemoteBranches;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            return resultList;
        }

        public IEnumerable<MenuCommand> GetViewMenuCommands()
        {
            return _viewMenuCommands;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // taken from http://stackoverflow.com/questions/5058254/inotifypropertychanged-propertychangedeventhandler-event-is-always-null
        // paramenter name not used
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
