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

            Assert.AreEqual((RepoStateVisualiser.IconClean, RepoStateVisualiser.BrushClean), commitIcon);
        }

        [Test]
        public void ReturnsIconDirtySubmodulesWhenThereAreOnlyWorkTreeSubmodules()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus(isSubmodule: true)
            });

            Assert.AreEqual((RepoStateVisualiser.IconDirtySubmodules, RepoStateVisualiser.BrushDirtySubmodules), commitIcon);
        }

        [Test]
        public void ReturnsIconDirtyWhenThereAreWorkTreeChanges()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isSubmodule: true),
                CreateGitItemStatus()
            });

            Assert.AreEqual((RepoStateVisualiser.IconDirty, RepoStateVisualiser.BrushDirty), commitIcon);
        }

        [Test]
        public void ReturnsIconMixedWhenThereAreIndexAndWorkTreeFiles()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus()
            });

            Assert.AreEqual((RepoStateVisualiser.IconMixed, RepoStateVisualiser.BrushMixed), commitIcon);
        }

        [Test]
        public void ReturnsIconStagedWhenThereAreOnlyIndexFiles()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isStaged: true),
                CreateGitItemStatus(isStaged: true)
            });

            Assert.AreEqual((RepoStateVisualiser.IconStaged, RepoStateVisualiser.BrushStaged), commitIcon);
        }

        [Test]
        public void ReturnsIconUntrackedOnlyWhenThereAreUntrackedFilesOnly()
        {
            var commitIcon = _repoStateVisualiser.Invoke(new[]
            {
                CreateGitItemStatus(isTracked: false),
                CreateGitItemStatus(isTracked: false)
            });

            Assert.AreEqual((RepoStateVisualiser.IconUntrackedOnly, RepoStateVisualiser.BrushUntrackedOnly), commitIcon);
        }
    }
}