using System;
using System.Threading;
using System.Threading.Tasks;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Script;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitExtensions.UITests.Script
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ScriptRunnerTests
    {
        // The scripts available during the tests are created by ScriptManager's GetDefaultScripts().
        // We will adapt the script named "Example" for the needs of the test cases in this file.
        private const string _keyOfExampleScript = "Example";

        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _uiCommands;
        private IGitModule _module;
        private ScriptInfo _exampleScript;

        [SetUp]
        public void Setup()
        {
            if (_referenceRepository == null)
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
            string errorMessage = null;

            var result = ScriptRunner.RunScript(null, _module, invalidScriptKey, uiCommands: null, revisionGrid: null, error => errorMessage = error);

            result.Executed.Should().BeFalse();
            errorMessage.Should().Be("Cannot find script: " + invalidScriptKey);
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

            var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null);

            result.Should().BeEquivalentTo(new CommandStatus(true, needsGridRefresh: false));
        }

        [Test]
        public void RunScript_with_arguments_with_c_option_without_revision_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {cHash}";

            _module.GetCurrentCheckout().Returns((ObjectId)null);

            string errorMessage = null;

            var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null, error => errorMessage = error);

            result.Should().BeEquivalentTo(new CommandStatus(executed: false, needsGridRefresh: false));
            errorMessage.Should().Be($"There must be a revision in order to substitute the argument option(s) for the script to run.");
        }

        [Test]
        public void RunScript_with_arguments_with_s_option_without_RevisionGrid_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {sHash}";

            string errorMessage = null;

            var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, uiCommands: null, revisionGrid: null, error => errorMessage = error);

            result.Executed.Should().BeFalse();
            errorMessage.Should().Be($"Option sHash is only supported when started with revision grid available.");
        }

        [Test]
        public void RunScript_with_arguments_with_s_option_with_RevisionGrid_without_selection_shall_display_error_and_return_false()
        {
            _exampleScript.Command = "cmd";
            _exampleScript.Arguments = "/c echo {sHash}";

            _uiCommands = new GitUICommands("C:\\");

            string errorMessage = null;

            RunFormTest(formBrowse =>
            {
                var result = ScriptRunner.RunScript(null, _module, _keyOfExampleScript, _uiCommands, formBrowse.RevisionGridControl, error => errorMessage = error);
                formBrowse.RevisionGridControl.LatestSelectedRevision.Should().BeNull(); // check for correct test setup

                result.Should().BeEquivalentTo(new CommandStatus(executed: false, needsGridRefresh: false));
                errorMessage.Should().Be($"There must be a revision in order to substitute the argument option(s) for the script to run.");
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
                var result = ScriptRunner.RunScript(formBrowse, _referenceRepository.Module, _keyOfExampleScript, _uiCommands,
                                                    formBrowse.RevisionGridControl, error => errorMessage = error);

                errorMessage.Should().BeNull();
                result.Should().BeEquivalentTo(new CommandStatus(executed: true, needsGridRefresh: false));
            });
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
