using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI;

namespace GitUITests
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
            Action showDialog,
            Func<T, Task> runAsync)
            where T : Form
        {
            // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
            // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
            // complete.
            var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await WaitForIdleAsync();
                var dialog = Application.OpenForms.OfType<T>().Single();
                try
                {
                    await runAsync(dialog);
                }
                finally
                {
                    dialog.Close();
                }
            });

            showDialog();

            // Join the asynchronous test operation so any exceptions are rethrown on this thread
            test.Join();
        }

        private readonly struct VoidResult
        {
        }
    }
}
