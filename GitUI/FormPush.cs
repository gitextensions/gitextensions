using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace GitUI
{
    public partial class FormPush : Form
    {
        public FormPush()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                PushDestination.Text = dialog.SelectedPath;
            
        }

        private void Push_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show("Please select a destination directory");
                return;
            }

            Process process = GitCommands.GitCommands.PushAsync(PushDestination.Text);
//            process.BeginErrorReadLine();
  //          process.BeginOutputReadLine();
    //        process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
      //      process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);


            process.WaitForExit();
            
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            string data = e.Data;
            if (data.StartsWith("Enter passphrase"))
                ((Process)sender).StandardInput.WriteLine("achttien");
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            string data = e.Data;
            if (data.StartsWith("Enter passphrase"))
                ((Process)sender).StandardInput.WriteLine("achttien");
        }
    }
}
