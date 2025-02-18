using FluentAssertions;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using NSubstitute;

namespace GitCommandsTests.UserRepositoryHistory;

[TestFixture]
public class RepositoryDescriptionProviderTests
{
    private string _tempDir;

    [SetUp]
    public void Setup()
    {
        _tempDir = Directory.CreateTempSubdirectory().FullName;
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    [Test]
    public void RepositoryDescriptionProvider_should_handle_subrepos()
    {
        IGitDirectoryResolver gitDirectoryResolver = Substitute.For<IGitDirectoryResolver>();
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);

        string repo = @$"{_tempDir}\test_repo";
        string submodule = @$"{repo}\submodule";
        string nested = @$"{submodule}\nested";
        string subsubmodule = @$"{nested}\subsubmodule";
        string leafsubmodule = @$"{nested}\leafsubmodule";
        Directory.CreateDirectory(leafsubmodule);

        repositoryDescriptionProvider.Get(repo, IsValidGitWorkingDir)
            .Should().Be($"test_repo");

        repositoryDescriptionProvider.Get(submodule, IsValidGitWorkingDir)
            .Should().Be("submodule < test_repo");

        repositoryDescriptionProvider.Get(subsubmodule, IsValidGitWorkingDir)
            .Should().Be("subsubmodule < test_repo");

        repositoryDescriptionProvider.Get(leafsubmodule, IsValidGitWorkingDir)
            .Should().Be("leafsubmodule < test_repo");
    }

    [Test]
    public void RepositoryDescriptionProvider_should_skip_uninformative_submodule_name([Values("app", "repo", "repository")] string uninformative)
    {
        IGitDirectoryResolver gitDirectoryResolver = Substitute.For<IGitDirectoryResolver>();
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);

        string rootrepo = nameof(rootrepo);
        string parent = nameof(parent);
        string repo = @$"{_tempDir}\{rootrepo}\{parent}\{uninformative}";
        Directory.CreateDirectory(repo);

        repositoryDescriptionProvider.Get(repo, IsValidGitWorkingDir)
            .Should().Be($"{parent} < {rootrepo}");
    }

    [Test]
    public void RepositoryDescriptionProvider_should_not_skip_uninformative_submodule_name_to_parent_repo([Values("app", "repo", "repository")] string uninformative)
    {
        IGitDirectoryResolver gitDirectoryResolver = Substitute.For<IGitDirectoryResolver>();
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);

        string parentrepo = nameof(parentrepo);
        string repo = @$"{_tempDir}\{parentrepo}\{uninformative}";
        Directory.CreateDirectory(repo);

        repositoryDescriptionProvider.Get(repo, IsValidGitWorkingDir)
            .Should().Be($"{uninformative} < {parentrepo}");
    }

    [Test]
    public void RepositoryDescriptionProvider_should_not_skip_uninformative_root_repo_name([Values("app", "repo", "repository")] string uninformative)
    {
        IGitDirectoryResolver gitDirectoryResolver = Substitute.For<IGitDirectoryResolver>();
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);

        string parent = nameof(parent);
        string repo = @$"{_tempDir}\{parent}\{uninformative}";
        Directory.CreateDirectory(repo);

        repositoryDescriptionProvider.Get(repo, IsValidGitWorkingDir)
            .Should().Be($"{uninformative}");
    }

    private static bool IsValidGitWorkingDir(string path)
        => path.EndsWith("submodule") || path.EndsWith("repo");
}
