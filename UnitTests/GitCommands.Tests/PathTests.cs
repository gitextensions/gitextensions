using System;
using GitCommands;
using NUnit.Framework;

namespace PathTests
{
    [TestFixture]
    public class PathTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(@"C:\WORK\..\WORK\.\GitExtensions\", @"C:\WORK\GitExtensions\")]
        [TestCase(@"\\my-pc\Work\.\GitExtensions\", @"\\my-pc\Work\GitExtensions\")]
        [TestCase(@"\\wsl$\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
        [TestCase(@"\\w$\work\", "")]
        public void NormalizePath(string input, string expected)
        {
            Assert.AreEqual(expected, PathUtil.NormalizePath(input));
        }

        [TestCase(@"C:\WORK\GitExtensions\", @"C:\WORK\GitExtensions\")]
        [TestCase(@"\\my-pc\Work\GitExtensions\", @"\\my-pc\Work\GitExtensions\")]
        [TestCase(@"\\wsl$\Ubuntu\home\jack\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
        [TestCase(@"\\wsl$\Ubuntu\home\jack\.\work\", @"\\wsl$\Ubuntu\home\jack\work\")]
        public void SolveUriLocalPath(string input, string expected)
        {
            Assert.AreEqual(expected, PathUtil.Resolve(input));
        }

        [TestCase(@"\\w$\work\", typeof(UriFormatException))]
        [TestCase(@":$\work\", typeof(UriFormatException))]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentNullException))]
        [TestCase(" ", typeof(ArgumentNullException))]
        public void SolveUriLocalPathExceptions(string input, Type expectedException)
        {
            Assert.Throws(expectedException, () => PathUtil.Resolve(input));
        }
    }
}
