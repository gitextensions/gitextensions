using FluentAssertions;
using GitCommands.Git;
using GitUI;
using NSubstitute;

namespace GitUITests;

[TestFixture]
public sealed class RepositoryCurrentBranchNameCacheTests
{
    private const string Path = @"C:\repo";
    private const string BranchName = "main";

    private IRepositoryCurrentBranchNameProvider _inner = null!;
    private IRepositoryCurrentBranchNameCache _cache = null!;

    [SetUp]
    public void SetUp()
    {
        _inner = Substitute.For<IRepositoryCurrentBranchNameProvider>();
        _cache = new RepositoryCurrentBranchNameCache(_inner);
    }

    [Test]
    public void GetCurrentBranchName_should_delegate_to_inner_on_cache_miss()
    {
        _inner.GetCurrentBranchName(Path).Returns(BranchName);

        string result = _cache.GetCurrentBranchName(Path);

        result.Should().Be(BranchName);
        _inner.Received(1).GetCurrentBranchName(Path);
    }

    [Test]
    public void GetCurrentBranchName_should_not_call_inner_on_cache_hit()
    {
        _inner.GetCurrentBranchName(Path).Returns(BranchName);
        _cache.GetCurrentBranchName(Path);
        _inner.ClearReceivedCalls();

        string result = _cache.GetCurrentBranchName(Path);

        result.Should().Be(BranchName);
        _inner.DidNotReceive().GetCurrentBranchName(Arg.Any<string>());
    }

    [Test]
    public void GetCurrentBranchName_should_cache_result_after_first_call()
    {
        _inner.GetCurrentBranchName(Path).Returns(BranchName);
        _cache.GetCurrentBranchName(Path);

        string? cached = _cache.GetCachedBranchName(Path);

        cached.Should().Be(BranchName);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void GetCurrentBranchName_should_not_cache_blank_branch_name(string? branchName)
    {
        _inner.GetCurrentBranchName(Path).Returns(branchName ?? string.Empty);

        _cache.GetCurrentBranchName(Path);

        _cache.GetCachedBranchName(Path).Should().BeNull();
    }

    [Test]
    public void GetCurrentBranchName_should_not_cache_unknown_branch_name()
    {
        _inner.GetCurrentBranchName(Path).Returns(DetachedHeadParser.UnknownBranchName);

        _cache.GetCurrentBranchName(Path);

        _cache.GetCachedBranchName(Path).Should().BeNull();
    }

    [Test]
    public void GetCachedBranchName_should_return_null_before_any_call()
    {
        _cache.GetCachedBranchName(Path).Should().BeNull();
    }

    [Test]
    public void GetCachedBranchName_should_return_cached_value_after_get()
    {
        _inner.GetCurrentBranchName(Path).Returns(BranchName);
        _cache.GetCurrentBranchName(Path);

        _cache.GetCachedBranchName(Path).Should().Be(BranchName);
    }

    [Test]
    public void UpdateCache_should_store_branch_name()
    {
        _cache.UpdateCache(Path, BranchName);

        _cache.GetCachedBranchName(Path).Should().Be(BranchName);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void UpdateCache_should_evict_blank_branch_name(string branchName)
    {
        _cache.UpdateCache(Path, BranchName);

        _cache.UpdateCache(Path, branchName);

        _cache.GetCachedBranchName(Path).Should().BeNull();
    }

    [Test]
    public void UpdateCache_should_evict_unknown_branch_name()
    {
        _cache.UpdateCache(Path, BranchName);

        _cache.UpdateCache(Path, DetachedHeadParser.UnknownBranchName);

        _cache.GetCachedBranchName(Path).Should().BeNull();
    }

    [Test]
    public void UpdateCache_should_overwrite_existing_entry()
    {
        const string newBranch = "feature/x";
        _cache.UpdateCache(Path, BranchName);

        _cache.UpdateCache(Path, newBranch);

        _cache.GetCachedBranchName(Path).Should().Be(newBranch);
    }

    [Test]
    public void InvalidateAll_should_clear_all_entries()
    {
        const string otherPath = @"C:\other";
        _cache.UpdateCache(Path, BranchName);
        _cache.UpdateCache(otherPath, "develop");

        _cache.InvalidateAll();

        _cache.GetCachedBranchName(Path).Should().BeNull();
        _cache.GetCachedBranchName(otherPath).Should().BeNull();
    }

    [Test]
    public void InvalidateAll_should_allow_inner_to_be_called_again_after_invalidation()
    {
        _inner.GetCurrentBranchName(Path).Returns(BranchName);
        _cache.GetCurrentBranchName(Path);
        _cache.InvalidateAll();
        _inner.ClearReceivedCalls();

        _cache.GetCurrentBranchName(Path);

        _inner.Received(1).GetCurrentBranchName(Path);
    }

    [Test]
    public void GetCurrentBranchName_should_cache_independently_per_path()
    {
        const string otherPath = @"C:\other";
        _inner.GetCurrentBranchName(Path).Returns(BranchName);
        _inner.GetCurrentBranchName(otherPath).Returns("develop");

        _cache.GetCurrentBranchName(Path);
        _cache.GetCurrentBranchName(otherPath);

        _cache.GetCachedBranchName(Path).Should().Be(BranchName);
        _cache.GetCachedBranchName(otherPath).Should().Be("develop");
    }
}
