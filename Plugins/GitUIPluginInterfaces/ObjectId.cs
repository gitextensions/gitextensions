using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    /// <summary>
    /// Models a SHA1 hash.
    /// </summary>
    /// <remarks>
    /// <para>Instances are immutable and are guaranteed to contain valid, 160-bit (20-byte) SHA1 hashes.</para>
    /// <para>String forms of this object must be in lower case.</para>
    /// </remarks>
    public sealed class ObjectId : IEquatable<ObjectId>, IComparable<ObjectId>
    {
        private static readonly ThreadLocal<byte[]> _buffer = new ThreadLocal<byte[]>(() => new byte[20], trackAllValues: false);
        private static readonly Random _random = new Random();

        /// <summary>
        /// Gets the artificial ObjectId used to represent working directory tree (unstaged) changes.
        /// </summary>
        [NotNull]
        public static ObjectId WorkTreeId { get; } = new ObjectId(0x11111111, 0x11111111, 0x11111111, 0x11111111, 0x11111111);

        /// <summary>
        /// Gets the artificial ObjectId used to represent changes staged to the index.
        /// </summary>
        [NotNull]
        public static ObjectId IndexId { get; } = new ObjectId(0x22222222, 0x22222222, 0x22222222, 0x22222222, 0x22222222);

        /// <summary>
        /// Gets the artificial ObjectId used to represent combined diff for merge commits.
        /// </summary>
        [NotNull]
        public static ObjectId CombinedDiffId { get; } = new ObjectId(0x33333333, 0x33333333, 0x33333333, 0x33333333, 0x33333333);

        /// <summary>
        /// Produces an <see cref="ObjectId"/> populated with random bytes.
        /// </summary>
        [NotNull]
        [MustUseReturnValue]
        public static ObjectId Random()
        {
            return new ObjectId(
                unchecked((uint)_random.Next()),
                unchecked((uint)_random.Next()),
                unchecked((uint)_random.Next()),
                unchecked((uint)_random.Next()),
                unchecked((uint)_random.Next()));
        }

        public bool IsArtificial => this == WorkTreeId || this == IndexId || this == CombinedDiffId;

        private const int Sha1ByteCount = 20;
        public const int Sha1CharCount = 40;

        #region Parsing

        /// <summary>
        /// Attempts to parse an <see cref="ObjectId"/> from <paramref name="s"/>.
        /// </summary>
        /// <remarks>
        /// For parsing to succeed, <paramref name="s"/> must be a valid 40-character SHA-1 string.
        /// Any extra characters at the end will cause parsing to fail, unlike for
        /// overload <see cref="TryParse(string,int,out ObjectId)"/>.
        /// </remarks>
        /// <param name="s">The string to try parsing from.</param>
        /// <param name="objectId">The parsed <see cref="ObjectId"/>, or <c>null</c> if parsing was unsuccessful.</param>
        /// <returns><c>true</c> if parsing was successful, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,objectId:null")]
        [ContractAnnotation("=>true,objectId:notnull")]
        public static bool TryParse([CanBeNull] string s, out ObjectId objectId)
        {
            if (s == null || s.Length != Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            return TryParse(s, 0, out objectId);
        }

        /// <summary>
        /// Attempts to parse an <see cref="ObjectId"/> from <paramref name="s"/>, starting at <paramref name="offset"/>.
        /// </summary>
        /// <remarks>
        /// For parsing to succeed, <paramref name="s"/> must contain a valid 40-character SHA-1 starting at <paramref name="offset"/>.
        /// Any extra characters before or after this substring will be ignored, unlike for
        /// overload <see cref="TryParse(string,out ObjectId)"/>.
        /// </remarks>
        /// <param name="s">The string to try parsing from.</param>
        /// <param name="offset">The position within <paramref name="s"/> to start parsing from.</param>
        /// <param name="objectId">The parsed <see cref="ObjectId"/>, or <c>null</c> if parsing was unsuccessful.</param>
        /// <returns><c>true</c> if parsing was successful, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,objectId:null")]
        [ContractAnnotation("=>true,objectId:notnull")]
        public static bool TryParse([CanBeNull] string s, int offset, out ObjectId objectId)
        {
            if (s == null || s.Length - offset < Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            var success = true;

            var i1 = HexToUInt32(offset);
            var i2 = HexToUInt32(offset + 8);
            var i3 = HexToUInt32(offset + 16);
            var i4 = HexToUInt32(offset + 24);
            var i5 = HexToUInt32(offset + 32);

            if (success)
            {
                objectId = new ObjectId(i1, i2, i3, i4, i5);
                return true;
            }

            objectId = default;
            return false;

            uint HexToUInt32(int j)
            {
                return (uint)(HexCharToInt(s[j]) << 28 |
                              HexCharToInt(s[j + 1]) << 24 |
                              HexCharToInt(s[j + 2]) << 20 |
                              HexCharToInt(s[j + 3]) << 16 |
                              HexCharToInt(s[j + 4]) << 12 |
                              HexCharToInt(s[j + 5]) << 8 |
                              HexCharToInt(s[j + 6]) << 4 |
                              HexCharToInt(s[j + 7]));
            }

            int HexCharToInt(char c)
            {
                if (c >= '0' && c <= '9')
                {
                    return c - 48;
                }

                if (c >= 'a' && c <= 'f')
                {
                    return c - 87;
                }

                success = false;
                return -1;
            }
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from <paramref name="s"/>.
        /// </summary>
        /// <remarks>
        /// For parsing to succeed, <paramref name="s"/> must be a valid 40-character SHA-1 string.
        /// Any extra characters at the end will cause parsing to fail, unlike for
        /// overload <see cref="Parse(string,int)"/>.
        /// </remarks>
        /// <param name="s">The string to try parsing from.</param>
        /// <returns>The parsed <see cref="ObjectId"/>.</returns>
        /// <exception cref="FormatException"><paramref name="s"/> did not contain a valid 40-character SHA-1 hash.</exception>
        [NotNull]
        [MustUseReturnValue]
        public static ObjectId Parse([NotNull] string s)
        {
            if (s == null || s.Length != Sha1CharCount || !TryParse(s, 0, out var id))
            {
                throw new FormatException($"Unable to parse object ID \"{s}\".");
            }

            return id;
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from <paramref name="s"/>.
        /// </summary>
        /// <remarks>
        /// For parsing to succeed, <paramref name="s"/> must contain a valid 40-character SHA-1 starting at <paramref name="offset"/>.
        /// Any extra characters before or after this substring will be ignored, unlike for
        /// overload <see cref="Parse(string)"/>.
        /// </remarks>
        /// <param name="s">The string to try parsing from.</param>
        /// <param name="offset">The position within <paramref name="s"/> to start parsing from.</param>
        /// <returns>The parsed <see cref="ObjectId"/>.</returns>
        /// <exception cref="FormatException"><paramref name="s"/> did not contain a valid 40-character SHA-1 hash.</exception>
        [NotNull]
        [MustUseReturnValue]
        public static ObjectId Parse([NotNull] string s, int offset)
        {
            if (!TryParse(s, offset, out var id))
            {
                throw new FormatException($"Unable to parse object ID \"{s}\" at offset {offset}.");
            }

            return id;
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from <paramref name="stream"/>.
        /// </summary>
        /// <remarks>
        /// For parsing to succeed, it must be possible to read 20 bytes from <paramref name="stream"/>.
        /// </remarks>
        /// <param name="stream">The stream to read bytes from.</param>
        /// <returns>The parsed <see cref="ObjectId"/>.</returns>
        /// <exception cref="IOException">General error reading from <paramref name="stream"/>.</exception>
        /// <exception cref="EndOfStreamException"><paramref name="stream"/> ended before 20 bytes could be read.</exception>
        [NotNull]
        [MustUseReturnValue]
        public static ObjectId Parse([NotNull] Stream stream)
        {
            var buffer = _buffer.Value;

            stream.ReadBytes(buffer, offset: 0, count: 20);

            return Parse(buffer, index: 0);
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from <paramref name="bytes"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <remarks>
        /// For parsing to succeed, there must be 20 bytes in <paramref name="bytes"/> starting at <paramref name="index"/>.
        /// </remarks>
        /// <param name="bytes">The byte array to parse from.</param>
        /// <param name="index">The index within <paramref name="bytes"/> to commence parsing from.</param>
        /// <returns>The parsed <see cref="ObjectId"/>.</returns>
        [NotNull]
        [MustUseReturnValue]
        public static ObjectId Parse([NotNull] byte[] bytes, int index)
        {
            return new ObjectId(Read(), Read(), Read(), Read(), Read());

            uint Read() => (uint)((bytes[index++] << 24) |
                                  (bytes[index++] << 16) |
                                  (bytes[index++] << 8) |
                                   bytes[index++]);
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from ASCII <paramref name="bytes"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <remarks>
        /// <para>Unlike <see cref="Parse(byte[],int)"/> which reads raw bytes, this method reads human-readable
        /// ASCII-encoded bytes, which are more verbose. Several git commands emit them in this form.</para>
        /// <para>For parsing to succeed, there must be 40 bytes in <paramref name="bytes"/> starting at <paramref name="index"/>.</para>
        /// </remarks>
        /// <param name="bytes">The byte array to parse from.</param>
        /// <param name="index">The index within <paramref name="bytes"/> to commence parsing from.</param>
        /// <param name="objectId">The parsed <see cref="ObjectId"/>.</param>
        /// <returns><c>true</c> if parsing succeeded, otherwise <c>false</c>.</returns>
        [MustUseReturnValue]
        [ContractAnnotation("=>false,objectId:null")]
        [ContractAnnotation("=>true,objectId:notnull")]
        public static bool TryParseAsciiHexBytes([NotNull] byte[] bytes, int index, out ObjectId objectId)
        {
            if (index < 0 || index > bytes.Length - Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            return TryParseAsciiHexBytes(
                new ArraySegment<byte>(bytes, index, Sha1CharCount),
                out objectId);
        }

        [MustUseReturnValue]
        [ContractAnnotation("=>false,objectId:null")]
        [ContractAnnotation("=>true,objectId:notnull")]
        public static bool TryParseAsciiHexBytes(ArraySegment<byte> bytes, int index, out ObjectId objectId)
        {
            // TODO get rid of this overload? slice the array segment instead

            return TryParseAsciiHexBytes(
                new ArraySegment<byte>(bytes.Array, bytes.Offset + index, Sha1CharCount),
                out objectId);
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from a segment of <paramref name="bytes"/> containing ASCII characters.
        /// </summary>
        /// <remarks>
        /// <para>Unlike <see cref="Parse(byte[],int)"/> which reads raw bytes, this method reads human-readable
        /// ASCII-encoded bytes, which are more verbose. Several git commands emit them in this form.</para>
        /// <para>For parsing to succeed, <paramref name="bytes"/> must contain 40 bytes.</para>
        /// </remarks>
        /// <param name="bytes">The byte array to parse from.</param>
        /// <param name="objectId">The parsed <see cref="ObjectId"/>.</param>
        /// <returns><c>true</c> if parsing succeeded, otherwise <c>false</c>.</returns>
        [MustUseReturnValue]
        [ContractAnnotation("=>false,objectId:null")]
        [ContractAnnotation("=>true,objectId:notnull")]
        public static bool TryParseAsciiHexBytes(ArraySegment<byte> bytes, out ObjectId objectId)
        {
            var index = bytes.Offset;

            if (bytes.Count != Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            var success = true;

            var i1 = HexAsciiBytesToUInt32(index);
            var i2 = HexAsciiBytesToUInt32(index + 8);
            var i3 = HexAsciiBytesToUInt32(index + 16);
            var i4 = HexAsciiBytesToUInt32(index + 24);
            var i5 = HexAsciiBytesToUInt32(index + 32);

            if (success)
            {
                objectId = new ObjectId(i1, i2, i3, i4, i5);
                return true;
            }

            objectId = default;
            return false;

            uint HexAsciiBytesToUInt32(int j)
            {
                var array = bytes.Array;

                return (uint)(HexAsciiByteToInt(array[j]) << 28 |
                              HexAsciiByteToInt(array[j + 1]) << 24 |
                              HexAsciiByteToInt(array[j + 2]) << 20 |
                              HexAsciiByteToInt(array[j + 3]) << 16 |
                              HexAsciiByteToInt(array[j + 4]) << 12 |
                              HexAsciiByteToInt(array[j + 5]) << 8 |
                              HexAsciiByteToInt(array[j + 6]) << 4 |
                              HexAsciiByteToInt(array[j + 7]));
            }

            int HexAsciiByteToInt(byte b)
            {
                if (b >= '0' && b <= '9')
                {
                    return b - 48;
                }

                if (b >= 'a' && b <= 'f')
                {
                    return b - 87;
                }

                success = false;
                return -1;
            }
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from a regex <see cref="Capture"/> that was produced by matching against <paramref name="s"/>.
        /// </summary>
        /// <remarks>
        /// <para>This method avoids the temporary string created by calling <see cref="Capture.Value"/>.</para>
        /// <para>For parsing to succeed, <paramref name="s"/> must be a valid 40-character SHA-1 string.</para>
        /// </remarks>
        /// <param name="s">The string that the regex <see cref="Capture"/> was produced from.</param>
        /// <param name="capture">The regex capture/group that describes the location of the SHA-1 hash within <paramref name="s"/>.</param>
        /// <returns>The parsed <see cref="ObjectId"/>.</returns>
        /// <exception cref="FormatException"><paramref name="s"/> did not contain a valid 40-character SHA-1 hash.</exception>
        [NotNull]
        [MustUseReturnValue]
        public static ObjectId Parse([NotNull] string s, [NotNull] Capture capture)
        {
            if (s == null || capture == null || capture.Length != Sha1CharCount || !TryParse(s, capture.Index, out var id))
            {
                throw new FormatException($"Unable to parse object ID \"{s}\".");
            }

            return id;
        }

        #endregion

        /// <summary>
        /// Identifies whether <paramref name="s"/> contains a valid 40-character SHA-1 hash.
        /// </summary>
        /// <param name="s">The string to validate.</param>
        /// <returns><c>true</c> if <paramref name="s"/> is a valid SHA-1 hash, otherwise <c>false</c>.</returns>
        [Pure]
        public static bool IsValid([NotNull] string s) => s.Length == Sha1CharCount && IsValidCharacters(s);

        /// <summary>
        /// Identifies whether <paramref name="s"/> contains between <paramref name="minLength"/> and 40 valid SHA-1 hash characters.
        /// </summary>
        /// <param name="s">The string to validate.</param>
        /// <returns><c>true</c> if <paramref name="s"/> is a valid partial SHA-1 hash, otherwise <c>false</c>.</returns>
        [Pure]
        public static bool IsValidPartial([NotNull] string s, int minLength) => s.Length >= minLength && s.Length <= Sha1CharCount && IsValidCharacters(s);

        private static bool IsValidCharacters(string s)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (!char.IsDigit(c) && (c < 'a' || c > 'f'))
                {
                    return false;
                }
            }

            return true;
        }

        private readonly uint _i1;
        private readonly uint _i2;
        private readonly uint _i3;
        private readonly uint _i4;
        private readonly uint _i5;

        private ObjectId(uint i1, uint i2, uint i3, uint i4, uint i5)
        {
            _i1 = i1;
            _i2 = i2;
            _i3 = i3;
            _i4 = i4;
            _i5 = i5;
        }

        #region IComparable<ObjectId>

        public int CompareTo(ObjectId other)
        {
            var result = 0;

            _ = Compare(_i1, other._i1) ||
                Compare(_i2, other._i2) ||
                Compare(_i3, other._i3) ||
                Compare(_i4, other._i4) ||
                Compare(_i5, other._i5);

            return result;

            bool Compare(uint i, uint j)
            {
                var c = i.CompareTo(j);

                if (c != 0)
                {
                    result = c;
                    return true;
                }

                return false;
            }
        }

        #endregion

        /// <summary>
        /// Returns the SHA-1 hash.
        /// </summary>
        public override string ToString()
        {
            return ToShortString(Sha1CharCount);
        }

        /// <summary>
        /// Returns the first <paramref name="length"/> characters of the SHA-1 hash.
        /// </summary>
        /// <param name="length">The length of the returned string. Defaults to <c>10</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is less than zero, or more than 40.</exception>
        [Pure]
        [NotNull]
        public unsafe string ToShortString(int length = 10)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
            }

            if (length > Sha1CharCount)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Cannot be greater than {Sha1CharCount}.");
            }

            char* buffer = stackalloc char[Sha1CharCount];
            var p = buffer;

            Write(_i1);
            Write(_i2);
            Write(_i3);
            Write(_i4);
            Write(_i5);

            return new string(buffer, 0, length);

            void Write(uint i)
            {
                *p++ = ParseHexDigit(i >> 28);
                *p++ = ParseHexDigit((i >> 24) & 0xF);
                *p++ = ParseHexDigit((i >> 20) & 0xF);
                *p++ = ParseHexDigit((i >> 16) & 0xF);
                *p++ = ParseHexDigit((i >> 12) & 0xF);
                *p++ = ParseHexDigit((i >> 8) & 0xF);
                *p++ = ParseHexDigit((i >> 4) & 0xF);
                *p++ = ParseHexDigit(i & 0xF);
            }

            char ParseHexDigit(uint j) => j < 10 ? (char)('0' + j) : (char)(j + 0x57);
        }

        #region Equality and hashing

        /// <inheritdoc />
        public bool Equals(ObjectId other)
        {
            return other != null &&
                   _i1 == other._i1 &&
                   _i2 == other._i2 &&
                   _i3 == other._i3 &&
                   _i4 == other._i4 &&
                   _i5 == other._i5;
        }

        /// <summary>
        /// Gets whether <paramref name="other"/> is equivalent to this <see cref="ObjectId"/>.
        /// </summary>
        /// <remarks>
        /// <para>This method does not allocate.</para>
        /// <para><paramref name="other"/> must be lower case and not have any surrounding white space.</para>
        /// </remarks>
        public bool Equals([CanBeNull] string other)
        {
            if (other == null || other.Length != Sha1CharCount)
            {
                return false;
            }

            var i = 0;

            return
                TestInt(_i1) &&
                TestInt(_i2) &&
                TestInt(_i3) &&
                TestInt(_i4) &&
                TestInt(_i5);

            bool TestInt(uint k)
            {
                return
                    TestDigit(k >> 28) &&
                    TestDigit((k >> 24) & 0xF) &&
                    TestDigit((k >> 20) & 0xF) &&
                    TestDigit((k >> 16) & 0xF) &&
                    TestDigit((k >> 12) & 0xF) &&
                    TestDigit((k >> 8) & 0xF) &&
                    TestDigit((k >> 4) & 0xF) &&
                    TestDigit(k & 0xF);

                bool TestDigit(uint j)
                {
                    var c = j < 10 ? (char)('0' + j) : (char)(j + 0x57);
                    return other[i++] == c;
                }
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ObjectId id && Equals(id);

        /// <inheritdoc />
        public override int GetHashCode() => unchecked((int)_i2);

        public static bool operator ==(ObjectId left, ObjectId right) => Equals(left, right);
        public static bool operator !=(ObjectId left, ObjectId right) => !Equals(left, right);

        #endregion
    }

    internal static class StreamExtensions
    {
        public static void ReadBytes([NotNull] this Stream stream, [NotNull] byte[] buffer, int offset, int count)
        {
            while (offset != count)
            {
                var read = stream.Read(buffer, offset, count - offset);

                if (read == 0)
                {
                    throw new EndOfStreamException();
                }

                offset += read;
            }
        }
    }
}