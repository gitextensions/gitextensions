using Avalonia.Threading;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

internal sealed class JoinableTaskContextTestScope : IDisposable
{
    private readonly JoinableTaskContext? _previous;
    private readonly JoinableTaskContext _context;

    public JoinableTaskContextTestScope()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previous = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
    }

    public void Dispose()
    {
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.RunJobs();
            }
        }
        finally
        {
            ThreadHelper.JoinableTaskContext = _previous!;
            _context.Dispose();
        }
    }
}
