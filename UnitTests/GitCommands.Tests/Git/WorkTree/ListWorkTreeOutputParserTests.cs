using System.Collections.Generic;
using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.Git.WorkTrees;
using NUnit.Framework;

namespace GitCommandsTests.Git.WorkTree
{
    [TestFixture]
    public class ListWorkTreeOutputParserTests
    {
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("Lorem ipsum dolor sit amet, solet soleat option mel no.")]
        public void Parse_should_return_empty_list(string output)
        {
            List<GitWorkTree> worktrees = ListWorkTreeOutputParser.Parse(output);
            Assert.AreEqual(0, worktrees.Count);
        }

        [Test]
        public void Parse_should_return_three_valid_items()
        {
            string resourceName = $"{GetType().Namespace}.MockData.List_worktree_output.txt";
            string output = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), resourceName);
            List<GitWorkTree> worktrees = ListWorkTreeOutputParser.Parse(output);

            Assert.AreEqual(3, worktrees.Count);

            Assert.AreEqual("/path/to/bare-source", worktrees[0].Path);
            Assert.AreEqual(HeadType.Bare, worktrees[0].Type);
            Assert.AreEqual(null, worktrees[0].CompleteBranchName);
            Assert.AreEqual(null, worktrees[0].BranchName);
            Assert.AreEqual(null, worktrees[0].Sha1);

            Assert.AreEqual("/path/to/linked-worktree", worktrees[1].Path);
            Assert.AreEqual(HeadType.Branch, worktrees[1].Type);
            Assert.AreEqual("refs/heads/master", worktrees[1].CompleteBranchName);
            Assert.AreEqual("master", worktrees[1].BranchName);
            Assert.AreEqual("abcd1234abcd1234abcd1234abcd1234abcd1234", worktrees[1].Sha1);

            Assert.AreEqual("/path/to/other-linked-worktree", worktrees[2].Path);
            Assert.AreEqual(HeadType.Detached, worktrees[2].Type);
            Assert.AreEqual(null, worktrees[2].CompleteBranchName);
            Assert.AreEqual(null, worktrees[2].BranchName);
            Assert.AreEqual("1234abc1234abc1234abc1234abc1234abc1234a", worktrees[2].Sha1);
        }
    }
}
