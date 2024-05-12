using System.Buffers.Binary;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
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
        private static readonly ThreadLocal<byte[]> _buffer = new(() => new byte[_sha1ByteCount], trackAllValues: false);
        private static readonly Random _random = new();

        /// <summary>
        /// Gets the artificial ObjectId used to represent working directory tree (unstaged) changes.
        /// </summary>
        public static ObjectId WorkTreeId { get; } = new(0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x1111_1111, 0, 0, false);

        /// <summary>
        /// Gets the artificial ObjectId used to represent changes staged to the index.
        /// </summary>
        public static ObjectId IndexId { get; } = new(0x2222_2222_2222_2222, 0x2222_2222_2222_2222, 0x2222_2222, 0, 0, false);

        /// <summary>
        /// Gets the artificial ObjectId used to represent combined diff for merge commits.
        /// </summary>
        public static ObjectId CombinedDiffId { get; } = new(0x3333_3333_3333_3333, 0x3333_3333_3333_3333, 0x3333_3333, 0, 0, false);

        /// <summary>
        /// Produces an <see cref="ObjectId"/> populated with random bytes.
        /// </summary>
        [MustUseReturnValue]
        public static ObjectId Random()
        {
            return new ObjectId(
                unchecked((ulong)_random.NextInt64()),
                unchecked((ulong)_random.NextInt64()),
                unchecked((uint)_random.Next()),
                0,
                0,
                false);
        }

        /// <summary>
        /// Checks if a char buffer with the given length can be a valid hash.
        /// </summary>
        /// <param name="length">The length of the buffer.</param>
        /// <returns>True if the buffer can be a valid hash; otherwise false. It must still be checked if the buffer contains a valid hash.</returns>
        public static bool IsValidLength(int length) => length == Sha1CharCount || length == Sha256CharCount;

        public bool IsArtificial => this == WorkTreeId || this == IndexId || this == CombinedDiffId;

        public int CharLength => _isSha256 ? Sha256CharCount : Sha1CharCount;

        private const int _sha1ByteCount = 20;
        private const int _sha256ByteCount = 32;
        public const int Sha1CharCount = 40;
        public const int Sha256CharCount = 64;

        #region Parsing

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from <paramref name="s"/>.
        /// </summary>
        /// <remarks>
        /// For parsing to succeed, <paramref name="s"/> must be a valid 40-character SHA-1 string.
        /// Any extra characters at the end will cause parsing to fail.
        /// </remarks>
        /// <param name="s">The string to try parsing from.</param>
        /// <returns>The parsed <see cref="ObjectId"/>.</returns>
        /// <exception cref="FormatException"><paramref name="s"/> did not contain a valid 40-character SHA-1 hash, or <paramref name="s"/> is <see langword="null"/>.</exception>
        [MustUseReturnValue]
        public static ObjectId Parse(string s)
        {
            if ((s?.Length is not Sha1CharCount && s?.Length is not Sha256CharCount) || !TryParse(s.AsSpan(), out ObjectId id))
            {
                throw new FormatException($"Unable to parse object ID \"{s}\".");
            }

            return id;
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
        [MustUseReturnValue]
        public static ObjectId Parse(string s, Capture capture)
        {
            if (s is null || (capture?.Length is not Sha1CharCount && capture?.Length is not Sha256CharCount) || !TryParse(s.AsSpan(capture.Index, capture.Length), out ObjectId id))
            {
                throw new FormatException($"Unable to parse object ID \"{s}\".");
            }

            return id;
        }

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
        public static bool TryParse(string? s, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
        {
            if (s is null)
            {
                objectId = default;
                return false;
            }

            return TryParse(s.AsSpan(), out objectId);
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
        public static bool TryParse(string? s, int offset, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
        {
            if (s is null || s.Length - offset < Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            if (s.Length - offset >= Sha256CharCount && TryParse(s.AsSpan(offset, Sha256CharCount), out objectId))
            {
                return true;
            }

            return TryParse(s.AsSpan(offset, Sha1CharCount), out objectId);
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from a span of chars <paramref name="array"/>.
        /// </summary>
        /// <remarks>
        /// <para>This method reads human-readable chars.
        /// Several git commands emit them in this form.</para>
        /// <para>For parsing to succeed, <paramref name="array"/> must contain 40 chars.</para>
        /// </remarks>
        /// <param name="array">The char span to parse.</param>
        /// <param name="objectId">The parsed <see cref="ObjectId"/>.</param>
        /// <returns><c>true</c> if parsing succeeded, otherwise <c>false</c>.</returns>
        [MustUseReturnValue]
        [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        public static bool TryParse(in ReadOnlySpan<char> array, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
        {
            if (array.Length != Sha1CharCount && array.Length != Sha256CharCount)
            {
                objectId = default;
                return false;
            }

            bool isSha256 = false;
            ulong i4 = 0;
            uint i5 = 0;

            if (!ulong.TryParse(array.Slice(0, 16), NumberStyles.AllowHexSpecifier, provider: null, out ulong i1)
                || !ulong.TryParse(array.Slice(16, 16), NumberStyles.AllowHexSpecifier, provider: null, out ulong i2)
                || !uint.TryParse(array.Slice(32, 8), NumberStyles.AllowHexSpecifier, provider: null, out uint i3))
            {
                objectId = default;
                return false;
            }

            if (array.Length == Sha256CharCount)
            {
                if (!ulong.TryParse(array.Slice(40, 16), NumberStyles.AllowHexSpecifier, provider: null, out i4)
                || !uint.TryParse(array.Slice(56, 8), NumberStyles.AllowHexSpecifier, provider: null, out i5))
                {
                    objectId = default;
                    return false;
                }

                isSha256 = true;
            }

            objectId = new ObjectId(i1, i2, i3, i4, i5, isSha256);
            return true;
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from a span of bytes <paramref name="array"/> containing ASCII characters.
        /// </summary>
        /// <remarks>
        /// <para>This method reads human-readable ASCII-encoded bytes (more verbose than raw values).
        /// Several git commands emit them in this form.</para>
        /// <para>For parsing to succeed, <paramref name="array"/> must contain 40 bytes.</para>
        /// </remarks>
        /// <param name="array">The byte span to parse.</param>
        /// <param name="objectId">The parsed <see cref="ObjectId"/>.</param>
        /// <returns><c>true</c> if parsing succeeded, otherwise <c>false</c>.</returns>
        [MustUseReturnValue]
        public static bool TryParse(in ReadOnlySpan<byte> array, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
        {
            if (array.Length != Sha1CharCount && array.Length != Sha256CharCount)
            {
                objectId = default;
                return false;
            }

            bool result = TryParse(array, out objectId, out int hashLength);

            if (result && array.Length == Sha256CharCount && hashLength == Sha1CharCount)
            {
                // The array has the length of a sha256 hash but only a sha1 hash could be parsed.
                return false;
            }

            return result;
        }

        /// <summary>
        /// Parses an <see cref="ObjectId"/> from a span of bytes <paramref name="array"/> containing ASCII characters.
        /// </summary>
        /// <remarks>
        /// <para>This method reads human-readable ASCII-encoded bytes (more verbose than raw values).
        /// Several git commands emit them in this form.</para>
        /// <para>For parsing to succeed, <paramref name="array"/> must contain at least 40 bytes.</para>
        /// </remarks>
        /// <param name="array">The byte span to parse.</param>
        /// <param name="objectId">The parsed <see cref="ObjectId"/>.</param>
        /// <param name="hashLength">Contains the length of the hash if <paramref name="array"/> could be parsed.</param>
        /// <returns><c>true</c> if parsing succeeded, otherwise <c>false</c>.</returns>
        [MustUseReturnValue]
        [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        public static bool TryParse(in ReadOnlySpan<byte> array, [NotNullWhen(returnValue: true)] out ObjectId? objectId, out int hashLength)
        {
            hashLength = 0;
            if (array.Length < Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            bool isSha256 = false;
            ulong i4 = 0;
            uint i5 = 0;

            if (!Utf8Parser.TryParse(array.Slice(0, 16), out ulong i1, out int _, standardFormat: 'X')
                || !Utf8Parser.TryParse(array.Slice(16, 16), out ulong i2, out int _, standardFormat: 'X')
                || !Utf8Parser.TryParse(array.Slice(32, 8), out uint i3, out int _, standardFormat: 'X'))
            {
                objectId = default;
                return false;
            }

            hashLength = Sha1CharCount;

            if (array.Length >= Sha256CharCount
                && Utf8Parser.TryParse(array.Slice(40, 16), out i4, out int _, standardFormat: 'X')
                && Utf8Parser.TryParse(array.Slice(56, 8), out i5, out int _, standardFormat: 'X'))
            {
                isSha256 = true;
                hashLength = Sha256CharCount;
            }

            objectId = new ObjectId(i1, i2, i3, i4, i5, isSha256);
            return true;
        }

        #endregion

        /// <summary>
        /// Identifies whether <paramref name="s"/> contains a valid 40-character SHA-1 hash.
        /// </summary>
        /// <param name="s">The string to validate.</param>
        /// <returns><c>true</c> if <paramref name="s"/> is a valid SHA-1 hash, otherwise <c>false</c>.</returns>
        [Pure]
        public static bool IsValid(string s) => (s.Length == Sha1CharCount || s.Length == Sha256CharCount) && IsValidCharacters(s);

        /// <summary>
        /// Identifies whether <paramref name="s"/> contains between <paramref name="minLength"/> and 40 valid SHA-1 hash characters.
        /// </summary>
        /// <param name="s">The string to validate.</param>
        /// <returns><c>true</c> if <paramref name="s"/> is a valid partial SHA-1 hash, otherwise <c>false</c>.</returns>
        [Pure]
        public static bool IsValidPartial(string s, int minLength) => s.Length >= minLength && s.Length <= Sha256CharCount && IsValidCharacters(s);

        private static bool IsValidCharacters(string s)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!char.IsDigit(c) && (c < 'a' || c > 'f'))
                {
                    return false;
                }
            }

            return true;
        }

        private readonly ulong _i1;
        private readonly ulong _i2;
        private readonly uint _i3;
        private readonly ulong _i4;
        private readonly uint _i5;
        private bool _isSha256;

        private ObjectId(ulong i1, ulong i2, uint i3, ulong i4, uint i5, bool isSha256)
        {
            _i1 = i1;
            _i2 = i2;
            _i3 = i3;
            _i4 = i4;
            _i5 = i5;
            _isSha256 = isSha256;
        }

        #region IComparable<ObjectId>

        public int CompareTo(ObjectId other)
        {
            int result = _isSha256.CompareTo(other._isSha256);
            if (result != 0)
            {
                return result;
            }

            result = _i1.CompareTo(other._i1);
            if (result != 0)
            {
                return result;
            }

            result = _i2.CompareTo(other._i2);
            if (result != 0)
            {
                return result;
            }

            result = _i3.CompareTo(other._i3);
            if (result != 0 || !_isSha256)
            {
                return result;
            }

            result = _i4.CompareTo(other._i4);
            if (result != 0)
            {
                return result;
            }

            return _i5.CompareTo(other._i5);
        }

        #endregion

        /// <summary>
        /// Returns the SHA-1 hash.
        /// </summary>
        public override string ToString()
        {
            return ToShortString(_isSha256 ? Sha256CharCount : Sha1CharCount);
        }

        /// <summary>
        /// Returns the first <paramref name="length"/> characters of the SHA-1 hash.
        /// </summary>
        /// <param name="length">The length of the returned string. Defaults to <c>8</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is less than one, or more than 40.</exception>
        [Pure]
        [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        public unsafe string ToShortString(int length = 8)
        {
            if (length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than one.");
            }

            if (!_isSha256 && length > Sha1CharCount)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Cannot be greater than {Sha1CharCount}.");
            }

            if (_isSha256 && length > Sha256CharCount)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Cannot be greater than {Sha256CharCount}.");
            }

            int neededBytesCount = (length + 1) / 2; // equivalent to Math.Ceiling(length / 2.0) with only int calculation
            Span<byte> buffer = stackalloc byte[_isSha256 ? _sha256ByteCount : _sha1ByteCount];

            BinaryPrimitives.WriteUInt64BigEndian(buffer, _i1);
            if (neededBytesCount > 8)
            {
                BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(8, 8), _i2);
                BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(16, 4), _i3);

                if (_isSha256)
                {
                    BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(20, 8), _i4);
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(28, 4), _i5);
                }
            }

            // Operate on the smaller buffer possible
            Span<byte> bufferSlice = buffer.Slice(0, neededBytesCount);

#if NET9_0_OR_GREATER
            return Convert.ToHexStringLower(bufferSlice).Substring(0, length);
#else
            return Convert.ToHexString(bufferSlice).Substring(0, length).ToLowerInvariant();
#endif
        }

        #region Equality and hashing

        /// <inheritdoc />
        public bool Equals(ObjectId? other)
        {
            return other is not null &&
                   _isSha256 == other._isSha256 &&
                   _i1 == other._i1 &&
                   _i2 == other._i2 &&
                   _i3 == other._i3 &&
                   _i4 == other._i4 &&
                   _i5 == other._i5;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ObjectId id && Equals(id);

        /// <inheritdoc />
        public override int GetHashCode() => unchecked((int)_i2);

        public static bool operator ==(ObjectId? left, ObjectId? right) => Equals(left, right);
        public static bool operator !=(ObjectId? left, ObjectId? right) => !Equals(left, right);

        #endregion
    }
}
