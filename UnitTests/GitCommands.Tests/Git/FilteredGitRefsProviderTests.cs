using FluentAssertions;
using GitExtUtils;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitCommandsTests.Git
{
    public sealed class FilteredGitRefsProviderTests
    {
        private IGitModule _module;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
        }

        private IGitRef CreateSubstituteRef(string guid, string completeName, string remote)
        {
            var isRemote = !string.IsNullOrEmpty(remote);
            var name = (isRemote ? remote + "/" : "") + completeName.LazySplit('/').LastOrDefault();
            var isTag = completeName.StartsWith("refs/tags/", StringComparison.InvariantCultureIgnoreCase);
            var isHead = completeName.StartsWith("refs/heads/", StringComparison.InvariantCultureIgnoreCase);
            var gitRef = Substitute.For<IGitRef>();
            gitRef.Module.Returns(_module);
            gitRef.Guid.Returns(guid);
            gitRef.CompleteName.Returns(completeName);
            gitRef.Name.Returns(name);
            gitRef.Remote.Returns(remote);
            gitRef.IsRemote.Returns(isRemote);
            gitRef.IsTag.Returns(isTag);
            gitRef.IsHead.Returns(isHead);
            return gitRef;
        }

        [Test]
        public void FilteredGitRefsProviderTest()
        {
            var refs = new[]
            {
                CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab4", "refs/heads/develop", ""),
                CreateSubstituteRef("02e10a13e06e7562f7c3c516abb2a0e1a0c0dd90", "refs/remotes/origin/develop", "origin"),
                CreateSubstituteRef("f6323b8e80f96dff017dd14bdb28a576556adab5", "refs/heads/local", ""),
            };

            _module.GetRefs(RefsFilter.NoFilter).ReturnsForAnyArgs(refs);

            FilteredGitRefsProvider filteredRefs = new(_module);

            filteredRefs.GetRefs(RefsFilter.NoFilter).Count.Should().Be(3);
            filteredRefs.GetRefs(RefsFilter.Remotes).Count.Should().Be(1);
            filteredRefs.GetRefs(RefsFilter.Heads).Count.Should().Be(2);
            filteredRefs.GetRefs(RefsFilter.Tags).Count.Should().Be(0);
            filteredRefs.GetRefs(RefsFilter.Remotes | RefsFilter.Heads).Count.Should().Be(3);
        }
    }
}
