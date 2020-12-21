using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.NBugReports;
using GitUI.Script;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitExtensions.UITests.Script
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    public class ScriptRunnerTests
    {
        // The scripts available during the tests are created by ScriptManager's GetDefaultScripts().
        // We will adapt the script named "Example" for the needs of the test cases in this file.
        private const string _keyOfExampleScript = "Example";

        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // perf optimisation: get hold of the static ScriptRunner.RunScript method for test invocations
        // we could have used TestAccessor, but it would involve more code.
        private static readonly MethodInfo _miRunScript = typeof(ScriptRunner).GetMethod("RunScriptInternal", BindingFlags.NonPublic | BindingFlags.Static);

        // Created once for each test
        private GitUICommands _uiCommands;
        private IGitModule _module;
        private ScriptInfo _exampleScript;

        [SetUp]
        public void Setup()
        {
            if (_referenceRepository is null)
            {
                _referenceRepository = new ReferenceRepository();
            }
            else
            {
                _referenceRepository.Reset();
            }

            _uiCommands = new GitUICommands(_referenceRepository.Module);

            _module = Substitute.For<IGitModule>();
            _module.GetCurrentRemote().ReturnsForAnyArgs("origin");
            _module.GetCurrentCheckout().ReturnsForAnyArgs(ObjectId.WorkTreeId);
            _exampleScript = ScriptManager.GetScript(_keyOfExampleScript);
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
        public void RunScript_without_scriptKey_shall_return_false([Values(null, "")] string scriptKey)
        {
            var result = ScriptRunner.RunScript(null, _module, scriptKey, uiCommands: null, revisionGrid: null);

            result.Executed.Should().BeFalse();
        }

        [Test]
        public void RunScript_with_invalid_scriptKey_shall_display_error_and_return_false()
        {
            const string invalidScriptKey = "InVaLid ScRiPt KeY";

            var ex = ((Action)(() => ExecuteRunScript(null, _module, invalidScriptKey, uiCommands: null, revisionGrid: null))).Should()
                .Throw<UserExternalOperationException>();
            ex.And.Context.Should().Be($"Unable to find script: '{invalidScriptKey}'");
            ex.And.Command.Should().BeNull();
            ex.And.Arguments.Should().BeNull();
            ex.And.WorkingDirectory.Should().Be(_module.WorkingDir);
        }

        [Test]
        public void RunScript_without_command_shall_return_false([Values(null, "")] string command)
        {
            _exampleScript.Command = command;

            var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null);

            result.Executed.Should().BeFalse();
        }

        [Test]
        public void RunScript_without_arguments_shall_succeed()
        {
            _exampleScript.Command = "{git}";
            _exampleScript.Arguments = "";

            var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null);

            result.Should().BeEquivalentTo(new CommandStatus(true, needsGridRefresh: false));
        }

        [Test]
        public void RunScript_with_arguments_without_options_shall_succeed()
        {
            _exampleScript.Command = "{git}";
            _exampleScript.Arguments = "--version";

            var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null);

            result.Should().BeEquivalentTo(new CommandStatus(true, needsGridRefresh: false));
        }

        [Test]
        public void RunScript_with_arguments_with_c_option_shall_succeed()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {cHash}";

            var revision = new GitRevision(ObjectId.IndexId);
            _module.GetRevision(shortFormat: true, loadRefs: true).Returns(x => revision);

            var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null);

            result.Should().BeEquivalentTo(new CommandStatus(true, needsGridRefresh: false));
        }

        [Test]
        public void RunScript_with_arguments_with_c_option_without_revision_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {cHash}";

            _module.GetCurrentCheckout().Returns((ObjectId)null);

            var ex = ((Action)(() => ExecuteRunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null))).Should()
                .Throw<UserExternalOperationException>();
            ex.And.Context.Should().Be($"Script: '{_keyOfExampleScript}'\r\nA valid revision is required to substitute the argument options");
            ex.And.Command.Should().Be(_exampleScript.Command);
            ex.And.Arguments.Should().Be(_exampleScript.Arguments);
            ex.And.WorkingDirectory.Should().Be(_module.WorkingDir);
        }

        [Test]
        public void RunScript_with_arguments_with_s_option_without_RevisionGrid_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {sHash}";

            var ex = ((Action)(() => ExecuteRunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null))).Should()
                .Throw<UserExternalOperationException>();
            ex.And.Context.Should().Be($"Script: '{_keyOfExampleScript}'\r\n'sHash' option is only supported when invoked from the revision grid");
            ex.And.Command.Should().Be(_exampleScript.Command);
            ex.And.Arguments.Should().Be(_exampleScript.Arguments);
            ex.And.WorkingDirectory.Should().Be(_module.WorkingDir);
        }

        [Test]
        public void RunScript_with_arguments_with_s_option_with_RevisionGrid_without_selection_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {sHash}";

            _uiCommands = new GitUICommands("C:\\");

            RunFormTest(formBrowse =>
            {
                formBrowse.RevisionGridControl.LatestSelectedRevision.Should().BeNull(); // check for correct test setup

                var ex = ((Action)(() => ExecuteRunScript(null, _module, _keyOfExampleScript, _uiCommands, formBrowse.RevisionGridControl))).Should()
                    .Throw<UserExternalOperationException>();
                ex.And.Context.Should().Be($"Script: '{_keyOfExampleScript}'\r\nA valid revision is required to substitute the argument options");
                ex.And.Command.Should().Be(_exampleScript.Command);
                ex.And.Arguments.Should().Be(_exampleScript.Arguments);
                ex.And.WorkingDirectory.Should().Be(_module.WorkingDir);
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
                var result = ExecuteRunScript(formBrowse, _referenceRepository.Module, _keyOfExampleScript, _uiCommands,
                                              formBrowse.RevisionGridControl);

                errorMessage.Should().BeNull();
                result.Should().BeEquivalentTo(new CommandStatus(executed: true, needsGridRefresh: false));
            });
        }

        private CommandStatus ExecuteRunScript(IWin32Window owner, IGitModule module, string scriptKey, IGitUICommands uiCommands,
            RevisionGridControl revisionGrid)
        {
            try
            {
                var result = (CommandStatus)_miRunScript.Invoke(null,
                                                                 new object[]
                                                                 {
                                                                     owner,
                                                                     module,
                                                                     scriptKey,
                                                                     uiCommands,
                                                                     revisionGrid
                                                                 });
                return result;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        private void RunFormTest(Action<FormBrowse> testDriver)
        {
            RunFormTest(form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            });
        }

        private void RunFormTest(Func<FormBrowse, Task> testDriverAsync)
        {
            UITest.RunForm(
                showForm: () => _uiCommands.StartBrowseDialog(owner: null).Should().BeTrue(),
                testDriverAsync);
        }
    }
}
