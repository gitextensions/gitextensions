using GitUI;
using Microsoft.VisualStudio.Threading;

namespace CommonTestUtils;

public readonly ref struct JoinableTaskScope : IDisposable
{
    private readonly bool _localScope;

    public JoinableTaskScope()
    {
        _localScope = ThreadHelper.JoinableTaskContext is null;

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
