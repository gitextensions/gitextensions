using System;
using System.Diagnostics;
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
            Console.WriteLine($"{nameof(RunForm)} entry");
            var idleCompletionSource = new TaskCompletionSource<VoidResult>();

            // Start runAsync before calling showForm.
            // The latter might block until the form is closed, especially if using Application.Run(form).

            // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
            // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
            // complete.
            var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
            {
                Console.WriteLine($"{nameof(RunForm)}.test entry");

                // Wait for the form to be opened by the test thread.
                await idleCompletionSource.Task;
                Console.WriteLine($"{nameof(RunForm)}.test idle");
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
                    Console.WriteLine($"{nameof(RunForm)}.test form loaded");

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    Console.WriteLine($"{nameof(RunForm)}.test on main thread");

                    await runTestAsync(form);
                    Console.WriteLine($"{nameof(RunForm)}.test passed");
                }
                finally
                {
                    form.Close(); // also calls form.Dispose()
                    Console.WriteLine($"{nameof(RunForm)}.test form closed");
                }
            });

            // The following call to showForm will trigger messages.
            // So we can be sure we don't stall waiting for idle if the application was already idle.
            Application.Idle += HandleApplicationIdle;
            Console.WriteLine($"{nameof(RunForm)} subscribed");
            try
            {
                try
                {
                    showForm();
                    Console.WriteLine($"{nameof(RunForm)} form shown");
                }
                catch (Exception outer)
                {
                    try
                    {
                        // Wake up and join the asynchronous test operation.
                        Console.WriteLine($"{nameof(RunForm)} exception {outer.Demystify()}");
                        idleCompletionSource.TrySetResult(default);
                        test.Join();
                        Console.WriteLine($"{nameof(RunForm)} test joined");
                    }
                    catch (Exception inner)
                    {
                        // ignore
                        Console.WriteLine($"{nameof(RunForm)} exception {inner.Demystify()}");
                    }

                    throw;
                }

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                test.Join();
                Console.WriteLine($"{nameof(RunForm)} test joined");
            }
            finally
            {
                Application.Idle -= HandleApplicationIdle;
                Console.WriteLine($"{nameof(RunForm)} unsubscribed");
            }

            return;

            void HandleApplicationIdle(object sender, EventArgs e)
            {
                Console.WriteLine($"{nameof(HandleApplicationIdle)}");
                idleCompletionSource.TrySetResult(default);
            }
        }

        private readonly struct VoidResult
        {
        }
    }
}
