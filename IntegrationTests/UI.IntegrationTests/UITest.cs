using System;
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
                Log(nameof(HandleApplicationIdle));
                idleCompletionSource.TrySetResult(default);
            }
        }

        public static void RunForm<T>(
            Action showForm,
            Func<T, Task> runTestAsync,
            bool joinPendingOperationsAfterwards = false)
            where T : Form
        {
            // If form.Dipose was called by the async test task, closing the window would be done in a strange order.
            T form = null;
            Log($"RunForm<T> entry: Application.MessageLoop {Application.MessageLoop}");
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
                        Log($"RunForm<T> async test entry: Application.MessageLoop {Application.MessageLoop}");
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        Log($"RunForm<T> async test on main thread: Application.MessageLoop {Application.MessageLoop}");
                        await WaitForIdleAsync();
                        Log($"RunForm<T> async test idle: Application.MessageLoop {Application.MessageLoop}");
                        form = Application.OpenForms.OfType<T>().Single();
                        await runTestAsync(form);
                    }
                    catch (Exception ex)
                    {
                        Log("RunForm<T> async test threw", ex);
                        throw;
                    }
                    finally
                    {
                        try
                        {
                            Log("RunForm<T> async test closing form");
                            form?.Close();
                            Log("RunForm<T> async test form closed");
                        }
                        finally
                        {
                            try
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
                                            Log($"RunForm<T> async test CLOSING DANGLING FORM {f.GetType().FullName} {f.Name}");
                                            f.Close();
                                            Log("RunForm<T> async test dangling form closed");
                                        }
                                        catch (Exception ex)
                                        {
                                            // ignore
                                            Log("RunForm<T> async test IGNORING EXCEPTION from closing dangling form", ex);
                                        }
                                    });
                                }
                            }
                            finally
                            {
                                if (joinPendingOperationsAfterwards)
                                {
                                    await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                                }
                            }
                        }
                    }
                });

                showForm();
                Log("RunForm<T> showForm returned");

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                using var cts = new CancellationTokenSource(AsyncTestHelper.UnexpectedTimeout);
                test.Join(cts.Token);
                Log("RunForm<T> async test joined");
            }
            catch (Exception ex)
            {
                Log($"RunForm<{form?.GetType().FullName ?? "T"}> failed", ex);
                throw;
            }
            finally
            {
                ConfigureJoinableTaskFactoryAttribute.LoggingService?.Flush();
                form?.Close();
                form?.Dispose();
                Assert.IsTrue(Application.OpenForms.Count == 0, $"{Application.OpenForms.Count} open form(s) after test");
            }
        }

        public static LoggingService Log(string message)
            => ConfigureJoinableTaskFactoryAttribute.LoggingService?.Log(message, debugOnly: false);

        public static LoggingService Log(string message, Exception ex)
            => ConfigureJoinableTaskFactoryAttribute.LoggingService?.Log(message, ex);

        private readonly struct VoidResult
        {
        }
    }
}
