﻿using System.ComponentModel.Design;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.Script
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class ScriptOptionsParserTests
    {
        private IGitUICommands _commands;
        private IGitModule _module;
        private IScriptHostControl _scriptHostControl;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _scriptHostControl = Substitute.For<IScriptHostControl>();
            _scriptHostControl.IsRevisionGrid.Returns(false);

            _commands = Substitute.For<IGitUICommands>();
            _commands.GitModule.Returns(_module);
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
        public void GetCurrentRevision_should_return_null_if_bare_or_empty_repo()
        {
            // bare repo has no current checkout, empty repo has no commits
            _module.GetRevision(shortFormat: true, loadRefs: true).Returns(x => null);

            GitRevision result = ScriptOptionsParser.GetTestAccessor()
                .GetCurrentRevision(module: _module, currentTags: null, currentLocalBranches: null, currentRemoteBranches: null, currentBranches: null, loadBody: false);

            result.Should().Be(null);
        }

        [Test]
        public void GetCurrentRevision_should_return_expected_if_current_revision_has_no_refs()
        {
            GitRevision revision = new(ObjectId.IndexId);
            _module.GetRevision(shortFormat: true, loadRefs: true).Returns(x => revision);

            GitRevision result = ScriptOptionsParser.GetTestAccessor()
                .GetCurrentRevision(module: _module, currentTags: null, currentLocalBranches: null, currentRemoteBranches: null, currentBranches: null, loadBody: false);

            result.Should().Be(revision);
        }

        [Test]
        public void Parse_should_throw_if_module_null()
        {
            ((Action)(() => ScriptOptionsParser.Parse(arguments: "bla", uiCommands: null, owner: null, _scriptHostControl))).Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'uiCommands')");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void Parse_should_return_without_process_if_arguments_unset(string arguments)
        {
            (string? arguments, bool abort) result = ScriptOptionsParser.Parse(arguments, uiCommands: null, owner: null, _scriptHostControl);

            result.arguments.Should().Be(arguments);
            result.abort.Should().Be(false);
        }

        [Test]
        public void Parse_should_return_unmodified_arguments_if_no_options_matched()
        {
            const string arguments = "{openUrl} https://gitlab.com{zeDefaultRemotePathFromUrl}/tree/{zeBranch}";

            (string? arguments, bool abort) result = ScriptOptionsParser.Parse(arguments: arguments, _commands, owner: null, _scriptHostControl);

            result.arguments.Should().Be(arguments);
            result.abort.Should().Be(false);
        }

        [Test]
        public void Parse_should_parse_c_arguments()
        {
            const string Subject = "line1";
            GitRevision revision = new(ObjectId.IndexId)
            {
                Subject = Subject,
                Body = $"{Subject}\n\nline3"
            };
            _module.GetRevision(shortFormat: false, loadRefs: true).Returns(x => revision);

            string expectedMessage = $"{Subject}\\n\\nline3";

            (string? arguments, bool abort) result = ScriptOptionsParser.Parse("echo {{cSubject}} {{cMessage}}", _commands, owner: null, _scriptHostControl);

            result.arguments.Should().Be($"echo \"{revision.Subject}\" \"{expectedMessage}\"");
            result.abort.Should().Be(false);
        }

        [Test]
        public void Parse_should_parse_s_arguments()
        {
            const string Subject = "line1";
            GitRevision revision = new(ObjectId.IndexId)
            {
                Subject = Subject,
                Body = $"{Subject}\n\nline3"
            };
            _scriptHostControl.IsRevisionGrid.Returns(true);
            _scriptHostControl.GetLatestSelectedRevision().Returns(x => revision);

            string expectedMessage = $"{Subject}\\n\\nline3";

            (string? arguments, bool abort) result = ScriptOptionsParser.Parse("echo {{sSubject}} {{sMessage}}", _commands, owner: null, _scriptHostControl);

            result.arguments.Should().Be($"echo \"{revision.Subject}\" \"{expectedMessage}\"");
            result.abort.Should().Be(false);
        }

        [Test]
        public void ParseScriptArguments_resolves_cDefaultRemotePathFromUrl_currentRemote_unset()
        {
            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", option: "cDefaultRemotePathFromUrl",
                owner: null, _scriptHostControl, uiCommands: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/tree/{sBranch}");
        }

        [Test]
        public void ParseScriptArguments_resolve_cDefaultRemotePathFromUrl_currentRemote_set()
        {
            string currentRemote = "myRemote";
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote)).Returns("https://gitlab.com/gitlabhq/gitlabhq.git");

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", option: "cDefaultRemotePathFromUrl",
                owner: null, _scriptHostControl, _commands, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote);

            result.Should().Be("{openUrl} https://gitlab.com/gitlabhq/gitlabhq/tree/{sBranch}");
        }

        [Test]
        public void ParseScriptArguments_resolve_sRemotePathFromUrl_selectedRemotes_empty()
        {
            List<string> noSelectedRemotes = new();

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{sRemotePathFromUrl}/tree/{sBranch}", option: "sRemotePathFromUrl",
                owner: null, _scriptHostControl, uiCommands: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, noSelectedRemotes, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be("{openUrl} https://gitlab.com/tree/{sBranch}");
        }

        [Test]
        public void ParseScriptArguments_resolve_sRemotePathFromUrl_currentRemote_set()
        {
            string currentRemote = "myRemote";
            List<string> selectedRemotes = new() { currentRemote };
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote)).Returns("https://gitlab.com/gitlabhq/gitlabhq.git");

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{openUrl} https://gitlab.com{sRemotePathFromUrl}/tree/{sBranch}", option: "sRemotePathFromUrl",
                owner: null, _scriptHostControl, _commands, allSelectedRevisions: null, selectedTags: null,
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
            string option = "sRemoteBranch";
            string branch = remoteName + '/' + branchName;
            List<IGitRef> remoteBranches = new() { new GitRef(null, null, branch) };

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, _scriptHostControl, uiCommands: null, allSelectedRevisions: null, selectedTags: null,
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
            string option = "sRemoteBranchName";
            string branch = remoteName + '/' + branchName;
            List<IGitRef> remoteBranches = new() { new GitRef(null, null, branch) };

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, _scriptHostControl, uiCommands: null, allSelectedRevisions: null, selectedTags: null,
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
            string option = "cRemoteBranch";
            string branch = remoteName + '/' + branchName;
            List<IGitRef> remoteBranches = new() { new GitRef(null, null, branch) };

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, _scriptHostControl, uiCommands: null, allSelectedRevisions: null, selectedTags: null,
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
            string option = "cRemoteBranchName";
            string branch = remoteName + '/' + branchName;
            List<IGitRef> remoteBranches = new() { new GitRef(null, null, branch) };

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, _scriptHostControl, uiCommands: null, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: remoteBranches, currentRevision: null, currentRemote: null);

            result.Should().Be(branchName);
        }

        [Test]
        public void ParseScriptArguments_resolve_RepoName()
        {
            const string option = "RepoName";
            const string dirName = "Windows"; // chose one which will never contain a repo

            IRepositoryDescriptionProvider repositoryDescriptionProvider = Substitute.For<IRepositoryDescriptionProvider>();
            repositoryDescriptionProvider.Get(Arg.Any<string>()).Returns(dirName);

            ServiceContainer serviceContainer = new();
            serviceContainer.AddService(repositoryDescriptionProvider);

            _commands = new GitUICommands(serviceContainer, new GitModule(""));

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments(
                arguments: "{" + option + "}", option,
                owner: null, _scriptHostControl, _commands, allSelectedRevisions: null, selectedTags: null,
                selectedBranches: null, selectedLocalBranches: null, selectedRemoteBranches: null, selectedRemotes: null, selectedRevision: null,
                currentTags: null,
                currentBranches: null, currentLocalBranches: null, currentRemoteBranches: null, currentRevision: null, currentRemote: null);

            result.Should().Be(dirName);
        }

        [Test]
        public void ParseScriptArguments_resolve_QuotedWithBackslashAtEnd()
        {
            _module.WorkingDir.Returns("C:\\test path with whitespaces\\");

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{{WorkingDir}} \"{WorkingDir}\"", "WorkingDir", null, null, _commands, null, null, null, null, null, null, null, null, null, null, null, null, null);

            result.Should().Be("\"C:\\test path with whitespaces\\\\\" \"C:\\test path with whitespaces\\\"");
        }

        [Test]
        public void ParseScriptArguments_resolve_StringWithDoubleQuotes()
        {
            GitRevision gitRevision = new(ObjectId.Random());
            gitRevision.Subject = "test string with \"double qoutes\" and escaped \\\"double qoutes\\\"";

            string result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{{sMessage}}", "sMessage", null, null, null, null, null, null, null, null, null, gitRevision, null, null, null, null, null, null);
            result.Should().Be("\"test string with \\\"double qoutes\\\" and escaped \\\"double qoutes\\\"\"");

            result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{sMessage}", "sMessage", null, null, null, null, null, null, null, null, null, gitRevision, null, null, null, null, null, null);
            result.Should().Be("test string with \"double qoutes\" and escaped \\\"double qoutes\\\"");
        }
    }
}
