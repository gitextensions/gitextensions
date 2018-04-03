using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class StreamReaderExtensionsTests
    {
        [TestCase("", new string[0])]
        [TestCase("a", new[] { "a" })]
        [TestCase("\0", new[] { "" })]
        [TestCase("\0\0", new[] { "", "" })]
        [TestCase("abcdef", new[] { "abcdef" })]
        [TestCase("abcdef\0", new[] { "abcdef" })]
        [TestCase("\0abcdef", new[] { "", "abcdef" })]
        [TestCase("abc\0def", new[] { "abc", "def" })]
        [TestCase("abc\0\0def", new[] { "abc", "", "def" })]
        [TestCase("abc\0def\0ghi", new[] { "abc", "def", "ghi" })]
        [TestCase("abc\0def\0ghi", new[] { "abc", "def", "ghi" })]
        [TestCase("\0a\0bc\0def\0ghij", new[] { "", "a", "bc", "def", "ghij" })]
        public void ReadNullTerminatedLines(string input, string[] expected)
        {
            var stream = new MemoryStream();

            var inputBufferSize = (input.Length * 2) + 1;

            using (var writer = new StreamWriter(stream, Encoding.UTF8, inputBufferSize, leaveOpen: true))
            {
                writer.Write(input);
            }

            // Run the test at multiple buffer sizes to test boundary conditions thoroughly
            for (var bufferSize = 1; bufferSize < input.Length + 2; bufferSize++)
            {
                stream.Position = 0;

                using (var reader = new StreamReader(stream, Encoding.UTF8, false, inputBufferSize, leaveOpen: true))
                {
                    var actual = reader.ReadNullTerminatedLines(bufferSize).ToArray();
                    Assert.AreEqual(expected, actual, $"bufferSize={bufferSize}");
                }
            }
        }
    }
}