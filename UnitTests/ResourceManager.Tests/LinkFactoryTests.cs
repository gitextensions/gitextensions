using ResourceManager;

namespace ResourceManagerTests
{
    [TestFixture]
    internal class LinkFactoryTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("no link")]
        public void ParseInvalidLink(string link)
        {
            LinkFactory linkFactory = new();
            Assert.False(linkFactory.GetTestAccessor().TryParseLink(link, out var actualUri));
            Assert.That(actualUri, Is.Null);
        }

        [Test]
        public void ParseGoToBranchLink()
        {
            LinkFactory linkFactory = new();
            linkFactory.CreateBranchLink("master");
            string expected = "gitext://gotobranch/master";
            Assert.True(linkFactory.GetTestAccessor().TryParseLink("master#gitext://gotobranch/master", out var actualUri));
            Assert.That(actualUri.AbsoluteUri, Is.EqualTo(expected));
        }

        [Test]
        public void ParseGoToBranchLinkWithHash()
        {
            LinkFactory linkFactory = new();
            linkFactory.CreateBranchLink("PR#23");
            string expected = "gitext://gotobranch/PR#23";
            Assert.True(linkFactory.GetTestAccessor().TryParseLink("PR#23#gitext://gotobranch/PR#23", out var actualUri));
            Assert.That(actualUri.AbsoluteUri, Is.EqualTo(expected));
        }

        public void ParseGoToBranchLinkWithDetachedHead()
        {
            const string linkCaption = "(HEAD detached at 178264)";
            LinkFactory linkFactory = new();
            linkFactory.CreateBranchLink(linkCaption);
            string expected = "gitext://gotobranch/HEAD";
            Assert.True(linkFactory.GetTestAccessor().TryParseLink($"{linkCaption}#{expected}", out Uri? actualUri));
            Assert.That(actualUri.AbsoluteUri, Is.EqualTo(expected));
        }

        private static void TestCreateLink(string caption, string uri)
        {
            LinkFactory linkFactory = new();
            linkFactory.CreateLink(caption, uri);
            string expected = uri;
            Assert.True(linkFactory.GetTestAccessor().TryParseLink(caption + "#" + uri, out var actualUri));
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
            LinkFactory linkFactory = new();

            string expected = "https://github.com/gitextensions/gitextensions/pull/3471#end";
            Assert.True(linkFactory.GetTestAccessor().TryParseLink("https://github.com/gitextensions/gitextensions/pull/3471#end", out var actualUri));
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
            LinkFactory linkFactory = new();
            Assert.False(linkFactory.GetTestAccessor().ParseInternalScheme(null, out var actualCommandEventArgs));
            Assert.That(actualCommandEventArgs, Is.Null);
        }

        [TestCase("slkldfjdfkj:fkjsd")]
        [TestCase("slkldfjdfkj://fkjsd")]
        [TestCase("slkldfjdfkj://")]
        [TestCase("http://x")]
        public void ParseInternalScheme_None(string link)
        {
            LinkFactory linkFactory = new();
            Uri uri = new(link);
            Assert.False(linkFactory.GetTestAccessor().ParseInternalScheme(uri, out var actualCommandEventArgs));
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
            LinkFactory linkFactory = new();
            Uri uri = new(link);
            Assert.True(linkFactory.GetTestAccessor().ParseInternalScheme(uri, out var actualCommandEventArgs));
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
            LinkFactory linkFactory = new();
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
