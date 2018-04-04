using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class StreamExtensions
    {
        [NotNull]
        [ItemNotNull]
        [MustUseReturnValue]
        public static IEnumerable<byte[]> ReadNullTerminatedChunks([NotNull] this Stream stream, int bufferSize = 4096)
        {
            var buffer = new byte[bufferSize];
            var accumulator = new MemoryStream();

            while (true)
            {
                var bytesRead = stream.Read(buffer, 0, bufferSize);

                if (bytesRead == 0)
                {
                    break;
                }

                var fromIndex = 0;

                while (fromIndex < bytesRead)
                {
                    // TODO is this a slow way of searching? the method boxes the byte value -- what is it doing internally?
                    var nullIndex = Array.IndexOf(buffer, (byte)0, fromIndex, bytesRead - fromIndex);

                    if (nullIndex == -1)
                    {
                        var count = bytesRead - fromIndex;
                        accumulator.Write(buffer, fromIndex, count);
                        break;
                    }

                    if (accumulator.Length == 0)
                    {
                        var count = nullIndex - fromIndex;
                        yield return count == 0
                            ? Array.Empty<byte>()
                            : buffer.Subsequence(fromIndex, count);
                    }
                    else
                    {
                        var count = nullIndex - fromIndex;
                        accumulator.Write(buffer, fromIndex, count);
                        yield return accumulator.ToArray();
                        accumulator.Position = 0;
                        accumulator.SetLength(0);
                    }

                    fromIndex = nullIndex + 1;
                }
            }

            if (accumulator.Length != 0)
            {
                yield return accumulator.ToArray();
            }
        }
    }
}