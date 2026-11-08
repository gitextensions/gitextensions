using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeApplicationChooserTests
{
    private Func<nint, string, uint, int> _previousWindows = null!;
    private Func<ProcessStartInfo, Task<bool>> _previousStart = null!;
    private Func<TopLevel, string?> _previousPicker = null!;
    private string _directory = null!;
    private readonly List<Window> _owners = [];

    [SetUp]
    public void SetUp()
    {
        _previousWindows = NativeApplicationChooser.TestAccessor.ShowWindowsDialog;
        _previousStart = NativeApplicationChooser.TestAccessor.StartApplication;
        _previousPicker = MacApplicationPicker.TestAccessor.ShowPanel;
        _directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ApplicationChooser-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        NativeApplicationChooser.TestAccessor.ShowWindowsDialog = _previousWindows;
        NativeApplicationChooser.TestAccessor.StartApplication = _previousStart;
        MacApplicationPicker.TestAccessor.ShowPanel = _previousPicker;
        foreach (Window owner in _owners)
        {
            owner.Close();
        }

        _owners.Clear();
        TestDirectory.Delete(_directory);
    }

    private Window CreateOwner()
    {
        Window owner = new();
        _owners.Add(owner);
        return owner;
    }

    [AvaloniaTest]
    [TestCase(0, true)]
    [TestCase(1, true)]
    [TestCase(unchecked((int)0x800704C7), true)]
    [TestCase(unchecked((int)0x80004004), true)]
    [TestCase(unchecked((int)0x80070005), false)]
    public void Windows_should_request_the_system_chooser_with_an_unquoted_file(int result, bool handled)
    {
        Window owner = CreateOwner();
        string target = Path.Combine(_directory, "a file with spaces ž.txt");
        int requests = 0;
        NativeApplicationChooser.TestAccessor.ShowWindowsDialog = (parent, file, flags) =>
        {
            parent.Should().Be(owner.TryGetPlatformHandle()?.Handle ?? 0);
            file.Should().Be(target);
            flags.Should().Be(4);
            requests++;
            return result;
        };

        NativeApplicationChooser.OpenOnWindows(owner, target).Should().Be(handled);

        requests.Should().Be(1);
    }

    [AvaloniaTest]
    public void Windows_should_report_an_unavailable_native_dialog()
    {
        Window owner = CreateOwner();
        NativeApplicationChooser.TestAccessor.ShowWindowsDialog = (_, _, _) => throw new System.ComponentModel.Win32Exception(5);

        NativeApplicationChooser.OpenOnWindows(owner, Path.Combine(_directory, "file.txt")).Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase(true)]
    [TestCase(false)]
    public async Task MacOS_should_open_the_file_with_the_chosen_application_bundle(bool launchAccepted)
    {
        Window owner = CreateOwner();
        string application = Path.Combine(_directory, "Chosen Application.app");
        Directory.CreateDirectory(application);
        string target = Path.Combine(_directory, "a file with spaces ž.txt");
        int picks = 0;
        int launches = 0;
        MacApplicationPicker.TestAccessor.ShowPanel = window =>
        {
            window.Should().BeSameAs(owner);
            picks++;
            return application;
        };
        NativeApplicationChooser.TestAccessor.StartApplication = info =>
        {
            info.FileName.Should().Be("/usr/bin/open");
            info.UseShellExecute.Should().BeFalse();
            info.ArgumentList.Should().Equal("-a", application, target);
            launches++;
            return Task.FromResult(launchAccepted);
        };

        bool handled = await NativeApplicationChooser.OpenOnMacOSAsync(owner, target);

        handled.Should().Be(launchAccepted);
        picks.Should().Be(1);
        launches.Should().Be(1);
    }

    [AvaloniaTest]
    public async Task MacOS_cancel_should_not_launch_an_application_or_report_an_error()
    {
        Window owner = CreateOwner();
        MacApplicationPicker.TestAccessor.ShowPanel = _ => null;
        NativeApplicationChooser.TestAccessor.StartApplication = _ => throw new AssertionException("Cancellation must not start a process.");

        bool handled = await NativeApplicationChooser.OpenOnMacOSAsync(owner, Path.Combine(_directory, "file.txt"));

        handled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task MacOS_should_reject_a_selection_that_is_not_an_application_bundle()
    {
        Window owner = CreateOwner();
        MacApplicationPicker.TestAccessor.ShowPanel = _ => _directory;
        NativeApplicationChooser.TestAccessor.StartApplication = _ => throw new AssertionException("An invalid selection must not start a process.");

        bool handled = await NativeApplicationChooser.OpenOnMacOSAsync(owner, Path.Combine(_directory, "file.txt"));

        handled.Should().BeFalse();
    }

    [AvaloniaTest]
    public async Task MacOS_should_report_an_unavailable_picker()
    {
        Window owner = CreateOwner();
        MacApplicationPicker.TestAccessor.ShowPanel = _ => throw new IOException("Picker unavailable");

        bool handled = await NativeApplicationChooser.OpenOnMacOSAsync(owner, Path.Combine(_directory, "file.txt"));

        handled.Should().BeFalse();
    }
}
