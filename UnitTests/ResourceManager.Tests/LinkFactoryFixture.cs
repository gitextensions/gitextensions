using System;
using NUnit.Framework;
using ResourceManager;

namespace ResourceManagerTests
{
    [TestFixture]
    internal class LinkFactoryFixture
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("no link")]
        public void ParseInvalidLink(string link)
        {
            var linkFactory = new LinkFactory();
            Assert.False(linkFactory.ParseLink(link, out var actualUri));
            Assert.That(actualUri, Is.Null);
        }

        [Test]
        public void ParseGoToBranchLink()
        {
            var linkFactory = new LinkFactory();
            linkFactory.CreateBranchLink("master");
            string expected = "gitext://gotobranch/master";
            Assert.True(linkFactory.ParseLink("master#gitext://gotobranch/master", out var actualUri));
            Assert.That(actualUri.AbsoluteUri, Is.EqualTo(expected));
        }

        [Test]
        public void ParseGoToBranchLinkWithHash()
        {
            var linkFactory = new LinkFactory();
            linkFactory.CreateBranchLink("PR#23");
            string expected = "gitext://gotobranch/PR#23";
            Assert.True(linkFactory.ParseLink("PR#23#gitext://gotobranch/PR#23", out var actualUri));
            Assert.That(actualUri.AbsoluteUri, Is.EqualTo(expected));
        }

        private static void TestCreateLink(string caption, string uri)
        {
            var linkFactory = new LinkFactory();
            linkFactory.CreateLink(caption, uri);
            string expected = uri;
            Assert.True(linkFactory.ParseLink(caption + "#" + uri, out var actualUri));
            Assert.That(actualUri.AbsoluteUri, Is.EqualTo(expected));
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
            Assert.True(linkFactory.ParseLink("https://github.com/gitextensions/gitextensions/pull/3471#end", out var actualUri));
            Assert.That(actualUri.AbsoluteUri, Is.EqualTo(expected));
        }

        [Test]
        public void ParseCustomSchemeLinkWithHash()
        {
            TestCreateLink("PR#3471 and Issue#64", "ftp://github.com/gitextensions/gitextensions/pull/3471#end");
        }

        [Test]
        public void ParseInternalScheme_Null()
        {
            var linkFactory = new LinkFactory();
            Assert.False(linkFactory.ParseInternalScheme(null, out var actualCommandEventArgs));
            Assert.That(actualCommandEventArgs, Is.Null);
        }

        [TestCase("slkldfjdfkj:fkjsd")]
        [TestCase("slkldfjdfkj://fkjsd")]
        [TestCase("slkldfjdfkj://")]
        [TestCase("http://x")]
        public void ParseInternalScheme_None(string link)
        {
            var linkFactory = new LinkFactory();
            var uri = new Uri(link);
            Assert.False(linkFactory.ParseInternalScheme(uri, out var actualCommandEventArgs));
            Assert.That(actualCommandEventArgs, Is.Null);
        }

        [TestCase("gitext://command/data", "command", "data")]
        [TestCase("gitext://command/data/more", "command", "data/more")]
        [TestCase("gitext://", "", "")]
        [TestCase("gitext://command", "command", "")]
        [TestCase("gitext://command/", "command", "")]
        [TestCase("gitext://command/d", "command", "d")]
        [TestCase("gitext://command/d/", "command", "d/")]
        [TestCase("gitext:not/an/internal/link", "", "not/an/internal/link")]
        public void ParseInternalScheme(string link, string expectedCommand, string expectedData)
        {
            var linkFactory = new LinkFactory();
            var uri = new Uri(link);
            Assert.True(linkFactory.ParseInternalScheme(uri, out var actualCommandEventArgs));
            Assert.That(actualCommandEventArgs.Command, Is.EqualTo(expectedCommand));
            Assert.That(actualCommandEventArgs.Data, Is.EqualTo(expectedData));
        }

        [TestCase("gitext://command/data", "command", "data", null, "", false, true)]
        [TestCase("gitext://command/data", "command", "data", null, "", false, false)]
        [TestCase("gitext://showall/what", null, null, "what", "", true, false)]
        [TestCase("gitext://showall/what", null, null, "what", "", false, false)]
        [TestCase("gitext://command/data", null, null, null, "unexpected internal link: gitext://command/data", true, false)]
        [TestCase("gitext://showall/what", null, null, null, "unexpected internal link: gitext://showall/what", false, true)]
        public void ExecuteLink(string link,
            string expectedCommand,
            string expectedData,
            string expectedShowAll,
            string expectedException,
            bool omitHandler,
            bool omitShowAll)
        {
            var linkFactory = new LinkFactory();
            CommandEventArgs actualCommandEventArgs = null;
            string actualShowAll = null;
            string actualException = "";
            Action<CommandEventArgs> handleInternalLink = commandEventArgs => actualCommandEventArgs = commandEventArgs;
            Action<string> showAll = what => actualShowAll = what;
            if (omitHandler)
            {
                handleInternalLink = null;
            }

            if (omitShowAll)
            {
                showAll = null;
            }

            try
            {
                linkFactory.ExecuteLink(link, handleInternalLink, showAll);
            }
            catch (InvalidOperationException ex)
            {
                actualException = ex.Message;
            }

            Assert.That(actualCommandEventArgs?.Command, Is.EqualTo(expectedCommand));
            Assert.That(actualCommandEventArgs?.Data, Is.EqualTo(expectedData));
            Assert.That(actualShowAll, Is.EqualTo(expectedShowAll));
            Assert.That(actualException, Is.EqualTo(expectedException));
        }
    }
}
