using System.Drawing.Imaging;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
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

        // Show/focus queues native client and popup painting. Settle those real
        // messages before another window covers the owned surfaces; PrintWindow
        // must render them independently of the later occluding desktop pixels.
        long paintingStartedAt = Environment.TickCount64;
        long paintingSettledAt = paintingStartedAt + 100;
        while (Environment.TickCount64 < paintingSettledAt)
        {
            Application.DoEvents();
        }

        form.Refresh();
        menu.Refresh();
        form.Update();
        menu.Update();
        long paintingSettlementMilliseconds = Environment.TickCount64 - paintingStartedAt;
        using Form occluder = CreateOccluder(form);
        occluder.Show();
        Application.DoEvents();
        menu.Visible.Should().BeTrue();
        form.DeviceDpi.Should().Be(96);

        using CaptureImageResult result = ImageCapture.Capture(form, [menu], []);
        Point sample = form.PointToScreen(new Point(20, 80));
        Point popupSample = new(menu.Left + 8, menu.Top + 8);
        Color client = result.Bitmap.GetPixel(sample.X - result.ScreenBounds.X, sample.Y - result.ScreenBounds.Y);
        Color popup = result.Bitmap.GetPixel(popupSample.X - result.ScreenBounds.X, popupSample.Y - result.ScreenBounds.Y);
        string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ImageCaptureProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        result.Bitmap.Save(Path.Combine(directory, "occluded-owned-popup.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, "metadata.json"), JsonSerializer.Serialize(new
        {
            inputMode = "ownedPopupOccludedByUnrelatedWindow",
            dpiMode = "nativeMonitor",
            deviceDpi = form.DeviceDpi,
            captureMethod = result.Method.ToString(),
            paintingSettlementMilliseconds,
            formVisible = form.Visible,
            popupVisible = menu.Visible,
            occluderVisible = occluder.Visible,
            occluder.TopMost,
            client = $"#{client.ToArgb():X8}",
            expectedClient = $"#{Color.CornflowerBlue.ToArgb():X8}",
            popup = $"#{popup.ToArgb():X8}",
            occluder = $"#{occluder.BackColor.ToArgb():X8}",
            result.ScreenBounds,
            popupBounds = menu.Bounds,
            occluderBounds = occluder.Bounds
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.Out.WriteLine($"diagnostics={directory} captureMethod={result.Method} deviceDpi={form.DeviceDpi} paintingSettlementMilliseconds={paintingSettlementMilliseconds} client=#{client.ToArgb():X8} popup=#{popup.ToArgb():X8} occluder=#{occluder.BackColor.ToArgb():X8} popupVisible={menu.Visible} occluderVisible={occluder.Visible}");
        result.Method.Should().Be(CaptureMethod.PrintWindow);
        client.ToArgb().Should().Be(Color.CornflowerBlue.ToArgb());
        popup.ToArgb().Should().NotBe(Color.Magenta.ToArgb());
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
