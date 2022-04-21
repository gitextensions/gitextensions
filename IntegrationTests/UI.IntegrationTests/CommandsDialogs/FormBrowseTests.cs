using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormBrowseTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        // Track the original setting value
        private bool _originalShowAuthorAvatarColumn;
        private bool _showAvailableDiffTools;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            // Remember the current settings...
            _originalShowAuthorAvatarColumn = AppSettings.ShowAuthorAvatarColumn;
            _showAvailableDiffTools = AppSettings.ShowAvailableDiffTools;

            // Stop loading custom diff tools
            AppSettings.ShowAvailableDiffTools = false;

            // We don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;
            AppSettings.ShowAvailableDiffTools = _showAvailableDiffTools;

            _referenceRepository.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);

            _commands = new GitUICommands(_referenceRepository.Module);

            var composition = TestComposition.Empty
                .AddParts(typeof(MockLinkFactory))
                .AddParts(typeof(MockWindowsJumpListManager))
                .AddParts(typeof(MockRepositoryDescriptionProvider))
                .AddParts(typeof(MockAppTitleGenerator));
            ExportProvider mefExportProvider = composition.ExportProviderFactory.CreateExportProvider();
            ManagedExtensibility.SetTestExportProvider(mefExportProvider);
        }

        [Test]
        public void PopulateFavouriteRepositoriesMenu_should_order_favourites_alphabetically()
        {
            RunFormTest(
                form =>
                {
                    ToolStripMenuItem tsmiFavouriteRepositories = new();
                    List<Repository> repositoryHistory = new()
                    {
                        new Repository(@"c:\") { Category = "D" },
                        new Repository(@"c:\") { Category = "A" },
                        new Repository(@"c:\") { Category = "C" },
                        new Repository(@"c:\") { Category = "B" }
                    };

                    form.GetTestAccessor().PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories, repositoryHistory);

                    // assert
                    var categories = tsmiFavouriteRepositories.DropDownItems.Cast<ToolStripMenuItem>().Select(x => x.Text).ToList();
                    categories.Should().BeInAscendingOrder();
                });
        }

#if !DEBUG
        [Ignore("This test is unstable in AppVeyor")]
#endif
        [Test]
        public void Filters_should_behave_as_expected()
        {
            _referenceRepository.CreateCommit("Commit1", "Commit1");
            _referenceRepository.CreateBranch("Branch1", _referenceRepository.CommitHash);
            _referenceRepository.CreateCommit("Commit2", "Commit2");
            _referenceRepository.CreateBranch("Branch2", _referenceRepository.CommitHash);

            _referenceRepository.CreateCommit("head commit");

            _referenceRepository.CheckoutBranch("Branch1");

            bool branchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool showCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
            AppSettings.BranchFilterEnabled = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.RevisionGraphShowArtificialCommits = false;

            RunFormTest(
                form =>
                {
                    try
                    {
                        // 1. Cycle between "Show all branches" > "Show current branch" > "Show filterd branches"
                        // ------------------------------------------------------------------------------------------------

                        Console.WriteLine("Scenario 1: set 'Show all branches'");
                        // Assert
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);
                        AppSettings.BranchFilterEnabled.Should().BeFalse();
                        AppSettings.ShowCurrentBranchOnly.Should().BeFalse();

                        Console.WriteLine("Scenario 1: set 'Show current branch'");
                        form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tsmiShowBranchesCurrent.PerformClick();
                        WaitForRevisionsToBeLoaded(form);
                        // Assert
                        AppSettings.BranchFilterEnabled.Should().BeTrue();
                        AppSettings.ShowCurrentBranchOnly.Should().BeTrue();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(2);

                        Console.WriteLine("Scenario 1: set 'Show filtered branches'");
                        form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tsmiShowBranchesFiltered.PerformClick();
                        WaitForRevisionsToBeLoaded(form);
                        // Assert
                        AppSettings.BranchFilterEnabled.Should().BeTrue();
                        AppSettings.ShowCurrentBranchOnly.Should().BeFalse();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);

                        // 2. Apply a branch filter
                        // ------------------------------------------------------------------------------------------------

                        Console.WriteLine("Scenario 2: apply branch filter 'Branch2'");
                        form.GetTestAccessor().ToolStripFilters.SetBranchFilter("Branch2");
                        WaitForRevisionsToBeLoaded(form);
                        // Assert
                        AppSettings.BranchFilterEnabled.Should().BeTrue();
                        AppSettings.ShowCurrentBranchOnly.Should().BeFalse();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(3);

                        Console.WriteLine("Scenario 2: set 'Show current branch'");
                        form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tsmiShowBranchesCurrent.PerformClick();
                        WaitForRevisionsToBeLoaded(form);
                        // Assert
                        AppSettings.BranchFilterEnabled.Should().BeTrue();
                        AppSettings.ShowCurrentBranchOnly.Should().BeTrue();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(2);
                        // The filter text is still present
                        form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tscboBranchFilter.Text.Should().Be("Branch2");

                        // 3. Switch to another repo - "Show current branch" must remain, filter text must be erased
                        // ------------------------------------------------------------------------------------------------

                        Console.WriteLine("Scenario 3: switch repo");
                        using ReferenceRepository repository = new();
                        form.SetWorkingDir(repository.Module.WorkingDir);
                        WaitForRevisionsToBeLoaded(form);
                        // Assert
                        AppSettings.BranchFilterEnabled.Should().BeFalse();
                        AppSettings.ShowCurrentBranchOnly.Should().BeTrue();
                        form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tscboBranchFilter.Text.Should().BeEmpty();
                    }
                    finally
                    {
                        AppSettings.BranchFilterEnabled = branchFilterEnabled;
                        AppSettings.ShowCurrentBranchOnly = showCurrentBranchOnly;
                        AppSettings.RevisionGraphShowArtificialCommits = revisionGraphShowArtificialCommits;
                    }
                });
        }

        [Test]
        public void ShowLatestStash_starting_disabled_should_filter_as_expected()
        {
            using ReferenceRepository referenceRepository = new();
            GitUICommands commands = new(referenceRepository.Module);

            referenceRepository.CreateCommit("Commit1");
            referenceRepository.Stash("Stash1");
            referenceRepository.CreateCommit("Commit2");
            referenceRepository.Stash("Stash2");
            referenceRepository.CreateCommit("Commit3");

            bool showLatestStash = AppSettings.ShowLatestStash;
            bool revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;

            AppSettings.ShowLatestStash = false;
            AppSettings.RevisionGraphShowArtificialCommits = false;
            RunFormTest(
                form =>
                {
                    try
                    {
                        // 1. Check with ShowLatestStash disabled
                        Console.WriteLine("Scenario 1: set 'Show latest stash' to false");
                        // Assert
                        AppSettings.ShowLatestStash.Should().BeFalse();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);

                        // 2. Change ShowLatestStash to enabled
                        Console.WriteLine("Scenario 1: change 'Show latest stash' to enabled");
                        form.GetTestAccessor().RevisionGrid.ToggleShowLatestStash();
                        WaitForRevisionsToBeLoaded(form);
                        // Assert
                        AppSettings.ShowLatestStash.Should().BeTrue();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(6);
                    }
                    finally
                    {
                        AppSettings.ShowLatestStash = showLatestStash;
                        AppSettings.RevisionGraphShowArtificialCommits = revisionGraphShowArtificialCommits;
                    }
                },
                commands);
        }

        [Test]
        public void ShowLatestStash_starting_enabled_should_filter_as_expected()
        {
            using ReferenceRepository referenceRepository = new();
            GitUICommands commands = new(referenceRepository.Module);

            referenceRepository.CreateCommit("Commit1");
            referenceRepository.Stash("Stash1");
            referenceRepository.CreateCommit("Commit2");
            referenceRepository.Stash("Stash2");
            referenceRepository.CreateCommit("Commit3");

            bool showLatestStash = AppSettings.ShowLatestStash;
            bool revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;

            AppSettings.ShowLatestStash = true;
            AppSettings.RevisionGraphShowArtificialCommits = false;
            RunFormTest(
                form =>
                {
                    try
                    {
                        // 2. Check with ShowLatestStash enabled
                        Console.WriteLine("Scenario 2: set 'Show latest stash' to true");
                        // Assert
                        AppSettings.ShowLatestStash.Should().BeTrue();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(6);

                        // 2. Change ShowLatestStash to disabled
                        Console.WriteLine("Scenario 2: change 'Show latest stash' to disabled");
                        form.GetTestAccessor().RevisionGrid.ToggleShowLatestStash();
                        WaitForRevisionsToBeLoaded(form);
                        // Assert
                        AppSettings.ShowLatestStash.Should().BeFalse();
                        form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);
                    }
                    finally
                    {
                        AppSettings.ShowLatestStash = showLatestStash;
                        AppSettings.RevisionGraphShowArtificialCommits = revisionGraphShowArtificialCommits;
                    }
                },
                commands);
        }

        private static void RunFormTest(Action<FormBrowse> testDriver, GitUICommands commands)
        {
            UITest.RunForm(
                showForm: () => commands.StartBrowseDialog(owner: null).Should().BeTrue(),
                (FormBrowse form) =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                });
        }

        private void RunFormTest(Action<FormBrowse> testDriver)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                });
        }

        private void RunFormTest(Func<FormBrowse, Task> testDriverAsync)
        {
            UITest.RunForm(
                showForm: () => _commands.StartBrowseDialog(owner: null).Should().BeTrue(),
                testDriverAsync);
        }

        private static void WaitForRevisionsToBeLoaded(FormBrowse form)
        {
            UITest.ProcessUntil("Loading Revisions", () => form.GetTestAccessor().RevisionGrid.GetTestAccessor().IsUiStable);
        }
    }
}
