using System;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid
{
    [TestFixture]
    public class GitRefListsForRevisionTests
    {
        private GitRevision _revision;
        private IGitModule _module;
        private IGitRef[] _refs;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _module.LocalConfigFile.Returns(Substitute.For<IConfigFileSettings>());

            _refs = new IGitRef[]
            {
                new GitRef(_module, ObjectId.Random(), $"{GitRefName.RefsTagsPrefix}tag1"),
                new GitRef(_module, ObjectId.Random(), $"{GitRefName.RefsHeadsPrefix}branch1"),
                new GitRef(_module, ObjectId.Random(), $"{GitRefName.RefsRemotesPrefix}branch1"),
            };
            _revision = new GitRevision(ObjectId.Random())
            {
                Refs = _refs
            };
        }

        [Test]
        public void ctor_must_throw_if_revision_null()
        {
            ((Action)(() => new GitRefListsForRevision(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ctor_must_throw_if_revision_ref_null()
        {
            ((Action)(() => new GitRefListsForRevision(new GitRevision(ObjectId.Random()) { Refs = null }))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AllBranches_must_return_all_revision_branches()
        {
            var grl = new GitRefListsForRevision(_revision);
            grl.AllBranches.Count.Should().Be(2);
        }

        [Test]
        public void AllTags_must_return_all_revision_tags()
        {
            var grl = new GitRefListsForRevision(_revision);
            grl.AllTags.Count.Should().Be(1);
        }

        [Test]
        public void GetAllBranchNames_must_return_branches_names()
        {
            var grl = new GitRefListsForRevision(_revision);
            grl.GetAllBranchNames().Should().BeEquivalentTo("branch1", "branch1");
        }

        [Test]
        public void GetAllTagNames_must_return_branches_names()
        {
            var grl = new GitRefListsForRevision(_revision);
            grl.GetAllTagNames().Should().BeEquivalentTo("tag1");
        }

        [Test]
        public void GetDeletableRefs_must_return_branches_names()
        {
            var grl = new GitRefListsForRevision(_revision);
            grl.GetDeletableRefs("branch1").Should().BeEquivalentTo(_refs[2], _refs[0]);
        }

        [Test]
        public void GetRenameableLocalBranches_must_return_branches_names()
        {
            var grl = new GitRefListsForRevision(_revision);
            grl.GetRenameableLocalBranches().Should().BeEquivalentTo(_refs[1]);
        }
    }
}