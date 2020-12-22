using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitExtensions.UITests.UserControls.RevisionGrid
{
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class RevisionGridControlTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;
        private string _initialCommit;
        private string _headCommit;
        private string _branch1Commit;

        // Created once for each test
        private GitUICommands _commands;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            // There is no need to restore the original AppSettings because AppSettings is routed to a temp folder.

            // We don't want avatars during tests, otherwise we will be attempting to download them from gravatar.
            AppSettings.ShowAuthorAvatarColumn = false;
        }

        [SetUp]
        public void SetUp()
        {
            _referenceRepository = new ReferenceRepository();
            _initialCommit = _referenceRepository.CommitHash;

            _referenceRepository.CreateCommit("Commit1", "Commit1");
            _branch1Commit = _referenceRepository.CommitHash;
            _referenceRepository.CreateBranch("Branch1", _branch1Commit);
            _referenceRepository.CreateCommit("Commit2", "Commit2");
            _referenceRepository.CreateBranch("Branch2", _referenceRepository.CommitHash);

            _referenceRepository.CreateCommit("head commit");
            _headCommit = _referenceRepository.CommitHash;

            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [TearDown]
        public void TearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void Assert_default_filter_related_settings()
        {
            AppSettings.BranchFilterEnabled = false;
            AppSettings.ShowCurrentBranchOnly = false;

            RunSetAndApplyBranchFilterTest(
                initialFilter: "",
                revisionGridControl =>
                {
                    Assert.False(AppSettings.BranchFilterEnabled);
                    Assert.False(AppSettings.ShowCurrentBranchOnly);

                    Assert.True(revisionGridControl.IsShowAllBranchesChecked);
                    Assert.False(revisionGridControl.IsShowCurrentBranchOnlyChecked);
                    Assert.False(revisionGridControl.IsShowFilteredBranchesChecked);

                    var ta = revisionGridControl.GetTestAccessor();
                    ta.RefFilterOptions.Should().Be(RefFilterOptions.All | RefFilterOptions.Boundary | RefFilterOptions.ShowGitNotes);
                });

            RunSetAndApplyBranchFilterTest(
                initialFilter: "Branch1",
                revisionGridControl =>
                {
                    Assert.True(AppSettings.BranchFilterEnabled);
                    Assert.False(AppSettings.ShowCurrentBranchOnly);

                    Assert.False(revisionGridControl.IsShowAllBranchesChecked);
                    Assert.False(revisionGridControl.IsShowCurrentBranchOnlyChecked);
                    Assert.True(revisionGridControl.IsShowFilteredBranchesChecked);

                    var ta = revisionGridControl.GetTestAccessor();
                    ta.RefFilterOptions.Should().Be(RefFilterOptions.None);
                });
        }

        [Test]
        public void View_refect_applied_branch_filter()
        {
            AppSettings.BranchFilterEnabled = false;
            AppSettings.ShowCurrentBranchOnly = false;

            RunSetAndApplyBranchFilterTest(
                "",
                revisionGridControl =>
                {
                    var ta = revisionGridControl.GetTestAccessor();
                    Assert.False(revisionGridControl.IsShowFilteredBranchesChecked);
                    ta.VisibleRevisionCount.Should().Be(6);

                    // Verify the view hasn't changed until we refresh
                    revisionGridControl.LatestSelectedRevision.ObjectId.ToString().Should().Be(_headCommit);

                    // set filter
                    revisionGridControl.SetAndApplyBranchFilter("Branch1");
                    Assert.True(revisionGridControl.IsShowFilteredBranchesChecked);

                    // ...assert nothing changed, applying the filter doesn't change the view
                    revisionGridControl.LatestSelectedRevision.ObjectId.ToString().Should().Be(_headCommit);

                    // Refresh the grid, to reflect the filter
                    revisionGridControl.ForceRefreshRevisions();
                    DoEvents();

                    // Confirm the filter has been applied
                    revisionGridControl.LatestSelectedRevision.ObjectId.ToString().Should().Be(_branch1Commit);
                    ta.VisibleRevisionCount.Should().Be(2);
                });
        }

        [Test]
        public void View_refect_reset_branch_filter()
        {
            AppSettings.BranchFilterEnabled = false;
            AppSettings.ShowCurrentBranchOnly = false;

            RunSetAndApplyBranchFilterTest(
                "Branch1",
                revisionGridControl =>
                {
                    var ta = revisionGridControl.GetTestAccessor();
                    Assert.True(revisionGridControl.IsShowFilteredBranchesChecked);
                    ta.VisibleRevisionCount.Should().Be(2);

                    // Verify the view hasn't changed until we refresh
                    revisionGridControl.LatestSelectedRevision.ObjectId.ToString().Should().Be(_branch1Commit);

                    // reset filter
                    revisionGridControl.SetAndApplyBranchFilter("");
                    Assert.False(revisionGridControl.IsShowFilteredBranchesChecked);

                    // ...assert nothing changed, applying the filter doesn't change the view
                    revisionGridControl.LatestSelectedRevision.ObjectId.ToString().Should().Be(_branch1Commit);

                    // Refresh the grid, to reflect the filter
                    revisionGridControl.ForceRefreshRevisions();
                    DoEvents();

                    // Confirm the filter has been reset, all commits are shown
                    revisionGridControl.LatestSelectedRevision.ObjectId.ToString().Should().Be(_branch1Commit);
                    ta.VisibleRevisionCount.Should().Be(6);
                });
        }

        [Test]
        public void ToggleBetweenArtificialAndHeadCommits_no_empty([Values(false, true)] bool showGitStatusForArtificialCommits)
        {
            RunToggleBetweenArtificialAndHeadCommitsTest(
                showGitStatusForArtificialCommits,
                revisionGridControl =>
                {
                    // If showGitStatusForArtificialCommits is false, we do not update ChangeCount and HasChanges returns false.
                    // Then ToggleBetweenArtificialAndHeadCommits does not check HasChanges and toggles through all three commits.
                    var hasChangesWorkTree = revisionGridControl.GetChangeCount(ObjectId.WorkTreeId).HasChanges;
                    var hasChangesIndex = revisionGridControl.GetChangeCount(ObjectId.IndexId).HasChanges;
                    hasChangesWorkTree.Should().Be(showGitStatusForArtificialCommits);
                    hasChangesIndex.Should().Be(showGitStatusForArtificialCommits);

                    revisionGridControl.GoToRef(_initialCommit, showNoRevisionMsg: false);
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_initialCommit);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.WorkTreeGuid);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.IndexGuid);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_headCommit);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.WorkTreeGuid);
                });
        }

        [Test]
        public void ToggleBetweenArtificialAndHeadCommits_no_workdir_change([Values(false, true)] bool showGitStatusForArtificialCommits)
        {
            File.Delete(Path.Combine(_referenceRepository.Module.WorkingDir, "A.txt"));

            RunToggleBetweenArtificialAndHeadCommitsTest(
                showGitStatusForArtificialCommits,
                revisionGridControl =>
                {
                    var hasChangesWorkTree = revisionGridControl.GetChangeCount(ObjectId.WorkTreeId).HasChanges;
                    var hasChangesIndex = revisionGridControl.GetChangeCount(ObjectId.IndexId).HasChanges;
                    hasChangesWorkTree.Should().BeFalse();
                    hasChangesIndex.Should().Be(showGitStatusForArtificialCommits);

                    revisionGridControl.GoToRef(_initialCommit, showNoRevisionMsg: false);
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_initialCommit);

                    if (!showGitStatusForArtificialCommits)
                    {
                        revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                        revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.WorkTreeGuid);
                    }

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.IndexGuid);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_headCommit);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid
                        .Should().Be(showGitStatusForArtificialCommits ? GitRevision.IndexGuid : GitRevision.WorkTreeGuid);
                });
        }

        [Test]
        public void ToggleBetweenArtificialAndHeadCommits_no_index_change([Values(false, true)] bool showGitStatusForArtificialCommits)
        {
            _referenceRepository.Module.Reset(ResetMode.Hard);
            File.WriteAllText(Path.Combine(_referenceRepository.Module.WorkingDir, "new.txt"), "new");

            RunToggleBetweenArtificialAndHeadCommitsTest(
                showGitStatusForArtificialCommits,
                revisionGridControl =>
                {
                    var hasChangesWorkTree = revisionGridControl.GetChangeCount(ObjectId.WorkTreeId).HasChanges;
                    var hasChangesIndex = revisionGridControl.GetChangeCount(ObjectId.IndexId).HasChanges;
                    hasChangesWorkTree.Should().Be(showGitStatusForArtificialCommits);
                    hasChangesIndex.Should().BeFalse();

                    revisionGridControl.GoToRef(_initialCommit, showNoRevisionMsg: false);
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_initialCommit);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.WorkTreeGuid);

                    if (!showGitStatusForArtificialCommits)
                    {
                        revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                        revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.IndexGuid);
                    }

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_headCommit);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.WorkTreeGuid);
                });
        }

        [Test]
        public void ToggleBetweenArtificialAndHeadCommits_no_change([Values(false, true)] bool showGitStatusForArtificialCommits)
        {
            _referenceRepository.Module.Reset(ResetMode.Hard);

            RunToggleBetweenArtificialAndHeadCommitsTest(
                showGitStatusForArtificialCommits,
                revisionGridControl =>
                {
                    var hasChangesWorkTree = revisionGridControl.GetChangeCount(ObjectId.WorkTreeId).HasChanges;
                    var hasChangesIndex = revisionGridControl.GetChangeCount(ObjectId.IndexId).HasChanges;
                    hasChangesWorkTree.Should().BeFalse();
                    hasChangesIndex.Should().BeFalse();

                    revisionGridControl.GoToRef(_initialCommit, showNoRevisionMsg: false);
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_initialCommit);

                    if (!showGitStatusForArtificialCommits)
                    {
                        revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                        revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.WorkTreeGuid);

                        revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                        revisionGridControl.LatestSelectedRevision.Guid.Should().Be(GitRevision.IndexGuid);
                    }

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid.Should().Be(_headCommit);

                    revisionGridControl.ToggleBetweenArtificialAndHeadCommits();
                    revisionGridControl.LatestSelectedRevision.Guid
                        .Should().Be(showGitStatusForArtificialCommits ? _headCommit : GitRevision.WorkTreeGuid);
                });
        }

        private void RunSetAndApplyBranchFilterTest(string initialFilter, Action<RevisionGridControl> runTest)
        {
            UITest.RunForm<FormBrowse>(
                showForm: () => _commands.StartBrowseDialog(owner: null).Should().BeTrue(),
                runTestAsync: async formBrowse =>
                {
                    DoEvents();

                    // wait for the revisions to be loaded
                    await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                    formBrowse.RevisionGridControl.SetAndApplyBranchFilter(initialFilter);

                    // Refresh the grid, to reflect the filter
                    formBrowse.RevisionGridControl.ForceRefreshRevisions();
                    DoEvents();

                    try
                    {
                        runTest(formBrowse.RevisionGridControl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{runTest.Method.Name} failed: {ex.Demystify()}");
                        Console.WriteLine(_referenceRepository.Module.GitExecutable.GetOutput("status"));
                    }
                });
        }

        private void RunToggleBetweenArtificialAndHeadCommitsTest(bool showGitStatusForArtificialCommits, Action<RevisionGridControl> runTest)
        {
            AppSettings.ShowGitStatusForArtificialCommits = showGitStatusForArtificialCommits;

            UITest.RunForm<FormBrowse>(
                showForm: () => _commands.StartBrowseDialog(owner: null).Should().BeTrue(),
                runTestAsync: async formBrowse =>
                {
                    DoEvents();

                    // wait for the revisions to be loaded
                    await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                    formBrowse.RevisionGridControl.LatestSelectedRevision.Guid.Should().Be(_headCommit);

                    var ta = formBrowse.GetTestAccessor();
                    ta.CommitInfoTabControl.SelectedTab = ta.TreeTabPage;

                    // let the focus events be handled
                    DoEvents();

                    try
                    {
                        runTest(formBrowse.RevisionGridControl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{runTest.Method.Name} failed: {ex.Demystify()}");
                        Console.WriteLine(_referenceRepository.Module.GitExecutable.GetOutput("status"));
                    }

                    // let the focus events be handled
                    DoEvents();

                    Assert.IsTrue(ta.CommitInfoTabControl.SelectedTab == ta.DiffTabPage, "Diff tab should be active");
                });
        }

        private static void DoEvents()
        {
            for (int i = 0; i < 5; ++i)
            {
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }
    }
}
