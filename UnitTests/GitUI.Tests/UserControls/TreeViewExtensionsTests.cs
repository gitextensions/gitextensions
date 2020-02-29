﻿using System.Collections.Generic;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.UserControls;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.UserControls
{
    [TestFixture]
    public class TreeViewExtensionsTests
    {
        private TreeView _treeView;
        private TreeNode _root;
        private TreeNode _a;
        private TreeNode _b;
        private TreeNode _c;
        private TreeNode _b1;
        private TreeNode _b2;
        private TreeNode _b3;
        private TreeNode _b2_1;

        [SetUp]
        public void Setup()
        {
            _treeView = new TreeView();

            _root = _treeView.Nodes.Add("Root");

            _a = _root.Nodes.Add("A");
            _b = _root.Nodes.Add("B");
            _c = _root.Nodes.Add("C");

            _b1 = _b.Nodes.Add("B_1");
            _b2 = _b.Nodes.Add("B_2");
            _b3 = _b.Nodes.Add("B_3");

            _b2_1 = _b2.Nodes.Add("B_2_1");
        }

        [Test]
        public void GetExpandedNodesState_should_return_no_expanded_nodes()
        {
            var expandedNodes = _root.GetExpandedNodesState();
            Assert.AreEqual(0, expandedNodes.Count);
        }

        [Test]
        public void GetExpandedNodesState_should_return_one_expanded_node()
        {
            _b.Expand();
            var expandedNodes = _root.GetExpandedNodesState();
            Assert.AreEqual(1, expandedNodes.Count);
            Assert.AreEqual(true, expandedNodes.Contains(@"Root\B"));
        }

        [Test]
        public void GetExpandedNodesState_should_return_all_expanded_nodes()
        {
            _root.ExpandAll();
            var expandedNodes = _root.GetExpandedNodesState();
            Assert.AreEqual(8, expandedNodes.Count);
        }

        [Test]
        public void RestoreExpandedNodesState_should_restore_no_nodes()
        {
            var expandedNodes = new HashSet<string>();
            _root.RestoreExpandedNodesState(expandedNodes);

            var expandedNodesPost = _root.GetExpandedNodesState();
            Assert.AreEqual(0, expandedNodesPost.Count);
        }

        [Test]
        public void RestoreExpandedNodesState_should_restore_one_node()
        {
            var expandedNodes = new HashSet<string> { @"Root\B" };
            _root.RestoreExpandedNodesState(expandedNodes);

            var expandedNodesPost = _root.GetExpandedNodesState();
            Assert.AreEqual(1, expandedNodesPost.Count);
            Assert.AreEqual(true, expandedNodes.Contains(@"Root\B"));
        }

        [Test]
        public void RestoreExpandedNodesState_should_restore_all_nodes()
        {
            var expandedNodes = new HashSet<string>
            {
                $"{_root.GetFullNamePath()}",
                $"{_a.GetFullNamePath()}",
                $"{_b.GetFullNamePath()}",
                $"{_b1.GetFullNamePath()}",
                $"{_b2.GetFullNamePath()}",
                $"{_b2_1.GetFullNamePath()}",
                $"{_b3.GetFullNamePath()}",
                $"{_c.GetFullNamePath()}",
            };

            _root.RestoreExpandedNodesState(expandedNodes);

            var expandedNodesPost = _root.GetExpandedNodesState();
            Assert.AreEqual(8, expandedNodesPost.Count);
            Assert.AreEqual(expandedNodes, expandedNodesPost);
        }

        [Test]
        public void GetFullNamePath_should_return_node_names_if_set()
        {
            // Only override the names for 2 of the 4 nodes
            _b.Name = "B_Name";
            _b2_1.Name = "B_2_1_Name";
            _b2_1.GetFullNamePath().Should().Be(@"Root\B_Name\B_2\B_2_1_Name");
        }
    }
}
