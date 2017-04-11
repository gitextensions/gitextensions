using NUnit.Framework;
using ResourceManager;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    class LinkFactoryFixture
    {
        [Test]
        public void ParseGoToBranchLink()
        {
            string expected = "gitext://gotobranch/master";
            string actual = LinkFactory.ParseLink("master#gitext://gotobranch/master");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseGoToBranchLinkWithHash()
        {
            string expected = "gitext://gotobranch/PR#23";
            string actual = LinkFactory.ParseLink("PR#23#gitext://gotobranch/PR#23");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseHtttpLinkWithHash()
        {
            string expected = "https://github.com/gitextensions/gitextensions/pull/3471#end";
            string actual = LinkFactory.ParseLink("#3471#https://github.com/gitextensions/gitextensions/pull/3471#end");
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
