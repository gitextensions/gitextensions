using System;
using GitCommands;
using GitUI.UserControls.ToolStripClasses;
using NUnit.Framework;

namespace GitUITests.UserControls.ToolStripClasses
{
    [TestFixture]
    public class CommitIconProviderTests
    {
        [SetUp]
        public void SetUp()
        {
            _commitIconProvider = new CommitIconProvider();
        }

        private CommitIconProvider _commitIconProvider;

        private static GitItemStatus CreateGitItemStatus(
            bool isStaged = false,
            bool isTracked = true,
            bool isSubmodule = false)
        {
            return new GitItemStatus
            {
                Staged = isStaged ? StagedStatus.Index : StagedStatus.WorkTree,
                IsTracked = isTracked,
                IsSubmodule = isSubmodule
            };
        }

        [Test]
        public void ReturnsIconCleanWhenThereIsNoChangedFiles()
        {
            var commitIcon = _commitIconProvider.GetCommitIcon(Array.Empty<GitItemStatus>());

            Assert.AreEqual(CommitIconProvider.IconClean, commitIcon);
        }

        [Test]
        public void ReturnsIconDirtySubmodulesWhenThereAreOnlyUnstagedSubmodules()
        {
            var commitIcon = _commitIconProvider.GetCommitIcon(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus(isSubmodule: true)
            });

            Assert.AreEqual(CommitIconProvider.IconDirtySubmodules, commitIcon);
        }

        [Test]
        public void ReturnsIconDirtyWhenThereAreUnstagedChanges()
        {
            var commitIcon = _commitIconProvider.GetCommitIcon(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus()
            });

            Assert.AreEqual(CommitIconProvider.IconDirty, commitIcon);
        }

        [Test]
        public void ReturnsIconMixedWhenThereAreStagedAndUnstagedFiles()
        {
            var commitIcon = _commitIconProvider.GetCommitIcon(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus()
            });

            Assert.AreEqual(CommitIconProvider.IconMixed, commitIcon);
        }

        [Test]
        public void ReturnsIconStagedWhenThereAreOnlyStagedFiles()
        {
            var commitIcon = _commitIconProvider.GetCommitIcon(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus(isStaged: true)
            });

            Assert.AreEqual(CommitIconProvider.IconStaged, commitIcon);
        }

        [Test]
        public void ReturnsIconUntrackedOnlyWhenThereAreUntrackedFilesOnly()
        {
            var commitIcon = _commitIconProvider.GetCommitIcon(new[]
            {
                CreateGitItemStatus(isTracked: false),
                CreateGitItemStatus(isTracked: false)
            });

            Assert.AreEqual(CommitIconProvider.IconUntrackedOnly, commitIcon);
        }
    }
}