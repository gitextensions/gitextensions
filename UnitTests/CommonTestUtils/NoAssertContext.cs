// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace System
{
    /// <summary>
    ///  Use (within a using) to eat asserts.
    /// </summary>
    public sealed class NoAssertContext : IDisposable
    {
        // For any given thread we don't need to lock to decide how to route messages, as any messages for that
        // given thread will not happen while we're in the constructor or dispose method on that thread. That
        // means we can safely check to see if we've hooked our thread without locking (outside of using a
        // concurrent collection to make sure the collection is in a known state).
        //
        // We do, however need to lock around hooking/unhooking our custom listener to make sure that we
        // are rerouting correctly if multiple threads are creating/disposing this class concurrently.

#pragma warning disable SA1308 // Variable names should not be prefixed
        private static readonly object s_lock = new object();
        private static bool s_hooked;

        private static readonly ConcurrentDictionary<int, int> s_suppressedThreads = new ConcurrentDictionary<int, int>();

        // "Default" is the listener that terminates the process when debug assertions fail.
        private static readonly TraceListener s_defaultListener = Trace.Listeners["Default"];
        private static readonly NoAssertListener s_noAssertListener = new NoAssertListener();
#pragma warning restore SA1308 // Variable names should not be prefixed

        public NoAssertContext()
        {
            s_suppressedThreads.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, 1, (key, oldValue) => oldValue + 1);

            // Lock to make sure we are hooked properly if two threads come into the constructor/dispose at the same time.
            lock (s_lock)
            {
                if (!s_hooked)
                {
                    // Hook our custom listener first so we don't lose assertions from other threads when
                    // we disconnect the default listener.
                    Trace.Listeners.Add(s_noAssertListener);
                    Trace.Listeners.Remove(s_defaultListener);
                    s_hooked = true;
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            int currentThread = Thread.CurrentThread.ManagedThreadId;
            if (s_suppressedThreads.TryRemove(currentThread, out int count))
            {
                if (count > 1)
                {
                    // We're in a nested assert context on a given thread, re-add with a decremented count.
                    // This doesn't need to be atomic as we're currently on the thread that would care about
                    // being rerouted.
                    s_suppressedThreads.TryAdd(currentThread, --count);
                }
            }

            lock (s_lock)
            {
                if (s_hooked && s_suppressedThreads.Count == 0)
                {
                    // We're the first to hit the need to unhook. Add the default listener back first to
                    // ensure we don't lose any asserts from other threads.
                    Trace.Listeners.Add(s_defaultListener);
                    Trace.Listeners.Remove(s_noAssertListener);
                    s_hooked = false;
                }
            }
        }

        ~NoAssertContext()
        {
            // We need this class to be used in a using to effectively rationalize about a test.
            throw new InvalidOperationException($"Did not dispose {nameof(NoAssertContext)}");
        }

        private class NoAssertListener : TraceListener
        {
            public NoAssertListener()
                : base(typeof(NoAssertListener).FullName)
            {
            }

            public override void Fail(string message)
            {
                if (!s_suppressedThreads.TryGetValue(Thread.CurrentThread.ManagedThreadId, out _))
                {
                    s_defaultListener.Fail(message);
                }
            }

            public override void Fail(string message, string detailMessage)
            {
                if (!s_suppressedThreads.TryGetValue(Thread.CurrentThread.ManagedThreadId, out _))
                {
                    s_defaultListener.Fail(message, detailMessage);
                }
            }

            // Write and WriteLine are virtual

            public override void Write(string message)
            {
                if (!s_suppressedThreads.TryGetValue(Thread.CurrentThread.ManagedThreadId, out _))
                {
                    s_defaultListener.Write(message);
                }
            }

            public override void WriteLine(string message)
            {
                if (!s_suppressedThreads.TryGetValue(Thread.CurrentThread.ManagedThreadId, out _))
                {
                    s_defaultListener.WriteLine(message);
                }
            }
        }
    }
}