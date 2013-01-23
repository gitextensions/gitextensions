using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;

using System;
using System.Text;
using GitCommands;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using GitCommands.Git;

namespace GitCommandsTest.Git
{
    [TestClass]
    public class GitCheckoutBranchCmdTest
    {
        private GitCheckoutBranchCmd GetInstance(bool remote)
        {
            return new GitCheckoutBranchCmd("branchName", remote);
        }

        [TestMethod]
        public void TestConstructor()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsTrue(cmd.Remote);
        }

        [TestMethod]
        public void TestConstructorRemoteIsFalse()
        {
            GitCheckoutBranchCmd cmd = GetInstance(false);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsFalse(cmd.Remote);
        }

        [TestMethod]
        public void TestGitCommandName()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.AreEqual(cmd.GitComandName(), "checkout");
        }

        [TestMethod]
        public void TestAccessesRemoteIsFalse()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.IsFalse(cmd.AccessesRemote());
        }

        [TestMethod]
        public void TestSetLocalChangesFromSettings()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            cmd.SetLocalChangesFromSettings(Settings.LocalChanges.Merge);
            Assert.AreEqual(GitCheckoutBranchCmd.LocalChanges.Merge, cmd.LocalChangesAction);

            cmd.SetLocalChangesFromSettings(Settings.LocalChanges.Reset);
            Assert.AreEqual(GitCheckoutBranchCmd.LocalChanges.Reset, cmd.LocalChangesAction);

            cmd.SetLocalChangesFromSettings(Settings.LocalChanges.Stash);
            Assert.AreEqual(GitCheckoutBranchCmd.LocalChanges.DontChange, cmd.LocalChangesAction);

            cmd.SetLocalChangesFromSettings(Settings.LocalChanges.Merge);
            cmd.SetLocalChangesFromSettings(Settings.LocalChanges.DontChange);
            Assert.AreEqual(GitCheckoutBranchCmd.LocalChanges.DontChange, cmd.LocalChangesAction);

        }

        [TestMethod]
        public void TestCollectArgumentsMergeReset()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);

            IEnumerable<string> whenMergeChangesOnly = new List<string> { "--merge", "\"branchName\"" };
            IEnumerable<string> whenMergeChangesWithRemoteNewBranchCreate = new List<string> { "--merge", "-b \"newBranchName\"", "\"branchName\"" };
            IEnumerable<string> whenMergeChangesWithRemoteNewBranchReset = new List<string> { "--merge", "-B \"newBranchName\"", "\"branchName\"" };

            IEnumerable<string> whenResetChangesOnly = new List<string> { "--force", "\"branchName\"" };
            IEnumerable<string> whenResetChangesWithRemoteNewBranchCreate = new List<string> { "--force", "-b \"newBranchName\"", "\"branchName\"" };
            IEnumerable<string> whenResetChangesWithRemoteNewBranchReset = new List<string> { "--force", "-B \"newBranchName\"", "\"branchName\"" };

            //Merge
            {
                cmd.LocalChangesAction = GitCheckoutBranchCmd.LocalChanges.Merge;
                cmd.Remote = false;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenMergeChangesOnly));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                cmd.Remote = true;
                cmd.NewBranchName = "newBranchName";

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenMergeChangesWithRemoteNewBranchCreate));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenMergeChangesWithRemoteNewBranchReset));
            }

            //Reset
            {
                cmd.LocalChangesAction = GitCheckoutBranchCmd.LocalChanges.Reset;
                cmd.Remote = false;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenResetChangesOnly));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                cmd.Remote = true;
                cmd.NewBranchName = "newBranchName";

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenResetChangesWithRemoteNewBranchCreate));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenResetChangesWithRemoteNewBranchReset));
            }

        }
    }
}

