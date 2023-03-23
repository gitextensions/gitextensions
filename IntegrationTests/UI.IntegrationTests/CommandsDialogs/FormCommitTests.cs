using System.Collections;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using ICSharpCode.TextEditor;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormCommitTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Track the original setting value
        private bool _showAvailableDiffTools;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Remember the current setting...
            _showAvailableDiffTools = AppSettings.ShowAvailableDiffTools;

            // ...and stop loading custom diff tools
            AppSettings.ShowAvailableDiffTools = false;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAvailableDiffTools = _showAvailableDiffTools;
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

                using (Form tempForm = new())
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
            _referenceRepository.CheckoutBranch("master");
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
        public void PreserveCommitMessageOnReopenFromAmendCommit()
        {
            var oldCommitMessage = _referenceRepository.Module.GetRevision().Body;
            var newCommitMessageWithAmend = $"amend! {oldCommitMessage}\n\nNew commit message";

            RunFormTest(
                form =>
                {
                    Assert.AreEqual($"amend! {oldCommitMessage}\n\n{oldCommitMessage}", form.GetTestAccessor().Message.Text);
                    form.GetTestAccessor().Message.Text = newCommitMessageWithAmend;
                },
                CommitKind.Amend);

            RunFormTest(form =>
            {
                Assert.AreEqual(newCommitMessageWithAmend, form.GetTestAccessor().Message.Text);
            });
        }

        [Test]
        public void SelectMessageFromHistory()
        {
            const string lastCommitMessage = "last commit message";
            AppSettings.LastCommitMessage = lastCommitMessage;

            RunFormTest(form =>
            {
                var commitMessageToolStripMenuItem = form.GetTestAccessor().CommitMessageToolStripMenuItem;

                // Verify the message appears correctly
                commitMessageToolStripMenuItem.ShowDropDown();
                commitMessageToolStripMenuItem.DropDownItems[0].Text.Should().Be(lastCommitMessage);

                // Verify the message is selected correctly
                commitMessageToolStripMenuItem.DropDownItems[0].PerformClick();
                form.GetTestAccessor().Message.Text.Should().Be(lastCommitMessage);
            });
        }

        [Test]
        public void Should_handle_well_commit_message_in_commit_message_menu()
        {
            const string lastCommitMessage = "last commit message";
            AppSettings.LastCommitMessage = lastCommitMessage;

            _referenceRepository.CreateCommit("Only first line\n\nof a multi-line commit message\nmust be displayed in the menu");
            _referenceRepository.CreateCommit("Too long commit message that should be shorten because first line of a commit message is only 50 chars long");
            RunFormTest(form =>
            {
                var commitMessageToolStripMenuItem = form.GetTestAccessor().CommitMessageToolStripMenuItem;

                // Verify the message appears correctly
                commitMessageToolStripMenuItem.ShowDropDown();
                commitMessageToolStripMenuItem.DropDownItems[0].Text.Should().Be(lastCommitMessage);
                commitMessageToolStripMenuItem.DropDownItems[1].Text.Should().Be("Too long commit message that should be shorten because first line of ...");
                commitMessageToolStripMenuItem.DropDownItems[2].Text.Should().Be("Only first line");
            });
        }

        [SetCulture("en-US")]
        [SetUICulture("en-US")]
        [Test]
        public void Should_stage_only_filtered_on_StageAll()
        {
            _referenceRepository.CreateRepoFile("file1A.txt", "Test");
            _referenceRepository.CreateRepoFile("file1B.txt", "Test");
            _referenceRepository.CreateRepoFile("file2.txt", "Test");

            RunFormTest(async form =>
            {
                using (CancellationTokenSource cts = new(AsyncTestHelper.UnexpectedTimeout))
                {
                    await ThreadHelper.JoinPendingOperationsAsync(cts.Token);
                }

                Assert.AreEqual("Stage all", form.GetTestAccessor().StageAllToolItem.ToolTipText);
            });

            RunFormTest(async form =>
            {
                using (CancellationTokenSource cts = new(AsyncTestHelper.UnexpectedTimeout))
                {
                    await ThreadHelper.JoinPendingOperationsAsync(cts.Token);
                }

                var testform = form.GetTestAccessor();

                testform.UnstagedList.ClearSelected();
                testform.UnstagedList.SetFilter("file1");

                Assert.AreEqual("Stage filtered", testform.StageAllToolItem.ToolTipText);

                testform.StageAllToolItem.PerformClick();

                var fileNotMatchedByFilterIsStillUnstaged = testform.UnstagedList.AllItems.Any(i => i.Item.Name == "file2.txt");

                Assert.AreEqual(2, testform.StagedList.AllItemsCount);
                Assert.AreEqual(1, testform.UnstagedList.AllItemsCount);
                Assert.IsTrue(fileNotMatchedByFilterIsStillUnstaged);
            });
        }

        [SetCulture("en-US")]
        [SetUICulture("en-US")]
        [Test]
        public void Should_unstage_only_filtered_on_UnstageAll()
        {
            _referenceRepository.CreateRepoFile("file1A-Привет.txt", "Test");   // escaped and not escaped in the same string
            _referenceRepository.CreateRepoFile("file1B-두다.txt", "Test");      // escaped octal code points (Korean Hangul in this case)
            _referenceRepository.CreateRepoFile("file2.txt", "Test");

            RunFormTest(async form =>
            {
                using (CancellationTokenSource cts = new(AsyncTestHelper.UnexpectedTimeout))
                {
                    await ThreadHelper.JoinPendingOperationsAsync(cts.Token);
                }

                Assert.AreEqual("Unstage all", form.GetTestAccessor().UnstageAllToolItem.ToolTipText);
            });

            RunFormTest(async form =>
            {
                using (CancellationTokenSource cts = new(AsyncTestHelper.UnexpectedTimeout))
                {
                    await ThreadHelper.JoinPendingOperationsAsync(cts.Token);
                }

                var testform = form.GetTestAccessor();

                Assert.AreEqual(0, testform.StagedList.AllItemsCount);
                Assert.AreEqual(3, testform.UnstagedList.AllItemsCount);

                testform.StagedList.SetFilter("");
                testform.StageAllToolItem.PerformClick();

                Assert.AreEqual(3, testform.StagedList.AllItemsCount);
                Assert.AreEqual(0, testform.UnstagedList.AllItemsCount);

                testform.StagedList.ClearSelected();
                testform.StagedList.SetFilter("file1");

                Assert.AreEqual("Unstage filtered", testform.UnstageAllToolItem.ToolTipText);

                testform.UnstageAllToolItem.PerformClick();

                var fileNotMatchedByFilterIsStillStaged = testform.StagedList.AllItems.Any(i => i.Item.Name == "file2.txt");

                Assert.AreEqual(2, testform.UnstagedList.AllItemsCount);
                Assert.AreEqual(1, testform.StagedList.AllItemsCount);
                Assert.IsTrue(fileNotMatchedByFilterIsStillStaged);
            });
        }

        [Test, TestCaseSource(typeof(CommitMessageTestData), nameof(CommitMessageTestData.TestCases))]
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

        [Test, TestCaseSource(typeof(CommitMessageTestData), nameof(CommitMessageTestData.TestCases))]
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

        [Test, TestCaseSource(typeof(CommitMessageTestData), nameof(CommitMessageTestData.TestCases))]
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
        public void EditFileToolStripMenuItem_Click_no_selection_should_not_throw()
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
        public void ResetAuthor_depends_on_amend()
        {
            RunFormTest(form =>
            {
                var testForm = form.GetTestAccessor();

                // check initial state
                Assert.False(testForm.Amend.Checked);
                Assert.False(testForm.ResetAuthor.Checked);
                Assert.False(testForm.ResetAuthor.Visible);

                testForm.Amend.Checked = true;

                // check that reset author checkbox becomes visible when amend is checked
                Assert.True(testForm.Amend.Checked);
                Assert.True(testForm.ResetAuthor.Visible);

                testForm.ResetAuthor.Checked = true;

                Assert.True(testForm.Amend.Checked);

                testForm.Amend.Checked = false;

                // check that reset author checkbox becomes invisible and unchecked when amend is unchecked
                Assert.False(testForm.Amend.Checked);
                Assert.False(testForm.ResetAuthor.Checked);
                Assert.False(testForm.ResetAuthor.Visible);

                testForm.Amend.Checked = true;

                // check that when amend is checked again reset author is still unchecked
                Assert.True(testForm.Amend.Checked);
                Assert.True(testForm.ResetAuthor.Visible);
                Assert.False(testForm.ResetAuthor.Checked);
            });
        }

        [Test]
        public void ResetSoft()
        {
            AppSettings.CommitAndPushForcedWhenAmend = true;
            AppSettings.DontConfirmAmend = true;
            AppSettings.CloseCommitDialogAfterCommit = false;
            AppSettings.CloseCommitDialogAfterLastCommit = false;
            AppSettings.CloseProcessDialog = true;

            int defaultBackColor = SystemColors.ButtonFace.AdaptBackColor().ToArgb();
            int forceBackColor = OtherColors.AmendButtonForcedColor.ToArgb();

            const string originalCommitMessage = "commit to be amended by reset soft";
            const string amendedCommitMessage = "replacement commit";

            ObjectId? previousCommitId = _commands.Module.RevParse("HEAD");
            string originalCommitHash = _referenceRepository.CreateCommit(originalCommitMessage, "original content", "theFile");

            RunFormTest(form =>
            {
                FormCommit.TestAccessor ta = form.GetTestAccessor();

                // initial state
                ta.Amend.Enabled.Should().BeTrue();
                ta.Amend.Checked.Should().BeFalse();
                ta.CommitAndPush.BackColor.ToArgb().Should().Be(defaultBackColor);
                ta.ResetSoft.Visible.Should().BeFalse();
                ta.Message.Text.Should().BeEmpty();

                // amend needs to be activated first
                ta.Amend.Checked = true;

                ta.Amend.Enabled.Should().BeTrue();
                ta.Amend.Checked.Should().BeTrue();
                ta.CommitAndPush.BackColor.ToArgb().Should().Be(forceBackColor);
                ta.ResetSoft.Visible.Should().BeTrue();
                ta.ResetSoft.Enabled.Should().BeTrue();
                ta.Message.Text.Should().Be(originalCommitMessage);

                // reset soft
                ta.Message.Text = amendedCommitMessage;
                ta.ResetSoft.PerformClick();

                // update the form
                Application.DoEvents();
                ThreadHelper.JoinPendingOperations();

                _commands.Module.RevParse("HEAD").Should().Be(previousCommitId);
                ta.Amend.Enabled.Should().BeFalse();
                ta.Amend.Checked.Should().BeFalse();
                ta.CommitAndPush.BackColor.ToArgb().Should().Be(forceBackColor);
                ta.CommitAndPush.Text.Should().Be(ta.CommitAndForcePushText);
                ta.ResetSoft.Visible.Should().BeFalse();
                ta.Message.Text.Should().Be(amendedCommitMessage);

                // commit
                ta.Commit.PerformClick();

                // update the form
                Application.DoEvents();
                ThreadHelper.JoinPendingOperations();

                ta.Amend.Enabled.Should().BeTrue();
                ta.Amend.Checked.Should().BeFalse();
                ta.CommitAndPush.BackColor.ToArgb().Should().Be(forceBackColor);
                ta.CommitAndPush.Text.Should().Be(TranslatedStrings.ButtonPush);
                ta.ResetSoft.Visible.Should().BeFalse();
                ta.Message.Text.Should().BeEmpty();
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

        [Test]
        public void ShouldNotUndoRenameFileWhenResettingStagedLines()
        {
            RunFormTest(form =>
            {
                FormCommit.TestAccessor ta = form.GetTestAccessor();

                FileViewer.TestAccessor selectedDiff = ta.SelectedDiff.GetTestAccessor();
                FileViewerInternal? selectedDiffInternal = selectedDiff.FileViewerInternal;

                // Commit a file, rename it and introduce a slight content change
                string contents = "this\nhas\nmany\nlines\nthis\nhas\nmany\nlines\nthis\nhas\nmany\nlines?\n";
                _referenceRepository.CreateCommit("commit", contents, "original.txt");
                _referenceRepository.DeleteRepoFile("original.txt");
                contents = contents.Replace("?", "!");
                _referenceRepository.CreateRepoFile("original2.txt", contents);

                ta.RescanChanges();
                ThreadHelper.JoinPendingOperations();

                ta.UnstagedList.SelectedItems = ta.UnstagedList.AllItems;
                ta.UnstagedList.Focus();
                ta.ExecuteCommand(FormCommit.Command.StageSelectedFile);

                ta.StagedList.SelectedGitItem = ta.StagedList.AllItems.Single(i => i.Item.Name.Contains("original2.txt")).Item;

                selectedDiffInternal.Focus();
                ThreadHelper.JoinPendingOperations();

                selectedDiffInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.SelectionManager.SetSelection(
                    new TextLocation(2, 11), new TextLocation(5, 12));

                int textLengthBeforeReset = selectedDiffInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.Document.TextLength;

                selectedDiff.ResetSelectedLinesConfirmationDialog.Created += (s, e) =>
                {
                    // Auto-press `Yes`
                    selectedDiff.ResetSelectedLinesConfirmationDialog.Buttons[0].PerformClick();
                };
                selectedDiff.ExecuteCommand(FileViewer.Command.ResetLines);

                ta.RescanChanges();
                ThreadHelper.JoinPendingOperations();

                int textLengthAfterReset = selectedDiffInternal.GetTestAccessor().TextEditor.ActiveTextAreaControl.Document.TextLength;

                textLengthBeforeReset.Should().BeGreaterThan(0);
                textLengthAfterReset.Should().BeGreaterThan(0);
                textLengthAfterReset.Should().BeLessThan(textLengthBeforeReset);
                FileStatusItem? stagedAndRenamed = ta.StagedList.AllItems.FirstOrDefault(i => i.Item.Name.Contains("original2.txt"));
                stagedAndRenamed.Should().NotBeNull();
                stagedAndRenamed!.Item.IsRenamed.Should().BeTrue();
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
                    Assert.True(commitKind switch
                    {
                        CommitKind.Normal => _commands.StartCommitDialog(owner: null),
                        CommitKind.Squash => _commands.StartSquashCommitDialog(owner: null, _referenceRepository.Module.GetRevision()),
                        CommitKind.Fixup => _commands.StartFixupCommitDialog(owner: null, _referenceRepository.Module.GetRevision()),
                        CommitKind.Amend => _commands.StartAmendCommitDialog(owner: null, _referenceRepository.Module.GetRevision()),
                        _ => throw new ArgumentException($"Unsupported commit kind: {commitKind}", nameof(commitKind))
                    });

                    // Await updated FileViewer
                    ThreadHelper.JoinPendingOperations();
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
