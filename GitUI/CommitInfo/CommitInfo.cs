using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Git;
using GitCommands.Remote;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using GitUI.Editor.RichTextBoxExtension;
using GitUI.Hotkey;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.CommitInfo
{
    public partial class CommitInfo : GitModuleControl
    {
        private readonly TranslationString _containedInBranches = new TranslationString("Contained in branches:");
        private readonly TranslationString _containedInNoBranch = new TranslationString("Contained in no branch");
        private readonly TranslationString _containedInTags = new TranslationString("Contained in tags:");
        private readonly TranslationString _containedInNoTag = new TranslationString("Contained in no tag");
        private readonly TranslationString _trsLinksRelatedToRevision = new TranslationString("Related links:");
        private readonly TranslationString _derivesFromTag = new TranslationString("Derives from tag:");
        private readonly TranslationString _derivesFromNoTag = new TranslationString("Derives from no tag");
        private readonly TranslationString _plusCommits = new TranslationString("commits");

        private const int MaximumDisplayedRefs = 20;
        private readonly ILinkFactory _linkFactory = new LinkFactory();
        private readonly IDateFormatter _dateFormatter = new DateFormatter();
        private readonly ICommitDataManager _commitDataManager;
        private readonly ICommitDataHeaderRenderer _commitDataHeaderRenderer;
        private readonly ICommitDataBodyRenderer _commitDataBodyRenderer;
        private readonly IExternalLinksLoader _externalLinksLoader;
        private readonly IConfiguredLinkDefinitionsProvider _effectiveLinkDefinitionsProvider;
        private readonly IGitRevisionExternalLinksParser _gitRevisionExternalLinksParser;
        private readonly IExternalLinkRevisionParser _externalLinkRevisionParser;
        private readonly IGitRemoteManager _gitRemoteManager;
        private readonly GitDescribeProvider _gitDescribeProvider;

        public CommitInfo()
        {
            InitializeComponent();
            Translate();
            GitUICommandsSourceSet += (a, uiCommandsSource) =>
            {
                _sortedRefs = null;
            };

            _commitDataManager = new CommitDataManager(() => Module);

            var labelFormatter = new TabbedHeaderLabelFormatter();
            var headerRenderer = new TabbedHeaderRenderStyleProvider();

            _commitDataHeaderRenderer = new CommitDataHeaderRenderer(labelFormatter, _dateFormatter, headerRenderer, _linkFactory);
            _commitDataBodyRenderer = new CommitDataBodyRenderer(() => Module, _linkFactory);
            _externalLinksLoader = new ExternalLinksLoader();
            _effectiveLinkDefinitionsProvider = new ConfiguredLinkDefinitionsProvider(_externalLinksLoader);
            _gitRemoteManager = new GitRemoteManager(() => Module);
            _externalLinkRevisionParser = new ExternalLinkRevisionParser(_gitRemoteManager);
            _gitRevisionExternalLinksParser = new GitRevisionExternalLinksParser(_effectiveLinkDefinitionsProvider, _externalLinkRevisionParser);
            _gitDescribeProvider = new GitDescribeProvider(() => Module);

            RevisionInfo.Font = AppSettings.Font;
            using (Graphics g = CreateGraphics())
            {
                _RevisionHeader.Font = _commitDataHeaderRenderer.GetFont(g);
            }

            _RevisionHeader.SelectionTabs = _commitDataHeaderRenderer.GetTabStops().ToArray();

            Hotkeys = HotkeySettingsManager.LoadHotkeys(FormBrowse.HotkeySettingsName);
            addNoteToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys((int)FormBrowse.Commands.AddNotes).ToShortcutKeyDisplayString();
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
        private IReadOnlyList<string> _children;
        public void SetRevisionWithChildren(GitRevision revision, IReadOnlyList<string> children)
        {
            _revision = revision;
            _children = children;

            ReloadCommitInfo();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitRevision Revision
        {
            get => _revision;
            set => SetRevisionWithChildren(value, null);
        }

        private string _revisionInfo;
        private string _linksInfo;
        private IDictionary<string, string> _annotatedTagsMessages;
        private string _annotatedTagsInfo;
        private List<string> _tags;
        private string _tagInfo;
        private List<string> _branches;
        private string _branchInfo;
        private string _gitDescribeInfo;
        private IList<string> _sortedRefs;
        private Rectangle _headerResize; // Cache desired size for commit header

        private void ReloadCommitInfo()
        {
            _RevisionHeader.BackColor = ColorHelper.MakeColorDarker(BackColor);

            showContainedInBranchesToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesLocal;
            showContainedInBranchesRemoteToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemote;
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            showContainedInTagsToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInTags;
            showMessagesOfAnnotatedTagsToolStripMenuItem.Checked = AppSettings.ShowAnnotatedTagsMessages;
            showTagThisCommitDerivesFromMenuItem.Checked = AppSettings.CommitInfoShowTagThisCommitDerivesFrom;

            ResetTextAndImage();

            if (string.IsNullOrEmpty(_revision.Guid))
            {
                Debug.Assert(false, "Unexpectedly called ReloadCommitInfo() with empty revision");
                return;
            }

            _RevisionHeader.Text = string.Empty;
            _RevisionHeader.Refresh();
            CommitData data = _commitDataManager.CreateFromRevision(_revision);
            if (_revision.Body == null)
            {
                _commitDataManager.UpdateBody(data, out _);
                _revision.Body = data.Body;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadLinksForRevisionAsync(_revision)).FileAndForget();

            if (_sortedRefs == null)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadSortedRefsAsync()).FileAndForget();
            }

            data.ChildrenGuids = _children;
            var header = _commitDataHeaderRenderer.Render(data, CommandClick != null);
            var body = _commitDataBodyRenderer.Render(data, CommandClick != null);

            _RevisionHeader.SetXHTMLText(header);
            _RevisionHeader.Height = GetRevisionHeaderHeight();
            _revisionInfo = body;

            UpdateRevisionInfo();
            LoadAuthorImage(data.Author ?? data.Committer);

            // No branch/tag data for artificial commands
            if (_revision.IsArtificial)
            {
                return;
            }

            if (AppSettings.CommitInfoShowContainedInBranches)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadBranchInfoAsync(_revision.Guid)).FileAndForget();
            }

            if (AppSettings.ShowAnnotatedTagsMessages)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadAnnotatedTagInfoAsync(_revision)).FileAndForget();
            }

            if (AppSettings.CommitInfoShowContainedInTags)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadTagInfoAsync(_revision.Guid)).FileAndForget();
            }

            if (AppSettings.CommitInfoShowTagThisCommitDerivesFrom)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadDescribeInfoAsync(_revision.Guid)).FileAndForget();
            }
        }

        private int GetRevisionHeaderHeight()
        {
            return _headerResize.Height;
        }

        private async Task LoadSortedRefsAsync()
        {
            await TaskScheduler.Default.SwitchTo(alwaysYield: true);
            _sortedRefs = Module.GetSortedRefs().ToList();

            await this.SwitchToMainThreadAsync();
            UpdateRevisionInfo();
        }

        private async Task LoadAnnotatedTagInfoAsync(GitRevision revision)
        {
            await TaskScheduler.Default.SwitchTo(alwaysYield: true);
            _annotatedTagsMessages = GetAnnotatedTagsMessages(revision);

            await this.SwitchToMainThreadAsync();
            UpdateRevisionInfo();
        }

        private IDictionary<string, string> GetAnnotatedTagsMessages(GitRevision revision)
        {
            if (revision == null)
            {
                return null;
            }

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
                    {
                        result.Add(gitRef.LocalName, content);
                    }
                }
            }

            return result;
        }

        private async Task LoadTagInfoAsync(string revision)
        {
            await TaskScheduler.Default.SwitchTo(alwaysYield: true);
            _tags = Module.GetAllTagsWhichContainGivenCommit(revision).ToList();

            await this.SwitchToMainThreadAsync();
            UpdateRevisionInfo();
        }

        private async Task LoadBranchInfoAsync(string revision)
        {
            await TaskScheduler.Default.SwitchTo(alwaysYield: true);

            // Include local branches if explicitly requested or when needed to decide whether to show remotes
            bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;

            // Include remote branches if requested
            bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                             AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            _branches = Module.GetAllBranchesWhichContainGivenCommit(revision, getLocal, getRemote).ToList();

            await this.SwitchToMainThreadAsync();
            UpdateRevisionInfo();
        }

        private async Task LoadLinksForRevisionAsync(GitRevision revision)
        {
            if (revision == null)
            {
                return;
            }

            await TaskScheduler.Default;
            _linksInfo = GetLinksForRevision(revision);

            await this.SwitchToMainThreadAsync();
            UpdateRevisionInfo();
        }

        private async Task LoadDescribeInfoAsync(string revision)
        {
            await TaskScheduler.Default;
            _gitDescribeInfo = GetDescribeInfoForRevision(revision);

            await this.SwitchToMainThreadAsync();
            UpdateRevisionInfo();
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
                {
                    a = "refs/" + a;
                }
                else
                {
                    a = _prefix + a;
                }

                if (b.StartsWith("remotes/"))
                {
                    b = "refs/" + b;
                }
                else
                {
                    b = _prefix + b;
                }

                int i = _otherList.IndexOf(a);
                int j = _otherList.IndexOf(b);
                return i - j;
            }
        }

        private void DisplayAvatarSetup(bool right)
        {
            tableLayout.SuspendLayout();
            tableLayout.ColumnStyles.Clear();
            int gravatarIndex, revInfoIndex, gravatarSpan, revInfoSpan;
            if (right)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tableLayout.ColumnStyles.Add(new ColumnStyle());
                gravatarIndex = 1;
                revInfoIndex = 0;
                gravatarSpan = 1;
                revInfoSpan = 2;
            }
            else
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle());
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                gravatarIndex = 0;
                revInfoIndex = 1;
                gravatarSpan = 2;
                revInfoSpan = 1;
            }

            tableLayout.SetColumn(gravatar1, gravatarIndex);
            tableLayout.SetColumn(_RevisionHeader, revInfoIndex);
            tableLayout.SetColumn(RevisionInfo, revInfoIndex);
            tableLayout.SetRowSpan(gravatar1, gravatarSpan);
            tableLayout.SetColumnSpan(RevisionInfo, revInfoSpan);
            tableLayout.ResumeLayout(true);
        }

        public void DisplayAvatarOnRight()
        {
            DisplayAvatarSetup(true);
        }

        public void DisplayAvatarOnLeft()
        {
            DisplayAvatarSetup(false);
        }

        private void UpdateRevisionInfo()
        {
            if (_sortedRefs != null)
            {
                if (_annotatedTagsMessages != null &&
                    _annotatedTagsMessages.Count > 0 &&
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

            string body = _revisionInfo;
            if (Revision != null && !Revision.IsArtificial)
            {
                body += "\n" + _annotatedTagsInfo + _linksInfo + _branchInfo + _tagInfo + _gitDescribeInfo;
            }

            RevisionInfo.SuspendLayout();
            RevisionInfo.SetXHTMLText(body);
            RevisionInfo.SelectionStart = 0; // scroll up
            RevisionInfo.ScrollToCaret();    // scroll up
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
                if (annotatedTagsMessages.TryGetValue(tag, out var annotatedContents))
                {
                    result += "<u>" + tag + "</u>: " + annotatedContents + Environment.NewLine;
                }
            }

            if (result.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return Environment.NewLine + result;
        }

        private void ResetTextAndImage()
        {
            _revisionInfo = string.Empty;
            _linksInfo = string.Empty;
            _branchInfo = string.Empty;
            _annotatedTagsInfo = string.Empty;
            _tagInfo = string.Empty;
            _gitDescribeInfo = string.Empty;
            _branches = null;
            _annotatedTagsMessages = null;
            _tags = null;
            _linkFactory.Clear();
            UpdateRevisionInfo();
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
            {
                return;
            }

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
                    {
                        noPrefixBranch = branch.Substring(remotesPrefix.Length);
                    }
                }
                else
                {
                    branchIsLocal = !getRemote;
                }

                if ((branchIsLocal && allowLocal) || (!branchIsLocal && allowRemote))
                {
                    var branchText = showBranchesAsLinks
                        ? _linkFactory.CreateBranchLink(noPrefixBranch)
                        : WebUtility.HtmlEncode(noPrefixBranch);

                    links.Add(branchText);
                }

                if (branchIsLocal && AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)
                {
                    allowRemote = false;
                }
            }

            if (links.Any())
            {
                return Environment.NewLine + WebUtility.HtmlEncode(_containedInBranches.Text) + " " + links.Join(", ");
            }

            return Environment.NewLine + WebUtility.HtmlEncode(_containedInNoBranch.Text);
        }

        private string GetTagsWhichContainsThisCommit(IEnumerable<string> tags, bool showBranchesAsLinks)
        {
            var tagString = tags
                .Select(s => showBranchesAsLinks ? _linkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s)).Join(", ");

            if (!string.IsNullOrEmpty(tagString))
            {
                return Environment.NewLine + WebUtility.HtmlEncode(_containedInTags.Text) + " " + tagString;
            }

            return Environment.NewLine + WebUtility.HtmlEncode(_containedInNoTag.Text);
        }

        private string GetLinksForRevision(GitRevision revision)
        {
            var links = _gitRevisionExternalLinksParser.Parse(revision, Module.EffectiveSettings).Distinct();
            var linksString = string.Empty;

            foreach (var link in links)
            {
                linksString = linksString.Combine(", ", _linkFactory.CreateLink(link.Caption, link.URI));
            }

            if (linksString.IsNullOrEmpty())
            {
                return string.Empty;
            }
            else
            {
                return Environment.NewLine + WebUtility.HtmlEncode(_trsLinksRelatedToRevision.Text) + " " + linksString;
            }
        }

        private string GetDescribeInfoForRevision(string revision)
        {
            var (precedingTag, commitCount) = _gitDescribeProvider.Get(revision);

            StringBuilder gitDescribeInfo = new StringBuilder();
            if (!string.IsNullOrEmpty(precedingTag))
            {
                string tagString = ShowBranchesAsLinks ? _linkFactory.CreateTagLink(precedingTag) : WebUtility.HtmlEncode(precedingTag);
                gitDescribeInfo.Append(Environment.NewLine + WebUtility.HtmlEncode(_derivesFromTag.Text) + " " + tagString);
                if (!string.IsNullOrEmpty(commitCount))
                {
                    gitDescribeInfo.Append(" + " + commitCount + " " + WebUtility.HtmlEncode(_plusCommits.Text));
                }
            }
            else
            {
                gitDescribeInfo.Append(Environment.NewLine + WebUtility.HtmlEncode(_derivesFromNoTag.Text));
            }

            return gitDescribeInfo.ToString();
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

        private void showTagThisCommitDerivesFromMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.CommitInfoShowTagThisCommitDerivesFrom = !AppSettings.CommitInfoShowTagThisCommitDerivesFrom;
            ReloadCommitInfo();
        }

        private void copyCommitInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var commitInfo = $"{_RevisionHeader.GetPlaintText()}{Environment.NewLine}{RevisionInfo.GetPlaintText()}";
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
            CommandClick?.Invoke(this, new CommandEventArgs(command, data));
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
