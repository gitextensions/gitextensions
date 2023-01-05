using System.Diagnostics;
using CommonTestUtils;
using GitUI;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitExtensions.UITests
{
    public static class UITest
    {
        // Same delay as RevisionDataGridView.BackgroundThreadUpdatePeriod
        private const int _processDelayMilliseconds = 25;

        public static async Task WaitForIdleAsync()
        {
            TaskCompletionSource<VoidResult> idleCompletionSource = new();
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

        public static void RunForm<T>(
            Action showForm,
            Func<T, Task> runTestAsync,
            bool debug = false)
            where T : Form
        {
            Assert.IsEmpty(Application.OpenForms.OfType<T>(), $"{Application.OpenForms.OfType<T>().Count()} open form(s) before test");

            // Needed for FormBrowse, ScriptOptionsParser
            ManagedExtensibility.Initialise(new[]
            {
#if GITUI
                typeof(GitUI.GitExtensionsForm).Assembly,
#endif
                typeof(GitCommands.GitModule).Assembly
            });

            T form = null;
            try
            {
                // Start runTestAsync before calling showForm, since the latter might block until the form is closed.
                //
                // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
                // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
                // complete.
                var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
                {
                    Log("switching to UI thread");
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    Log("waiting for idle");
                    await WaitForIdleAsync();
                    Log("idle");
                    form = Application.OpenForms.OfType<T>().Single();

                    try
                    {
                        await runTestAsync(form);
                    }
                    finally
                    {
                        // Close the form after the test completes. This will unblock the 'showForm()' call if it's
                        // waiting for the form to close.
                        form.Close();

                        // This should be changed to assert no pending operations once background operations are tied
                        // to the life of the owning dialog - issue #7792.
                        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                    }
                });

                Log("showing form");
                showForm();
                Log("form shown");

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                test.Join();
                Log("test task joined");
            }
            finally
            {
                form?.Dispose();
                Assert.IsEmpty(Application.OpenForms.OfType<T>(), $"{Application.OpenForms.OfType<T>().Count()} open form(s) after test");
            }

            return;

            void Log(string message)
            {
                if (debug)
                {
                    Console.WriteLine($"{DateTime.Now.TimeOfDay} {message}");
                }
            }
        }

        public static void RunControl<T>(
            Func<Form, T> createControl,
            Func<T, Task> runTestAsync)
            where T : Control
        {
            T control = null;
            RunForm<Form>(
                showForm: () =>
                {
                    Form form = new() { Text = $"Test {typeof(T).Name}" };
                    control = createControl(form);
                    Assert.True(form.Controls.Contains(control));
                    Application.Run(form);
                },
                runTestAsync: form => runTestAsync(control));
        }

        public static void ProcessUntil(string processName, Func<bool> condition, int maxMilliseconds = 1500)
        {
            int maxIterations = (maxMilliseconds + _processDelayMilliseconds - 1) / _processDelayMilliseconds;
            for (int iteration = 0; iteration < maxIterations; ++iteration)
            {
                if (condition())
                {
                    Debug.WriteLine($"'{processName}' successfully finished in iteration {iteration}");
                    return;
                }

                Application.DoEvents();
                Thread.Sleep(_processDelayMilliseconds);
            }

            Assert.Fail($"'{processName}' didn't finish in {maxIterations} iterations");
        }

        public static void ProcessEventsFor(int milliseconds)
        {
            int maxIterations = (milliseconds + _processDelayMilliseconds - 1) / _processDelayMilliseconds;
            for (int iteration = 0; iteration < maxIterations; ++iteration)
            {
                Application.DoEvents();
                Thread.Sleep(_processDelayMilliseconds);
            }
        }

        private readonly struct VoidResult
        {
        }
    }
}
