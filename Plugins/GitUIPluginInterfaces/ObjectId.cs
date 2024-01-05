using System;
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
    public sealed class ObjectId : IEquatable<ObjectId>, IComparable<ObjectId>, IEquatable<string>
    {
        private static readonly ThreadLocal<byte[]> _buffer = new(() => new byte[_sha1ByteCount], trackAllValues: false);
        private static readonly Random _random = new();

        /// <summary>
        /// Gets the artificial ObjectId used to represent working directory tree (unstaged) changes.
        /// </summary>
        public static ObjectId WorkTreeId { get; } = new(0x1111_1111_1111_1111, 0x1111_1111_1111_1111, 0x1111_1111);

        /// <summary>
        /// Gets the artificial ObjectId used to represent changes staged to the index.
        /// </summary>
        public static ObjectId IndexId { get; } = new(0x2222_2222_2222_2222, 0x2222_2222_2222_2222, 0x2222_2222);

        /// <summary>
        /// Gets the artificial ObjectId used to represent combined diff for merge commits.
        /// </summary>
        public static ObjectId CombinedDiffId { get; } = new(0x3333_3333_3333_3333, 0x3333_3333_3333_3333, 0x3333_3333);

        /// <summary>
        /// Produces an <see cref="ObjectId"/> populated with random bytes.
        /// </summary>
        [MustUseReturnValue]
        public static ObjectId Random()
        {
            return new ObjectId(
                unchecked((ulong)_random.NextInt64()),
                unchecked((ulong)_random.NextInt64()),
                unchecked((uint)_random.Next()));
        }

        public bool IsArtificial => this == WorkTreeId || this == IndexId || this == CombinedDiffId;

        private const int _sha1ByteCount = 20;
        public const int Sha1CharCount = 40;

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
            if (s?.Length is not Sha1CharCount || !TryParse(s.AsSpan(), out ObjectId id))
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
            if (s is null || capture?.Length is not Sha1CharCount || !TryParse(s.AsSpan(capture.Index, capture.Length), out ObjectId id))
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
            if (array.Length != Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            if (!ulong.TryParse(array.Slice(0, 16), NumberStyles.AllowHexSpecifier, provider: null, out ulong i1)
                || !ulong.TryParse(array.Slice(16, 16), NumberStyles.AllowHexSpecifier, provider: null, out ulong i2)
                || !uint.TryParse(array.Slice(32, 8), NumberStyles.AllowHexSpecifier, provider: null, out uint i3))
            {
                objectId = default;
                return false;
            }

            objectId = new ObjectId(i1, i2, i3);
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
        [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        public static bool TryParse(in ReadOnlySpan<byte> array, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
        {
            if (array.Length != Sha1CharCount)
            {
                objectId = default;
                return false;
            }

            if (!Utf8Parser.TryParse(array.Slice(0, 16), out ulong i1, out int _, standardFormat: 'X')
                || !Utf8Parser.TryParse(array.Slice(16, 16), out ulong i2, out int _, standardFormat: 'X')
                || !Utf8Parser.TryParse(array.Slice(32, 8), out uint i3, out int _, standardFormat: 'X'))
            {
                objectId = default;
                return false;
            }

            objectId = new ObjectId(i1, i2, i3);
            return true;
        }

        #endregion

        /// <summary>
        /// Identifies whether <paramref name="s"/> contains a valid 40-character SHA-1 hash.
        /// </summary>
        /// <param name="s">The string to validate.</param>
        /// <returns><c>true</c> if <paramref name="s"/> is a valid SHA-1 hash, otherwise <c>false</c>.</returns>
        [Pure]
        public static bool IsValid(string s) => s.Length == Sha1CharCount && IsValidCharacters(s);

        /// <summary>
        /// Identifies whether <paramref name="s"/> contains between <paramref name="minLength"/> and 40 valid SHA-1 hash characters.
        /// </summary>
        /// <param name="s">The string to validate.</param>
        /// <returns><c>true</c> if <paramref name="s"/> is a valid partial SHA-1 hash, otherwise <c>false</c>.</returns>
        [Pure]
        public static bool IsValidPartial(string s, int minLength) => s.Length >= minLength && s.Length <= Sha1CharCount && IsValidCharacters(s);

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

        private ObjectId(ulong i1, ulong i2, uint i3)
        {
            _i1 = i1;
            _i2 = i2;
            _i3 = i3;
        }

        #region IComparable<ObjectId>

        public int CompareTo(ObjectId other)
        {
            int result = _i1.CompareTo(other._i1);
            if (result != 0)
            {
                return result;
            }

            result = _i2.CompareTo(other._i2);
            if (result != 0)
            {
                return result;
            }

            return _i3.CompareTo(other._i3);
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
        /// <param name="length">The length of the returned string. Defaults to <c>8</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is less than zero, or more than 40.</exception>
        [Pure]
        [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
        public unsafe string ToShortString(int length = 8)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
            }

            if (length > Sha1CharCount)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Cannot be greater than {Sha1CharCount}.");
            }

            Span<byte> buffer = stackalloc byte[_sha1ByteCount];

            BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(0, 8), _i1);
            BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(8, 8), _i2);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(16, 4), _i3);

            return Convert.ToHexString(buffer).Substring(0, length).ToLowerInvariant();
        }

        #region Equality and hashing

        /// <inheritdoc />
        public bool Equals(ObjectId? other)
        {
            return other is not null &&
                   _i1 == other._i1 &&
                   _i2 == other._i2 &&
                   _i3 == other._i3;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ObjectId id && Equals(id);

        /// <inheritdoc />
        public override int GetHashCode() => unchecked((int)_i2);

        public bool Equals(string? other)
        {
            if (other == null || other.Length != Sha1CharCount)
            {
                return false;
            }

            return PartToString(1).AsSpan().CompareTo(other.AsSpan(0, 16), StringComparison.InvariantCulture) == 0
                && PartToString(2).AsSpan().CompareTo(other.AsSpan(16, 16), StringComparison.InvariantCulture) == 0
                && PartToString(3).AsSpan().CompareTo(other.AsSpan(32), StringComparison.InvariantCulture) == 0;

            string PartToString(int part)
            {
                int allocSize = part == 3 ? 4 : 8;
                Span<byte> buffer = stackalloc byte[allocSize];

                switch (part)
                {
                    case 1:
                        BinaryPrimitives.WriteUInt64BigEndian(buffer, _i1);
                        break;
                    case 2:
                        BinaryPrimitives.WriteUInt64BigEndian(buffer, _i2);
                        break;
                    case 3:
                        BinaryPrimitives.WriteUInt32BigEndian(buffer, _i3);
                        break;
                    default:
                        break;
                }

#if NET9_0_OR_GREATER
                return Convert.ToHexStringLower(buffer);
#else
                return Convert.ToHexString(buffer).ToLowerInvariant();
#endif
            }
        }

        public static bool operator ==(ObjectId? left, ObjectId? right) => Equals(left, right);
        public static bool operator !=(ObjectId? left, ObjectId? right) => !Equals(left, right);

        #endregion
    }
}
