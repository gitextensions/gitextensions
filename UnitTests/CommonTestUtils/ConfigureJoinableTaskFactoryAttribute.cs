using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ResourceManager;

namespace CommonTestUtils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ConfigureJoinableTaskFactoryAttribute : Attribute, ITestAction
    {
        private readonly bool _checkForUpdatesOriginalValue = AppSettings.CheckForUpdates;

        private DenyExecutionSynchronizationContext _denyExecutionSynchronizationContext;
        private ExceptionDispatchInfo _threadException;

        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test)
        {
            AppSettings.CheckForUpdates = false;

            // Ignore instances from previous failed tests.
            GitExtensionsFormBase.InstanceRegistry.Clear();

            Assert.IsNull(ThreadHelper.JoinableTaskContext, "Tests with joinable tasks must not be run in parallel!");

            Application.ThreadException += HandleApplicationThreadException;

            IList apartmentState = null;
            for (var scope = test; scope != null; scope = scope.Parent)
            {
                apartmentState = scope.Properties[nameof(ApartmentState)];
                if (apartmentState.Count > 0)
                {
                    break;
                }
            }

            if (!apartmentState.Contains(ApartmentState.STA))
            {
                _denyExecutionSynchronizationContext = new DenyExecutionSynchronizationContext(SynchronizationContext.Current);
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext(_denyExecutionSynchronizationContext.MainThread, _denyExecutionSynchronizationContext);
                return;
            }

            Assert.AreEqual(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());

            // This form is created to obtain an UI synchronization context only.
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
            }
        }

        public void AfterTest(ITest test)
        {
            try
            {
                try
                {
                    Console.WriteLine($"{nameof(AfterTest)} entry");

                    // Wait for eventual pending operations triggered by the test.
                    if (ThreadHelper.JoinableTaskContext == null)
                    {
                        throw new InvalidOperationException("A JoinableTaskContext must have been created by BeforeTest.");
                    }

                    using (var cts = new CancellationTokenSource(AsyncTestHelper.UnexpectedTimeout))
                    {
                        try
                        {
                            ThreadHelper.JoinableTaskContext.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync(cts.Token));
                            Console.WriteLine($"{nameof(AfterTest)} pending operations finished");
                        }
                        catch (OperationCanceledException ex)
                        {
                            var ctsCancelled = cts.IsCancellationRequested ? string.Empty : "not ";
                            Console.WriteLine($"{nameof(AfterTest)} cts {ctsCancelled}cancelled, ex {ex.Demystify()}");
                        }
                    }

                    Assert.IsTrue(GitExtensionsFormBase.InstanceRegistry.Count == 0,
                        $"{GitExtensionsFormBase.InstanceRegistry.Count} undisposed form(s):{Environment.NewLine}"
                        + $"{GitExtensionsFormBase.InstanceRegistry}");
                }
                finally
                {
                    ThreadHelper.JoinableTaskContext = null;
                    if (_denyExecutionSynchronizationContext != null)
                    {
                        SynchronizationContext.SetSynchronizationContext(_denyExecutionSynchronizationContext.UnderlyingContext);
                    }
                }

                _denyExecutionSynchronizationContext?.ThrowIfSwitchOccurred();
            }
            catch (Exception ex) when (_threadException != null)
            {
                // ignore the follow-up exception
                Console.WriteLine($"{nameof(AfterTest)} ignored {ex.Demystify()}");
            }
            finally
            {
                AppSettings.CheckForUpdates = _checkForUpdatesOriginalValue;
                Application.ThreadException -= HandleApplicationThreadException;
                string threadException = _threadException?.SourceException.Demystify().ToString() ?? "none";
                Console.WriteLine($"{nameof(AfterTest)} rethrow {threadException}");
                _threadException?.Throw();
            }

            Console.WriteLine($"{nameof(AfterTest)} exit");
        }

        private void HandleApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            bool ignore = e.Exception.GetType() == typeof(ThreadAbortException);
            string ignoring = ignore ? "ignoring " : string.Empty;
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} {ignoring}{e.Exception.Demystify()}");
            if (!ignore && _threadException == null)
            {
                _threadException = ExceptionDispatchInfo.Capture(e.Exception);
            }
        }

        private class DenyExecutionSynchronizationContext : SynchronizationContext
        {
            private readonly SynchronizationContext _underlyingContext;
            private readonly Thread _mainThread;
            private readonly StrongBox<ExceptionDispatchInfo> _failedTransfer;

            public DenyExecutionSynchronizationContext(SynchronizationContext underlyingContext)
                : this(underlyingContext, mainThread: null, failedTransfer: null)
            {
            }

            private DenyExecutionSynchronizationContext(SynchronizationContext underlyingContext, Thread mainThread, StrongBox<ExceptionDispatchInfo> failedTransfer)
            {
                _underlyingContext = underlyingContext;
                _mainThread = mainThread ?? new Thread(MainThreadStart);
                _failedTransfer = failedTransfer ?? new StrongBox<ExceptionDispatchInfo>();
            }

            internal SynchronizationContext UnderlyingContext => _underlyingContext;

            internal Thread MainThread => _mainThread;

            private static void MainThreadStart() => throw new InvalidOperationException("This thread should never be started.");

            internal void ThrowIfSwitchOccurred()
            {
                if (_failedTransfer.Value == null)
                {
                    return;
                }

                _failedTransfer.Value.Throw();
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                try
                {
                    if (_failedTransfer.Value == null)
                    {
                        ThrowFailedTransferExceptionForCapture();
                    }
                }
                catch (InvalidOperationException e)
                {
                    _failedTransfer.Value = ExceptionDispatchInfo.Capture(e);
                }

#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                (_underlyingContext ?? new SynchronizationContext()).Post(d, state);
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                try
                {
                    if (_failedTransfer.Value == null)
                    {
                        ThrowFailedTransferExceptionForCapture();
                    }
                }
                catch (InvalidOperationException e)
                {
                    _failedTransfer.Value = ExceptionDispatchInfo.Capture(e);
                }

#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                (_underlyingContext ?? new SynchronizationContext()).Send(d, state);
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }

            public override SynchronizationContext CreateCopy()
            {
                return new DenyExecutionSynchronizationContext(_underlyingContext.CreateCopy(), _mainThread, _failedTransfer);
            }

            private static void ThrowFailedTransferExceptionForCapture()
            {
                throw new InvalidOperationException("Tests cannot use SwitchToMainThreadAsync unless they are marked with ApartmentState.STA.");
            }
        }
    }
}
