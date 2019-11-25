using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using GitUI;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace GitUITests
{
    public static class UITest
    {
        public static void RunForm<T>(
            Action showForm,
            Func<T, Task> runTestAsync)
            where T : Form
        {
            var idleCompletionSource = new TaskCompletionSource<VoidResult>();

            // Start runAsync before calling showForm.
            // The latter might block until the form is closed, especially if using Application.Run(form).

            // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
            // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
            // complete.
            var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
            {
                // Wait for the form to be opened by the test thread.
                await idleCompletionSource.Task;
                var form = Application.OpenForms.OfType<T>().Single();
                if (!form.Visible)
                {
                    var errorMessage = $"{form.GetType().FullName} {form.Name} should be visible";
                    form.Dispose();
                    Assert.Fail(errorMessage);
                }

                try
                {
                    // Wait for potential pending asynchronous tasks triggered by the form.
                    AsyncTestHelper.WaitForPendingOperations(AsyncTestHelper.UnexpectedTimeout);

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    await runTestAsync(form);
                }
                finally
                {
                    form.Close(); // also calls form.Dispose()
                }
            });

            // The following call to showForm will trigger messages.
            // So we can be sure we don't stall waiting for idle if the application was already idle.
            Application.Idle += HandleApplicationIdle;
            try
            {
                try
                {
                    showForm();
                }
                catch (Exception)
                {
                    try
                    {
                        // Wake up and join the asynchronous test operation.
                        idleCompletionSource.TrySetResult(default);
                        test.Join();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                    throw;
                }

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                test.Join();
            }
            finally
            {
                Application.Idle -= HandleApplicationIdle;
            }

            return;

            void HandleApplicationIdle(object sender, EventArgs e)
            {
                idleCompletionSource.TrySetResult(default);
            }
        }

        private readonly struct VoidResult
        {
        }
    }
}
