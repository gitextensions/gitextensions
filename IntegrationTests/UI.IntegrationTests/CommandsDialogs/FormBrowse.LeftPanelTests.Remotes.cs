using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitCommands.Remotes;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class FormBrowse_LeftPanel_RemotesTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Track the original setting value
        private bool _originalShowAuthorAvatarColumn;
        private bool _showAvailableDiffTools;

        // Created once for each test
        private GitUICommands _commands;
        private IConfigFileRemoteSettingsManager _remotesManager;
        private static readonly string[] RemoteNames = new[] { "remote1", "remote5", "remote3", "remote4", "remote2" };

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
            // we will be modifying .git/config and need to completely reset each time
            _referenceRepository = new ReferenceRepository();

            foreach (var name in RemoteNames)
            {
                _referenceRepository.Module.AddRemote(name, $"http://localhost/remotes/{name}.git");
            }

            _commands = new GitUICommands(_referenceRepository.Module);
            _remotesManager = new ConfigFileRemoteSettingsManager(() => _referenceRepository.Module);

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
        public void RepoObjectTree_should_load_active_remotes()
        {
            RunRepoObjectsTreeTest(
                remotesNode =>
                {
                    // act
                    // no-op: by the virtue of loading the form, the left panel has loaded its content

                    // assert
                    remotesNode.Nodes.Count.Should().Be(RemoteNames.Length);
                });
        }

        [Test]
        public void RepoObjectTree_should_order_active_remotes_alphabetically()
        {
            RunRepoObjectsTreeTest(
                remotesNode =>
                {
                    // act
                    // no-op: by the virtue of loading the form, the left panel has loaded its content

                    // assert
                    var names = remotesNode.Nodes.OfType<TreeNode>().Select(x => x.Text).ToList();
                    names.Should().BeEquivalentTo(RemoteNames);
                    names.Should().BeInAscendingOrder();
                });
        }

        [Test]
        public void RepoObjectTree_should_order_should_not_display_inactive_if_none()
        {
            RunRepoObjectsTreeTest(
                remotesNode =>
                {
                    // act
                    // no-op: by the virtue of loading the form, the left panel has loaded its content

                    // assert
                    remotesNode.Nodes.OfType<TreeNode>().Any(n => n.Text == TranslatedStrings.Inactive).Should().BeFalse();
                });
        }

        [Test]
        public void RepoObjectTree_should_order_should_display_inactive_if_present()
        {
            // setup
            DeactivateTreeNode(RemoteNames[1]);

            RunRepoObjectsTreeTest(
                remotesNode =>
                {
                    // act
                    // no-op: by the virtue of loading the form, the left panel has loaded its content

                    // assert
                    remotesNode.Nodes.OfType<TreeNode>().Count(n => n.Text == TranslatedStrings.Inactive).Should().Be(1);
                });
        }

        [Test]
        public void RepoObjectTree_should_order_should_display_inactive_after_active_nodes()
        {
            // setup
            DeactivateTreeNode(RemoteNames[1]);

            RunRepoObjectsTreeTest(
                remotesNode =>
                {
                    // act
                    // no-op: by the virtue of loading the form, the left panel has loaded its content

                    // assert
                    remotesNode.Nodes.OfType<TreeNode>().Last().Text.Should().Be(TranslatedStrings.Inactive);
                });
        }

        [Test]
        public void RepoObjectTree_should_order_inactive_remotes_alphabetically()
        {
            // setup
            DeactivateTreeNode(RemoteNames[3]);
            DeactivateTreeNode(RemoteNames[0]);
            DeactivateTreeNode(RemoteNames[1]);

            RunRepoObjectsTreeTest(
                remotesNode =>
                {
                    // act
                    // no-op: by the virtue of loading the form, the left panel has loaded its content

                    // assert
                    var inactiveNodes = remotesNode.Nodes.OfType<TreeNode>().Last().Nodes.OfType<TreeNode>().Select(n => n.Text).ToList();
                    inactiveNodes.Count.Should().Be(3);
                    inactiveNodes.Should().BeEquivalentTo(RemoteNames[3], RemoteNames[0], RemoteNames[1]);
                    inactiveNodes.Should().BeInAscendingOrder();
                });
        }

        private void DeactivateTreeNode(string nodeText)
        {
            _remotesManager.ToggleRemoteState(nodeText, true);
        }

        private static TreeNode GetRemoteNode(FormBrowse form)
        {
            Assert.IsFalse(form.MainSplitContainer.Panel1Collapsed);

            // Await all async operation such as load of branches and remotes in the left panel
            ThreadHelper.JoinPendingOperations();

            var treeView = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor().TreeView;
            var remotesNode = treeView.Nodes.OfType<TreeNode>().FirstOrDefault(n => n.Text == TranslatedStrings.Remotes);
            remotesNode.Should().NotBeNull();

            return remotesNode;
        }

        private void RunRepoObjectsTreeTest(Action<TreeNode> testDriver)
        {
            RunFormTest(
                form =>
                {
                    testDriver(GetRemoteNode(form));
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
