using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class StreamExtensions
    {
        [NotNull]
        [MustUseReturnValue]
        public static IEnumerable<ArraySegment<byte>> ReadNullTerminatedChunks([NotNull] this Stream stream, ref byte[] buffer)
        {
            // Work around generator functions and ref parameters
            var buf = buffer;
            var chunks = Run();
            buffer = buf;
            return chunks;

            IEnumerable<ArraySegment<byte>> Run()
            {
                var yieldFromIndex = 0;
                var writeToIndex = 0;

                while (true)
                {
                    // Fill the buffer with data
                    var bytesRead = stream.Read(buf, writeToIndex, buf.Length - writeToIndex);

                    if (bytesRead == 0)
                    {
                        if (writeToIndex != 0 && writeToIndex != yieldFromIndex)
                        {
                            yield return new ArraySegment<byte>(buf, yieldFromIndex, writeToIndex - yieldFromIndex);
                        }

                        yield break;
                    }

                    var dataUntilIndex = writeToIndex + bytesRead;

                    var searchFromIndex = writeToIndex;
                    writeToIndex += bytesRead;
                    var searchBeforeIndex = writeToIndex;

                    while (searchFromIndex < searchBeforeIndex)
                    {
                        var nullIndex = Array.IndexOf<byte>(buf, 0, searchFromIndex, searchBeforeIndex - searchFromIndex);

                        if (nullIndex == -1)
                        {
                            // We didn't find a null in the data we have available to process
                            if (searchBeforeIndex == buf.Length)
                            {
                                // We are at the end of the buffer
                                if (yieldFromIndex != 0)
                                {
                                    // There is free space earlier in the buffer, so shift this chunk to the beginning

                                    var unprocessedByteCount = buf.Length - yieldFromIndex;

                                    // Move unprocessed bytes from the end to the start of the buffer.
                                    Array.Copy(buf, yieldFromIndex, buf, 0, unprocessedByteCount);

                                    // Restart loop
                                    yieldFromIndex = 0;
                                    writeToIndex = unprocessedByteCount;
                                }
                                else
                                {
                                    // The buffer is full, with no nulls, so allocate a larger buffer

                                    // Allocate new buffer with twice the size
                                    var newBuffer = new byte[buf.Length * 2];

                                    // Copy old to new buffers
                                    Array.Copy(buf, newBuffer, buf.Length);

                                    // Restart loop
                                    writeToIndex = buf.Length;

                                    // Writeback
                                    buf = newBuffer;
                                }
                            }

                            break;
                        }

                        // yield any inner chunks
                        yield return new ArraySegment<byte>(buf, yieldFromIndex, nullIndex - yieldFromIndex);

                        if (nullIndex + 1 == dataUntilIndex)
                        {
                            yieldFromIndex = writeToIndex = 0;
                            break;
                        }
                        else
                        {
                            searchFromIndex = yieldFromIndex = nullIndex + 1;
                        }
                    }
                }
            }
        }

        [NotNull]
        [MustUseReturnValue]
        public static byte[] ReadAllBytes([NotNull] this Stream stream)
        {
            // NOTE no need to dispose MemoryStream
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}