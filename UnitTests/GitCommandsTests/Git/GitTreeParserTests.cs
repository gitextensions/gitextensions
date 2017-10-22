using System.Linq;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
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
            var items = _parser.Parse(GetLsTreeOutput()).ToList();

            items.Count.Should().Be(10);
            var item = (GitItem)items[3];
            item.Guid.Should().Be("46cccae116d2e5a1a2f818b0b31adde4ab3800a9");
            item.Mode.Should().Be("100644");
            item.Name.Should().Be(".gitignore");
            item.ObjectType.Should().Be(GitObjectType.Blob);

            item = (GitItem)items[8];
            item.Guid.Should().Be("58d57013ed2ef925fc1b3f6fe72ead258c522e75");
            item.Mode.Should().Be("040000");
            item.Name.Should().Be("Bin");
            item.ObjectType.Should().Be(GitObjectType.Tree);
        }

        [Test]
        public void ParseSingle_should_return_null_for_null()
        {
            _parser.ParseSingle(null).Should().BeNull();
        }

        [Test]
        public void ParseSingle_should_return_null_if_input_shorter_than_required()
        {
            _parser.ParseSingle("").Should().BeNull();
            _parser.ParseSingle(new string('c', GitTreeParser.MinimumStringLength - 1)).Should().BeNull();
        }

        [Test]
        public void ParseSingle_should_return_GitItem()
        {
            const string s = "100644 blob 25d7b5d771e84982a3dfd8bd537531d8fb45d491    .editorconfig";
            var item = _parser.ParseSingle(s);

            item.Guid.Should().Be("25d7b5d771e84982a3dfd8bd537531d8fb45d491");
            item.Mode.Should().Be("100644");
            item.Name.Should().Be(".editorconfig");
            item.ObjectType.Should().Be(GitObjectType.Blob);
        }


        private static string GetLsTreeOutput()
        {
            return string.Concat("100644 blob 25d7b5d771e84982a3dfd8bd537531d8fb45d491	.editorconfig", "\n",
                "100644 blob bf29d31ff93be092ce746849e8db0984d4a83231	.gitattributes", "\0",
                "040000 tree 93185d6bd18327f5a23bc34e7eb75e66ec0ef2d1	.github", "\n",
                "100644 blob 46cccae116d2e5a1a2f818b0b31adde4ab3800a9	.gitignore", "\0",
                "100644 blob e55070b6c781e278bc68fc1b2525f56318d18244	.gitmodules", "\n",
                "100644 blob 1a569e3aa555e8cdf14dcc29f9bf4edf9aa465eb	.mailmap", "\n",
                "040000 tree 5c1f6ae123f16e2bee1c5a064cf293c11250d98f	.nuget", "\n",
                "100644 blob 1e53ed8f6759a92d4596af6a99ef04f1554bfd57	.travis.yml", "\0",
                "040000 tree 58d57013ed2ef925fc1b3f6fe72ead258c522e75	Bin", "\0",
                "040000 tree 0c7cce8981b980d03431f65b9b54c680a467fa2e	Build");
        }
    }
}
