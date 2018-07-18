using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace GitCommands
{
    public class SynchronizedProcessReader
    {
        public Process Process { get; }
        public byte[] Output { get; private set; }
        public byte[] Error { get; private set; }

        private readonly Thread _stdOutputLoaderThread;
        private readonly Thread _stdErrLoaderThread;

        public SynchronizedProcessReader(Process process)
        {
            Process = process;
            _stdOutputLoaderThread = new Thread(_ => Output = ReadByte(Process.StandardOutput.BaseStream));
            _stdOutputLoaderThread.Start();
            _stdErrLoaderThread = new Thread(_ => Error = ReadByte(Process.StandardError.BaseStream));
            _stdErrLoaderThread.Start();
        }

        public void WaitForExit()
        {
            _stdOutputLoaderThread.Join();
            _stdErrLoaderThread.Join();
            Process.WaitForExit();
        }

        public string OutputString(Encoding encoding)
        {
            return encoding.GetString(Output);
        }

        public string ErrorString(Encoding encoding)
        {
            return encoding.GetString(Error);
        }

        /// <summary>
        /// This function reads the output to a string. This function can be dangerous, because it returns a string
        /// and needs to know the correct encoding. This function should be avoided!
        /// </summary>
        public static void Read(Process process, out string stdOutput, out string stdError)
        {
            string stdOutputLoader = null;

            var stdOutputLoaderThread = new Thread(_ => stdOutputLoader = process.StandardOutput.ReadToEnd());
            stdOutputLoaderThread.Start();

            stdError = process.StandardError.ReadToEnd();

            stdOutputLoaderThread.Join();

            stdOutput = stdOutputLoader;
        }

        /// <summary>
        /// This function reads the output to a byte[]. This function is used because it doesn't need to know the
        /// correct encoding.
        /// </summary>
        public static void ReadBytes(Process process, out byte[] stdOutput, [CanBeNull] out byte[] stdError)
        {
            byte[] stdOutputLoader = null;

            // We cannot use the async functions because these functions will read the output to a string, this
            // can cause problems because the correct encoding is not used.
            var stdOutputLoaderThread = new Thread(_ => stdOutputLoader = ReadByte(process.StandardOutput.BaseStream));
            stdOutputLoaderThread.Start();

            stdError = ReadByte(process.StandardError.BaseStream);

            stdOutputLoaderThread.Join();

            stdOutput = stdOutputLoader;
        }

        [CanBeNull]
        private static byte[] ReadByte(Stream stream)
        {
            if (!stream.CanRead)
            {
                return null;
            }

            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }
}
