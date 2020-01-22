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
            // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
            // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
            // complete.
            var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await WaitForIdleAsync();
                using var form = Application.OpenForms.OfType<T>().Single();
                Assert.True(form.Visible, $"{form.GetType().FullName} {form.Name} should be visible");
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

            // Join the asynchronous test operation so any exceptions are rethrown on this thread
            test.Join();
        }

        public static void RunDialog<T>(
            Action<Form> showDialog,
            Func<T, Task> runTestAsync)
            where T : Form
        {
            using var mainForm = new Form { Text = $"Test {typeof(T).Name}" };
            RunForm<T>(
                showForm: () =>
                {
                    mainForm.Shown += (s, e) =>
                    {
                        showDialog(mainForm);
                    };

                    Application.Run(mainForm);
                },
                runTestAsync: async (dialog) =>
                {
                    try
                    {
                        await runTestAsync(dialog);
                    }
                    finally
                    {
                        // Explicitely close the mainForm here, in order to let Application.Run(mainForm) finish.
                        mainForm.Close();
                    }
                });
        }

        private readonly struct VoidResult
        {
        }
    }
}
