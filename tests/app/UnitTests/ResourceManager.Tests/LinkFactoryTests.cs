using ResourceManager;

namespace ResourceManagerTests;
internal class LinkFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("no link")]
    public void ParseInvalidLink(string? link)
    {
        LinkFactory linkFactory = new();
        linkFactory.GetTestAccessor().TryParseLink(link, out Uri? actualUri).Should().BeFalse();
        actualUri.Should().BeNull();
    }

    [Test]
    public void ParseGoToBranchLink()
    {
        LinkFactory linkFactory = new();
        linkFactory.CreateBranchLink("master");
        string expected = "gitext://gotobranch/master";
        linkFactory.GetTestAccessor().TryParseLink("master#gitext://gotobranch/master", out Uri? actualUri).Should().BeTrue();
        actualUri!.AbsoluteUri.Should().Be(expected);
    }

    [Test]
    public void ParseGoToBranchLinkWithHash()
    {
        LinkFactory linkFactory = new();
        linkFactory.CreateBranchLink("PR#23");
        string expected = "gitext://gotobranch/PR#23";
        linkFactory.GetTestAccessor().TryParseLink("PR#23#gitext://gotobranch/PR#23", out Uri? actualUri).Should().BeTrue();
        actualUri!.AbsoluteUri.Should().Be(expected);
    }

    public void ParseGoToBranchLinkWithDetachedHead()
    {
        const string linkCaption = "(HEAD detached at 178264)";
        LinkFactory linkFactory = new();
        linkFactory.CreateBranchLink(linkCaption);
        string expected = "gitext://gotobranch/HEAD";
        linkFactory.GetTestAccessor().TryParseLink($"{linkCaption}#{expected}", out Uri? actualUri).Should().BeTrue();
        actualUri!.AbsoluteUri.Should().Be(expected);
    }

    private static void TestCreateLink(string caption, string uri)
    {
        LinkFactory linkFactory = new();
        linkFactory.CreateLink(caption, uri);
        string expected = uri;
        linkFactory.GetTestAccessor().TryParseLink(caption + "#" + uri, out Uri? actualUri).Should().BeTrue();
        actualUri!.AbsoluteUri.Should().Be(expected);
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
        linkFactory.GetTestAccessor().TryParseLink("https://github.com/gitextensions/gitextensions/pull/3471#end", out Uri? actualUri).Should().BeTrue();
        actualUri!.AbsoluteUri.Should().Be(expected);
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
        linkFactory.GetTestAccessor().ParseInternalScheme(null!, out CommandEventArgs? actualCommandEventArgs).Should().BeFalse();
        actualCommandEventArgs.Should().BeNull();
    }

    [TestCase("slkldfjdfkj:fkjsd")]
    [TestCase("slkldfjdfkj://fkjsd")]
    [TestCase("slkldfjdfkj://")]
    [TestCase("http://x")]
    public void ParseInternalScheme_None(string? link)
    {
        LinkFactory linkFactory = new();
        Uri uri = new(link!);
        linkFactory.GetTestAccessor().ParseInternalScheme(uri, out CommandEventArgs? actualCommandEventArgs).Should().BeFalse();
        actualCommandEventArgs.Should().BeNull();
    }

    [TestCase("gitext://command/data", "command", "data")]
    [TestCase("gitext://command/data/more", "command", "data/more")]
    [TestCase("gitext://", "", "")]
    [TestCase("gitext://command", "command", "")]
    [TestCase("gitext://command/", "command", "")]
    [TestCase("gitext://command/d", "command", "d")]
    [TestCase("gitext://command/d/", "command", "d/")]
    [TestCase("gitext:not/an/internal/link", "", "not/an/internal/link")]
    public void ParseInternalScheme(string? link, string? expectedCommand, string? expectedData)
    {
        LinkFactory linkFactory = new();
        Uri uri = new(link!);
        linkFactory.GetTestAccessor().ParseInternalScheme(uri, out CommandEventArgs? actualCommandEventArgs).Should().BeTrue();
        actualCommandEventArgs!.Command.Should().Be(expectedCommand);
        actualCommandEventArgs.Data.Should().Be(expectedData);
    }

    [TestCase("gitext://command/data", "command", "data", null, "", false, true)]
    [TestCase("gitext://command/data", "command", "data", null, "", false, false)]
    [TestCase("gitext://showall/what", null, null, "what", "", true, false)]
    [TestCase("gitext://showall/what", null, null, "what", "", false, false)]
    [TestCase("gitext://command/data", null, null, null, "unexpected internal link: gitext://command/data", true, false)]
    [TestCase("gitext://showall/what", null, null, null, "unexpected internal link: gitext://showall/what", false, true)]
    public void ExecuteLink(string? link,
        string? expectedCommand,
        string? expectedData,
        string? expectedShowAll,
        string expectedException,
        bool omitHandler,
        bool omitShowAll)
    {
        LinkFactory linkFactory = new();
        CommandEventArgs? actualCommandEventArgs = null;
        string? actualShowAll = null;
        string actualException = "";
        Action<CommandEventArgs>? handleInternalLink = commandEventArgs => actualCommandEventArgs = commandEventArgs;
        Action<string?>? showAll = what => actualShowAll = what;
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

        actualCommandEventArgs?.Command.Should().Be(expectedCommand);
        actualCommandEventArgs?.Data.Should().Be(expectedData);
        actualShowAll.Should().Be(expectedShowAll);
        actualException.Should().Be(expectedException);
    }
}
