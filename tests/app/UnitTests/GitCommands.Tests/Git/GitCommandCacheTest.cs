using GitCommands;

namespace GitCommandsTests.Git;
public class GitCommandCacheTest
{
    private CommandCache _cache = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new CommandCache();
    }

    [Test]
    public void TestAdd()
    {
        string output = "Git result";
        string error = "No Git error!";
        string[] expectedCachedCommand = ["git command"];

        _cache.Add("git command", output, error);

        expectedCachedCommand.SequenceEqual(_cache.GetCachedCommands()).Should().BeTrue();
    }

    [Test]
    public void TestAddCannotCache()
    {
        _cache.Add(null, null!, null!);
        _cache.GetCachedCommands().Any().Should().BeFalse();
    }

    [Test]
    public void TestTryGet()
    {
        string originalOutput = "Another Git result";
        string originalError = "Still no Git error.";

        _cache.Add("git command", originalOutput, originalError);

        _cache.TryGet("git command", out string? cachedOutput, out string? cachedError).Should().BeTrue();
        originalOutput.Should().Be(cachedOutput);
        originalError.Should().Be(cachedError);
    }

    [Test]
    public void TestTryGetFails()
    {
        _cache.TryGet(null, out string? output, out string? error).Should().BeFalse();
        _cache.TryGet("", out output, out error).Should().BeFalse();
        output.Should().BeNull();
        error.Should().BeNull();
    }
}
