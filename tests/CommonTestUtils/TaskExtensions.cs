using System.Runtime.CompilerServices;

namespace GitExtUtils.Tasks;

internal static class TaskExtensions
{
    // VSTHRD003: Avoid awaiting foreign Tasks. But, this extension method isn't
    // really its own independent context, so these Tasks aren't foreign. This is
    // just a tool for the callers to use to show intent.
#pragma warning disable VSTHRD003
    public static ConfiguredTaskAwaitable DetachFromSynchronizationContext(this Task task)
        => task.ConfigureAwait(continueOnCapturedContext: false);

    public static ConfiguredTaskAwaitable<TResult> DetachFromSynchronizationContext<TResult>(this Task<TResult> task)
        => task.ConfigureAwait(continueOnCapturedContext: false);
#pragma warning restore VSTHRD003

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
