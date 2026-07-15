using Avalonia;
using Avalonia.Threading;
using GitExtUtils;

namespace GitUI;

/// <summary>
///  Twins of the WinForms-control threading extensions in GitExtUtils
///  (<c>ThreadHelper.InvokeAndForget</c>, <c>ControlThreadingExtensions.SwitchToMainThreadAsync</c>)
///  for Avalonia visuals, so ported code-behind compiles unchanged.
/// </summary>
public static class AvaloniaThreadingExtensions
{
    /// <summary>Asynchronously runs <paramref name="action"/> on the UI thread, forwarding exceptions like WinForms' InvokeAndForget.</summary>
    public static void InvokeAndForget(this Visual control, Action action, CancellationToken cancellationToken = default)
        => control.InvokeAndForget(
            () =>
            {
                action();
                return Task.CompletedTask;
            },
            cancellationToken);

    /// <summary>Asynchronously runs <paramref name="asyncAction"/> on the UI thread, forwarding exceptions like WinForms' InvokeAndForget.</summary>
    public static void InvokeAndForget(this Visual control, Func<Task> asyncAction, CancellationToken cancellationToken = default)
        => ThreadHelper.FileAndForget(async () =>
        {
            await control.SwitchToMainThreadAsync(cancellationToken);
            await asyncAction();
        });

    /// <summary>Switches to the UI thread, like the WinForms control extension.</summary>
    public static DispatcherPriorityAwaitable SwitchToMainThreadAsync(this Visual control, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return control.Dispatcher.Resume();
    }
}
