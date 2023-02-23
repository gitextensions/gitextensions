using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace ResourceManagerTests.CommitDataRenders
{
    [TestFixture]
    public class CommitDataBodyRendererTests
    {
        private IGitModule _module;
        private Func<IGitModule> _getModule;
        private ILinkFactory _linkFactory;
        private CommitDataBodyRenderer _renderer;
        private CommitDataBodyRenderer _rendererReal;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _getModule = () => _module;
            _linkFactory = Substitute.For<ILinkFactory>();

            _renderer = new CommitDataBodyRenderer(_getModule, _linkFactory);
            _rendererReal = new CommitDataBodyRenderer(_getModule, new LinkFactory());
        }

        [Test]
        public void Render_should_throw_if_data_null()
        {
            ((Action)(() => _renderer.Render(null!, true))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Render_should_render_body_with_links()
        {
            _module.TryResolvePartialCommitId(Arg.Any<string>(), out _).Returns(ci =>
            {
                ci[1] = ci[0] switch
                {
                    "b3e7944" => ObjectId.Parse("b3e79447928051cfb3494c9c0ef1a1d0ecde56a8"),
                    "11119447928051cfb3494c9c0ef1a1d0ecde56a8" => ObjectId.Parse((string)ci[0]),
                    _ => throw new Exception($"<shall not be called for {ci[0]}>")
                };
                return true;
            });

            CommitData data = new(ObjectId.Random(),
                Array.Empty<ObjectId>(),
                "John Doe (Acme Inc) <John.Doe@test.com>", DateTime.UtcNow,
                "John Doe <John.Doe@test.com>", DateTime.UtcNow,
                @"fix\n\nAllow cherry-picking multiple commits from FormBrowse menu

This short hex number shall not be turned into a link 12ab56, neither a github e-mail 1234567+user@github.com
b3e79447928051cfb3494c9c0ef1a1d0ecde56a8f is too long for a commit hash.
The ability to do so from the RevisionGrid context menu has been added in commit
b3e7944 and 11119447928051cfb3494c9c0ef1a1d0ecde56a8
@line 42.
");

            var result = _rendererReal.Render(data, true);

            result.Should().Be(@"fix\n\nAllow cherry-picking multiple commits from FormBrowse menu

This short hex number shall not be turned into a link 12ab56, neither a github e-mail 1234567+user@github.com
b3e79447928051cfb3494c9c0ef1a1d0ecde56a8f is too long for a commit hash.
The ability to do so from the RevisionGrid context menu has been added in commit
<a href='gitext://gotocommit/b3e79447928051cfb3494c9c0ef1a1d0ecde56a8'>b3e7944</a> and <a href='gitext://gotocommit/11119447928051cfb3494c9c0ef1a1d0ecde56a8'>11119447928051cfb3494c9c0ef1a1d0ecde56a8</a>
@line 42.");
        }

        [Test]
        public void Render_should_render_body_without_links()
        {
            CommitData data = new(ObjectId.Random(),
                Array.Empty<ObjectId>(),
                "John Doe (Acme Inc) <John.Doe@test.com>", DateTime.UtcNow,
                "John Doe <John.Doe@test.com>", DateTime.UtcNow,
                "fix\n\nAllow cherry-picking multiple commits from FormBrowse menu\r\n\r\nThe ability to do so from the RevisionGrid context menu has been added in commit\r\nb3e79447928051cfb3494c9c0ef1a1d0ecde56a8\r\n");

            var result = _rendererReal.Render(data, false);

            // TODO remove leading newline and achieve padding at the top via the control layout
            result.Should().Be("fix\n\nAllow cherry-picking multiple commits from FormBrowse menu\r\n\r\nThe ability to do so from the RevisionGrid context menu has been added in commit\r\nb3e79447928051cfb3494c9c0ef1a1d0ecde56a8");
        }
    }
}
