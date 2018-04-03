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
            var incompleteBlock = new StringBuilder();

            while (true)
            {
                int bytesRead = reader.ReadBlock(buffer, 0, bufferSize);

                if (bytesRead == 0)
                {
                    break;
                }

                string bufferString = new string(buffer, 0, bytesRead);
                string[] dataBlocks = bufferString.Split('\0');

                if (dataBlocks.Length > 1)
                {
                    // There are at least two blocks, so we can return the first one
                    incompleteBlock.Append(dataBlocks[0]);
                    yield return incompleteBlock.ToString();
                    incompleteBlock.Clear();
                }

                int lastDataBlockIndex = dataBlocks.Length - 1;

                // Return all the blocks until the last one
                for (int i = 1; i < lastDataBlockIndex; i++)
                {
                    yield return dataBlocks[i];
                }

                // Append the beginning of the last block
                incompleteBlock.Append(dataBlocks[lastDataBlockIndex]);
            }

            if (incompleteBlock.Length > 0)
            {
                yield return incompleteBlock.ToString();
            }
        }
    }
}