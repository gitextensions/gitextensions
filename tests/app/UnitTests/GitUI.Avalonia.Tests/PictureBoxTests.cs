using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitUI.Compat.WinFormsControls;

namespace GitExtensionsTests;

[TestFixture]
public sealed class PictureBoxTests
{
    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(185, 44, 400, 100, 4, 0, 176, 44)]
    [TestCase(185, 44, 403, 100, 4, 0, 177, 44)]
    [TestCase(44, 185, 100, 403, 0, 4, 44, 177)]
    [TestCase(17, 14, 13, 11, 0, 0, 16, 14)]
    [TestCase(14, 17, 11, 13, 0, 0, 14, 16)]
    public void Zoom_should_truncate_the_source_rectangle_before_integer_centering(
        int width, int height, int imageWidth, int imageHeight, int left, int top, int drawnWidth, int drawnHeight)
    {
        PictureBox picture = new()
        {
            Width = width,
            Height = height,
            Source = CreateImage(imageWidth, imageHeight),
            SizeMode = PictureBoxSizeMode.Zoom,
        };
        Window window = new() { Width = width, Height = height, Content = picture };
        try
        {
            window.Show();
            window.UpdateLayout();
            picture.Bounds.Size.Should().Be(new Size(width, height));
            Image image = (Image)picture.Child!;
            image.Bounds.Should().Be(new Rect(left, top, drawnWidth, drawnHeight));
            image.Stretch.Should().Be(Stretch.Fill);
            picture.SizeMode.Should().Be(PictureBoxSizeMode.Zoom);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(0, 8, 0, 169, 44)]
    [TestCase(1, 12, 1, 161, 42)]
    [TestCase(2, 15, 2, 154, 40)]
    public void Zoom_should_exclude_the_nonclient_border_but_ignore_source_padding(
        int borderWidth, int left, int top, int drawnWidth, int drawnHeight)
    {
        PictureBox picture = new()
        {
            BorderThickness = new Thickness(borderWidth),
            Padding = new Thickness(7, 3, 9, 5),
            Source = CreateImage(385, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
        };
        Window window = new() { Width = 185, Height = 44, Content = picture };
        try
        {
            window.Show();
            window.UpdateLayout();
            ((Image)picture.Child!).Bounds.Should().Be(new Rect(left, top, drawnWidth, drawnHeight));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Zoom_should_round_the_native_client_size_before_scaling()
    {
        PictureBox picture = new()
        {
            Width = 185.6,
            Height = 44.4,
            UseLayoutRounding = false,
            Source = CreateImage(400, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
        };
        Window window = new() { Width = 200, Height = 100, Content = picture };
        try
        {
            window.Show();
            window.UpdateLayout();
            ((Image)picture.Child!).Bounds.Should().Be(new Rect(5, 0, 176, 44));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Zoom_should_recalculate_its_rectangle_when_the_source_or_frame_changes()
    {
        PictureBox picture = new()
        {
            Source = CreateImage(403, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
        };
        Window window = new() { Width = 185, Height = 44, Content = picture };
        try
        {
            window.Show();
            window.UpdateLayout();
            Image image = (Image)picture.Child!;
            image.Bounds.Should().Be(new Rect(4, 0, 177, 44));

            picture.Source = CreateImage(100, 403);
            window.UpdateLayout();
            image.Bounds.Should().Be(new Rect(87, 0, 10, 44));

            window.Width = 44;
            window.Height = 185;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            image.Bounds.Should().Be(new Rect(0, 4, 44, 177));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(PictureBoxSizeMode.Normal)]
    [TestCase(PictureBoxSizeMode.CenterImage)]
    public void Zoom_should_restore_the_other_size_mode_without_retaining_the_zoom_rectangle(PictureBoxSizeMode mode)
    {
        PictureBox picture = new()
        {
            Source = CreateImage(40, 20),
            SizeMode = PictureBoxSizeMode.Zoom,
        };
        Window window = new() { Width = 100, Height = 60, Content = picture };
        try
        {
            window.Show();
            window.UpdateLayout();
            Image image = (Image)picture.Child!;
            image.Bounds.Should().Be(new Rect(0, 5, 100, 50));

            picture.SizeMode = mode;
            window.UpdateLayout();
            picture.SizeMode.Should().Be(mode);
            image.Stretch.Should().Be(Stretch.None);
            image.Bounds.Should().Be(mode == PictureBoxSizeMode.CenterImage
                ? new Rect(30, 20, 40, 20) : new Rect(0, 0, 40, 20));
        }
        finally
        {
            window.Close();
        }
    }

    private static DrawingImage CreateImage(int width, int height)
        => new(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(0, 0, width, height)),
            Brush = Brushes.Red,
        });
}
