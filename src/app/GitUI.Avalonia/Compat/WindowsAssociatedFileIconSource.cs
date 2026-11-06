using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GitUI.Compat;

internal sealed class WindowsAssociatedFileIconSource : IAssociatedFileIconSource
{
    private const uint DiNormal = 0x0003;
    private const uint DibRgbColors = 0;
    private const uint FileAttributeNormal = 0x00000080;
    private const uint ShgfiIcon = 0x00000100;
    private const uint ShgfiSmallIcon = 0x00000001;
    private const uint ShgfiUseFileAttributes = 0x00000010;
    private const int IconSize = 16;

    public IImage? Get(string workingDirectory, string relativeFilePath)
    {
        ShFileInfo fileInfo = default;
        nuint result = SHGetFileInfo(
            relativeFilePath,
            FileAttributeNormal,
            ref fileInfo,
            (uint)Marshal.SizeOf<ShFileInfo>(),
            ShgfiIcon | ShgfiSmallIcon | ShgfiUseFileAttributes);
        if (result == 0 || fileInfo.Icon == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return CopyIcon(fileInfo.Icon);
        }
        finally
        {
            DestroyIcon(fileInfo.Icon);
        }
    }

    private static IImage? CopyIcon(IntPtr icon)
    {
        BitmapInfo bitmapInfo = new()
        {
            Header = new BitmapInfoHeader
            {
                Size = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                Width = IconSize,
                Height = -IconSize,
                Planes = 1,
                BitCount = 32,
                Compression = 0,
                SizeImage = IconSize * IconSize * 4,
            },
        };
        IntPtr bits;
        IntPtr bitmap = CreateDIBSection(IntPtr.Zero, ref bitmapInfo, DibRgbColors, out bits, IntPtr.Zero, 0);
        if (bitmap == IntPtr.Zero || bits == IntPtr.Zero)
        {
            return null;
        }

        IntPtr deviceContext = CreateCompatibleDC(IntPtr.Zero);
        if (deviceContext == IntPtr.Zero)
        {
            DeleteObject(bitmap);
            return null;
        }

        IntPtr previous = SelectObject(deviceContext, bitmap);
        try
        {
            int byteCount = IconSize * IconSize * 4;
            byte[] transparent = new byte[byteCount];
            Marshal.Copy(transparent, 0, bits, byteCount);
            if (!DrawIconEx(deviceContext, 0, 0, icon, IconSize, IconSize, 0, IntPtr.Zero, DiNormal))
            {
                return null;
            }

            return new WriteableBitmap(
                PixelFormat.Bgra8888,
                AlphaFormat.Premul,
                bits,
                new PixelSize(IconSize, IconSize),
                new Vector(96, 96),
                IconSize * 4);
        }
        finally
        {
            SelectObject(deviceContext, previous);
            DeleteDC(deviceContext);
            DeleteObject(bitmap);
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern nuint SHGetFileInfo(
        string path,
        uint fileAttributes,
        ref ShFileInfo fileInfo,
        uint fileInfoSize,
        uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr icon);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DrawIconEx(
        IntPtr deviceContext,
        int x,
        int y,
        IntPtr icon,
        int width,
        int height,
        uint step,
        IntPtr flickerFreeBrush,
        uint flags);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr deviceContext);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDIBSection(
        IntPtr deviceContext,
        ref BitmapInfo bitmapInfo,
        uint usage,
        out IntPtr bits,
        IntPtr section,
        uint offset);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr deviceContext, IntPtr value);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteDC(IntPtr deviceContext);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr value);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileInfo
    {
        public IntPtr Icon;
        public int IconIndex;
        public uint Attributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string DisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string TypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public BitmapInfoHeader Header;
        public uint Colors;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public int SizeImage;
        public int XPixelsPerMeter;
        public int YPixelsPerMeter;
        public uint ColorsUsed;
        public uint ColorsImportant;
    }
}
