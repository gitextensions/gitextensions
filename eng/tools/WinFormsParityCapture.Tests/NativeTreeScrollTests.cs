using System.Drawing;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using GitUI.UserControls;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeTreeScrollTests
{
    [Test]
    [TestCase(9)]
    [TestCase(11)]
    public void Native_tree_horizontal_wheel_should_follow_native_line_and_page_scroll_units(float points)
    {
        using Font font = new("Segoe UI", points);
        using Form window = new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(240, 140), ShowInTaskbar = false };
        using NativeTreeView tree = new() { Dock = DockStyle.Fill, Font = font, BorderStyle = BorderStyle.None };
        tree.Nodes.Add(new string('W', 200));
        window.Controls.Add(tree);
        window.Show();
        Application.DoEvents();
        tree.DeviceDpi.Should().Be(96);
        ScrollInformation initial = ReadScroll(tree);
        initial.Maximum.Should().BeGreaterThan((int)initial.Page);

        SendMessage(tree.Handle, 0x114, 1, 0); // WM_HSCROLL / SB_LINERIGHT.
        int line = ReadScroll(tree).Position;
        line.Should().BeGreaterThan(0);
        SendMessage(tree.Handle, 0x114, 6, 0); // SB_LEFT.
        SendMessage(tree.Handle, 0x114, 3, 0); // SB_PAGERIGHT.
        int page = ReadScroll(tree).Position;
        page.Should().BeGreaterThan(line);

        int scrollLines = SystemInformation.MouseWheelScrollLines;
        foreach (int delta in new[] { 120, 40, 0 })
        {
            SendMessage(tree.Handle, 0x114, 6, 0);
            SendMessage(tree.Handle, 0x20E, unchecked((nint)(delta << 16)), 0); // WM_MOUSEHWHEEL.
            int horizontal = ReadScroll(tree).Position;
            int expected = delta == 0 ? 0
                : scrollLines == -1 ? page
                : Math.Max(1, (Math.Abs(delta) * scrollLines * 3) / 120) * line;
            horizontal.Should().Be(expected);
            SendMessage(tree.Handle, 0x114, 6, 0);
            SendMessage(tree.Handle, 0x20A, unchecked((nint)((-delta << 16) | 4)), 0); // Shift+WM_MOUSEWHEEL.
            ReadScroll(tree).Position.Should().Be(horizontal);
        }

        TestContext.Out.WriteLine($"inputMode=windowMessage dpiMode=nativeMonitor deviceDpi={tree.DeviceDpi} fontPoints={points} nativeLinePixels={line} nativePagePixels={page} mouseWheelScrollLines={scrollLines} scrollRange={initial.Maximum} scrollPage={initial.Page}");
    }

    private static ScrollInformation ReadScroll(NativeTreeView tree)
    {
        ScrollInformation information = new() { Size = (uint)Marshal.SizeOf<ScrollInformation>(), Mask = 0x17 };
        GetScrollInfo(tree.Handle, 0, ref information).Should().BeTrue();
        return information;
    }

    [DllImport("user32.dll")]
    private static extern bool GetScrollInfo(nint window, int bar, ref ScrollInformation information);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint parameter, nint data);

    [StructLayout(LayoutKind.Sequential)]
    private struct ScrollInformation
    {
        public uint Size;
        public uint Mask;
        public int Minimum;
        public int Maximum;
        public uint Page;
        public int Position;
        public int TrackPosition;
    }
}
