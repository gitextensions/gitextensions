using System;
using System.Collections.Generic;
using ConEmu.WinForms;
using GitCommands;
using GitUI.UserControls;
using NUnit.Framework;

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

            var filter = new ConsoleCommandLineOutputProcessor(cmd.Length, FireDataReceived);

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

            var filter = new ConsoleCommandLineOutputProcessor(cmd.Length, FireDataReceived);

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
            List<string> outputData = new List<string>();
            outputData.Add(cmd);
            outputData.Add(Environment.NewLine);
            outputData.Add("Receiving: 10%\r");
            outputData.Add("Receiving: 20%\r");
            outputData.Add("Receiving: 30%\rR");
            outputData.Add("eceiving: 40%\r");
            outputData.Add("Receiving: 100%\nReceived data\n");

            List<string> received = new List<string>();

            void FireDataReceived(TextEventArgs e)
            {
                received.Add(e.Text);
            }

            var filter = new ConsoleCommandLineOutputProcessor(cmd.Length, FireDataReceived);

            foreach (string chunk in outputData)
            {
                filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk)));
            }

            filter.Flush();

            List<string> expectedData = new List<string>();
            expectedData.Add("Receiving: 10%\r");
            expectedData.Add("Receiving: 20%\r");
            expectedData.Add("Receiving: 30%\r");
            expectedData.Add("Receiving: 40%\r");
            expectedData.Add("Receiving: 100%\n");
            expectedData.Add("Received data\n");

            CollectionAssert.AreEqual(expectedData, received);
        }

        [Test]
        public void CRLFInOutput()
        {
            string cmd = "\"C:\\Program Files\\Git\\bin\\git.exe\" rebase  -i --autosquash --autostash \"branch_foo\"";
            List<string> outputData = new List<string>();
            outputData.Add(cmd);
            outputData.Add(Environment.NewLine);
            outputData.Add("Receiving: 10%\r");
            outputData.Add("Receiving: 20%\r");
            outputData.Add("Receiving: 30%\rR");
            outputData.Add("eceiving: 40%\r");
            outputData.Add("Receiving: 100%\nReceived\r\ndata\n");

            List<string> received = new List<string>();

            void FireDataReceived(TextEventArgs e)
            {
                received.Add(e.Text);
            }

            var filter = new ConsoleCommandLineOutputProcessor(cmd.Length, FireDataReceived);

            foreach (string chunk in outputData)
            {
                filter.AnsiStreamChunkReceived(null, new AnsiStreamChunkEventArgs(GitModule.SystemEncoding.GetBytes(chunk)));
            }

            filter.Flush();

            List<string> expectedData = new List<string>();
            expectedData.Add("Receiving: 10%\r");
            expectedData.Add("Receiving: 20%\r");
            expectedData.Add("Receiving: 30%\r");
            expectedData.Add("Receiving: 40%\r");
            expectedData.Add("Receiving: 100%\n");
            expectedData.Add("Received\r");
            expectedData.Add("\n");
            expectedData.Add("data\n");

            CollectionAssert.AreEqual(expectedData, received);
        }
    }
}
