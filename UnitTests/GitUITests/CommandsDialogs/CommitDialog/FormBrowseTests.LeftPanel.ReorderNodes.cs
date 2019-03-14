using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.CommitDialog
{
    [Apartment(ApartmentState.STA)]
    public class FormBrowseLeftPanelReorderNodesTest
    {
        // Created once for each test
        private GitUICommands _commands;
        private bool _originalShowAuthorAvatarColumn;
        private List<bool> _originalRepoObjectsTreeShow = new List<bool>();

        private GitModuleTestHelper _repo1;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            _originalShowAuthorAvatarColumn = AppSettings.ShowAuthorAvatarColumn;

            // we don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;

            // Show all root nodes for test, restore when done
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowBranches);
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowRemotes);
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowTags);
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowSubmodules);
            AppSettings.RepoObjectsTreeShowBranches = true;
            AppSettings.RepoObjectsTreeShowRemotes = true;
            AppSettings.RepoObjectsTreeShowTags = true;
            AppSettings.RepoObjectsTreeShowSubmodules = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;

            AppSettings.RepoObjectsTreeShowBranches = _originalRepoObjectsTreeShow[0];
            AppSettings.RepoObjectsTreeShowRemotes = _originalRepoObjectsTreeShow[1];
            AppSettings.RepoObjectsTreeShowTags = _originalRepoObjectsTreeShow[2];
            AppSettings.RepoObjectsTreeShowSubmodules = _originalRepoObjectsTreeShow[3];
        }

        [SetUp]
        public void SetUp()
        {
            _repo1 = new GitModuleTestHelper("repo1");
            _commands = new GitUICommands(_repo1.Module);
        }

        [TearDown]
        public void TearDown()
        {
            _commands = null;
            _repo1.Dispose();
        }

        [Test]
        public void RepoObjectTree_moving_first_up_and_last_down_does_nothing()
        {
            RunFormTest(
                form =>
                {
                    // act
                    var repoObjectTree = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor();
                    var currNodes = repoObjectTree.TreeView.Nodes;
                    List<TreeNode> initialNodes = currNodes.OfType<TreeNode>().ToList();

                    // assert
                    currNodes.Count.Should().Be(4);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3);

                    int first = 0;
                    int last = currNodes.Count - 1;

                    // Trying to move first node up should do nothing
                    repoObjectTree.ReorderTreeNode(currNodes[first], up: true);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3);

                    // Similarly, moving last node down should do nothing
                    repoObjectTree.ReorderTreeNode(currNodes[last], up: false);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3);
                });
        }

        [Test]
        public void RepoObjectTree_moving_node_legally_moves_it()
        {
            RunFormTest(
                form =>
                {
                    // act
                    var repoObjectTree = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor();
                    var currNodes = repoObjectTree.TreeView.Nodes;
                    List<TreeNode> initialNodes = currNodes.OfType<TreeNode>().ToList();

                    // assert
                    currNodes.Count.Should().Be(4);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3);

                    // Move first down
                    repoObjectTree.ReorderTreeNode(currNodes[0], up: false);
                    ValidateOrder(initialNodes, currNodes, 1, 0, 2, 3);
                    repoObjectTree.ReorderTreeNode(currNodes[1], up: false);
                    ValidateOrder(initialNodes, currNodes, 1, 2, 0, 3);
                    repoObjectTree.ReorderTreeNode(currNodes[2], up: false);
                    ValidateOrder(initialNodes, currNodes, 1, 2, 3, 0);

                    // Then back up
                    repoObjectTree.ReorderTreeNode(currNodes[3], up: true);
                    ValidateOrder(initialNodes, currNodes, 1, 2, 0, 3);
                    repoObjectTree.ReorderTreeNode(currNodes[2], up: true);
                    ValidateOrder(initialNodes, currNodes, 1, 0, 2, 3);
                    repoObjectTree.ReorderTreeNode(currNodes[1], up: true);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3);
                });
        }

        [Test]
        public void RepoObjectTree_moving_node_across_hidden_trees_skips_them()
        {
            RunFormTest(
                form =>
                {
                    // act
                    var repoObjectTree = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor();

                    List<TreeNode> initialNodes = repoObjectTree.TreeView.Nodes.OfType<TreeNode>().ToList();

                    // Hide nodes between first and last
                    repoObjectTree.SetTreeVisibleByIndex(1, false);
                    repoObjectTree.SetTreeVisibleByIndex(2, false);

                    // assert
                    var currNodes = repoObjectTree.TreeView.Nodes;
                    currNodes.Count.Should().Be(2);

                    // Move node 0 down, which should move it to index 3
                    repoObjectTree.ReorderTreeNode(currNodes[0], up: false);

                    // Unhide nodes between first and last
                    repoObjectTree.SetTreeVisibleByIndex(1, true);
                    repoObjectTree.SetTreeVisibleByIndex(2, true);

                    // Reset currNodes, should be back to 4
                    currNodes = repoObjectTree.TreeView.Nodes;
                    currNodes.Count.Should().Be(4);

                    // Only first and last nodes should have swapped
                    ValidateOrder(initialNodes, currNodes, 3, 1, 2, 0);
                });
        }

        private void ValidateOrder(List<TreeNode> initialNodes, TreeNodeCollection currNodes, params int[] expectedOrder)
        {
            initialNodes.Count.Should().Be(currNodes.Count);
            initialNodes.Count.Should().Be(expectedOrder.Count());

            for (int i = 0; i < initialNodes.Count; ++i)
            {
                currNodes[i].Should().Be(initialNodes[expectedOrder[i]]);
            }
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
                () =>
                {
                    Assert.True(_commands.StartBrowseDialog(owner: null));
                },
                testDriverAsync);
        }
    }
}
