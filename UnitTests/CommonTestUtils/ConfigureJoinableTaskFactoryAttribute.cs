using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using GitUI;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CommonTestUtils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ConfigureJoinableTaskFactoryAttribute : Attribute, ITestAction
    {
        private DenyExecutionSynchronizationContext _denyExecutionSynchronizationContext;
        private HangReporter _hangReporter;
        private ExceptionDispatchInfo _threadException;

        public ActionTargets Targets => ActionTargets.Test;

        public ConfigureJoinableTaskFactoryAttribute()
        {
            Application.ThreadException += HandleApplicationThreadException;
        }

        public void BeforeTest(ITest test)
        {
            Assert.IsNull(ThreadHelper.JoinableTaskContext, "Tests with joinable tasks must not be run in parallel!");

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

            // This form is created to obtain a UI synchronization context only.
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
                _hangReporter = new HangReporter(ThreadHelper.JoinableTaskContext);
            }
        }

        public void AfterTest(ITest test)
        {
            try
            {
                try
                {
                    // Wait for eventual pending operations triggered by the test.
                    using var cts = new CancellationTokenSource(AsyncTestHelper.UnexpectedTimeout);
                    try
                    {
                        // Note that ThreadHelper.JoinableTaskContext.Factory must be used to bypass the default behavior of
                        // ThreadHelper.JoinableTaskFactory since the latter adds new tasks to the collection and would therefore
                        // never complete.
                        ThreadHelper.JoinableTaskContext.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync(cts.Token));
                    }
                    catch (OperationCanceledException) when (cts.IsCancellationRequested)
                    {
                        if (int.TryParse(Environment.GetEnvironmentVariable("GE_TEST_SLEEP_SECONDS_ON_HANG"), out var sleepSeconds) && sleepSeconds > 0)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(sleepSeconds));
                        }

                        throw;
                    }
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
                StoreThreadException(ex);
            }
            finally
            {
                // Reset _threadException to null, and throw if it was set during the current test.
                Interlocked.Exchange(ref _threadException, null)?.Throw();
            }
        }

        private void HandleApplicationThreadException(object sender, ThreadExceptionEventArgs e)
            => StoreThreadException(e.Exception);

        private void StoreThreadException(Exception ex)
        {
            if (_threadException != null)
            {
                ex = new AggregateException(new Exception[] { _threadException.SourceException, ex });
            }

            _threadException = ExceptionDispatchInfo.Capture(ex);
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

        private sealed class HangReporter : JoinableTaskContextNode
        {
            public HangReporter(JoinableTaskContext context)
                : base(context)
            {
                RegisterOnHangDetected();
            }

            protected override void OnHangDetected(TimeSpan hangDuration, int notificationCount, Guid hangId)
            {
                if (notificationCount > 1)
                {
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Environment.NewLine}HANG DETECTED: guid {hangId}{Environment.NewLine}");
                Console.ResetColor();

                if (Environment.GetEnvironmentVariable("GE_TEST_LAUNCH_DEBUGGER_ON_HANG") != "1")
                {
                    return;
                }

                Console.WriteLine("launching debugger...");

                Debugger.Launch();
                Debugger.Break();
            }
        }
    }
}
