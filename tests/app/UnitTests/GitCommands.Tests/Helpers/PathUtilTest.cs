using FluentAssertions;
using GitCommands;

namespace GitCommandsTests.Helpers;

[TestFixture]
public class PathUtilTest
{
    [Test]
    public void ToPosixPathTest()
    {
        if (Path.DirectorySeparatorChar == '\\')
        {
            ClassicAssert.AreEqual("C:/Work/GitExtensions/".ToPosixPath(), "C:/Work/GitExtensions/");
            ClassicAssert.AreEqual("C:\\Work\\GitExtensions\\".ToPosixPath(), "C:/Work/GitExtensions/");
        }
        else
        {
            ClassicAssert.AreEqual("C:/Work/GitExtensions/".ToPosixPath(), "C:/Work/GitExtensions/");
            ClassicAssert.AreEqual("C:\\Work\\GitExtensions\\".ToPosixPath(), "C:\\Work\\GitExtensions\\");
            ClassicAssert.AreEqual("/var/tmp/".ToPosixPath(), "/var/tmp/");
        }
    }

    [Test]
    public void ToNativePathTest()
    {
        if (Path.DirectorySeparatorChar == '\\')
        {
            ClassicAssert.AreEqual("C:\\Work\\GitExtensions\\".ToNativePath(), "C:\\Work\\GitExtensions\\");
            ClassicAssert.AreEqual("C:/Work/GitExtensions/".ToNativePath(), "C:\\Work\\GitExtensions\\");
            ClassicAssert.AreEqual("\\\\my-pc\\Work\\GitExtensions\\".ToNativePath(), "\\\\my-pc\\Work\\GitExtensions\\");
        }
        else
        {
            ClassicAssert.AreEqual("C:\\Work\\GitExtensions\\".ToNativePath(), "C:\\Work\\GitExtensions\\");
            ClassicAssert.AreEqual("/Work/GitExtensions/".ToNativePath(), "/Work/GitExtensions/");
            ClassicAssert.AreEqual("//server/share/".ToNativePath(), "//server/share/");
        }
    }

    [Test]
    public void ToWslPathTest()
    {
        ClassicAssert.AreEqual(PathUtil.ToWslPath(null), null);
        ClassicAssert.AreEqual(@"C:/Work/GitExtensions/".ToWslPath(), "/mnt/c/Work/GitExtensions/");
        ClassicAssert.AreEqual(@"C:\Work\GitExtensions\".ToWslPath(), "/mnt/c/Work/GitExtensions/");
        ClassicAssert.AreEqual(@"/var/tmp/".ToWslPath(), "/var/tmp/");
    }

    [Test]
    public void ToCygwinPathTest()
    {
        ClassicAssert.AreEqual(PathUtil.ToCygwinPath(null), null);
        ClassicAssert.AreEqual(@"C:/Work/GitExtensions/".ToCygwinPath(), "/cygdrive/c/Work/GitExtensions/");
        ClassicAssert.AreEqual(@"C:\Work\GitExtensions\".ToCygwinPath(), "/cygdrive/c/Work/GitExtensions/");
        ClassicAssert.AreEqual(@"/var/tmp/".ToCygwinPath(), "/var/tmp/");
    }

    [Test]
    public void ToMountPathTest()
    {
        const string prefix = "PrEfiX";
        ClassicAssert.AreEqual(PathUtil.ToMountPath(null, prefix), null);
        ClassicAssert.AreEqual(@"".ToMountPath(prefix), "");
        ClassicAssert.AreEqual(@"C".ToMountPath(prefix), "C");
        ClassicAssert.AreEqual(@".:".ToMountPath(prefix), $".:");
        ClassicAssert.AreEqual(@"C:".ToMountPath(prefix), $"{prefix}c");
        ClassicAssert.AreEqual(@"C:_".ToMountPath(prefix), $"{prefix}c_");
        ClassicAssert.AreEqual(@"C:\".ToMountPath(prefix), $"{prefix}c/");
        ClassicAssert.AreEqual(@"C:/".ToMountPath(prefix), $"{prefix}c/");
        ClassicAssert.AreEqual(@"C:\folder".ToMountPath(prefix), $"{prefix}c/folder");
        ClassicAssert.AreEqual(@"C:/Work/GitExtensions/".ToMountPath(prefix), $"{prefix}c/Work/GitExtensions/");
        ClassicAssert.AreEqual(@"C:\Work\GitExtensions\".ToMountPath(prefix), $"{prefix}c/Work/GitExtensions/");
        ClassicAssert.AreEqual(@"/var/tmp/".ToMountPath(prefix), "/var/tmp/");
    }

    [Test]
    public void EnsureTrailingPathSeparatorTest()
    {
        ClassicAssert.IsNull(((string)null).EnsureTrailingPathSeparator());
        ClassicAssert.AreEqual("".EnsureTrailingPathSeparator(), "");

        if (Path.DirectorySeparatorChar == '\\')
        {
            ClassicAssert.AreEqual("C".EnsureTrailingPathSeparator(), "C\\");
            ClassicAssert.AreEqual("C:".EnsureTrailingPathSeparator(), "C:\\");
            ClassicAssert.AreEqual("C:\\".EnsureTrailingPathSeparator(), "C:\\");
            ClassicAssert.AreEqual("C:\\Work\\GitExtensions".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
            ClassicAssert.AreEqual("C:\\Work\\GitExtensions\\".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
            ClassicAssert.AreEqual("C:/Work/GitExtensions/".EnsureTrailingPathSeparator(), "C:/Work/GitExtensions/");
            ClassicAssert.AreEqual("\\".EnsureTrailingPathSeparator(), "\\");
            ClassicAssert.AreEqual("/".EnsureTrailingPathSeparator(), "/");
        }
        else
        {
            ClassicAssert.AreEqual("/".EnsureTrailingPathSeparator(), "/");
            ClassicAssert.AreEqual("/Work/GitExtensions".EnsureTrailingPathSeparator(), "/Work/GitExtensions/");
            ClassicAssert.AreEqual("/Work/GitExtensions/".EnsureTrailingPathSeparator(), "/Work/GitExtensions/");
            ClassicAssert.AreEqual("/Work/GitExtensions\\".EnsureTrailingPathSeparator(), "/Work/GitExtensions\\/");
        }
    }

    [Test]
    public void RemoveTrailingPathSeparatorTest()
    {
        ClassicAssert.IsNull(((string)null).RemoveTrailingPathSeparator());
        ClassicAssert.AreEqual("".RemoveTrailingPathSeparator(), "");

        char s = Path.DirectorySeparatorChar;

        ClassicAssert.AreEqual($"C:{s}".RemoveTrailingPathSeparator(), "C:");
        ClassicAssert.AreEqual("foo".RemoveTrailingPathSeparator(), "foo");
        ClassicAssert.AreEqual($"foo{s}".RemoveTrailingPathSeparator(), "foo");
        ClassicAssert.AreEqual($"foo{s}bar".RemoveTrailingPathSeparator(), $"foo{s}bar");
        ClassicAssert.AreEqual($"foo{s}bar{s}".RemoveTrailingPathSeparator(), $"foo{s}bar");

        ClassicAssert.AreEqual("foo/".RemoveTrailingPathSeparator(), "foo");
        ClassicAssert.AreEqual("foo/bar".RemoveTrailingPathSeparator(), "foo/bar");
        ClassicAssert.AreEqual("foo/bar/".RemoveTrailingPathSeparator(), "foo/bar");
    }

    [Test]
    public void IsLocalFileTest()
    {
        ClassicAssert.AreEqual(PathUtil.IsLocalFile("\\\\my-pc\\Work\\GitExtensions"), true);
        ClassicAssert.AreEqual(PathUtil.IsLocalFile("//my-pc/Work/GitExtensions"), true);
        ClassicAssert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions"), true);
        ClassicAssert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions\\"), true);
        ClassicAssert.AreEqual(PathUtil.IsLocalFile("/Work/GitExtensions"), true);
        ClassicAssert.AreEqual(PathUtil.IsLocalFile("/Work/GitExtensions/"), true);
        ClassicAssert.AreEqual(PathUtil.IsLocalFile("ssh://domain\\user@serverip/cache/git/something/something.git"), false);
    }

    [Test]
    public void GetFileNameTest()
    {
        if (Path.DirectorySeparatorChar == '\\')
        {
            ClassicAssert.AreEqual(PathUtil.GetFileName("\\\\my-pc\\Work\\GitExtensions"), "GitExtensions");
            ClassicAssert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions"), "GitExtensions");
            ClassicAssert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions\\"), "");
        }
        else
        {
            ClassicAssert.AreEqual(PathUtil.GetFileName("//my-pc/Work/GitExtensions"), "GitExtensions");
            ClassicAssert.AreEqual(PathUtil.GetFileName("/Work/GitExtensions"), "GitExtensions");
            ClassicAssert.AreEqual(PathUtil.GetFileName("/Work/GitExtensions/"), "");
        }
    }

    [Test]
    public void GetRepositoryNameTest()
    {
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("https://github.com/gitextensions/gitextensions.git"), "gitextensions");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("https://github.com/jeffqc/gitextensions"), "gitextensions");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("git://mygitserver/git/test.git"), "test");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("ssh://mygitserver/git/test.git"), "test");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("ssh://john.doe@mygitserver/git/test.git"), "test");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("ssh://john-abraham.doe@mygitserver/git/MyAwesomeRepo.git"), "MyAwesomeRepo");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("git@anotherserver.mysubnet.com:project/somerepo.git"), "somerepo");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("http://anotherserver.mysubnet.com/project/somerepo.git"), "somerepo");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("http://anotherserver.mysubnet.com/project/Hello+G%C3%BCnter.git"), "Hello Günter");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("ssh://anotherserver.mysubnet.com/project/Hello+G%C3%BCnter.git"), "Hello Günter");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("git://anotherserver.mysubnet.com/project/Hello+G%C3%BCnter.git"), "Hello Günter");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName("git@anotherserver.mysubnet.com:project/Hello+G%C3%BCnter.git"), "Hello Günter");

        ClassicAssert.AreEqual(PathUtil.GetRepositoryName(""), "");
        ClassicAssert.AreEqual(PathUtil.GetRepositoryName(null), "");
        if (Path.DirectorySeparatorChar == '\\')
        {
            ClassicAssert.AreEqual(PathUtil.GetRepositoryName(@"C:\dev\my_repo"), "my_repo");
            ClassicAssert.AreEqual(PathUtil.GetRepositoryName(@"C:\dev\Hello+G%C3%BCnter"), "Hello+G%C3%BCnter");
            ClassicAssert.AreEqual(PathUtil.GetRepositoryName(@"\\networkshare\folder1\folder2\gitextensions"), "gitextensions");
        }
        else
        {
            ClassicAssert.AreEqual(PathUtil.GetRepositoryName(@"/dev/my_repo"), "my_repo");
            ClassicAssert.AreEqual(PathUtil.GetRepositoryName(@"/dev/Hello+G%C3%BCnter"), "Hello+G%C3%BCnter");
            ClassicAssert.AreEqual(PathUtil.GetRepositoryName(@"//networkshare/folder1/folder2/gitextensions"), "gitextensions");
        }
    }

    [Platform(Include = "Win")]
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase(" ", "")]
    [TestCase("c:", "")]
    [TestCase("C:\\", "C:\\")]
    [TestCase("a:\\folder\\filename.txt", "a:\\folder\\filename.txt")]
    [TestCase("a:\\folder\\..\\filename.txt", "a:\\filename.txt")]
    [TestCase("file:///C:/Test%20Project.exe", "C:\\Test Project.exe")]
    [TestCase("C:\\Progra~1\\", "C:\\Program Files\\")]
    [TestCase("C:\\Progra~1", "C:\\Program Files")]
    [TestCase("\\\\folder\\filename.txt", "\\\\folder\\filename.txt")]
    [TestCase("a:\\\\folder/filename.txt", "a:\\folder\\filename.txt")]
    [TestCase(@"c:\folder#\filename.txt", @"c:\folder#\filename.txt")]
    [TestCase(@"C:\WORK\..\WORK\.\GitExtensions\", @"C:\WORK\GitExtensions\")]
    [TestCase(@"\\my-pc\Work\.\GitExtensions\", @"\\my-pc\Work\GitExtensions\")]
    [TestCase(@"\\wsl$\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    [TestCase(@"\\Wsl$\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    [TestCase(@"\\w$\work\", "")]
    public void NormalizePath(string path, string expected)
    {
        PathUtil.NormalizePath(path).Should().Be(expected);
    }

    [Platform(Include = "Win")]
    [TestCase("", "")]
    [TestCase(" ", " ")]
    [TestCase("c:", "c:")]
    [TestCase("C:\\", "C:\\")]
    [TestCase(@"\\wsl$\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    [TestCase(@"\\Wsl.LoCALhosT\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    [TestCase(@"\\wsl.localhost\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    public void NormalizeWslPath(string path, string expected)
    {
        PathUtil.NormalizeWslPath(path).Should().Be(expected);
    }

    [TestCase(@"C:\WORK\GitExtensions\", @"C:\WORK\GitExtensions\")]
    [TestCase(@"\\my-pc\Work\GitExtensions\", @"\\my-pc\Work\GitExtensions\")]
    [TestCase(@"\\wsl$\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    [TestCase(@"\\wsl$\Ubuntu\home\jack\.\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    [TestCase(@"\\WSL$\Ubuntu\home\jack\.\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    public void Resolve(string path, string expected)
    {
        PathUtil.Resolve(path).Should().Be(expected);
    }

    [TestCase(@"C:\WORK\", @"GitExtensions\", @"C:\WORK\GitExtensions\")]
    [TestCase(@"C:\WORK\", @" file .txt ", @"C:\WORK\ file .txt ")]
    [TestCase(@"\\wsl$\", @"Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
    public void Resolve(string path, string relativePath, string expected)
    {
        PathUtil.Resolve(path, relativePath).Should().Be(expected);
    }

    [TestCase(@"\\w$\work\", typeof(UriFormatException))]
    [TestCase(@":$\work\", typeof(UriFormatException))]
    [TestCase(@"C:", typeof(UriFormatException))]
    [TestCase(@"\\wsl$", typeof(UriFormatException))]
    [TestCase(null, typeof(ArgumentException))]
    [TestCase("", typeof(ArgumentException))]
    [TestCase(" ", typeof(ArgumentException))]
    public void Resolve(string input, Type expectedException)
    {
        ClassicAssert.Throws(expectedException, () => PathUtil.Resolve(input));
    }

    [TestCase(@"\\wsl$\Ubuntu\work\..\GitExtensions\", @"\\wsl$\Ubuntu\GitExtensions\")]
    [TestCase(@"\\Wsl$\Ubuntu\work\..\GitExtensions\", @"\\wsl$\Ubuntu\GitExtensions\")]
    [TestCase(@"\\WSL$\Ubuntu\work\..\GitExtensions\", @"\\wsl$\Ubuntu\GitExtensions\")]
    public void ResolveWsl(string path, string expected)
    {
        PathUtil.ResolveWsl(path).Should().Be(expected);
    }

    [TestCase(@"\\w$\work\", typeof(ArgumentException))]
    [TestCase(@":$\work\", typeof(ArgumentException))]
    [TestCase(@"C:\work\", typeof(ArgumentException))]
    [TestCase(null, typeof(ArgumentException))]
    [TestCase("", typeof(ArgumentException))]
    [TestCase(" ", typeof(ArgumentException))]
    public void ResolveWsl(string input, Type expectedException)
    {
        ClassicAssert.Throws(expectedException, () => PathUtil.ResolveWsl(input));
    }

    [TestCase(@"C:\work\..\GitExtensions\", false)]
    [TestCase(@"\\Wsl$\Ubuntu\work\..\GitExtensions\", false)]
    [TestCase(@"\\wsl.localhost\Ubuntu\work\..\GitExtensions\", true)]
    [TestCase(@"\\wsl.localhost/Ubuntu\work\..\GitExtensions\", false)]
    public void IsWslLocalhostPath(string path, bool expected)
    {
        PathUtil.TestAccessor.IsWslLocalhostPrefixPath(path).Should().Be(expected);
    }

    [TestCase(@"\\Wsl$\Ubuntu\work\..\GitExtensions\", true, true)]
    [TestCase(@"\\wsl$\Ubuntu\work\..\GitExtensions\", true, true)]
    [TestCase(@"C:\work\..\GitExtensions\", false, false)]
    [TestCase(@"\\Wsl$/Ubuntu\work\..\GitExtensions\", false, false)]
    [TestCase(@"\\Wsl.localhost\GitExtensions\", true, false)]
    public void IsWslPath(string path, bool expected, bool expectedPrefix)
    {
        PathUtil.IsWslPath(path).Should().Be(expected);
        PathUtil.TestAccessor.IsWslPrefixPath(path).Should().Be(expectedPrefix);
    }

    [TestCase(@"\\Wsl$\Ubuntu\work\..\GitExtensions\", "Ubuntu")]
    [TestCase(@"\\wsl$\Ubuntu/work/../GitExtensions", "Ubuntu")]
    [TestCase(@"\\wsl$\Ubuntu-20.04\work\..\GitExtensions\", "Ubuntu-20.04")]
    [TestCase(@"C:\work\..\GitExtensions\", "")]
    public void GetWslDistro(string path, string expected)
    {
        PathUtil.GetWslDistro(path).Should().Be(expected);
    }

    [TestCase(@"\\wsl$/Ubuntu/work/../GitExtensions", "")]
    public void GetWslDistro_unexpected_usage(string path, string expected)
    {
        PathUtil.GetWslDistro(path).Should().Be(expected);
    }

    [TestCase(@"\\wsl$\Ubuntu\work\..\GitExtensions\", "", @"//wsl$/Ubuntu/work/../GitExtensions/")]
    [TestCase(@"C:\work\..\GitExtensions\", "", @"C:/work/../GitExtensions/")]
    [TestCase(@"work\..\GitExtensions\", "", @"work/../GitExtensions/")]
    public void GetPathForGitExecution_GetWindowsPath_default(string path, string wslDistro, string expected)
    {
        PathUtil.GetPathForGitExecution(path, wslDistro).Should().Be(expected);
        PathUtil.GetWindowsPath(expected, wslDistro).Should().Be(path);
    }

    [TestCase(@"\\Wsl$\Ubuntu\work\..\GitExtensions\", "Ubuntu", @"/work/../GitExtensions/")]
    [TestCase(@"\\wsl$\Ubuntu\work/../GitExtensions", "Ubuntu", @"/work/../GitExtensions")]
    [TestCase(@"\\wsl$\Ubuntu-20.04\work\..\GitExtensions\", "Ubuntu-20.04", @"/work/../GitExtensions/")]
    [TestCase(@"C:\work\..\GitExtensions\", "Ubuntu", @"/mnt/c/work/../GitExtensions/")]
    [TestCase(@"work\..\GitExtensions\", "Ubuntu", @"work/../GitExtensions/")]
    public void GetPathForGitExecution_wsl(string path, string wslDistro, string expected)
    {
        PathUtil.GetPathForGitExecution(path, wslDistro).Should().Be(expected);
    }

    [TestCase(@"\\wsl$/Ubuntu/work/../GitExtensions", "Ubuntu", @"//wsl$/Ubuntu/work/../GitExtensions")]
    [TestCase(@"\\wsl$\Ubuntu-20.04\work\..\GitExtensions\", "Ubuntu", @"//wsl$/Ubuntu-20.04/work/../GitExtensions/")]
    public void GetPathForGitExecution_unexpected_usage(string path, string wslDistro, string expected)
    {
        PathUtil.GetPathForGitExecution(path, wslDistro).Should().Be(expected);
    }

    // Mostly opposite to GetRepoPath_wsl
    [TestCase(@"\\wsl$\Ubuntu\work\..\GitExtensions\", "Ubuntu", @"/work/../GitExtensions/")]
    [TestCase(@"\\wsl$\Ubuntu\work\..\GitExtensions", "Ubuntu", @"/work/../GitExtensions")]
    [TestCase(@"\\wsl$\Ubuntu-20.04\work\..\GitExtensions\", "Ubuntu-20.04", @"/work/../GitExtensions/")]
    [TestCase(@"C:\work\..\GitExtensions\", "Ubuntu", @"/mnt/c/work/../GitExtensions/")]
    [TestCase(@"\\wsl$\Ubuntu\work\..\GitExtensions\", "Ubuntu", @"work/../GitExtensions/")]
    public void GetWindowsPath_wsl(string expected, string wslDistro, string path)
    {
        PathUtil.GetWindowsPath(path, wslDistro).Should().Be(expected);
    }

    [Platform(Include = "Win")]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a:\\folder\\filename.txt")]
    [TestCase("a:\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\filename.txt")]
    [TestCase("file:////folder/filename.txt")]
    [TestCaseSource(nameof(GetInvalidPaths))]
    public void TryFindFullPath_not_throw_if_file_not_exist(string fileName)
    {
        PathUtil.TryFindFullPath(fileName, out _).Should().BeFalse();
    }

    private static IEnumerable<string> GetInvalidPaths()
    {
        if (Path.DirectorySeparatorChar == '\\')
        {
            yield return @"c::\word";
            yield return "\"c:\\word\t\\\"";
            yield return @".c:\Programs\";
            yield return "c:\\Programs\\Get\"\\";
        }
        else
        {
            // I am not able to figure out any invalid (giving exception) path under mono
        }
    }

    [Test]
    public void GetDisplayPath()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        ClassicAssert.AreEqual(@"~\SomePath", PathUtil.GetDisplayPath(Path.Combine(home, "SomePath")));
        ClassicAssert.AreEqual("c:\\SomePath", PathUtil.GetDisplayPath("c:\\SomePath"));
    }

    [TestCase("/foo/bar", new[] { "\\foo\\", "\\" })]
    [TestCase("/foo/bar/", new[] { "\\foo\\", "\\" })]
    [TestCase("/foo", new[] { "\\" })]
    [TestCase("/foo/", new[] { "\\" })]
    [TestCase("/", new string[0])]
    [TestCase("C:\\foo\\bar", new[] { "C:\\foo\\", "C:\\" })]
    [TestCase("C:\\", new string[0])]
    public void FindAncestors(string path, string[] expected)
    {
        ClassicAssert.AreEqual(expected, PathUtil.FindAncestors(path).ToArray());
    }

    private static IEnumerable<TestCaseData> InvalidFolders
    {
        get
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData("");
            yield return new TestCaseData(@"A:\does\not\exist");
        }
    }

    [Test, TestCaseSource(nameof(InvalidFolders))]
    public void DeleteWithExtremePrejudice_should_return_true_for_empty_input_or_absent_folders(string path)
    {
        path.TryDeleteDirectory(out string errorMessage).Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void DeleteWithExtremePrejudice_should_return_true_if_delete_successful()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempPath);
        Directory.Exists(tempPath).Should().BeTrue();

        tempPath.TryDeleteDirectory(out string errorMessage).Should().BeTrue();
        errorMessage.Should().BeNull();
        Directory.Exists(tempPath).Should().BeFalse();
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase(" ", false)]
    [TestCase("blah", false)]
    [TestCase("https://github.com/gitextensions/gitextensions", true)]
    [TestCase("https://github.com/gitextensions/gitextensions.git", true)]
    [TestCase("github.com/gitextensions/gitextensions.git", true)]
    [TestCase("HTTPS://MYPRIVATEGITHUB.COM:8080/LOUDREPO.GIT", true)]
    [TestCase("git://myurl/myrepo.git", true)]
    [TestCase("git@github.com:gitextensions/gitextensions.git", true)]
    [TestCase("git@github.com:gitextensions/gitextensions.git/", true)]
    [TestCase("https://github.com/gitextensions/gitextensions/issues/12210#issuecomment-2672903979", false)]
    [TestCase("https://github.com/gitextensions/gitextensions?query=param", false)]
    [TestCase("github.com/gitextensions", false)]
    [TestCase("github.com/gitextensions/gitextensions/pull/9018", false)]
    [TestCase("http://", false)]
    [TestCase("HTTPS://www", true)]
    [TestCase("git://", true)]
    [TestCase("file:", false)]
    [TestCase("SSH:", true)]
    [TestCase("SSH", false)]
    [TestCase("https://myhost:12368/", true)]
    public void CanBeGitURL(string url, bool expected)
    {
        ClassicAssert.AreEqual(expected, PathUtil.CanBeGitURL(url));
    }
}
