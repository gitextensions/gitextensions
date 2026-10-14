using System.Text;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls;

[TestFixture]
public class DiffProviderTests
{
    private string _workingDir = null!;

    [SetUp]
    public void SetUp()
    {
        _workingDir = Path.Combine(Path.GetTempPath(), nameof(DiffProviderTests), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workingDir);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_workingDir, recursive: true);
    }

    [Test]
    public async Task AppendDiffsAsync_should_format_untracked_file_as_synthetic_diff()
    {
        await File.WriteAllLinesAsync(Path.Combine(_workingDir, "new.txt"), ["first", "second"]);
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDir);
        FileStatusItem item = new(firstRev: null, new GitRevision(ObjectId.WorkTreeId), new GitItemStatus("new.txt") { IsTracked = false });
        StringBuilder sb = new();

        await DiffProvider.AppendDiffsAsync(sb, [item], module, contextLines: 3);

        await Verify(sb.ToString());
    }

    [Test]
    public async Task AppendDiffsAsync_should_skip_missing_untracked_file()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDir);
        FileStatusItem item = new(firstRev: null, new GitRevision(ObjectId.WorkTreeId), new GitItemStatus("missing.txt") { IsTracked = false });
        StringBuilder sb = new();

        await DiffProvider.AppendDiffsAsync(sb, [item], module, contextLines: 3);

        sb.ToString().Should().BeEmpty();
    }

    [Test]
    public async Task AppendDiffsAsync_should_format_untracked_symlink_as_link_target()
    {
        const string linkTarget = "target.txt";
        try
        {
            File.CreateSymbolicLink(Path.Combine(_workingDir, "link.txt"), linkTarget);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            Assert.Ignore("Creating symbolic links is not permitted in this environment.");
        }

        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDir);
        FileStatusItem item = new(firstRev: null, new GitRevision(ObjectId.WorkTreeId), new GitItemStatus("link.txt") { IsTracked = false });
        StringBuilder sb = new();

        await DiffProvider.AppendDiffsAsync(sb, [item], module, contextLines: 3);

        sb.ToString().Should().Be($"link.txt -> {linkTarget}{Environment.NewLine}");
    }
}
