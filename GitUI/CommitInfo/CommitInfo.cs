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
using ResourceManager;

namespace GitUI.CommitInfo
{
    public partial class CommitInfo : GitModuleControl
    {
        private readonly TranslationString containedInBranches = new TranslationString("Contained in branches:");
        private readonly TranslationString containedInNoBranch = new TranslationString("Contained in no branch");
        private readonly TranslationString containedInTags = new TranslationString("Contained in tags:");
        private readonly TranslationString containedInNoTag = new TranslationString("Contained in no tag");
        private readonly TranslationString trsLinksRelatedToRevision = new TranslationString("Related links:");

        private const int MaximumDisplayedRefs = 20;

        public CommitInfo()
        {
            InitializeComponent();
            Translate();
            GitUICommandsSourceSet += (a, uiCommandsSource) =>
            {
                _sortedRefs = null;
            };
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
        private List<string> _tags;
        private string _tagInfo;
        private List<string> _branches;
        private string _branchInfo;
        private IList<string> _sortedRefs;

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

            if (_sortedRefs == null)
                ThreadPool.QueueUserWorkItem(_ => loadSortedRefs());

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

        /// <summary>
        /// Returns an array of strings contains titles of fields field returned by GetHeader.
        /// Used to calculate layout in advance
        /// </summary>
        /// <returns></returns>
        private static string[] GetPossibleHeaders()
        {
            return new string[]
                   {
                       Strings.GetAuthorText(), Strings.GetAuthorDateText(), Strings.GetCommitterText(),
                       Strings.GetCommitDateText(), Strings.GetCommitHashText(), Strings.GetChildrenText(),
                       Strings.GetParentsText()
                   };
        }

        private int[] _revisionHeaderTabStops;
        private int[] GetRevisionHeaderTabStops()
        {
            if (_revisionHeaderTabStops != null)
                return _revisionHeaderTabStops;
            int tabStop = 0;
            foreach (string s in GetPossibleHeaders())
            {
                tabStop = Math.Max(tabStop, TextRenderer.MeasureText(s + "  ", _RevisionHeader.Font).Width);
            }
            // simulate a two column layout even when there's more then one tab used
            _revisionHeaderTabStops = new int[] { tabStop, tabStop + 1, tabStop + 2, tabStop + 3 };
            return _revisionHeaderTabStops;
        }

        private void loadSortedRefs()
        {
            _sortedRefs = Module.GetSortedRefs();
            this.InvokeAsync(updateText);
        }

        private void loadTagInfo(string revision)
        {
            _tags = Module.GetAllTagsWhichContainGivenCommit(revision).ToList();
            this.InvokeAsync(updateText);
        }

        private void loadBranchInfo(string revision)
        {
            // Include local branches if explicitly requested or when needed to decide whether to show remotes
            bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            // Include remote branches if requested
            bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                             AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            _branches = Module.GetAllBranchesWhichContainGivenCommit(revision, getLocal, getRemote).ToList();
            this.InvokeAsync(updateText);
        }

        private void loadLinksForRevision(GitRevision revision)
        {
            if (revision == null)
                return;

            _linksInfo = GetLinksForRevision(revision);
            this.InvokeAsync(updateText);
        }

        private class ItemTpComparer : IComparer<string>
        {
            private readonly IList<string> _otherList;
            private readonly string _prefix;

            public ItemTpComparer(IList<string> otherList, string prefix)
            {
                _otherList = otherList;
                _prefix = prefix;
            }

            public int Compare(string a, string b)
            {
                if (a.StartsWith("remotes/"))
                    a = "refs/" + a;
                else
                    a = _prefix + a;
                if (b.StartsWith("remotes/"))
                    b = "refs/" + b;
                else
                    b = _prefix + b;
                int i = _otherList.IndexOf(a);
                int j = _otherList.IndexOf(b);
                return i - j;
            }
        }

        private void updateText()
        {
            if (_sortedRefs != null)
            {
                if (_tags != null && string.IsNullOrEmpty(_tagInfo))
                {
                    _tags.Sort(new ItemTpComparer(_sortedRefs, "refs/tags/"));
                    if (_tags.Count > MaximumDisplayedRefs)
                    {
                        _tags[MaximumDisplayedRefs - 2] = "…";
                        _tags[MaximumDisplayedRefs - 1] = _tags[_tags.Count - 1];
                        _tags.RemoveRange(MaximumDisplayedRefs, _tags.Count - MaximumDisplayedRefs);
                    }
                    _tagInfo = GetTagsWhichContainsThisCommit(_tags, ShowBranchesAsLinks);
                }
                if (_branches != null && string.IsNullOrEmpty(_branchInfo))
                {
                    _branches.Sort(new ItemTpComparer(_sortedRefs, "refs/heads/"));
                    if (_branches.Count > MaximumDisplayedRefs)
                    {
                        _branches[MaximumDisplayedRefs - 2] = "…";
                        _branches[MaximumDisplayedRefs - 1] = _branches[_branches.Count - 1];
                        _branches.RemoveRange(MaximumDisplayedRefs, _branches.Count - MaximumDisplayedRefs);
                    }
                    _branchInfo = GetBranchesWhichContainsThisCommit(_branches, ShowBranchesAsLinks);
                }
            }
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
            _branches = null;
            _tags = null;
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

        private string GetBranchesWhichContainsThisCommit(IEnumerable<string> branches, bool showBranchesAsLinks)
        {
            const string remotesPrefix= "remotes/";
            // Include local branches if explicitly requested or when needed to decide whether to show remotes
            bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            // Include remote branches if requested
            bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                             AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
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

        private string GetTagsWhichContainsThisCommit(IEnumerable<string> tags, bool showBranchesAsLinks)
        {
            var tagString = tags
                .Select(s => showBranchesAsLinks ? LinkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s)).Join(", ");

            if (!String.IsNullOrEmpty(tagString))
                return Environment.NewLine + WebUtility.HtmlEncode(containedInTags.Text) + " " + tagString;
            return Environment.NewLine + WebUtility.HtmlEncode(containedInNoTag.Text);
        }

        private string GetLinksForRevision(GitRevision revision)
        {
            GitExtLinksParser parser = new GitExtLinksParser(Module.EffectiveSettings);
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

        private void DoCommandClick(string command, string data)
        {
            if (CommandClick != null)
            {
                CommandClick(this, new CommandEventArgs(command, data));
            }
        }

        private void _RevisionHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                DoCommandClick("navigatebackward", null);
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                DoCommandClick("navigateforward", null);
            }
        }
    }
}