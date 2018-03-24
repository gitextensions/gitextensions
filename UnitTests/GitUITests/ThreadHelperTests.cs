using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public sealed class ThreadHelperTests
    {
        private static readonly Action EmptyAction = () => { };

        [Test]
        public void FileAndForgetReportsThreadException()
        {
            using (var helper = new ThreadExceptionHelper())
            using (var form = new Form())
            {
                var ex = new Exception();

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await TaskScheduler.Default;
                    form.InvokeAsync(() => throw ex).FileAndForget();
                });

                JoinPendingOperations();
                Assert.AreSame(ex, helper.Exception);
            }
        }

        [Test]
        public void FileAndForgetIgnoresCancellationExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var form = new Form();
                form.Dispose();

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await TaskScheduler.Default;
                    form.InvokeAsync(EmptyAction).FileAndForget();
                });

                JoinPendingOperations();
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        [Test]
        public void FileAndForgetFilterCanAllowExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            using (var form = new Form())
            {
                var ex = new Exception();

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await TaskScheduler.Default;
                    form.InvokeAsync(() => throw ex).FileAndForget(fileOnlyIf: e => e == ex);
                });

                JoinPendingOperations();
                Assert.AreSame(ex, helper.Exception);
            }
        }

        [Test]
        public void FileAndForgetFilterCanIgnoreExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            using (var form = new Form())
            {
                var ex = new Exception();

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await TaskScheduler.Default;
                    form.InvokeAsync(() => throw ex).FileAndForget(fileOnlyIf: e => e != ex);
                });

                JoinPendingOperations();
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        [Test]
        public void FileAndForgetFilterIgnoresCancellationExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var form = new Form();
                form.Dispose();

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await TaskScheduler.Default;
                    form.InvokeAsync(EmptyAction).FileAndForget(fileOnlyIf: ex => true);
                });

                JoinPendingOperations();
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        private static void JoinPendingOperations()
        {
            // Since we are testing a FileAndForget method, we need to join all pending operations before continuing.
            // Note that ThreadHelper.JoinableTaskContext.Factory must be used to bypass the default behavior of
            // ThreadHelper.JoinableTaskFactory since the latter adds new tasks to the collection and would therefore
            // never complete.
            ThreadHelper.JoinableTaskContext?.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync());
        }

        private sealed class ThreadExceptionHelper : IDisposable
        {
            public ThreadExceptionHelper()
            {
                Application.ThreadException += HandleThreadException;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            public string Message => Exception?.Message;

            public void Dispose()
            {
                Application.ThreadException -= HandleThreadException;
            }

            private void HandleThreadException(object sender, ThreadExceptionEventArgs e)
            {
                Exception = e.Exception;
            }
        }
    }
}
