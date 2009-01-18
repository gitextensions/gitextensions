using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace GitUI
{
    delegate void DataCallback(string text);
    public partial class FormProcess : Form
    {
        public FormProcess(string process, string arguments)
        {
            InitializeComponent();

            ProcessString = process;
            ProcessArguments = arguments;

            ShowDialog();
        }

        public FormProcess(string arguments)
        {
            InitializeComponent();

            ProcessString = GitCommands.Settings.GitDir + "git.cmd";
            ProcessArguments = arguments;

            ShowDialog();
        }

        public string ProcessString { get; set; }
        public string ProcessArguments { get; set; }
        public Process Process { get; set; }
        private GitCommands.GitCommands gitCommand;

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormProcess_Load(object sender, EventArgs e)
        {
            Output.Text = "";
            AddOutput(ProcessString + " " + ProcessArguments);

            ProgressBar.Visible = true;
            
            outputString = new StringBuilder();

            gitCommand = new GitCommands.GitCommands();
            gitCommand.CollectOutput = false;
            gitCommand.CmdStartProcess(ProcessString, ProcessArguments);

            gitCommand.Exited += new EventHandler(gitCommand_Exited);
            gitCommand.DataReceived += new DataReceivedEventHandler(gitCommand_DataReceived);

            Ok.Enabled = false;
        }


        void SetProgress(string text)
        {
            int index = text.IndexOf('%');
            int progressValue;
            if (index > 4 && int.TryParse(text.Substring(index - 3, 3), out progressValue))
            {
                if (ProgressBar.Style != ProgressBarStyle.Blocks)
                    ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressBar.Value = Math.Min(100, progressValue);
            }
        }

        void AddOutput(string text)
        {
            Output.Text += text + "\n";
        }

        public StringBuilder outputString;

        void Done()
        {
            AddOutput(outputString.ToString());
            AddOutput("Done");
            ProgressBar.Visible = false;
            Ok.Enabled = true;
            //An error occured!
            if (gitCommand != null && gitCommand.Process != null && gitCommand.Process.ExitCode != 0)
            {
                ErrorImage.Visible = true;
                SuccessImage.Visible = false;
                if (GitCommands.GitCommands.GetSsh().Contains("plink"))
                {
                    if (ProcessArguments.Contains("pull") ||
                        ProcessArguments.Contains("push") ||
                        ProcessArguments.Contains("clone"))
                    {
                        if (Output.Text.Contains("FATAL ERROR") && Output.Text.Contains("authentication"))
                        {
                            if (MessageBox.Show("This error usually means that the PuTTY authentication agent is not running.\nor that the correct private key is not (yet) loaded.\n\nWhen the key is loaded, you can press retry. If not, cancel", "Authentication error", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                            {
                                FormProcess_Load(null, null);
                            }
                        }
                    }
                }
            }
            else
            {
                ErrorImage.Visible = false;
                SuccessImage.Visible = true;
            }
        }

        void gitCommand_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (e.Data.Contains("%"))
            {
                if (ProgressBar.InvokeRequired)
                {
                    // It's on a different thread, so use Invoke.
                    DataCallback d = new DataCallback(SetProgress);
                    this.Invoke(d, new object[] { e.Data });
                } else
                {
                    SetProgress(e.Data);
                }
            } else
            {
                /*if (Output.InvokeRequired)
                {
                    // It's on a different thread, so use Invoke.
                    DataCallback d = new DataCallback(AddOutput);
                    this.Invoke(d, new object[] { e.Data });
                } else
                {
                    AddOutput(e.Data);
                }*/
                outputString.Append(e.Data);
                outputString.Append("\n");
            }
        }



        void gitCommand_Exited(object sender, EventArgs e)
        {
            if (Ok.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                DoneCallback d = new DoneCallback(Done);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                Done();
            }
        }
    }
}
