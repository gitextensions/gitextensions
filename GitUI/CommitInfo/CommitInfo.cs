using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Editor.RichTextBoxExtension;
using GitUI.Hotkey;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.CommitInfo
{
    public partial class CommitInfo : GitModuleControl
    {
        private event EventHandler<CommandEventArgs> CommandClickedEvent;

        public event EventHandler<CommandEventArgs> CommandClicked
        {
            add
            {
                CommandClickedEvent += value;
                commitInfoHeader.CommandClicked += value;
            }
            remove
            {
                CommandClickedEvent -= value;
                commitInfoHeader.CommandClicked -= value;
            }
        }

        private static readonly TranslationString _copyLink = new TranslationString("Copy &link ({0})");
        private static readonly TranslationString _containedInBranches = new TranslationString("Contained in branches:");
        private static readonly TranslationString _containedInNoBranch = new TranslationString("Contained in no branch");
        private static readonly TranslationString _containedInTags = new TranslationString("Contained in tags:");
        private static readonly TranslationString _containedInNoTag = new TranslationString("Contained in no tag");
        private static readonly TranslationString _trsLinksRelatedToRevision = new TranslationString("Related links:");
        private static readonly TranslationString _derivesFromTag = new TranslationString("Derives from tag:");
        private static readonly TranslationString _derivesFromNoTag = new TranslationString("Derives from no tag");
        private static readonly TranslationString _plusCommits = new TranslationString("commits");

        private const int MaximumDisplayedRefs = 20;
        private readonly ILinkFactory _linkFactory = new LinkFactory();
        private readonly ICommitDataManager _commitDataManager;
        private readonly ICommitDataBodyRenderer _commitDataBodyRenderer;
        private readonly IExternalLinksStorage _externalLinksStorage;
        private readonly IConfiguredLinkDefinitionsProvider _effectiveLinkDefinitionsProvider;
        private readonly IGitRevisionExternalLinksParser _gitRevisionExternalLinksParser;
        private readonly IExternalLinkRevisionParser _externalLinkRevisionParser;
        private readonly IGitRemoteManager _gitRemoteManager;
        private readonly GitDescribeProvider _gitDescribeProvider;
        private readonly CancellationTokenSequence _asyncLoadCancellation = new CancellationTokenSequence();

        private readonly IDisposable _revisionInfoResizedSubscription;
        private readonly IDisposable _commitMessageResizedSubscription;

        private GitRevision _revision;
        private IReadOnlyList<ObjectId> _children;
        private string _linksInfo;
        private IDictionary<string, string> _annotatedTagsMessages;
        private string _annotatedTagsInfo;
        private List<string> _tags;
        private string _tagInfo;
        private List<string> _branches;
        private string _branchInfo;
        private string _gitDescribeInfo;
        [CanBeNull] private IDictionary<string, int> _refsOrderDict;
        private int _revisionInfoHeight;
        private int _commitMessageHeight;

        [DefaultValue(false)]
        public bool ShowBranchesAsLinks { get; set; }

        public CommitInfo()
        {
            InitializeComponent();
            InitializeComplete();

            UICommandsSourceSet += delegate { this.InvokeAsync(() => ReloadCommitInfo()).FileAndForget(); };

            _commitDataManager = new CommitDataManager(() => Module);

            _commitDataBodyRenderer = new CommitDataBodyRenderer(() => Module, _linkFactory);
            _externalLinksStorage = new ExternalLinksStorage();
            _effectiveLinkDefinitionsProvider = new ConfiguredLinkDefinitionsProvider(_externalLinksStorage);
            _gitRemoteManager = new GitRemoteManager(() => Module);
            _externalLinkRevisionParser = new ExternalLinkRevisionParser(_gitRemoteManager);
            _gitRevisionExternalLinksParser = new GitRevisionExternalLinksParser(_effectiveLinkDefinitionsProvider, _externalLinkRevisionParser);
            _gitDescribeProvider = new GitDescribeProvider(() => Module);

            var color = SystemColors.Window.MakeColorDarker(0.04);
            pnlCommitMessage.BackColor = color;
            rtbxCommitMessage.BackColor = color;
            rtbxCommitMessage.Font = AppSettings.CommitFont;
            RevisionInfo.Font = AppSettings.Font;

            Hotkeys = HotkeySettingsManager.LoadHotkeys(FormBrowse.HotkeySettingsName);
            addNoteToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys((int)FormBrowse.Command.AddNotes).ToShortcutKeyDisplayString();

            _commitMessageResizedSubscription = subscribeToContentsResized(rtbxCommitMessage, CommitMessage_ContentsResized);
            _revisionInfoResizedSubscription = subscribeToContentsResized(RevisionInfo, RevisionInfo_ContentsResized);

            IDisposable subscribeToContentsResized(RichTextBox richTextBox, Action<ContentsResizedEventArgs> handler) =>
                Observable
                    .FromEventPattern<ContentsResizedEventHandler, ContentsResizedEventArgs>(
                        h => richTextBox.ContentsResized += h,
                        h => richTextBox.ContentsResized -= h)
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .ObserveOn(MainThreadScheduler.Instance)
                    .Subscribe(_ => handler(_.EventArgs));

            commitInfoHeader.SetContextMenuStrip(commitInfoContextMenuStrip);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitRevision Revision
        {
            get => _revision;
            set => SetRevisionWithChildren(value, null);
        }

        private void RevisionInfoLinkClicked(object sender, LinkClickedEventArgs e)
        {
            var link = _linkFactory.ParseLink(e.LinkText);

            try
            {
                var result = new Uri(link);
                if (result.Scheme == "gitext")
                {
                    CommandClickedEvent?.Invoke(sender, new CommandEventArgs(result.Host, result.AbsolutePath.TrimStart('/')));
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

        public void SetRevisionWithChildren(GitRevision revision, IReadOnlyList<ObjectId> children)
        {
            _revision = revision;
            _children = children;

            if (revision == null)
            {
                tableLayout.Visible = false;
                return;
            }

            tableLayout.Visible = true;
            commitInfoHeader.ShowCommitInfo(revision, children);
            ReloadCommitInfo();
        }

        private void ReloadCommitInfo()
        {
            showContainedInBranchesToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesLocal;
            showContainedInBranchesRemoteToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemote;
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            showContainedInTagsToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInTags;
            showMessagesOfAnnotatedTagsToolStripMenuItem.Checked = AppSettings.ShowAnnotatedTagsMessages;
            showTagThisCommitDerivesFromMenuItem.Checked = AppSettings.CommitInfoShowTagThisCommitDerivesFrom;

            _branches = null;
            _annotatedTagsMessages = null;
            _tags = null;
            _linkFactory.Clear();

            UpdateCommitMessage();

            _annotatedTagsInfo = "";
            _linksInfo = "";
            _branchInfo = "";
            _tagInfo = "";
            _gitDescribeInfo = "";

            if (_revision != null && !_revision.IsArtificial)
            {
                UpdateRevisionInfo();
                StartAsyncDataLoad();
            }

            void UpdateCommitMessage()
            {
                var data = _commitDataManager.CreateFromRevision(_revision, _children);

                if (_revision != null && (_revision.Body == null || (AppSettings.ShowGitNotes && !_revision.HasNotes)))
                {
                    _commitDataManager.UpdateBody(data, out _);
                    _revision.Body = data.Body;
                    _revision.HasNotes = true;
                }

                var commitMessage = _commitDataBodyRenderer.Render(data, showRevisionsAsLinks: CommandClickedEvent != null);

                // workaround the problem that with some DPI RichTextBox size height
                // passed to ContentResized event is ~1 line less than needed
                rtbxCommitMessage.SetXHTMLText(commitMessage + Environment.NewLine);
            }

            void StartAsyncDataLoad()
            {
                var cancellationToken = _asyncLoadCancellation.Next();

                ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadLinksForRevisionAsync(_revision)).FileAndForget();

                if (_refsOrderDict == null)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadSortedRefsAsync()).FileAndForget();
                }

                // No branch/tag data for artificial commands

                if (AppSettings.CommitInfoShowContainedInBranches)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadBranchInfoAsync(_revision.ObjectId)).FileAndForget();
                }

                if (AppSettings.ShowAnnotatedTagsMessages)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadAnnotatedTagInfoAsync(_revision.Refs)).FileAndForget();
                }

                if (AppSettings.CommitInfoShowContainedInTags)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadTagInfoAsync(_revision.ObjectId)).FileAndForget();
                }

                if (AppSettings.CommitInfoShowTagThisCommitDerivesFrom)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadDescribeInfoAsync(_revision.ObjectId)).FileAndForget();
                }

                return;

                async Task LoadLinksForRevisionAsync(GitRevision revision)
                {
                    if (revision == null)
                    {
                        return;
                    }

                    await TaskScheduler.Default;
                    var linksInfo = GetLinksForRevision();

                    // Most commits do not have link; do not switch to main thread if nothing is changed
                    if (_linksInfo == linksInfo)
                    {
                        return;
                    }

                    await this.SwitchToMainThreadAsync(cancellationToken);
                    _linksInfo = linksInfo;
                    UpdateRevisionInfo();

                    return;

                    string GetLinksForRevision()
                    {
                        var links = _gitRevisionExternalLinksParser.Parse(revision, Module.EffectiveSettings);
                        var result = string.Join(", ", links.Distinct().Select(link => _linkFactory.CreateLink(link.Caption, link.Uri)));

                        if (string.IsNullOrEmpty(result))
                        {
                            return "";
                        }

                        return $"{WebUtility.HtmlEncode(_trsLinksRelatedToRevision.Text)} {result}";
                    }
                }

                async Task LoadSortedRefsAsync()
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                    _refsOrderDict = ToDictionary(Module.GetSortedRefs());

                    await this.SwitchToMainThreadAsync(cancellationToken);
                    UpdateRevisionInfo();

                    IDictionary<string, int> ToDictionary(IReadOnlyList<string> list)
                    {
                        var dict = new Dictionary<string, int>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            dict.Add(list[i], i);
                        }

                        return dict;
                    }
                }

                async Task LoadAnnotatedTagInfoAsync(IReadOnlyList<IGitRef> refs)
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                    var annotatedTagsMessages = GetAnnotatedTagsMessages();

                    await this.SwitchToMainThreadAsync(cancellationToken);
                    _annotatedTagsMessages = annotatedTagsMessages;
                    UpdateRevisionInfo();

                    return;

                    IDictionary<string, string> GetAnnotatedTagsMessages()
                    {
                        if (refs == null)
                        {
                            return null;
                        }

                        var result = new Dictionary<string, string>();

                        foreach (var gitRef in refs)
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
                            // GitRevision.Refs by design contains GitRefs where Guids are equal to the GitRevision.Guid,
                            // so this collection contains only dereferencing GitRef's - just because GitRef_2 has the same
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
                }

                async Task LoadTagInfoAsync(ObjectId objectId)
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                    var tags = Module.GetAllTagsWhichContainGivenCommit(objectId).ToList();

                    await this.SwitchToMainThreadAsync(cancellationToken);
                    _tags = tags;
                    UpdateRevisionInfo();
                }

                async Task LoadBranchInfoAsync(ObjectId revision)
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                    // Include local branches if explicitly requested or when needed to decide whether to show remotes
                    bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                                    AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;

                    // Include remote branches if requested
                    bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                                     AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
                    var branches = Module.GetAllBranchesWhichContainGivenCommit(revision, getLocal, getRemote).ToList();

                    await this.SwitchToMainThreadAsync(cancellationToken);
                    _branches = branches;
                    UpdateRevisionInfo();
                }

                async Task LoadDescribeInfoAsync(ObjectId commitId)
                {
                    await TaskScheduler.Default;
                    var info = GetDescribeInfoForRevision();

                    await this.SwitchToMainThreadAsync(cancellationToken);
                    _gitDescribeInfo = info;
                    UpdateRevisionInfo();

                    return;

                    string GetDescribeInfoForRevision()
                    {
                        var (precedingTag, commitCount) = _gitDescribeProvider.Get(commitId);

                        var gitDescribeInfo = new StringBuilder();
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
                }
            }
        }

        private void UpdateRevisionInfo()
        {
            if (_refsOrderDict != null)
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

                    thisRevisionTagNames.Sort(new ItemTpComparer(_refsOrderDict, "refs/tags/"));
                    _annotatedTagsInfo = GetAnnotatedTagsInfo(thisRevisionTagNames, _annotatedTagsMessages);
                }

                if (_tags != null && string.IsNullOrEmpty(_tagInfo))
                {
                    _tags.Sort(new ItemTpComparer(_refsOrderDict, "refs/tags/"));
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
                    _branches.Sort(new ItemTpComparer(_refsOrderDict, "refs/heads/"));
                    if (_branches.Count > MaximumDisplayedRefs)
                    {
                        _branches[MaximumDisplayedRefs - 2] = "…";
                        _branches[MaximumDisplayedRefs - 1] = _branches[_branches.Count - 1];
                        _branches.RemoveRange(MaximumDisplayedRefs, _branches.Count - MaximumDisplayedRefs);
                    }

                    _branchInfo = GetBranchesWhichContainsThisCommit(_branches, ShowBranchesAsLinks);
                }
            }

            string body = string.Join(Environment.NewLine,
                new[] { _annotatedTagsInfo, _linksInfo, _branchInfo, _tagInfo, _gitDescribeInfo }
                    .Where(_ => !string.IsNullOrEmpty(_)));

            RevisionInfo.SetXHTMLText(body);
            return;

            string GetAnnotatedTagsInfo(
                IEnumerable<string> tagNames,
                IDictionary<string, string> annotatedTagsMessages)
            {
                var result = new StringBuilder();

                foreach (var tag in tagNames)
                {
                    if (annotatedTagsMessages.TryGetValue(tag, out var annotatedContents))
                    {
                        result.Append("<u>").Append(tag).Append("</u>: ").Append(annotatedContents).AppendLine();
                    }
                }

                return result.ToString();
            }

            string GetBranchesWhichContainsThisCommit(IEnumerable<string> branches, bool showBranchesAsLinks)
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
                    return WebUtility.HtmlEncode(_containedInBranches.Text) + " " + links.Join(", ");
                }

                return WebUtility.HtmlEncode(_containedInNoBranch.Text);
            }

            string GetTagsWhichContainsThisCommit(IEnumerable<string> tags, bool showBranchesAsLinks)
            {
                var tagString = tags
                    .Select(s => showBranchesAsLinks ? _linkFactory.CreateTagLink(s) : WebUtility.HtmlEncode(s)).Join(", ");

                if (!string.IsNullOrEmpty(tagString))
                {
                    return WebUtility.HtmlEncode(_containedInTags.Text) + " " + tagString;
                }

                return WebUtility.HtmlEncode(_containedInNoTag.Text);
            }
        }

        private void commitInfoContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var rtb = (sender as ContextMenuStrip)?.SourceControl as RichTextBox;
            if (rtb == null)
            {
                copyLinkToolStripMenuItem.Visible = false;
                return;
            }

            int charIndex = rtb.GetCharIndexFromPosition(rtb.PointToClient(MousePosition));
            string link = rtb.GetLink(charIndex);
            copyLinkToolStripMenuItem.Visible = link != null;
            copyLinkToolStripMenuItem.Text = string.Format(_copyLink.Text, link);
            copyLinkToolStripMenuItem.Tag = link;
        }

        private void copyLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipboardUtil.TrySetText(copyLinkToolStripMenuItem.Tag as string);
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
            var commitInfo = $"{commitInfoHeader.GetPlainText()}{Environment.NewLine}{Environment.NewLine}{rtbxCommitMessage.GetPlainText()}{Environment.NewLine}{RevisionInfo.GetPlainText()}";
            ClipboardUtil.TrySetText(commitInfo);
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
            Module.EditNotes(_revision.ObjectId);
            _revision.HasNotes = false;
            _revision.Body = null;
            ReloadCommitInfo();
        }

        private void _RevisionHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                DoCommandClick("navigatebackward");
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                DoCommandClick("navigateforward");
            }

            void DoCommandClick(string command)
            {
                CommandClickedEvent?.Invoke(this, new CommandEventArgs(command, null));
            }
        }

        private void RevisionInfo_ContentsResized(ContentsResizedEventArgs e)
        {
            _revisionInfoHeight = e.NewRectangle.Height;
            PerformLayout();
        }

        private void CommitMessage_ContentsResized(ContentsResizedEventArgs e)
        {
            _commitMessageHeight = e.NewRectangle.Height;
            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            tableLayout.SuspendLayout();

            int[] heights =
            {
                commitInfoHeader.Height + commitInfoHeader.Margin.Vertical,
                _commitMessageHeight + rtbxCommitMessage.Margin.Vertical + pnlCommitMessage.Margin.Vertical,
                _revisionInfoHeight + RevisionInfo.Margin.Vertical
            };

            // leave 1st row SizeType = AutoWidth to let CommitInfoHeader.AutoSize be correctly applied
            for (int i = 1; i < tableLayout.RowStyles.Count; i++)
            {
                tableLayout.RowStyles[i].SizeType = SizeType.Absolute;
                tableLayout.RowStyles[i].Height = heights[i];
            }

            int height = heights.Sum();

            int clientWidth = Width;
            if (height > Height)
            {
                clientWidth -= SystemInformation.VerticalScrollBarWidth;
            }

            int width = Math.Max(clientWidth, commitInfoHeader.Width + commitInfoHeader.Margin.Horizontal);

            tableLayout.ColumnStyles[0].SizeType = SizeType.Absolute;
            tableLayout.ColumnStyles[0].Width = width;

            tableLayout.Size = new Size(width, height);

            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();

            base.OnLayout(e);
        }

        private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var rtb = sender as RichTextBox;
            if (rtb == null || !e.Control || e.KeyCode != Keys.C)
            {
                return;
            }

            // Override RichTextBox Ctrl-c handling to copy plain text
            ClipboardUtil.TrySetText(rtb.GetSelectionPlainText());
            e.Handled = true;
        }

        protected override void DisposeCustomResources()
        {
            _asyncLoadCancellation.Dispose();
            _revisionInfoResizedSubscription.Dispose();
            _commitMessageResizedSubscription.Dispose();
            base.DisposeCustomResources();
        }

        private sealed class ItemTpComparer : IComparer<string>
        {
            private readonly IDictionary<string, int> _orderDict;
            private readonly string _prefix;

            public ItemTpComparer(IDictionary<string, int> orderDict, string prefix)
            {
                _orderDict = orderDict;
                _prefix = prefix;
            }

            public int Compare(string a, string b)
            {
                return IndexOf(a) - IndexOf(b);

                int IndexOf(string s)
                {
                    if (s.StartsWith("remotes/"))
                    {
                        s = "refs/" + s;
                    }
                    else
                    {
                        s = _prefix + s;
                    }

                    if (_orderDict.TryGetValue(s, out var index))
                    {
                        return index;
                    }

                    return -1;
                }
            }
        }
    }
}
