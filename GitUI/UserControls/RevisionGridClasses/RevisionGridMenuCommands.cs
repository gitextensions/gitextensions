using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal class RevisionGridMenuCommands : MenuCommandsBase
    {
        private readonly TranslationString _quickSearchQuickHelp =
            new TranslationString("Start typing in revision grid to start quick search.");

        private readonly TranslationString _noRevisionFoundError =
            new TranslationString("No revision found.");

        private readonly RevisionGrid _revisionGrid;

        // must both be created only once
        private IEnumerable<MenuCommand> _navigateMenuCommands;
        private IEnumerable<MenuCommand> _viewMenuCommands;

        public RevisionGridMenuCommands(RevisionGrid revisionGrid)
        {
            _revisionGrid = revisionGrid;
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

                if (_revisionGrid != null)
                {
                    // null when TranslationApp is started
                    TriggerMenuChanged(); // trigger refresh
                }
            }
        }

        public void TriggerMenuChanged()
        {
            Debug.WriteLine("RevisionGridMenuCommands.TriggerMenuChanged()");
            OnMenuChanged();
        }

        private static void UpdateMenuCommandShortcutKeyDisplayString(IEnumerable<MenuCommand> targetList, IEnumerable<MenuCommand> sourceList)
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
            return new List<MenuCommand>
            {
                new MenuCommand
                {
                    Name = "GotoCurrentRevision",
                    Text = "Go to current revision",
                    Image = Properties.Resources.IconGotoCurrentRevision,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.SelectCurrentRevision),
                    ExecuteAction = SelectCurrentRevisionExecute
                },
                new MenuCommand
                {
                    Name = "GotoCommit",
                    Text = "Go to commit...",
                    Image = Properties.Resources.IconGotoCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.GoToCommit),
                    ExecuteAction = GotoCommitExcecute
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "GotoChildCommit",
                    Text = "Go to child commit",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.GoToChild),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGrid.Commands.GoToChild)
                },
                new MenuCommand
                {
                    Name = "GotoParentCommit",
                    Text = "Go to parent commit",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.GoToParent),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGrid.Commands.GoToParent)
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "NavigateBackward",
                    Text = "Navigate backward",
                    ShortcutKeyDisplayString = (Keys.Alt | Keys.Left).ToShortcutKeyDisplayString(),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGrid.Commands.NavigateBackward)
                },
                new MenuCommand
                {
                    Name = "NavigateForward",
                    Text = "Navigate forward",
                    ShortcutKeyDisplayString = (Keys.Alt | Keys.Right).ToShortcutKeyDisplayString(),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGrid.Commands.NavigateForward)
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "QuickSearch",
                    Text = "Quick search",
                    ExecuteAction = () => MessageBox.Show(_quickSearchQuickHelp.Text)
                },
                new MenuCommand
                {
                    Name = "PrevQuickSearch",
                    Text = "Quick search previous",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.PrevQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGrid.Commands.PrevQuickSearch)
                },
                new MenuCommand
                {
                    Name = "NextQuickSearch",
                    Text = "Quick search next",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.NextQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGrid.Commands.NextQuickSearch)
                }
            };
        }

        /// <summary>
        /// this is needed because _revsionGrid is null when TranslationApp is called
        /// </summary>
        private string GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands revGridCommands)
        {
            return _revisionGrid?.GetShortcutKeys(revGridCommands).ToShortcutKeyDisplayString();
        }

        private IEnumerable<MenuCommand> CreateViewMenuCommands()
        {
            return new List<MenuCommand>
            {
                // the first three MenuCommands just reuse (the currently rather
                // convoluted) logic from RevisionGrid.
                //
                // After refactoring the three items should be added to RevisionGrid
                // as done with "ShowRemoteBranches" and not via RevisionGrid.Designer.cs
                new MenuCommand
                {
                    Name = "ShowAllBranches",
                    Text = "Show all branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ShowAllBranches),
                    ExecuteAction = () => _revisionGrid.ShowAllBranches_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => _revisionGrid.ShowAllBranches_ToolStripMenuItemChecked
                },
                new MenuCommand
                {
                    Name = "ShowCurrentBranchOnly",
                    Text = "Show current branch only",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ShowCurrentBranchOnly),
                    ExecuteAction = () => _revisionGrid.ShowCurrentBranchOnly_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => _revisionGrid.ShowCurrentBranchOnly_ToolStripMenuItemChecked
                },
                new MenuCommand
                {
                    Name = "ShowFilteredBranches",
                    Text = "Show filtered branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ShowFilteredBranches),
                    ExecuteAction = () => _revisionGrid.ShowFilteredBranches_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => _revisionGrid.ShowFilteredBranches_ToolStripMenuItemChecked
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ShowRemoteBranches",
                    Text = "Show remote branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ShowRemoteBranches),
                    ExecuteAction = () => _revisionGrid.ShowRemoteBranches_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowRemoteBranches
                },
                new MenuCommand
                {
                    Name = "ShowReflogReferences",
                    Text = "Show reflog references",
                    ExecuteAction = () => _revisionGrid.ShowReflogReferences_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowReflogReferences
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ShowSuperprojectTags",
                    Text = "Show superproject tags",
                    ExecuteAction = () => _revisionGrid.ShowSuperprojectTags_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectTags
                },
                new MenuCommand
                {
                    Name = "ShowSuperprojectBranches",
                    Text = "Show superproject branches",
                    ExecuteAction = () => _revisionGrid.ShowSuperprojectBranches_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectBranches
                },
                new MenuCommand
                {
                    Name = "ShowSuperprojectRemoteBranches",
                    Text = "Show superproject remote branches",
                    ExecuteAction = () => _revisionGrid.ShowSuperprojectRemoteBranches_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectRemoteBranches
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "showRevisionGraphToolStripMenuItem",
                    Text = "Show revision graph",
                    ExecuteAction = () => _revisionGrid.ShowRevisionGraph_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => _revisionGrid.IsGraphLayout()
                },
                new MenuCommand
                {
                    Name = "drawNonrelativesGrayToolStripMenuItem",
                    Text = "Draw non relatives gray",
                    ExecuteAction = () => _revisionGrid.DrawNonrelativesGray_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.RevisionGraphDrawNonRelativesGray
                },
                new MenuCommand
                {
                    Name = "orderRevisionsByDateToolStripMenuItem",
                    Text = "Order revisions by date",
                    ExecuteAction = () => _revisionGrid.OrderRevisionsByDate_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.OrderRevisionByDate
                },
                new MenuCommand
                {
                    Name = "showAuthorDateToolStripMenuItem",
                    Text = "Show author date",
                    ExecuteAction = () => _revisionGrid.ShowAuthorDate_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowAuthorDate
                },
                new MenuCommand
                {
                    Name = "showRelativeDateToolStripMenuItem",
                    Text = "Show relative date",
                    ExecuteAction = () => _revisionGrid.ShowRelativeDate_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.RelativeDate
                },
                new MenuCommand
                {
                    Name = "showMergeCommitsToolStripMenuItem",
                    Text = "Show merge commits",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ToggleShowMergeCommits),
                    ExecuteAction = () => _revisionGrid.ShowMergeCommits_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowMergeCommits
                },
                new MenuCommand
                {
                    Name = "showTagsToolStripMenuItem",
                    Text = "Show tags",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ToggleShowTags),
                    ExecuteAction = () => _revisionGrid.ShowTags_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowTags
                },
                new MenuCommand
                {
                    Name = "showIdsToolStripMenuItem",
                    Text = "Show SHA-1",
                    ExecuteAction = () => _revisionGrid.ShowIds_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowIds
                },
                new MenuCommand
                {
                    Name = "showGitNotesToolStripMenuItem",
                    Text = "Show git notes",
                    ExecuteAction = () => _revisionGrid.ShowGitNotes_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowGitNotes
                },
                new MenuCommand
                {
                    Name = "showIsMessageMultilineToolStripMenuItem",
                    Text = "Show indicator for multiline message",
                    ExecuteAction = () =>
                    {
                        AppSettings.ShowIndicatorForMultilineMessage = !AppSettings.ShowIndicatorForMultilineMessage;
                        _revisionGrid.ForceRefreshRevisions();
                    },
                    IsCheckedFunc = () => AppSettings.ShowIndicatorForMultilineMessage
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ToggleHighlightSelectedBranch",
                    Text = "Highlight selected branch (until refresh)",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ToggleHighlightSelectedBranch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGrid.Commands.ToggleHighlightSelectedBranch)
                },
                new MenuCommand
                {
                    Name = "ToggleRevisionCardLayout",
                    Text = "Change commit view layout",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ToggleRevisionCardLayout),
                    ExecuteAction = () => _revisionGrid.ToggleRevisionCardLayout()
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "showFirstParent",
                    Text = "Show first parents",
                    Image = Properties.Resources.IconShowFirstParent,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.ShowFirstParent),
                    ExecuteAction = () => _revisionGrid.ShowFirstParent_ToolStripMenuItemClick(null, null),
                    IsCheckedFunc = () => AppSettings.ShowFirstParent
                },
                new MenuCommand
                {
                    Name = "filterToolStripMenuItem",
                    Text = "Set advanced filter",
                    Image = Properties.Resources.IconFilter,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGrid.Commands.RevisionFilter),
                    ExecuteAction = () => _revisionGrid.FilterToolStripMenuItemClick(null, null)
                },
                new MenuCommand
                {
                    Name = "ToggleBranchTreePanel",
                    Text = "Toggle left panel",
                    ExecuteAction = () => _revisionGrid.OnToggleBranchTreePanelRequested()
                }
            };
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

            MenuChanged?.Invoke(this, null);

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
            _revisionGrid.ExecuteCommand(RevisionGrid.Commands.SelectCurrentRevision);
        }

        public void GotoCommitExcecute()
        {
            using (FormGoToCommit formGoToCommit = new FormGoToCommit(_revisionGrid.UICommands))
            {
                if (formGoToCommit.ShowDialog(_revisionGrid) != DialogResult.OK)
                {
                    return;
                }

                string revisionGuid = formGoToCommit.ValidateAndGetSelectedRevision();
                if (!string.IsNullOrEmpty(revisionGuid))
                {
                    _revisionGrid.SetSelectedRevision(new GitRevision(revisionGuid));
                }
                else
                {
                    MessageBox.Show(_revisionGrid, _noRevisionFoundError.Text);
                }
            }
        }
    }
}
