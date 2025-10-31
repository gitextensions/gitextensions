using System.Runtime.CompilerServices;

namespace GitExtUtils.Tasks
{
    internal static class TaskExtensions
    {
#pragma warning disable VSTHRD002, VSTHRD003, VSTHRD200
        public static ConfiguredTaskAwaitable DetachFromSynchronizationContext(this Task task)
            => task.ConfigureAwait(continueOnCapturedContext: false);

        public static ConfiguredTaskAwaitable<TResult> DetachFromSynchronizationContext<TResult>(this Task<TResult> task)
            => task.ConfigureAwait(continueOnCapturedContext: false);

        public static void WaitDirect(this ConfiguredTaskAwaitable task)
            => task.GetAwaiter().GetResult();

        public static TResult GetResultDirect<TResult>(this ConfiguredTaskAwaitable<TResult> task)
            => task.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002, VSTHRD003, VSTHRD200
    }
}
