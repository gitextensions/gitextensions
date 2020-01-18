using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
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
            _executable.Start(Arg.Any<ArgumentString>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<Encoding>()).Returns(x => _process);

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

        [TestCase("::::my-branch")]
        [TestCase("::[gone]::branch-with-no-more-remote")]
        [TestCase("::::")]
        public void GetData_should_return_null_if_git_output_has_no_data(string result)
        {
            SetResultOfGitCommand(result);

            _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch").Should().BeNull();
        }

        [Test]
        public void GetData_should_return_empty_result_if_unable_match_branch()
        {
            SetResultOfGitCommand("results!");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "**");

            data.Should().BeNull();
        }

        [Test]
        public void GetData_should_return_ahead_for_a_branch()
        {
            SetResultOfGitCommand("::[ahead 10]::my-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("10");
            aheadBehindData.BehindCount.Should().Be(string.Empty);
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.ToDisplay().Should().Be("10↑");
        }

        [Test]
        public void GetData_should_return_behind_for_a_branch()
        {
            SetResultOfGitCommand("::[behind 2]::my-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be(string.Empty);
            aheadBehindData.BehindCount.Should().Be("2");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.ToDisplay().Should().Be("2↓");
        }

        [Test]
        public void GetData_should_return_ahead_and_behind_for_a_branch()
        {
            SetResultOfGitCommand("::[ahead 99, behind 3]::my-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "my-branch");

            data.Should().HaveCount(1);
            var aheadBehindData = data.Values.First();
            aheadBehindData.AheadCount.Should().Be("99");
            aheadBehindData.BehindCount.Should().Be("3");
            aheadBehindData.Branch.Should().Be("my-branch");
            aheadBehindData.ToDisplay().Should().Be("99↑ 3↓");
        }

        [Test]
        public void GetData_should_return_ahead_and_behind_for_all_branches()
        {
            SetResultOfGitCommand("::branch-not-changed\n"
                    + "::[gone]::branch-with-no-more-remote\n"
                    + "::[ahead 99, behind 3]::ahead-behind-branch\n"
                    + "::[ahead 3]::ahead-branch\n"
                    + "::[behind 4]::behind-branch");

            var data = _provider.GetTestAccessor().GetData(Encoding.UTF8, "*");

            data.Should().HaveCount(3);
            data.Should().BeEquivalentTo(
                new Dictionary<string, AheadBehindData>
                {
                    { "ahead-behind-branch", new AheadBehindData { AheadCount = "99", BehindCount = "3", Branch = "ahead-behind-branch" } },
                    { "ahead-branch", new AheadBehindData { AheadCount = "3", BehindCount = string.Empty, Branch = "ahead-branch" } },
                    { "behind-branch", new AheadBehindData { AheadCount = string.Empty, BehindCount = "4", Branch = "behind-branch" } },
                });
        }

        private void SetResultOfGitCommand(string result)
        {
            var writer = new StreamWriter(_standardOutputStream);
            writer.Write(result);
            writer.Flush();
            _standardOutputStream.Position = 0;
        }
    }
}