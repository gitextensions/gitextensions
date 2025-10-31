using System.Text;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommandsTests.Git;

// TODO SUT is in GitUIPluginInterfaces but no test assembly exists for that assembly

[TestFixture]
public sealed partial class ObjectIdTests
{
    [GeneratedRegex(@"[a-f0-9]{40}", RegexOptions.ExplicitCapture)]
    private static partial Regex Sha40Regex();
    [GeneratedRegex(@"[a-f0-9]{39}", RegexOptions.ExplicitCapture)]
    private static partial Regex Sha39Regex();
    [GeneratedRegex(@"[XYZa-f0-9]{39}", RegexOptions.ExplicitCapture)]
    private static partial Regex ShaXYZRegex();

    [TestCase("0000000000000000000000000000000000000000")]
    [TestCase("0102030405060708091011121314151617181920")]
    [TestCase("0123456789abcdef0123456789abcdef01234567")]
    public void TryParse_handles_valid_hashes(string sha1)
    {
        ClassicAssert.True(ObjectId.TryParse(sha1, out ObjectId? id));
        ClassicAssert.AreEqual(sha1.ToLower(), id.ToString());
    }

    [TestCase("00000000000000000000000000000000000000")]
    [TestCase("000000000000000000000000000000000000000")]
    [TestCase("01020304050607080910111213141516171819200")]
    [TestCase("010203040506070809101112131415161718192001")]
    [TestCase("ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
    [TestCase("  0000000000000000000000000000000000000000  ")]
    public void TryParse_identifies_invalid_hashes(string sha1)
    {
        ClassicAssert.False(ObjectId.TryParse(sha1, out _));
    }

    [TestCase("0000000000000000000000000000000000000000", 0)]
    [TestCase("0000000000000000000000000000000000000000__", 0)]
    [TestCase("_0102030405060708091011121314151617181920", 1)]
    [TestCase("_0102030405060708091011121314151617181920_", 1)]
    [TestCase("__0102030405060708091011121314151617181920", 2)]
    [TestCase("__0102030405060708091011121314151617181920__", 2)]
    public void TryParse_with_offset_handles_valid_hashes(string sha1, int offset)
    {
        ClassicAssert.True(ObjectId.TryParse(sha1, offset, out ObjectId? id));
        ClassicAssert.AreEqual(
            sha1.Substring(offset, 40),
            id.ToString());
    }

    [TestCase("0000000000000000000000000000000000000000")]
    [TestCase("0102030405060708091011121314151617181920")]
    [TestCase("0123456789abcdef0123456789abcdef01234567")]
    public void Parse_handles_valid_hashes(string sha1)
    {
        ClassicAssert.AreEqual(
            sha1.ToLower(),
            ObjectId.Parse(sha1).ToString());
    }

    [TestCase("00000000000000000000000000000000000000")]
    [TestCase("000000000000000000000000000000000000000")]
    [TestCase("01020304050607080910111213141516171819200")]
    [TestCase("010203040506070809101112131415161718192001")]
    [TestCase("ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
    [TestCase("  0000000000000000000000000000000000000000  ")]
    public void Parse_throws_for_invalid_hashes(string sha1)
    {
        ClassicAssert.Throws<FormatException>(() => ObjectId.Parse(sha1));
    }

    [TestCase("0000000000000000000000000000000000000000")]
    [TestCase("0102030405060708091011121314151617181920")]
    [TestCase("0123456789abcdef0123456789abcdef01234567")]
    public void IsValid_identifies_valid_hashes(string sha1)
    {
        ClassicAssert.True(ObjectId.IsValid(sha1));
    }

    [TestCase("00000000000000000000000000000000000000")]
    [TestCase("000000000000000000000000000000000000000")]
    [TestCase("01020304050607080910111213141516171819200")]
    [TestCase("010203040506070809101112131415161718192001")]
    [TestCase("ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
    [TestCase("  0000000000000000000000000000000000000000  ")]
    public void IsValid_identifies_invalid_hashes(string sha1)
    {
        ClassicAssert.False(ObjectId.IsValid(sha1));
    }

    [TestCase("0000000000000000000000000000000000000000", 0)]
    [TestCase("0000000000000000000000000000000000000000__", 0)]
    [TestCase("_0102030405060708091011121314151617181920", 1)]
    [TestCase("_0102030405060708091011121314151617181920_", 1)]
    [TestCase("__0102030405060708091011121314151617181920", 2)]
    [TestCase("__0102030405060708091011121314151617181920__", 2)]
    public void Parse_with_offset_handles_valid_hashes(string sha1, int offset)
    {
        ClassicAssert.AreEqual(
            sha1.Substring(offset, 40),
            ObjectId.Parse(sha1.Substring(offset, 40)).ToString());
    }

    [Test]
    public void ParseFromRegexCapture()
    {
        ObjectId objectId = ObjectId.Random();
        string str = "XYZ" + objectId + "XYZ";

        ClassicAssert.AreEqual(objectId, ObjectId.Parse(str, Sha40Regex().Match(str)));
        ClassicAssert.Throws<FormatException>(() => ObjectId.Parse(str, Sha39Regex().Match(str)));
        ClassicAssert.Throws<FormatException>(() => ObjectId.Parse(str, ShaXYZRegex().Match(str)));
    }

    [Test]
    public void WorkTreeId_has_expected_value()
    {
        ClassicAssert.AreEqual(
            "1111111111111111111111111111111111111111",
            ObjectId.WorkTreeId.ToString());
    }

    [Test]
    public void IndexId_has_expected_value()
    {
        ClassicAssert.AreEqual(
            "2222222222222222222222222222222222222222",
            ObjectId.IndexId.ToString());
    }

    [Test]
    public void CombinedDiffId_has_expected_value()
    {
        ClassicAssert.AreEqual(
            "3333333333333333333333333333333333333333",
            ObjectId.CombinedDiffId.ToString());
    }

    [Test]
    public void WorkTreeId_is_artificial()
    {
        ClassicAssert.IsTrue(ObjectId.WorkTreeId.IsArtificial);
    }

    [Test]
    public void IndexId_is_artificial()
    {
        ClassicAssert.IsTrue(ObjectId.IndexId.IsArtificial);
    }

    [Test]
    public void CombinedDiffId_is_artificial()
    {
        ClassicAssert.IsTrue(ObjectId.CombinedDiffId.IsArtificial);
    }

    [Test]
    public void Equivalent_ids_are_equal()
    {
        ClassicAssert.AreEqual(
            ObjectId.Parse("0102030405060708091011121314151617181920"),
            ObjectId.Parse("0102030405060708091011121314151617181920"));

        ClassicAssert.AreEqual(
            ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd"),
            ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd"));

        ClassicAssert.AreEqual(
            ObjectId.WorkTreeId,
            ObjectId.WorkTreeId);

        ClassicAssert.AreEqual(
            ObjectId.WorkTreeId,
            ObjectId.Parse(GitRevision.WorkTreeGuid));

        ClassicAssert.AreEqual(
            ObjectId.IndexId,
            ObjectId.IndexId);

        ClassicAssert.AreEqual(
            ObjectId.IndexId,
            ObjectId.Parse(GitRevision.IndexGuid));

        ClassicAssert.AreEqual(
            ObjectId.CombinedDiffId,
            ObjectId.CombinedDiffId);

        ClassicAssert.AreEqual(
            ObjectId.CombinedDiffId,
            ObjectId.Parse(GitRevision.CombinedDiffGuid));
    }

    [Test]
    public void Different_ids_are_not_equal()
    {
        ClassicAssert.AreNotEqual(
            ObjectId.Parse("0000000000000000000000000000000000000000"),
            ObjectId.Parse("0102030405060708091011121314151617181920"));

        ClassicAssert.AreNotEqual(
            ObjectId.WorkTreeId,
            ObjectId.IndexId);
    }

    [Test]
    public void Equivalent_ids_have_equal_hash_codes()
    {
        ClassicAssert.AreEqual(
            ObjectId.Parse("0102030405060708091011121314151617181920").GetHashCode(),
            ObjectId.Parse("0102030405060708091011121314151617181920").GetHashCode());

        ClassicAssert.AreEqual(
            ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd").GetHashCode(),
            ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd").GetHashCode());

        ClassicAssert.AreEqual(
            ObjectId.WorkTreeId.GetHashCode(),
            ObjectId.WorkTreeId.GetHashCode());

        ClassicAssert.AreEqual(
            ObjectId.IndexId.GetHashCode(),
            ObjectId.IndexId.GetHashCode());
    }

    [Test]
    public void Different_ids_have_different_hash_codes()
    {
        ClassicAssert.AreNotEqual(
            ObjectId.Parse("0000000000000000000000000000000000000000").GetHashCode(),
            ObjectId.Parse("0102030405060708091011121314151617181920").GetHashCode());

        ClassicAssert.AreNotEqual(
            ObjectId.WorkTreeId.GetHashCode(),
            ObjectId.IndexId.GetHashCode());
    }

    private const string NonHexAscii = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz";
    private const string HexAscii = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";

    [TestCase(HexAscii, 0, "000102030405060708090a0b0c0d0e0f10111213")]
    [TestCase(HexAscii, 1, "00102030405060708090a0b0c0d0e0f101112131")]
    [TestCase(HexAscii, 2, "0102030405060708090a0b0c0d0e0f1011121314")]
    [TestCase(HexAscii, 3, "102030405060708090a0b0c0d0e0f10111213141")]
    [TestCase(HexAscii, 26, "0d0e0f101112131415161718191a1b1c1d1e1f20")]
    public void TryParse_works_as_expected(string source, int offset, [CanBeNull] string expected)
    {
        byte[] sourceBytes = Encoding.ASCII.GetBytes(source);

        ClassicAssert.AreEqual(expected is not null, ObjectId.TryParse(sourceBytes.AsSpan(offset, 40), out ObjectId id));

        if (expected is not null)
        {
            ClassicAssert.AreEqual(ObjectId.Parse(expected), id);
        }
    }

    [TestCase(NonHexAscii, 0, null)]
    public void TryParse_bytes_throws_with_illegal_input(string source, int offset, [CanBeNull] string expected)
    {
        byte[] sourceBytes = Encoding.ASCII.GetBytes(source);
        ClassicAssert.AreEqual(expected is not null, ObjectId.TryParse(sourceBytes.AsSpan(offset, 40), out ObjectId id));
    }

    [Test]
    public void TryParse_returns_false_when_array_null()
    {
        ClassicAssert.False(ObjectId.TryParse(default, out ObjectId objectId));
        ClassicAssert.Null(objectId);
        ClassicAssert.False(ObjectId.TryParse(default(Span<byte>), out objectId));
        ClassicAssert.Null(objectId);
    }

    [Test]
    public void TryParse_returns_false_when_bounds_check_fails()
    {
        byte[] bytes = new byte[ObjectId.Sha1CharCount];

        ClassicAssert.False(ObjectId.TryParse(bytes.AsSpan(1), out ObjectId objectId));
        ClassicAssert.Null(objectId);
    }

    [Test]
    public void ToShortString()
    {
        const string s = "0102030405060708091011121314151617181920";
        ObjectId id = ObjectId.Parse(s);

        for (int length = 1; length < ObjectId.Sha1CharCount; length++)
        {
            ClassicAssert.AreEqual(s[..length], id.ToShortString(length));
        }

        ClassicAssert.Throws<ArgumentOutOfRangeException>(() => id.ToShortString(-1));
        ClassicAssert.Throws<ArgumentOutOfRangeException>(() => id.ToShortString(ObjectId.Sha1CharCount + 1));
    }

    [Test]
    public void Equals_using_operator()
    {
        string objectIdString = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        ClassicAssert.IsTrue(ObjectId.Parse(objectIdString) == ObjectId.Parse(objectIdString));
        ClassicAssert.IsFalse(ObjectId.Parse(objectIdString) != ObjectId.Parse(objectIdString));
        ClassicAssert.IsFalse(ObjectId.Parse(objectIdString) == ObjectId.Random());
        ClassicAssert.IsTrue(ObjectId.Parse(objectIdString) != ObjectId.Random());
    }
}
