﻿using System.IO.Abstractions;
using FluentAssertions;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;
using NSubstitute;

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
            ObjectId[] firstSelected = new[] { ObjectId.IndexId, ObjectId.Random() };

            GitRevision selectedRevision = new(ObjectId.WorkTreeId)
            {
                ParentIds = new[] { ObjectId.IndexId }
            };

            _tester.AllFirstAreParentsToSelected(firstSelected, selectedRevision).Should().BeFalse();
        }

        [Test]
        public void AllFirstAreParentsToSelected_should_return_true_if_all_parents_contains_all_of_selected_items()
        {
            ObjectId parent1 = ObjectId.Random();
            ObjectId parent2 = ObjectId.Random();

            ObjectId[] firstSelected2 = new[]
            {
                parent1,
                parent2
            };

            GitRevision selectedRevision2 = new(ObjectId.Random())
            {
                ParentIds = new[] { parent1, parent2 }
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
                new("file1") { IsTracked = true },
                new("file1") { IsTracked = false }
            };
            _tester.AnyLocalFileExists(selectedItemsWithParent).Should().BeTrue();
        }

        [Test]
        public void LocalExists_should_return_true_if_file_exists()
        {
            IEnumerable<GitItemStatus> selectedItemsWithParent = new List<GitItemStatus>
            {
                new("file1") { IsTracked = true },
                new("file2") { IsTracked = true }
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
                new("file1") { IsTracked = true },
                new("file2") { IsTracked = true }
            };
            _fullPathResolver.Resolve("file1").Returns("file1");
            _fullPathResolver.Resolve("file2").Returns("file2");
            _file.Exists("file1").Returns(false);
            _file.Exists("file2").Returns(false);
            _tester.AnyLocalFileExists(selectedItemsWithParent).Should().BeFalse();
        }

        [Test]
        public void Matches_should_not_throw_if_revision_null()
        {
            _tester.Matches(null, null).Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Matches_should_not_throw_if_criteria_null_or_empty(string criteria)
        {
            _tester.Matches(new GitRevision(ObjectId.Random()), criteria).Should().BeFalse();
        }

        [TestCase("myname")]
        [TestCase("myName")]
        public void Matches_should_match_name(string criteria)
        {
            IGitRef gitRef = Substitute.For<IGitRef>();
            gitRef.Name.Returns(x => "Name is MyName");
            GitRevision revision = new(ObjectId.Random()) { Refs = new[] { gitRef } };

            _tester.Matches(revision, criteria).Should().BeTrue();
        }

        [TestCase("001122", true)]
        [TestCase("", false)]
        [TestCase("0", false)]
        [TestCase("012", false)]
        public void Matches_should_match_guid(string criteria, bool expected)
        {
            IGitRef gitRef = Substitute.For<IGitRef>();
            gitRef.Name.Returns(x => "Name is MyName");

            GitRevision revision = new(ObjectId.Parse("0011223344556677889900112233445566778899")) { Refs = new[] { gitRef } };

            _tester.Matches(revision, criteria).Should().Be(expected);
        }

        [Test]
        public async Task ReverseSelection_expected_changes_none()
        {
            await Verifier.VerifyJson(JsonConvert.SerializeObject(new GitItemStatus("file1").InvertStatus()));
        }

        [Test]
        public async Task ReverseSelection_expected_changes_renamed()
        {
            await Verifier.VerifyJson(JsonConvert.SerializeObject(new GitItemStatus("file1") { IsRenamed = true, OldName = "file2" }.InvertStatus()));
        }

        [Test]
        public async Task ReverseSelection_expected_changes_new()
        {
            await Verifier.VerifyJson(JsonConvert.SerializeObject(new GitItemStatus("file1") { IsNew = true }.InvertStatus()));
        }

        [Test]
        public async Task ReverseSelection_expected_changes_deleted()
        {
            await Verifier.VerifyJson(JsonConvert.SerializeObject(new GitItemStatus("file1") { IsDeleted = true }.InvertStatus()));
        }

        [Test]
        public async Task ReverseSelection_expected_changes_unmerged()
        {
            await Verifier.VerifyJson(JsonConvert.SerializeObject(new GitItemStatus("file1") { IsUnmerged = true }.InvertStatus()));
        }

        [Test]
        public async Task ReverseSelection_expected_changes_getdefaults()
        {
            await Verifier.VerifyJson(JsonConvert.SerializeObject(GitItemStatus.GetDefaultStatus("file1").InvertStatus()));
        }
    }
}
