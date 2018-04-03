using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace GitCommands
{
    internal static class StreamReaderExtensions
    {
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<string> ReadNullTerminatedLines([NotNull] this StreamReader reader, int bufferSize = 4096)
        {
            var buffer = new char[bufferSize];
            var accumulator = new StringBuilder();

            while (true)
            {
                var charsRead = reader.ReadBlock(buffer, 0, bufferSize);

                if (charsRead == 0)
                {
                    break;
                }

                var fromIndex = 0;

                while (fromIndex < charsRead)
                {
                    var nullIndex = Array.IndexOf(buffer, '\0', fromIndex, charsRead - fromIndex);

                    if (nullIndex == -1)
                    {
                        var count = charsRead - fromIndex;
                        accumulator.Append(buffer, fromIndex, count);
                        break;
                    }

                    if (accumulator.Length == 0)
                    {
                        var count = nullIndex - fromIndex;
                        yield return count == 0 ? "" : new string(buffer, fromIndex, count);
                    }
                    else
                    {
                        var count = nullIndex - fromIndex;
                        accumulator.Append(buffer, fromIndex, count);
                        yield return accumulator.ToString();
                        accumulator.Clear();
                    }

                    fromIndex = nullIndex + 1;
                }
            }

            if (accumulator.Length != 0)
            {
                yield return accumulator.ToString();
            }
        }
    }
}