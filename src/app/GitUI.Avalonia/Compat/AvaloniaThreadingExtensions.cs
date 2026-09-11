using Avalonia;
using Avalonia.Threading;
using GitExtUtils;

namespace GitUI;

/// <summary>
///  Provides the WinForms-control threading extension surface from GitExtUtils
///  (<c>ThreadHelper.InvokeAndForget</c>, <c>ControlThreadingExtensions.SwitchToMainThreadAsync</c>)
///  for Avalonia visuals, so ported code-behind retains the same call sites.
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
        => _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            try
            {
                if (!control.Dispatcher.CheckAccess())
                {
                    await control.SwitchToMainThreadAsync(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                await asyncAction();
            }
            catch (OperationCanceledException)
            {
                // Match TaskManager.InvokeAndForget: cancellation is an expected terminal state.
            }
            catch (Exception ex)
            {
                if (!control.Dispatcher.CheckAccess())
                {
                    await control.SwitchToMainThreadAsync();
                }

                GitExtensions.Shims.WinForms.Application.OnThreadException(ex);
            }
        });

    /// <summary>Switches to the UI thread, like the WinForms control extension.</summary>
    public static DispatcherPriorityAwaitable SwitchToMainThreadAsync(this Visual control, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return control.Dispatcher.Resume();
    }
}
