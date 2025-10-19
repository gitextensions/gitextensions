using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitExtUtils.GitUI;

public sealed class JoinableTaskScope : IDisposable
{
    private readonly bool _localScope;

    public JoinableTaskScope()
    {
        _localScope = ThreadHelper.JoinableTaskContext == null;

        if (_localScope)
        {
            ThreadHelper.JoinableTaskContext = new();
        }
    }

    public void Dispose()
    {
        if (_localScope)
        {
            JoinableTaskContext context = ThreadHelper.JoinableTaskContext;

            ThreadHelper.JoinableTaskContext = null;

            context?.Dispose();
        }
    }
}
