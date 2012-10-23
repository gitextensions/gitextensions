using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor.RichTextBoxExtension;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class CommitInfo : GitModuleControl
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
        }

        [DefaultValue(false)]
        public bool ShowBranchesAsLinks { get; set; }

        public delegate void CommandClickHandler(string command, string data);

        public event CommandClickHandler CommandClick;

        private void RevisionInfoLinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                var url = e.LinkText;
                var data = url.Split(new[] { '#' }, 2);
                if (data.Length > 1)
                    url = data[1];

                try
                {
                    var result = new Uri(url);
                    if (result.Scheme == "gitex")
                    {
                        if (CommandClick != null)
                        {
                            string path = result.AbsolutePath.TrimStart('/');
                            CommandClick(result.Host, path);
                        }
                        return;
                    }
                }
                catch (UriFormatException)
                {

                }

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
        private List<string> _children;
        public void SetRevision(string revision, List<string> children)
        {
            _revision = revision;
            _children = children;
            ReloadCommitInfo();
        }

        public void SetRevision(string revision)
        {
            SetRevision(revision, null);
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

            string error = "";
            CommitData data = CommitData.GetCommitData(Module, _revision, ref error);
            data.ChildrenGuids = _children;
            CommitInformation commitInformation = CommitInformation.GetCommitInfo(data);

            _RevisionHeader.SetXHTMLText(commitInformation.Header);
            _RevisionHeader.Height = _RevisionHeader.GetPreferredSize(new System.Drawing.Size(0, 0)).Height;
            _revisionInfo = commitInformation.Body;
            updateText();
            LoadAuthorImage(data.Author ?? data.Committer);

            if (Settings.CommitInfoShowContainedInBranches)
                ThreadPool.QueueUserWorkItem(_ => loadBranchInfo(_revision));

            if (Settings.CommitInfoShowContainedInTags)
                ThreadPool.QueueUserWorkItem(_ => loadTagInfo(_revision));
        }

        private void loadTagInfo(string revision)
        {
            _tagInfo = GetTagsWhichContainsThisCommit(revision, ShowBranchesAsLinks);
            _syncContext.Post(  s => updateText(), null);
        }

        private void loadBranchInfo(string revision)
        {
            _branchInfo = GetBranchesWhichContainsThisCommit(revision, ShowBranchesAsLinks);
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

        private void LoadAuthorImage(string author)
        {
            var matches = Regex.Matches(author, @"<([\w\-\.]+@[\w\-\.]+)>");

            if (matches.Count == 0)
                return;

            gravatar1.LoadImageForEmail(matches[0].Groups[1].Value);
        }

        private string GetBranchesWhichContainsThisCommit(string revision, bool showBranchesAsLinks)
        {
            const string remotesPrefix= "remotes/";
            // Include local branches if explicitly requested or when needed to decide whether to show remotes
            bool getLocal = Settings.CommitInfoShowContainedInBranchesLocal ||
                            Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            // Include remote branches if requested
            bool getRemote = Settings.CommitInfoShowContainedInBranchesRemote ||
                             Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            var branches = CommitInformation.GetAllBranchesWhichContainGivenCommit(Module, revision, getLocal, getRemote);
            var links = new List<string>();
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
                    string branchText;
                    if (showBranchesAsLinks)
                        branchText = LinkFactory.CreateBranchLink(noPrefixBranch);
                    else 
                        branchText = WebUtility.HtmlEncode(noPrefixBranch);
                    links.Add(branchText);
                }

                if (branchIsLocal && Settings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)
                    allowRemote = false;
            }
            if (links.Any())
                return Environment.NewLine + WebUtility.HtmlEncode(containedInBranches.Text) + " " + links.Join(", ");
            return Environment.NewLine + WebUtility.HtmlEncode(containedInNoBranch.Text);
        }

        private string GetTagsWhichContainsThisCommit(string revision, bool showBranchesAsLinks)
        {
            var tagString = CommitInformation
                .GetAllTagsWhichContainGivenCommit(Module, revision)
                .Select(s => showBranchesAsLinks ? LinkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s)).Join(", ");

            if (tagString != string.Empty)
                return Environment.NewLine + WebUtility.HtmlEncode(containedInTags.Text) + " " + tagString;
            return Environment.NewLine + WebUtility.HtmlEncode(containedInNoTag.Text);
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
            Clipboard.SetText(string.Concat(_RevisionHeader.GetPlaintText(), Environment.NewLine, RevisionInfo.GetPlaintText()));
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
            Module.EditNotes(_revision);
            ReloadCommitInfo();
        }
    }
}