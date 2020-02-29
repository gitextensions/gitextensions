﻿using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitUI.Script;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Script
{
    [TestFixture]
    public class ScriptOptionsParserTests
    {
        private IGitModule _module;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
        }

        [Test]
        public void ScriptOptionsParser_resolve_cDefaultRemotePathFromUrl_currentRemote_unset()
        {
            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", option: "cDefaultRemotePathFromUrl",
                owner: null, revisionGrid: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/tree/{sBranch}");
        }

        [Test]
        public void ScriptOptionsParser_resolve_cDefaultRemotePathFromUrl_currentRemote_set()
        {
            var currentRemote = "myRemote";
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote)).Returns("https://gitlab.com/gitlabhq/gitlabhq.git");

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", option: "cDefaultRemotePathFromUrl",
                owner: null, revisionGrid: null, _module, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote);

            result.Should().Be("{openUrl} https://gitlab.com/gitlabhq/gitlabhq/tree/{sBranch}");
        }

        [Test]
        public void ScriptOptionsParser_resolve_sRemotePathFromUrl_selectedRemotes_empty()
        {
            var noSelectedRemotes = new List<string>();

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{sRemotePathFromUrl}/tree/{sBranch}", option: "sRemotePathFromUrl",
                owner: null, revisionGrid: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, noSelectedRemotes, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/tree/{sBranch}");
        }

        [Test]
        public void ScriptOptionsParser_resolve_sRemotePathFromUrl_currentRemote_set()
        {
            var currentRemote = "myRemote";
            var selectedRemotes = new List<string>() { currentRemote };
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote)).Returns("https://gitlab.com/gitlabhq/gitlabhq.git");

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{sRemotePathFromUrl}/tree/{sBranch}", option: "sRemotePathFromUrl",
                owner: null, revisionGrid: null, _module, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/gitlabhq/gitlabhq/tree/{sBranch}");
        }

        [Test]
        public void ScriptOptionsParser_resolve_sRemoteBranch(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "sRemoteBranch";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, revisionGrid: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: remoteBranches, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(branch);
        }

        [Test]
        public void ScriptOptionsParser_resolve_sRemoteBranchName(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "sRemoteBranchName";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, revisionGrid: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: remoteBranches, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(branchName);
        }

        [Test]
        public void ScriptOptionsParser_resolve_cRemoteBranch(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "cRemoteBranch";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, revisionGrid: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: remoteBranches, currentRevision: null, currentRemote: null);

            result.Should().Be(branch);
        }

        [Test]
        public void ScriptOptionsParser_resolve_cRemoteBranchName(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "cRemoteBranchName";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, revisionGrid: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: remoteBranches, currentRevision: null, currentRemote: null);

            result.Should().Be(branchName);
        }

        [Test]
        public void ScriptOptionsParser_resolve_RepoName_null()
        {
            var option = "RepoName";

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, revisionGrid: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(string.Empty);
        }

        [Test]
        public void ScriptOptionsParser_resolve_RepoName()
        {
            var option = "RepoName";
            var dirName = "Windows"; // chose one which will never contain a repo
            _module.WorkingDir.Returns("C:\\" + dirName);

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, revisionGrid: null, _module, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(dirName);
        }

        [Test]
        public void ScriptOptionsParser_resolve_QuotedWithBackslashAtEnd()
        {
            _module.WorkingDir.Returns("C:\\test path with whitespaces\\");

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{{WorkingDir}} \"{WorkingDir}\"", "WorkingDir", null, null, _module, null, null, null, null, null, null, null, null, null, null, null, null, null);

            result.Should().Be("\"C:\\test path with whitespaces\\\\\" \"C:\\test path with whitespaces\\\"");
        }

        [Test]
        public void ScriptOptionsParser_resolve_StringWithDoubleQuotes()
        {
            GitRevision gitRevision = new GitRevision(ObjectId.Random());
            gitRevision.Subject = "test string with \"double qoutes\" and escaped \\\"double qoutes\\\"";

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{{sMessage}}", "sMessage", null, null, null, null, null, null, null, null, null, gitRevision, null, null, null, null, null, null);
            result.Should().Be("\"test string with \\\"double qoutes\\\" and escaped \\\"double qoutes\\\"\"");

            result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{sMessage}", "sMessage", null, null, null, null, null, null, null, null, null, gitRevision, null, null, null, null, null, null);
            result.Should().Be("test string with \"double qoutes\" and escaped \\\"double qoutes\\\"");
        }
    }
}
