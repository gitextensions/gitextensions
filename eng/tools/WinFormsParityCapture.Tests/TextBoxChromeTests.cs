using System.Drawing;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class TextBoxChromeTests
{
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Search_border_should_retain_native_Fixed3D_chrome_independently_of_its_background(bool focused)
    {
        Application.EnableVisualStyles();
        using Form window = new() { ClientSize = new Size(350, 100), ShowInTaskbar = false, AutoScaleMode = AutoScaleMode.None };
        using Font font = new("Segoe UI", 9);
        using TextBox search = new() { Font = font, Location = new Point(10, 10), Width = 300, BackColor = Color.FromArgb(32, 32, 32) };
        using Button other = new() { Location = new Point(10, 50), Text = "Other" };
        window.Controls.Add(search);
        window.Controls.Add(other);
        window.Show();
        window.Activate();
        (focused ? (Control)search : other).Focus().Should().BeTrue();

        // Native non-client EDIT painting is queued after activation/focus changes.
        // Pump those real messages before asking PrintWindow for the rendered chrome.
        long settledAt = Environment.TickCount64 + 100;
        while (Environment.TickCount64 < settledAt)
        {
            Application.DoEvents();
        }

        search.Focused.Should().Be(focused);
        search.Refresh();
        search.DeviceDpi.Should().Be(96);
        search.BorderStyle.Should().Be(BorderStyle.Fixed3D);
        search.Height.Should().Be(23);
        search.ClientSize.Should().Be(new Size(search.Width - 4, search.Height - 4));
        SendMessage(search.Handle, 0x00D4, 0, 0).Should().Be(0, "native EDIT adds no text margin inside the Fixed3D client inset");
        using CaptureImageResult capture = ImageCapture.Capture(window, [], []);
        Point origin = window.PointToScreen(search.Location) - new Size(capture.ScreenBounds.Location);
        Color top = capture.Bitmap.GetPixel(origin.X + (search.Width / 2), origin.Y);
        Color innerTop = capture.Bitmap.GetPixel(origin.X + (search.Width / 2), origin.Y + 1);
        Color bottom = capture.Bitmap.GetPixel(origin.X + (search.Width / 2), origin.Y + search.Height - 1);
        TestContext.Progress.WriteLine($"focused={focused} margins={SendMessage(search.Handle, 0x00D4, 0, 0):X} top={top.Name} inner={innerTop.Name} bottom={bottom.Name}");
        top.ToArgb().Should().Be(Color.FromArgb(236, 236, 236).ToArgb());
        bottom.ToArgb().Should().Be((focused ? Color.FromArgb(0, 103, 192) : Color.FromArgb(131, 131, 131)).ToArgb());
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint parameter, nint data);
}
