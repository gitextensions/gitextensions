using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using GitUI;
using NUnit.Framework;

namespace GitExtensions.UITests
{
    public static class UITest
    {
        public static void RunForm<T>(
            Action showForm,
            Func<T, Task> runTestAsync)
            where T : Form
        {
            LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} entry");
            var idleCompletionSource = new TaskCompletionSource<VoidResult>();

            // Start runAsync before calling showForm.
            // The latter might block until the form is closed, especially if using Application.Run(form).

            // Avoid using ThreadHelper.JoinableTaskFactory for the outermost operation because we don't want the task
            // tracked by its collection. Otherwise, test code would not be able to wait for pending operations to
            // complete.
            var test = ThreadHelper.JoinableTaskContext.Factory.RunAsync(async () =>
            {
                LoggingService.Log($"{MethodBase.GetCurrentMethod().Name}.test entry");

                // Wait for the form to be opened by the test thread.
                await idleCompletionSource.Task;
                LoggingService.Log($"{MethodBase.GetCurrentMethod().Name}.test idle");
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
                    WaitForPendingOperations();
                    LoggingService.Log($"{MethodBase.GetCurrentMethod().Name}.test form loaded");

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    LoggingService.Log($"{MethodBase.GetCurrentMethod().Name}.test on main thread");

                    try
                    {
                        await runTestAsync(form);
                        LoggingService.Log($"{MethodBase.GetCurrentMethod().Name}.test passed");
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Log($"{MethodBase.GetCurrentMethod().Name}.test failed", ex);
                        throw;
                    }
                }
                finally
                {
                    CloseOrDispose(form, $"{MethodBase.GetCurrentMethod().Name}.test");
                }
            });

            // The following call to showForm will trigger messages.
            // So we can be sure we don't stall waiting for idle if the application was already idle.
            Application.Idle += HandleApplicationIdle;
            LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} subscribed");
            try
            {
                try
                {
                    showForm();
                    LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} form shown");
                }
                catch (Exception outer)
                {
                    try
                    {
                        // Wake up and join the asynchronous test operation.
                        LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} exception",  outer);
                        idleCompletionSource.TrySetResult(default);
                        test.Join();
                        LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} test joined");
                    }
                    catch (Exception inner)
                    {
                        // ignore
                        LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} exception", inner);
                    }

                    throw;
                }

                // Join the asynchronous test operation so any exceptions are rethrown on this thread.
                test.Join();
                LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} test joined");
            }
            finally
            {
                Application.Idle -= HandleApplicationIdle;
                LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} unsubscribed");

                // Safety net for the case the task "test" threw very early and didn't close the form.
                var form = Application.OpenForms.OfType<T>().SingleOrDefault();
                if (form != null)
                {
                    string formState = form.Visible ? "open" : "exists";
                    LoggingService.Log($"{MethodBase.GetCurrentMethod().Name} {typeof(T).FullName} still {formState}");

                    CloseOrDispose(form, $"{MethodBase.GetCurrentMethod().Name} {typeof(T).FullName}");
                }

                Assert.IsTrue(Application.OpenForms.Count == 0, $"{Application.OpenForms.Count} open form(s)");
            }

            return;

            void HandleApplicationIdle(object sender, EventArgs e)
            {
                LoggingService.Log($"{MethodBase.GetCurrentMethod().Name}");
                idleCompletionSource.TrySetResult(default);
            }
        }

        public static void RunDialog<T>(
            Action<Form> showDialog,
            Func<T, Task> runTestAsync)
            where T : Form
        {
            Form mainForm = null;
            try
            {
                RunForm<T>(
                    () =>
                    {
                        mainForm = new Form { Text = $"Test {typeof(T).Name}" };
                        mainForm.Shown += (s, e) =>
                        {
                            showDialog(mainForm);
                        };

                        if (Application.MessageLoop)
                        {
                            mainForm.Show(owner: null);
                        }
                        else
                        {
                            Application.Run(mainForm);
                        }
                    },
                    async (dialogForm) =>
                    {
                        try
                        {
                            await runTestAsync(dialogForm);
                        }
                        finally
                        {
                            CloseOrDispose(mainForm, $"{MethodBase.GetCurrentMethod().Name} main");
                            mainForm = null;
                        }
                    });
            }
            finally
            {
                CloseOrDispose(mainForm, $"{MethodBase.GetCurrentMethod().Name} finally main");
            }
        }

        private static void CloseOrDispose(Form form, string description)
        {
            try
            {
                ConfigureJoinableTaskFactoryAttribute.IgnoreExceptions = true;

                if (form == null)
                {
                    LoggingService.Log($"{description} form is null");
                    return;
                }

                var actionDescription = description + " form ";
                try
                {
                    if (form.Visible)
                    {
                        actionDescription += "close";
                        form.Close(); // also calls form.Dispose()
                    }
                    else
                    {
                        actionDescription += "dispose";
                        form.Dispose();
                    }

                    LoggingService.Log($"{actionDescription}d");
                }
                catch (Exception ex)
                {
                    LoggingService.Log($"{actionDescription} threw {ex.Demystify()}");
                    form.Dispose();
                    LoggingService.Log($"{description} form disposed");
                }
            }
            finally
            {
                // Wait for potential pending asynchronous tasks triggered by the form.
                WaitForPendingOperations();
                LoggingService.Log($"{description} form operations done");
            }
        }

        private static void WaitForPendingOperations()
        {
            // Workaround for tests hanging in conjunction with canceled operations:
            // Process the message loop before and after the wait.
            Application.DoEvents();
            AsyncTestHelper.WaitForPendingOperations(AsyncTestHelper.UnexpectedTimeout);
            Application.DoEvents();
        }

        private readonly struct VoidResult
        {
        }
    }
}
