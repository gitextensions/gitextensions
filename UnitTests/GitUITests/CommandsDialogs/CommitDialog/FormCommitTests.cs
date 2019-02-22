﻿using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitUI;
using GitUI.CommandsDialogs;
using ICSharpCode.TextEditor;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.CommitDialog
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

        [TearDown]
        public void TearDown()
        {
            _commands = null;
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
            RunFormTest(form =>
            {
                var commitAuthorStatus = form.GetTestAccessor().CommitAuthorStatusToolStripStatusLabel;

                Assert.AreEqual("Committer author <author@mail.com>", commitAuthorStatus.Text);

                using (var tempForm = new Form())
                {
                    tempForm.Show();
                    tempForm.Focus();

                    _referenceRepository.Module.GitExecutable.GetOutput(@"config user.name ""new author""");
                    _referenceRepository.Module.GitExecutable.GetOutput(@"config user.email ""new_author@mail.com""");
                }

                form.Focus();

                Assert.AreEqual("Committer new author <new_author@mail.com>", commitAuthorStatus.Text);
            });
        }

        [Test]
        [Ignore("Fail on the CI server (because what is asserted is done in another not awaited task)")]
        public void Should_display_branch_and_no_remote_info_in_statusbar()
        {
            using (var repository = new ReferenceRepository(new GitModuleTestHelper("branch_and_no_remote")))
            {
                var commands = new GitUICommands(repository.Module);
                repository.CheckoutMaster();
                UITest.RunForm<FormCommit>(
                    () => { Assert.True(commands.StartCommitDialog(owner: null)); },
                    form =>
                    {
                        var currentBranchNameLabelStatus = form.GetTestAccessor().CurrentBranchNameLabelStatus;
                        var remoteNameLabelStatus = form.GetTestAccessor().RemoteNameLabelStatus;

                        Assert.AreEqual("master →", currentBranchNameLabelStatus.Text);
                        Assert.AreEqual("(remote not configured)", remoteNameLabelStatus.Text);
                        return Task.CompletedTask;
                    });
            }
        }

        [Test]
        public void Should_display_detached_head_info_in_statusbar()
        {
            using (var repository = new ReferenceRepository(new GitModuleTestHelper("detached_head")))
            {
                var commands = new GitUICommands(repository.Module);
                repository.CheckoutRevision();
                UITest.RunForm<FormCommit>(
                    () => { Assert.True(commands.StartCommitDialog(owner: null)); },
                    form =>
                    {
                        var currentBranchNameLabelStatus = form.GetTestAccessor().CurrentBranchNameLabelStatus;
                        var remoteNameLabelStatus = form.GetTestAccessor().RemoteNameLabelStatus;

                        Assert.AreEqual("(no branch)", currentBranchNameLabelStatus.Text);
                        Assert.AreEqual(string.Empty, remoteNameLabelStatus.Text);
                        return Task.CompletedTask;
                    });
            }
        }

        [Test]
        public void Should_display_branch_and_remote_info_in_statusbar()
        {
            using (var repository = new ReferenceRepository(new GitModuleTestHelper("branch_and_remote")))
            {
                var commands = new GitUICommands(repository.Module);
                repository.CreateRemoteForMasterBranch();
                UITest.RunForm<FormCommit>(
                    () => { Assert.True(commands.StartCommitDialog(owner: null)); },
                    form =>
                {
                    var currentBranchNameLabelStatus = form.GetTestAccessor().CurrentBranchNameLabelStatus;
                    var remoteNameLabelStatus = form.GetTestAccessor().RemoteNameLabelStatus;

                    Assert.AreEqual("master →", currentBranchNameLabelStatus.Text);
                    Assert.AreEqual("origin/master", remoteNameLabelStatus.Text);
                    return Task.CompletedTask;
                });
            }
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
                () =>
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