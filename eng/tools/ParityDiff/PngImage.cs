using System.Buffers.Binary;
using System.IO.Compression;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Decodes capture PNGs for temporary parity measurements.
internal sealed record PngImage(int Width, int Height, byte[] Rgba)
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    public static PngImage Load(string path)
    {
        using FileStream stream = File.OpenRead(path);
        Span<byte> signature = stackalloc byte[Signature.Length];
        stream.ReadExactly(signature);
        if (!signature.SequenceEqual(Signature))
        {
            throw new InvalidDataException($"Image '{path}' is not a PNG file.");
        }

        int width = 0;
        int height = 0;
        byte bitDepth = 0;
        byte colorType = 0;
        byte interlace = 0;
        byte[]? palette = null;
        byte[]? transparency = null;
        using MemoryStream compressed = new();
        Span<byte> lengthBytes = stackalloc byte[4];
        Span<byte> typeBytes = stackalloc byte[4];
        while (true)
        {
            stream.ReadExactly(lengthBytes);
            int length = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);
            stream.ReadExactly(typeBytes);
            string type = System.Text.Encoding.ASCII.GetString(typeBytes);
            byte[] data = new byte[length];
            stream.ReadExactly(data);
            stream.Position += 4;

            switch (type)
            {
                case "IHDR":
                    width = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(0, 4));
                    height = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(4, 4));
                    bitDepth = data[8];
                    colorType = data[9];
                    interlace = data[12];
                    break;
                case "PLTE":
                    palette = data;
                    break;
                case "tRNS":
                    transparency = data;
                    break;
                case "IDAT":
                    compressed.Write(data);
                    break;
                case "IEND":
                    return Decode(path, width, height, bitDepth, colorType, interlace, palette, transparency, compressed);
            }
        }
    }

    private static PngImage Decode(
        string path,
        int width,
        int height,
        byte bitDepth,
        byte colorType,
        byte interlace,
        byte[]? palette,
        byte[]? transparency,
        MemoryStream compressed)
    {
        if (width <= 0 || height <= 0 || bitDepth != 8 || interlace != 0)
        {
            throw new InvalidDataException($"Image '{path}' must be a non-interlaced, 8-bit PNG.");
        }

        int bytesPerPixel = colorType switch
        {
            0 => 1,
            2 => 3,
            3 => 1,
            4 => 2,
            6 => 4,
            _ => throw new InvalidDataException($"Image '{path}' uses unsupported PNG color type {colorType}.")
        };
        int stride = checked(width * bytesPerPixel);
        byte[] filtered = new byte[checked((stride + 1) * height)];
        compressed.Position = 0;
        using (ZLibStream zlib = new(compressed, CompressionMode.Decompress, leaveOpen: true))
        {
            zlib.ReadExactly(filtered);
            if (zlib.ReadByte() != -1)
            {
                throw new InvalidDataException($"Image '{path}' contains unexpected decompressed PNG data.");
            }
        }

        byte[] scanlines = Unfilter(filtered, height, stride, bytesPerPixel);
        byte[] rgba = ConvertToRgba(path, scanlines, width, height, colorType, palette, transparency);
        return new PngImage(width, height, rgba);
    }

    private static byte[] ConvertToRgba(
        string path,
        byte[] source,
        int width,
        int height,
        byte colorType,
        byte[]? palette,
        byte[]? transparency)
    {
        byte[] target = new byte[checked(width * height * 4)];
        int sourceIndex = 0;
        for (int pixel = 0; pixel < width * height; pixel++)
        {
            int targetIndex = pixel * 4;
            switch (colorType)
            {
                case 0:
                    target[targetIndex] = source[sourceIndex];
                    target[targetIndex + 1] = source[sourceIndex];
                    target[targetIndex + 2] = source[sourceIndex++];
                    target[targetIndex + 3] = 255;
                    break;
                case 2:
                    target[targetIndex] = source[sourceIndex++];
                    target[targetIndex + 1] = source[sourceIndex++];
                    target[targetIndex + 2] = source[sourceIndex++];
                    target[targetIndex + 3] = 255;
                    break;
                case 3:
                    int paletteIndex = source[sourceIndex++];
                    int paletteOffset = paletteIndex * 3;
                    if (palette is null || paletteOffset + 2 >= palette.Length)
                    {
                        throw new InvalidDataException($"Image '{path}' has an invalid PNG palette index.");
                    }

                    target[targetIndex] = palette[paletteOffset];
                    target[targetIndex + 1] = palette[paletteOffset + 1];
                    target[targetIndex + 2] = palette[paletteOffset + 2];
                    target[targetIndex + 3] = transparency is not null && paletteIndex < transparency.Length
                        ? transparency[paletteIndex]
                        : byte.MaxValue;
                    break;
                case 4:
                    target[targetIndex] = source[sourceIndex];
                    target[targetIndex + 1] = source[sourceIndex];
                    target[targetIndex + 2] = source[sourceIndex++];
                    target[targetIndex + 3] = source[sourceIndex++];
                    break;
                case 6:
                    Array.Copy(source, sourceIndex, target, targetIndex, 4);
                    sourceIndex += 4;
                    break;
            }
        }

        return target;
    }

    private static byte[] Unfilter(byte[] source, int height, int stride, int bytesPerPixel)
    {
        byte[] target = new byte[checked(height * stride)];
        for (int row = 0; row < height; row++)
        {
            int sourceOffset = row * (stride + 1);
            int targetOffset = row * stride;
            byte filter = source[sourceOffset];
            for (int column = 0; column < stride; column++)
            {
                byte raw = source[sourceOffset + column + 1];
                byte left = column >= bytesPerPixel ? target[targetOffset + column - bytesPerPixel] : (byte)0;
                byte above = row > 0 ? target[targetOffset - stride + column] : (byte)0;
                byte upperLeft = row > 0 && column >= bytesPerPixel
                    ? target[targetOffset - stride + column - bytesPerPixel]
                    : (byte)0;
                target[targetOffset + column] = filter switch
                {
                    0 => raw,
                    1 => unchecked((byte)(raw + left)),
                    2 => unchecked((byte)(raw + above)),
                    3 => unchecked((byte)(raw + ((left + above) / 2))),
                    4 => unchecked((byte)(raw + Paeth(left, above, upperLeft))),
                    _ => throw new InvalidDataException($"Unsupported PNG filter {filter}.")
                };
            }
        }

        return target;
    }

    private static byte Paeth(byte left, byte above, byte upperLeft)
    {
        int prediction = left + above - upperLeft;
        int leftDistance = Math.Abs(prediction - left);
        int aboveDistance = Math.Abs(prediction - above);
        int upperLeftDistance = Math.Abs(prediction - upperLeft);
        return leftDistance <= aboveDistance && leftDistance <= upperLeftDistance
            ? left
            : aboveDistance <= upperLeftDistance
                ? above
                : upperLeft;
    }
}
