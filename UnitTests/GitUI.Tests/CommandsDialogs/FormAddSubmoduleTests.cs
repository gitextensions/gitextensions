using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using GitExtUtils;
using GitUI.CommandsDialogs.SubmodulesDialog;
using GitUI.CommitInfo;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    public class FormAddSubmoduleTests
    {
        private const string DummyUrl = "URL";
        private const string DummyUrlEncoded = "\"URL\"";
        private const string Heads = "1234567890123456789012345678901234567890 /refs/heads/master\n"
                                   + "/refs/heads/branch\n"
                                   + "abc /refs/heads/\n"
                                   + "123/refs/heads/b123\n"
                                   + "456\t/refs/heads/b456\n"
                                   + "789  /refs/heads/b789";

        private readonly string[] _branches = { "master", "branch", "", "b123", "b456", "b789" };

        // Created once for each test
        private IExecutable _gitExecutable;

        [SetUp]
        public void SetUp()
        {
            _gitExecutable = Substitute.For<IExecutable>();
        }

        [Test]
        public void LoadRemoteRepoBranches_NoUrl([Values(null, "", " ")] string url)
        {
            FormAddSubmodule.TestAccessor.LoadRemoteRepoBranches(_gitExecutable, url)
                .Should().BeEmpty();
        }

        [TestCase("git@github.com:gitextensions/gitextensions.git", "\"git@github.com:gitextensions/gitextensions.git\"")]
        [TestCase("https://github.com/gitextensions/gitextensions.git", "\"https://github.com/gitextensions/gitextensions.git\"")]
        [TestCase("C:\\Repo", "\"C:/Repo\"")]
        public void LoadRemoteRepoBranches_Url(string url, string encodedUrl)
        {
            MockupGitOutput(Heads, encodedUrl);
            FormAddSubmodule.TestAccessor.LoadRemoteRepoBranches(_gitExecutable, url)
                .Should().BeEquivalentTo(_branches);
        }

        [Test]
        public void LoadRemoteRepoBranches_GitWarnings()
        {
            MockupGitOutput($"warning: this\n{Heads}\nwarning: or that");
            FormAddSubmodule.TestAccessor.LoadRemoteRepoBranches(_gitExecutable, DummyUrl)
                .Should().BeEquivalentTo(_branches);
        }

        [Test]
        public void LoadRemoteRepoBranches_GitError()
        {
            MockupGitOutput("error: no such repo");
            FormAddSubmodule.TestAccessor.LoadRemoteRepoBranches(_gitExecutable, DummyUrl)
                .Should().BeEmpty();
        }

        private void MockupGitOutput(string output, string encodedUrl = DummyUrlEncoded)
        {
            var gitArguments = new GitArgumentBuilder("ls-remote") { "--heads", encodedUrl };
            _gitExecutable.GetOutput(gitArguments).Returns(_ => output);
        }
    }
}