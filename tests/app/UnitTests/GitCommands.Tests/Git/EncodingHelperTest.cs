using System.Text;
using GitCommands;

namespace GitCommandsTests.Git;
public class EncodingHelperTest
{
    #region Unit tests of single methods

    [Test]
    public void GetStringEncodingNull()
    {
        ((Action)(() => EncodingHelper.GetString([0x30], [0x31], null!))).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetStringTestSingleLineOutput()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        string getString = EncodingHelper.GetString(testBytes, null, Encoding.ASCII);

        getString.Should().Be(asciiString);
    }

    [Test]
    public void GetStringTestSingleLineError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        string getString = EncodingHelper.GetString(null, testBytes, Encoding.ASCII);

        getString.Should().Be(asciiString);
    }

    [Test]
    public void GetStringTestOutputAndError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytesOutput = new ASCIIEncoding().GetBytes(asciiString);
        byte[] testBytesError = new ASCIIEncoding().GetBytes(asciiString.ToUpper());

        string getString = EncodingHelper.GetString(testBytesOutput, testBytesError, Encoding.ASCII);

        getString.Should().Be(asciiString + Environment.NewLine + asciiString.ToUpper());
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

        getString.Should().Be(utf8String);
    }

    [Test]
    public void ConvertToTestEncodingNull()
    {
        ((Action)(() => EncodingHelper.ConvertTo(null!, "abcd"))).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ConvertToTestUtf8()
    {
        string unicodeString = "\u30a2\u30c3";
        byte[] convertedBytes = EncodingHelper.ConvertTo(Encoding.UTF8, unicodeString);

        6.Should().Be(convertedBytes.Length);
        0xE3.Should().Be(convertedBytes[0]);
        0x82.Should().Be(convertedBytes[1]);
        0xA2.Should().Be(convertedBytes[2]);
        0xE3.Should().Be(convertedBytes[3]);
        0x83.Should().Be(convertedBytes[4]);
        0x83.Should().Be(convertedBytes[5]);
    }

    [Test]
    public void ConvertToTestUtf16()
    {
        string unicodeString = "\u30a2\u30c3";
        byte[] convertedBytes = EncodingHelper.ConvertTo(Encoding.Unicode, unicodeString);

        4.Should().Be(convertedBytes.Length);
        0xA2.Should().Be(convertedBytes[0]);
        0x30.Should().Be(convertedBytes[1]);
        0xC3.Should().Be(convertedBytes[2]);
        0x30.Should().Be(convertedBytes[3]);
    }

    [Test]
    public void DecodeStringTestEncodingNull()
    {
        Encoding? enc = null;
        ((Action)(() => EncodingHelper.DecodeString([0x30], [0x31], ref enc!))).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void DecodeStringTestSingleLineOutput()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        Encoding enc = new ASCIIEncoding();

        string decodedString = EncodingHelper.DecodeString(testBytes, null, ref enc);

        decodedString.Should().Be(asciiString);
    }

    [Test]
    public void DecodeStringTestSingleLineError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

        Encoding enc = new ASCIIEncoding();

        string decodedString = EncodingHelper.DecodeString(null, testBytes, ref enc);

        decodedString.Should().Be(asciiString);
    }

    [Test]
    public void DecodeStringTestOutputAndError()
    {
        string asciiString = "abcdefghijklmnop";
        byte[] testBytesOutput = new ASCIIEncoding().GetBytes(asciiString);
        byte[] testBytesError = new ASCIIEncoding().GetBytes(asciiString.ToUpper());

        Encoding enc = new ASCIIEncoding();

        string decodedString = EncodingHelper.DecodeString(testBytesOutput, testBytesError, ref enc);

        decodedString.Should().Be(asciiString + Environment.NewLine + asciiString.ToUpper());
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

        decodedString.Should().Be(utf8String);
        enc.Should().Be(new UTF8Encoding());
    }

    // Insert a Test here which checks whether EncodingHelper.DecodeString
    // detects correct encoding (StreamReader.CurrentEncoding).
    // Couldn't find a test so far where StreamReader.CurrentEncoding is different than given Encoding

    #endregion
}
