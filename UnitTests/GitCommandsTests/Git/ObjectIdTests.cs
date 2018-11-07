using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    // TODO SUT is in GitUIPluginInterfaces but no test assembly exists for that assembly

    [TestFixture]
    public sealed class ObjectIdTests
    {
        [TestCase("0000000000000000000000000000000000000000")]
        [TestCase("0102030405060708091011121314151617181920")]
        [TestCase("0123456789abcdef0123456789abcdef01234567")]
        public void TryParse_handles_valid_hashes(string sha1)
        {
            Assert.True(ObjectId.TryParse(sha1, out var id));
            Assert.AreEqual(sha1.ToLower(), id.ToString());
        }

        [TestCase("00000000000000000000000000000000000000")]
        [TestCase("000000000000000000000000000000000000000")]
        [TestCase("01020304050607080910111213141516171819200")]
        [TestCase("010203040506070809101112131415161718192001")]
        [TestCase("ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        [TestCase("  0000000000000000000000000000000000000000  ")]
        public void TryParse_identifies_invalid_hashes(string sha1)
        {
            Assert.False(ObjectId.TryParse(sha1, out _));
        }

        [TestCase("0000000000000000000000000000000000000000", 0)]
        [TestCase("0000000000000000000000000000000000000000__", 0)]
        [TestCase("_0102030405060708091011121314151617181920", 1)]
        [TestCase("_0102030405060708091011121314151617181920_", 1)]
        [TestCase("__0102030405060708091011121314151617181920", 2)]
        [TestCase("__0102030405060708091011121314151617181920__", 2)]
        public void TryParse_with_offset_handles_valid_hashes(string sha1, int offset)
        {
            Assert.True(ObjectId.TryParse(sha1, offset, out var id));
            Assert.AreEqual(
                sha1.Substring(offset, 40),
                id.ToString());
        }

        [TestCase("0000000000000000000000000000000000000000")]
        [TestCase("0102030405060708091011121314151617181920")]
        [TestCase("0123456789abcdef0123456789abcdef01234567")]
        public void Parse_handles_valid_hashes(string sha1)
        {
            Assert.AreEqual(
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
            Assert.Throws<FormatException>(() => ObjectId.Parse(sha1));
        }

        [TestCase("0000000000000000000000000000000000000000")]
        [TestCase("0102030405060708091011121314151617181920")]
        [TestCase("0123456789abcdef0123456789abcdef01234567")]
        public void IsValid_identifies_valid_hashes(string sha1)
        {
            Assert.True(ObjectId.IsValid(sha1));
        }

        [TestCase("00000000000000000000000000000000000000")]
        [TestCase("000000000000000000000000000000000000000")]
        [TestCase("01020304050607080910111213141516171819200")]
        [TestCase("010203040506070809101112131415161718192001")]
        [TestCase("ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        [TestCase("  0000000000000000000000000000000000000000  ")]
        public void IsValid_identifies_invalid_hashes(string sha1)
        {
            Assert.False(ObjectId.IsValid(sha1));
        }

        [TestCase("0000000000000000000000000000000000000000", 0)]
        [TestCase("0000000000000000000000000000000000000000__", 0)]
        [TestCase("_0102030405060708091011121314151617181920", 1)]
        [TestCase("_0102030405060708091011121314151617181920_", 1)]
        [TestCase("__0102030405060708091011121314151617181920", 2)]
        [TestCase("__0102030405060708091011121314151617181920__", 2)]
        public void Parse_with_offset_handles_valid_hashes(string sha1, int offset)
        {
            Assert.AreEqual(
                sha1.Substring(offset, 40),
                ObjectId.Parse(sha1, offset).ToString());
        }

        [Test]
        public void ParseFromRegexCapture()
        {
            var objectId = ObjectId.Random();
            var str = "XYZ" + objectId + "XYZ";

            Assert.AreEqual(objectId, ObjectId.Parse(str, Regex.Match(str, "[a-f0-9]{40}")));
            Assert.Throws<FormatException>(() => ObjectId.Parse(str, Regex.Match(str, "[a-f0-9]{39}")));
            Assert.Throws<FormatException>(() => ObjectId.Parse(str, Regex.Match(str, "[XYZa-f0-9]{39}")));
        }

        [Test]
        public void WorkTreeId_has_expected_value()
        {
            Assert.AreEqual(
                "1111111111111111111111111111111111111111",
                ObjectId.WorkTreeId.ToString());
        }

        [Test]
        public void IndexId_has_expected_value()
        {
            Assert.AreEqual(
                "2222222222222222222222222222222222222222",
                ObjectId.IndexId.ToString());
        }

        [Test]
        public void CombinedDiffId_has_expected_value()
        {
            Assert.AreEqual(
                "3333333333333333333333333333333333333333",
                ObjectId.CombinedDiffId.ToString());
        }

        [Test]
        public void WorkTreeId_is_artificial()
        {
            Assert.IsTrue(ObjectId.WorkTreeId.IsArtificial);
        }

        [Test]
        public void IndexId_is_artificial()
        {
            Assert.IsTrue(ObjectId.IndexId.IsArtificial);
        }

        [Test]
        public void CombinedDiffId_is_artificial()
        {
            Assert.IsTrue(ObjectId.CombinedDiffId.IsArtificial);
        }

        [Test]
        public void Equivalent_ids_are_equal()
        {
            Assert.AreEqual(
                ObjectId.Parse("0102030405060708091011121314151617181920"),
                ObjectId.Parse("0102030405060708091011121314151617181920"));

            Assert.AreEqual(
                ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd"),
                ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd"));

            Assert.AreEqual(
                ObjectId.WorkTreeId,
                ObjectId.WorkTreeId);

            Assert.AreEqual(
                ObjectId.WorkTreeId,
                ObjectId.Parse(GitRevision.WorkTreeGuid));

            Assert.AreEqual(
                ObjectId.IndexId,
                ObjectId.IndexId);

            Assert.AreEqual(
                ObjectId.IndexId,
                ObjectId.Parse(GitRevision.IndexGuid));

            Assert.AreEqual(
                ObjectId.CombinedDiffId,
                ObjectId.CombinedDiffId);

            Assert.AreEqual(
                ObjectId.CombinedDiffId,
                ObjectId.Parse(GitRevision.CombinedDiffGuid));
        }

        [Test]
        public void Different_ids_are_not_equal()
        {
            Assert.AreNotEqual(
                ObjectId.Parse("0000000000000000000000000000000000000000"),
                ObjectId.Parse("0102030405060708091011121314151617181920"));

            Assert.AreNotEqual(
                ObjectId.WorkTreeId,
                ObjectId.IndexId);
        }

        [Test]
        public void Equivalent_ids_have_equal_hash_codes()
        {
            Assert.AreEqual(
                ObjectId.Parse("0102030405060708091011121314151617181920").GetHashCode(),
                ObjectId.Parse("0102030405060708091011121314151617181920").GetHashCode());

            Assert.AreEqual(
                ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd").GetHashCode(),
                ObjectId.Parse("abcdefabcdefabcdefabcdefabcdefabcdefabcd").GetHashCode());

            Assert.AreEqual(
                ObjectId.WorkTreeId.GetHashCode(),
                ObjectId.WorkTreeId.GetHashCode());

            Assert.AreEqual(
                ObjectId.IndexId.GetHashCode(),
                ObjectId.IndexId.GetHashCode());
        }

        [Test]
        public void Different_ids_have_different_hash_codes()
        {
            Assert.AreNotEqual(
                ObjectId.Parse("0000000000000000000000000000000000000000").GetHashCode(),
                ObjectId.Parse("0102030405060708091011121314151617181920").GetHashCode());

            Assert.AreNotEqual(
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
        [TestCase(HexAscii, 27, null)]
        [TestCase(HexAscii, -1, null)]
        [TestCase(NonHexAscii, 0, null)]
        public void TryParseAsciiHexBytes_works_as_expected(string source, int offset, [CanBeNull] string expected)
        {
            var sourceBytes = Encoding.ASCII.GetBytes(source);

            Assert.AreEqual(expected != null, ObjectId.TryParseAsciiHexBytes(sourceBytes, offset, out var id));

            if (expected != null)
            {
                Assert.AreEqual(ObjectId.Parse(expected), id);
            }
        }

        [Test]
        [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
        public void ToShortString()
        {
            const string s = "0102030405060708091011121314151617181920";
            var id = ObjectId.Parse(s);

            for (var length = 0; length < ObjectId.Sha1CharCount; length++)
            {
                Assert.AreEqual(s.Substring(0, length), id.ToShortString(length));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => id.ToShortString(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => id.ToShortString(ObjectId.Sha1CharCount + 1));
        }

        [Test]
        public void Equals_with_string()
        {
            for (var i = 0; i < 100; i++)
            {
                var objectId = ObjectId.Random();
                Assert.True(objectId.Equals(objectId.ToString()));
            }

            Assert.False(ObjectId.Random().Equals((string)null));
            Assert.False(ObjectId.Random().Equals(""));
            Assert.False(ObjectId.Random().Equals("gibberish"));
            Assert.False(ObjectId.Parse("0123456789012345678901234567890123456789").Equals(" 0123456789012345678901234567890123456789 "));
            Assert.False(ObjectId.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa").Equals("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
        }

        [Test]
        public void Equals_using_operator()
        {
            string objectIdString = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            Assert.IsTrue(ObjectId.Parse(objectIdString) == ObjectId.Parse(objectIdString));
            Assert.IsFalse(ObjectId.Parse(objectIdString) != ObjectId.Parse(objectIdString));
            Assert.IsFalse(ObjectId.Parse(objectIdString) == ObjectId.Random());
            Assert.IsTrue(ObjectId.Parse(objectIdString) != ObjectId.Random());
        }
    }
}