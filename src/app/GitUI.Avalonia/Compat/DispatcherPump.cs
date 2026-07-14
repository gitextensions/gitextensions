using Avalonia.Threading;

namespace GitUI.Compat;

/// <summary>
///  Runs an async UI operation to completion with WinForms-like blocking semantics.
///  On the UI thread a nested dispatcher frame keeps the UI responsive (the WinForms
///  <c>ShowDialog</c> model); on a background thread the call simply blocks.
/// </summary>
public static class DispatcherPump
{
    public static T Wait<T>(Func<Task<T>> asyncFunc)
    {
#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks
        // Deliberate WinForms-compat blocking: on the UI thread the nested dispatcher frame
        // below keeps pumping (no deadlock); on a background thread the UI thread stays free.
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return Dispatcher.UIThread.InvokeAsync(asyncFunc).GetAwaiter().GetResult();
        }

        Task<T> task = asyncFunc();
        if (!task.IsCompleted)
        {
            DispatcherFrame frame = new();
            task.ContinueWith(_ => frame.Continue = false, TaskScheduler.Default);
            Dispatcher.UIThread.PushFrame(frame);
        }

        return task.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }
}
