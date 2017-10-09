using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using NUnit.Framework;
using System.IO;
using GitCommands.Utils;

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
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual("".EnsureTrailingPathSeparator(), "");
                Assert.AreEqual("C".EnsureTrailingPathSeparator(), "C\\");
                Assert.AreEqual("C:".EnsureTrailingPathSeparator(), "C:\\");
                Assert.AreEqual("C:\\".EnsureTrailingPathSeparator(), "C:\\");
                Assert.AreEqual("C:\\Work\\GitExtensions".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("C:\\Work\\GitExtensions\\".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
                Assert.AreEqual("C:/Work/GitExtensions/".EnsureTrailingPathSeparator(), "C:/Work/GitExtensions/");
            }
            else
            {
                Assert.AreEqual("".EnsureTrailingPathSeparator(), "");
                Assert.AreEqual("/".EnsureTrailingPathSeparator(), "/");
                Assert.AreEqual("/Work/GitExtensions".EnsureTrailingPathSeparator(), "/Work/GitExtensions/");
                Assert.AreEqual("/Work/GitExtensions/".EnsureTrailingPathSeparator(), "/Work/GitExtensions/");
                Assert.AreEqual("/Work/GitExtensions\\".EnsureTrailingPathSeparator(), "/Work/GitExtensions\\/");
            }
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
        public void EqualTest()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.AreEqual(PathUtil.Equal("C:\\Work\\GitExtensions\\", "C:/Work/GitExtensions/"), true);
                Assert.AreEqual(PathUtil.Equal("\\\\my-pc\\Work\\GitExtensions\\", "//my-pc/Work/GitExtensions/"), true);
            }
            else
            {
                Assert.AreEqual(PathUtil.Equal("/Work/GitExtensions/", "/Work/GitExtensions/"), true);
                Assert.AreEqual(PathUtil.Equal("//my-pc/Work/GitExtensions/", "//my-pc/Work/GitExtensions/"), true);
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

        [Test]
        public void IsValidPathTest()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                Assert.IsTrue(PathUtil.IsValidPath("\\\\my-pc\\Work\\GitExtensions\\"), "\\\\my-pc\\Work\\GitExtensions");
                Assert.IsTrue(PathUtil.IsValidPath("C:\\Work\\GitExtensions\\"), "C:\\Work\\GitExtensions");
                Assert.IsTrue(PathUtil.IsValidPath("C:\\Work\\"), "C:\\Work");
                Assert.IsTrue(PathUtil.IsValidPath("C:\\"), "");
                Assert.IsTrue(PathUtil.IsValidPath("C:"), "");
                Assert.IsFalse(PathUtil.IsValidPath(""), "");
                Assert.IsFalse(PathUtil.IsValidPath("\"C:\\Work\\GitExtensions\\"), "C:\\Work\\GitExtensions\"");
            }
            else
            {
                string path = "//my-pc/Work/GitExtensions/";
                Assert.IsTrue(PathUtil.IsValidPath(path), path);
                path = "/my-pc/Work/GitExtensions/";
                Assert.IsTrue(PathUtil.IsValidPath(path), path);
                path = "/my-pc/Work/GitExtensions";
                Assert.IsTrue(PathUtil.IsValidPath(path), path);
            }
        }

        [Test]
        public void GetEnvironmentPathsTest()
        {
            string pathVariable = string.Join(EnvUtils.EnvVariableSeparator.ToString(), 
                GetValidPaths().Concat(GetInvalidPaths()));
            var paths = PathUtil.GetEnvironmentPaths(pathVariable);
            var validEnvPaths = PathUtil.GetValidPaths(paths);
            CollectionAssert.AreEqual(GetValidPaths().ToArray(), validEnvPaths.ToArray());
        }

        [Test]
        public void GetEnvironmentPathsQuotedTest()
        {
            var paths = GetValidPaths().Concat(GetInvalidPaths());
            var quotedPaths = paths.Select(path => path.Quote(" ")).Select(path => path.Quote());
            string pathVariable = string.Join(EnvUtils.EnvVariableSeparator.ToString(), quotedPaths);
            var envPaths = PathUtil.GetEnvironmentPaths(pathVariable);
            var validEnvPaths = PathUtil.GetValidPaths(envPaths);
            CollectionAssert.AreEqual(GetValidPaths().ToArray(), validEnvPaths.ToArray());
        }

        [Test]
        public void ExistingPathsTest()
        {
            Assert.IsTrue(PathUtil.PathExists(GetType().Assembly.Location));
        }

        [Test]
        public void NonExistingPathsTest()
        {
            GetInvalidPaths().ForEach((path) =>
            {
                Assert.IsFalse(PathUtil.PathExists(path));
            });
            Assert.IsFalse(PathUtil.PathExists("c:\\94fc5ae63a6c5ed7c110219ade20374ea4d237b9.xyz"));
        }

        private static IEnumerable<string> GetValidPaths()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                yield return @"c:\work";
                yield return @"c:\work\";
                yield return @"c:\Program Files(86)\";
                yield return @"c:\Program Files(86)\Git";
            }
            else
            {
                yield return "/etc/init.d/xvfb";
                yield return "/var";
                yield return "/";
            }
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
                //I am not able to figure out any invalid (giving exception) path under mono
            }
        }
    }
}
