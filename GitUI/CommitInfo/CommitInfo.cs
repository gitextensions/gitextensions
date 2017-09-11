﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitCommands.GitExtLinks;
using GitUI.Editor.RichTextBoxExtension;
using ResourceManager;
using GitUI.Editor;

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
        private LinkFactory _linkFactory = new LinkFactory();

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
            string link = _linkFactory.ParseLink(e.LinkText);
            HandleLink(link, sender);
        }

        private void HandleLink(string link, object sender)
        {
            try
            {
                var result = new Uri(link);
                if (result.Scheme == "gitext")
                {
                    if (CommandClick != null)
                    {
                        string path = result.AbsolutePath.TrimStart('/');
                        CommandClick(sender, new CommandEventArgs(result.Host, path));
                    }
                }
                else
                {
                    using (var process = new Process
                    {
                        EnableRaisingEvents = false,
                        StartInfo = { FileName = result.AbsoluteUri }
                    })
                    {
                        process.Start();
                    }
                }
            }
            catch (UriFormatException)
            {
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
        private IDictionary<string, string> _annotatedTagsMessages;
        private string _annotatedTagsInfo;
        private List<string> _tags;
        private string _tagInfo;
        private List<string> _branches;
        private string _branchInfo;
        private IList<string> _sortedRefs;
        private System.Drawing.Rectangle _headerResize; // Cache desired size for commit header

        private void ReloadCommitInfo()
        {
            _RevisionHeader.BackColor = ColorHelper.MakeColorDarker(this.BackColor);

            showContainedInBranchesToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesLocal;
            showContainedInBranchesRemoteToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemote;
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            showContainedInTagsToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInTags;
            showMessagesOfAnnotatedTagsToolStripMenuItem.Checked = AppSettings.ShowAnnotatedTagsMessages;

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
            CommitInformation commitInformation = CommitInformation.GetCommitInfo(data, _linkFactory, CommandClick != null, Module);

            _RevisionHeader.SetXHTMLText(commitInformation.Header);
            _RevisionHeader.Height = GetRevisionHeaderHeight();
            _revisionInfo = commitInformation.Body;
            updateText();
            LoadAuthorImage(data.Author ?? data.Committer);

            if (AppSettings.CommitInfoShowContainedInBranches)
                ThreadPool.QueueUserWorkItem(_ => loadBranchInfo(_revision.Guid));

            if (AppSettings.ShowAnnotatedTagsMessages)
                ThreadPool.QueueUserWorkItem(_ => loadAnnotatedTagInfo(_revision));

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

        private int GetRevisionHeaderHeight()
        {
            if (EnvUtils.IsMonoRuntime())
                return (int)(_RevisionHeader.Lines.Length * (0.8 + _RevisionHeader.Font.GetHeight()));

            return _headerResize.Height;
        }

        private void loadSortedRefs()
        {
            _sortedRefs = Module.GetSortedRefs();
            this.InvokeAsync(updateText);
        }

        private void loadAnnotatedTagInfo(GitRevision revision)
        {
            _annotatedTagsMessages = GetAnnotatedTagsMessages(revision);
            this.InvokeAsync(updateText);
        }

        private IDictionary<string, string> GetAnnotatedTagsMessages(GitRevision revision)
        {
            if (revision == null)
                return null;

            IDictionary<string, string> result = new Dictionary<string, string>();

            foreach (GitRef gitRef in revision.Refs)
            {
                #region Note on annotated tags
                // Notice that for the annotated tags, gitRef's come in pairs because they're produced 
                // by the "show-ref --dereference" command. GitRef's in such pair have the same Name, 
                // a bit different CompleteName's, and completely different checksums:
                //      GitRef_1:
                //      { 
                //          Name: "some_tag"
                //          CompleteName: "refs/tags/some_tag"
                //          Guid: <some_tag_checksum>
                //      },
                //       
                //      GitRef_2:
                //      { 
                //          Name: "some_tag"
                //          CompleteName: "refs/tags/some_tag^{}"   <- by "^{}", IsDereference is true.
                //          Guid: <target_object_checksum>
                //      }
                //
                // The 2nd one is a dereference: a link between the tag and the object which it references.
                // GitRevions.Refs by design contains GitRef's where Guid's are equal to the GitRevision.Guid,
                // so this collection contains only derefencing GitRef's - just because GitRef_2 has the same 
                // Guid as the GitRevision, while GitRef_1 doesn't. So annotated tag's GitRef would always be
                // of 2nd type in GitRevision.Refs collection, i.e. the one that has IsDereference==true.
                #endregion

                if (gitRef.IsTag && gitRef.IsDereference)
                {
                    string content = WebUtility.HtmlEncode(Module.GetTagMessage(gitRef.LocalName));

                    if (content != null)
                        result.Add(gitRef.LocalName, content);
                }
            }

            return result;
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

        public void DisplayAvatarOnRight()
        {
            tableLayout.SuspendLayout();
            this.tableLayout.ColumnStyles.Clear();
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayout.SetColumn(gravatar1, 1);
            tableLayout.SetColumn(_RevisionHeader, 0);
            tableLayout.SetColumn(RevisionInfo, 0);
            tableLayout.SetRowSpan(gravatar1, 1);
            tableLayout.SetColumnSpan(RevisionInfo, 2);
            tableLayout.ResumeLayout(true);

        }

        private void updateText()
        {
            if (_sortedRefs != null)
            {
                if (_annotatedTagsMessages != null &&
                    _annotatedTagsMessages.Count() > 0 &&
                    string.IsNullOrEmpty(_annotatedTagsInfo) &&
                    Revision != null)
                {
                    // having both lightweight & annotated tags in thisRevisionTagNames,
                    // but GetAnnotatedTagsInfo will process annotated only:
                    List<string> thisRevisionTagNames =
                        Revision
                        .Refs
                        .Where(r => r.IsTag)
                        .Select(r => r.LocalName)
                        .ToList();

                    thisRevisionTagNames.Sort(new ItemTpComparer(_sortedRefs, "refs/tags/"));
                    _annotatedTagsInfo = GetAnnotatedTagsInfo(Revision, thisRevisionTagNames, _annotatedTagsMessages);
                }
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
            RevisionInfo.SetXHTMLText(_revisionInfo + "\n" + _annotatedTagsInfo + _linksInfo + _branchInfo + _tagInfo);
            RevisionInfo.SelectionStart = 0; //scroll up
            RevisionInfo.ScrollToCaret();    //scroll up
            RevisionInfo.ResumeLayout(true);
        }

        private static string GetAnnotatedTagsInfo(
            GitRevision revision,
            IEnumerable<string> tagNames,
            IDictionary<string, string> annotatedTagsMessages)
        {
            string result = string.Empty;

            foreach (string tag in tagNames)
            {
                string annotatedContents;
                if (annotatedTagsMessages.TryGetValue(tag, out annotatedContents))
                    result += "<u>" + tag + "</u>: " + annotatedContents + Environment.NewLine;
            }

            if (result.IsNullOrEmpty())
                return string.Empty;

            return Environment.NewLine + result;
        }

        private void ResetTextAndImage()
        {
            _revisionInfo = string.Empty;
            _linksInfo = string.Empty;
            _branchInfo = string.Empty;
            _annotatedTagsInfo = string.Empty;
            _tagInfo = string.Empty;
            _branches = null;
            _annotatedTagsMessages = null;
            _tags = null;
            _linkFactory.Clear();
            updateText();
            gravatar1.Visible = AppSettings.ShowAuthorGravatar;
            if (AppSettings.ShowAuthorGravatar)
            {
                gravatar1.LoadImage("");
            }
        }

        private void LoadAuthorImage(string author)
        {
            var matches = Regex.Matches(author, @"<([\w\-\.]+@[\w\-\.]+)>");

            if (matches.Count == 0)
                return;

            if (AppSettings.ShowAuthorGravatar)
            {
                gravatar1.LoadImage(matches[0].Groups[1].Value);
            }
        }

        private string GetBranchesWhichContainsThisCommit(IEnumerable<string> branches, bool showBranchesAsLinks)
        {
            const string remotesPrefix = "remotes/";
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
                        branchText = _linkFactory.CreateBranchLink(noPrefixBranch);
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
                .Select(s => showBranchesAsLinks ? _linkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s)).Join(", ");

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
                linksString = linksString.Combine(", ", _linkFactory.CreateLink(link.Caption, link.URI));
            }

            if (linksString.IsNullOrEmpty())
                return string.Empty;
            else
                return Environment.NewLine + WebUtility.HtmlEncode(trsLinksRelatedToRevision.Text) + " " + linksString;
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
            var commitInfo = string.Empty;
            if (EnvUtils.IsMonoRuntime())
            {
                commitInfo = $"{_RevisionHeader.Text}{Environment.NewLine}{RevisionInfo.Text}";
            }
            else
            {
                commitInfo = $"{_RevisionHeader.GetPlaintText()}{Environment.NewLine}{RevisionInfo.GetPlaintText()}";
            }
            Clipboard.SetText(commitInfo);
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

        private void showMessagesOfAnnotatedTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.ShowAnnotatedTagsMessages = !AppSettings.ShowAnnotatedTagsMessages;
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

        private void _RevisionHeader_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            // Cache desired size for commit header
            _headerResize = e.NewRectangle;
        }

    }
}