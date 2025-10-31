using System.Runtime.CompilerServices;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

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
    }

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();

        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _referenceRepository.Dispose();
    }

    [Test]
    public void Filters_should_behave_as_expected()
    {
        _referenceRepository.CreateCommit("Commit1", "Commit1");
        _referenceRepository.CreateBranch("Branch1", _referenceRepository.CommitHash);
        _referenceRepository.CreateCommit("Commit2", "Commit2");
        _referenceRepository.CreateBranch("Branch2", _referenceRepository.CommitHash);

        _referenceRepository.CreateCommit("head commit");

        _referenceRepository.CheckoutBranch("Branch1");

        bool reflogEnabled = AppSettings.ShowReflogReferences;
        bool branchFilterEnabled = AppSettings.BranchFilterEnabled;
        bool showCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
        bool revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        AppSettings.ShowReflogReferences.Value = false;
        AppSettings.BranchFilterEnabled.Value = false;
        AppSettings.ShowCurrentBranchOnly.Value = false;
        AppSettings.RevisionGraphShowArtificialCommits = false;

        RunFormTest(
            form =>
            {
                try
                {
                    // 1. Cycle between "Show all branches" > "Show current branch" > "Show filterd branches"
                    // ------------------------------------------------------------------------------------------------

                    Console.WriteLine("Scenario 1: set 'Show all branches'");
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);
                    AppSettings.BranchFilterEnabled.Value.Should().BeFalse();
                    AppSettings.ShowCurrentBranchOnly.Value.Should().BeFalse();

                    Console.WriteLine("Scenario 1: set 'Show current branch'");
                    form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tsmiShowBranchesCurrent.PerformClick();
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.BranchFilterEnabled.Value.Should().BeFalse();
                    AppSettings.ShowCurrentBranchOnly.Value.Should().BeTrue();
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(2);

                    Console.WriteLine("Scenario 1: set 'Show filtered branches'");
                    form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tsmiShowBranchesFiltered.PerformClick();
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.BranchFilterEnabled.Value.Should().BeTrue();
                    AppSettings.ShowCurrentBranchOnly.Value.Should().BeFalse();
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);

                    // 2. Apply a branch filter
                    // ------------------------------------------------------------------------------------------------

                    Console.WriteLine("Scenario 2: apply branch filter 'Branch2'");
                    form.GetTestAccessor().ToolStripFilters.SetBranchFilter("Branch2");
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.BranchFilterEnabled.Value.Should().BeTrue();
                    AppSettings.ShowCurrentBranchOnly.Value.Should().BeFalse();
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(3);

                    Console.WriteLine("Scenario 2: set 'Show current branch'");
                    form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tsmiShowBranchesCurrent.PerformClick();
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.BranchFilterEnabled.Value.Should().BeFalse();
                    AppSettings.ShowCurrentBranchOnly.Value.Should().BeTrue();
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
                    AppSettings.BranchFilterEnabled.Value.Should().BeFalse();
                    AppSettings.ShowCurrentBranchOnly.Value.Should().BeTrue();
                    form.GetTestAccessor().ToolStripFilters.GetTestAccessor().tscboBranchFilter.Text.Should().BeEmpty();
                }
                finally
                {
                    AppSettings.ShowReflogReferences.Value = reflogEnabled;
                    AppSettings.BranchFilterEnabled.Value = branchFilterEnabled;
                    AppSettings.ShowCurrentBranchOnly.Value = showCurrentBranchOnly;
                    AppSettings.RevisionGraphShowArtificialCommits = revisionGraphShowArtificialCommits;
                }
            });
    }

    [TestCase("", "file.txt")]
    [TestCase("", "file with spaces.txt")]
    [TestCase("Dir with spaces", "file.txt")]
    [TestCase("Dir with spaces", "file with spaces.txt")]
    public void File_history_should_behave_as_expected(string fileRelativePath, string fileName)
    {
        using ReferenceRepository referenceRepository = new();
        GitUICommands commands = new(GlobalServiceContainer.CreateDefaultMockServiceContainer(), referenceRepository.Module);

        string revision1 = referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Create '{fileName}' in directory '{fileRelativePath}'");
        string revision2 = referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Update '{fileName}' in directory '{fileRelativePath}'");
        string revision3 = referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Update '{fileName}' in directory '{fileRelativePath}' again");

        bool useBrowseForFileHistory = AppSettings.UseBrowseForFileHistory.Value;

        AppSettings.UseBrowseForFileHistory.Value = true;

        UITest.RunForm(
            showForm: () => commands.GetTestAccessor().ShowFileHistoryDialog(Path.Combine(fileRelativePath, fileName)),
            (FormBrowse form) =>
            {
                try
                {
                    WaitForRevisionsToBeLoaded(form);

                    form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision1)).Should().NotBeNull();
                    form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision2)).Should().NotBeNull();
                    form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision3)).Should().NotBeNull();

                    return Task.CompletedTask;
                }
                finally
                {
                    AppSettings.UseBrowseForFileHistory.Value = useBrowseForFileHistory;
                }
            });
    }

    [Test]
    public void FileSelection_should_be_restored_after_refresh()
    {
        // create a commit with multiple files changed
        const string fileA = "fileA.txt";
        const string fileB = "fileB.txt";
        const string contentA = nameof(contentA);
        const string contentB = nameof(contentB);

        using ReferenceRepository referenceRepository = new();
        GitUICommands commands = new(GlobalServiceContainer.CreateDefaultMockServiceContainer(), referenceRepository.Module);
        referenceRepository.CreateCommit("multiple files",
            $"{contentA}\n{new string('A', 20000)}", fileA,
            $"{contentB}\n{new string('B', 20000)}", fileB);

        const int maxMilliseconds = 1_000;
        RunFormTest(
            async form =>
            {
                FormBrowse.TestAccessor ta = form.GetTestAccessor();
                ((TabControl)ta.DiffTabPage.Parent).SelectedTab = ta.DiffTabPage;

                RevisionDiffControl.TestAccessor tadiff = ta.RevisionDiffControl.GetTestAccessor();
                FileStatusList fileStatusList = tadiff.DiffFiles;

                WaitForRevisionsToBeLoaded(form);

                IReadOnlyList<GitItemStatus> files = [];
                UITest.ProcessUntil("waiting for files", () => { return (files = fileStatusList.GitItemStatuses).Count == 2; }, maxMilliseconds);
                GitItemStatus fileToSelect = files[1];
                fileStatusList.SelectedGitItems = [fileToSelect];
                await VerifySelectionAsync();
                const int expectedLine = 2;
                tadiff.DiffText.GoToLine(expectedLine);

                // repeat: refresh and check selection
                for (int i = 0; i < 15; ++i)
                {
                    ta.RefreshRevisions();
                    WaitForRevisionsToBeLoaded(form);
                    await VerifySelectionAsync(expectedLine);
                }

                return;

                async Task VerifySelectionAsync(int expectedLine = 1)
                {
                    await Task.Delay(FileStatusList.SelectedIndexChangeThrottleDuration);
                    UITest.ProcessUntil("waiting for selection",
                        () => fileStatusList.SelectedGitItem?.Name == fileB
                                && tadiff.DiffText.GetText().StartsWith(contentB)
                                && tadiff.DiffText.CurrentFileLine == expectedLine,
                        maxMilliseconds);
                }
            },
            commands);
    }

    [Test]
    public void ShowStashes_starting_disabled_should_filter_as_expected()
    {
        using ReferenceRepository referenceRepository = new();
        GitUICommands commands = new(GlobalServiceContainer.CreateDefaultMockServiceContainer(), referenceRepository.Module);

        referenceRepository.CreateCommit("Commit1");
        referenceRepository.Stash("Stash1");
        referenceRepository.CreateCommit("Commit2");
        referenceRepository.Stash("Stash2");
        referenceRepository.CreateCommit("Commit3");

        bool showStashes = AppSettings.ShowStashes;
        bool revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;

        AppSettings.ShowStashes = false;
        AppSettings.RevisionGraphShowArtificialCommits = false;

        RunFormTest(
            form =>
            {
                try
                {
                    // 1. Check with ShowStashes disabled
                    Console.WriteLine("Scenario 1: set 'Show stashes' to false");
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.ShowStashes.Should().BeFalse();
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);

                    // 2. Change ShowStashes to enabled
                    Console.WriteLine("Scenario 2: change 'Show stashes' to enabled");
                    form.GetTestAccessor().RevisionGrid.ToggleShowStashes();
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.ShowStashes.Should().BeTrue();
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(7);
                }
                finally
                {
                    AppSettings.ShowStashes = showStashes;
                    AppSettings.RevisionGraphShowArtificialCommits = revisionGraphShowArtificialCommits;
                }
            },
            commands);
    }

    [Test]
    public void ShowStashes_starting_enabled_should_filter_as_expected()
    {
        using ReferenceRepository referenceRepository = new();
        GitUICommands commands = new(GlobalServiceContainer.CreateDefaultMockServiceContainer(), referenceRepository.Module);

        referenceRepository.CreateCommit("Commit1");
        referenceRepository.Stash("Stash1");
        referenceRepository.CreateCommit("Commit2");
        referenceRepository.Stash("Stash2");
        referenceRepository.CreateCommit("Commit3");

        bool showStashes = AppSettings.ShowStashes;
        bool revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;

        AppSettings.ShowStashes = true;
        AppSettings.RevisionGraphShowArtificialCommits = false;
        RunFormTest(
            form =>
            {
                try
                {
                    // 1. Check with ShowStashes enabled
                    Console.WriteLine("Scenario 1: set 'Show stash' to true");
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.ShowStashes.Should().BeTrue();
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(7);

                    // 2. Change ShowStashes to disabled
                    Console.WriteLine("Scenario 2: change 'Show stash' to disabled");
                    form.GetTestAccessor().RevisionGrid.ToggleShowStashes();
                    WaitForRevisionsToBeLoaded(form);
                    // Assert
                    AppSettings.ShowStashes.Should().BeFalse();
                    // https://github.com/gitextensions/gitextensions/issues/10170
                    // This test occasionaly fails with 3 visible revisions
                    form.GetTestAccessor().RevisionGrid.GetTestAccessor().VisibleRevisionCount.Should().Be(4);
                }
                finally
                {
                    AppSettings.ShowStashes = showStashes;
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

    private void RunFormTest(Func<FormBrowse, Task> testDriverAsync, GitUICommands? commands = null)
    {
        UITest.RunForm(
            showForm: () => (commands ?? _commands).StartBrowseDialog(owner: null).Should().BeTrue(),
            testDriverAsync);
    }

    private static void WaitForRevisionsToBeLoaded(FormBrowse form, [CallerMemberName] string caller = "")
    {
        UITest.ProcessUntil($"{caller} loading revisions", () => form.GetTestAccessor().RevisionGrid.GetTestAccessor().IsDataLoadComplete, maxMilliseconds: 10_000);
    }
}
