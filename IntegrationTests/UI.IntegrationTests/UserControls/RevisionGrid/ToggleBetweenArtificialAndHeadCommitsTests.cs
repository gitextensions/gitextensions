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
    public class ToggleBetweenArtificialAndHeadCommitsTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;
        private string _initialCommit;
        private string _headCommit;

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

                    return;

                    static void DoEvents()
                    {
                        for (int i = 0; i < 10; ++i)
                        {
                            Thread.Sleep(100);
                            Application.DoEvents();
                        }
                    }
                });
        }
    }
}
