﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Hotkey;
using GitUI.UserControls.RevisionGrid.Layouts;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    internal class RevisionGridMenuCommands : MenuCommandsBase
    {
        private readonly TranslationString _quickSearchQuickHelp =
            new TranslationString("Start typing in revision grid to start quick search.");

        private readonly TranslationString _noRevisionFoundError =
            new TranslationString("No revision found.");

        RevisionGridControl _revisionGrid;
        private readonly ILayoutRendererFactory _layoutRendererFactory;

        // must both be created only once
        IEnumerable<MenuCommand> _navigateMenuCommands;
        IEnumerable<MenuCommand> _viewMenuCommands;

        public RevisionGridMenuCommands(RevisionGridControl revisionGrid, ILayoutRendererFactory layoutRendererFactory)
        {
            _revisionGrid = revisionGrid;
            _layoutRendererFactory = layoutRendererFactory;
            CreateOrUpdateMenuCommands(); // for translation
            TranslationCategoryName = "RevisionGrid";
            Translate();
        }

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

                if (_revisionGrid != null) // null when TranslationApp is started
                {
                    TriggerMenuChanged(); // trigger refresh
                }
            }
        }

        public void TriggerMenuChanged()
        {
            Debug.WriteLine("RevisionGridMenuCommands.TriggerMenuChanged()");
            OnMenuChanged();
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
                menuCommand.Name = "GotoCurrentRevision";
                menuCommand.Text = "Go to current revision";
                menuCommand.Image = global::GitUI.Properties.Resources.IconGotoCurrentRevision;
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.SelectCurrentRevision);
                menuCommand.ExecuteAction = SelectCurrentRevisionExecute;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoCommit";
                menuCommand.Text = "Go to commit...";
                menuCommand.Image = global::GitUI.Properties.Resources.IconGotoCommit;
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.GoToCommit);
                menuCommand.ExecuteAction = GotoCommitExcecute;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoChildCommit";
                menuCommand.Text = "Go to child commit";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.GoToChild);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.GoToChild);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "GotoParentCommit";
                menuCommand.Text = "Go to parent commit";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.GoToParent);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.GoToParent);

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "NavigateBackward";
                menuCommand.Text = "Navigate backward";
                menuCommand.ShortcutKeyDisplayString = (Keys.Alt | Keys.Left).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.NavigateBackward);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "NavigateForward";
                menuCommand.Text = "Navigate forward";
                menuCommand.ShortcutKeyDisplayString = (Keys.Alt | Keys.Right).ToShortcutKeyDisplayString();
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.NavigateForward);

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
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.PrevQuickSearch);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.PrevQuickSearch);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "NextQuickSearch";
                menuCommand.Text = "Quick search next";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.NextQuickSearch);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.NextQuickSearch);

                resultList.Add(menuCommand);
            }

            return resultList;
        }

        /// <summary>
        /// this is needed because _revsionGrid is null when TranslationApp is called
        /// </summary>
        /// <param name="revGridCommands"></param>
        /// <returns></returns>
        private string GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands revGridCommands)
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
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowAllBranches);
                menuCommand.ExecuteAction = () => _revisionGrid.ShowAllBranches_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => _revisionGrid.ShowAllBranches_ToolStripMenuItemChecked;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowCurrentBranchOnly";
                menuCommand.Text = "Show current branch only";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowCurrentBranchOnly);
                menuCommand.ExecuteAction = () => _revisionGrid.ShowCurrentBranchOnly_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => _revisionGrid.ShowCurrentBranchOnly_ToolStripMenuItemChecked;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowFilteredBranches";
                menuCommand.Text = "Show filtered branches";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowFilteredBranches);
                menuCommand.ExecuteAction = () => _revisionGrid.ShowFilteredBranches_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => _revisionGrid.ShowFilteredBranches_ToolStripMenuItemChecked;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowRemoteBranches";
                menuCommand.Text = "Show remote branches";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowRemoteBranches);
                menuCommand.ExecuteAction = () => _revisionGrid.ShowRemoteBranches_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowRemoteBranches;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowReflogReferences";
                menuCommand.Text = "Show reflog references";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowReflogReferences_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowReflogReferences;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowSuperprojectTags";
                menuCommand.Text = "Show superproject tags";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowSuperprojectTags_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowSuperprojectTags;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowSuperprojectBranches";
                menuCommand.Text = "Show superproject branches";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowSuperprojectBranches_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowSuperprojectBranches;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ShowSuperprojectRemoteBranches";
                menuCommand.Text = "Show superproject remote branches";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowSuperprojectRemoteBranches_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowSuperprojectRemoteBranches;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "showRevisionGraphToolStripMenuItem";
                menuCommand.Text = "Show revision graph";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowRevisionGraph_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => _layoutRendererFactory.GetCurrent().IsGraphLayout;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "drawNonrelativesGrayToolStripMenuItem";
                menuCommand.Text = "Draw non relatives gray";
                menuCommand.ExecuteAction = () => _revisionGrid.DrawNonrelativesGray_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.RevisionGraphDrawNonRelativesGray;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "orderRevisionsByDateToolStripMenuItem";
                menuCommand.Text = "Order revisions by date";
                menuCommand.ExecuteAction = () => _revisionGrid.OrderRevisionsByDate_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.OrderRevisionByDate;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "showAuthorDateToolStripMenuItem";
                menuCommand.Text = "Show author date";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowAuthorDate_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowAuthorDate;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "showRelativeDateToolStripMenuItem";
                menuCommand.Text = "Show relative date";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowRelativeDate_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.RelativeDate;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "showMergeCommitsToolStripMenuItem";
                menuCommand.Text = "Show merge commits";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ToggleShowMergeCommits);
                menuCommand.ExecuteAction = () => _revisionGrid.ShowMergeCommits_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowMergeCommits;

                resultList.Add(menuCommand);
            }

            {
              var menuCommand = new MenuCommand();
              menuCommand.Name = "showTagsToolStripMenuItem";
              menuCommand.Text = "Show tags";
              menuCommand.ExecuteAction = () => _revisionGrid.ShowTags_ToolStripMenuItemClick(null, null);
              menuCommand.IsCheckedFunc = () => AppSettings.ShowTags;

              resultList.Add(menuCommand);
            }

            {
              var menuCommand = new MenuCommand();
              menuCommand.Name = "showIdsToolStripMenuItem";
              menuCommand.Text = "Show SHA-1";
              menuCommand.ExecuteAction = () => _revisionGrid.ShowIds_ToolStripMenuItemClick(null, null);
              menuCommand.IsCheckedFunc = () => AppSettings.ShowIds;

              resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "showGitNotesToolStripMenuItem";
                menuCommand.Text = "Show git notes";
                menuCommand.ExecuteAction = () => _revisionGrid.ShowGitNotes_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowGitNotes;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "showIsMessageMultilineToolStripMenuItem";
                menuCommand.Text = "Show indicator for multiline message";
                menuCommand.ExecuteAction = () => 
                {
                    AppSettings.ShowIndicatorForMultilineMessage = !AppSettings.ShowIndicatorForMultilineMessage;
                    _revisionGrid.ForceRefreshRevisions();
                };
                menuCommand.IsCheckedFunc = () => AppSettings.ShowIndicatorForMultilineMessage;

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ToggleHighlightSelectedBranch";
                menuCommand.Text = "Highlight selected branch (until refresh)";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ToggleHighlightSelectedBranch);
                menuCommand.ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.ToggleHighlightSelectedBranch);

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "ToggleRevisionCardLayout";
                menuCommand.Text = "Change commit view layout";
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ToggleRevisionCardLayout);
                menuCommand.ExecuteAction = () => _revisionGrid.ToggleRevisionCardLayout();

                resultList.Add(menuCommand);
            }

            resultList.Add(MenuCommand.CreateSeparator());

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "showFirstParent";
                menuCommand.Text = "Show first parents";
                menuCommand.Image = global::GitUI.Properties.Resources.IconShowFirstParent;
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowFirstParent);
                menuCommand.ExecuteAction = () => _revisionGrid.ShowFirstParent_ToolStripMenuItemClick(null, null);
                menuCommand.IsCheckedFunc = () => AppSettings.ShowFirstParent;

                resultList.Add(menuCommand);
            }

            {
                var menuCommand = new MenuCommand();
                menuCommand.Name = "filterToolStripMenuItem";
                menuCommand.Text = "Set advanced filter";
                menuCommand.Image = global::GitUI.Properties.Resources.IconFilter;
                menuCommand.ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.RevisionFilter);
                menuCommand.ExecuteAction = () => _revisionGrid.FilterToolStripMenuItemClick(null, null);

                resultList.Add(menuCommand);
            }

            return resultList;
        }

        public IEnumerable<MenuCommand> GetViewMenuCommands()
        {
            return _viewMenuCommands;
        }

        public event EventHandler MenuChanged;

        // taken from http://stackoverflow.com/questions/5058254/inotifypropertychanged-propertychangedeventhandler-event-is-always-null
        // paramenter name not used
        protected void OnMenuChanged()
        {
            Debug.WriteLine("RevisionGridMenuCommands.OnPropertyChanged()");

            EventHandler handler = MenuChanged;
            if (handler != null)
            {
                handler(this, null);
            }

            foreach (var menuCommand in GetMenuCommandsWithoutSeparators())
            {
                menuCommand.SetCheckForRegisteredMenuItems();
                menuCommand.UpdateMenuItemsShortcutKeyDisplayString();
            }
        }

        protected override IEnumerable<MenuCommand> GetMenuCommandsForTranslation()
        {
            return GetMenuCommandsWithoutSeparators();
        }

        private IEnumerable<MenuCommand> GetMenuCommandsWithoutSeparators()
        {
            return _navigateMenuCommands.Concat(_viewMenuCommands).Where(mc => !mc.IsSeparator);
        }

        public void SelectCurrentRevisionExecute()
        {
            _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.SelectCurrentRevision);
        }

        public void GotoCommitExcecute()
        {
            using (FormGoToCommit formGoToCommit = new FormGoToCommit(_revisionGrid.UICommands))
            {
                if (formGoToCommit.ShowDialog(_revisionGrid) != DialogResult.OK)
                    return;

                string revisionGuid = formGoToCommit.ValidateAndGetSelectedRevision();
                if (!string.IsNullOrEmpty(revisionGuid))
                {
                    _revisionGrid.SetSelectedRevision(new GitRevision(_revisionGrid.Module, revisionGuid));
                }
                else
                {
                    MessageBox.Show(_revisionGrid, _noRevisionFoundError.Text);
                }
            }
        }
    }
}
