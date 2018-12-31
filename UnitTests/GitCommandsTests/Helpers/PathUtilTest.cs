using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("    ", false)]
        [TestCase("http://", true)]
        [TestCase("HTTPS://www", true)]
        [TestCase("git://", true)]
        [TestCase("SSH", true)]
        public void IsUrl(string path, bool expected)
        {
            PathUtil.IsUrl(path).Should().Be(expected);
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
        public void GetDirectoryNameTest()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual(PathUtil.GetDirectoryName("\\\\my-pc\\Work\\GitExtensions\\"), "\\\\my-pc\\Work\\GitExtensions");
                Assert.AreEqual(PathUtil.GetDirectoryName("C:\\Work\\GitExtensions\\"), "C:\\Work\\GitExtensions");
                Assert.AreEqual(PathUtil.GetDirectoryName("C:\\Work\\GitExtensions"), "C:\\Work");
                Assert.AreEqual(PathUtil.GetDirectoryName("C:\\Work\\"), "C:\\Work");
                Assert.AreEqual(PathUtil.GetDirectoryName("C:\\Work"), "");
                Assert.AreEqual(PathUtil.GetDirectoryName("C:\\"), "");
                Assert.AreEqual(PathUtil.GetDirectoryName("C:"), "");
                Assert.AreEqual(PathUtil.GetDirectoryName(""), "");
            }

            Assert.AreEqual(PathUtil.GetDirectoryName("//my-pc/Work/GitExtensions/"), "//my-pc/Work/GitExtensions");
            Assert.AreEqual(PathUtil.GetDirectoryName("/Work/GitExtensions/"), "/Work/GitExtensions");
            Assert.AreEqual(PathUtil.GetDirectoryName("/Work/GitExtensions"), "/Work");
            Assert.AreEqual(PathUtil.GetDirectoryName("/Work/"), "/Work");
            Assert.AreEqual(PathUtil.GetDirectoryName("/"), "");
            Assert.AreEqual("/", PathUtil.GetDirectoryName("/Work"), "/Work");
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

        [Test]
        public void GetFileExtension()
        {
            Assert.AreEqual("txt", PathUtil.GetFileExtension("foo.txt"));
            Assert.AreEqual("txt", PathUtil.GetFileExtension("foo.txt.txt"));
            Assert.AreEqual("txt", PathUtil.GetFileExtension(".txt"));
            Assert.AreEqual("", PathUtil.GetFileExtension("foo."));
            Assert.AreEqual("", PathUtil.GetFileExtension("."));
            Assert.AreEqual("", PathUtil.GetFileExtension(".."));
            Assert.AreEqual("", PathUtil.GetFileExtension("..."));
            Assert.Null(PathUtil.GetFileExtension("foo"));
            Assert.Null(PathUtil.GetFileExtension(""));
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
    }
}
