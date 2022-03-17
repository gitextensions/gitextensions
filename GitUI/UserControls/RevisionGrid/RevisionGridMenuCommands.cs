using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Hotkey;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    internal class RevisionGridMenuCommands : MenuCommandsBase
    {
        public event EventHandler? MenuChanged;

        private readonly TranslationString _quickSearchQuickHelp = new("Start typing in revision grid to start quick search.");

        private readonly RevisionGridControl _revisionGrid;

        public IReadOnlyList<MenuCommand> NavigateMenuCommands { get; }
        public IReadOnlyList<MenuCommand> ViewMenuCommands { get; }

        public RevisionGridMenuCommands(RevisionGridControl revisionGrid)
        {
            _revisionGrid = revisionGrid;
            NavigateMenuCommands = CreateNavigateMenuCommands();
            ViewMenuCommands = CreateViewMenuCommands();
            Translate();
        }

        protected override string TranslationCategoryName => "RevisionGrid";

        /// <summary>
        /// ... "Update" because the hotkey settings might change.
        /// </summary>
        public void CreateOrUpdateMenuCommands()
        {
            UpdateMenuCommandShortcutKeyDisplayString(NavigateMenuCommands, CreateNavigateMenuCommands());
            UpdateMenuCommandShortcutKeyDisplayString(ViewMenuCommands, CreateViewMenuCommands());

            if (_revisionGrid is not null)
            {
                // null when TranslationApp is started
                TriggerMenuChanged(); // trigger refresh
                _revisionGrid.SetShortcutKeys();
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

        private IReadOnlyList<MenuCommand> CreateNavigateMenuCommands()
        {
            /*
                NAVIGATE menu
                ═════════════
                        DEGIJKLMOSVWXYZ
                    A   Go to common ancestor (merge base)
                    B   Navigate backward
                    C   Go to commit...
                    F   Navigate forward
                    H   Go to child commit
                    N   Quick search next
                    P   Go to parent commit
                    Q   Quick search
                    R   Quick search previous
                    T   Toggle between artificial and HEAD commits
                    U   Go to current revision
            */

            return new[]
            {
                new MenuCommand
                {
                    Name = "ToggleBetweenArtificialAndHeadCommits",
                    Text = "&Toggle between artificial and HEAD commits",
                    Image = Images.WorkingDirChanges,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ToggleBetweenArtificialAndHeadCommits),
                    ExecuteAction = () => _revisionGrid.ToggleBetweenArtificialAndHeadCommits()
                },
                new MenuCommand
                {
                    Name = "GotoCurrentRevision",
                    Text = "Go to c&urrent revision",
                    Image = Images.GotoCurrentRevision,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.SelectCurrentRevision),
                    ExecuteAction = SelectCurrentRevisionExecute
                },
                new MenuCommand
                {
                    Name = "GotoCommit",
                    Text = "Go to &commit...",
                    Image = Images.GotoCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToCommit),
                    ExecuteAction = GotoCommitExecute
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "GotoChildCommit",
                    Text = "Go to c&hild commit",
                    Image = Images.GoToChildCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToChild),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.GoToChild)
                },
                new MenuCommand
                {
                    Name = "GotoParentCommit",
                    Text = "Go to &parent commit",
                    Image = Images.GoToParentCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToParent),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.GoToParent)
                },
                new MenuCommand
                {
                    Name = "GotoMergeBaseCommit",
                    Text = "Go to common &ancestor (merge base)",
                    ToolTipText = "Selects the common ancestor commit (merge base), which is the most recent shared ancestor of the selected commits (or if only one commit is selected, between it and the checked out commit (HEAD))",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToMergeBase),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.GoToMergeBase)
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "NavigateBackward",
                    Text = "Navigate &backward",
                    Image = Images.NavigateBackward,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.NavigateBackward),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.NavigateBackward)
                },
                new MenuCommand
                {
                    Name = "NavigateForward",
                    Text = "Navigate &forward",
                    Image = Images.NavigateForward,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.NavigateForward),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.NavigateForward)
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "QuickSearch",
                    Text = "&Quick search",
                    ToolTipText = _quickSearchQuickHelp.Text,
                    ExecuteAction = () => MessageBox.Show(_quickSearchQuickHelp.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
                },
                new MenuCommand
                {
                    Name = "PrevQuickSearch",
                    Text = "Quick search p&revious",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.PrevQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.PrevQuickSearch)
                },
                new MenuCommand
                {
                    Name = "NextQuickSearch",
                    Text = "Quick search &next",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.NextQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.NextQuickSearch)
                }
            };
        }

        private IReadOnlyList<MenuCommand> CreateViewMenuCommands()
        {
            /*
                VIEW menu
                ═════════════
                        KQZ
                    !   Highlight selected branch (until refresh)
                    !   Set advanced filter
                    !   Show commit message body
                    !   Show first parents
                    -   Show SHA-1 column
                    A   Show all branches
                    B   Show remote branches
                    C   Show current branch only
                    D   Show date column
                    E   Show superproject branches
                    F   Show filtered branches
                    G   Show revision graph column
                    H   Show author avatar column
                    I   Show build status icon
                    J   Show superproject remote branches
                    L   Show latest stash
                    M   Show merge commits
                    N   Show git notes
                    O   Show artificial commits
                    P   Show superproject tags
                    R   Show reflog references
                    S   Sort commits by author date
                    T   Show tags
                    U   Show author name column
                    V   Show relative date
                    W   Show author date
                    X   Show build status text
                    Y   Draw non relatives gray
            */

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
                    Text = "Show &all branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowAllBranches),
                    ExecuteAction = () => _revisionGrid.ShowAllBranches(),
                    IsCheckedFunc = () => _revisionGrid.CurrentFilter.IsShowAllBranchesChecked
                },
                new MenuCommand
                {
                    Name = "ShowCurrentBranchOnly",
                    Text = "Show &current branch only",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowCurrentBranchOnly),
                    ExecuteAction = () => _revisionGrid.ShowCurrentBranchOnly(),
                    IsCheckedFunc = () => _revisionGrid.CurrentFilter.IsShowCurrentBranchOnlyChecked
                },
                new MenuCommand
                {
                    Name = "ShowFilteredBranches",
                    Text = "Show &filtered branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowFilteredBranches),
                    ExecuteAction = () => _revisionGrid.ShowFilteredBranches(),
                    IsCheckedFunc = () => _revisionGrid.CurrentFilter.IsShowFilteredBranchesChecked
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ShowRemoteBranches",
                    Text = "Show remote &branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowRemoteBranches),
                    ExecuteAction = () => _revisionGrid.ToggleShowRemoteBranches(),
                    IsCheckedFunc = () => AppSettings.ShowRemoteBranches
                },
                new MenuCommand
                {
                    Name = "ShowArtificialCommits",
                    Text = "Show artificial commits",
                    ExecuteAction = () => _revisionGrid.ToggleShowArtificialCommits(),
                    IsCheckedFunc = () => AppSettings.RevisionGraphShowArtificialCommits
                },
                new MenuCommand
                {
                    Name = "ShowReflogReferences",
                    Text = "Show &reflog references",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowReflogReferences),
                    ExecuteAction = () => _revisionGrid.ToggleShowReflogReferences(),
                    IsCheckedFunc = () => _revisionGrid.CurrentFilter.ShowReflogReferences
                },
                new MenuCommand
                {
                    Name = "ShowLatestStash",
                    Text = "Show &latest stash",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowLatestStash),
                    ExecuteAction = () => _revisionGrid.ToggleShowLatestStash(),
                    IsCheckedFunc = () => AppSettings.ShowLatestStash
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ShowSuperprojectTags",
                    Text = "Show su&perproject tags",
                    ExecuteAction = () => _revisionGrid.ToggleShowSuperprojectTags(),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectTags
                },
                new MenuCommand
                {
                    Name = "ShowSuperprojectBranches",
                    Text = "Show sup&erproject branches",
                    ExecuteAction = () => _revisionGrid.ShowSuperprojectBranches_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectBranches
                },
                new MenuCommand
                {
                    Name = "ShowSuperprojectRemoteBranches",
                    Text = "Show superpro&ject remote branches",
                    ExecuteAction = () => _revisionGrid.ShowSuperprojectRemoteBranches_ToolStripMenuItemClick(),
                    IsCheckedFunc = () => AppSettings.ShowSuperprojectRemoteBranches
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "showRevisionGraphColumnToolStripMenuItem",
                    Text = "Show revision &graph column",
                    ExecuteAction = () => _revisionGrid.ToggleRevisionGraphColumn(),
                    IsCheckedFunc = () => AppSettings.ShowRevisionGridGraphColumn
                },
                new MenuCommand
                {
                    Name = "showAuthorAvatarColumnToolStripMenuItem",
                    Text = "Show aut&hor avatar column",
                    ExecuteAction = () => _revisionGrid.ToggleAuthorAvatarColumn(),
                    IsCheckedFunc = () => AppSettings.ShowAuthorAvatarColumn
                },
                new MenuCommand
                {
                    Name = "showAuthorNameColumnToolStripMenuItem",
                    Text = "Show a&uthor name column",
                    ExecuteAction = () => _revisionGrid.ToggleAuthorNameColumn(),
                    IsCheckedFunc = () => AppSettings.ShowAuthorNameColumn
                },
                new MenuCommand
                {
                    Name = "showDateColumnToolStripMenuItem",
                    Text = "Show &date column",
                    ExecuteAction = () => _revisionGrid.ToggleDateColumn(),
                    IsCheckedFunc = () => AppSettings.ShowDateColumn
                },
                new MenuCommand
                {
                    Name = "showIdColumnToolStripMenuItem",
                    Text = "Show SHA&-1 column",
                    ExecuteAction = () => _revisionGrid.ToggleObjectIdColumn(),
                    IsCheckedFunc = () => AppSettings.ShowObjectIdColumn
                },
                new MenuCommand
                {
                    Name = "showBuildStatusIconToolStripMenuItem",
                    Text = "Show build status &icon",
                    ExecuteAction = () => _revisionGrid.ToggleBuildStatusIconColumn(),
                    IsCheckedFunc = () => AppSettings.ShowBuildStatusIconColumn
                },
                new MenuCommand
                {
                    Name = "showBuildStatusTextToolStripMenuItem",
                    Text = "Show build status te&xt",
                    ExecuteAction = () => _revisionGrid.ToggleBuildStatusTextColumn(),
                    IsCheckedFunc = () => AppSettings.ShowBuildStatusTextColumn
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "showAuthorDateToolStripMenuItem",
                    Text = "Sho&w author date",
                    ExecuteAction = () => _revisionGrid.ToggleShowAuthorDate(),
                    IsCheckedFunc = () => AppSettings.ShowAuthorDate
                },
                new MenuCommand
                {
                    Name = "showRelativeDateToolStripMenuItem",
                    Text = "Show relati&ve date",
                    ExecuteAction = () => _revisionGrid.ToggleShowRelativeDate(EventArgs.Empty),
                    IsCheckedFunc = () => AppSettings.RelativeDate
                },
                new MenuCommand
                {
                    Name = "showMergeCommitsToolStripMenuItem",
                    Text = "Show &merge commits",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ToggleShowMergeCommits),
                    ExecuteAction = () => _revisionGrid.ToggleShowMergeCommits(),
                    IsCheckedFunc = () => AppSettings.ShowMergeCommits
                },
                new MenuCommand
                {
                    Name = "showTagsToolStripMenuItem",
                    Text = "Show &tags",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ToggleShowTags),
                    ExecuteAction = () => _revisionGrid.ToggleShowTags(),
                    IsCheckedFunc = () => AppSettings.ShowTags
                },
                new MenuCommand
                {
                    Name = "showGitNotesToolStripMenuItem",
                    Text = "Show git &notes",
                    ExecuteAction = () => _revisionGrid.ToggleShowGitNotes(),
                    IsCheckedFunc = () => AppSettings.ShowGitNotes
                },
                new MenuCommand
                {
                    Name = "showCommitMessageBodyToolStripMenuItem",
                    Text = "Show commit message body",
                    ExecuteAction = () => _revisionGrid.ToggleShowCommitBodyInRevisionGrid(),
                    IsCheckedFunc = () => AppSettings.ShowCommitBodyInRevisionGrid
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "drawNonrelativesGrayToolStripMenuItem",
                    Text = "Draw non relatives gra&y",
                    ExecuteAction = () => _revisionGrid.ToggleDrawNonRelativesGray(),
                    IsCheckedFunc = () => AppSettings.RevisionGraphDrawNonRelativesGray
                },
                new MenuCommand
                {
                    Name = "ToggleHighlightSelectedBranch",
                    Text = "Highlight selected branch (until refresh)",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ToggleHighlightSelectedBranch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.ToggleHighlightSelectedBranch)
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "AuthorDateSort",
                    Text = "&Sort commits by author date",
                    ExecuteAction = () => _revisionGrid.ToggleAuthorDateSort(),
                    IsCheckedFunc = () => AppSettings.RevisionSortOrder == RevisionSortOrder.AuthorDate
                },
                new MenuCommand
                {
                    Name = "TopoOrder",
                    Text = "Arrange c&ommits by topo order (ancestor order)",
                    ExecuteAction = () => _revisionGrid.ToggleTopoOrder(),
                    IsCheckedFunc = () => AppSettings.RevisionSortOrder == RevisionSortOrder.Topology
                },
                new MenuCommand
                {
                    Name = "showFirstParent",
                    Text = "Show first parents",
                    Image = Images.ShowFirstParent,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowFirstParent),
                    ExecuteAction = () => _revisionGrid.ToggleShowFirstParent(),
                    IsCheckedFunc = () => _revisionGrid.CurrentFilter.ShowFirstParent
                },
                new MenuCommand
                {
                    Name = "fullHistory",
                    Text = "Full history",
                    ExecuteAction = () => _revisionGrid.ToggleFullHistory(),
                    IsCheckedFunc = () => AppSettings.FullHistoryInFileHistory
                },
                new MenuCommand
                {
                    Name = "simplifyMerges",
                    Text = "Simplify merges",
                    ExecuteAction = () => _revisionGrid.ToggleSimplifyMerges(),
                    IsCheckedFunc = () => AppSettings.SimplifyMergesInFileHistory,
                    IsEnabledFunc = () => AppSettings.FullHistoryInFileHistory
                },
                new MenuCommand
                {
                    Name = "filterToolStripMenuItem",
                    Text = "Set advanced filter",
                    Image = Images.EditFilter,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.RevisionFilter),
                    ExecuteAction = () => _revisionGrid.ShowRevisionFilterDialog()
                }
            };
        }

        private string? GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command revGridCommands)
        {
            // _revisionGrid is null when TranslationApp is called
            return _revisionGrid?.GetShortcutKeys(revGridCommands).ToShortcutKeyDisplayString();
        }

        protected override IEnumerable<MenuCommand> GetMenuCommandsForTranslation()
        {
            return GetMenuCommandsWithoutSeparators();
        }

        private IEnumerable<MenuCommand> GetMenuCommandsWithoutSeparators()
        {
            return NavigateMenuCommands.Concat(ViewMenuCommands).Where(mc => !mc.IsSeparator);
        }

        private void SelectCurrentRevisionExecute()
        {
            _revisionGrid.ExecuteCommand(RevisionGridControl.Command.SelectCurrentRevision);
        }

        public void GotoCommitExecute()
        {
            using FormGoToCommit formGoToCommit = new(_revisionGrid.UICommands);
            if (formGoToCommit.ShowDialog(_revisionGrid) != DialogResult.OK)
            {
                return;
            }

            var objectId = formGoToCommit.ValidateAndGetSelectedRevision();

            if (objectId is not null)
            {
                if (!_revisionGrid.SetSelectedRevision(objectId))
                {
                    MessageBoxes.RevisionFilteredInGrid(_revisionGrid, objectId);
                }
            }
            else
            {
                MessageBoxes.CannotFindGitRevision(owner: _revisionGrid);
            }
        }
    }
}
