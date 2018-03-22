using System.Collections.Generic;
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
        public void AllFirstAreParentsToSelected_should_return_false_if_parents_null()
        {
            _tester.AllFirstAreParentsToSelected(null, null).Should().BeFalse();
        }

        [Test]
        public void AllFirstAreParentsToSelected_should_return_false_if_no_parents_contains_any_of_selected_items()
        {
            IEnumerable<GitRevision> firstSelected = new List<GitRevision> { new GitRevision(GitRevision.IndexGuid), new GitRevision("HEAD") };
            GitRevision selectedRevision = new GitRevision(GitRevision.UnstagedGuid)
            {
                ParentGuids = new[] { GitRevision.IndexGuid }
            };
            _tester.AllFirstAreParentsToSelected(firstSelected, selectedRevision).Should().BeFalse();
        }

        [Test]
        public void AllFirstAreParentsToSelected_should_return_true_if_all_parents_contains_all_of_selected_items()
        {
            IEnumerable<GitRevision> firstSelected2 = new List<GitRevision> { new GitRevision("Parent1"), new GitRevision("Parent2") };
            GitRevision selectedRevision2 = new GitRevision("HEAD")
            {
                ParentGuids = new[] { "Parent1", "Parent2" }
            };
            _tester.AllFirstAreParentsToSelected(firstSelected2, selectedRevision2).Should().BeTrue();
        }

        [Test]
        public void LocalExists_should_should_return_false_if_items_null()
        {
            _tester.AnyLocalFileExists(null).Should().BeFalse();
        }

        [Test]
        public void LocalExists_should_return_true_if_any_items_not_tracked()
        {
            IEnumerable<GitItemStatus> selectedItemsWithParent = new List<GitItemStatus>
            {
                new GitItemStatus() { IsTracked = true },
                new GitItemStatus() { IsTracked = false }
            };
            _tester.AnyLocalFileExists(selectedItemsWithParent).Should().BeTrue();
        }

        [Test]
        public void LocalExists_should_return_true_if_file_exists()
        {
            IEnumerable<GitItemStatus> selectedItemsWithParent = new List<GitItemStatus>
            {
                new GitItemStatus() { IsTracked = true, Name = "file1" },
                new GitItemStatus() { IsTracked = true, Name = "file2" }
            };
            _fullPathResolver.Resolve("file1").Returns("file1");
            _fullPathResolver.Resolve("file2").Returns("file2");
            _file.Exists("file1").Returns(false);
            _file.Exists("file2").Returns(true);
            _tester.AnyLocalFileExists(selectedItemsWithParent).Should().BeTrue();
        }

        [Test]
        public void LocalExists_should_return_false_if_none_of_locally_tracked_items_have_files()
        {
            IEnumerable<GitItemStatus> selectedItemsWithParent = new List<GitItemStatus>
            {
                new GitItemStatus() { IsTracked = true, Name = "file1" },
                new GitItemStatus() { IsTracked = true, Name = "file2" }
            };
            _fullPathResolver.Resolve("file1").Returns("file1");
            _fullPathResolver.Resolve("file2").Returns("file2");
            _file.Exists("file1").Returns(false);
            _file.Exists("file2").Returns(false);
            _tester.AnyLocalFileExists(selectedItemsWithParent).Should().BeFalse();
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