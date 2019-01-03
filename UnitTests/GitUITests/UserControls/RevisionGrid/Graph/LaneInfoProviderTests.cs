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
        /// <summary>
        /// triples of merge subject, merged branch and destination branch ("into")
        /// for testing the LaneInfoProvider.References.MergeRegex
        /// "(?i)^merged? (pull request (.*) from )?(.*branch |tag )?'?([^ ']*[^ '.])'?( of [^ ]*[^ .])?( into (.*[^.]))?\\.?$"
        /// </summary>
        private static readonly List<string> MergeSubjectsWithDecoding = new List<string>()
        {
            "Merge Branch xxx", // case-insignificance
            "xxx", "master",

            "merge branch xxx", // imperative
            "xxx", "master",

            "merged branch xxx", // past tense
            "xxx", "master",

            "merge pull request #1234 from xxx", // pull request
            "xxx" + string.Format(LaneInfoProvider.TestAccessor.ByPullRequestText.Text, "#1234"), "master",

            "merge tag yyy of remote/branch", // tag with (ignored) remote branch
            "yyy", "master",

            "merge tag yyy", // tag without ''
            "yyy", "master",

            "merge tag 'yyy'", // tag in ''
            "yyy", "master",

            "merge branch 'xxx'", // branch in ''
            "xxx", "master",

            "merge remote tracking branch xxx", // any text before branch
            "xxx", "master",

            "merge the branch xxx", // any text before branch
            "xxx", "master",

            "merge branch xxx into zzz", // into
            "xxx", "zzz",

            "Merged branch xxx.", // sentence
            "xxx", "master",

            "Merged branch xx.x.", // sentence with dot in branch
            "xx.x", "master",

            "Merged branch xxx into zzz.", // sentence with into
            "xxx", "zzz",

            "Merged branch xxx into zz.z.", // sentence with dot in into
            "xxx", "zz.z",

            "Merged tag yyy.", // sentence with tag
            "yyy", "master",

            "Merged tag yy.y.", // sentence with dot in tag
            "yy.y", "master",

            "Merged tag yyy of remote/branch.", // sentence with tag and remote
            "yyy", "master",

            "Merged tag yyy of remote/branc.h.", // sentence with tag and dot in remote
            "yyy", "master",

            "Merged tag yyy of remote/branch into zzz.", // sentence with tag, remote and into
            "yyy", "zzz",

            "Merged tag yyy of remote/branch into zz.z.", // sentence with tag, remote and dot in into
            "yyy", "zz.z"
        };

        private RevisionGraphRevision _artificialCommitNode;
        private RevisionGraphRevision _realCommitNode;
        private RevisionGraphRevision _mergeCommitNode;
        private RevisionGraphRevision _undetectedMergeCommitNode;
        private RevisionGraphRevision _innerCommitNode;
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
            var mergeCommitObjectId = ObjectId.Parse("b48da1aba59a65b2a7f0df7e3512817caf16819f");
            _mergeCommitNode = new RevisionGraphRevision(mergeCommitObjectId, 0)
            {
                GitRevision = new GitCommands.GitRevision(mergeCommitObjectId)
                {
                    Author = "John Doe",
                    AuthorDate = DateTime.Parse("2010-03-24 13:37:12"),
                    AuthorEmail = "j.doe@some.email.dotcom",
                    Subject = "merge remote tracking branch upstream/branch",
                    Body = "merge commit's subject here will not be parsed\r\n\r\nmerge commit's body might list details and/or conflicts...",
                    HasMultiLineMessage = true
                }
            };
            var undetectedMergeCommitObjectId = ObjectId.Parse("c48da1aba59a65b2a7f0df7e3512817caf16819f");
            _undetectedMergeCommitNode = new RevisionGraphRevision(undetectedMergeCommitObjectId, 0)
            {
                GitRevision = new GitCommands.GitRevision(undetectedMergeCommitObjectId)
                {
                    Author = "John Doe",
                    AuthorDate = DateTime.Parse("2010-03-24 13:37:12"),
                    AuthorEmail = "j.doe@some.email.dotcom",
                    Subject = "special merge",
                    Body = "merge commit's subject here will not be parsed\r\n\r\nmerge commit's body might list details and/or conflicts...",
                    HasMultiLineMessage = true
                }
            };
            var innerCommitObjectId = ObjectId.Parse("d48da1aba59a65b2a7f0df7e3512817caf16819f");
            _innerCommitNode = new RevisionGraphRevision(innerCommitObjectId, 0)
            {
                GitRevision = new GitCommands.GitRevision(innerCommitObjectId)
                {
                    Author = "John Doe",
                    AuthorDate = DateTime.Parse("2010-03-24 13:37:12"),
                    AuthorEmail = "j.doe@some.email.dotcom",
                    Subject = "fix: further bugs",
                    Body = "fix: further bugs"
                }
            };
            _laneNodeLocator = Substitute.For<ILaneNodeLocator>();
            _infoProvider = new LaneInfoProvider(_laneNodeLocator);
        }

        private void GetLaneInfo_should_display(RevisionGraphRevision node,
            string branch = null, string mergedWith = null,
            string prefix = "", string suffix = "")
        {
            _infoProvider.GetLaneInfo(0, 0).Should()
                .Be(string.Format(branch == null ? "{0}{1}{2}{2}{6}{7}"
                                                 : "{0}{1}{2}\n{3}: {4}{5}{2}{6}{7}",
                    prefix,
                    node.GitRevision.Guid,
                    Environment.NewLine,
                    Strings.Branch,
                    branch,
                    mergedWith == null ? "" : string.Format(LaneInfoProvider.TestAccessor.MergedWithText.Text, mergedWith),
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

        [Test]
        public void GetLaneInfo_should_display_branch_and_source_from_merge_node()
        {
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_mergeCommitNode, isAtNode: false));

            for (int index = 0; index < MergeSubjectsWithDecoding.Count; index += 3)
            {
                string subject = MergeSubjectsWithDecoding[index + 0];
                string mergedWith = MergeSubjectsWithDecoding[index + 1];
                string into = MergeSubjectsWithDecoding[index + 2];
                _mergeCommitNode.GitRevision.Subject = subject;

                GetLaneInfo_should_display(_mergeCommitNode, into, mergedWith);
            }
        }

        [Test]
        public void GetLaneInfo_should_display_only_the_branch_from_next_merge_node()
        {
            int maxScoreIsIgnored;
            _mergeCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            for (int index = 0; index < MergeSubjectsWithDecoding.Count; index += 3)
            {
                string subject = MergeSubjectsWithDecoding[index + 0];
                string mergedWith = MergeSubjectsWithDecoding[index + 1];
                string into = MergeSubjectsWithDecoding[index + 2];
                _mergeCommitNode.GitRevision.Subject = subject;

                GetLaneInfo_should_display(_realCommitNode, into);
            }
        }

        [Test]
        public void GetLaneInfo_should_display_only_the_branch_from_detected_merge_node_in_a_tree()
        {
            // synthetic test tree which does not need the right junction of merges
            //
            // merge
            // |     \
            // inner  (omitted)
            // |
            // undetected merge
            // |               \
            // real             (omitted)
            int maxScoreIsIgnored;
            _mergeCommitNode.AddParent(_innerCommitNode, out maxScoreIsIgnored);
            _innerCommitNode.AddParent(_undetectedMergeCommitNode, out maxScoreIsIgnored);
            _undetectedMergeCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            string subject = MergeSubjectsWithDecoding[0];
            string mergedWith = MergeSubjectsWithDecoding[1];
            string into = MergeSubjectsWithDecoding[2];
            _mergeCommitNode.GitRevision.Subject = subject;

            GetLaneInfo_should_display(_realCommitNode, into);
        }

        [Test]
        public void GetLaneInfo_should_display_only_the_branch_from_inner_node_in_a_tree()
        {
            // synthetic test tree which does not need the right junction of merges
            //
            // merge
            // |    \
            // inner (omitted)
            // |
            // real
            int maxScoreIsIgnored;
            _mergeCommitNode.AddParent(_innerCommitNode, out maxScoreIsIgnored);
            _innerCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);
            _realCommitNode.GitRevision.Refs = new GitRef[] { new GitRef(null, null, GitRefName.RefsTagsPrefix + "tag_shall_be_ignored") };
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            Check(new GitRef(null, null, GitRefName.RefsHeadsPrefix + "local_branch"));
            Check(new GitRef(null, null, GitRefName.RefsRemotesPrefix + "remote_branch", "origin"));
            Check(new GitRef(null, null, GitRefName.RefsStashPrefix + "@0"));

            return;

            void Check(GitRef gitRef)
            {
                _innerCommitNode.GitRevision.Refs = new GitRef[] { gitRef };

                GetLaneInfo_should_display(_realCommitNode, gitRef.Name);
            }
        }

        [Test]
        public void GetLaneInfo_should_prefer_the_branch_from_merge_to_a_GitRef__at_junction_node()
        {
            // synthetic test tree which does not need the right junction of merges
            //
            // merge
            // |    \
            // real  (omitted)
            int maxScoreIsIgnored;
            _mergeCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);

            GetLaneInfo_should_prefer_the_branch_from_merge_to_a_GitRef();
        }

        [Test]
        public void GetLaneInfo_should_prefer_the_branch_from_merge_to_a_GitRef__at_inner_node()
        {
            // synthetic test tree which does not need the right junction of merges
            //
            // artificial
            // |
            // merge
            // |    \
            // real  (omitted or maybe even missing)
            int maxScoreIsIgnored;
            _artificialCommitNode.AddParent(_mergeCommitNode, out maxScoreIsIgnored);
            _mergeCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);

            GetLaneInfo_should_prefer_the_branch_from_merge_to_a_GitRef();
        }

        private void GetLaneInfo_should_prefer_the_branch_from_merge_to_a_GitRef()
        {
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            string subject = MergeSubjectsWithDecoding[0];
            string mergedWith = MergeSubjectsWithDecoding[1];
            string into = MergeSubjectsWithDecoding[2];
            _mergeCommitNode.GitRevision.Subject = subject;

            var gitRef = new GitRef(null, null, GitRefName.RefsHeadsPrefix + "local_branch");
            _mergeCommitNode.GitRevision.Refs = new GitRef[] { gitRef };

            GetLaneInfo_should_display(_realCommitNode, into);
        }

        [Test]
        public void GetLaneInfo_should_display_the_merged_branch()
        {
            // test tree
            //
            // merge
            // |    \
            // inner real
            int maxScoreIsIgnored;
            _mergeCommitNode.AddParent(_innerCommitNode, out maxScoreIsIgnored);
            _mergeCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            string subject = MergeSubjectsWithDecoding[0];
            string mergedWith = MergeSubjectsWithDecoding[1];
            string into = MergeSubjectsWithDecoding[2];
            _mergeCommitNode.GitRevision.Subject = subject;

            GetLaneInfo_should_display(_realCommitNode, branch: mergedWith);
        }

        [Test]
        public void GetLaneInfo_should_not_display_a_branch_if_none_to_detect()
        {
            // synthetic test tree
            //
            // artificial
            // |
            // undetected merge
            // |   \
            // |    inner
            // |   /
            // real
            int maxScoreIsIgnored;
            _artificialCommitNode.AddParent(_undetectedMergeCommitNode, out maxScoreIsIgnored);
            _undetectedMergeCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);
            _undetectedMergeCommitNode.AddParent(_innerCommitNode, out maxScoreIsIgnored);
            _innerCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            GetLaneInfo_should_display(_realCommitNode);
        }

        [Test]
        public void GetLaneInfo_should_not_display_a_branch_if_none_at_first_children()
        {
            // synthetic test tree which does not need the right junction of merges
            //
            // artificial
            // |  merge
            // |  |     \
            // |  inner  (omitted)
            // | /
            // undetected merge
            // |               \
            // real             (omitted)
            int maxScoreIsIgnored;
            _artificialCommitNode.AddParent(_undetectedMergeCommitNode, out maxScoreIsIgnored);
            _undetectedMergeCommitNode.AddParent(_realCommitNode, out maxScoreIsIgnored);
            _mergeCommitNode.AddParent(_innerCommitNode, out maxScoreIsIgnored);
            _innerCommitNode.AddParent(_undetectedMergeCommitNode, out maxScoreIsIgnored);
            _laneNodeLocator.FindPrevNode(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (_realCommitNode, isAtNode: false));

            string subject = MergeSubjectsWithDecoding[0];
            string mergedWith = MergeSubjectsWithDecoding[1];
            string into = MergeSubjectsWithDecoding[2];
            _mergeCommitNode.GitRevision.Subject = subject;

            GetLaneInfo_should_display(_realCommitNode);
        }
    }
}