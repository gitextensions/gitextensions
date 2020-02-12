using System;
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
                        await runTestAsync(form);
                    }
                    finally
                    {
                        try
                        {
                            form?.Close();
                        }
                        finally
                        {
                            // Try to close all open Forms in order to let return the blocking Application.Run(form)
                            // which might have been called by showForm() below.
                            for (int trial = 0; trial < 3 && Application.OpenForms.Count > 0; ++trial)
                            {
                                Application.DoEvents();
                                Application.OpenForms.OfType<Form>().ForEach(f =>
                                {
                                    try
                                    {
                                        f.Close();
                                    }
                                    catch (Exception)
                                    {
                                        // ignore
                                    }
                                });
                            }
                        }
                    }
                });

                showForm();

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                test.Join();
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
