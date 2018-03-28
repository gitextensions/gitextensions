using System;
using FluentAssertions;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class LongShaProviderTests
    {
        private LongShaProvider _provider;
        private IGitModule _module;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();

            _provider = new LongShaProvider(() => _module);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void Get_should_return_revision_if_null_or_empty(string sha1)
        {
            var rev = _provider.Get(sha1);

            rev.Should().Be(sha1);
        }

        [Test]
        public void Get_should_throw_if_module_is_not_provided()
        {
            _module = null;

            ((Action)(() => _provider.Get("xx"))).Should().Throw<ArgumentException>();
        }

        [TestCase("0123", true)]
        [TestCase("0123", false)]
        public void Get_should_get_full_sha_for_existing_commits(string sha1Fragment, bool existing)
        {
            const string fullSha = "01234567890";
            _module.IsExistingCommitHash(sha1Fragment, out _)
                .Returns(x =>
                {
                    x[1] = fullSha;
                    return existing;
                });

            var rev = _provider.Get(sha1Fragment);

            rev.Should().NotBeNull();
            rev.Should().Be(existing ? fullSha : sha1Fragment);
        }
    }
}