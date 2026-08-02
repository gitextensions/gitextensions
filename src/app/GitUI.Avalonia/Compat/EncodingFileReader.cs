using System.Text;

namespace GitUI.Compat;

/// <summary>
///  Opens text streams with the UTF-8-without-BOM detection used by the original text editor.
/// </summary>
internal static class EncodingFileReader
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public static StreamReader OpenStream(Stream stream, Encoding defaultEncoding)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.Length < 2)
        {
            return new StreamReader(stream, defaultEncoding);
        }

        int firstByte = stream.ReadByte();
        int secondByte = stream.ReadByte();
        switch ((firstByte << 8) | secondByte)
        {
            case 0x0000:
            case 0xfffe:
            case 0xfeff:
            case 0xefbb:
                stream.Position = 0;
                return new StreamReader(stream);
            default:
                return AutoDetect(stream, (byte)firstByte, (byte)secondByte, defaultEncoding);
        }
    }

    private static StreamReader AutoDetect(
        Stream stream,
        byte firstByte,
        byte secondByte,
        Encoding defaultEncoding)
    {
        const int ascii = 0;
        const int error = 1;
        const int utf8 = 2;
        const int utf8Sequence = 3;
        int maximumLength = (int)Math.Min(stream.Length, 500_000);
        int state = ascii;
        int sequenceLength = 0;

        for (int i = 0; i < maximumLength; i++)
        {
            byte value = i switch
            {
                0 => firstByte,
                1 => secondByte,
                _ => (byte)stream.ReadByte(),
            };

            if (value < 0x80)
            {
                if (state == utf8Sequence)
                {
                    state = error;
                    break;
                }
            }
            else if (value < 0xc0)
            {
                if (state != utf8Sequence || --sequenceLength < 0)
                {
                    state = error;
                    break;
                }

                if (sequenceLength == 0)
                {
                    state = utf8;
                }
            }
            else if (value is >= 0xc2 and < 0xf5)
            {
                if (state is not (ascii or utf8))
                {
                    state = error;
                    break;
                }

                state = utf8Sequence;
                sequenceLength = value < 0xe0 ? 1 : value < 0xf0 ? 2 : 3;
            }
            else
            {
                state = error;
                break;
            }
        }

        stream.Position = 0;
        if (state is not (ascii or error))
        {
            return new StreamReader(stream, Utf8WithoutBom);
        }

        Encoding fallback = IsUnicode(defaultEncoding) ? Encoding.Default : defaultEncoding;
        return new StreamReader(stream, fallback);
    }

    private static bool IsUnicode(Encoding encoding)
        => encoding.CodePage is 65001 or 65000 or 1200 or 1201;
}
