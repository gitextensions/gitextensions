using GitExtensions.Extensibility.Git;
using GitUI.UserControls;

namespace GitUITests.UserControls;
public sealed class RepoStateVisualiserTests
{
    [SetUp]
    public void SetUp()
    {
        _repoStateVisualiser = new RepoStateVisualiser();
    }

    private RepoStateVisualiser _repoStateVisualiser = null!;

    private static GitItemStatus CreateGitItemStatus(
        bool isStaged = false,
        bool isTracked = true,
        bool isSubmodule = false)
    {
        return new GitItemStatus("file1")
        {
            Staged = isStaged ? StagedStatus.Index : StagedStatus.WorkTree,
            IsTracked = isTracked,
            IsSubmodule = isSubmodule
        };
    }

    [Test]
    public void ReturnsIconCleanWhenThereIsNoChangedFiles()
    {
        (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke([]);

        commitIcon.Should().Be(RepoStateVisualiser.Clean);
    }

    [Test]
    public void ReturnsIconDirtySubmodulesWhenThereAreOnlyWorkTreeSubmodules()
    {
        (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
        {
            CreateGitItemStatus(isSubmodule: true),
            CreateGitItemStatus(isSubmodule: true)
        });

        commitIcon.Should().Be(RepoStateVisualiser.DirtySubmodules);
    }

    [Test]
    public void ReturnsIconDirtyWhenThereAreWorkTreeChanges()
    {
        (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
        {
            CreateGitItemStatus(isSubmodule: true),
            CreateGitItemStatus()
        });

        commitIcon.Should().Be(RepoStateVisualiser.Dirty);
    }

    [Test]
    public void ReturnsIconMixedWhenThereAreIndexAndWorkTreeFiles()
    {
        (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
        {
            CreateGitItemStatus(isStaged: true),
            CreateGitItemStatus()
        });

        commitIcon.Should().Be(RepoStateVisualiser.Mixed);
    }

    [Test]
    public void ReturnsIconStagedWhenThereAreOnlyIndexFiles()
    {
        (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
        {
            CreateGitItemStatus(isStaged: true),
            CreateGitItemStatus(isStaged: true)
        });

        commitIcon.Should().Be(RepoStateVisualiser.Staged);
    }

    [Test]
    public void ReturnsIconUntrackedOnlyWhenThereAreUntrackedFilesOnly()
    {
        (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(new[]
        {
            CreateGitItemStatus(isTracked: false),
            CreateGitItemStatus(isTracked: false)
        });

        commitIcon.Should().Be(RepoStateVisualiser.UntrackedOnly);
    }

    [Test]
    public void ReturnsIconUnknownWhenNull()
    {
        (Image image, Brush brush) commitIcon = _repoStateVisualiser.Invoke(null);

        commitIcon.Should().Be(RepoStateVisualiser.Unknown);
    }
}
