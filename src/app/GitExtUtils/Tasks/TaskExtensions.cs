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
}
