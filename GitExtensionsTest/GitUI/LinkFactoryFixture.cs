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
        public void ParseMailTo()
        {
            string expected = "mailto:jbialobr@o2.pl";
            string actual = LinkFactory.ParseLink("Janusz Białobrzewski <jbialobr@o2.pl>#mailto:jbialobr@o2.pl");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseHttpLinkWithHash()
        {
            string expected = "https://github.com/gitextensions/gitextensions/pull/3471#end";
            string actual = LinkFactory.ParseLink("#3471#https://github.com/gitextensions/gitextensions/pull/3471#end");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseRawHttpLinkWithHash()
        {
            string expected = "https://github.com/gitextensions/gitextensions/pull/3471#end";
            string actual = LinkFactory.ParseLink("https://github.com/gitextensions/gitextensions/pull/3471#end");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseCustomeSchemeLinkWithHash()
        {
            string expected = "ftp://github.com/gitextensions/gitextensions/pull/3471#end";
            string actual = LinkFactory.ParseLink("PR#3471 and Issue#64#ftp://github.com/gitextensions/gitextensions/pull/3471#end");
            Assert.That(actual, Is.EqualTo(expected));
        }

    }
}
