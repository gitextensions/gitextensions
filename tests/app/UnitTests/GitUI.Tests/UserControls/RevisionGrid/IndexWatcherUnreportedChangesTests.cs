using GitUI.UserControls.RevisionGrid;

namespace GitUITests.UserControls.RevisionGrid;

[TestFixture]
public sealed class IndexWatcherUnreportedChangesTests
{
    private static readonly DateTime _past = DateTime.UtcNow.AddHours(-1);
    private string _gitDir = null!;

    [SetUp]
    public void SetUp()
    {
        _gitDir = Path.Join(Path.GetTempPath(), "GitExtensionsTests", Path.GetRandomFileName());
        Directory.CreateDirectory(Path.Join(_gitDir, "refs", "heads", "feature"));
        Directory.CreateDirectory(Path.Join(_gitDir, "refs", "tags"));
        File.WriteAllText(Path.Join(_gitDir, "index"), "");
        File.WriteAllText(Path.Join(_gitDir, "HEAD"), "ref: refs/heads/master");
        File.WriteAllText(Path.Join(_gitDir, "packed-refs"), "");
        File.WriteAllText(Path.Join(_gitDir, "refs", "heads", "feature", "branch"), "");

        foreach (string file in Directory.EnumerateFiles(_gitDir, "*", SearchOption.AllDirectories))
        {
            File.SetLastWriteTimeUtc(file, _past);
        }

        foreach (string directory in Directory.EnumerateDirectories(_gitDir, "*", SearchOption.AllDirectories))
        {
            Directory.SetLastWriteTimeUtc(directory, _past);
        }
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_gitDir, recursive: true);
    }

    [Test]
    public void HasChangedSince_should_return_false_if_nothing_changed()
    {
        IndexWatcher.TestAccessor.HasChangedSince(_gitDir, _gitDir, _past.AddMinutes(1)).Should().BeFalse();
    }

    [TestCase("index")]
    [TestCase("HEAD")]
    [TestCase("packed-refs")]
    public void HasChangedSince_should_return_true_if_file_changed(string fileName)
    {
        File.SetLastWriteTimeUtc(Path.Join(_gitDir, fileName), _past.AddMinutes(2));

        IndexWatcher.TestAccessor.HasChangedSince(_gitDir, _gitDir, _past.AddMinutes(1)).Should().BeTrue();
    }

    [TestCase("refs")]
    [TestCase("refs/tags")]
    [TestCase("refs/heads/feature")]
    public void HasChangedSince_should_return_true_if_refs_directory_changed(string directory)
    {
        Directory.SetLastWriteTimeUtc(Path.Join(_gitDir, directory), _past.AddMinutes(2));

        IndexWatcher.TestAccessor.HasChangedSince(_gitDir, _gitDir, _past.AddMinutes(1)).Should().BeTrue();
    }

    [Test]
    public void HasChangedSince_should_return_true_if_loose_ref_updated_by_rename()
    {
        string refPath = Path.Join(_gitDir, "refs", "heads", "feature", "branch");
        File.WriteAllText($"{refPath}.lock", "");
        File.Move($"{refPath}.lock", refPath, overwrite: true);

        IndexWatcher.TestAccessor.HasChangedSince(_gitDir, _gitDir, _past.AddMinutes(1)).Should().BeTrue();
    }

    [Test]
    public void HasChangedSince_should_return_false_if_refs_directory_missing()
    {
        Directory.Delete(Path.Join(_gitDir, "refs"), recursive: true);

        IndexWatcher.TestAccessor.HasChangedSince(_gitDir, _gitDir, _past.AddMinutes(1)).Should().BeFalse();
    }
}
