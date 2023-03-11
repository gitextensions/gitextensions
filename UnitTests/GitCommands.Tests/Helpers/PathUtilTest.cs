using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Helpers
{
    [TestFixture]
    public class PathUtilTest
    {
        [Test]
        public void ToPosixPathTest()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual("C:/Work/GitExtensions/".ToPosixPath(), "C:/Work/GitExtensions/");
                Assert.AreEqual("C:\\Work\\GitExtensions\\".ToPosixPath(), "C:/Work/GitExtensions/");
            }
            else
            {
                Assert.AreEqual("C:/Work/GitExtensions/".ToPosixPath(), "C:/Work/GitExtensions/");
                Assert.AreEqual("C:\\Work\\GitExtensions\\".ToPosixPath(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("/var/tmp/".ToPosixPath(), "/var/tmp/");
            }
        }

        [Test]
        public void ToNativePathTest()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual("C:\\Work\\GitExtensions\\".ToNativePath(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("C:/Work/GitExtensions/".ToNativePath(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("\\\\my-pc\\Work\\GitExtensions\\".ToNativePath(), "\\\\my-pc\\Work\\GitExtensions\\");
            }
            else
            {
                Assert.AreEqual("C:\\Work\\GitExtensions\\".ToNativePath(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("/Work/GitExtensions/".ToNativePath(), "/Work/GitExtensions/");
                Assert.AreEqual("//server/share/".ToNativePath(), "//server/share/");
            }
        }

        [Test]
        public void ToWslPathTest()
        {
            Assert.AreEqual(PathUtil.ToWslPath(null), null);
            Assert.AreEqual(@"C:/Work/GitExtensions/".ToWslPath(), "/mnt/c/Work/GitExtensions/");
            Assert.AreEqual(@"C:\Work\GitExtensions\".ToWslPath(), "/mnt/c/Work/GitExtensions/");
            Assert.AreEqual(@"/var/tmp/".ToWslPath(), "/var/tmp/");
        }

        [Test]
        public void ToCygwinPathTest()
        {
            Assert.AreEqual(PathUtil.ToCygwinPath(null), null);
            Assert.AreEqual(@"C:/Work/GitExtensions/".ToCygwinPath(), "/cygdrive/c/Work/GitExtensions/");
            Assert.AreEqual(@"C:\Work\GitExtensions\".ToCygwinPath(), "/cygdrive/c/Work/GitExtensions/");
            Assert.AreEqual(@"/var/tmp/".ToCygwinPath(), "/var/tmp/");
        }

        [Test]
        public void ToMountPathTest()
        {
            const string prefix = "PrEfiX";
            Assert.AreEqual(PathUtil.ToMountPath(null, prefix), null);
            Assert.AreEqual(@"".ToMountPath(prefix), "");
            Assert.AreEqual(@"C".ToMountPath(prefix), "C");
            Assert.AreEqual(@".:".ToMountPath(prefix), $".:");
            Assert.AreEqual(@"C:".ToMountPath(prefix), $"{prefix}c");
            Assert.AreEqual(@"C:_".ToMountPath(prefix), $"{prefix}c_");
            Assert.AreEqual(@"C:\".ToMountPath(prefix), $"{prefix}c/");
            Assert.AreEqual(@"C:/".ToMountPath(prefix), $"{prefix}c/");
            Assert.AreEqual(@"C:\folder".ToMountPath(prefix), $"{prefix}c/folder");
            Assert.AreEqual(@"C:/Work/GitExtensions/".ToMountPath(prefix), $"{prefix}c/Work/GitExtensions/");
            Assert.AreEqual(@"C:\Work\GitExtensions\".ToMountPath(prefix), $"{prefix}c/Work/GitExtensions/");
            Assert.AreEqual(@"/var/tmp/".ToMountPath(prefix), "/var/tmp/");
        }

        [Test]
        public void EnsureTrailingPathSeparatorTest()
        {
            Assert.IsNull(((string)null).EnsureTrailingPathSeparator());
            Assert.AreEqual("".EnsureTrailingPathSeparator(), "");

            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual("C".EnsureTrailingPathSeparator(), "C\\");
                Assert.AreEqual("C:".EnsureTrailingPathSeparator(), "C:\\");
                Assert.AreEqual("C:\\".EnsureTrailingPathSeparator(), "C:\\");
                Assert.AreEqual("C:\\Work\\GitExtensions".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("C:\\Work\\GitExtensions\\".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("C:/Work/GitExtensions/".EnsureTrailingPathSeparator(), "C:/Work/GitExtensions/");
                Assert.AreEqual("\\".EnsureTrailingPathSeparator(), "\\");
                Assert.AreEqual("/".EnsureTrailingPathSeparator(), "/");
            }
            else
            {
                Assert.AreEqual("/".EnsureTrailingPathSeparator(), "/");
                Assert.AreEqual("/Work/GitExtensions".EnsureTrailingPathSeparator(), "/Work/GitExtensions/");
                Assert.AreEqual("/Work/GitExtensions/".EnsureTrailingPathSeparator(), "/Work/GitExtensions/");
                Assert.AreEqual("/Work/GitExtensions\\".EnsureTrailingPathSeparator(), "/Work/GitExtensions\\/");
            }
        }

        [Test]
        public void RemoveTrailingPathSeparatorTest()
        {
            Assert.IsNull(((string)null).RemoveTrailingPathSeparator());
            Assert.AreEqual("".RemoveTrailingPathSeparator(), "");

            var s = Path.DirectorySeparatorChar;

            Assert.AreEqual($"C:{s}".RemoveTrailingPathSeparator(), "C:");
            Assert.AreEqual("foo".RemoveTrailingPathSeparator(), "foo");
            Assert.AreEqual($"foo{s}".RemoveTrailingPathSeparator(), "foo");
            Assert.AreEqual($"foo{s}bar".RemoveTrailingPathSeparator(), $"foo{s}bar");
            Assert.AreEqual($"foo{s}bar{s}".RemoveTrailingPathSeparator(), $"foo{s}bar");

            Assert.AreEqual("foo/".RemoveTrailingPathSeparator(), "foo");
            Assert.AreEqual("foo/bar".RemoveTrailingPathSeparator(), "foo/bar");
            Assert.AreEqual("foo/bar/".RemoveTrailingPathSeparator(), "foo/bar");
        }

        [Test]
        public void IsLocalFileTest()
        {
            Assert.AreEqual(PathUtil.IsLocalFile("\\\\my-pc\\Work\\GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("//my-pc/Work/GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions\\"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("/Work/GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("/Work/GitExtensions/"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("ssh://domain\\user@serverip/cache/git/something/something.git"), false);
        }

        [Test]
        public void GetFileNameTest()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual(PathUtil.GetFileName("\\\\my-pc\\Work\\GitExtensions"), "GitExtensions");
                Assert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions"), "GitExtensions");
                Assert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions\\"), "");
            }
            else
            {
                Assert.AreEqual(PathUtil.GetFileName("//my-pc/Work/GitExtensions"), "GitExtensions");
                Assert.AreEqual(PathUtil.GetFileName("/Work/GitExtensions"), "GitExtensions");
                Assert.AreEqual(PathUtil.GetFileName("/Work/GitExtensions/"), "");
            }
        }

        [Test]
        public void GetRepositoryNameTest()
        {
            Assert.AreEqual(PathUtil.GetRepositoryName("https://github.com/gitextensions/gitextensions.git"), "gitextensions");
            Assert.AreEqual(PathUtil.GetRepositoryName("https://github.com/jeffqc/gitextensions"), "gitextensions");
            Assert.AreEqual(PathUtil.GetRepositoryName("git://mygitserver/git/test.git"), "test");
            Assert.AreEqual(PathUtil.GetRepositoryName("ssh://mygitserver/git/test.git"), "test");
            Assert.AreEqual(PathUtil.GetRepositoryName("ssh://john.doe@mygitserver/git/test.git"), "test");
            Assert.AreEqual(PathUtil.GetRepositoryName("ssh://john-abraham.doe@mygitserver/git/MyAwesomeRepo.git"), "MyAwesomeRepo");
            Assert.AreEqual(PathUtil.GetRepositoryName("git@anotherserver.mysubnet.com:project/somerepo.git"), "somerepo");
            Assert.AreEqual(PathUtil.GetRepositoryName("http://anotherserver.mysubnet.com/project/somerepo.git"), "somerepo");

            Assert.AreEqual(PathUtil.GetRepositoryName(""), "");
            Assert.AreEqual(PathUtil.GetRepositoryName(null), "");
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual(PathUtil.GetRepositoryName(@"C:\dev\my_repo"), "my_repo");
                Assert.AreEqual(PathUtil.GetRepositoryName(@"\\networkshare\folder1\folder2\gitextensions"), "gitextensions");
            }
            else
            {
                Assert.AreEqual(PathUtil.GetRepositoryName(@"/dev/my_repo"), "my_repo");
                Assert.AreEqual(PathUtil.GetRepositoryName(@"//networkshare/folder1/folder2/gitextensions"), "gitextensions");
            }
        }

        [Platform(Include = "Win")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("c:")]
        public void NormalizePath(string path)
        {
            PathUtil.NormalizePath(path).Should().BeEmpty();
        }

        [Platform(Include = "Win")]
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
            Assert.Throws(expectedException, () => PathUtil.Resolve(input));
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
            Assert.Throws(expectedException, () => PathUtil.ResolveWsl(input));
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
        public void GetGitExecPath_GetWindowsPath_default(string path, string wslDistro, string expected)
        {
            PathUtil.GetGitExecPath(path, wslDistro).Should().Be(expected);
            PathUtil.GetWindowsPath(expected, wslDistro).Should().Be(path);
        }

        [TestCase(@"\\Wsl$\Ubuntu\work\..\GitExtensions\", "Ubuntu", @"/work/../GitExtensions/")]
        [TestCase(@"\\wsl$\Ubuntu\work/../GitExtensions", "Ubuntu", @"/work/../GitExtensions")]
        [TestCase(@"\\wsl$\Ubuntu-20.04\work\..\GitExtensions\", "Ubuntu-20.04", @"/work/../GitExtensions/")]
        [TestCase(@"C:\work\..\GitExtensions\", "Ubuntu", @"/mnt/c/work/../GitExtensions/")]
        [TestCase(@"work\..\GitExtensions\", "Ubuntu", @"work/../GitExtensions/")]
        public void GetGitExecPath_wsl(string path, string wslDistro, string expected)
        {
            PathUtil.GetGitExecPath(path, wslDistro).Should().Be(expected);
        }

        [TestCase(@"\\wsl$/Ubuntu/work/../GitExtensions", "Ubuntu", @"//wsl$/Ubuntu/work/../GitExtensions")]
        [TestCase(@"\\wsl$\Ubuntu-20.04\work\..\GitExtensions\", "Ubuntu", @"//wsl$/Ubuntu-20.04/work/../GitExtensions/")]
        public void GetGitExecPath_unexpected_usage(string path, string wslDistro, string expected)
        {
            PathUtil.GetGitExecPath(path, wslDistro).Should().Be(expected);
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
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            Assert.AreEqual(@"~\SomePath", PathUtil.GetDisplayPath(Path.Combine(home, "SomePath")));
            Assert.AreEqual("c:\\SomePath", PathUtil.GetDisplayPath("c:\\SomePath"));
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
            Assert.AreEqual(expected, PathUtil.FindAncestors(path).ToArray());
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
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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
            Assert.AreEqual(expected, PathUtil.CanBeGitURL(url));
        }
    }
}
