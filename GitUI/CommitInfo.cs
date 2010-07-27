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
            ResetTextAndImage();
            if (string.IsNullOrEmpty(revision))
                return;

            RevisionInfo.Text =
                CommitInformation.GetCommitInfo(revision) +
                GetBranchesWhichContainsThisCommit(revision);

            LoadAuthorImage();
        }

        private void ResetTextAndImage()
        {
            RevisionInfo.Text = "";
            gravatar1.LoadImageForEmail("");
        }

        private void LoadAuthorImage()
        {
            var matches =
                Regex.Matches(
                    RevisionInfo.Text,
                    @"([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})");

            if (matches.Count == 0)
                return;

            gravatar1.LoadImageForEmail(matches[0].Value);
        }

        private static string GetBranchesWhichContainsThisCommit(string revision)
        {
            var branchString = "";
            foreach (var branch in CommitInformation.GetAllBranchesWhichContainGivenCommit(revision))
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