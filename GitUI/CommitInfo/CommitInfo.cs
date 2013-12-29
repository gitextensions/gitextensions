using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.GitExtLinks;
using GitUI.Editor.RichTextBoxExtension;
using ResourceManager.Translation;

namespace GitUI.CommitInfo
{
    public partial class CommitInfo : GitModuleControl
    {
        private readonly TranslationString containedInBranches = new TranslationString("Contained in branches:");
        private readonly TranslationString containedInNoBranch = new TranslationString("Contained in no branch");
        private readonly TranslationString containedInTags = new TranslationString("Contained in tags:");
        private readonly TranslationString containedInNoTag = new TranslationString("Contained in no tag");
        private readonly TranslationString trsLinksRelatedToRevision = new TranslationString("Related links:");

        public CommitInfo()
        {
            InitializeComponent();
            Translate();
        }

        [DefaultValue(false)]
        public bool ShowBranchesAsLinks { get; set; }
        
        public event EventHandler<CommandEventArgs> CommandClick;

        private void RevisionInfoLinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                var url = e.LinkText;
                var data = url.Split(new[] { '#' }, 2);

                try
                {
                    if (data.Length > 1)
                    {
                        var result = new Uri(data[1]);
                        if (result.Scheme == "gitext")
                        {
                            if (CommandClick != null)
                            {
                                string path = result.AbsolutePath.TrimStart('/');
                                CommandClick(sender, new CommandEventArgs(result.Host, path));
                            }
                            return;
                        }
                        else
                        {
                            url = result.AbsoluteUri;
                        }
                    }
                }
                catch (UriFormatException)
                {

                }

                using (var process = new Process
                    {
                        EnableRaisingEvents = false,
                        StartInfo = { FileName = url }
                    })
                    process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private GitRevision _revision;
        private List<string> _children;
        public void SetRevisionWithChildren(GitRevision revision, List<string> children)
        {
            _revision = revision;
            _children = children;
            ReloadCommitInfo();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitRevision Revision
        {
            get
            {
                return _revision;
            }
            set
            {
                SetRevisionWithChildren(value, null);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string RevisionGuid
        {
            get
            {
                return _revision.Guid;
            }
        }

        private string _revisionInfo;
        private string _linksInfo;
        private string _tagInfo;
        private string _branchInfo;

        private void ReloadCommitInfo()
        {
            showContainedInBranchesToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesLocal;
            showContainedInBranchesRemoteToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemote;
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            showContainedInTagsToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInTags;

            ResetTextAndImage();

            if (string.IsNullOrEmpty(_revision.Guid))
                return; //is it regular case or should throw an exception

            _RevisionHeader.SelectionTabs = GetRevisionHeaderTabStops(); 
            _RevisionHeader.Text = string.Empty;
            _RevisionHeader.Refresh();

            string error = "";
            CommitData data = CommitData.CreateFromRevision(_revision);
            if (_revision.Body == null)
            {
                CommitData.UpdateCommitMessage(data, Module, _revision.Guid, ref error);
                _revision.Body = data.Body;
            }

            ThreadPool.QueueUserWorkItem(_ => loadLinksForRevision(_revision));

            data.ChildrenGuids = _children;
            CommitInformation commitInformation = CommitInformation.GetCommitInfo(data, CommandClick != null);

            _RevisionHeader.SetXHTMLText(commitInformation.Header);
            _RevisionHeader.Height = _RevisionHeader.GetPreferredSize(new System.Drawing.Size(0, 0)).Height;
            _revisionInfo = commitInformation.Body;
            updateText();
            LoadAuthorImage(data.Author ?? data.Committer);

            if (AppSettings.CommitInfoShowContainedInBranches)
                ThreadPool.QueueUserWorkItem(_ => loadBranchInfo(_revision.Guid));

            if (AppSettings.CommitInfoShowContainedInTags)
                ThreadPool.QueueUserWorkItem(_ => loadTagInfo(_revision.Guid));
        }

        private int[] _revisionHeaderTabStops;
        private int[] GetRevisionHeaderTabStops()
        {
            if (_revisionHeaderTabStops != null)
                return _revisionHeaderTabStops;
            int tabStop = 0;
            foreach (string s in CommitData.GetPossibleHeaders())
            {
                tabStop = Math.Max(tabStop, TextRenderer.MeasureText(s + "  ", _RevisionHeader.Font).Width);
            }
            // simulate a two column layout even when there's more then one tab used
            _revisionHeaderTabStops = new int[] { tabStop, tabStop + 1, tabStop + 2, tabStop + 3 };
            return _revisionHeaderTabStops;
        }

        private void loadTagInfo(string revision)
        {
            _tagInfo = GetTagsWhichContainsThisCommit(revision, ShowBranchesAsLinks);
            this.InvokeAsync(updateText);
        }

        private void loadBranchInfo(string revision)
        {
            _branchInfo = GetBranchesWhichContainsThisCommit(revision, ShowBranchesAsLinks);
            this.InvokeAsync(updateText);
        }

        private void loadLinksForRevision(GitRevision revision)
        {
            if (revision == null)
                return;

            _linksInfo = GetLinksForRevision(revision);
            this.InvokeAsync(updateText);
        }

        private void updateText()
        {
            RevisionInfo.SuspendLayout();
            RevisionInfo.SetXHTMLText(_revisionInfo + "\n\n" + _linksInfo + _branchInfo + _tagInfo);
            RevisionInfo.SelectionStart = 0; //scroll up
            RevisionInfo.ScrollToCaret();    //scroll up
            RevisionInfo.ResumeLayout(true);
        }

        private void ResetTextAndImage()
        {
            _revisionInfo = string.Empty;
            _linksInfo = string.Empty;
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
            bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            // Include remote branches if requested
            bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                             AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            var branches = Module.GetAllBranchesWhichContainGivenCommit(revision, getLocal, getRemote);
            var links = new List<string>();
            bool allowLocal = AppSettings.CommitInfoShowContainedInBranchesLocal;
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

                if (branchIsLocal && AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)
                    allowRemote = false;
            }
            if (links.Any())
                return Environment.NewLine + WebUtility.HtmlEncode(containedInBranches.Text) + " " + links.Join(", ");
            return Environment.NewLine + WebUtility.HtmlEncode(containedInNoBranch.Text);
        }

        private string GetTagsWhichContainsThisCommit(string revision, bool showBranchesAsLinks)
        {
            var tagString = Module.GetAllTagsWhichContainGivenCommit(revision)
                .Select(s => showBranchesAsLinks ? LinkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s)).Join(", ");

            if (!String.IsNullOrEmpty(tagString))
                return Environment.NewLine + WebUtility.HtmlEncode(containedInTags.Text) + " " + tagString;
            return Environment.NewLine + WebUtility.HtmlEncode(containedInNoTag.Text);
        }

        private string GetLinksForRevision(GitRevision revision)
        {
            GitExtLinksParser parser = new GitExtLinksParser(Module.Settings);
            var links = parser.Parse(revision).Distinct();
            var linksString = string.Empty;

            foreach (var link in links)
            { 
               linksString = linksString.Combine(", ", LinkFactory.CreateLink(link.Caption, link.URI));
            }

            if (linksString.IsNullOrEmpty())
                return string.Empty;
            else
                return WebUtility.HtmlEncode(trsLinksRelatedToRevision.Text) + " " + linksString;
        }

        private void showContainedInBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.CommitInfoShowContainedInBranchesLocal = !AppSettings.CommitInfoShowContainedInBranchesLocal;
            ReloadCommitInfo();
        }

        private void showContainedInTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.CommitInfoShowContainedInTags = !AppSettings.CommitInfoShowContainedInTags;
            ReloadCommitInfo();
        }

        private void copyCommitInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Concat(_RevisionHeader.GetPlaintText(), Environment.NewLine, RevisionInfo.GetPlaintText()));
        }

        private void showContainedInBranchesRemoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.CommitInfoShowContainedInBranchesRemote = !AppSettings.CommitInfoShowContainedInBranchesRemote;
            ReloadCommitInfo();
        }

        private void showContainedInBranchesRemoteIfNoLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = !AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            ReloadCommitInfo();
        }

        private void addNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Module.EditNotes(_revision.Guid);
            ReloadCommitInfo();
        }
    }
}