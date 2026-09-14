using GitCommands;
using GitCommands.Git;

namespace GitCommandsTests;

[Platform("Win")]
public sealed class PathUtilDirectoryTests
{
    [TestCase(@"C:\repos\main", @"c:\REPOS\main\", true)]
    [TestCase(@"C:\repos\main", @"C:/repos/main/", true)]
    [TestCase(@"C:\repos\main", @"C:\repos\other\..\main", true)]
    [TestCase(@"C:\", @"C:/", true)]
    [TestCase(@"C:\repos\main", @"C:\repos\main-linked", false)]
    public void AreSameDirectory_should_compare_normalized_paths(string path1, string path2, bool expected)
    {
        PathUtil.AreSameDirectory(path1, path2).Should().Be(expected);
    }

    [TestCase("")]
    [TestCase("repo")]
    public void AreSameDirectory_should_resolve_junctions_in_directory_and_parent_paths(string subdirectory)
    {
        DirectoryInfo temporaryDirectory = Directory.CreateTempSubdirectory();
        string target = Path.Combine(temporaryDirectory.FullName, "target");
        string junction = Path.Combine(temporaryDirectory.FullName, "junction");
        string other = Path.Combine(temporaryDirectory.FullName, "other");
        try
        {
            Directory.CreateDirectory(Path.Combine(target, subdirectory));
            Directory.CreateDirectory(other);
            Executable command = new("cmd.exe");
            command.Execute($"/c mklink /J \"{junction}\" \"{target}\"").ThrowIfErrorExit();

            string targetPath = Path.Combine(target, subdirectory);
            string junctionPath = Path.Combine(junction, subdirectory);
            PathUtil.AreSameDirectory(junctionPath, targetPath).Should().BeTrue();
            PathUtil.AreSameDirectory(targetPath, junctionPath + Path.DirectorySeparatorChar).Should().BeTrue();
            PathUtil.AreSameDirectory(junctionPath, other).Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(junction))
            {
                Directory.Delete(junction);
            }

            temporaryDirectory.Delete(recursive: true);
        }
    }

    [Test]
    public void AreSameDirectory_should_handle_missing_directories()
    {
        string missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        PathUtil.AreSameDirectory(missingPath, missingPath + Path.DirectorySeparatorChar).Should().BeTrue();
        PathUtil.AreSameDirectory(missingPath, Path.GetTempPath()).Should().BeFalse();
    }
}
