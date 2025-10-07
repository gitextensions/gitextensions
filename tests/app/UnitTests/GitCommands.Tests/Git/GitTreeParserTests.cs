using FluentAssertions;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Git;

[TestFixture]
public class GitTreeParserTests
{
    private IGitTreeParser _parser;

    [SetUp]
    public void Setup()
    {
        _parser = new GitTreeParser();
    }

    [Test]
    public void Parse_should_return_empty_if_null()
    {
        _parser.Parse(null).Should().BeEmpty();
    }

    [Test]
    public void Parse_should_return_the_list()
    {
        List<GitItem> items = _parser.Parse(GetLsTreeOutput()).ToList();

        items.Should().HaveCount(13);

        items[3].Guid.Should().Be("46cccae116d2e5a1a2f818b0b31adde4ab3800a9");
        items[3].Mode.Should().Be(100644);
        items[3].Name.Should().Be(".gitignore");
        items[3].ObjectType.Should().Be(GitObjectType.Blob);

        items[9].Guid.Should().Be("38f33cf556b4aae690c640e48375cf1ff659b7a6");
        items[9].Mode.Should().Be(100644);
        items[9].Name.Should().Be(" space /in / path .txt");
        items[9].ObjectType.Should().Be(GitObjectType.Blob);

        items[10].Guid.Should().Be("58d57013ed2ef925fc1b3f6fe72ead258c522e75");
        items[10].Mode.Should().Be(040000);
        items[10].Name.Should().Be("Bin");
        items[10].ObjectType.Should().Be(GitObjectType.Tree);

        items[11].Guid.Should().Be("ec097fc11ec61f502d5fced60e27d54b5fe326c0");
        items[11].Mode.Should().Be(160000);
        items[11].Name.Should().Be("subm");
        items[11].ObjectType.Should().Be(GitObjectType.Commit);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("Hello World")]
    [TestCase("ZZZZZZ blob 0000000000000000000000000000000000000000\tREADME.md")]
    [TestCase("100644 blob 000000000000000000000000000000000000000\tREADME.md")]
    [TestCase("100644 blob 00000000000000000000000000000000000000000\tREADME.md")]
    [TestCase("100644 ZZZZ 00000000000000000000000000000000000000000\tREADME.md")]
    [TestCase("1006444 blob 0000000000000000000000000000000000000000\tREADME.md")]
    [TestCase("10064 blob 0000000000000000000000000000000000000000\tREADME.md")]
    [TestCase("100644 blob 960498876e233a6119e20fc73171bca2e26f57c0 space")]
    public void ParseSingle_should_return_null_if_input_invalid(string s)
    {
        _parser.ParseSingle(s).Should().BeNull();
    }

    [Test]
    public void ParseSingle_should_return_GitItem()
    {
        const string s = "100644 blob 25d7b5d771e84982a3dfd8bd537531d8fb45d491\t.editorconfig";
        GitItem item = _parser.ParseSingle(s);

        item.Guid.Should().Be("25d7b5d771e84982a3dfd8bd537531d8fb45d491");
        item.Mode.Should().Be(100644);
        item.Name.Should().Be(".editorconfig");
        item.ObjectType.Should().Be(GitObjectType.Blob);
    }

    private static string GetLsTreeOutput()
    {
        return string.Concat(
            "100644 blob 25d7b5d771e84982a3dfd8bd537531d8fb45d491\t.editorconfig", "\0",
            "100644 blob bf29d31ff93be092ce746849e8db0984d4a83231\t.gitattributes", "\0",
            "040000 tree 93185d6bd18327f5a23bc34e7eb75e66ec0ef2d1\t.github", "\0",
            "100644 blob 46cccae116d2e5a1a2f818b0b31adde4ab3800a9\t.gitignore", "\0",
            "100644 blob e55070b6c781e278bc68fc1b2525f56318d18244\t.gitmodules", "\0",
            "100644 blob 1a569e3aa555e8cdf14dcc29f9bf4edf9aa465eb\t.mailmap", "\0",
            "040000 tree 5c1f6ae123f16e2bee1c5a064cf293c11250d98f\t.nuget", "\0",
            "100644 blob 1e53ed8f6759a92d4596af6a99ef04f1554bfd57\t.travis.yml", "\0",
            "100644 blob 960498876e233a6119e20fc73171bca2e26f57c0\t space first", "\0",
            "100644 blob 38f33cf556b4aae690c640e48375cf1ff659b7a6\t space /in / path .txt", "\0",
            "040000 tree 58d57013ed2ef925fc1b3f6fe72ead258c522e75\tBin", "\0",
            "160000 commit ec097fc11ec61f502d5fced60e27d54b5fe326c0\tsubm", "\0",
            "040000 tree 0c7cce8981b980d03431f65b9b54c680a467fa2e\tBuild");
    }

    [Test]
    public void ParseLsFiles_should_return_the_list()
    {
        List<GitItem> items = _parser.ParseLsFiles(GetLsFilesOutput()).ToList();

        items.Should().HaveCount(6);

        items[1].Guid.Should().Be("532e4f49ecac926e5ff3881ec9cd46a9d48b5ddd");
        items[1].Mode.Should().Be(100755);
        items[1].Name.Should().Be("externals/Directory.Build.targets");
        items[1].ObjectType.Should().Be(GitObjectType.Blob);

        items[2].Guid.Should().Be("1b0386aea1acdd2ba258977bd79e40a0a7b95665");
        items[2].Mode.Should().Be(160000);
        items[2].Name.Should().Be("externals/Git.hub");
        items[2].ObjectType.Should().Be(GitObjectType.Commit);

        items[5].Guid.Should().Be("0c7cce8981b980d03431f65b9b54c680a467fa2e");
        items[5].Mode.Should().Be(040000);
        items[5].Name.Should().Be("externals");
        items[5].ObjectType.Should().Be(GitObjectType.Tree);
    }

    private static string GetLsFilesOutput()
    {
        return string.Concat(
            "100644 07c4d877fa885b9ef1ea2c343fe237beaf7a087c 0\texternals/Directory.Build.props", "\0",
            "100755 532e4f49ecac926e5ff3881ec9cd46a9d48b5ddd 0\texternals/Directory.Build.targets", "\0",
            "160000 1b0386aea1acdd2ba258977bd79e40a0a7b95665 0\texternals/Git.hub", "\0",
            "120000 be6183dc8f29079ce677b6834c56b05752828f23 0\texternals/ICSharpCode.TextEditor", "\0",
            "100644 b5400269ebd000ab4b8f44e4530ec3edfe104f8c 0\texternals/NetSpell.SpellChecker/Dictionary/Affix/AffixEntry.cs", "\0",
            "040000 0c7cce8981b980d03431f65b9b54c680a467fa2e 0\texternals", "\0",
            "040000 illegal-sha 0\texternals");
    }
}
