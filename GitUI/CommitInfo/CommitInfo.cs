using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
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

        private static readonly TranslationString _brokenRefs = new TranslationString("The repository refs seem to be broken:");
        private static readonly TranslationString _copyLink = new TranslationString("Copy &link ({0})");
        private static readonly TranslationString _trsLinksRelatedToRevision = new TranslationString("Related links:");
        private static readonly TranslationString _derivesFromTag = new TranslationString("Derives from tag:");
        private static readonly TranslationString _derivesFromNoTag = new TranslationString("Derives from no tag");
        private static readonly TranslationString _plusCommits = new TranslationString("commits");
        private static readonly TranslationString _repoFailure = new TranslationString("Repository failure");

        private readonly ILinkFactory _linkFactory = new LinkFactory();
        private readonly ICommitDataManager _commitDataManager;
        private readonly ICommitDataBodyRenderer _commitDataBodyRenderer;
        private readonly IExternalLinksStorage _externalLinksStorage;
        private readonly IConfiguredLinkDefinitionsProvider _effectiveLinkDefinitionsProvider;
        private readonly IGitRevisionExternalLinksParser _gitRevisionExternalLinksParser;
        private readonly IExternalLinkRevisionParser _externalLinkRevisionParser;
        private readonly IConfigFileRemoteSettingsManager _remotesManager;
        private readonly GitDescribeProvider _gitDescribeProvider;
        private readonly CancellationTokenSequence _asyncLoadCancellation = new CancellationTokenSequence();
        private readonly RefsFormatter _refsFormatter;

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
        [CanBeNull] private IDictionary<string, int> _tagsOrderDict;
        private int _revisionInfoHeight;
        private int _commitMessageHeight;
        private bool _showAllBranches;
        private bool _showAllTags;

        [DefaultValue(false)]
        public bool ShowBranchesAsLinks { get; set; }

        public CommitInfo()
        {
            InitializeComponent();
            InitializeComplete();

            UICommandsSourceSet += delegate
            {
                this.InvokeAsync(() =>
                {
                    UICommandsSource.UICommandsChanged += delegate { RefreshSortedTags(); };

                    // call this event handler also now (necessary for "Contained in branches/tags")
                    RefreshSortedTags();
                }).FileAndForget();
            };

            _commitDataManager = new CommitDataManager(() => Module);

            _commitDataBodyRenderer = new CommitDataBodyRenderer(() => Module, _linkFactory);
            _externalLinksStorage = new ExternalLinksStorage();
            _effectiveLinkDefinitionsProvider = new ConfiguredLinkDefinitionsProvider(_externalLinksStorage);
            _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
            _externalLinkRevisionParser = new ExternalLinkRevisionParser(_remotesManager);
            _gitRevisionExternalLinksParser = new GitRevisionExternalLinksParser(_effectiveLinkDefinitionsProvider, _externalLinkRevisionParser);
            _gitDescribeProvider = new GitDescribeProvider(() => Module);
            _refsFormatter = new RefsFormatter(_linkFactory);

            var messageBackground = KnownColor.Window.MakeBackgroundDarkerBy(0.04);
            pnlCommitMessage.BackColor = messageBackground;
            rtbxCommitMessage.BackColor = messageBackground;

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

            // at this point rtbxCommitMessage.Bounds = {X = 8 Y = 8 Width = 440 Height = 0}
            // and with Height=0 we won't be receiving any ContentsResizedEvents
            rtbxCommitMessage.Height = 1;
        }

        private void RefreshSortedTags()
        {
            if (!Module.IsValidGitWorkingDir())
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await LoadSortedTagsAsync();
            }).FileAndForget();
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
            try
            {
                _linkFactory.ExecuteLink(e.LinkText, commandEventArgs => CommandClickedEvent?.Invoke(sender, commandEventArgs), ShowAll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void ShowAll(string what)
        {
            switch (what)
            {
                case "branches":
                    _showAllBranches = true;
                    _branchInfo = null; // forces update
                    break;
                case "tags":
                    _showAllTags = true;
                    _tagInfo = null; // forces update
                    break;
                default:
                    Debug.Fail("unsupported type in ShowAll(\"" + what + "\")");
                    return;
            }

            UpdateRevisionInfo();
        }

        private IDictionary<string, int> GetSortedTags()
        {
            var args = new GitArgumentBuilder("for-each-ref")
            {
                "--sort=-taggerdate",
                "--format=\"%(refname)\"",
                "refs/tags/"
            };

            string tree = Module.GitExecutable.GetOutput(args);
            int warningPos = tree.IndexOf("warning:");
            if (warningPos >= 0)
            {
                throw new RefsWarningException(tree.Substring(warningPos).SplitLines()[0]);
            }

            int i = 0;
            var dict = new Dictionary<string, int>();
            foreach (var entry in tree.Split('\n'))
            {
                if (dict.ContainsKey(entry))
                {
                    continue;
                }

                dict.Add(entry, i);
                i++;
            }

            return dict;
        }

        private async Task LoadSortedTagsAsync()
        {
            ThreadHelper.AssertOnUIThread();
            _tagsOrderDict = null;

            await TaskScheduler.Default.SwitchTo();
            try
            {
                var tagsOrderDict = GetSortedTags();

                await this.SwitchToMainThreadAsync();
                _tagsOrderDict = tagsOrderDict;
                UpdateRevisionInfo();
            }
            catch (RefsWarningException ex)
            {
                await this.SwitchToMainThreadAsync();
                MessageBox.Show(this, string.Format("{0}{1}{1}{2}", _brokenRefs.Text, Environment.NewLine, ex.Message), _repoFailure.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return;
        }

        private void ReloadCommitInfo()
        {
            showContainedInBranchesToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesLocal;
            showContainedInBranchesRemoteToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemote;
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            showContainedInTagsToolStripMenuItem.Checked = AppSettings.CommitInfoShowContainedInTags;
            showMessagesOfAnnotatedTagsToolStripMenuItem.Checked = AppSettings.ShowAnnotatedTagsMessages;
            showTagThisCommitDerivesFromMenuItem.Checked = AppSettings.CommitInfoShowTagThisCommitDerivesFrom;

            _showAllBranches = false;
            _showAllTags = false;
            _branches = null;
            _tags = null;
            _annotatedTagsMessages = null;
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

            return;

            void UpdateCommitMessage()
            {
                if (_revision == null)
                {
                    rtbxCommitMessage.SetXHTMLText(string.Empty);
                    return;
                }

                var data = _commitDataManager.CreateFromRevision(_revision, _children);

                if (_revision.Body == null || (AppSettings.ShowGitNotes && !_revision.HasNotes))
                {
                    _commitDataManager.UpdateBody(data, out _);
                    _revision.Body = data.Body;
                    _revision.HasNotes = true;
                }

                var commitMessage = _commitDataBodyRenderer.Render(data, showRevisionsAsLinks: CommandClickedEvent != null);
                rtbxCommitMessage.SetXHTMLText(commitMessage);
            }

            void StartAsyncDataLoad()
            {
                var cancellationToken = _asyncLoadCancellation.Next();

                var initialRevision = _revision;

                ThreadHelper.JoinableTaskFactory.RunAsync(async () => { await LoadLinksForRevisionAsync(initialRevision); }).FileAndForget();

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    // No branch/tag data for artificial commands
                    if (AppSettings.CommitInfoShowContainedInBranches)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await LoadBranchInfoAsync(initialRevision.ObjectId);
                    }

                    if (AppSettings.ShowAnnotatedTagsMessages)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await LoadAnnotatedTagInfoAsync(initialRevision.Refs);
                    }

                    if (AppSettings.CommitInfoShowContainedInTags)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await LoadTagInfoAsync(initialRevision.ObjectId);
                    }

                    if (AppSettings.CommitInfoShowTagThisCommitDerivesFrom)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await LoadDescribeInfoAsync(initialRevision.ObjectId);
                    }
                }).FileAndForget();

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

                async Task LoadAnnotatedTagInfoAsync(IReadOnlyList<IGitRef> refs)
                {
                    await TaskScheduler.Default;

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
                    await TaskScheduler.Default;

                    var tags = Module.GetAllTagsWhichContainGivenCommit(objectId).ToList();

                    await this.SwitchToMainThreadAsync(cancellationToken);
                    _tags = tags;
                    UpdateRevisionInfo();
                }

                async Task LoadBranchInfoAsync(ObjectId revision)
                {
                    await TaskScheduler.Default;

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
            if (_tagsOrderDict != null)
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

                    thisRevisionTagNames.Sort(new TagsComparer(_tagsOrderDict));
                    _annotatedTagsInfo = GetAnnotatedTagsInfo(thisRevisionTagNames, _annotatedTagsMessages);
                }

                if (_tags != null && string.IsNullOrEmpty(_tagInfo))
                {
                    _tags.Sort(new TagsComparer(_tagsOrderDict));
                    _tagInfo = _refsFormatter.FormatTags(_tags, ShowBranchesAsLinks, limit: !_showAllTags);
                }
            }

            if (_branches != null && string.IsNullOrEmpty(_branchInfo))
            {
                _branches.Sort(new BranchComparer(Module.GetSelectedBranch()));
                _branchInfo = _refsFormatter.FormatBranches(_branches, ShowBranchesAsLinks, limit: !_showAllBranches);
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

            // at scale factor of 150% there is rendering artifact - the last line is almost lost
            // at all other scaling factors from 100% to 350% everything is rendered as expected
            // see https://github.com/gitextensions/gitextensions/issues/6898 for more details
            //
            // absent of an obvious way to fix it, hardcode the fix - add an extra space at the bottom
            if (Math.Abs(DpiUtil.ScaleX - 1.5f) <= 0.01f)
            {
                _commitMessageHeight += 16;
            }

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
            try
            {
                _asyncLoadCancellation.Dispose();
                _revisionInfoResizedSubscription?.Dispose();
                _commitMessageResizedSubscription?.Dispose();

                base.DisposeCustomResources();
            }
            catch (InvalidOperationException)
            {
                // System.Reactive causes the app to fail with: 'Invoke or BeginInvoke cannot be called on a control until the window handle has been created.'
            }
        }

        internal sealed class BranchComparer : IComparer<string>
        {
            private const string RemoteBranchPrefix = "remotes/";

            private static readonly Regex _importantRepoRegex = new Regex($"^{RemoteBranchPrefix}(origin|upstream)/",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);
            private static readonly Regex _remoteMasterRegex = new Regex($"^{RemoteBranchPrefix}.*/master[^/]*$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

            private readonly string _currentBranch;

            public BranchComparer(string currentBranch)
            {
                _currentBranch = currentBranch;
            }

            public int Compare(string a, string b)
            {
                int priorityA = GetBranchPriority(a);
                int priorityB = GetBranchPriority(b);
                return priorityA == priorityB ? Comparer<string>.Default.Compare(a, b)
                    : priorityA - priorityB;
            }

            private int GetBranchPriority(string branch)
            {
                return branch == _currentBranch ? 0
                    : IsImportantLocalBranch() ? 1
                    : IsImportantRemoteBranch() ? IsImportantRepo() ? 2 : 3
                    : IsLocalBranch() ? 4
                    : IsImportantRepo() ? 5
                    : 6;

                // Note: This assumes that branches starting with "master" are important branches, this is not configurable.
                bool IsImportantLocalBranch() => branch.StartsWith("master");
                bool IsImportantRemoteBranch() => _remoteMasterRegex.IsMatch(branch);
                bool IsImportantRepo() => _importantRepoRegex.IsMatch(branch);
                bool IsLocalBranch() => !branch.StartsWith(RemoteBranchPrefix);
            }
        }

        private sealed class TagsComparer : IComparer<string>
        {
            private readonly IDictionary<string, int> _orderDict;
            private readonly string _prefix;

            public TagsComparer(IDictionary<string, int> orderDict, string prefix = "refs/tags/")
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

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly CommitInfo _commitInfo;

            public TestAccessor(CommitInfo commitInfo)
            {
                _commitInfo = commitInfo;
            }

            public IDictionary<string, int> GetSortedTags() => _commitInfo.GetSortedTags();
        }
    }
}
