using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using System.Threading;

namespace GitUI
{
    public partial class CommitInfo : GitExtensionsControl
    {
        private readonly SynchronizationContext _syncContext;

        public CommitInfo()
        {
            _syncContext = SynchronizationContext.Current;

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

        private string _revision;
        public void SetRevision(string revision)
        {
            _revision = revision;
            ReloadCommitInfo();
        }

        private void ReloadCommitInfo()
        {
            showContainedInBranchesToolStripMenuItem.Checked = Settings.CommitInfoShowContainedInBranches;
            showContainedInTagsToolStripMenuItem.Checked = Settings.CommitInfoShowContainedInTags;

            ResetTextAndImage();
            if (string.IsNullOrEmpty(_revision))
                return;

            RevisionInfo.Text = CommitInformation.GetCommitInfo(_revision);
            RevisionInfo.Refresh();
            LoadAuthorImage();

            if (Settings.CommitInfoShowContainedInBranches)
            {
                _syncContext.Post(
                                    s =>
                                    {
                                        RevisionInfo.Text += GetBranchesWhichContainsThisCommit(_revision);
                                    }, null);
            }
            if (Settings.CommitInfoShowContainedInTags)
            {
                _syncContext.Post(
                                    s =>
                                    {
                                        RevisionInfo.Text += GetTagsWhichContainsThisCommit(_revision);
                                    }, null);
            }
        }

        private void ResetTextAndImage()
        {
            RevisionInfo.Text = "";
            RevisionInfo.Refresh();
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
            return "\r\nContained in no branch";
        }

        private static string GetTagsWhichContainsThisCommit(string revision)
        {
            var tagString = "";
            foreach (var tag in CommitInformation.GetAllTagsWhichContainGivenCommit(revision))
            {
                if (tagString != string.Empty)
                    tagString += ", ";
                tagString += tag;
            }

            if (tagString != string.Empty)
                return "\r\nContained in tags: " + tagString;
            return "\r\nContained in no tag";
        }

        private void tableLayout_Paint(object sender, PaintEventArgs e)
        {

        }

        private void showContainedInBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.CommitInfoShowContainedInBranches = !Settings.CommitInfoShowContainedInBranches;
            ReloadCommitInfo();
        }

        private void showContainedInTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.CommitInfoShowContainedInTags = !Settings.CommitInfoShowContainedInTags;
            ReloadCommitInfo();            
        }
    }
}