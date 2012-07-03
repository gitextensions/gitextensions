﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor.RichTextBoxExtension;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class CommitInfo : GitExtensionsControl
    {
        private readonly TranslationString containedInBranches = new TranslationString("Contained in branches:");
        private readonly TranslationString containedInNoBranch = new TranslationString("Contained in no branch");
        private readonly TranslationString containedInTags = new TranslationString("Contained in tags:");
        private readonly TranslationString containedInNoTag = new TranslationString("Contained in no tag");

        private readonly SynchronizationContext _syncContext;

        public CommitInfo()
        {
            _syncContext = SynchronizationContext.Current;

            InitializeComponent();
            Translate();

            tableLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayout.AutoSize = true;

            _RevisionHeader.LinkClicked += RevisionInfoLinkClicked;
            RevisionInfo.LinkClicked += RevisionInfoLinkClicked;
        }

        private static void RevisionInfoLinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                string url = e.GetUrl();
                new Process
                    {
                        EnableRaisingEvents = false,
                        StartInfo = { FileName = url }
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

        public string GetRevision()
        {
            return _revision;
        }

        private string _revisionInfo;
        private string _tagInfo;
        private string _branchInfo;

        private void ReloadCommitInfo()
        {
            showContainedInBranchesToolStripMenuItem.Checked = Settings.CommitInfoShowContainedInBranchesLocal;
            showContainedInBranchesRemoteToolStripMenuItem.Checked = Settings.CommitInfoShowContainedInBranchesRemote;
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Checked = Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            showContainedInTagsToolStripMenuItem.Checked = Settings.CommitInfoShowContainedInTags;

            ResetTextAndImage();
            if (string.IsNullOrEmpty(_revision))
                return;
            _RevisionHeader.Text = string.Empty;
            _RevisionHeader.Refresh();
            CommitInformation commitInformation = CommitInformation.GetCommitInfo(_revision);
            _RevisionHeader.SetXHTMLText(commitInformation.Header);
            splitContainer1.SplitterDistance = _RevisionHeader.GetPreferredSize(new System.Drawing.Size(0, 0)).Height;
            _revisionInfo = commitInformation.Body;
            updateText();
            LoadAuthorImage();

            if (Settings.CommitInfoShowContainedInBranches)
                ThreadPool.QueueUserWorkItem(_ => loadBranchInfo(_revision));

            if (Settings.CommitInfoShowContainedInTags)
                ThreadPool.QueueUserWorkItem(_ => loadTagInfo(_revision));
        }

        private void loadTagInfo(string revision)
        {
            _tagInfo = GetTagsWhichContainsThisCommit(revision);
            _syncContext.Post(  s => updateText(), null);
        }

        private void loadBranchInfo(string revision)
        {
            _branchInfo = GetBranchesWhichContainsThisCommit(revision);
            _syncContext.Post(s => updateText(), null);
        }

        private void updateText()
        {
            RevisionInfo.SuspendLayout();
            RevisionInfo.SetXHTMLText(_revisionInfo + _branchInfo + _tagInfo);
            RevisionInfo.SelectionStart = 0; //scroll up
            RevisionInfo.ScrollToCaret();    //scroll up
            RevisionInfo.ResumeLayout(true);
        }

        private void ResetTextAndImage()
        {
            _revisionInfo = string.Empty;
            _branchInfo = string.Empty;
            _tagInfo = string.Empty;
            updateText();
            gravatar1.LoadImageForEmail("");
        }

        private void LoadAuthorImage()
        {
            var matches =
                Regex.Matches(
                    _RevisionHeader.Text,
                    @"([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})");

            if (matches.Count == 0)
                return;

            gravatar1.LoadImageForEmail(matches[0].Value);
        }


        private string GetBranchesWhichContainsThisCommit(string revision)
        {
            const string remotesPrefix= "remotes/";
            // Include local branches if explicitly requested or when needed to decide whether to show remotes
            bool getLocal = Settings.CommitInfoShowContainedInBranchesLocal ||
                            Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            // Include remote branches if requested
            bool getRemote = Settings.CommitInfoShowContainedInBranchesRemote ||
                             Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            var branches = CommitInformation.GetAllBranchesWhichContainGivenCommit(revision, getLocal, getRemote);
            var branchString = "";
            bool allowLocal = Settings.CommitInfoShowContainedInBranchesLocal;
            bool allowRemote = getRemote;
            foreach (var branch in branches)
            {
                string noPrefixBranch = branch;
                bool branchIsLocal;
                if (getLocal && getRemote)
                {
                    // "git branch -a" prefixes remote branches with "remotes/"
                    // It is possible to create a local branch named "remotes/origin/something"
                    // so this check is not 100% reliable.
                    // This shouldn't be a big problem if we're only displaying information.
                    branchIsLocal = !branch.StartsWith(remotesPrefix);
                    if (!branchIsLocal)
                        noPrefixBranch = branch.Substring(remotesPrefix.Length);
                }
                else
                {
                    branchIsLocal = !getRemote;
                }

                if ((branchIsLocal && allowLocal) || (!branchIsLocal && allowRemote))
                {
                    if (branchString != string.Empty)
                        branchString += ", ";
                    branchString += noPrefixBranch;
                }

                if (branchIsLocal && Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)
                    allowRemote = false;
            }
            if (branchString != string.Empty)
                return Environment.NewLine + HttpUtility.HtmlEncode(containedInBranches.Text + " " + branchString);
            return Environment.NewLine + HttpUtility.HtmlEncode(containedInNoBranch.Text);
        }

        private string GetTagsWhichContainsThisCommit(string revision)
        {
            var tagString = "";
            foreach (var tag in CommitInformation.GetAllTagsWhichContainGivenCommit(revision))
            {
                if (tagString != string.Empty)
                    tagString += ", ";
                tagString += tag;
            }

            if (tagString != string.Empty)
                return Environment.NewLine + HttpUtility.HtmlEncode(containedInTags.Text + " " + tagString);
            return Environment.NewLine + HttpUtility.HtmlEncode(containedInNoTag.Text);
        }

        private void tableLayout_Paint(object sender, PaintEventArgs e)
        {

        }

        private void showContainedInBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.CommitInfoShowContainedInBranchesLocal = !Settings.CommitInfoShowContainedInBranchesLocal;
            ReloadCommitInfo();
        }

        private void showContainedInTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.CommitInfoShowContainedInTags = !Settings.CommitInfoShowContainedInTags;
            ReloadCommitInfo();
        }

        private void copyCommitInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Concat(_RevisionHeader.Text, Environment.NewLine, RevisionInfo.Text).Replace("\n",Environment.NewLine));
        }

        private void showContainedInBranchesRemoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.CommitInfoShowContainedInBranchesRemote = !Settings.CommitInfoShowContainedInBranchesRemote;
            ReloadCommitInfo();
        }

        private void showContainedInBranchesRemoteIfNoLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = !Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            ReloadCommitInfo();
        }

        private void addNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Module.EditNotes(_revision);
            ReloadCommitInfo();
        }
    }
}