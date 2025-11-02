using System.Runtime.CompilerServices;

namespace GitExtUtils.Tasks;

internal static class TaskExtensions
{
    // VSTHRD002: Synchronously waiting on tasks could cause deadlocks.
    // Specifically, if they're associated with a synchronization
    // context, and that synchronization context's message pump is the
    // current thread, then it deadlocks because it can't pump messages
    // and synchronously wait at the same time. These extension methods
    // operate on ConfiguredTaskAwaitable specifically, the type
    // returned by .ConfigureAwait(false). They can't be used directly
    // on unconfigured tasks.
#pragma warning disable VSTHRD002
    public static void WaitDirect(this ConfiguredTaskAwaitable task)
        => task.GetAwaiter().GetResult();

    public static TResult GetResultDirect<TResult>(this ConfiguredTaskAwaitable<TResult> task)
        => task.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
}
