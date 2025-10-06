using System.Text;
using GitCommands;

namespace GitCommandsTests.Git;

[TestFixture]
public class EncodingHelperTest
{
    #region Unit tests of single methods

    [Test]
    public void GetStringEncodingNull()
    {
        ClassicAssert.Throws<ArgumentNullException>(() => EncodingHelper.GetString(new byte[] { 0x30 }, new byte[] { 0x31 }, null));
    }

    [Test]
    public void GetStringTestSingleLineOutput()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        string getString = EncodingHelper.GetString(testBytes, null, Encoding.ASCII);

        ClassicAssert.AreEqual(asciiString, getString);
    }

    [Test]
    public void GetStringTestSingleLineError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        string getString = EncodingHelper.GetString(null, testBytes, Encoding.ASCII);

        ClassicAssert.AreEqual(asciiString, getString);
    }

    [Test]
    public void GetStringTestOutputAndError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytesOutput = new ASCIIEncoding().GetBytes(asciiString);
        byte[] testBytesError = new ASCIIEncoding().GetBytes(asciiString.ToUpper());

        string getString = EncodingHelper.GetString(testBytesOutput, testBytesError, Encoding.ASCII);

        ClassicAssert.AreEqual(asciiString + Environment.NewLine + asciiString.ToUpper(), getString);
    }

    [Test]
    public void GetStringTestUtf8()
    {
        byte[] testBytes = new byte[4];
        testBytes[0] = 0xE3;
        testBytes[1] = 0x82;
        testBytes[2] = 0xA2;

        string utf8String = new UTF8Encoding().GetString(testBytes);

        string getString = EncodingHelper.GetString(null, testBytes, Encoding.UTF8);

        ClassicAssert.AreEqual(utf8String, getString);
    }

    [Test]
    public void ConvertToTestEncodingNull()
    {
        ClassicAssert.Throws<ArgumentNullException>(() => EncodingHelper.ConvertTo(null, "abcd"));
    }

    [Test]
    public void ConvertToTestUtf8()
    {
        string unicodeString = "\u30a2\u30c3";
        byte[] convertedBytes = EncodingHelper.ConvertTo(Encoding.UTF8, unicodeString);

        ClassicAssert.AreEqual(convertedBytes.Length, 6);
        ClassicAssert.AreEqual(convertedBytes[0], 0xE3);
        ClassicAssert.AreEqual(convertedBytes[1], 0x82);
        ClassicAssert.AreEqual(convertedBytes[2], 0xA2);
        ClassicAssert.AreEqual(convertedBytes[3], 0xE3);
        ClassicAssert.AreEqual(convertedBytes[4], 0x83);
        ClassicAssert.AreEqual(convertedBytes[5], 0x83);
    }

    [Test]
    public void ConvertToTestUtf16()
    {
        string unicodeString = "\u30a2\u30c3";
        byte[] convertedBytes = EncodingHelper.ConvertTo(Encoding.Unicode, unicodeString);

        ClassicAssert.AreEqual(convertedBytes.Length, 4);
        ClassicAssert.AreEqual(convertedBytes[0], 0xA2);
        ClassicAssert.AreEqual(convertedBytes[1], 0x30);
        ClassicAssert.AreEqual(convertedBytes[2], 0xC3);
        ClassicAssert.AreEqual(convertedBytes[3], 0x30);
    }

    [Test]
    public void DecodeStringTestEncodingNull()
    {
        Encoding enc = null;
        ClassicAssert.Throws<ArgumentNullException>(() => EncodingHelper.DecodeString(new byte[] { 0x30 }, new byte[] { 0x31 }, ref enc));
    }

    [Test]
    public void DecodeStringTestSingleLineOutput()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        Encoding enc = new ASCIIEncoding();

        string decodedString = EncodingHelper.DecodeString(testBytes, null, ref enc);

        ClassicAssert.AreEqual(asciiString, decodedString);
    }

    [Test]
    public void DecodeStringTestSingleLineError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        Encoding enc = new ASCIIEncoding();

        string decodedString = EncodingHelper.DecodeString(null, testBytes, ref enc);

        ClassicAssert.AreEqual(asciiString, decodedString);
    }

    [Test]
    public void DecodeStringTestOutputAndError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytesOutput = new ASCIIEncoding().GetBytes(asciiString);
        byte[] testBytesError = new ASCIIEncoding().GetBytes(asciiString.ToUpper());

        Encoding enc = new ASCIIEncoding();

        string decodedString = EncodingHelper.DecodeString(testBytesOutput, testBytesError, ref enc);

        ClassicAssert.AreEqual(asciiString + Environment.NewLine + asciiString.ToUpper(), decodedString);
    }

    [Test]
    public void DecodeStringTestUtf8()
    {
        byte[] testBytes = new byte[4];
        testBytes[0] = 0xE3;
        testBytes[1] = 0x82;
        testBytes[2] = 0xA2;

        Encoding enc = new UTF8Encoding();

        string utf8String = new UTF8Encoding().GetString(testBytes);

        string decodedString = EncodingHelper.DecodeString(null, testBytes, ref enc);

        ClassicAssert.AreEqual(utf8String, decodedString);
        ClassicAssert.AreEqual(new UTF8Encoding(), enc);
    }

    // Insert a Test here which checks whether EncodingHelper.DecodeString
    // detects correct encoding (StreamReader.CurrentEncoding).
    // Couldn't find a test so far where StreamReader.CurrentEncoding is different than given Encoding

    #endregion
}
