using System;
using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitUI.Script;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Script
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class ScriptOptionsParserTests
    {
        private IGitModule _module;
        private IScriptHostControl _scriptHostControl;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _scriptHostControl = Substitute.For<IScriptHostControl>();
        }

        [Test]
        public void Contains_should_throw_if_arguments_null()
        {
            ((Action)(() => ScriptOptionsParser.Contains(null, null))).Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'arguments')");
        }

        [Test]
        public void Contains_should_throw_if_option_null()
        {
            ((Action)(() => ScriptOptionsParser.Contains("", null))).Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'option')");
        }

        [TestCase("", "", true)]
        [TestCase("", " ", true)]
        [TestCase(" ", "", true)]
        [TestCase(" ", " ", true)]
        [TestCase("{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", "sBranch", true)]
        [TestCase("{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", "zeBranch", false)]
        public void Contains_should_return_expected(string arguments, string option, bool expected)
        {
            ScriptOptionsParser.Contains(arguments, option).Should().Be(expected);
        }

        [Test]
        public void DependsOnSelectedRevision_should_throw_if_option_null()
        {
            ((Action)(() => ScriptOptionsParser.DependsOnSelectedRevision(null))).Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'option')");
        }

        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase("\t", false)]
        [TestCase("s", true)]
        [TestCase("sBranch", true)]
        [TestCase("zeBranch", false)]
        public void DependsOnSelectedRevision_should_return_expected(string option, bool expected)
        {
            ScriptOptionsParser.DependsOnSelectedRevision(option).Should().Be(expected);
        }

        [TestCase("", false, "")]
        [TestCase("", true, "")]
        [TestCase(" ", false, "")]
        [TestCase(" ", true, "")]
        [TestCase("\t", false, "")]
        [TestCase("\t", true, "")]
        [TestCase("a", false, "{a}")]
        [TestCase("a", true, "{{a}}")]
        [TestCase(" a", false, "{a}")]
        [TestCase(" a", true, "{{a}}")]
        [TestCase("a ", false, "{a}")]
        [TestCase("a ", true, "{{a}}")]
        [TestCase(" a ", false, "{a}")]
        [TestCase(" a ", true, "{{a}}")]
        [TestCase(" a\ta ", false, "{a\ta}")]
        [TestCase(" a\ta ", true, "{{a\ta}}")]
        [TestCase(" a\ta ", false, "{a\ta}")]
        [TestCase(" a\ta ", true, "{{a\ta}}")]
        public void CreateOption_returns_expected(string option, bool quoted, string expected)
        {
            ScriptOptionsParser.GetTestAccessor().CreateOption(option, quoted).Should().Be(expected);
        }

        [Test]
        public void GetCurrentRevision_should_return_null_if_scriptHostControl_unset_bare_or_empty_repo()
        {
            // bare repo has no current checkout, empty repo has no commits
            _module.GetCurrentCheckout().Returns(x => null);

            var result = ScriptOptionsParser.GetTestAccessor()
                .GetCurrentRevision(module: _module, scriptHostControl: null, currentTags: null, currentLocalBranches: null, currentRemoteBranches: null, currentBranches: null);

            result.Should().Be(null);
        }

        [Test]
        public void GetCurrentRevision_should_return_expected_if_scriptHostControl_unset_unmatched_ref()
        {
            _module.GetCurrentCheckout().Returns(x => ObjectId.IndexId);

            var result = ScriptOptionsParser.GetTestAccessor()
                .GetCurrentRevision(module: _module, scriptHostControl: null, currentTags: null, currentLocalBranches: null, currentRemoteBranches: null, currentBranches: null);

            result.ObjectId.Should().Be(ObjectId.IndexId);
        }

        [Test]
        public void GetCurrentRevision_should_return_null_if_scriptHostControl_set_current_revision_null()
        {
            _scriptHostControl.GetCurrentRevision().Returns(x => null);

            var result = ScriptOptionsParser.GetTestAccessor()
                .GetCurrentRevision(module: _module, scriptHostControl: _scriptHostControl, currentTags: null, currentLocalBranches: null, currentRemoteBranches: null, currentBranches: null);

            result.Should().Be(null);
        }

        [Test]
        public void GetCurrentRevision_should_return_expected_if_scriptHostControl_set_current_revision_has_no_refs()
        {
            var revision = new GitRevision(ObjectId.IndexId);
            _scriptHostControl.GetCurrentRevision().Returns(x => revision);

            var result = ScriptOptionsParser.GetTestAccessor()
                .GetCurrentRevision(module: _module, scriptHostControl: _scriptHostControl, currentTags: null, currentLocalBranches: null, currentRemoteBranches: null, currentBranches: null);

            result.Should().Be(revision);
        }

        [Test]
        public void Parse_should_throw_if_module_null()
        {
            ((Action)(() => ScriptOptionsParser.Parse(arguments: "bla", module: null, owner: null, scriptHostControl: null))).Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'module')");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void Parse_should_return_without_process_if_arguments_unset(string arguments)
        {
            var result = ScriptOptionsParser.Parse(arguments, module: null, owner: null, scriptHostControl: null);

            result.arguments.Should().Be(arguments);
            result.abort.Should().Be(false);
        }

        [Test]
        public void Parse_should_return_unmodified_arguments_if_no_options_matched()
        {
            const string arguments = "{openUrl} https://gitlab.com{zeDefaultRemotePathFromUrl}/tree/{zeBranch}";

            var result = ScriptOptionsParser.Parse(arguments: arguments, module: _module, owner: null, scriptHostControl: null);

            result.arguments.Should().Be(arguments);
            result.abort.Should().Be(false);
        }

        [Test]
        public void ParseScriptArguments_resolves_cDefaultRemotePathFromUrl_currentRemote_unset()
        {
            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", option: "cDefaultRemotePathFromUrl",
                owner: null, scriptHostControl: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/tree/{sBranch}");
        }

        [Test]
        public void ParseScriptArguments_resolve_cDefaultRemotePathFromUrl_currentRemote_set()
        {
            var currentRemote = "myRemote";
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote)).Returns("https://gitlab.com/gitlabhq/gitlabhq.git");

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", option: "cDefaultRemotePathFromUrl",
                owner: null, scriptHostControl: null, _module, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote);

            result.Should().Be("{openUrl} https://gitlab.com/gitlabhq/gitlabhq/tree/{sBranch}");
        }

        [Test]
        public void ParseScriptArguments_resolve_sRemotePathFromUrl_selectedRemotes_empty()
        {
            var noSelectedRemotes = new List<string>();

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{sRemotePathFromUrl}/tree/{sBranch}", option: "sRemotePathFromUrl",
                owner: null, scriptHostControl: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, noSelectedRemotes, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/tree/{sBranch}");
        }

        [Test]
        public void ParseScriptArguments_resolve_sRemotePathFromUrl_currentRemote_set()
        {
            var currentRemote = "myRemote";
            var selectedRemotes = new List<string>() { currentRemote };
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote)).Returns("https://gitlab.com/gitlabhq/gitlabhq.git");

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{sRemotePathFromUrl}/tree/{sBranch}", option: "sRemotePathFromUrl",
                owner: null, scriptHostControl: null, _module, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/gitlabhq/gitlabhq/tree/{sBranch}");
        }

        [Test]
        public void ParseScriptArguments_resolve_sRemoteBranch(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "sRemoteBranch";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, scriptHostControl: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: remoteBranches, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(branch);
        }

        [Test]
        public void ParseScriptArguments_resolve_sRemoteBranchName(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "sRemoteBranchName";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, scriptHostControl: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: remoteBranches, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(branchName);
        }

        [Test]
        public void ParseScriptArguments_resolve_cRemoteBranch(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "cRemoteBranch";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, scriptHostControl: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: remoteBranches, currentRevision: null, currentRemote: null);

            result.Should().Be(branch);
        }

        [Test]
        public void ParseScriptArguments_resolve_cRemoteBranchName(
            [Values("", "origin", "upstream", "remote")] string remoteName,
            [Values("", "branch", "feature/branch")] string branchName)
        {
            var option = "cRemoteBranchName";
            var branch = remoteName + '/' + branchName;
            var remoteBranches = new List<IGitRef>() { new GitRef(null, null, branch) };

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, scriptHostControl: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: remoteBranches, currentRevision: null, currentRemote: null);

            result.Should().Be(branchName);
        }

        [Test]
        public void ParseScriptArguments_resolve_RepoName_null()
        {
            var option = "RepoName";

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, scriptHostControl: null, module: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(string.Empty);
        }

        [Test]
        public void ParseScriptArguments_resolve_RepoName()
        {
            var option = "RepoName";
            var dirName = "Windows"; // chose one which will never contain a repo
            _module.WorkingDir.Returns("C:\\" + dirName);

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, scriptHostControl: null, _module, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(dirName);
        }

        [Test]
        public void ParseScriptArguments_resolve_QuotedWithBackslashAtEnd()
        {
            _module.WorkingDir.Returns("C:\\test path with whitespaces\\");

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{{WorkingDir}} \"{WorkingDir}\"", "WorkingDir", null, null, _module, null, null, null, null, null, null, null, null, null, null, null, null, null);

            result.Should().Be("\"C:\\test path with whitespaces\\\\\" \"C:\\test path with whitespaces\\\"");
        }

        [Test]
        public void ParseScriptArguments_resolve_StringWithDoubleQuotes()
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
