using System.Reflection;
using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using NSubstitute;

namespace GitUITests.CommandsDialogs.BrowseDialog;

[Apartment(ApartmentState.STA)]
public sealed class MultiRepositoryStatusControlTests
{
    [Test]
    public async Task RefreshContent_marshals_control_updates_to_UI_thread()
    {
        bool previousCheckForIllegalCrossThreadCalls = Control.CheckForIllegalCrossThreadCalls;
        Control.CheckForIllegalCrossThreadCalls = true;
        Exception? threadException = null;
        ThreadExceptionEventHandler handler = (_, args) => threadException = args.Exception;
        Application.ThreadException += handler;

        try
        {
            using Form form = new();
            using MultiRepositoryStatusControl control = new();
            form.Controls.Add(control);
            _ = form.Handle;
            _ = control.Handle;
            _ = GetField<Label>(control, "_operationLabel").Handle;

            SetField(control, "_initialized", true);
            SetField(control, "_statusProvider", Substitute.For<IMultiRepositoryStatusProvider>());

            control.RefreshContent();
            await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

            threadException.Should().BeNull();
        }
        finally
        {
            Application.ThreadException -= handler;
            Control.CheckForIllegalCrossThreadCalls = previousCheckForIllegalCrossThreadCalls;
        }
    }

    private static void SetField<T>(MultiRepositoryStatusControl control, string name, T value)
    {
        FieldInfo? field = typeof(MultiRepositoryStatusControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        field!.SetValue(control, value);
    }

    private static T GetField<T>(MultiRepositoryStatusControl control, string name)
    {
        FieldInfo? field = typeof(MultiRepositoryStatusControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        return field!.GetValue(control).Should().BeOfType<T>().Subject;
    }
}
