using System;
using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using ResourceManager;

namespace GitUITests.UserControls.RevisionGrid.Graph
{
    [TestFixture]
    public class LaneInfoProviderTests
    {
        private RevisionGraphRevision _artificialCommitNode;
        private RevisionGraphRevision _realCommitNode;
        private ILaneNodeLocator _laneNodeLocator;
        private LaneInfoProvider _infoProvider;

        [SetUp]
        public void Setup()
        {
            _artificialCommitNode = new RevisionGraphRevision(ObjectId.WorkTreeId, 0)
            {
                GitRevision = new GitCommands.GitRevision(ObjectId.WorkTreeId)
                {
                    Author = "John Doe",
                    AuthorDate = DateTime.Parse("2010-03-24 13:37:12"),
                    AuthorEmail = "j.doe@some.email.dotcom",
                    Body = "WIP: fixing bugs"
                }
            };
            var realCommitObjectId = ObjectId.Parse("a48da1aba59a65b2a7f0df7e3512817caf16819f");
            _realCommitNode = new RevisionGraphRevision(realCommitObjectId, 0)
            {
                GitRevision = new GitCommands.GitRevision(realCommitObjectId)
                {
                    Author = "John Doe",
                    AuthorDate = DateTime.Parse("2010-03-24 13:37:12"),
                    AuthorEmail = "j.doe@some.email.dotcom",
                    Subject = "fix: bugs",
                    Body = "fix: bugs\r\n\r\nall bugs fixed"
                }
            };
            _laneNodeLocator = Substitute.For<ILaneNodeLocator>();
            _infoProvider = new LaneInfoProvider(_laneNodeLocator);
        }

        private void GetLaneInfo_should_display(RevisionGraphRevision node, string prefix = "", string suffix = "")
        {
            _infoProvider.GetLaneInfo(0, 0).Should()
                .Be(string.Format("{0}{1}{2}{2}{3}{4}",
                    prefix,
                    node.GitRevision.Guid,
                    Environment.NewLine,
                    node.GitRevision.Body,
                    suffix));
        }

        [Test]
        public void GetLaneInfo_should_return_empty_if_node_null()
        {
            _infoProvider.GetLaneInfo(0, 0).Should().BeEmpty();
        }

        [Test]
        public void GetLaneInfo_should_return_no_info_if_node_revision_null()
        {
            var nodeWithoutRevision = new RevisionGraphRevision(ObjectId.WorkTreeId, 0);
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (nodeWithoutRevision, isAtNode: false));

            _infoProvider.GetLaneInfo(0, 0).Should().Be(LaneInfoProvider.TestAccessor.NoInfoText.Text);
        }

        [Test]
        public void GetLaneInfo_for_non_artificial_commit_should_contain_guid_and_mark_if_at_node()
        {
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: true));

            GetLaneInfo_should_display(_realCommitNode, prefix: "* ");
        }

        [Test]
        public void GetLaneInfo_for_non_artificial_commit_should_contain_guid_and_no_mark_if_not_at_node()
        {
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            GetLaneInfo_should_display(_realCommitNode);
        }

        [Test]
        public void GetLaneInfo_for_artificial_commit_should_not_add_guid_and_mark_if_at_node()
        {
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_artificialCommitNode, isAtNode: true));

            _infoProvider.GetLaneInfo(0, 0).Should().Be(_artificialCommitNode.GitRevision.Body);
        }

        [Test]
        public void GetLaneInfo_for_artificial_commit_should_not_add_guid_and_mark_if_not_at_node()
        {
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_artificialCommitNode, isAtNode: false));

            _infoProvider.GetLaneInfo(0, 0).Should().Be(_artificialCommitNode.GitRevision.Body);
        }

        [Test]
        public void GetLaneInfo_should_display_the_subject_if_singleline_body_null()
        {
            _realCommitNode.GitRevision.Body = null;
            _realCommitNode.GitRevision.HasMultiLineMessage = false;
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            GetLaneInfo_should_display(_realCommitNode, suffix: _realCommitNode.GitRevision.Subject);
        }

        [Test]
        public void GetLaneInfo_should_display_the_subject_and_hint_if_multiline_body_null()
        {
            _realCommitNode.GitRevision.Body = null;
            _realCommitNode.GitRevision.HasMultiLineMessage = true;
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            GetLaneInfo_should_display(_realCommitNode, suffix: _realCommitNode.GitRevision.Subject + Strings.BodyNotLoaded);
        }
    }
}