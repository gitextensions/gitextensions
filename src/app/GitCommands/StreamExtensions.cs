using JetBrains.Annotations;

namespace GitCommands
{
    public static class StreamExtensions
    {
        [MustUseReturnValue]
        public static IEnumerable<ReadOnlyMemory<byte>> SplitLogOutput(this Stream stream, int initialBufferSize = 4096)
        {
            byte[] buffer = new byte[initialBufferSize];

            int yieldFromIndex = 0;
            int writeToIndex = 0;

            while (true)
            {
                // Fill the buffer with data
                int bytesRead = stream.Read(buffer, writeToIndex, buffer.Length - writeToIndex);

                if (bytesRead == 0)
                {
                    // End of stream, just yield the remaining data (no null terminator for last commit)
                    if (writeToIndex != 0 && writeToIndex != yieldFromIndex)
                    {
                        yield return new ReadOnlyMemory<byte>(buffer, yieldFromIndex, writeToIndex - yieldFromIndex);
                    }

                    yield break;
                }

                int dataUntilIndex = writeToIndex + bytesRead;

                int searchFromIndex = writeToIndex;
                writeToIndex += bytesRead;
                int searchBeforeIndex = writeToIndex;

                while (searchFromIndex < searchBeforeIndex)
                {
                    int nullIndex = Array.IndexOf<byte>(buffer, 0, searchFromIndex, searchBeforeIndex - searchFromIndex);

                    if (nullIndex == -1)
                    {
                        // null not found in the data available to process
                        if (searchBeforeIndex == buffer.Length)
                        {
                            // end of the buffer
                            if (yieldFromIndex != 0)
                            {
                                // Shift unprocessed to the beginning
                                int unprocessedByteCount = buffer.Length - yieldFromIndex;
                                Array.Copy(buffer, yieldFromIndex, buffer, 0, unprocessedByteCount);

                                // Restart loop
                                yieldFromIndex = 0;
                                writeToIndex = unprocessedByteCount;
                            }
                            else
                            {
                                // The buffer is full, with no nulls, so allocate a larger buffer

                                // Allocate new buffer with twice the size and copy
                                byte[] newBuffer = new byte[buffer.Length * 2];
                                Array.Copy(buffer, newBuffer, buffer.Length);

                                // Restart loop (yieldFromIndex is unchanged)
                                writeToIndex = buffer.Length;

                                // Writeback
                                buffer = newBuffer;
                            }
                        }

                        break;
                    }

                    // yield any inner chunks
                    yield return new ReadOnlyMemory<byte>(buffer, yieldFromIndex, nullIndex - yieldFromIndex);

                    if (nullIndex + 1 == dataUntilIndex)
                    {
                        // all data in the buffer processed
                        yieldFromIndex = writeToIndex = 0;
                        break;
                    }

                    // process the remaining (possibly partial) commits
                    searchFromIndex = yieldFromIndex = nullIndex + 1;
                }
            }
        }
    }
}
