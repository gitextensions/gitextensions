﻿using ConEmu.WinForms;
using GitCommands;
using GitUI.UserControls;

namespace GitUITests.UserControls
{
    [TestFixture]
    public class ConsoleEmulatorOutputControlFixture
    {
        [Test]
        public void FilterOutConsoleCommandLine_NoFlush()
        {
            string cmd = "\"C:\\Program Files\\Git\\bin\\git.exe\" rebase  -i --autosquash --autostash \"branch_foo\"";
            string outputData = "output data";
            string received = string.Empty;

            void FireDataReceived(TextEventArgs e)
            {
                received += e.Text;
            }

            ConsoleCommandLineOutputProcessor filter = new(cmd.Length, FireDataReceived);

            string chunk1 = cmd.Substring(0, 10);
            string chunk2 = cmd.Substring(10, cmd.Length - 10) + Environment.NewLine + outputData;
            filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk1)));
            filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk2)));

            Assert.AreEqual(string.Empty, received);
        }

        [Test]
        public void FilterOutConsoleCommandLine_Flush()
        {
            string cmd = "\"C:\\Program Files\\Git\\bin\\git.exe\" rebase  -i --autosquash --autostash \"branch_foo\"";
            string outputData = "output data";
            string received = string.Empty;

            void FireDataReceived(TextEventArgs e)
            {
                received += e.Text;
            }

            ConsoleCommandLineOutputProcessor filter = new(cmd.Length, FireDataReceived);

            string chunk1 = cmd.Substring(0, 10);
            string chunk2 = cmd.Substring(10, cmd.Length - 10) + Environment.NewLine + outputData;
            filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk1)));
            filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk2)));
            filter.Flush();

            Assert.AreEqual(outputData, received);
        }

        [Test]
        public void PercentageOutput()
        {
            string cmd = "\"C:\\Program Files\\Git\\bin\\git.exe\" rebase  -i --autosquash --autostash \"branch_foo\"";

            var outputData = new[]
            {
                cmd,
                Environment.NewLine,
                "Receiving: 10%\r",
                "Receiving: 20%\r",
                "Receiving: 30%\rR",
                "eceiving: 40%\r",
                "Receiving: 100%\nReceived data\n"
            };

            List<string> received = new();

            void FireDataReceived(TextEventArgs e)
            {
                received.Add(e.Text);
            }

            ConsoleCommandLineOutputProcessor filter = new(cmd.Length, FireDataReceived);

            foreach (string chunk in outputData)
            {
                filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk)));
            }

            filter.Flush();

            var expectedData = new[]
            {
                "Receiving: 10%\r",
                "Receiving: 20%\r",
                "Receiving: 30%\r",
                "Receiving: 40%\r",
                "Receiving: 100%\n",
                "Received data\n"
            };

            CollectionAssert.AreEqual(expectedData, received);
        }

        [Test]
        public void CRLFInOutput()
        {
            string cmd = "\"C:\\Program Files\\Git\\bin\\git.exe\" rebase  -i --autosquash --autostash \"branch_foo\"";

            var outputData = new[]
            {
                cmd,
                Environment.NewLine,
                "Receiving: 10%\r",
                "Receiving: 20%\r",
                "Receiving: 30%\rR",
                "eceiving: 40%\r",
                "Receiving: 100%\nReceived\r\ndata\n"
            };

            List<string> received = new();

            void FireDataReceived(TextEventArgs e)
            {
                received.Add(e.Text);
            }

            ConsoleCommandLineOutputProcessor filter = new(cmd.Length, FireDataReceived);

            foreach (string chunk in outputData)
            {
                filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk)));
            }

            filter.Flush();

            var expectedData = new[]
            {
                "Receiving: 10%\r",
                "Receiving: 20%\r",
                "Receiving: 30%\r",
                "Receiving: 40%\r",
                "Receiving: 100%\n",
                "Received\r",
                "\n",
                "data\n"
            };

            CollectionAssert.AreEqual(expectedData, received);
        }
    }
}
