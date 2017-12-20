using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ResourceManager;

namespace ResourceManagerTests
{
    [TestFixture]
    public class CommitDataHeaderRendererTests
    {
        private ILinkFactory _linkFactory;
        private CommitDataHeaderRenderer _renderer;


        [SetUp]
        public void Setup()
        {
            _linkFactory = Substitute.For<ILinkFactory>();

            _renderer = new CommitDataHeaderRenderer(_linkFactory);
        }


        [Test]
        public void Render_should_throw_if_data_null()
        {
            ((Action)(() => _renderer.Render(null, false))).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void RenderPlain_should_throw_if_data_null()
        {
            ((Action)(() => _renderer.RenderPlain(null))).ShouldThrow<ArgumentNullException>();
        }
    }
}