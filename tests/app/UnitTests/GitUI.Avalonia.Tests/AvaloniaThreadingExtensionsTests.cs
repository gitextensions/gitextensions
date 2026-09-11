using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AvaloniaThreadingExtensionsTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void InvokeAndForget_should_run_synchronous_UI_work_before_returning()
    {
        Control control = new();
        List<string> order = [];

        order.Add("before");
        control.InvokeAndForget(() => order.Add("action"));
        order.Add("after");

        order.Should().Equal("before", "action", "after");
    }

    [AvaloniaTest]
    public void InvokeAndForget_should_ignore_pre_cancelled_work()
    {
        Control control = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        bool invoked = false;

        control.InvokeAndForget(() => invoked = true, cancellation.Token);

        invoked.Should().BeFalse();
    }
}
