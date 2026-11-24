using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitUITests.CommandsDialogs;

/// <summary>
/// Supplies the same joinable-task context that GitUI.Tests installs assembly-wide to the
/// framework-neutral tests linked into the Avalonia assembly.
/// </summary>
[SetUpFixture]
[NonParallelizable]
public sealed class SharedGitUITestsSetup
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }
}
