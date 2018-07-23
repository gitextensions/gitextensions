using NUnit.Framework;
using ResourceManager;

namespace ResourceManagerTests
{
    [TestFixture]
    internal class LinkFactoryFixture
    {
        [Test]
        public void ParseGoToBranchLink()
        {
            var linkFactory = new LinkFactory();
            linkFactory.CreateBranchLink("master");
            string expected = "gitext://gotobranch/master";
            string actual = linkFactory.ParseLink("master#gitext://gotobranch/master");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseGoToBranchLinkWithHash()
        {
            var linkFactory = new LinkFactory();
            linkFactory.CreateBranchLink("PR#23");
            string expected = "gitext://gotobranch/PR#23";
            string actual = linkFactory.ParseLink("PR#23#gitext://gotobranch/PR#23");
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static void TestCreateLink(string caption, string uri)
        {
            var linkFactory = new LinkFactory();
            linkFactory.CreateLink(caption, uri);
            string actual = linkFactory.ParseLink(caption + "#" + uri);
            string expected = uri;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseMailTo()
        {
            TestCreateLink("Janusz Białobrzewski <jbialobr@o2.pl>", "mailto:jbialobr@o2.pl");
        }

        [Test]
        public void ParseHttpLinkWithHash()
        {
            TestCreateLink("#3471", "https://github.com/gitextensions/gitextensions/pull/3471#end");
        }

        [Test]
        public void ParseRawHttpLinkWithHash()
        {
            var linkFactory = new LinkFactory();

            string expected = "https://github.com/gitextensions/gitextensions/pull/3471#end";
            string actual = linkFactory.ParseLink("https://github.com/gitextensions/gitextensions/pull/3471#end");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParseCustomSchemeLinkWithHash()
        {
            TestCreateLink("PR#3471 and Issue#64", "ftp://github.com/gitextensions/gitextensions/pull/3471#end");
        }
    }
}
