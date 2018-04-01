using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;
using Path = System.IO.Path;

namespace GitUITests.CommandsDialogs.CommitDialog
{
    [Apartment(ApartmentState.STA)]
    public class FormCommitTests
    {
        // Created once for the fixture
        private GitModuleTestHelper _moduleTestHelper;
        private string _commitHash;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            if (_moduleTestHelper == null)
            {
                _moduleTestHelper = new GitModuleTestHelper();

                using (var repository = new LibGit2Sharp.Repository(_moduleTestHelper.Module.WorkingDir))
                {
                    _moduleTestHelper.CreateRepoFile("A.txt", "A");
                    repository.Index.Add("A.txt");

                    var message = "A commit message";
                    var author = new LibGit2Sharp.Signature("GitUITests", "unittests@gitextensions.com", DateTimeOffset.Now);
                    var committer = author;
                    var options = new LibGit2Sharp.CommitOptions();
                    var commit = repository.Commit(message, author, committer, options);
                    _commitHash = commit.Id.Sha;
                }
            }
            else
            {
                // Undo potential impact from earlier tests
                using (var repository = new LibGit2Sharp.Repository(_moduleTestHelper.Module.WorkingDir))
                {
                    var options = new LibGit2Sharp.CheckoutOptions();
                    repository.Reset(LibGit2Sharp.ResetMode.Hard, (LibGit2Sharp.Commit)repository.Lookup(_commitHash, LibGit2Sharp.ObjectType.Commit), options);
                    repository.RemoveUntrackedFiles();
                }
            }

            CommitHelper.SetCommitMessage(_moduleTestHelper.Module, commitMessageText: null, amendCommit: false);

            _commands = new GitUICommands(_moduleTestHelper.Module);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _moduleTestHelper.Dispose();
        }

        [Test]
        public void PreserveCommitMessageOnReopen()
        {
            var generatedCommitMessage = Path.GetRandomFileName();

            RunFormCommitTest(formCommit =>
            {
                Assert.IsEmpty(formCommit.GetTestAccessor().Message.Text);
                formCommit.GetTestAccessor().Message.Text = generatedCommitMessage;
            });

            RunFormCommitTest(formCommit =>
            {
                Assert.AreEqual(generatedCommitMessage, formCommit.GetTestAccessor().Message.Text);
            });
        }

        [TestCase(CommitKind.Fixup)]
        [TestCase(CommitKind.Squash)]
        public void DoNotPreserveCommitMessageOnReopenFromSpecialCommit(CommitKind commitKind)
        {
            var generatedCommitMessage = Path.GetRandomFileName();

            RunFormCommitTest(
                formCommit =>
                {
                    string prefix = commitKind.ToString().ToLowerInvariant();
                    Assert.AreEqual($"{prefix}! A commit message", formCommit.GetTestAccessor().Message.Text);
                    formCommit.GetTestAccessor().Message.Text = generatedCommitMessage;
                },
                commitKind);

            RunFormCommitTest(formCommit =>
            {
                Assert.IsEmpty(formCommit.GetTestAccessor().Message.Text);
            });
        }

        [Test]
        public void SelectMessageFromHistory()
        {
            var generatedCommitMessage = Path.GetRandomFileName();

            RunFormCommitTest(formCommit =>
            {
                var commitMessageToolStripMenuItem = formCommit.GetTestAccessor().CommitMessageToolStripMenuItem;

                // Verify the message appears correctly
                commitMessageToolStripMenuItem.ShowDropDown();
                Assert.AreEqual("A commit message", commitMessageToolStripMenuItem.DropDownItems[0].Text);

                // Verify the message is selected correctly
                commitMessageToolStripMenuItem.DropDownItems[0].PerformClick();
                Assert.AreEqual("A commit message", formCommit.GetTestAccessor().Message.Text);
            });
        }

        private void RunFormCommitTest(Action<FormCommit> testDriver, CommitKind commitKind = CommitKind.Normal)
        {
            RunFormCommitTest(
                formCommit =>
                {
                    testDriver(formCommit);
                    return Task.CompletedTask;
                },
                commitKind);
        }

        private void RunFormCommitTest(Func<FormCommit, Task> testDriverAsync, CommitKind commitKind = CommitKind.Normal)
        {
            UITest.RunForm(
                () =>
                {
                    switch (commitKind)
                    {
                    case CommitKind.Normal:
                        Assert.True(_commands.StartCommitDialog());
                        break;

                    case CommitKind.Squash:
                        Assert.True(_commands.StartSquashCommitDialog(owner: null, _referenceRepository.Module.GetRevision("HEAD")));
                        break;

                    case CommitKind.Fixup:
                        Assert.True(_commands.StartFixupCommitDialog(owner: null, _referenceRepository.Module.GetRevision("HEAD")));
                        break;

                    default:
                        throw new ArgumentException($"Unsupported commit kind: {commitKind}", nameof(commitKind));
                    }
                },
                testDriverAsync);
        }
    }
}
