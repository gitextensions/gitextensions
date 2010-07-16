using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class CommitInfo : GitExtensionsControl
    {
        public CommitInfo()
        {
            InitializeComponent();
            Translate();

            tableLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayout.AutoSize = true;

            RevisionInfo.LinkClicked += RevisionInfoLinkClicked;
        }

        private static void RevisionInfoLinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                new Process
                    {
                        EnableRaisingEvents = false,
                        StartInfo = {FileName = e.LinkText}
                    }.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetRevision(string revision)
        {
            if (string.IsNullOrEmpty(revision))
            {
                RevisionInfo.Text = "";
                gravatar1.email = "";
                return;
            }

            RevisionInfo.Text = CommitInformation.GetCommitInfo(revision) + GetBranchesWhichContainsThisCommit(revision);

            MatchCollection matches = Regex.Matches(RevisionInfo.Text,
                                                    @"([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})");
            foreach (Match emailMatch in matches)
                gravatar1.email = emailMatch.Value;
        }

        private static string GetBranchesWhichContainsThisCommit(string revision)
        {
            string branchString = "";
            foreach (string branch in CommitInformation.GetAllBranchesWhichContainGivenCommit(revision))
            {
                if (branchString != string.Empty)
                    branchString += ", ";
                branchString += branch;
            }

            if (branchString != string.Empty)
                return "\r\nContained in branches: " + branchString;
            return "Contained in no branch";
        }
    }
}