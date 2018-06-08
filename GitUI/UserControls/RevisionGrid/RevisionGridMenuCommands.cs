using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Hotkey;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    internal class RevisionGridMenuCommands : MenuCommandsBase
    {
        private readonly TranslationString _quickSearchQuickHelp = new TranslationString("Start typing in revision grid to start quick search.");
        private readonly TranslationString _noRevisionFoundError = new TranslationString("No revision found.");

        private readonly RevisionGridControl _revisionGrid;

        // must both be created only once
        private IReadOnlyList<MenuCommand> _navigateMenuCommands;
        private IReadOnlyList<MenuCommand> _viewMenuCommands;

        public RevisionGridMenuCommands(RevisionGridControl revisionGrid)
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

            return;

            void UpdateMenuCommandShortcutKeyDisplayString(IReadOnlyList<MenuCommand> targetList, IEnumerable<MenuCommand> sourceList)
            {
                foreach (var source in sourceList.Where(mc => !mc.IsSeparator))
                {
                    var target = targetList.Single(mc => !mc.IsSeparator && mc.Name == source.Name);
                    target.ShortcutKeyDisplayString = source.ShortcutKeyDisplayString;
                }
            }
        }

        public void TriggerMenuChanged()
        {
            MenuChanged?.Invoke(this, null);

            foreach (var menuCommand in GetMenuCommandsWithoutSeparators())
            {
                menuCommand.SetCheckForRegisteredMenuItems();
                menuCommand.UpdateMenuItemsShortcutKeyDisplayString();
            }
        }

        public IReadOnlyList<MenuCommand> GetNavigateMenuCommands()
        {
            return _navigateMenuCommands;
        }

        private IReadOnlyList<MenuCommand> CreateNavigateMenuCommands()
        {
            return new[]
            {
                new MenuCommand
                {
                    Name = "GotoCurrentRevision",
                    Text = "Go to current revision",
                    Image = Properties.Resources.IconGotoCurrentRevision,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.SelectCurrentRevision),
                    ExecuteAction = SelectCurrentRevisionExecute
                },
                new MenuCommand
                {
                    Name = "GotoCommit",
                    Text = "Go to commit...",
                    Image = Properties.Resources.IconGotoCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.GoToCommit),
                    ExecuteAction = GotoCommitExcecute
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "GotoChildCommit",
                    Text = "Go to child commit",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.GoToChild),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.GoToChild)
                },
                new MenuCommand
                {
                    Name = "GotoParentCommit",
                    Text = "Go to parent commit",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.GoToParent),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.GoToParent)
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "NavigateBackward",
                    Text = "Navigate backward",
                    ShortcutKeyDisplayString = (Keys.Alt | Keys.Left).ToShortcutKeyDisplayString(),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.NavigateBackward)
                },
                new MenuCommand
                {
                    Name = "NavigateForward",
                    Text = "Navigate forward",
                    ShortcutKeyDisplayString = (Keys.Alt | Keys.Right).ToShortcutKeyDisplayString(),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.NavigateForward)
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
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.PrevQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.PrevQuickSearch)
                },
                new MenuCommand
                {
                    Name = "NextQuickSearch",
                    Text = "Quick search next",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.NextQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.NextQuickSearch)
                }
            };
        }

        /// <summary>
        /// this is needed because _revisionGrid is null when TranslationApp is called
        /// </summary>
        [CanBeNull]
        private string GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands revGridCommands)
        {
            return _revisionGrid?.GetShortcutKeys(revGridCommands).ToShortcutKeyDisplayString();
        }

        private IReadOnlyList<MenuCommand> CreateViewMenuCommands()
        {
            return new[]
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
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowAllBranches),
                    ExecuteAction = () => _revisionGrid.ShowAllBranches(),
                    IsCheckedFunc = () => _revisionGrid.IsShowAllBranchesChecked
                },
                new MenuCommand
                {
                    Name = "ShowCurrentBranchOnly",
                    Text = "Show current branch only",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowCurrentBranchOnly),
                    ExecuteAction = () => _revisionGrid.ShowCurrentBranchOnly(),
                    IsCheckedFunc = () => _revisionGrid.IsShowCurrentBranchOnlyChecked
                },
                new MenuCommand
                {
                    Name = "ShowFilteredBranches",
                    Text = "Show filtered branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowFilteredBranches),
                    ExecuteAction = () => _revisionGrid.ShowFilteredBranches(),
                    IsCheckedFunc = () => _revisionGrid.IsShowFilteredBranchesChecked
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ShowRemoteBranches",
                    Text = "Show remote branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowRemoteBranches),
                    ExecuteAction = () => _revisionGrid.ToggleShowRemoteBranches(),
                    IsCheckedFunc = () => AppSettings.ShowRemoteBranches
                },
                new MenuCommand
                {
                    Name = "ShowReflogReferences",
                    Text = "Show reflog references",
                    ExecuteAction = () => _revisionGrid.ToggleShowReflogReferences(),
                    IsCheckedFunc = () => AppSettings.ShowReflogReferences
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ShowSuperprojectTags",
                    Text = "Show superproject tags",
                    ExecuteAction = () => _revisionGrid.ToggleShowSuperprojectTags(),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectTags
                },
                new MenuCommand
                {
                    Name = "ShowSuperprojectBranches",
                    Text = "Show superproject branches",
                    ExecuteAction = () => _revisionGrid.ShowSuperprojectBranches_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectBranches
                },
                new MenuCommand
                {
                    Name = "ShowSuperprojectRemoteBranches",
                    Text = "Show superproject remote branches",
                    ExecuteAction = () => _revisionGrid.ShowSuperprojectRemoteBranches_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectRemoteBranches
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "showRevisionGraphToolStripMenuItem",
                    Text = "Show revision graph",
                    ExecuteAction = () => _revisionGrid.ToggleRevisionGraph(),
                    IsCheckedFunc = () => AppSettings.ShowRevisionGridGraphColumn
                },
                new MenuCommand
                {
                    Name = "drawNonrelativesGrayToolStripMenuItem",
                    Text = "Draw non relatives gray",
                    ExecuteAction = () => _revisionGrid.DrawNonrelativesGray_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.RevisionGraphDrawNonRelativesGray
                },
                new MenuCommand
                {
                    Name = "orderRevisionsByDateToolStripMenuItem",
                    Text = "Order revisions by date",
                    ExecuteAction = () => _revisionGrid.ToggleOrderRevisionByDate(),
                    IsCheckedFunc = () => AppSettings.OrderRevisionByDate
                },
                new MenuCommand
                {
                    Name = "showAuthorDateToolStripMenuItem",
                    Text = "Show author date",
                    ExecuteAction = () => _revisionGrid.ToggleShowAuthorDate(),
                    IsCheckedFunc = () => AppSettings.ShowAuthorDate
                },
                new MenuCommand
                {
                    Name = "showRelativeDateToolStripMenuItem",
                    Text = "Show relative date",
                    ExecuteAction = () => _revisionGrid.ShowRelativeDate_ToolStripMenuItemClick(null),
                    IsCheckedFunc = () => AppSettings.RelativeDate
                },
                new MenuCommand
                {
                    Name = "showMergeCommitsToolStripMenuItem",
                    Text = "Show merge commits",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ToggleShowMergeCommits),
                    ExecuteAction = () => _revisionGrid.ShowMergeCommits_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.ShowMergeCommits
                },
                new MenuCommand
                {
                    Name = "showTagsToolStripMenuItem",
                    Text = "Show tags",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ToggleShowTags),
                    ExecuteAction = () => _revisionGrid.ShowTags_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.ShowTags
                },
                new MenuCommand
                {
                    Name = "showIdsToolStripMenuItem",
                    Text = "Show SHA-1",
                    ExecuteAction = () => _revisionGrid.ShowIds_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.ShowIds
                },
                new MenuCommand
                {
                    Name = "showGitNotesToolStripMenuItem",
                    Text = "Show git notes",
                    ExecuteAction = () => _revisionGrid.ShowGitNotes_ToolStripMenuItemClick(),
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
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ToggleHighlightSelectedBranch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.ToggleHighlightSelectedBranch)
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "showFirstParent",
                    Text = "Show first parents",
                    Image = Properties.Resources.IconShowFirstParent,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.ShowFirstParent),
                    ExecuteAction = () => _revisionGrid.ShowFirstParent(),
                    IsCheckedFunc = () => AppSettings.ShowFirstParent
                },
                new MenuCommand
                {
                    Name = "filterToolStripMenuItem",
                    Text = "Set advanced filter",
                    Image = Properties.Resources.IconFilter,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Commands.RevisionFilter),
                    ExecuteAction = () => _revisionGrid.ShowRevisionFilterDialog()
                },
                new MenuCommand
                {
                    Name = "ToggleBranchTreePanel",
                    Text = "Toggle left panel",
                    Image = Properties.MsVsImages.Branch_16x,
                    ExecuteAction = () => _revisionGrid.OnToggleBranchTreePanelRequested()
                }
            };
        }

        public IReadOnlyList<MenuCommand> GetViewMenuCommands()
        {
            return _viewMenuCommands;
        }

        public event EventHandler MenuChanged;

        protected override IEnumerable<MenuCommand> GetMenuCommandsForTranslation()
        {
            return GetMenuCommandsWithoutSeparators();
        }

        private IEnumerable<MenuCommand> GetMenuCommandsWithoutSeparators()
        {
            return _navigateMenuCommands.Concat(_viewMenuCommands).Where(mc => !mc.IsSeparator);
        }

        private void SelectCurrentRevisionExecute()
        {
            _revisionGrid.ExecuteCommand(RevisionGridControl.Commands.SelectCurrentRevision);
        }

        public void GotoCommitExcecute()
        {
            using (var formGoToCommit = new FormGoToCommit(_revisionGrid.UICommands))
            {
                if (formGoToCommit.ShowDialog(_revisionGrid) != DialogResult.OK)
                {
                    return;
                }

                var revisionGuid = formGoToCommit.ValidateAndGetSelectedRevision();
                if (revisionGuid != null)
                {
                    _revisionGrid.SetSelectedRevision(new GitRevision(revisionGuid.ToString()));
                }
                else
                {
                    MessageBox.Show(_revisionGrid, _noRevisionFoundError.Text);
                }
            }
        }
    }
}
