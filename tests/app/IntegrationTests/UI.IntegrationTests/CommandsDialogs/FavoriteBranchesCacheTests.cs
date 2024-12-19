using System.ComponentModel.Design;
using System.IO.Abstractions;
using FluentAssertions;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.LeftPanel;
using NSubstitute;

namespace UITests.CommandsDialogs;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class FavoriteBranchesCacheTests
{
    private FavoriteBranchesCache _cache;
    private IFileSystem _fileSystem;
    private FileBase _file;
    private IDirectory _directory;

    [SetUp]
    public void Setup()
    {
        ServiceContainer serviceContainer = new();
        _file = Substitute.For<FileBase>();
        _directory = Substitute.For<IDirectory>();
        _fileSystem = Substitute.For<IFileSystem>();
        _fileSystem.File.Returns(_file);
        _fileSystem.Directory.Returns(_directory);
        serviceContainer.AddService(_fileSystem);
        _directory.Exists(Arg.Any<string>()).Returns(true);
        _cache = new FavoriteBranchesCache(serviceContainer);
    }

    [Test]
    public void Add_ShouldAddBranchToFavorites_WhenValid()
    {
        ObjectId? objectId = ObjectId.Random();
        string? branchName = "branch";

        // Act
        _cache.Add(objectId, branchName);

        // Assert
        _cache.Contains(objectId, branchName).Should().BeTrue();
    }

    [Test]
    public void Add_ShouldNotAddBranch_WhenInvalid()
    {
        // Act
        _cache.Add(default, null);

        // Assert
        _cache.Contains(default, null).Should().BeFalse();
    }

    [Test]
    public void Remove_ShouldRemoveBranchFromFavorites_WhenExists()
    {
        ObjectId? objectId = ObjectId.Random();
        string? branchName = "branch";
        _cache.Add(objectId, branchName);

        // Act
        _cache.Remove(objectId, branchName);

        // Assert
        _cache.Contains(objectId, branchName).Should().BeFalse();
    }

    [Test]
    public void Remove_ShouldNotRemoveBranch_WhenNotExists()
    {
        // Act
        _cache.Remove(ObjectId.Random(), "branch");

        // Assert
        _cache.Contains(ObjectId.Random(), "branch").Should().BeFalse();
    }

    [Test]
    public void Load_ShouldLoadFavorites_WhenFileExists()
    {
        // Arrange
        ObjectId objectId = ObjectId.Random();
        string branchName = "branch";

        string fileContent = $"[{{\"ObjectId\":\"{objectId}\",\"Name\":\"{branchName}\"}}]";
        _fileSystem.File.Exists(Arg.Any<string>()).Returns(true);
        _fileSystem.File.ReadAllText(Arg.Any<string>()).Returns(fileContent);

        // Act
        _cache.Load();

        // Assert
        _cache.Contains(objectId, branchName).Should().BeTrue();
    }

    [Test]
    public void Save_ShouldSaveFavorites_WhenFavoritesExist()
    {
        ObjectId? objectId = ObjectId.Random();
        string? branchName = "branch";
        _cache.Add(objectId, branchName);
        _file.ClearReceivedCalls();

        // Act
        _cache.Save();

        // Assert
        _fileSystem.File.Received(1).WriteAllText(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void Synchronize_ShouldMatchFavoritesCorrectly()
    {
        // Arrange
        ObjectId objectId1 = ObjectId.Random();
        ObjectId objectId2 = ObjectId.Random();
        string branchName1 = "branch1";
        string branchName2 = "branch2";
        string branchName3 = "branch3";

        _cache.Add(objectId1, branchName1);
        _cache.Add(objectId2, branchName2);

        var gitRef1 = CreateMockGitRef(objectId1, branchName1);
        var gitRef3 = CreateMockGitRef(ObjectId.Random(), branchName3);

        var gitRefs = new List<IGitRef> { gitRef1, gitRef3 };

        // Act
        var matches = _cache.Synchronize(gitRefs, out IList<BranchIdentifier>? noMatches);

        // Assert
        matches.Should().ContainSingle(r => r.ObjectId == objectId1 && r.Name == branchName1);
        noMatches.Should().ContainSingle(f => f.Name == branchName2);
    }

    [Test]
    public void ConfigFile_ShouldThrow_WhenPathIsInvalid()
    {
        // Arrange
        _fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns("invalid\\path");
        _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(false);

        // Act & Assert
        Action act = () => { var file = _cache.ConfigFile; };
        act.Should().Throw<DirectoryNotFoundException>();
    }

    [Test]
    public void Load_ShouldHandleFileNotAccessible()
    {
        // Arrange
        _fileSystem.File.Exists(Arg.Any<string>()).Returns(true);
        _fileSystem.File.When(x => x.ReadAllText(Arg.Any<string>())).Do(_ => throw new IOException());

        // Act
        Action act = () => _cache.Load();

        // Assert
        act.Should().NotThrow(); // Should fail gracefully
    }

    [Test]
    public void GetLatestCommitId_ShouldReturnNull_WhenRevParseFails()
    {
        // Arrange
        var mockGitModule = Substitute.For<IGitModule>();
        mockGitModule.RevParse(Arg.Any<string>()).Returns((ObjectId?)null);

        // Act
        var result = _cache.GetLatestCommitId(mockGitModule, "branchName");

        // Assert
        result.Should().BeNull();
    }

    private IGitRef CreateMockGitRef(ObjectId objectId, string name)
    {
        var mockGitRef = Substitute.For<IGitRef>();
        mockGitRef.ObjectId.Returns(objectId);
        mockGitRef.Name.Returns(name);
        mockGitRef.Module.Returns(Substitute.For<IGitModule>());
        return mockGitRef;
    }
}
