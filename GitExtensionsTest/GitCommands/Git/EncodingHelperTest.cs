using System;
using System.Text;
using GitCommands;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest.Git
{
    [TestClass]
    public class EncodingHelperTest
    {
        #region Unit tests of single methods

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetStringEncodingNull()
        {
            EncodingHelper.GetString(new byte[] { 0x30 }, new byte[] { 0x31 }, null);
        }

        [TestMethod]
        public void GetStringTestSingleLineOutput()
        {
            string asciiString = "abcdefghijklmnop";
            byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

            string getString = EncodingHelper.GetString(testBytes, null, Encoding.ASCII);

            Assert.AreEqual(asciiString, getString);
        }

        [TestMethod]
        public void GetStringTestSingleLineError()
        {
            string asciiString = "abcdefghijklmnop";
            byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

            string getString = EncodingHelper.GetString(null, testBytes, Encoding.ASCII);

            Assert.AreEqual(asciiString, getString);
        }

        [TestMethod]
        public void GetStringTestOutputAndError()
        {
            string asciiString = "abcdefghijklmnop";
            byte[] testBytesOutput = new ASCIIEncoding().GetBytes(asciiString);
            byte[] testBytesError = new ASCIIEncoding().GetBytes(asciiString.ToUpper());

            string getString = EncodingHelper.GetString(testBytesOutput, testBytesError, Encoding.ASCII);

            Assert.AreEqual(asciiString + Environment.NewLine + asciiString.ToUpper(), getString);
        }

        [TestMethod]
        public void GetStringTestUtf8()
        {
            byte[] testBytes = new byte[4];
            testBytes[0] = 0xE3;
            testBytes[1] = 0x82;
            testBytes[2] = 0xA2;

            string utf8String = new UTF8Encoding().GetString(testBytes);

            string getString = EncodingHelper.GetString(null, testBytes, Encoding.UTF8);

            Assert.AreEqual(utf8String, getString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertToTestEncodingNull()
        {
            EncodingHelper.ConvertTo(null, "abcd");
        }

        [TestMethod]
        public void ConvertToTestUtf8()
        {
            string unicodeString = "\u30a2\u30c3";
            byte[] convertedBytes = EncodingHelper.ConvertTo(Encoding.UTF8, unicodeString);

            Assert.AreEqual(convertedBytes.Length, 6);
            Assert.AreEqual(convertedBytes[0], 0xE3);
            Assert.AreEqual(convertedBytes[1], 0x82);
            Assert.AreEqual(convertedBytes[2], 0xA2);
            Assert.AreEqual(convertedBytes[3], 0xE3);
            Assert.AreEqual(convertedBytes[4], 0x83);
            Assert.AreEqual(convertedBytes[5], 0x83);
        }

        [TestMethod]
        public void ConvertToTestUtf16()
        {
            string unicodeString = "\u30a2\u30c3";
            byte[] convertedBytes = EncodingHelper.ConvertTo(Encoding.Unicode, unicodeString);

            Assert.AreEqual(convertedBytes.Length, 4);
            Assert.AreEqual(convertedBytes[0], 0xA2);
            Assert.AreEqual(convertedBytes[1], 0x30);
            Assert.AreEqual(convertedBytes[2], 0xC3);
            Assert.AreEqual(convertedBytes[3], 0x30);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DecodeStringTestEncodingNull()
        {
            Encoding enc = null;
            EncodingHelper.DecodeString(new byte[] { 0x30 }, new byte[] { 0x31 }, ref enc);
        }

        [TestMethod]
        public void DecodeStringTestSingleLineOutput()
        {
            string asciiString = "abcdefghijklmnop";
            byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

            Encoding enc = new ASCIIEncoding();

            string decodedString = EncodingHelper.DecodeString(testBytes, null, ref enc);

            Assert.AreEqual(asciiString, decodedString);
        }

        [TestMethod]
        public void DecodeStringTestSingleLineError()
        {
            string asciiString = "abcdefghijklmnop";
            byte[] testBytes = new ASCIIEncoding().GetBytes(asciiString);

            Encoding enc = new ASCIIEncoding();

            string decodedString = EncodingHelper.DecodeString(null, testBytes, ref enc);

            Assert.AreEqual(asciiString, decodedString);
        }

        [TestMethod]
        public void DecodeStringTestOutputAndError()
        {
            string asciiString = "abcdefghijklmnop";
            byte[] testBytesOutput = new ASCIIEncoding().GetBytes(asciiString);
            byte[] testBytesError = new ASCIIEncoding().GetBytes(asciiString.ToUpper());

            Encoding enc = new ASCIIEncoding();

            string decodedString = EncodingHelper.DecodeString(testBytesOutput, testBytesError, ref enc);

            Assert.AreEqual(asciiString + Environment.NewLine + asciiString.ToUpper(), decodedString);
        }

        [TestMethod]
        public void DecodeStringTestUtf8()
        {
            byte[] testBytes = new byte[4];
            testBytes[0] = 0xE3;
            testBytes[1] = 0x82;
            testBytes[2] = 0xA2;

            Encoding enc = new UTF8Encoding();

            string utf8String = new UTF8Encoding().GetString(testBytes);

            string decodedString = EncodingHelper.DecodeString(null, testBytes, ref enc);

            Assert.AreEqual(utf8String, decodedString);
            Assert.AreEqual(new UTF8Encoding(), enc);
        }

        // Insert a Test here which checks whether EncodingHelper.DecodeString 
        // detects correct encoding (StreamReader.CurrentEncoding).
        // Couldn't find a test so far where StreamReader.CurrentEncoding is different than given Encoding

        #endregion

    }
}
