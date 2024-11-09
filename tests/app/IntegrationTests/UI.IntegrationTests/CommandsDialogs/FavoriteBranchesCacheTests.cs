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

    [SetUp]
    public void Setup()
    {
        ServiceContainer serviceContainer = new();
        _file = Substitute.For<FileBase>();
        _fileSystem = Substitute.For<IFileSystem>();
        _fileSystem.File.Returns(_file);
        serviceContainer.AddService(_fileSystem);

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
    public void CleanUp_ShouldRemoveNonExistingBranches()
    {
        ObjectId? objectId = ObjectId.Random();
        string? branchName = "branch";
        _cache.Add(objectId, branchName);

        IGitRef? gitRefs = Substitute.For<IGitRef>();
        gitRefs.ObjectId.Returns(ObjectId.Random());
        gitRefs.Name.Returns("otherBranch");

        List<IGitRef>? branches = new() { gitRefs };

        // Act
        _cache.CleanUp(branches);

        // Assert
        _cache.Contains(objectId, branchName).Should().BeFalse();
    }

    [Test]
    public void Load_ShouldLoadFavorites_WhenFileExists()
    {
        // Arrange
        ObjectId objectId = ObjectId.Random();
        string branchName = "branch";

        string fileContent = $"[{{\"ObjectId\":\"{objectId}\",\"Name\":\"{branchName}\"}}]";
        int times = 0;
        _fileSystem.File.Exists(Arg.Any<string>()).Returns(info => times++ == 0);
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
}
