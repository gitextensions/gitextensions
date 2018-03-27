using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;
using Path = System.IO.Path;

namespace GitUITests.CommandsDialogs.CommitDialog
{
    [Apartment(ApartmentState.STA)]
    public class FormCommitTests
    {
        // Created once for the fixture
        private GitModuleTestHelper _moduleTestHelper;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            if (_moduleTestHelper == null)
            {
                _moduleTestHelper = new GitModuleTestHelper();
            }

            // Undo potential impact from earlier tests
            CommitHelper.SetCommitMessage(_moduleTestHelper.Module, commitMessageText: null, amendCommit: false);

            _commands = new GitUICommands(_moduleTestHelper.Module);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _moduleTestHelper.Dispose();
        }

        [Test]
        public void PreserveCommitMessageOnReopen()
        {
            var generatedCommitMessage = Path.GetRandomFileName();

            RunFormCommitTest(formCommit =>
            {
                Assert.IsEmpty(formCommit.GetTestAccessor().Message.Text);
                formCommit.GetTestAccessor().Message.Text = generatedCommitMessage;
            });

            RunFormCommitTest(formCommit =>
            {
                Assert.AreEqual(generatedCommitMessage, formCommit.GetTestAccessor().Message.Text);
            });
        }

        private static async Task WaitForIdleAsync()
        {
            var idleCompletionSource = new TaskCompletionSource<VoidResult>();
            Application.Idle += HandleApplicationIdle;

            // Queue an event to make sure we don't stall if the application was already idle
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await Task.Yield();

            await idleCompletionSource.Task;
            Application.Idle -= HandleApplicationIdle;

            void HandleApplicationIdle(object sender, EventArgs e)
            {
                idleCompletionSource.TrySetResult(default);
            }
        }

        private void RunFormCommitTest(Action<FormCommit> testDriver)
        {
            RunFormCommitTest(formCommit =>
            {
                testDriver(formCommit);
                return Task.CompletedTask;
            });
        }

        private void RunFormCommitTest(Func<FormCommit, Task> testDriverAsync)
        {
            var asyncTest = ThreadHelper.JoinableTaskFactory.RunAsync(TestWrapperAsync);
            try
            {
                Assert.True(_commands.StartCommitDialog());
            }
            finally
            {
                asyncTest.Join();
            }

            return;

            // Local functions
            async Task TestWrapperAsync()
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await WaitForIdleAsync();

                var formCommit = Application.OpenForms.OfType<FormCommit>().Single();

                try
                {
                    await testDriverAsync(formCommit);
                }
                finally
                {
                    formCommit.Close();
                }
            }
        }

        private readonly struct VoidResult
        {
        }
    }
}
