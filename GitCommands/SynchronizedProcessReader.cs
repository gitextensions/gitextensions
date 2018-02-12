using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GitCommands
{
    public class SynchronizedProcessReader
    {
        public Process Process { get; }
        public byte[] Output { get; private set; }
        public byte[] Error { get; private set; }

        private readonly Task stdOutReadTask;
        private readonly Task stdErrReadTask;

        public SynchronizedProcessReader(Process process)
        {
            Process = process;
            stdOutReadTask = Task.Run(async () => Output = await process.StandardOutput.BaseStream.ReadToEndAsync());
            stdErrReadTask = Task.Run(async () => Error = await process.StandardError.BaseStream.ReadToEndAsync());
        }

        public void WaitForExit()
        {
            stdOutReadTask.Wait();
            stdErrReadTask.Wait();
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
        /// Reads string data written by <paramref name="process"/> to both <see cref="System.Diagnostics.Process.StandardOutput"/>
        /// and <see cref="System.Diagnostics.Process.StandardError"/>.
        /// </summary>
        /// <remarks>
        /// This method uses the <see cref="Encoding"/> specified in the <see cref="ProcessStartInfo"/> used
        /// to create <paramref name="process"/>.
        /// <para />
        /// If raw byte streams are required, use <see cref="ReadBytes"/> instead.
        /// </remarks>
        public static void Read(Process process, out string stdOutput, out string stdError)
        {
            var stdOutTask = Task.Run(async () => await process.StandardOutput.ReadToEndAsync());
            var stdErrTask = Task.Run(async () => await process.StandardError.ReadToEndAsync());

            stdOutput = stdOutTask.GetAwaiter().GetResult();
            stdError = stdErrTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reads bytes written by <paramref name="process"/> to both <see cref="System.Diagnostics.Process.StandardOutput"/>
        /// and <see cref="System.Diagnostics.Process.StandardError"/>.
        /// </summary>
        /// <remarks>
        /// As this method returns byte data, it may later be interpreted using whatever <see cref="Encoding"/>
        /// is required.
        /// <para />
        /// To use the process's default encoding, use <see cref="Read"/> instead.
        /// </remarks>
        public static void ReadBytes(Process process, out byte[] stdOutput, out byte[] stdError)
        {
            var stdOutTask = Task.Run(async () => await process.StandardOutput.BaseStream.ReadToEndAsync());
            var stdErrTask = Task.Run(async () => await process.StandardError.BaseStream.ReadToEndAsync());

            stdOutput = stdOutTask.GetAwaiter().GetResult();
            stdError = stdErrTask.GetAwaiter().GetResult();
        }
    }

    internal static class StreamExtensions
    {
        public static async Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            if (!stream.CanRead)
            {
                return null;
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                await stream.CopyToAsync(memStream);
                return memStream.ToArray();
            }
        }
    }
}
