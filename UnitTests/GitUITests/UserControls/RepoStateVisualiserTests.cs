using System;
using GitCommands;
using GitUI.UserControls;
using NUnit.Framework;

namespace GitUITests.UserControls
{
    [TestFixture]
    public sealed class RepoStateVisualiserTests
    {
        [SetUp]
        public void SetUp()
        {
            _repoStateVisualiser = new RepoStateVisualiser();
        }

        private RepoStateVisualiser _repoStateVisualiser;

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
            var commitIcon = _repoStateVisualiser.Invoke(Array.Empty<GitItemStatus>());

            Assert.AreEqual(RepoStateVisualiser.Clean, commitIcon);
        }

        [Test]
        public void ReturnsIconDirtySubmodulesWhenThereAreOnlyWorkTreeSubmodules()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus(isSubmodule: true)
            });

            Assert.AreEqual(RepoStateVisualiser.DirtySubmodules, commitIcon);
        }

        [Test]
        public void ReturnsIconDirtyWhenThereAreWorkTreeChanges()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus()
            });

            Assert.AreEqual(RepoStateVisualiser.Dirty, commitIcon);
        }

        [Test]
        public void ReturnsIconMixedWhenThereAreIndexAndWorkTreeFiles()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus()
            });

            Assert.AreEqual(RepoStateVisualiser.Mixed, commitIcon);
        }

        [Test]
        public void ReturnsIconStagedWhenThereAreOnlyIndexFiles()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus(isStaged: true)
            });

            Assert.AreEqual(RepoStateVisualiser.Staged, commitIcon);
        }

        [Test]
        public void ReturnsIconUntrackedOnlyWhenThereAreUntrackedFilesOnly()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isTracked: false),
                CreateGitItemStatus(isTracked: false)
            });

            Assert.AreEqual(RepoStateVisualiser.UntrackedOnly, commitIcon);
        }

        [Test]
        public void ReturnsIconUnknownWhenNull()
        {
            var commitIcon = _repoStateVisualiser.Invoke(null);

            Assert.AreEqual(RepoStateVisualiser.Unknown, commitIcon);
        }
    }
}