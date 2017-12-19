using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using ResourceManager;

namespace GitCommandsTests
{
    [TestFixture]
    public class CommitInformationProviderTests
    {
        private CommitData _data;
        private IGitModule _module;
        private Func<IGitModule> _getModule;
        private ILinkFactory _linkFactory;
        private ICommitDataHeaderRenderer _commitInfoRenderer;
        private CommitInformationProvider _provider;


        [SetUp]
        public void Setup()
        {
            _data = new CommitData("c3c03473f3bc7fb3807b3132a20ac70743cdbda3", Guid.NewGuid().ToString(),
                                   new ReadOnlyCollection<string>(new List<string> { "a2b24c7f6ed5dacbac5470561914fcb27a992024" }),
                                   "John Doe (Acme Inc) <John.Doe@test.com>", DateTime.UtcNow,
                                   "John Doe <John.Doe@test.com>", DateTime.UtcNow,
                                   "\tI made a really neato change.\n\nNotes (p4notes):\n\tP4@547123");

            _linkFactory = Substitute.For<ILinkFactory>();

            _commitInfoRenderer = Substitute.For<ICommitDataHeaderRenderer>();
            _commitInfoRenderer.Render(Arg.Any<CommitData>(), true).Returns(x => null);

            _module = Substitute.For<IGitModule>();
            _getModule = () => _module;

            _provider = new CommitInformationProvider(_getModule, _linkFactory, _commitInfoRenderer);
        }


        [Test]
        public void GetCommitInfo_should_throw_if_data_null()
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Get(null, true));
        }

        [Test]
        public void GetCommitInfo_should_provide_information_without_revisions()
        {
            var header = @"Author:      <a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>
Date:        6 days ago (10/12/2017 10:52:13 PM)
Committer:   <a href='mailto:John.Doe@test.com'>John Doe &lt;John.Doe@test.com&gt;</a>
Commit hash: c3c03473f3bc7fb3807b3132a20ac70743cdbda3
Parent(s):   a2b24c7f6e
";
            _commitInfoRenderer.Render(Arg.Any<CommitData>(), false).Returns(x => header);

            var result = _provider.Get(_data, false);

            result.Should().NotBeNull();
            result.Header.Should().Be(header);
            result.Body.Should().Be("\n" + _data.Body.Trim());
        }
    }
}
