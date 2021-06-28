using System.IO;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class StreamExtensionsTests
    {
        private const byte nil = 0;

        [TestCase(
            new byte[] { },
            false)]
        [TestCase(
            new byte[] { 1 },
            false,
            new byte[] { 1 })]
        [TestCase(
            new byte[] { nil },
            false,
            new byte[0])]
        [TestCase(
            new byte[] { nil, nil },
            false,
            new byte[0], new byte[0])]
        [TestCase(
            new byte[] { 1, 2, 3, 4, 5, 6 },
            false,
            new byte[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase(
            new byte[] { 2, 3, 4, 5, 6, nil },
            false,
            new byte[] { 2, 3, 4, 5, 6 })]
        [TestCase(
            new byte[] { nil, 1, 2, 3, 4, 5, 6 },
            false,
            new byte[0], new byte[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase(
            new byte[] { 1, 2, 3, nil, 4, 5, 6 },
            false,
            new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 })]
        [TestCase(
            new byte[] { 1, 2, 3, nil, nil, 4, 5, 6 },
            false,
            new byte[] { 1, 2, 3 }, new byte[0], new byte[] { 4, 5, 6 })]
        [TestCase(
            new byte[] { 1, 2, 3, nil, 4, 5, 6, nil, 7, 8, 9 },
            false,
            new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, new byte[] { 7, 8, 9 })]
        [TestCase(
            new byte[] { nil, 1, nil, 2, 3, nil, 4, 5, 6, nil, 7, 8, 9, 10 },
            false,
            new byte[0], new byte[] { 1 }, new byte[] { 2, 3 }, new byte[] { 4, 5, 6 }, new byte[] { 7, 8, 9, 10 })]
        [TestCase(
            new byte[] { nil, 1, nil, 2, 3, nil, nil, 4, 5, 6, nil, 7, 8, 9, 10 },
            true,
            new byte[] { nil, 1, nil, 2, 3, nil }, new byte[] { 4, 5, 6, nil, 7, 8, 9, 10 })]
        [TestCase(
            new byte[] { nil, nil, 1, nil, 2, 3, nil, nil, 4, 5, 6, nil, 7, 8, 9, 10 },
            true,
            new byte[] { nil }, new byte[] { 1, nil, 2, 3, nil }, new byte[] { 4, 5, 6, nil, 7, 8, 9, 10 })]
        public void ReadNullTerminatedLines(byte[] input, bool useDoubleNull, params byte[][] expectedChunks)
        {
            MemoryStream stream = new(input);

            // Run the test at multiple buffer sizes to test boundary conditions thoroughly
            for (var bufferSize = 1; bufferSize < input.Length + 2; bufferSize++)
            {
                // Test overload that uses external buffer
                var buffer = new byte[bufferSize];

                stream.Position = 0;

                using var e = stream.ReadNullTerminatedChunks(ref buffer, useDoubleNull).GetEnumerator();
                for (var chunkIndex = 0; chunkIndex < expectedChunks.Length; chunkIndex++)
                {
                    var expected = expectedChunks[chunkIndex];
                    Assert.IsTrue(e.MoveNext());
                    Assert.AreEqual(
                        expected,
                        e.Current.ToArray(),
                        "input=[{0}] chunkIndex={1} bufferSize={2}", string.Join(",", expected), chunkIndex, bufferSize);
                }

                Assert.IsFalse(e.MoveNext(), "bufferSize={0}", bufferSize);
            }
        }
    }
}
