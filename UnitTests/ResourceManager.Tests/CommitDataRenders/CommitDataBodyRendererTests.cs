using System;
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
            ((Action)(() => _renderer.Render(null, true))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Render_should_render_body_with_links()
        {
            _module.TryResolvePartialCommitId("b3e7944792", out _).Returns(x =>
            {
                x[1] = ObjectId.Parse("b3e79447928051cfb3494c9c0ef1a1d0ecde56a8");
                return true;
            });
            _module.TryResolvePartialCommitId("11119447928051cfb3494c9c0ef1a1d0ecde56a8", out _).Returns(x =>
            {
                x[1] = ObjectId.Parse("11119447928051cfb3494c9c0ef1a1d0ecde56a8");
                return true;
            });

            var data = new CommitData(ObjectId.Random(), ObjectId.Random(),
                Array.Empty<ObjectId>(),
                "John Doe (Acme Inc) <John.Doe@test.com>", DateTime.UtcNow,
                "John Doe <John.Doe@test.com>", DateTime.UtcNow,
                "fix\n\nAllow cherry-picking multiple commits from FormBrowse menu\r\n\r\nThe ability to do so from the RevisionGrid context menu has been added in commit\r\nb3e7944792 and 11119447928051cfb3494c9c0ef1a1d0ecde56a8\r\n");

            var result = _rendererReal.Render(data, true);

            result.Should().Be("fix\n\nAllow cherry-picking multiple commits from FormBrowse menu\r\n\r\nThe ability to do so from the RevisionGrid context menu has been added in commit\r\n<a href='gitext://gotocommit/b3e79447928051cfb3494c9c0ef1a1d0ecde56a8'>b3e7944792</a> and <a href='gitext://gotocommit/11119447928051cfb3494c9c0ef1a1d0ecde56a8'>11119447928051cfb3494c9c0ef1a1d0ecde56a8</a>");
        }

        [Test]
        public void Render_should_render_body_without_links()
        {
            var data = new CommitData(ObjectId.Random(), ObjectId.Random(),
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
