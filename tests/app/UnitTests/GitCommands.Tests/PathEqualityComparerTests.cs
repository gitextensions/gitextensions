using GitCommands;

namespace GitCommandsTests;
public class PathEqualityComparerTests
{
    private PathEqualityComparer _comparer = null!;

    [SetUp]
    public void Setup()
    {
        _comparer = new PathEqualityComparer();
    }

    [TestCase("C:\\WORK\\GitExtensions\\", "C:/Work/GitExtensions/")]
    [TestCase("\\\\my-pc\\Work\\GitExtensions\\", "//my-pc/WORK/GitExtensions/")]
    public void Equals(string input, string expected)
    {
        true.Should().Be(_comparer.Equals(input, expected));
    }
}
