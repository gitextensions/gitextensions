using System.ComponentModel.Design;
using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using FluentAssertions.Specialized;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.NBugReports;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using NSubstitute;
using ResourceManager;

namespace GitExtensions.UITests.ScriptEngine
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    public class ScriptRunnerTests
    {
        // The scripts available during the tests are created by ScriptManager's GetDefaultScripts().
        // We will adapt the script named "Example" for the needs of the test cases in this file.
        private const int _keyOfExampleScript = 9002;

        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // perf optimisation: get hold of the static ScriptsManager.ScriptRunner.RunScript method for test invocations
        // we could have used TestAccessor, but it would involve more code.
        private static readonly MethodInfo _miRunScript = typeof(ScriptsManager.ScriptRunner).GetMethod("RunScriptInternal", BindingFlags.NonPublic | BindingFlags.Static);

        // Created once for each test
        private GitUICommands _uiCommands;
        private ScriptInfo _exampleScript;
        private MockForm _mockForm;
        private IGitModule _module;
        private IGitUICommands _commands;

        [SetUp]
        public void Setup()
        {
            ScriptsManager scriptsManager = new();
            scriptsManager.GetScripts();

            ServiceContainer serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();
            serviceContainer.RemoveService<IScriptsManager>();
            serviceContainer.RemoveService<IScriptsRunner>();
            serviceContainer.AddService<IScriptsManager>(scriptsManager);
            serviceContainer.AddService<IScriptsRunner>(scriptsManager);

            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _uiCommands = new GitUICommands(serviceContainer, _referenceRepository.Module);

            _module = Substitute.For<IGitModule>();
            _module.GetCurrentRemote().ReturnsForAnyArgs("origin");
            _module.GetCurrentCheckout().ReturnsForAnyArgs(ObjectId.WorkTreeId);

            _commands = Substitute.For<IGitUICommands>();
            _commands.Module.Returns(_module);

            _mockForm = new(_commands);

            _exampleScript = scriptsManager.GetScript(_keyOfExampleScript);
            _exampleScript.AskConfirmation = false; // avoid any dialogs popping up
            _exampleScript.RunInBackground = true; // avoid any dialogs popping up

            if (_miRunScript is null)
            {
                throw new InvalidOperationException();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void RunScript_without_command_shall_return_false([Values(null, "")] string command)
        {
            _exampleScript.Command = command;

            bool result = ScriptsManager.ScriptRunner.RunScript(_exampleScript, _mockForm, _mockForm.UICommands);

            result.Should().BeFalse();
        }

        [Test]
        public void RunScript_without_arguments_shall_succeed()
        {
            _exampleScript.Command = "{git}";
            _exampleScript.Arguments = "";

            bool result = ScriptsManager.ScriptRunner.RunScript(_exampleScript, _mockForm, _mockForm.UICommands);

            result.Should().BeTrue();
        }

        [Test]
        public void RunScript_with_arguments_without_options_shall_succeed()
        {
            _exampleScript.Command = "{git}";
            _exampleScript.Arguments = "--version";

            bool result = ScriptsManager.ScriptRunner.RunScript(_exampleScript, _mockForm, _mockForm.UICommands);

            result.Should().BeTrue();
        }

        [Test]
        public void RunScript_with_arguments_with_c_option_shall_succeed()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {cHash}";

            GitRevision revision = new(ObjectId.IndexId);
            _module.GetRevision(shortFormat: true, loadRefs: true).Returns(x => revision);

            bool result = ScriptsManager.ScriptRunner.RunScript(_exampleScript, _mockForm, _mockForm.UICommands);

            result.Should().BeTrue();
        }

        [Test]
        public void RunScript_with_arguments_with_c_option_without_revision_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {cHash}";

            _module.GetCurrentCheckout().Returns((ObjectId)null);

            ExceptionAssertions<UserExternalOperationException> ex = ((Action)(() => ExecuteRunScript(_exampleScript, _mockForm, _commands))).Should()
                .Throw<UserExternalOperationException>();
            ex.And.Context.Should().Be($"Script: '{_exampleScript.Name}'\r\nA valid revision is required to substitute the argument options");
            ex.And.Command.Should().Be(_exampleScript.Command);
            ex.And.Arguments.Should().Be(_exampleScript.Arguments);
            ex.And.WorkingDirectory.Should().Be(_module.WorkingDir);
        }

        [Test]
        public void RunScript_with_arguments_with_s_option_without_RevisionGrid_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {sHash}";

            _mockForm.UICommands.BrowseRepo = null;

            ExceptionAssertions<UserExternalOperationException> ex = ((Action)(() => ExecuteRunScript(_exampleScript, _mockForm, _mockForm.UICommands))).Should()
                .Throw<UserExternalOperationException>();
            ex.And.Context.Should().Be($"Script: '{_exampleScript.Name}'\r\n'sHash' option is only supported when invoked from the revision grid");
            ex.And.Command.Should().Be(_exampleScript.Command);
            ex.And.Arguments.Should().Be(_exampleScript.Arguments);
            ex.And.WorkingDirectory.Should().Be(_module.WorkingDir);
        }

        [Test]
        public void RunScript_with_arguments_with_s_option_with_RevisionGrid_without_selection_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {sHash}";

            RunFormTest(async formBrowse =>
            {
                // wait until the revisions are loaded
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                // check for correct test setup
                formBrowse.RevisionGridControl.GetTestAccessor().ClearSelection();
                Assert.AreEqual(0, formBrowse.RevisionGridControl.GetSelectedRevisions().Count);
                formBrowse.RevisionGridControl.LatestSelectedRevision.Should().BeNull();

                ExceptionAssertions<UserExternalOperationException> ex = ((Action)(() => ExecuteRunScript(_exampleScript, formBrowse, formBrowse.UICommands))).Should()
                        .Throw<UserExternalOperationException>();
                ex.And.Context.Should().Be($"Script: '{_exampleScript.Name}'\r\nA valid revision is required to substitute the argument options");
                ex.And.Command.Should().Be(_exampleScript.Command);
                ex.And.Arguments.Should().Be(_exampleScript.Arguments);
                ex.And.WorkingDirectory.Should().Be(_referenceRepository.Module.WorkingDir);
            });
        }

        [Test]
        public void RunScript_with_arguments_with_s_option_with_RevisionGrid_with_selection_shall_succeed()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {sHash}";
            _referenceRepository.CheckoutRevision();

            RunFormTest(async formBrowse =>
            {
                // wait until the revisions are loaded
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                Assert.AreEqual(1, formBrowse.RevisionGridControl.GetSelectedRevisions().Count);

                string errorMessage = null;
                bool result = ExecuteRunScript(_exampleScript, formBrowse, formBrowse.UICommands);

                errorMessage.Should().BeNull();
                result.Should().BeTrue();
            });
        }

        private static bool ExecuteRunScript(ScriptInfo script, IWin32Window owner, IGitUICommands uiCommands, IScriptOptionsProvider? scriptOptionsProvider = null)
        {
            try
            {
                bool result = (bool)_miRunScript.Invoke(null,
                    new object[]
                    {
                        script,
                        owner,
                        uiCommands,
                        scriptOptionsProvider
                    });
                return result;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        private void RunFormTest(Func<FormBrowse, Task> testDriverAsync)
        {
            UITest.RunForm(
                showForm: () => _uiCommands.StartBrowseDialog(owner: null).Should().BeTrue(),
                testDriverAsync);
        }

        private class MockForm : IWin32Window, IGitModuleForm
        {
            public MockForm(IGitUICommands commands)
            {
                UICommands = commands;
            }

            public IntPtr Handle { get; }
            public IGitUICommands UICommands { get; }
        }
    }
}
