using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            // If form.Dipose was called by the async test task, closing the window would be done in a strange order.
            T form = null;
            try
            {
                // Start runTestAsync before calling showForm.
                // The latter might block until the form is closed, especially if using Application.Run(form).

                // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
                // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
                // complete.
                var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
                {
                    try
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        await WaitForIdleAsync();
                        form = Application.OpenForms.OfType<T>().Single();
                    }
                    catch (Exception ex)
                    {
                        // This exception might not be visible because showForm might not return if the opened form is not closed.
                        // Log it for the case the "finally" code does not help.
                        Console.WriteLine($"{nameof(RunForm)}<{typeof(T).FullName}> async test preparation threw {ex.Demystify()}");
                        throw;
                    }
                    finally
                    {
                        Application.OpenForms.OfType<Form>().ForEach(f => f.Close());
                    }

                    try
                    {
                        await runTestAsync(form);
                    }
                    finally
                    {
                        form.Close();
                    }
                });

                showForm();

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                test.Join();
            }
            finally
            {
                form?.Dispose();
                Assert.IsTrue(Application.OpenForms.Count == 0, $"{Application.OpenForms.Count} open form(s) after test");
            }
        }

        private readonly struct VoidResult
        {
        }
    }
}
