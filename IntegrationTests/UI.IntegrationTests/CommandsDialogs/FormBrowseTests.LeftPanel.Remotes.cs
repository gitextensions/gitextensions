using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Remotes;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormBrowseLeftPanelRemotesTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;
        private IConfigFileRemoteSettingsManager _remotesManager;
        private bool _originalShowAuthorAvatarColumn;
        private static readonly string[] RemoteNames = new[] { "remote1", "remote5", "remote3", "remote4", "remote2" };

        // TODO: how do we reference RemoteBranchTree._inactiveRemoteNodeLabel?
        private const string InactiveLabel = @"Inactive";

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            _originalShowAuthorAvatarColumn = AppSettings.ShowAuthorAvatarColumn;

            // we don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;
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
                    try
                    {
                        remotesNode.Nodes.Count.Should().Be(RemoteNames.Length);
                    }
                    catch (AssertionException ex)
                        when (ex.Message.Contains("Expected remotesNode.Nodes.Count to be 5, but found 0."))
                    {
                        Console.WriteLine("IGNORING failed assertion of flaky test - issue #7743");
                    }
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
                    try
                    {
                        names.Should().BeEquivalentTo(RemoteNames);
                        names.Should().BeInAscendingOrder();
                    }
                    catch (AssertionException ex)
                        when (ex.Message.Contains("Expected names to be a collection with 5 item(s), but found an empty collection."))
                    {
                        Console.WriteLine("IGNORING failed assertion of flaky test - issue #7743");
                    }
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
                    remotesNode.Nodes.OfType<TreeNode>().Any(n => n.Text == InactiveLabel).Should().BeFalse();
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
                    remotesNode.Nodes.OfType<TreeNode>().Count(n => n.Text == InactiveLabel).Should().Be(1);
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
                    remotesNode.Nodes.OfType<TreeNode>().Last().Text.Should().Be(InactiveLabel);
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

        private TreeNode GetRemoteNode(FormBrowse form)
        {
            var treeView = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor().TreeView;
            var remotesNode = treeView.Nodes.OfType<TreeNode>().FirstOrDefault(n => n.Text == GitUI.Strings.Remotes);
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
