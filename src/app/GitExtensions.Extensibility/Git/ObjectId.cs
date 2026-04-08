using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace GitExtensions.Extensibility.Git;

/// <summary>
/// Models a SHA1 hash.
/// </summary>
/// <remarks>
/// <para>Instances are immutable and are guaranteed to contain valid, 160-bit (20-byte) SHA1 hashes.</para>
/// <para>String forms of this object must be in lower case.</para>
/// <para>Data is stored in a fixed-size 20-byte buffer in big-endian byte order (SHA-1 natural order),
///  enabling direct hex conversion and SIMD-accelerated equality and comparison via
///  <see cref="MemoryExtensions.SequenceEqual{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>.</para>
/// </remarks>
public sealed class ObjectId : IEquatable<ObjectId>, IComparable<ObjectId>, ISpanFormattable
{
    /// <summary>
    /// A set of valid hex characters for validating input strings without allocating.
    /// </summary>
    /// <remarks>
    /// Note that this set contains only lowercase characters, since uppercase hex is not valid for our purposes.
    /// </remarks>
    private static readonly SearchValues<char> _hexChars = SearchValues.Create("0123456789abcdef");

    private static readonly Random _random = new();

    /// <summary>
    /// Gets the artificial ObjectId used to represent working directory tree (unstaged) changes.
    /// </summary>
    public static ObjectId WorkTreeId { get; } = Parse("1111111111111111111111111111111111111111");

    /// <summary>
    /// Gets the artificial ObjectId used to represent changes staged to the index.
    /// </summary>
    public static ObjectId IndexId { get; } = Parse("2222222222222222222222222222222222222222");

    /// <summary>
    /// Gets the artificial ObjectId used to represent combined diff for merge commits.
    /// </summary>
    public static ObjectId CombinedDiffId { get; } = Parse("3333333333333333333333333333333333333333");

    /// <summary>
    /// Produces an <see cref="ObjectId"/> populated with random bytes.
    /// </summary>
    [Pure]
    public static ObjectId Random()
    {
        Sha1 data = default;
        _random.NextBytes((Span<byte>)data);
        return new ObjectId(data);
    }

    public bool IsArtificial => this == WorkTreeId || this == IndexId || this == CombinedDiffId;

    private const int _sha1ByteCount = 20;
    public const int Sha1CharCount = 40;

    /// <summary>
    /// Fixed-size 20-byte inline buffer for SHA-1 data, stored in big-endian byte order.
    /// </summary>
    [InlineArray(_sha1ByteCount)]
    private struct Sha1
    {
        private byte _element0;
    }

    private readonly Sha1 _data;

    private ObjectId(Sha1 data)
    {
        _data = data;
    }

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
    [Pure]
    public static ObjectId Parse(string s)
    {
        if (s?.Length is not Sha1CharCount || !TryParse(s.AsSpan(), out ObjectId? id))
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
    [Pure]
    public static ObjectId Parse(string s, Capture capture)
    {
        if (s is null || capture?.Length is not Sha1CharCount || !TryParse(s.AsSpan(capture.Index, capture.Length), out ObjectId? id))
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
    /// Parses an <see cref="ObjectId"/> from a span of hex chars.
    /// </summary>
    /// <remarks>
    /// <para>For parsing to succeed, <paramref name="hex"/> must contain exactly 40 hex characters.</para>
    /// </remarks>
    /// <param name="hex">The char span to parse.</param>
    /// <param name="objectId">The parsed <see cref="ObjectId"/>.</param>
    /// <returns><c>true</c> if parsing succeeded, otherwise <c>false</c>.</returns>
    [Pure]
    public static bool TryParse(in ReadOnlySpan<char> hex, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
    {
        if (hex.Length != Sha1CharCount)
        {
            objectId = default;
            return false;
        }

        Sha1 data = default;
        if (Convert.FromHexString(hex, data, out _, out int bytesWritten) != OperationStatus.Done || bytesWritten != _sha1ByteCount)
        {
            objectId = default;
            return false;
        }

        objectId = new ObjectId(data);
        return true;
    }

    /// <summary>
    /// Parses an <see cref="ObjectId"/> from a span of ASCII hex bytes.
    /// </summary>
    /// <remarks>
    /// <para>For parsing to succeed, <paramref name="hex"/> must contain exactly 40 ASCII hex bytes.</para>
    /// </remarks>
    /// <param name="hex">The byte span to parse.</param>
    /// <param name="objectId">The parsed <see cref="ObjectId"/>.</param>
    /// <returns><c>true</c> if parsing succeeded, otherwise <c>false</c>.</returns>
    [Pure]
    public static bool TryParse(in ReadOnlySpan<byte> hex, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
    {
        if (hex.Length != Sha1CharCount)
        {
            objectId = default;
            return false;
        }

        Sha1 data = default;
        if (Convert.FromHexString(hex, data, out _, out int bytesWritten) != OperationStatus.Done || bytesWritten != _sha1ByteCount)
        {
            objectId = default;
            return false;
        }

        objectId = new ObjectId(data);
        return true;
    }

    #endregion

    /// <summary>
    /// Identifies whether <paramref name="s"/> contains a valid 40-character SHA-1 hash.
    /// </summary>
    /// <remarks>
    /// Valid hash strings may only contain lowercase hex characters, and must be exactly 40 characters long.
    /// </remarks>
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

    private static bool IsValidCharacters(string s) => !s.AsSpan().ContainsAnyExcept(_hexChars);

    #region IComparable<ObjectId>

    public int CompareTo(ObjectId? other)
    {
        if (other is null)
        {
            return 1;
        }

        return ((ReadOnlySpan<byte>)_data).SequenceCompareTo(other._data);
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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is less than one, or more than 40.</exception>
    [Pure]
    [SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Performance")]
    public string ToShortString(int length = 8)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than one.");
        }

        if (length > Sha1CharCount)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, $"Cannot be greater than {Sha1CharCount}.");
        }

        // Even lengths: write hex directly into the string buffer — single allocation, no Substring.
        if ((length & 1) == 0)
        {
            return string.Create(length, this, static (chars, self) =>
            {
                int neededBytes = chars.Length / 2;
                ReadOnlySpan<byte> src = ((ReadOnlySpan<byte>)self._data).Slice(0, neededBytes);
                Convert.TryToHexStringLower(src, chars, out _);
            });
        }

        // Odd lengths: decode one extra byte then trim.
        int neededBytesCount = (length + 1) / 2;
        ReadOnlySpan<byte> bytes = ((ReadOnlySpan<byte>)_data).Slice(0, neededBytesCount);
        return Convert.ToHexStringLower(bytes).Substring(0, length);
    }

    /// <summary>
    /// Writes the full 40-character lowercase hex SHA-1 directly into <paramref name="destination"/>
    /// without allocating a string. Ideal for building git command arguments.
    /// </summary>
    /// <param name="destination">A span of at least <see cref="Sha1CharCount"/> characters.</param>
    /// <returns><see langword="true"/> if the write succeeded, <see langword="false"/> if the destination is too small.</returns>
    [Pure]
    public bool WriteTo(Span<char> destination)
    {
        if (destination.Length < Sha1CharCount)
        {
            return false;
        }

        return Convert.TryToHexStringLower((ReadOnlySpan<byte>)_data, destination, out _);
    }

    #region ISpanFormattable

    /// <inheritdoc />
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

    /// <inheritdoc />
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < Sha1CharCount)
        {
            charsWritten = 0;
            return false;
        }

        bool result = Convert.TryToHexStringLower((ReadOnlySpan<byte>)_data, destination, out charsWritten);
        return result;
    }

    #endregion

    #region Equality and hashing

    /// <inheritdoc />
    public bool Equals(ObjectId? other)
    {
        return other is not null &&
               ((ReadOnlySpan<byte>)_data).SequenceEqual(other._data);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ObjectId id && Equals(id);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference((ReadOnlySpan<byte>)_data));
    }

    public static bool operator ==(ObjectId? left, ObjectId? right) => Equals(left, right);
    public static bool operator !=(ObjectId? left, ObjectId? right) => !Equals(left, right);

    #endregion
}
