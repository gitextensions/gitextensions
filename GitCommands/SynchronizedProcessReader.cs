using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace GitCommands
{
    public class SynchronizedProcessReader
    {
        public Process Process { get; private set; }
        public byte[] Output { get; private set; }
        public byte[] Error { get; private set; }
      
        private readonly Thread stdOutputLoaderThread;
        private readonly Thread stdErrLoaderThread;

        public SynchronizedProcessReader(Process aProcess)
        {
            Process = aProcess;
            stdOutputLoaderThread = new Thread(_ => Output = ReadByte(Process.StandardOutput.BaseStream));
            stdOutputLoaderThread.Start();
            stdErrLoaderThread = new Thread(_ => Error = ReadByte(Process.StandardError.BaseStream));
            stdErrLoaderThread.Start();  
        }

        public void WaitForExit()
        {
            stdOutputLoaderThread.Join();
            stdErrLoaderThread.Join();              
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

            Thread stdOutputLoaderThread = new Thread(_ => stdOutputLoader = process.StandardOutput.ReadToEnd());
            stdOutputLoaderThread.Start();

            stdError = process.StandardError.ReadToEnd();

            stdOutputLoaderThread.Join();

            stdOutput = stdOutputLoader;
        }

        /// <summary>
        /// This function reads the output to a byte[]. This function is used because it doesn't need to know the
        /// correct encoding.
        /// </summary>
        public static void ReadBytes(Process process, out byte[] stdOutput, out byte[] stdError)
        {
            byte[] stdOutputLoader = null;

            //We cannot use the async functions because these functions will read the output to a string, this
            //can cause problems because the correct encoding is not used.
            Thread stdOutputLoaderThread = new Thread(_ => stdOutputLoader = ReadByte(process.StandardOutput.BaseStream));
            stdOutputLoaderThread.Start();

            stdError = ReadByte(process.StandardError.BaseStream);

            stdOutputLoaderThread.Join();

            stdOutput = stdOutputLoader;
        }

        private static byte[] ReadByte(Stream stream)
        {
            if (!stream.CanRead)
            {
                return null;
            }
            int commonLen = 0;
            List<byte[]> list = new List<byte[]>();
            byte[] buffer = new byte[4096];
            int len;
            while ((len = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                byte[] newbuff = new byte[len];
                Array.Copy(buffer, newbuff, len);
                commonLen += len;
                list.Add(newbuff);
            }
            buffer = new byte[commonLen];
            commonLen = 0;
            for (int i = 0; i < list.Count; i++)
            {
                Array.Copy(list[i], 0, buffer, commonLen, list[i].Length);
                commonLen += list[i].Length;
            }
            return buffer;
        }
    }
}
