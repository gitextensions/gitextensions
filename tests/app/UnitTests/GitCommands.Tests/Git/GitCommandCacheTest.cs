using GitCommands;

namespace GitCommandsTests.Git;

[TestFixture]
public class GitCommandCacheTest
{
    private CommandCache _cache;

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
        string[] expectedCachedCommand = { "git command" };

        _cache.Add("git command", output, error);

        ClassicAssert.IsTrue(expectedCachedCommand.SequenceEqual(_cache.GetCachedCommands()));
    }

    [Test]
    public void TestAddCannotCache()
    {
        _cache.Add(null, null, null);
        ClassicAssert.IsFalse(_cache.GetCachedCommands().Any());
    }

    [Test]
    public void TestTryGet()
    {
        string originalOutput = "Another Git result";
        string originalError = "Still no Git error.";

        _cache.Add("git command", originalOutput, originalError);

        ClassicAssert.IsTrue(_cache.TryGet("git command", out string? cachedOutput, out string? cachedError));
        ClassicAssert.AreEqual(cachedOutput, originalOutput);
        ClassicAssert.AreEqual(cachedError, originalError);
    }

    [Test]
    public void TestTryGetFails()
    {
        ClassicAssert.IsFalse(_cache.TryGet(null, out string? output, out string? error));
        ClassicAssert.IsFalse(_cache.TryGet("", out output, out error));
        ClassicAssert.IsNull(output);
        ClassicAssert.IsNull(error);
    }
}
