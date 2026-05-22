using CommonTestUtils;
using GitUI.CommandsDialogs;

namespace GitUITests.CommandsDialogs;
public sealed class FormFileHistoryControllerTests
{
    private FormFileHistoryController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _controller = new FormFileHistoryController();
    }

    [TestCase(@"Does not exist")]
    [TestCase("")]
    [TestCase(" ")]
    public void TryGetExactPathName_Should_return_null_on_not_existing_file(string path)
    {
        string lowercasePath = path.ToLower();
        bool isExistingOnFileSystem = _controller.TryGetExactPath(lowercasePath, out string? exactPath);

        isExistingOnFileSystem.Should().BeFalse();
        exactPath.Should().BeNull();
    }

    [Test]
    public void TryGetExactPathName_Should_handle_network_path()
    {
        string path = @"\\" + Environment.MachineName.ToLower() + @"\c$\Windows\System32";

        string lowercasePath = path.ToLower();
        bool isExistingOnFileSystem = _controller.TryGetExactPath(lowercasePath, out string? exactPath);

        isExistingOnFileSystem.Should().BeTrue();

        exactPath.Should().Be(path);
    }

    [TestCase("Folder1\\file1.txt", true, true)]
    [TestCase("FOLDER1\\file1.txt", true, false)]
    [TestCase("fOLDER1\\file1.txt", true, false)]
    [TestCase("Folder2\\file1.txt", false, false)]
    public void TryGetExactPathName_should_check_if_path_matches_case(string relativePath, bool isResolved, bool doesMatch)
    {
        using GitModuleTestHelper repo = new();

        // Create a file
        string notUsed = repo.CreateFile(Path.Combine(repo.TemporaryPath, "Folder1"), "file1.txt", "bla");

        string expected = Path.Combine(repo.TemporaryPath, relativePath);

        _controller.TryGetExactPath(expected, out string? exactPath).Should().Be(isResolved);
        if (doesMatch)
        {
            exactPath.Should().Be(expected);
        }
        else
        {
            exactPath.Should().NotBe(expected);
        }
    }
}
