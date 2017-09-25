using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.AreEqual("C:/Work/GitExtensions/".ToPosixPath(), "C:/Work/GitExtensions/");
            Assert.AreEqual("C:\\Work\\GitExtensions\\".ToPosixPath(), "C:/Work/GitExtensions/");
        }

        [Test]
        public void ToNativePathTest()
        {
            Assert.AreEqual("C:\\Work\\GitExtensions\\".ToNativePath(), "C:\\Work\\GitExtensions\\");
            Assert.AreEqual("C:/Work/GitExtensions/".ToNativePath(), "C:\\Work\\GitExtensions\\");
            Assert.AreEqual("\\\\my-pc\\Work\\GitExtensions\\".ToNativePath(), "\\\\my-pc\\Work\\GitExtensions\\");
        }

        [Test]
        public void EnsureTrailingPathSeparatorTest()
        {
            Assert.AreEqual("".EnsureTrailingPathSeparator(), "");
            Assert.AreEqual("C".EnsureTrailingPathSeparator(), "C\\");
            Assert.AreEqual("C:".EnsureTrailingPathSeparator(), "C:\\");
            Assert.AreEqual("C:\\".EnsureTrailingPathSeparator(), "C:\\");
            Assert.AreEqual("C:\\Work\\GitExtensions".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
            Assert.AreEqual("C:\\Work\\GitExtensions\\".EnsureTrailingPathSeparator(), "C:\\Work\\GitExtensions\\");
            Assert.AreEqual("C:/Work/GitExtensions/".EnsureTrailingPathSeparator(), "C:/Work/GitExtensions/");
        }

        [Test]
        public void IsLocalFileTest()
        {
            Assert.AreEqual(PathUtil.IsLocalFile("\\\\my-pc\\Work\\GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions\\"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("ssh://domain\\user@serverip/cache/git/something/something.git"), false);
        }

        [Test]
        public void GetFileNameTest()
        {
            Assert.AreEqual(PathUtil.GetFileName("\\\\my-pc\\Work\\GitExtensions"), "GitExtensions");
            Assert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions"), "GitExtensions");
            Assert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions\\"), "");
        }

        [Test]
        public void GetDirectoryNameTest()
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

        [Test]
        public void EqualTest()
        {
            Assert.AreEqual(PathUtil.Equal("C:\\Work\\GitExtensions\\", "C:/Work/GitExtensions/"), true);
            Assert.AreEqual(PathUtil.Equal("\\\\my-pc\\Work\\GitExtensions\\", "//my-pc/Work/GitExtensions/"), true);
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
            Assert.AreEqual(PathUtil.GetRepositoryName(@"C:\dev\my_repo"), "my_repo");
            Assert.AreEqual(PathUtil.GetRepositoryName(@"\\networkshare\folder1\folder2\gitextensions"), "gitextensions");

            Assert.AreEqual(PathUtil.GetRepositoryName(""), "");
            Assert.AreEqual(PathUtil.GetRepositoryName(null), "");
        }

        [Test]
        public void IsValidPathTest()
        {
            Assert.IsTrue(PathUtil.IsValidPath("\\\\my-pc\\Work\\GitExtensions\\"), "\\\\my-pc\\Work\\GitExtensions");
            Assert.IsTrue(PathUtil.IsValidPath("C:\\Work\\GitExtensions\\"), "C:\\Work\\GitExtensions");
            Assert.IsTrue(PathUtil.IsValidPath("C:\\Work\\"), "C:\\Work");
            Assert.IsTrue(PathUtil.IsValidPath("C:\\"), "");
            Assert.IsTrue(PathUtil.IsValidPath("C:"), "");
            Assert.IsFalse(PathUtil.IsValidPath(""), "");
            Assert.IsFalse(PathUtil.IsValidPath("\"C:\\Work\\GitExtensions\\"), "C:\\Work\\GitExtensions\"");
        }

        [Test]
        public void GetEnvironmentPathsTest()
        {
            string pathVariable = string.Join(";", GetValidPaths().Concat(GetInvalidPaths()));
            var paths = PathUtil.GetEnvironmentPaths(pathVariable);
            var validEnvPaths = PathUtil.GetValidPaths(paths);
            CollectionAssert.AreEqual(validEnvPaths, GetValidPaths());
        }

        [Test]
        public void GetEnvironmentPathsQuotedTest()
        {
            var paths = GetValidPaths().Concat(GetInvalidPaths());
            var quotedPaths = paths.Select(path => path.Quote(" ")).Select(path => path.Quote());
            string pathVariable = string.Join(";", quotedPaths);
            var envPaths = PathUtil.GetEnvironmentPaths(pathVariable);
            var validEnvPaths = PathUtil.GetValidPaths(envPaths);
            CollectionAssert.AreEqual(validEnvPaths, GetValidPaths());
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
            yield return @"c:\work";
            yield return @"c:\work\";
            yield return @"c:\Program Files(86)\";
            yield return @"c:\Program Files(86)\Git";
        }

        private static IEnumerable<string> GetInvalidPaths()
        {
            yield return @"c::\word";
            yield return "\"c:\\word\t\\\"";
            yield return @".c:\Programs\";
            yield return "c:\\Programs\\Get\"\\";
        }
    }
}
