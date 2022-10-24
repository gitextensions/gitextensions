﻿using System.Collections;
using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.BranchTreePanel;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class FormBrowse_LeftPanel_ReorderNodesTest
    {
        // Created once for each test
        private GitUICommands _commands;

        // Track the original setting value
        private bool _originalShowAuthorAvatarColumn;
        private bool _showAvailableDiffTools;

        private List<bool> _originalRepoObjectsTreeShow = new();

        private GitModuleTestHelper _repo1;

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

            // Show all root nodes for test, restore when done
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowBranches);
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowRemotes);
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowTags);
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowStashes);
            _originalRepoObjectsTreeShow.Add(AppSettings.RepoObjectsTreeShowSubmodules);
            AppSettings.RepoObjectsTreeShowBranches = true;
            AppSettings.RepoObjectsTreeShowRemotes = true;
            AppSettings.RepoObjectsTreeShowTags = true;
            AppSettings.RepoObjectsTreeShowStashes = true;
            AppSettings.RepoObjectsTreeShowSubmodules = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;
            AppSettings.ShowAvailableDiffTools = _showAvailableDiffTools;

            AppSettings.RepoObjectsTreeShowBranches = _originalRepoObjectsTreeShow[0];
            AppSettings.RepoObjectsTreeShowRemotes = _originalRepoObjectsTreeShow[1];
            AppSettings.RepoObjectsTreeShowTags = _originalRepoObjectsTreeShow[2];
            AppSettings.RepoObjectsTreeShowSubmodules = _originalRepoObjectsTreeShow[3];
            AppSettings.RepoObjectsTreeShowStashes = _originalRepoObjectsTreeShow[4];
        }

        [SetUp]
        public void SetUp()
        {
            _repo1 = new GitModuleTestHelper("repo1");
            _commands = new GitUICommands(_repo1.Module);

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
            _repo1.Dispose();
        }

        [Test]
        public void RepoObjectTree_moving_first_up_and_last_down_does_nothing()
        {
            RunRepoObjectsTreeTest(
                repoObjectTree =>
                {
                    var testAccessor = repoObjectTree.GetTestAccessor();

                    // act
                    var currNodes = testAccessor.TreeView.Nodes;
                    List<TreeNode> initialNodes = currNodes.OfType<TreeNode>().ToList();

                    // assert
                    AssertListCount(currNodes, 5);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3, 4);

                    int first = 0;
                    int last = currNodes.Count - 1;

                    // Trying to move first node up should do nothing
                    testAccessor.ReorderTreeNode(currNodes[first], up: true);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3, 4);

                    // Similarly, moving last node down should do nothing
                    testAccessor.ReorderTreeNode(currNodes[last], up: false);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3, 4);
                });
        }

        [Test]
        public void RepoObjectTree_moving_node_legally_moves_it()
        {
            RunRepoObjectsTreeTest(
                repoObjectTree =>
                {
                    var testAccessor = repoObjectTree.GetTestAccessor();

                    // act
                    var currNodes = testAccessor.TreeView.Nodes;
                    List<TreeNode> initialNodes = currNodes.OfType<TreeNode>().ToList();

                    // assert
                    AssertListCount(currNodes, 5);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3, 4);

                    // Move first down
                    testAccessor.ReorderTreeNode(currNodes[0], up: false);
                    ValidateOrder(initialNodes, currNodes, 1, 0, 2, 3, 4);
                    testAccessor.ReorderTreeNode(currNodes[1], up: false);
                    ValidateOrder(initialNodes, currNodes, 1, 2, 0, 3, 4);
                    testAccessor.ReorderTreeNode(currNodes[2], up: false);
                    ValidateOrder(initialNodes, currNodes, 1, 2, 3, 0, 4);

                    // Then back up
                    testAccessor.ReorderTreeNode(currNodes[3], up: true);
                    ValidateOrder(initialNodes, currNodes, 1, 2, 0, 3, 4);
                    testAccessor.ReorderTreeNode(currNodes[2], up: true);
                    ValidateOrder(initialNodes, currNodes, 1, 0, 2, 3, 4);
                    testAccessor.ReorderTreeNode(currNodes[1], up: true);
                    ValidateOrder(initialNodes, currNodes, 0, 1, 2, 3, 4);
                });
        }

        [Test]
        public void RepoObjectTree_moving_node_across_hidden_trees_skips_them()
        {
            RunRepoObjectsTreeTest(
                repoObjectTree =>
                {
                    var testAccessor = repoObjectTree.GetTestAccessor();

                    // act
                    var currNodes = testAccessor.TreeView.Nodes;
                    List<TreeNode> initialNodes = currNodes.OfType<TreeNode>().ToList();
                    AssertListCount(initialNodes, 5);

                    // Hide nodes between first and last
                    testAccessor.SetTreeVisibleByIndex(1, false);
                    testAccessor.SetTreeVisibleByIndex(2, false);

                    // assert
                    AssertListCount(currNodes, 3);

                    // Move node 0 down, which should move it to index 3
                    testAccessor.ReorderTreeNode(currNodes[0], up: false);

                    // Unhide nodes between first and last
                    testAccessor.SetTreeVisibleByIndex(1, true);
                    testAccessor.SetTreeVisibleByIndex(2, true);

                    // Reset currNodes, should be back
                    AssertListCount(currNodes, 5);

                    // Only first and last nodes should have swapped
                    ValidateOrder(initialNodes, currNodes, 3, 1, 2, 0, 4);
                });
        }

        private static void AssertListCount(ICollection collection, int expectedCount)
        {
            int actualCount = collection.Count;
            if (actualCount == expectedCount)
            {
                return;
            }

            string items = collection.OfType<object>().Select(n => n.ToString()).Join(", ");
            Assert.Fail($"Actual count {actualCount} differs from expected {expectedCount}.{Environment.NewLine}Actual items: {items}");
        }

        private void ValidateOrder(List<TreeNode> initialNodes, TreeNodeCollection currNodes, params int[] expectedOrder)
        {
            AssertListCount(currNodes, expectedOrder.Length);
            AssertListCount(initialNodes, expectedOrder.Length);

            for (int i = 0; i < initialNodes.Count; ++i)
            {
                currNodes[i].Should().Be(initialNodes[expectedOrder[i]]);
            }
        }

        private void RunRepoObjectsTreeTest(Action<RepoObjectsTree> testDriver)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form.GetTestAccessor().RepoObjectsTree);
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
