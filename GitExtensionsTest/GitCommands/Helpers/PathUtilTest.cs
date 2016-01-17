using System;
using System.IO;
using GitCommands;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest
{
    [TestClass]
    public class PathUtilTest
    {
        [TestMethod]
        public void ToPosixPathTest()
        {
            Assert.AreEqual("C:/Work/GitExtensions/".ToPosixPath(), "C:/Work/GitExtensions/");
            Assert.AreEqual("C:\\Work\\GitExtensions\\".ToPosixPath(), "C:/Work/GitExtensions/");
        }

        [TestMethod]
        public void ToNativePathTest()
        {
            Assert.AreEqual("C:\\Work\\GitExtensions\\".ToNativePath(), "C:\\Work\\GitExtensions\\");
            Assert.AreEqual("C:/Work/GitExtensions/".ToNativePath(), "C:\\Work\\GitExtensions\\");
            Assert.AreEqual("\\\\my-pc\\Work\\GitExtensions\\".ToNativePath(), "\\\\my-pc\\Work\\GitExtensions\\");
        }

        [TestMethod]
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

        [TestMethod]
        public void IsLocalFileTest()
        {
            Assert.AreEqual(PathUtil.IsLocalFile("\\\\my-pc\\Work\\GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("C:\\Work\\GitExtensions\\"), true);
            Assert.AreEqual(PathUtil.IsLocalFile("ssh://domain\\user@serverip/cache/git/something/something.git"), false);
        }

        [TestMethod]
        public void GetFileNameTest()
        {
            Assert.AreEqual(PathUtil.GetFileName("\\\\my-pc\\Work\\GitExtensions"), "GitExtensions");
            Assert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions"), "GitExtensions");
            Assert.AreEqual(PathUtil.GetFileName("C:\\Work\\GitExtensions\\"), "");
        }

        [TestMethod]
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

        [TestMethod]
        public void EqualTest()
        {
            Assert.AreEqual(PathUtil.Equal("C:\\Work\\GitExtensions\\", "C:/Work/GitExtensions/"), true);
            Assert.AreEqual(PathUtil.Equal("\\\\my-pc\\Work\\GitExtensions\\", "//my-pc/Work/GitExtensions/"), true);
        }

        [TestMethod]
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
    }
}
