using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitRevisionTesterTests
    {
        private IGitRevisionTester _tester;


        [SetUp]
        public void Setup()
        {
            _tester = new GitRevisionTester();
        }


        [Test]
        public void Matches_should_not_throw_if_revsion_null()
        {
            _tester.Matches(null, null).Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Matches_should_not_throw_if_criteria_null_or_empty(string criteria)
        {
            _tester.Matches(new GitRevision(""), criteria).Should().BeFalse();
        }

        [TestCase("myname")]
        [TestCase("myName")]
        public void Matches_should_match_name(string criteria)
        {
            var gitRef = Substitute.For<IGitRef>();
            gitRef.Name.Returns(x => "Name is MyName");
            var revision = new GitRevision("");
            revision.Refs.Add(gitRef);

            _tester.Matches(revision, criteria).Should().BeTrue();
        }

        [TestCase("001122", true)]
        [TestCase("", false)]
        [TestCase("0", false)]
        [TestCase("012", false)]
        public void Matches_should_match_guid(string criteria, bool expected)
        {
            var gitRef = Substitute.For<IGitRef>();
            gitRef.Name.Returns(x => "Name is MyName");

            var revision = new GitRevision("0011223344");
            revision.Refs.Add(gitRef);

            _tester.Matches(revision, criteria).Should().Be(expected);
        }
    }
}