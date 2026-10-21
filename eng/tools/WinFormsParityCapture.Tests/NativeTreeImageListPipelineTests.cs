using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using GitUI.UserControls;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeTreeImageListPipelineTests
{
    [TestCase("synthetic-black")]
    [TestCase("synthetic-white")]
    [TestCase("synthetic-gray")]
    [TestCase("synthetic-color")]
    [TestCase("BranchLocalRoot")]
    [TestCase("Stash")]
    [TestCase("TagHorizontal")]
    public void Source_image_list_should_record_every_alpha_conversion_boundary(string asset)
    {
        string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TreeImageListProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using Bitmap source = CreateSource(asset);
        source.Size.Should().Be(new Size(16, 16), "the source consumer authors 16-pixel icons at its native 96-DPI baseline");
        using Bitmap padded = new(source.Width, source.Height + 2, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(padded))
        {
            // This is the original RepoObjectsTree.InitImageList pipeline, not a
            // premultiplication formula inferred from the resulting screenshots.
            graphics.DrawImageUnscaled(source, 0, 1);
        }

        using ImageList images = new() { ImageSize = padded.Size, ColorDepth = ColorDepth.Depth32Bit };
        images.Images.Add(padded);
        nint handle = images.Handle;
        using Bitmap retainedGetter = (Bitmap)images.Images[0];
        using Bitmap retained = new(retainedGetter);
        retained.Size.Should().Be(padded.Size);
        ImageListGetImageInfo(handle, 0, out NativeImageInformation imageInformation).Should().BeTrue();
        Rectangle imageRectangle = Rectangle.FromLTRB(imageInformation.Rectangle.Left, imageInformation.Rectangle.Top,
            imageInformation.Rectangle.Right, imageInformation.Rectangle.Bottom);
        imageRectangle.Size.Should().Be(padded.Size);

        nint mask = ControlPaint.CreateHBitmapTransparencyMask(padded);
        mask.Should().NotBe(0);
        nint color = 0;
        try
        {
            color = ControlPaint.CreateHBitmapColorMask(padded, mask);
            color.Should().NotBe(0);
            NativeBitmapReport colorMask = ReadBitmap(color);
            NativeBitmapReport transparencyMask = ReadBitmap(mask);
            NativeBitmapReport atlas = ReadBitmap(imageInformation.ImageBitmap);
            NativeBitmapReport atlasMask = ReadBitmap(imageInformation.MaskBitmap);
            source.Save(Path.Combine(directory, "source.png"), ImageFormat.Png);
            padded.Save(Path.Combine(directory, "padded.png"), ImageFormat.Png);
            retainedGetter.Save(Path.Combine(directory, "retained-getter.png"), ImageFormat.Png);
            retained.Save(Path.Combine(directory, "retained.png"), ImageFormat.Png);

            List<DrawReport> drawnImages = [];
            foreach (Color backdrop in new[]
            {
                Color.White, Color.Black, Color.FromArgb(50, 50, 50),
                Color.FromArgb(204, 232, 255), Color.FromArgb(43, 45, 58)
            })
            {
                using Bitmap drawn = DrawImageList(images, backdrop);
                string fileName = $"draw-{backdrop.ToArgb():X8}.png";
                drawn.Save(Path.Combine(directory, fileName), ImageFormat.Png);
                drawnImages.Add(new DrawReport(Argb(backdrop), fileName, ReadPixels(drawn)));
                for (int x = 0; x < padded.Width; x++)
                {
                    drawn.GetPixel(x, 0).ToArgb().Should().Be(backdrop.ToArgb(), "the source padding is fully transparent");
                    drawn.GetPixel(x, drawn.Height - 1).ToArgb().Should().Be(backdrop.ToArgb());
                }

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        Color original = source.GetPixel(x, y);
                        Color actual = drawn.GetPixel(x, y + 1);
                        actual.A.Should().Be(byte.MaxValue, "ImageList_Draw is composited into an initialized opaque native HDC");
                        if (original.A == 0)
                        {
                            actual.ToArgb().Should().Be(backdrop.ToArgb());
                        }
                        else if (original.A == byte.MaxValue)
                        {
                            actual.ToArgb().Should().Be(original.ToArgb(), "the fully opaque native image pixels do not depend on the backdrop");
                        }

                        // Partial-alpha equations intentionally are not asserted until
                        // all independent native boundaries have been measured.
                    }
                }
            }

            nint deviceContext = GetDC(0);
            deviceContext.Should().NotBe(0);
            int horizontalDpi;
            int verticalDpi;
            try
            {
                horizontalDpi = GetDeviceCaps(deviceContext, 88);
                verticalDpi = GetDeviceCaps(deviceContext, 90);
            }
            finally
            {
                ReleaseDC(0, deviceContext);
            }

            horizontalDpi.Should().Be(96);
            verticalDpi.Should().Be(96);
            List<TreeDrawReport> treeDrawn = DrawTreeView(images, drawnImages, directory);
            PipelineReport report = new(
                asset, "originalRepoObjectsTreePaddingThenImageListDepth32Bit", "nativeMonitor", horizontalDpi, verticalDpi,
                Application.ColorMode.ToString(), Application.IsDarkModeEnabled, "nativeImageListDrawIntoInitializedGdiHdc", 1,
                ReadPixels(source), ReadPixels(padded), colorMask, transparencyMask, atlas, atlasMask,
                imageRectangle, ReadPixels(retainedGetter), ReadPixels(retained), drawnImages, treeDrawn);
            string metadataPath = Path.Combine(directory, "pipeline.json");
            File.WriteAllText(metadataPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"asset={asset} dpiMode=nativeMonitor dpi={horizontalDpi},{verticalDpi} output={metadataPath}");
        }
        finally
        {
            if (color != 0)
            {
                DeleteObject(color).Should().BeTrue();
            }

            DeleteObject(mask).Should().BeTrue();
        }
    }

    private static Bitmap CreateSource(string asset)
    {
        if (!asset.StartsWith("synthetic-", StringComparison.Ordinal))
        {
            Bitmap source = asset switch
            {
                "BranchLocalRoot" => GitUI.Properties.Images.BranchLocalRoot,
                "Stash" => GitUI.Properties.Images.Stash,
                "TagHorizontal" => GitUI.Properties.Images.TagHorizontal,
                _ => throw new ArgumentOutOfRangeException(nameof(asset))
            };
            return (Bitmap)source.Clone();
        }

        Color color = asset switch
        {
            "synthetic-black" => Color.Black,
            "synthetic-white" => Color.White,
            "synthetic-gray" => Color.FromArgb(85, 85, 85),
            "synthetic-color" => Color.FromArgb(53, 103, 173),
            _ => throw new ArgumentOutOfRangeException(nameof(asset))
        };
        Bitmap bitmap = new(16, 16, PixelFormat.Format32bppArgb);
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                bitmap.SetPixel(x, y, Color.FromArgb(x + (y * 16), color));
            }
        }

        return bitmap;
    }

    private static Bitmap DrawImageList(ImageList images, Color backdrop)
    {
        Bitmap bitmap = new(images.ImageSize.Width, images.ImageSize.Height, PixelFormat.Format24bppRgb);
        try
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            nint deviceContext = graphics.GetHdc();
            nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
            brush.Should().NotBe(0);
            try
            {
                // Graphics.Clear initializes GDI+'s image, not necessarily the
                // transient surface returned by GetHdc. Fill that actual HDC.
                NativeRectangle bounds = new() { Right = bitmap.Width, Bottom = bitmap.Height };
                FillRect(deviceContext, ref bounds, brush).Should().NotBe(0);
                ImageListDraw(images.Handle, 0, deviceContext, 0, 0, 1).Should().BeTrue();
            }
            finally
            {
                DeleteObject(brush);
                graphics.ReleaseHdc(deviceContext);
            }

            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    private static List<TreeDrawReport> DrawTreeView(ImageList images, List<DrawReport> expected, string directory)
    {
        using Font font = new("Segoe UI", 9);
        using Form window = new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(250, 80), ShowInTaskbar = false };
        using NativeTreeView tree = new()
        {
            Font = font,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            ImageList = images,
            ShowLines = false,
            ShowRootLines = false,
            ShowPlusMinus = false,
        };
        TreeNode node = tree.Nodes.Add("Native image pipeline");
        node.ImageIndex = 0;
        node.SelectedImageIndex = 0;
        window.Controls.Add(tree);
        window.Show();
        window.Activate();
        tree.Focus().Should().BeTrue();
        List<TreeDrawReport> reports = [];
        foreach (DrawReport draw in expected)
        {
            tree.BackColor = Color.FromArgb(unchecked((int)Convert.ToUInt32(draw.Backdrop[1..], 16)));
            tree.SelectedNode = null;

            // Native client painting is queued after Show and each BackColor
            // update. Settle real messages, never retry based on captured colors.
            long settledAt = Environment.TickCount64 + 100;
            while (Environment.TickCount64 < settledAt)
            {
                Application.DoEvents();
            }

            window.Refresh();
            tree.Refresh();
            window.Update();
            tree.Update();
            tree.DeviceDpi.Should().Be(96);
            window.DeviceDpi.Should().Be(96);
            tree.ItemHeight.Should().Be(images.ImageSize.Height);
            int imageY = node.Bounds.Top + ((tree.ItemHeight - images.ImageSize.Height) / 2);
            int[] imageColumns = Enumerable.Range(0, tree.ClientSize.Width)
                .Where(x => tree.HitTest(x, imageY + (images.ImageSize.Height / 2)).Location.HasFlag(TreeViewHitTestLocations.Image)).ToArray();
            using CaptureImageResult capture = ImageCapture.Capture(window, [], []);
            capture.Method.Should().Be(CaptureMethod.PrintWindow);
            string fileName = $"tree-{draw.Backdrop[1..]}.png";
            capture.Bitmap.Save(Path.Combine(directory, fileName), ImageFormat.Png);
            imageColumns.Length.Should().BeGreaterThanOrEqualTo(images.ImageSize.Width,
                "native hit testing includes image-slot slack; the HIMAGELIST independently authors the raster width");
            Rectangle imageBounds = new(imageColumns[0], imageY, images.ImageSize.Width, images.ImageSize.Height);
            Rectangle hitTestBounds = new(imageColumns[0], imageY, imageColumns.Length, images.ImageSize.Height);
            Point origin = tree.PointToScreen(Point.Empty) - new Size(capture.ScreenBounds.Location);
            string[][] actual = new string[imageBounds.Height][];
            for (int y = 0; y < imageBounds.Height; y++)
            {
                actual[y] = new string[imageBounds.Width];
                for (int x = 0; x < imageBounds.Width; x++)
                {
                    actual[y][x] = Argb(capture.Bitmap.GetPixel(origin.X + imageBounds.Left + x, origin.Y + imageBounds.Top + y));
                }
            }

            TreeDrawReport report = new(draw.Backdrop, fileName, "PrintWindow", tree.DeviceDpi, tree.Focused,
                node.Bounds, imageBounds, hitTestBounds, "nativeTreeViewHitTestLeadingEdgeAndHimlRasterDescriptor", actual);
            reports.Add(report);
            File.WriteAllText(Path.Combine(directory, "tree-draw.json"), JsonSerializer.Serialize(reports, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"nativeTreeDraw={Path.Combine(directory, fileName)} backdrop={draw.Backdrop} captureMethod={capture.Method} imageBounds={imageBounds} hitTestBounds={hitTestBounds} labelBounds={node.Bounds} nativeIndent={tree.Indent} nativeDpi={tree.DeviceDpi}");
            for (int y = 0; y < imageBounds.Height; y++)
            {
                actual[y].Should().Equal(draw.Pixels[y], "actual native TREEVIEW drawing must corroborate the independent ImageList_Draw RGB pipeline");
            }
        }

        return reports;
    }

    private static NativeBitmapReport ReadBitmap(nint bitmap)
    {
        bitmap.Should().NotBe(0);
        GetObject(bitmap, Marshal.SizeOf<NativeBitmap>(), out NativeBitmap descriptor).Should().NotBe(0);
        descriptor.Width.Should().BeGreaterThan(0);
        descriptor.Height.Should().BeGreaterThan(0);
        int objectBytes = GetDibSectionObject(bitmap, Marshal.SizeOf<NativeDibSection>(), out NativeDibSection section);
        NativeStorageReport storage;
        if (objectBytes == Marshal.SizeOf<NativeDibSection>() && section.Bitmap.Bits != 0)
        {
            // Public ControlPaint and ImageList operations can queue GDI writes.
            // Synchronize that batch before reading the actual DIBSECTION memory.
            GdiFlush().Should().BeTrue();
            int byteCount = checked(Math.Abs(section.Bitmap.WidthBytes) * section.Bitmap.Height);
            byte[] nativeBytes = new byte[byteCount];
            Marshal.Copy(section.Bitmap.Bits, nativeBytes, 0, nativeBytes.Length);
            string[] byteRows = new string[section.Bitmap.Height];
            for (int y = 0; y < byteRows.Length; y++)
            {
                byteRows[y] = Convert.ToHexString(nativeBytes.AsSpan(y * Math.Abs(section.Bitmap.WidthBytes), Math.Abs(section.Bitmap.WidthBytes)));
            }

            storage = new NativeStorageReport("supported", "actualDibSectionNativeBytesUnconverted", null, objectBytes,
                section.Header.Height, section.Header.BitCount, section.Header.Compression,
                section.RedMask, section.GreenMask, section.BlueMask, byteRows);
        }
        else
        {
            storage = new NativeStorageReport("unsupported", "deviceDependentBitmapNativeStorageNotExposed",
                "A device-dependent bitmap exposes no raw storage pointer. GetDIBits RGB conversion is not evidence of preserved native alpha bytes.", objectBytes,
                null, null, null, null, null, null, null);
        }

        nint deviceContext = GetDC(0);
        deviceContext.Should().NotBe(0);
        try
        {
            BitmapInformation information = new()
            {
                Header = new BitmapInformationHeader
                {
                    Size = (uint)Marshal.SizeOf<BitmapInformationHeader>(),
                    Width = descriptor.Width,
                    Height = -descriptor.Height,
                    Planes = 1,
                    BitCount = 32,
                    Compression = 0,
                    SizeImage = checked((uint)(descriptor.Width * descriptor.Height * 4))
                }
            };
            byte[] bytes = new byte[information.Header.SizeImage];
            GetDIBits(deviceContext, bitmap, 0, (uint)descriptor.Height, bytes, ref information, 0).Should().Be(descriptor.Height);
            string[][] pixels = new string[descriptor.Height][];
            for (int y = 0; y < descriptor.Height; y++)
            {
                pixels[y] = new string[descriptor.Width];
                for (int x = 0; x < descriptor.Width; x++)
                {
                    int index = ((y * descriptor.Width) + x) * 4;

                    // The alpha byte is recorded exactly as returned, not made
                    // opaque and not assumed to be straight/premultiplied.
                    pixels[y][x] = $"#{bytes[index + 3]:X2}{bytes[index + 2]:X2}{bytes[index + 1]:X2}{bytes[index]:X2}";
                }
            }

            return new NativeBitmapReport(descriptor.Width, descriptor.Height, descriptor.WidthBytes,
                descriptor.Planes, descriptor.BitsPixel, storage,
                "GetDIBits BI_RGB 32bpp topDown BGRA conversion; RGB only is documented, alpha is not assumed preserved", pixels);
        }
        finally
        {
            ReleaseDC(0, deviceContext);
        }
    }

    private static string[][] ReadPixels(Bitmap bitmap)
    {
        string[][] rows = new string[bitmap.Height][];
        for (int y = 0; y < bitmap.Height; y++)
        {
            rows[y] = new string[bitmap.Width];
            for (int x = 0; x < bitmap.Width; x++)
            {
                rows[y][x] = Argb(bitmap.GetPixel(x, y));
            }
        }

        return rows;
    }

    private static string Argb(Color color) => $"#{color.ToArgb():X8}";

    private sealed record PipelineReport(string Asset, string SourceBoundary, string DpiMode, int HorizontalDpi, int VerticalDpi,
        string ColorMode, bool DarkModeEnabled, string CaptureMethod, uint ImageListDrawStyle, string[][] Source,
        string[][] Padded, NativeBitmapReport PublicColorMask, NativeBitmapReport PublicTransparencyMask,
        NativeBitmapReport NativeAtlas, NativeBitmapReport NativeAtlasMask, Rectangle NativeImageRectangle,
        string[][] RetainedGetter, string[][] RetainedClone, List<DrawReport> Drawn, List<TreeDrawReport> TreeDrawn);

    private sealed record NativeBitmapReport(int Width, int Height, int WidthBytes, ushort Planes, ushort BitsPerPixel,
        NativeStorageReport NativeStorage, string ReadFormat, string[][] Pixels);

    private sealed record NativeStorageReport(string Status, string Kind, string? Reason, int GetObjectBytes, int? DibHeight, ushort? DibBitCount,
        uint? Compression, uint? RedMask, uint? GreenMask, uint? BlueMask, string[]? NativeByteRows);

    private sealed record DrawReport(string Backdrop, string ImagePath, string[][] Pixels);

    private sealed record TreeDrawReport(string Backdrop, string ImagePath, string CaptureMethod, int NativeDpi, bool Focused,
        Rectangle NativeLabelBounds, Rectangle NativeImageBounds, Rectangle NativeHitTestImageBounds,
        string ImageLocationProvenance, string[][] Pixels);

    [DllImport("comctl32.dll", EntryPoint = "ImageList_GetImageInfo")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ImageListGetImageInfo(nint imageList, int index, out NativeImageInformation information);

    [DllImport("comctl32.dll", EntryPoint = "ImageList_Draw")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ImageListDraw(nint imageList, int index, nint deviceContext, int x, int y, uint style);

    [DllImport("gdi32.dll", EntryPoint = "GetObjectW")]
    private static extern int GetObject(nint handle, int count, out NativeBitmap bitmap);

    [DllImport("gdi32.dll", EntryPoint = "GetObjectW")]
    private static extern int GetDibSectionObject(nint handle, int count, out NativeDibSection section);

    [DllImport("gdi32.dll")]
    private static extern int GetDIBits(nint deviceContext, nint bitmap, uint start, uint count, [Out] byte[] bits,
        ref BitmapInformation information, uint usage);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(nint deviceContext, int index);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GdiFlush();

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint handle);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint deviceContext, ref NativeRectangle bounds, nint brush);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeImageInformation
    {
        public nint ImageBitmap;
        public nint MaskBitmap;
        public int Unused1;
        public int Unused2;
        public NativeRectangle Rectangle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeBitmap
    {
        public int Type;
        public int Width;
        public int Height;
        public int WidthBytes;
        public ushort Planes;
        public ushort BitsPixel;
        public nint Bits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInformationHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPixelsPerMeter;
        public int YPixelsPerMeter;
        public uint ColorsUsed;
        public uint ColorsImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeDibSection
    {
        public NativeBitmap Bitmap;
        public BitmapInformationHeader Header;
        public uint RedMask;
        public uint GreenMask;
        public uint BlueMask;
        public nint Section;
        public uint Offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInformation
    {
        public BitmapInformationHeader Header;
        public uint UnusedColor;
    }
}
