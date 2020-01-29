using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class PlinkTests
    {
        [Test]
        public void TestGetPlinkCompatibleUrl_Incompatible()
        {
            // Test urls that are incompatible and need to be changed
            string inUrl, expectUrl, outUrl;

            // ssh urls can cause problems
            inUrl = "ssh://user@example.com/path/to/project.git";
            expectUrl = "\"user@example.com:path/to/project.git\"";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);

            inUrl = "ssh://user@example.com:29418/path/to/project.git";
            expectUrl = "-P 29418 \"user@example.com:path/to/project.git\"";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);

            // ssh, no user
            inUrl = "ssh://example.com/path/to/project.git";
            expectUrl = "\"example.com:path/to/project.git\"";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);
        }

        [Test]
        public void TestGetPlinkCompatibleUrl_Compatible()
        {
            // Test urls that are already compatible, these shouldn't be changed
            string inUrl, outUrl;

            // ssh in compatible form
            inUrl = "git@github.com:gitextensions/gitextensions.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // ssh in compatible form, no user
            inUrl = "example.org:some/path/to/repo.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }

        [Test]
        public void TestGetPlinkCompatibleUrl_NoPlink()
        {
            // Test urls that are no valid uris, these should be ignored
            string inUrl, outUrl;

            // git protocol does not have authentication
            inUrl = "git://server/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // git protocol, different port
            inUrl = "git://server:123/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // we don't need plink for http
            inUrl = "http://user@server/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // http, different port
            inUrl = "http://user@server:123/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // http, no user
            inUrl = "http://server/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // we don't need plink for https
            inUrl = "https://user@server/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // https, different port
            inUrl = "https://user@server:123/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // https, no user
            inUrl = "https://server/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }

        [Test]
        public void TestGetPlinkCompatibleUrl_Invalid()
        {
            // Test urls that are no valid uris, these should be ignored
            string inUrl, outUrl;

            inUrl = "foo://server/path/to/project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            inUrl = @"ssh:\\server\path\to\project.git";
            outUrl = Plink.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }
    }
}