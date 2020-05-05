using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    internal class RevisionGridMenuCommands : MenuCommandsBase
    {
        public event EventHandler MenuChanged;

        private readonly TranslationString _quickSearchQuickHelp = new TranslationString("Start typing in revision grid to start quick search.");
        private readonly TranslationString _noRevisionFoundError = new TranslationString("No revision found.");

        private readonly RevisionGridControl _revisionGrid;

        public IReadOnlyList<MenuCommand> NavigateMenuCommands { get; }
        public IReadOnlyList<MenuCommand> ViewMenuCommands { get; }

        public RevisionGridMenuCommands(RevisionGridControl revisionGrid)
        {
            _revisionGrid = revisionGrid;
            NavigateMenuCommands = CreateNavigateMenuCommands();
            ViewMenuCommands = CreateViewMenuCommands();
            TranslationCategoryName = "RevisionGrid";
            Translate();
        }

        /// <summary>
        /// ... "Update" because the hotkey settings might change
        /// </summary>
        public void CreateOrUpdateMenuCommands()
        {
            UpdateMenuCommandShortcutKeyDisplayString(NavigateMenuCommands, CreateNavigateMenuCommands());
            UpdateMenuCommandShortcutKeyDisplayString(ViewMenuCommands, CreateViewMenuCommands());

            if (_revisionGrid != null)
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
            return new[]
            {
                new MenuCommand
                {
                    Name = "ToggleBetweenArtificialAndHeadCommits",
                    Text = "Toggle between artificial and HEAD commits",
                    Image = Images.WorkingDirChanges,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ToggleBetweenArtificialAndHeadCommits),
                    ExecuteAction = () => _revisionGrid.ToggleBetweenArtificialAndHeadCommits()
                },
                new MenuCommand
                {
                    Name = "GotoCurrentRevision",
                    Text = "Go to current revision",
                    Image = Images.GotoCurrentRevision,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.SelectCurrentRevision),
                    ExecuteAction = SelectCurrentRevisionExecute
                },
                new MenuCommand
                {
                    Name = "GotoCommit",
                    Text = "Go to commit...",
                    Image = Images.GotoCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToCommit),
                    ExecuteAction = GotoCommitExecute
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "GotoChildCommit",
                    Text = "Go to child commit",
                    Image = Images.GoToChildCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToChild),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.GoToChild)
                },
                new MenuCommand
                {
                    Name = "GotoParentCommit",
                    Text = "Go to parent commit",
                    Image = Images.GoToParentCommit,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToParent),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.GoToParent)
                },
                new MenuCommand
                {
                    Name = "GotoMergeBaseCommit",
                    Text = "Go to common ancestor (merge base)",
                    ToolTipText = "Selects the common ancestor commit (merge base), which is the most recent shared ancestor of the selected commits (or if only one commit is selected, between it and the checked out commit (HEAD))",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.GoToMergeBaseCommit),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.GoToMergeBaseCommit)
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "NavigateBackward",
                    Text = "Navigate backward",
                    Image = Images.NavigateBackward,
                    ShortcutKeyDisplayString = (Keys.Alt | Keys.Left).ToShortcutKeyDisplayString(),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.NavigateBackward)
                },
                new MenuCommand
                {
                    Name = "NavigateForward",
                    Text = "Navigate forward",
                    Image = Images.NavigateForward,
                    ShortcutKeyDisplayString = (Keys.Alt | Keys.Right).ToShortcutKeyDisplayString(),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.NavigateForward)
                },
                MenuCommand.CreateSeparator(),
                new MenuCommand
                {
                    Name = "QuickSearch",
                    Text = "Quick search",
                    ToolTipText = _quickSearchQuickHelp.Text,
                    ExecuteAction = () => MessageBox.Show(_quickSearchQuickHelp.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
                },
                new MenuCommand
                {
                    Name = "PrevQuickSearch",
                    Text = "Quick search previous",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.PrevQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.PrevQuickSearch)
                },
                new MenuCommand
                {
                    Name = "NextQuickSearch",
                    Text = "Quick search next",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.NextQuickSearch),
                    ExecuteAction = () => _revisionGrid.ExecuteCommand(RevisionGridControl.Command.NextQuickSearch)
                }
            };
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
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowAllBranches),
                    ExecuteAction = () => _revisionGrid.ShowAllBranches(),
                    IsCheckedFunc = () => _revisionGrid.IsShowAllBranchesChecked
                },
                new MenuCommand
                {
                    Name = "ShowCurrentBranchOnly",
                    Text = "Show current branch only",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowCurrentBranchOnly),
                    ExecuteAction = () => _revisionGrid.ShowCurrentBranchOnly(),
                    IsCheckedFunc = () => _revisionGrid.IsShowCurrentBranchOnlyChecked
                },
                new MenuCommand
                {
                    Name = "ShowFilteredBranches",
                    Text = "Show filtered branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowFilteredBranches),
                    ExecuteAction = () => _revisionGrid.ShowFilteredBranches(),
                    IsCheckedFunc = () => _revisionGrid.IsShowFilteredBranchesChecked
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "ShowRemoteBranches",
                    Text = "Show remote branches",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowRemoteBranches),
                    ExecuteAction = () => _revisionGrid.ToggleShowRemoteBranches(),
                    IsCheckedFunc = () => AppSettings.ShowRemoteBranches
                },
                new MenuCommand
                {
                    Name = "ShowArtificialCommits",
                    Text = "Show artificial commits",
                    ExecuteAction = () => _revisionGrid.ToggleShowArtificialCommits(),
                    IsCheckedFunc = () => AppSettings.RevisionGraphShowWorkingDirChanges
                },
                new MenuCommand
                {
                    Name = "ShowReflogReferences",
                    Text = "Show reflog references",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowReflogReferences),
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
                    Name = "showRevisionGraphColumnToolStripMenuItem",
                    Text = "Show revision graph column",
                    ExecuteAction = () => _revisionGrid.ToggleRevisionGraphColumn(),
                    IsCheckedFunc = () => AppSettings.ShowRevisionGridGraphColumn
                },
                new MenuCommand
                {
                    Name = "showAuthorAvatarColumnToolStripMenuItem",
                    Text = "Show author avatar column",
                    ExecuteAction = () => _revisionGrid.ToggleAuthorAvatarColumn(),
                    IsCheckedFunc = () => AppSettings.ShowAuthorAvatarColumn
                },
                new MenuCommand
                {
                    Name = "showAuthorNameColumnToolStripMenuItem",
                    Text = "Show author name column",
                    ExecuteAction = () => _revisionGrid.ToggleAuthorNameColumn(),
                    IsCheckedFunc = () => AppSettings.ShowAuthorNameColumn
                },
                new MenuCommand
                {
                    Name = "showDateColumnToolStripMenuItem",
                    Text = "Show date column",
                    ExecuteAction = () => _revisionGrid.ToggleDateColumn(),
                    IsCheckedFunc = () => AppSettings.ShowDateColumn
                },
                new MenuCommand
                {
                    Name = "showIdColumnToolStripMenuItem",
                    Text = "Show SHA-1 column",
                    ExecuteAction = () => _revisionGrid.ToggleObjectIdColumn(),
                    IsCheckedFunc = () => AppSettings.ShowObjectIdColumn
                },
                new MenuCommand
                {
                    Name = "showBuildStatusIconToolStripMenuItem",
                    Text = "Show build status icon",
                    ExecuteAction = () => _revisionGrid.ToggleBuildStatusIconColumn(),
                    IsCheckedFunc = () => AppSettings.ShowBuildStatusIconColumn
                },
                new MenuCommand
                {
                    Name = "showBuildStatusTextToolStripMenuItem",
                    Text = "Show build status text",
                    ExecuteAction = () => _revisionGrid.ToggleBuildStatusTextColumn(),
                    IsCheckedFunc = () => AppSettings.ShowBuildStatusTextColumn
                },

                MenuCommand.CreateSeparator(),

                new MenuCommand
                {
                    Name = "drawNonrelativesGrayToolStripMenuItem",
                    Text = "Draw non relatives gray",
                    ExecuteAction = () => _revisionGrid.ToggleDrawNonRelativesGray(),
                    IsCheckedFunc = () => AppSettings.RevisionGraphDrawNonRelativesGray
                },
                new MenuCommand
                {
                    Name = "AuthorDateSort",
                    Text = "Sort commits by author date",
                    ExecuteAction = () => _revisionGrid.ToggleAuthorDateSort(),
                    IsCheckedFunc = () => AppSettings.SortByAuthorDate
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
                    ExecuteAction = () => _revisionGrid.ToggleShowRelativeDate(null),
                    IsCheckedFunc = () => AppSettings.RelativeDate
                },
                new MenuCommand
                {
                    Name = "showMergeCommitsToolStripMenuItem",
                    Text = "Show merge commits",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ToggleShowMergeCommits),
                    ExecuteAction = () => _revisionGrid.ToggleShowMergeCommits(),
                    IsCheckedFunc = () => AppSettings.ShowMergeCommits
                },
                new MenuCommand
                {
                    Name = "showTagsToolStripMenuItem",
                    Text = "Show tags",
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ToggleShowTags),
                    ExecuteAction = () => _revisionGrid.ToggleShowTags(),
                    IsCheckedFunc = () => AppSettings.ShowTags
                },
                new MenuCommand
                {
                    Name = "showGitNotesToolStripMenuItem",
                    Text = "Show git notes",
                    ExecuteAction = () => _revisionGrid.ToggleShowGitNotes(),
                    IsCheckedFunc = () => AppSettings.ShowGitNotes
                },

                MenuCommand.CreateSeparator(),

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
                    Name = "showFirstParent",
                    Text = "Show first parents",
                    Image = Images.ShowFirstParent,
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command.ShowFirstParent),
                    ExecuteAction = () => _revisionGrid.ShowFirstParent(),
                    IsCheckedFunc = () => AppSettings.ShowFirstParent
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

        [CanBeNull]
        private string GetShortcutKeyDisplayStringFromRevisionGridIfAvailable(RevisionGridControl.Command revGridCommands)
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
            using (var formGoToCommit = new FormGoToCommit(_revisionGrid.UICommands))
            {
                if (formGoToCommit.ShowDialog(_revisionGrid) != DialogResult.OK)
                {
                    return;
                }

                var objectId = formGoToCommit.ValidateAndGetSelectedRevision();

                if (objectId != null)
                {
                    _revisionGrid.SetSelectedRevision(objectId);
                }
                else
                {
                    MessageBox.Show(_revisionGrid, _noRevisionFoundError.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
