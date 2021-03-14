using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class FormBrowse_LeftPanelTests
    {
        private const string RemoteName = "remote1";

        // Created once for the fixture
        private ReferenceRepository _remoteReferenceRepository;

        // Created once for each test
        private ReferenceRepository _referenceRepository;
        private GitUICommands _commands;
        private bool _originalShowAuthorAvatarColumn;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            _originalShowAuthorAvatarColumn = AppSettings.ShowAuthorAvatarColumn;

            // we don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;

            AppSettings.RepoObjectsTreeShowTags = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;

            _remoteReferenceRepository.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            if (_remoteReferenceRepository is null)
            {
                _remoteReferenceRepository = new ReferenceRepository();
            }

            // we will be modifying .git/config and need to completely reset each time
            _referenceRepository = new ReferenceRepository();

            _referenceRepository.Module.AddRemote(RemoteName, _remoteReferenceRepository.Module.WorkingDir);
            _referenceRepository.Fetch(RemoteName);

            _commands = new GitUICommands(_referenceRepository.Module);

            _referenceRepository.CreateCommit("Commit1", "Commit1");
            _referenceRepository.CreateBranch("Branch1", _referenceRepository.CommitHash);
            _referenceRepository.CreateTag("Branch1", _referenceRepository.CommitHash);
            _referenceRepository.CreateCommit("Commit2", "Commit2");
            _referenceRepository.CreateBranch("Branch2", _referenceRepository.CommitHash);

            _referenceRepository.CreateCommit("head commit");
        }

        [TearDown]
        public void TearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void RepoObjectTree_should_show_Sort_menu_before_Expand_menus()
        {
            RunRepoObjectsTreeTest(
                contextMenu =>
                {
                    contextMenu.Items.Count.Should().BeGreaterThan(6);

                    int count = contextMenu.Items.Count;

                    // Assert items from bottom to the top
                    contextMenu.Items[--count].Text.Should().Be("Collapse");
                    contextMenu.Items[--count].Text.Should().Be("Expand");
                    contextMenu.Items[--count].Should().BeOfType<ToolStripSeparator>();
                    contextMenu.Items[--count].Text.Should().Be(GitUI.Strings.SortOrder);
                    contextMenu.Items[--count].Text.Should().Be(GitUI.Strings.SortBy);
                    contextMenu.Items[--count].Should().BeOfType<ToolStripSeparator>();
                });
        }

        private void RunRepoObjectsTreeTest(Action<ContextMenuStrip> testDriver)
        {
            RunFormTest(
                form =>
                {
                    var ta = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor();

                    // We are running several tests one after another to speed up the test execution
                    // as we don't need to re-create the host form

                    ContextMenuStrip contextMenu = ta.BranchContextMenu;
                    ta.OnContextMenuOpening(contextMenu, new CancelEventArgs());
                    testDriver(contextMenu);

                    contextMenu = ta.RemoteContextMenu;
                    ta.OnContextMenuOpening(contextMenu, new CancelEventArgs());
                    testDriver(contextMenu);

                    contextMenu = ta.TagContextMenu;
                    ta.OnContextMenuOpening(contextMenu, new CancelEventArgs());
                    testDriver(contextMenu);

                    return Task.CompletedTask;
                });
        }

        private void RunFormTest(Func<FormBrowse, Task> testDriverAsync)
        {
            UITest.RunForm(
                showForm: () => _commands.StartBrowseDialog(owner: null).Should().BeTrue(),
                testDriverAsync);
        }
    }
}
