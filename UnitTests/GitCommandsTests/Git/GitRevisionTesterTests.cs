using System.IO.Abstractions;
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
        private FileBase _file;
        private IFileSystem _fileSystem;
        private IFullPathResolver _fullPathResolver;
        private GitRevisionTester _tester;

        [SetUp]
        public void Setup()
        {
            _file = Substitute.For<FileBase>();
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);

            _fullPathResolver = Substitute.For<IFullPathResolver>();

            _tester = new GitRevisionTester(_fullPathResolver, _fileSystem);
        }

        [Test]
        public void IsFirstParent_should_return_false_if_parents_null()
        {
            _tester.IsFirstParent(null, null).Should().BeFalse();
        }

        [Test]
        public void IsFirstParent_should_return_false_if_no_parents_contains_any_of_selected_items()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void IsFirstParent_should_return_true_if_all_parents_contains_all_of_selected_items()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void LocalExists_should_should_return_false_if_items_null()
        {
            _tester.LocalRevisionExists(null).Should().BeFalse();
        }

        [Test]
        public void LocalExists_should_return_true_if_no_items_tracked()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void LocalExists_should_return_true_if_file_exists()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void LocalExists_should_return_false_if_none_of_locally_tracked_items_have_files()
        {
            Assert.Inconclusive();
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