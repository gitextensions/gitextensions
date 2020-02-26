using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using GitUI;
using NUnit.Framework;

namespace GitExtensions.UITests
{
    public static class UITest
    {
        public static async Task WaitForIdleAsync()
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

        public static void RunForm<T>(
            Action showForm,
            Func<T, Task> runTestAsync)
            where T : Form
        {
            Assert.IsEmpty(Application.OpenForms.OfType<T>(), $"{Application.OpenForms.OfType<T>().Count()} open form(s) before test");

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
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    await WaitForIdleAsync();
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

                showForm();

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                test.Join();
            }
            finally
            {
                form?.Dispose();
                Assert.IsEmpty(Application.OpenForms.OfType<T>(), $"{Application.OpenForms.OfType<T>().Count()} open form(s) after test");
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
                    var form = new Form { Text = $"Test {typeof(T).Name}" };
                    control = createControl(form);
                    Assert.True(form.Controls.Contains(control));
                    Application.Run(form);
                },
                runTestAsync: form => runTestAsync(control));
        }

        private readonly struct VoidResult
        {
        }
    }
}
