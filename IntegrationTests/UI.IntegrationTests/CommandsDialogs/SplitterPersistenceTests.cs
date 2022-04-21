using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class SplitterPersistenceTests
    {
        // Created once for the fixture
        private TestComposition _composition;
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            // Stop loading custom diff tools
            AppSettings.ShowAvailableDiffTools = false;

            // We don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;

            _composition = TestComposition.Empty
                .AddParts(typeof(MockLinkFactory))
                .AddParts(typeof(MockWindowsJumpListManager))
                .AddParts(typeof(MockRepositoryDescriptionProvider))
                .AddParts(typeof(MockAppTitleGenerator));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [Test]
        public void SplitterPositionsShouldBeRestored([Values(true, false)] bool leftPanelVisible)
        {
            AppSettings.SetBool("FormBrowse.MainSplitContainer_Panel1Collapsed", leftPanelVisible);
            AppSettings.SetString("Detailed.CommitInfoPosition", "RightwardFromList");

            const int LeftPanelWidth = 123;
            const int CommitInfoWidth = 124;
            const int RevisionGridHeight = 125;
            const int FileListWidth = 126;
            const int FileTreeWidth = 127;

            RunFormTest(
                form =>
                {
                    FormBrowse.TestAccessor ta = form.GetTestAccessor();

                    WaitForRevisionsToBeLoaded(ta.RevisionGrid);

                    form.MainSplitContainer.SplitterDistance = LeftPanelWidth;
                    ta.RevisionsSplitContainer.SplitterDistance = ta.RevisionsSplitContainer.Width - CommitInfoWidth;
                    ta.RightSplitContainer.SplitterDistance = RevisionGridHeight;
                    ta.RevisionDiffControl.GetTestAccessor().DiffSplitContainer.SplitterDistance = FileListWidth;
                    ta.RevisionFileTreeControl.GetTestAccessor().FileTreeSplitContainer.SplitterDistance = FileTreeWidth;
                });
            AppSettings.SaveSettings();
            RunFormTest(
                form =>
                {
                    FormBrowse.TestAccessor ta = form.GetTestAccessor();

                    WaitForRevisionsToBeLoaded(ta.RevisionGrid);
                    Thread.Sleep(1000);

                    form.MainSplitContainer.Panel1Collapsed.Should().Be(leftPanelVisible);
                    //// does not work: form.MainSplitContainer.SplitterDistance.Should().Be(LeftPanelWidth);
                    ta.RevisionsSplitContainer.SplitterDistance.Should().Be(ta.RevisionsSplitContainer.Width - CommitInfoWidth);
                    ta.RightSplitContainer.SplitterDistance.Should().Be(RevisionGridHeight);
                    ta.RevisionDiffControl.GetTestAccessor().DiffSplitContainer.SplitterDistance.Should().Be(FileListWidth);
                    ta.RevisionFileTreeControl.GetTestAccessor().FileTreeSplitContainer.SplitterDistance.Should().Be(FileTreeWidth);
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

        private static void ProcessUntil<T>(Func<T> getCurrent, T expected, int maxIterations = 100) where T : IEquatable<T>
        {
            T current = default(T);
            for (int iteration = 0; iteration < maxIterations; ++iteration)
            {
                current = getCurrent();
                if (current.Equals(expected))
                {
                    Debug.WriteLine($"{nameof(ProcessUntil)} '{expected}' in iteration {iteration}");
                    return;
                }

                Application.DoEvents();
                Thread.Sleep(5);
            }

            Assert.Fail($"{current} != {expected} in {maxIterations} iterations");
        }

        private static void WaitForRevisionsToBeLoaded(RevisionGridControl revisionGridControl)
        {
            UITest.ProcessUntil("Loading Revisions", () => revisionGridControl.GetTestAccessor().IsUiStable);
        }
    }
}
