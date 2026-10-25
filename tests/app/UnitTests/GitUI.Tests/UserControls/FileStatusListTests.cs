using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls;

[TestFixture]
public class FileStatusListTests
{
    private string _workingDir;
    private IFullPathResolver _fullPathResolver;

    [SetUp]
    public void Setup()
    {
        _workingDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_workingDir);

        _fullPathResolver = Substitute.For<IFullPathResolver>();
        _fullPathResolver.Resolve(Arg.Any<string>()).Returns(callInfo => Path.Combine(_workingDir, callInfo.Arg<string>()));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_workingDir))
        {
            Directory.Delete(_workingDir, recursive: true);
        }
    }

    [Test]
    public void HasFilesWhichMayBeDeleted_should_return_false_for_a_file_deleted_in_the_revision()
    {
        GitItemStatus status = new("deleted.txt") { IsDeleted = true, IsTracked = true };

        FileStatusList.HasFilesWhichMayBeDeleted([Item(status)], _fullPathResolver).Should().BeFalse();
    }

    [Test]
    public void HasFilesWhichMayBeDeleted_should_return_true_for_a_path_which_exists_in_the_working_directory()
    {
        File.WriteAllText(Path.Combine(_workingDir, "deleted.txt"), "an untracked file at the same path");
        GitItemStatus status = new("deleted.txt") { IsDeleted = true, IsTracked = true };

        FileStatusList.HasFilesWhichMayBeDeleted([Item(status)], _fullPathResolver).Should().BeTrue();
    }

    [Test]
    public void HasFilesWhichMayBeDeleted_should_return_true_for_a_directory_which_exists_in_the_working_directory()
    {
        Directory.CreateDirectory(Path.Combine(_workingDir, "submodule"));
        GitItemStatus status = new("submodule") { IsNew = true, IsSubmodule = true };

        FileStatusList.HasFilesWhichMayBeDeleted([Item(status)], _fullPathResolver).Should().BeTrue();
    }

    [Test]
    public void HasFilesWhichMayBeDeleted_should_return_false_for_changed_files()
    {
        File.WriteAllText(Path.Combine(_workingDir, "changed.txt"), "the file which is about to be reset");
        GitItemStatus status = new("changed.txt") { IsChanged = true, IsTracked = true };

        FileStatusList.HasFilesWhichMayBeDeleted([Item(status)], _fullPathResolver).Should().BeFalse();
    }

    [Test]
    public void HasFilesWhichMayBeDeleted_should_return_true_when_only_the_old_name_of_a_rename_exists()
    {
        File.WriteAllText(Path.Combine(_workingDir, "old.txt"), "the name the file had before the rename");
        GitItemStatus status = new("new.txt") { IsRenamed = true, IsTracked = true, OldName = "old.txt" };

        FileStatusList.HasFilesWhichMayBeDeleted([Item(status)], _fullPathResolver).Should().BeTrue();
    }

    [Test]
    public void HasFilesWhichMayBeDeleted_should_return_true_if_any_item_may_be_deleted()
    {
        File.WriteAllText(Path.Combine(_workingDir, "added.txt"), "a file which was added");
        GitItemStatus deleted = new("deleted.txt") { IsDeleted = true, IsTracked = true };
        GitItemStatus added = new("added.txt") { IsNew = true };

        FileStatusList.HasFilesWhichMayBeDeleted([Item(deleted), Item(added)], _fullPathResolver).Should().BeTrue();
    }

    private static FileStatusItem Item(GitItemStatus status)
        => new(firstRev: null, secondRev: new GitRevision(ObjectId.WorkTreeId), status);
}
