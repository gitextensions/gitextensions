using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace GitUI
{
    public partial class CommitInfo : UserControl
    {
        public CommitInfo()
        {
            InitializeComponent();

            RevisionInfo.LinkClicked += new LinkClickedEventHandler(RevisionInfo_LinkClicked);
        }

        void RevisionInfo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = e.LinkText;

                proc.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetRevision(string revision)
        {
            RevisionInfo.Text = GitCommands.GitCommands.GetCommitInfo(revision);

            Match emailMatch = Regex.Match(RevisionInfo.Text, @"([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})");

            if (emailMatch != null)
                gravatar1.email = emailMatch.Value;
        }
    }
}
