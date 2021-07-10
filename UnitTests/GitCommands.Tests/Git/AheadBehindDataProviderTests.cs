using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using GitCommands.Git;
using GitExtUtils;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class AheadBehindDataProviderTests
    {
        private MemoryStream _standardOutputStream;
        private MemoryStream _standardErrorStream;
        private StreamReader _outputStreamReader;
        private StreamReader _errorStreamReader;
        private IProcess _process;
        private IExecutable _executable;
        private AheadBehindDataProvider _provider;

        [SetUp]
        public void Setup()
        {
            _standardOutputStream = new MemoryStream();
            _standardErrorStream = new MemoryStream();
            _outputStreamReader = new StreamReader(_standardOutputStream);
            _errorStreamReader = new StreamReader(_standardErrorStream);

            _process = Substitute.For<IProcess>();
            _process.StandardOutput.Returns(x => _outputStreamReader);
            _process.StandardError.Returns(x => _errorStreamReader);

            _executable = Substitute.For<IExecutable>();
            _executable.Start(Arg.Any<ArgumentString>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<Encoding>(), Arg.Any<bool>()).Returns(x => _process);

            _provider = new AheadBehindDataProvider(() => _executable);
        }

        [TearDown]
        public void TearDown()
        {
            _standardOutputStream?.Dispose();
            _standardErrorStream?.Dispose();
            _outputStreamReader?.Dispose();
            _errorStreamReader?.Dispose();
        }

        [TestCase(null)]
        public void GetData_should_throw_if_branch_null(string branchName)
        {
            ((Action)(() => _provider.GetTestAccessor().GetData(Encoding.UTF8, branchName))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void GetData_should_return_null_if_detached_head()
        {
            _provider.GetTestAccessor().GetData(Encoding.UTF8, "(no branch)").Should().BeNull();
        }

        [Test]
        public void GetData_should_return_null_if_git_output_empty()
        {
            _provider.GetTestAccessor().GetData(Encoding.UTF8, "**").Should().BeNull();
        }

        [TestCase("")]
        [TestCase(null)]
        public void GetData_should_return_null_result_if_unable_match_branch(string result)
        {
            SetResultOfGitCommand(result);

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "**");

            data.Should().BeNull();
        }

        [TestCase("::::::::my-branch")]
        [TestCase("::gone::::::branch-with-no-more-remote")]
        [TestCase("::::::::")]
        [TestCase("results!")]
        public void GetData_should_return_empty_if_git_output_has_no_data(string result)
        {
            SetResultOfGitCommand(result);

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "**");

            data.Should().HaveCount(0);
        }

        [TestCase("::ahead 1::::::my-branch")]
        [TestCase("results!")]
        public void GetData_should_return_empty_if_no_remote(string result)
        {
            SetResultOfGitCommand(result);

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "**");

            data.Should().HaveCount(0);
        }

        [Test]
        public void GetData_should_return_empty_string_for_unknown()
        {
            SetResultOfGitCommand("::translated-ahead 3::::refs/remotes/upstream/branch::my-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be(string.Empty);
            aheadBehindData.BehindCount.Should().Be(string.Empty);
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.RemoteRef.Should().Be("refs/remotes/upstream/branch");
            aheadBehindData.ToDisplay().Should().Be(string.Empty);
        }

        [Test]
        public void GetData_should_return_ahead_for_a_branch()
        {
            SetResultOfGitCommand("::ahead 10::::refs/remotes/upstream/branch::my-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("10");
            aheadBehindData.BehindCount.Should().Be(string.Empty);
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.RemoteRef.Should().Be("refs/remotes/upstream/branch");
            aheadBehindData.ToDisplay().Should().Be("10↑");
        }

        [Test]
        public void GetData_should_return_behind_for_a_branch()
        {
            SetResultOfGitCommand("::behind 2::::refs/remotes/upstream/my-branch::my-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("0");
            aheadBehindData.BehindCount.Should().Be("2");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.RemoteRef.Should().Be("refs/remotes/upstream/my-branch");
            aheadBehindData.ToDisplay().Should().Be("2↓");
        }

        [Test]
        public void GetData_should_return_zero_for_a_branch()
        {
            SetResultOfGitCommand("::::::refs/remotes/upstream/my-branch::my-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("0");
            aheadBehindData.BehindCount.Should().Be("");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.ToDisplay().Should().Be("0↑↓");
        }

        [TestCase("::ahead 99, behind 3::::refs/remotes/upstream/branch::my-branch")]
        [TestCase("ahead 99, behind 3::::refs/remotes/upstream/branch::::my-branch")]
        public void GetData_should_return_ahead_and_behind_for_a_branch(string result)
        {
            SetResultOfGitCommand(result);

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("99");
            aheadBehindData.BehindCount.Should().Be("3");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.RemoteRef.Should().Be("refs/remotes/upstream/branch");
            aheadBehindData.ToDisplay().Should().Be("99↑ 3↓");
        }

        [TestCase("ahead 99, behind 97::ahead 9, behind 7::refs/remotes/upstream/push-branch::refs/remotes/upstream/upstream-branch::my-branch")]
        public void GetData_should_prefer_push_before_upstream_ahead_behind(string result)
        {
            SetResultOfGitCommand(result);

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("99");
            aheadBehindData.BehindCount.Should().Be("97");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.RemoteRef.Should().Be("refs/remotes/upstream/push-branch");
            aheadBehindData.ToDisplay().Should().Be("99↑ 97↓");
        }

        [TestCase("ahead 99::ahead 9, behind 7::refs/remotes/upstream/push-branch::refs/remotes/upstream/upstream-branch::my-branch")]
        public void GetData_should_prefer_push_before_upstream_ahead(string result)
        {
            SetResultOfGitCommand(result);

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("99");
            aheadBehindData.BehindCount.Should().Be("");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.RemoteRef.Should().Be("refs/remotes/upstream/push-branch");
            aheadBehindData.ToDisplay().Should().Be("99↑");
        }

        [TestCase("behind 97::ahead 9, behind 7::refs/remotes/upstream/push-branch::refs/remotes/upstream/upstream-branch::my-branch")]
        public void GetData_should_prefer_push_before_upstream_behind(string result)
        {
            SetResultOfGitCommand(result);

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("");
            aheadBehindData.BehindCount.Should().Be("97");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.RemoteRef.Should().Be("refs/remotes/upstream/push-branch");
            aheadBehindData.ToDisplay().Should().Be("97↓");
        }

        [Test]
        public void GetData_should_return_ahead_and_behind_for_all_branches()
        {
            SetResultOfGitCommand("::::::::branch-not-changed\n"
                    + "::gone::::refs/remotes/upstream/branch::branch-with-no-more-remote\n"
                    + "::ahead 99, behind 3::::refs/remotes/upstream/branch::ahead-behind-branch\n"
                    + "::ahead 3::::refs/remotes/upstream/branch::ahead-branch\n"
                    + "::behind 4::::refs/remotes/upstream/branch::behind-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "*");

            data.Should().HaveCount(4);
            data.Should().BeEquivalentTo(
                new Dictionary<string, AheadBehindData>
                {
                    { "branch-with-no-more-remote", new AheadBehindData { AheadCount = "gone", BehindCount = "", Branch = "branch-with-no-more-remote", RemoteRef = "refs/remotes/upstream/branch" } },
                    { "ahead-behind-branch", new AheadBehindData { AheadCount = "99", BehindCount = "3", Branch = "ahead-behind-branch", RemoteRef = "refs/remotes/upstream/branch" } },
                    { "ahead-branch", new AheadBehindData { AheadCount = "3", BehindCount = string.Empty, Branch = "ahead-branch", RemoteRef = "refs/remotes/upstream/branch" } },
                    { "behind-branch", new AheadBehindData { AheadCount = "0", BehindCount = "4", Branch = "behind-branch", RemoteRef = "refs/remotes/upstream/branch" } },
                });
        }

        private void SetResultOfGitCommand(string result)
        {
            StreamWriter writer = new(_standardOutputStream);
            writer.Write(result);
            writer.Flush();
            _standardOutputStream.Position = 0;
        }
    }
}
