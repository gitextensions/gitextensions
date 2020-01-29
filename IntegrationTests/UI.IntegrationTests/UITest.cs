using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            // If form.Dipose was called by the async test task, closing the window would be done in a strange order.
            T form = null;
            Console.WriteLine($"RunForm<{nameof(T)}> entry");
            try
            {
                // Start runTestAsync before calling showForm.
                // The latter might block until the form is closed, especially if using Application.Run(form).

                // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
                // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
                // complete.
                var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
                {
                    Console.WriteLine($"RunForm<{form.GetType().FullName}> async test entry");
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    Console.WriteLine($"RunForm<{form.GetType().FullName}> async test on main thread");
                    await WaitForIdleAsync();
                    Console.WriteLine($"RunForm<{form.GetType().FullName}> async test idle");
                    form = Application.OpenForms.OfType<T>().Single();
                    try
                    {
                        Assert.True(form.Visible, $"{form.GetType().FullName} {form.Name} should be visible");
                        await runTestAsync(form);
                    }
                    finally
                    {
                        form.Close();
                    }
                });

                showForm();

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                using var cts = new CancellationTokenSource(AsyncTestHelper.UnexpectedTimeout);
                test.Join(cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RunForm<{form.GetType().FullName}> failed: {ex.Demystify()}");
                throw;
            }
            finally
            {
                form?.Close();
                form?.Dispose();
                Assert.IsTrue(Application.OpenForms.Count == 0, $"{Application.OpenForms.Count} open form(s) after test");
            }
        }

        private readonly struct VoidResult
        {
        }
    }
}
