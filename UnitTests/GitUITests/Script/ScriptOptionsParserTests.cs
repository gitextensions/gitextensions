using FluentAssertions;
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
            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", "{cDefaultRemotePathFromUrl}", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

            result.Should().Be("{openUrl} https://gitlab.com/tree/{sBranch}");
        }

        [Test]
        public void ScriptOptionsParser_resolve_cDefaultRemotePathFromUrl_currentRemote_set()
        {
            var currentRemote = "myRemote";
            _module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote)).Returns("https://gitlab.com/gitlabhq/gitlabhq.git");

            var result = ScriptOptionsParser.GetTestAccessor().ParseScriptArguments("{openUrl} https://gitlab.com{cDefaultRemotePathFromUrl}/tree/{sBranch}", "{cDefaultRemotePathFromUrl}", null, null, _module, null, null, null, null, null, null, null, null, null, null, null, null, currentRemote);

            result.Should().Be("{openUrl} https://gitlab.com/gitlabhq/gitlabhq/tree/{sBranch}");
        }
    }
}
