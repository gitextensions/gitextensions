using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitUI;
using GitUI.CommandsDialogs;
using ICSharpCode.TextEditor;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormCommitTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            if (_referenceRepository == null)
            {
                _referenceRepository = new ReferenceRepository();
            }
            else
            {
                _referenceRepository.Reset();
            }

            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void Should_show_committer_info_on_open()
        {
            RunFormTest(form =>
            {
                var commitAuthorStatus = form.GetTestAccessor().CommitAuthorStatusToolStripStatusLabel;

                Assert.AreEqual("Committer author <author@mail.com>", commitAuthorStatus.Text);
            });
        }

        [Test]
        public void Should_update_committer_info_on_form_activated()
        {
            RunFormTest(async form =>
            {
                var commitAuthorStatus = form.GetTestAccessor().CommitAuthorStatusToolStripStatusLabel;

                Assert.AreEqual("Committer author <author@mail.com>", commitAuthorStatus.Text);

                using (var tempForm = new Form())
                {
                    tempForm.Owner = form;
                    tempForm.Show();
                    tempForm.Focus();

                    _referenceRepository.Module.GitExecutable.GetOutput(@"config user.name ""new author""");
                    _referenceRepository.Module.GitExecutable.GetOutput(@"config user.email ""new_author@mail.com""");
                }

                form.Focus();

                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                Assert.AreEqual("Committer new author <new_author@mail.com>", commitAuthorStatus.Text);
            });
        }

        [Test]
        public void Should_display_branch_and_no_remote_info_in_statusbar()
        {
            _referenceRepository.CheckoutMaster();
            RunFormTest(async form =>
            {
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                var currentBranchNameLabelStatus = form.GetTestAccessor().CurrentBranchNameLabelStatus;
                var remoteNameLabelStatus = form.GetTestAccessor().RemoteNameLabelStatus;

                Assert.AreEqual("master →", currentBranchNameLabelStatus.Text);
                Assert.AreEqual("(remote not configured)", remoteNameLabelStatus.Text);
            });
        }

        [Test]
        public void Should_display_detached_head_info_in_statusbar()
        {
            _referenceRepository.CheckoutRevision();
            RunFormTest(async form =>
            {
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                var currentBranchNameLabelStatus = form.GetTestAccessor().CurrentBranchNameLabelStatus;
                var remoteNameLabelStatus = form.GetTestAccessor().RemoteNameLabelStatus;

                // For a yet unknown cause randomly, the wait in UITest.RunForm does not suffice.
                if (!string.IsNullOrEmpty(remoteNameLabelStatus.Text))
                {
                    Console.WriteLine($"{nameof(Should_display_detached_head_info_in_statusbar)} waits again");
                    await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                }

                Assert.AreEqual("(no branch)", currentBranchNameLabelStatus.Text);
                Assert.AreEqual(string.Empty, remoteNameLabelStatus.Text);
            });
        }

        [Test]
        public void Should_display_branch_and_remote_info_in_statusbar()
        {
            _referenceRepository.CreateRemoteForMasterBranch();
            RunFormTest(async form =>
            {
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                var currentBranchNameLabelStatus = form.GetTestAccessor().CurrentBranchNameLabelStatus;
                var remoteNameLabelStatus = form.GetTestAccessor().RemoteNameLabelStatus;

                Assert.AreEqual("master →", currentBranchNameLabelStatus.Text);
                Assert.AreEqual("origin/master", remoteNameLabelStatus.Text);
            });
        }

        [Test]
        public void PreserveCommitMessageOnReopen()
        {
            var generatedCommitMessage = Guid.NewGuid().ToString();

            RunFormTest(form =>
            {
                Assert.IsEmpty(form.GetTestAccessor().Message.Text);
                form.GetTestAccessor().Message.Text = generatedCommitMessage;
            });

            RunFormTest(form =>
            {
                Assert.AreEqual(generatedCommitMessage, form.GetTestAccessor().Message.Text);
            });
        }

        [TestCase(CommitKind.Fixup)]
        [TestCase(CommitKind.Squash)]
        public void DoNotPreserveCommitMessageOnReopenFromSpecialCommit(CommitKind commitKind)
        {
            var generatedCommitMessage = Guid.NewGuid().ToString();

            RunFormTest(
                form =>
                {
                    string prefix = commitKind.ToString().ToLowerInvariant();
                    Assert.AreEqual($"{prefix}! A commit message", form.GetTestAccessor().Message.Text);
                    form.GetTestAccessor().Message.Text = generatedCommitMessage;
                },
                commitKind);

            RunFormTest(form =>
            {
                Assert.IsEmpty(form.GetTestAccessor().Message.Text);
            });
        }

        [Test]
        public void SelectMessageFromHistory()
        {
            RunFormTest(form =>
            {
                var commitMessageToolStripMenuItem = form.GetTestAccessor().CommitMessageToolStripMenuItem;

                // Verify the message appears correctly
                commitMessageToolStripMenuItem.ShowDropDown();
                Assert.AreEqual("A commit message", commitMessageToolStripMenuItem.DropDownItems[0].Text);

                // Verify the message is selected correctly
                commitMessageToolStripMenuItem.DropDownItems[0].PerformClick();
                Assert.AreEqual("A commit message", form.GetTestAccessor().Message.Text);
            });
        }

        [Test]
        public void Should_handle_well_commit_message_in_commit_message_menu()
        {
            _referenceRepository.CreateCommit("Only first line\n\nof a multi-line commit message\nmust be displayed in the menu");
            _referenceRepository.CreateCommit("Too long commit message that should be shorten because first line of a commit message is only 50 chars long");
            RunFormTest(form =>
            {
                var commitMessageToolStripMenuItem = form.GetTestAccessor().CommitMessageToolStripMenuItem;

                // Verify the message appears correctly
                commitMessageToolStripMenuItem.ShowDropDown();
                Assert.AreEqual("Too long commit message that should be shorten because first line of ...", commitMessageToolStripMenuItem.DropDownItems[0].Text);
                Assert.AreEqual("Only first line", commitMessageToolStripMenuItem.DropDownItems[1].Text);
            });
        }

        [Test, TestCaseSource(typeof(CommitMessageTestData), "TestCases")]
        public void AddSelectionToCommitMessage_shall_be_ignored_unless_diff_is_focused(
            string message,
            int selectionStart,
            int selectionLength,
            string expectedMessage,
            int expectedSelectionStart)
        {
            TestAddSelectionToCommitMessage(focusSelectedDiff: false, CommitMessageTestData.SelectedText,
                message, selectionStart, selectionLength,
                expectedResult: false, expectedMessage: message, expectedSelectionStart: selectionStart);
        }

        [Test, TestCaseSource(typeof(CommitMessageTestData), "TestCases")]
        public void AddSelectionToCommitMessage_shall_be_ignored_if_no_difftext_is_selected(
            string message,
            int selectionStart,
            int selectionLength,
            string expectedMessage,
            int expectedSelectionStart)
        {
            TestAddSelectionToCommitMessage(focusSelectedDiff: true, selectedText: "",
                message, selectionStart, selectionLength,
                expectedResult: false, expectedMessage: message, expectedSelectionStart: selectionStart);
        }

        [Test, TestCaseSource(typeof(CommitMessageTestData), "TestCases")]
        public void AddSelectionToCommitMessage_shall_modify_the_commit_message(
            string message,
            int selectionStart,
            int selectionLength,
            string expectedMessage,
            int expectedSelectionStart)
        {
            TestAddSelectionToCommitMessage(focusSelectedDiff: true, CommitMessageTestData.SelectedText,
                message, selectionStart, selectionLength,
                expectedResult: true, expectedMessage, expectedSelectionStart);
        }

        [Test]
        public void editFileToolStripMenuItem_Click_no_selection_should_not_throw()
        {
            RunFormTest(async form =>
            {
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                form.GetTestAccessor().UnstagedList.ClearSelected();

                var editFileToolStripMenuItem = form.GetTestAccessor().EditFileToolStripMenuItem;

                // asserting by the virtue of not crashing
                editFileToolStripMenuItem.PerformClick();
            });
        }

        [Test]
        public void Dialog_remembers_window_geometry()
        {
            RunGeometryMemoryTest(
                form => form.GetTestAccessor().Bounds,
                (bounds1, bounds2) => bounds2.Should().Be(bounds1));
        }

        [Test]
        public void MessageEdit_remembers_geometry()
        {
            RunGeometryMemoryTest(
                form => form.GetTestAccessor().Message.Bounds,
                (bounds1, bounds2) => bounds2.Should().Be(bounds1));
        }

        [Test]
        public void UnstagedList_remembers_geometry()
        {
            RunGeometryMemoryTest(
                form => form.GetTestAccessor().UnstagedList.Bounds,
                (bounds1, bounds2) =>
                {
                    bounds2.Width.Should().Be(bounds1.Width);

                    // The method to determine the height is prone to rounding errors.
                    // This seems not to affect the user experience, because
                    // - the rounding error is only +- 1 pixel
                    // - if the user does not change the geometry, the height will oscillate to a constant value
                    var height1 = bounds1.Height;
                    var height2 = bounds2.Height;
                    Assert.IsTrue(height1 >= height2 - 1 && height1 <= height2 + 1);
                });
        }

        [Test]
        public void SelectedDiff_remembers_geometry()
        {
            RunGeometryMemoryTest(
                form => form.GetTestAccessor().SelectedDiff.Bounds,
                (bounds1, bounds2) => bounds2.Should().Be(bounds1));
        }

        private void TestAddSelectionToCommitMessage(
            bool focusSelectedDiff,
            string selectedText,
            string message,
            int selectionStart,
            int selectionLength,
            bool expectedResult,
            string expectedMessage,
            int expectedSelectionStart)
        {
            RunFormTest(form =>
            {
                var ta = form.GetTestAccessor();

                var selectedDiff = ta.SelectedDiff.GetTestAccessor().FileViewerInternal;
                selectedDiff.SetText(selectedText, openWithDifftool: null);
                selectedDiff.GetTestAccessor().TextEditor.ActiveTextAreaControl.SelectionManager.SetSelection(
                    new TextLocation(0, 0), new TextLocation(selectedText.Length, 0));
                if (focusSelectedDiff)
                {
                    selectedDiff.Focus();
                }

                ta.Message.Text = message;
                ta.Message.SelectionStart = selectionStart;
                ta.Message.SelectionLength = selectionLength;
                ta.ExecuteCommand(FormCommit.Command.AddSelectionToCommitMessage).Should().Be((GitCommands.CommandStatus)expectedResult);
                ta.Message.Text.Should().Be(expectedMessage);
                ta.Message.SelectionStart.Should().Be(expectedSelectionStart);
                ta.Message.SelectionLength.Should().Be(expectedResult ? 0 : selectionLength);
            });
        }

        private void RunGeometryMemoryTest(Func<FormCommit, Rectangle> boundsAccessor, Action<Rectangle, Rectangle> testDriver)
        {
            var bounds1 = Rectangle.Empty;
            var bounds2 = Rectangle.Empty;
            RunFormTest(form => bounds1 = boundsAccessor(form));
            RunFormTest(form => bounds2 = boundsAccessor(form));
            testDriver(bounds1, bounds2);
        }

        private void RunFormTest(Action<FormCommit> testDriver, CommitKind commitKind = CommitKind.Normal)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                },
                commitKind);
        }

        private void RunFormTest(Func<FormCommit, Task> testDriverAsync, CommitKind commitKind = CommitKind.Normal)
        {
            UITest.RunForm(
                showForm: () =>
                {
                    switch (commitKind)
                    {
                        case CommitKind.Normal:
                            Assert.True(_commands.StartCommitDialog(owner: null));
                            break;

                        case CommitKind.Squash:
                            Assert.True(_commands.StartSquashCommitDialog(owner: null, _referenceRepository.Module.GetRevision()));
                            break;

                        case CommitKind.Fixup:
                            Assert.True(_commands.StartFixupCommitDialog(owner: null, _referenceRepository.Module.GetRevision()));
                            break;

                        default:
                            throw new ArgumentException($"Unsupported commit kind: {commitKind}", nameof(commitKind));
                    }
                },
                testDriverAsync);
        }
    }

    public class CommitMessageTestData
    {
        internal const string SelectedText = "selection";
        private const string SelectedTextWithNewLine = SelectedText + "\n";

        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData("", 0, 0, SelectedTextWithNewLine, 0 + SelectedTextWithNewLine.Length);
                yield return new TestCaseData("msg", 0, 0, SelectedTextWithNewLine + "msg", 0 + SelectedTextWithNewLine.Length);
                yield return new TestCaseData("msg", 1, 0, "m" + SelectedTextWithNewLine + "sg", 1 + SelectedTextWithNewLine.Length);
                yield return new TestCaseData("msg", 2, 0, "ms" + SelectedTextWithNewLine + "g", 2 + SelectedTextWithNewLine.Length);
                yield return new TestCaseData("msg", 3, 0, "msg" + SelectedTextWithNewLine, 3 + SelectedTextWithNewLine.Length);
                yield return new TestCaseData("msg", 0, 1, "" + SelectedText + "sg", 0 + SelectedText.Length);
                yield return new TestCaseData("msg", 1, 1, "m" + SelectedText + "g", 1 + SelectedText.Length);
                yield return new TestCaseData("msg", 2, 1, "ms" + SelectedText, 2 + SelectedText.Length);
            }
        }
    }
}
