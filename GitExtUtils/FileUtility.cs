using System.IO;
using System.Text;

namespace GitExtUtils
{
    public static class FileUtility
    {
        /// <summary>
        /// Writes all text to a file. Works around issues with hidden files encountered by File.WriteAllText.
        /// </summary>
        /// <param name="fileName">Destination file.</param>
        /// <param name="contents">Text to write as file contents.</param>
        public static void SafeWriteAllText(string fileName, string contents, Encoding encoding)
        {
            using FileStream fs = new(fileName, FileMode.Open);
            using (TextWriter tw = new StreamWriter(fs, encoding, bufferSize: 4096, leaveOpen: true))
            {
                tw.Write(contents);
            }

            // after flushing, set the stream length to the current position in order to truncate leftover text
            fs.SetLength(fs.Position);
        }
    }
}
