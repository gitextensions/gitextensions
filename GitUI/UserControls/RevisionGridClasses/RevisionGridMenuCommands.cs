using GitUI.CommandsDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUI.Hotkey;
using System.Windows.Forms;
using System.ComponentModel;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager.Translation;
using System.Diagnostics;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal class RevisionGridMenuCommands : MenuCommandsBase, INotifyPropertyChanged
    {
        private readonly TranslationString _quickSearchQuickHelp =
            new TranslationString("Start typing in revision grid to start quick search.");

        RevisionGrid _revisionGrid;

        // must both be created only once
        IEnumerable<MenuCommand> _navigateMenuCommands;
        IEnumerable<MenuCommand> _viewMenuCommands;

        public RevisionGridMenuCommands(RevisionGrid revisionGrid)
        {
            _revisionGrid = revisionGrid;
            CreateOrUpdateMenuCommands(); // for translation
            TranslationCategoryName = "RevisionGrid";
            Translate();
        }

        #region BusinessLogic

        bool _showRemoteBranches = true;
        public bool ShowRemoteBranches
        {
            get { return _showRemoteBranches; }
            set { _showRemoteBranches = value; _revisionGrid.InvalidateRevisions(); OnPropertyChanged(); }
        }

        bool _showSuperprojectTags = true;
        public bool ShowSuperprojectTags
        {
            get { return _showSuperprojectTags; }
            set
            {
                _showSuperprojectTags = value;
                if (value)
                    _revisionGrid.ForceRefreshRevisions();
                else
                    _revisionGrid.InvalidateRevisions();

                OnPropertyChanged();
            }
        }

        bool _showSuperprojectBranches = true;
        public bool ShowSuperprojectBranches
        {
            get { return _showSuperprojectBranches; }
            set
            {
                _showSuperprojectBranches = value;
                if (value)
                    _revisionGrid.ForceRefreshRevisions();
                else
                    _revisionGrid.InvalidateRevisions();
                OnPropertyChanged();
            }
        }

        bool _showSuperprojectRemoteBranches = true;
        public bool ShowSuperprojectRemoteBranches
        {
            get { return _showSuperprojectRemoteBranches; }
            set
            {
                _showSuperprojectRemoteBranches = value;
                if (value)
                    _revisionGrid.ForceRefreshRevisions();
                else
                    _revisionGrid.InvalidateRevisions();

                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// ... "Update" because the hotkey settings might change
        /// </summary>
        public void CreateOrUpdateMenuCommands()
        {
            if (_navigateMenuCommands == null && _viewMenuCommands == null)
            {
                _navigateMenuCommands = CreateNavigateMenuCommands();
                _viewMenuCommands = CreateViewMenuCommands();
            }

            if (_navigateMenuCommands != null && _viewMenuCommands != null)
            {
                var navigateMenuCommands2 = CreateNavigateMenuCommands();
                var viewMenuCommands2 = CreateViewMenuCommands();

                UpdateMenuCommandShortcutKeyDisplayString(_navigateMenuCommands, navigateMenuCommands2);
                UpdateMenuCommandShortcutKeyDisplayString(_viewMenuCommands, viewMenuCommands2);

                TriggerPropertyChanged(); // trigger refresh
            }
        }

        public void TriggerPropertyChanged()
        {
            Debug.WriteLine("RevisionGridMenuCommands.TriggerPropertyChanged()");
            if (PropertyChanged != null)
            {
                PropertyChanged(null, null);
            }
        }

        private void UpdateMenuCommandShortcutKeyDisplayString(IEnumerable<MenuCommand> targetList, IEnumerable<MenuCommand> sourceList)
        {
            foreach (var sourceMc in sourceList.Where(mc => !mc.IsSeparator))
            {
                var targetMc = targetList.Single(mc => !mc.IsSeparator && mc.Name == sourceMc.Name);
                targetMc.ShortcutKeyDisplayString = sourceMc.ShortcutKeyDisplayString;
            }
        }

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
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(GitUI.RevisionGrid.Commands.GoToChild);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.GoToChild);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoParentCommit";
                menuCommand.Text = "Go to parent commit";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(GitUI.RevisionGrid.Commands.GoToParent);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.GoToParent);

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "QuickSearch";
                menuCommand.Text = "Quick search";
                menuCommand.ExecuteAction = () => MessageBox.Show(_quickSearchQuickHelp.Text);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "PrevQuickSearch";
                menuCommand.Text = "Quick search previous";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(GitUI.RevisionGrid.Commands.PrevQuickSearch);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.PrevQuickSearch);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "NextQuickSearch";
                menuCommand.Text = "Quick search next";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(GitUI.RevisionGrid.Commands.NextQuickSearch);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.NextQuickSearch);

                resultList.Add(menuCommand);
            }

            return resultList;
        }

        /// <summary>
        /// this is needed because _revsionGrid is null when TranslationApp is called
        /// </summary>
        /// <param name="revGridCommands"></param>
        /// <returns></returns>
        private string GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(GitUI.RevisionGrid.Commands revGridCommands)
        {
            if (_revisionGrid == null)
            {
                return null;
            }

            return _revisionGrid.GetShortcutKeys(revGridCommands).ToShortcutKeyDisplayString();
        }

        private IEnumerable<MenuCommand> CreateViewMenuCommands()
        {
            var resultList = new List<MenuCommand>();

            // the next three MenuCommands just reuse (the currently rather
            //  convoluted) logic from RevisionGrid.
            //  After refactoring the three items should be added to RevisionGrid
            //  as done with "ShowRemoteBranches" and not via RevisionGrid.Designer.cs
            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowAllBranches";
                menuCommand.Text = "Show all branches";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowAllBranchesToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => _revisionGrid.ShowAllBranchesToolStripMenuItemChecked;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowCurrentBranchOnly";
                menuCommand.Text = "Show current branch only";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowCurrentBranchOnlyToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => _revisionGrid.ShowCurrentBranchOnlyToolStripMenuItemChecked;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowFilteredBranches";
                menuCommand.Text = "Show filtered branches";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowFilteredBranchesToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => _revisionGrid.ShowFilteredBranchesToolStripMenuItemChecked;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowRemoteBranches";
                menuCommand.Text = "Show remote branches";
                menuCommand.ExecuteAction = () => ShowRemoteBranches = !ShowRemoteBranches;
                menuCommand.IsCheckedFunc = () => ShowRemoteBranches;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowSuperprojectTags";
                menuCommand.Text = "Show superproject tags";
                menuCommand.ExecuteAction = () => ShowSuperprojectTags = !ShowSuperprojectTags;
                menuCommand.IsCheckedFunc = () => ShowSuperprojectTags;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowSuperprojectBranches";
                menuCommand.Text = "Show superproject branches";
                menuCommand.ExecuteAction = () => ShowSuperprojectBranches = !ShowSuperprojectBranches;
                menuCommand.IsCheckedFunc = () => ShowSuperprojectBranches;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowSuperprojectRemoteBranches";
                menuCommand.Text = "Show superproject remote branches";
                menuCommand.ExecuteAction = () => ShowSuperprojectRemoteBranches = !ShowSuperprojectRemoteBranches;
                menuCommand.IsCheckedFunc = () => ShowSuperprojectRemoteBranches;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ToggleHighlightSelectedBranch";
                menuCommand.Text = "Highlight selected branch (until refresh)";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(GitUI.RevisionGrid.Commands.ToggleHighlightSelectedBranch);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(GitUI.RevisionGrid.Commands.ToggleHighlightSelectedBranch);

                resultList.Add(menuCommand);
            }

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

        protected override IEnumerable<MenuCommand> GetMenuCommandsForTranslation()
        {
            return _navigateMenuCommands.Concat(_viewMenuCommands).Where(mc => !mc.IsSeparator);
        }
    }
}
