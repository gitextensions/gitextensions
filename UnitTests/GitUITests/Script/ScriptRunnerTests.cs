using System;
using System.Windows.Forms;
using GitUI.Browsing;
using GitUI.Browsing.Dialogs;
using GitUI.Script;
using GitUIPluginInterfaces;
using Moq;
using NUnit.Framework;

namespace GitUITests.Script
{
    [TestFixture]
    internal sealed class ScriptRunnerTests
    {
        private IScriptRunner _scriptRunner;

        private readonly Mock<IGitModule> _moduleMock = new Mock<IGitModule>();
        private readonly Mock<IScriptOptionsParser> _scriptOptionsParserMock = new Mock<IScriptOptionsParser>();
        private readonly Mock<ISimpleDialog> _simpleDialogMock = new Mock<ISimpleDialog>();
        private readonly Mock<IScriptManager> _scriptManagerMock = new Mock<IScriptManager>();
        private readonly Mock<ICanGoToRef> _canGoToRef = new Mock<ICanGoToRef>();

        [SetUp]
        public void SetUp()
        {
            _scriptRunner = new ScriptRunner(
                _moduleMock.Object,
                new GitUIEventArgs(new Mock<IWin32Window>().Object, new Mock<IGitUICommands>().Object),
                _scriptOptionsParserMock.Object,
                _simpleDialogMock.Object,
                _scriptManagerMock.Object,
                _canGoToRef.Object);
        }

        [Test]
        public void RunScript_should_work()
        {
            // Arrange
            var scriptKey = Guid.NewGuid().ToString();

            // Act
            var result = _scriptRunner.RunScript(scriptKey);

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}
