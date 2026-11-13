using System.Drawing;
using System.Reflection;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class PictureBoxSizingTests
{
    [Test]
    [TestCase(381, BorderStyle.None, 0, 185, 44, 9, 0, 167, 44)]
    [TestCase(385, BorderStyle.None, 0, 185, 44, 8, 0, 169, 44)]
    [TestCase(385, BorderStyle.None, 7, 185, 44, 8, 0, 169, 44)]
    [TestCase(385, BorderStyle.FixedSingle, 7, 183, 42, 11, 0, 161, 42)]
    [TestCase(385, BorderStyle.Fixed3D, 7, 181, 40, 13, 0, 154, 40)]
    public void Zoom_should_truncate_and_integer_center_the_client_rectangle_independently_of_padding(
        int imageWidth, BorderStyle border, int padding, int clientWidth, int clientHeight,
        int left, int top, int drawnWidth, int drawnHeight)
    {
        using Bitmap image = new(imageWidth, 100);
        using PictureBox picture = new()
        {
            Size = new Size(185, 44),
            Image = image,
            SizeMode = PictureBoxSizeMode.Zoom,
            Padding = new Padding(padding, 3, 9, 5),
            BorderStyle = border,
        };
        using Form window = new()
        {
            ClientSize = new Size(250, 120),
            AutoScaleMode = AutoScaleMode.None,
            ShowInTaskbar = false,
        };
        window.Controls.Add(picture);
        window.Show();
        Application.DoEvents();
        window.DeviceDpi.Should().Be(96, "this probes native 100% PictureBox behavior");
        picture.ClientSize.Should().Be(new Size(clientWidth, clientHeight));
        Rectangle destination = (Rectangle)(typeof(PictureBox)
            .GetProperty("ImageRectangle", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(picture) ?? throw new InvalidOperationException("The native image rectangle is unavailable."));
        destination.Should().Be(new Rectangle(left, top, drawnWidth, drawnHeight));
    }
}
