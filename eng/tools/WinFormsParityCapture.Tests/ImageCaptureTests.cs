using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class ImageCaptureTests
{
    [Test]
    public void Popup_capture_should_render_owned_windows_instead_of_an_occluding_desktop_window()
    {
        using Form form = new() { ClientSize = new Size(240, 180), BackColor = Color.CornflowerBlue };
        using ContextMenuStrip menu = new() { AutoClose = false };
        menu.Items.Add("Captured action");
        form.Show();
        Application.DoEvents();
        form.Refresh();
        menu.Show(form, new Point(180, 120));
        using Form occluder = CreateOccluder(form);
        occluder.Show();
        Application.DoEvents();
        menu.Visible.Should().BeTrue();

        using CaptureImageResult result = ImageCapture.Capture(form, [menu], []);
        result.Method.Should().Be(CaptureMethod.PrintWindow);
        Point sample = form.PointToScreen(new Point(20, 80));
        result.Bitmap.GetPixel(sample.X - result.ScreenBounds.X, sample.Y - result.ScreenBounds.Y)
            .ToArgb().Should().Be(Color.CornflowerBlue.ToArgb());
        Point popupSample = new(menu.Left + 8, menu.Top + 8);
        result.Bitmap.GetPixel(popupSample.X - result.ScreenBounds.X, popupSample.Y - result.ScreenBounds.Y)
            .ToArgb().Should().NotBe(Color.Magenta.ToArgb());
        result.ScreenBounds.Contains(menu.Bounds).Should().BeTrue();
    }

    [Test]
    public void Native_browser_capture_should_reject_an_occluded_screen_surface()
    {
        using Form form = new() { ClientSize = new Size(240, 180) };
        using WebBrowser browser = new() { Dock = DockStyle.Fill };
        form.Controls.Add(browser);
        form.Show();
        using Form occluder = CreateOccluder(form);
        occluder.Show();
        Application.DoEvents();

        ImageCapture.RequiresScreenGrab(form).Should().BeTrue();
        Action capture = () => ImageCapture.Capture(form, [], []);
        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*unrelated window occludes*");
    }

    private static Form CreateOccluder(Form form) => new()
    {
        StartPosition = FormStartPosition.Manual,
        Location = form.Location,
        Size = new Size(form.Width + 300, form.Height + 300),
        FormBorderStyle = FormBorderStyle.None,
        BackColor = Color.Magenta,
        TopMost = true,
        ShowInTaskbar = false
    };
}
