using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.LeftPanel;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class FormBrowse_LeftPanelTests
    {
        private const string RemoteName = "remote1";

        // Created once for the fixture
        private ReferenceRepository _remoteReferenceRepository;

        // Track the original setting value
        private bool _originalShowAuthorAvatarColumn;
        private bool _showAvailableDiffTools;

        // Created once for each test
        private ReferenceRepository _referenceRepository;
        private GitUICommands _commands;

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

            AppSettings.RepoObjectsTreeShowTags = true;
            AppSettings.RepoObjectsTreeShowStashes = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;
            AppSettings.ShowAvailableDiffTools = _showAvailableDiffTools;

            _remoteReferenceRepository.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _remoteReferenceRepository);

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

            var composition = TestComposition.Empty
                .AddParts(typeof(MockLinkFactory))
                .AddParts(typeof(MockWindowsJumpListManager))
                .AddParts(typeof(MockRepositoryDescriptionProvider))
                .AddParts(typeof(MockAppTitleGenerator));
            ExportProvider mefExportProvider = composition.ExportProviderFactory.CreateExportProvider();
            ManagedExtensibility.SetTestExportProvider(mefExportProvider);
        }

        [TearDown]
        public void TearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void Branch_and_tag_context_menu_should_show_Sort_By_entry()
        {
            RunRepoObjectsTreeTest(contextMenu =>
            {
                contextMenu.Items.Count.Should().BeGreaterThan(5);

                int count = contextMenu.Items.Count;

                // Assert items from bottom to the top
                ToolStripItem item = contextMenu.Items[--count];
                item.Text.Should().Be("Run script");
                item.Enabled.Should().BeFalse("because this test includes no user scripts");

                contextMenu.Items[--count].Should().BeOfType<ToolStripSeparator>()
                    .Which.Enabled.Should().BeFalse("because this test includes no user scripts");

                item = contextMenu.Items[--count];
                item.Text.Should().Be(TranslatedStrings.SortOrder);
                item.Enabled.Should().BeFalse("because sort order in this test is default");

                item = contextMenu.Items[--count];
                item.Text.Should().Be(TranslatedStrings.SortBy);
                item.Enabled.Should().BeTrue("because tags and branches are sortable");

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

                    ContextMenuStrip contextMenu = ta.ContextMenu;

                    ta.SelectNode<LocalBranchNode>(new[] { TranslatedStrings.Branches, "master" });
                    ta.OpenContextMenu();
                    testDriver(contextMenu);

                    ta.SelectNode<RemoteBranchNode>(new[] { TranslatedStrings.Remotes, RemoteName, "master" });
                    ta.OpenContextMenu();
                    testDriver(contextMenu);

                    ta.SelectNode<TagNode>(new[] { TranslatedStrings.Tags, "Branch1" });
                    ta.OpenContextMenu();
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
