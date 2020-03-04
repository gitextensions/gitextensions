using System;
using System.Collections.Generic;
using AzureDevOpsIntegration;
using FluentAssertions;
using NUnit.Framework;

namespace AzureDevOpsIntegrationTests
{
    [TestFixture]
    public class AzureDevOpsAdapterTests
    {
        private AzureDevOpsAdapter.TestAccessor _sut;

        [SetUp]
        public void Initialize()
        {
            var adapter = new AzureDevOpsAdapter();
            _sut = adapter.GetTestAccessor();
        }

        [Test]
        public void Should_not_filter_running_builds_When_only_one_returned()
        {
            var myBuild = new Build { SourceVersion = "ACommitHash" };
            IList<Build> runningBuilds = new List<Build> { myBuild };

            var filteredRunningBuilds = _sut.FilterRunningBuilds(runningBuilds);

            filteredRunningBuilds.Should().ContainSingle().And.Contain(myBuild);
        }

        [Test]
        public void Should_not_filter_running_builds_When_builds_are_on_different_commits()
        {
            var buildOnOneCommit = new Build { SourceVersion = "a_commit_Hash" };
            var buildOnAnotherCommit = new Build { SourceVersion = "another_commit_Hash" };
            IList<Build> runningBuilds = new List<Build> { buildOnOneCommit, buildOnAnotherCommit };

            var filteredRunningBuilds = _sut.FilterRunningBuilds(runningBuilds);

            filteredRunningBuilds.Should().ContainInOrder(buildOnOneCommit, buildOnAnotherCommit);
        }

        [Test]
        public void Should_take_only_the_first_started_running_builds_When_multiple_builds_on_same_commit()
        {
            var firstBuildStartedOnACommit = new Build { SourceVersion = "a_commit_Hash", StartTime = new DateTime(2010, 1, 1) };
            var buildAlsoStartedOnSameCommitButAfter = new Build { SourceVersion = "a_commit_Hash", StartTime = new DateTime(2010, 1, 2) };
            var buildAlsoStartedOnSameCommitButAfterAlso = new Build { SourceVersion = "a_commit_Hash", StartTime = new DateTime(2010, 1, 3) };
            IList<Build> runningBuilds = new List<Build> { firstBuildStartedOnACommit, buildAlsoStartedOnSameCommitButAfter, buildAlsoStartedOnSameCommitButAfterAlso };

            var filteredRunningBuilds = _sut.FilterRunningBuilds(runningBuilds);

            filteredRunningBuilds.Should().HaveCount(1).And.ContainInOrder(firstBuildStartedOnACommit);
        }

        [Test]
        public void Should_take_whatever_build_When_multiple_not_yet_started_builds_on_same_commit()
        {
            var firstBuildStartedOnACommit = new Build { SourceVersion = "a_commit_Hash" };
            var buildAlsoStartedOnSameCommitButAfter = new Build { SourceVersion = "a_commit_Hash" };
            var buildAlsoStartedOnSameCommitButAfterAlso = new Build { SourceVersion = "a_commit_Hash" };
            IList<Build> runningBuilds = new List<Build> { firstBuildStartedOnACommit, buildAlsoStartedOnSameCommitButAfter, buildAlsoStartedOnSameCommitButAfterAlso };

            var filteredRunningBuilds = _sut.FilterRunningBuilds(runningBuilds);

            filteredRunningBuilds.Should().HaveCount(1);
        }

        [Test]
        public void Should_take_only_a_started_running_builds_When_multiple_builds_on_same_commit()
        {
            var notStartedBuildOnACommit = new Build { SourceVersion = "a_commit_Hash" };
            var startedBuild = new Build { SourceVersion = "a_commit_Hash", StartTime = new DateTime(2010, 1, 2) };
            var anotherNotStartedBuildOnACommit = new Build { SourceVersion = "a_commit_Hash" };
            IList<Build> runningBuilds = new List<Build> { notStartedBuildOnACommit, startedBuild, anotherNotStartedBuildOnACommit };

            var filteredRunningBuilds = _sut.FilterRunningBuilds(runningBuilds);

            filteredRunningBuilds.Should().HaveCount(1).And.ContainInOrder(startedBuild);
        }
    }
}
