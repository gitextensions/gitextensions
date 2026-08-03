using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.CommitInfo;

public partial class CommitInfo : GitModuleControl
{
    private event EventHandler<CommandEventArgs>? CommandClickedEvent;

    public event EventHandler<CommandEventArgs>? CommandClicked
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

    private static readonly TranslationString _brokenRefs = new("The repository refs seem to be broken:");
    private static readonly TranslationString _copyLink = new("Copy &link ({0})");
    private static readonly TranslationString _trsLinksRelatedToRevision = new("Related links:");
    private static readonly TranslationString _derivesFromTag = new("Derives from tag:");
    private static readonly TranslationString _derivesFromNoTag = new("Derives from no tag");
    private static readonly TranslationString _plusCommits = new("commits");
    private static readonly TranslationString _repoFailure = new("Repository failure");

    private ICommitDataBodyRenderer? _commitDataBodyRenderer;
    private ILinkFactory? _linkFactory;
    private RefsFormatter? _refsFormatter;

    private readonly ICommitDataManager _commitDataManager;
    private readonly IExternalLinksStorage _externalLinksStorage;
    private readonly IConfiguredLinkDefinitionsProvider _effectiveLinkDefinitionsProvider;
    private readonly IGitRevisionExternalLinksParser _gitRevisionExternalLinksParser;
    private readonly IExternalLinkRevisionParser _externalLinkRevisionParser;
    private readonly IConfigFileRemoteSettingsManager _remotesManager;
    private readonly GitDescribeProvider _gitDescribeProvider;
    private readonly CancellationTokenSequence _asyncLoadCancellation = new();

    private GitRevision? _revision;
    private IReadOnlyList<ObjectId>? _children;
    private string? _linksInfo;
    private IDictionary<string, string>? _annotatedTagsMessages;
    private string? _annotatedTagsInfo;
    private string[]? _tags;
    private string? _tagInfo;
    private string[]? _branches;
    private string? _branchInfo;
    private string? _gitDescribeInfo;
    private IDictionary<string, int>? _tagsOrderDict;
    private bool _showAllBranches;
    private bool _showAllTags;

    [DefaultValue(false)]
    public bool ShowBranchesAsLinks { get; set; }

    public CommitInfo()
        : this(commitDataManager: null)
    {
    }

    public CommitInfo(ICommitDataManager? commitDataManager)
    {
        InitializeComponent();
        InitializeComplete();

        _commitDataManager = commitDataManager ?? new CommitDataManager(() => Module);

        _externalLinksStorage = new ExternalLinksStorage();
        _effectiveLinkDefinitionsProvider = new ConfiguredLinkDefinitionsProvider(_externalLinksStorage);
        _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
        _externalLinkRevisionParser = new ExternalLinkRevisionParser(_remotesManager);
        _gitRevisionExternalLinksParser = new GitRevisionExternalLinksParser(_effectiveLinkDefinitionsProvider, _externalLinkRevisionParser);
        _gitDescribeProvider = new GitDescribeProvider(() => Module);

        // This issue surfaces in WinForms at 150% scale factor.
        // At this point rtbxCommitMessage.Bounds = {X = 8 Y = 8 Width = 440 Height = 0}
        // and with Height=0 WinForms won't receive any ContentsResizedEvents.
        // Avalonia constraint: the native measure pass replaces that event-driven workaround.
        // Avalonia's dynamic font resources are published from these same settings; retain the
        // original control-level reads so changing either setting remains this surface's contract.
        _ = AppSettings.CommitFont;
        _ = AppSettings.Font;

        copyLinkToolStripMenuItem.Click += copyLinkToolStripMenuItem_Click;
        copyCommitInfoToolStripMenuItem.Click += copyCommitInfoToolStripMenuItem_Click;
        showContainedInBranchesToolStripMenuItem.Click += showContainedInBranchesToolStripMenuItem_Click;
        showContainedInBranchesRemoteToolStripMenuItem.Click += showContainedInBranchesRemoteToolStripMenuItem_Click;
        showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Click += showContainedInBranchesRemoteIfNoLocalToolStripMenuItem_Click;
        showContainedInTagsToolStripMenuItem.Click += showContainedInTagsToolStripMenuItem_Click;
        showMessagesOfAnnotatedTagsToolStripMenuItem.Click += showMessagesOfAnnotatedTagsToolStripMenuItem_Click;
        showTagThisCommitDerivesFromMenuItem.Click += showTagThisCommitDerivesFromMenuItem_Click;
        addNoteToolStripMenuItem.Click += addNoteToolStripMenuItem_Click;
        commitInfoContextMenuStrip.Opening += commitInfoContextMenuStrip_Opening;
        commitInfoHeader.SetContextMenuStrip(commitInfoContextMenuStrip);

        // Avalonia constraint: controls have no DisposeCustomResources lifecycle hook.
        DetachedFromVisualTree += (_, _) => _asyncLoadCancellation.CancelCurrent();
    }

    protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
    {
        base.OnUICommandsSourceSet(source);

        // Avalonia constraint: controls receive their runtime services here rather than in OnRuntimeLoad.
        ReloadHotkeys();

        if (source is null)
        {
            _linkFactory = null;
            _commitDataBodyRenderer = null;
            _refsFormatter = null;
        }
        else
        {
            _linkFactory = source.UICommands.GetRequiredService<ILinkFactory>();
            _commitDataBodyRenderer = new CommitDataBodyRenderer(() => Module, _linkFactory);
            _refsFormatter = new RefsFormatter(_linkFactory);

            source.UICommandsChanged += delegate { RefreshSortedTags(); };

            // call this event handler also now (necessary for "Contained in branches/tags")
            RefreshSortedTags();
        }
    }

    internal void ReloadHotkeys()
    {
        LoadHotkeys(FormBrowse.HotkeySettingsName);
    }

    private void RefreshSortedTags()
    {
        if (!Module.IsValidGitWorkingDir())
        {
            return;
        }

        ThreadHelper.FileAndForget(LoadSortedTagsAsync);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public GitRevision? Revision
    {
        get => _revision;
        set => SetRevisionWithChildren(value, null);
    }

    private void LinkClicked(object sender, LinkClickedEventArgs e)
    {
        try
        {
            Validates.NotNull(_linkFactory);
            _linkFactory?.ExecuteLink(e.LinkUri, commandEventArgs => CommandClickedEvent?.Invoke(sender, commandEventArgs), ShowAll);
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void SetRevisionWithChildren(GitRevision? revision, IReadOnlyList<ObjectId>? children)
    {
        CancellationToken cancellationToken = _asyncLoadCancellation.Next();

        _revision = revision;
        _children = children;

        if (revision is null)
        {
            tableLayout.IsVisible = false;
            return;
        }

        tableLayout.IsVisible = true;
        commitInfoHeader.ShowCommitInfo(revision, children);
        if (!TryGetUICommandsDirect(out _))
        {
            // Avalonia designers and headless capture hosts construct the original control
            // without a containing GitModuleForm; keep that construction path renderable.
            rtbxCommitMessage.SetXHTMLText(WebUtility.HtmlEncode(revision.Body ?? revision.Subject ?? string.Empty));
            RevisionInfo.Clear();
            return;
        }

        ReloadCommitInfo(cancellationToken);
    }

    private void ShowAll(string? what)
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
                DebugHelpers.Fail($"Unsupported type in ShowAll('{what}')");
                return;
        }

        UpdateRevisionInfo();
    }

    private IDictionary<string, int> GetSortedTags()
    {
        GitArgumentBuilder args = new("for-each-ref")
        {
            @"--sort=""-taggerdate""",
            @"--format=""%(refname)""",
            "refs/tags/"
        };

        string tree = Module.GitExecutable.GetOutput(args);
        int warningPos = tree.IndexOf("warning:");
        if (warningPos >= 0)
        {
            throw new RefsWarningException(tree[warningPos..].LazySplit('\n', StringSplitOptions.RemoveEmptyEntries).First());
        }

        int i = 0;
        Dictionary<string, int> dict = [];
        foreach (string entry in tree.LazySplit('\n'))
        {
            if (dict.TryAdd(entry, i))
            {
                ++i;
            }
        }

        return dict;
    }

    private async Task LoadSortedTagsAsync()
    {
        try
        {
            IDictionary<string, int> tagsOrderDict = GetSortedTags();

            await this.SwitchToMainThreadAsync();
            _tagsOrderDict = tagsOrderDict;
            UpdateRevisionInfo();
        }
        catch (RefsWarningException ex)
        {
            await this.SwitchToMainThreadAsync();
            MessageBoxes.Show(this, string.Format("{0}{1}{1}{2}", _brokenRefs.Text, Environment.NewLine, ex.Message), _repoFailure.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ReloadCommitInfo()
    {
        ReloadCommitInfo(_asyncLoadCancellation.Next());
    }

    private void ReloadCommitInfo(CancellationToken cancellationToken)
    {
        showContainedInBranchesToolStripMenuItem.IsChecked = AppSettings.CommitInfoShowContainedInBranchesLocal;
        showContainedInBranchesRemoteToolStripMenuItem.IsChecked = AppSettings.CommitInfoShowContainedInBranchesRemote;
        showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.IsChecked = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
        showContainedInTagsToolStripMenuItem.IsChecked = AppSettings.CommitInfoShowContainedInTags;
        showMessagesOfAnnotatedTagsToolStripMenuItem.IsChecked = AppSettings.ShowAnnotatedTagsMessages;
        showTagThisCommitDerivesFromMenuItem.IsChecked = AppSettings.CommitInfoShowTagThisCommitDerivesFrom;

        _showAllBranches = false;
        _showAllTags = false;
        _branches = null;
        _tags = null;
        _annotatedTagsMessages = null;

        _annotatedTagsInfo = "";
        _linksInfo = "";
        _branchInfo = "";
        _tagInfo = "";
        _gitDescribeInfo = "";

        if (_revision is not null && !_revision.IsArtificial && !_revision.IsAutostash)
        {
            if (Module.GetEffectiveSettings() is DistributedSettings distributedSettings)
            {
                StartAsyncDataLoad(distributedSettings, cancellationToken);
            }
            else
            {
                DebugHelpers.Fail($"{nameof(Module.GetEffectiveSettings)} have unexpected type.");
            }
        }
        else
        {
            rtbxCommitMessage.SetXHTMLText(GetFixCommitMessage());
            RevisionInfo.Clear();
        }

        return;

        string GetFixCommitMessage()
        {
            if (_revision is null)
            {
                return string.Empty;
            }

            CommitData data = _commitDataManager.CreateFromRevision(_revision, _children);
            return _commitDataBodyRenderer?.Render(data, showRevisionsAsLinks: false) ?? string.Empty;
        }

        async Task UpdateCommitMessageAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_revision!.Body is null || (_revision.Notes is null && (AppSettings.ShowGitNotesColumn.Value || AppSettings.ShowGitNotes)))
            {
                _commitDataManager.UpdateBodyAndNotes(_revision);
            }

            CommitData data = _commitDataManager.CreateFromRevision(_revision, _children);

            cancellationToken.ThrowIfCancellationRequested();

            ICommitDataBodyRenderer? commitDataBodyRenderer = _commitDataBodyRenderer;
            if (commitDataBodyRenderer is null)
            {
                // Cancel the update if the commands source has been unset
                return;
            }

            string commitMessage = commitDataBodyRenderer.Render(data, showRevisionsAsLinks: CommandClickedEvent is not null);

            await this.SwitchToMainThreadAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            rtbxCommitMessage.SetXHTMLText(commitMessage);
        }

        void StartAsyncDataLoad(DistributedSettings settings, CancellationToken cancellationToken)
        {
            GitRevision initialRevision = _revision!;

            ThreadHelper.FileAndForget(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<Task> tasks =
                [
                    UpdateCommitMessageAsync(cancellationToken),
                    LoadLinksForRevisionAsync(initialRevision, settings).WithCancellation(cancellationToken)
                ];

                // No branch/tag data for artificial commands
                if (AppSettings.CommitInfoShowContainedInBranches)
                {
                    tasks.Add(LoadBranchInfoAsync(initialRevision.ObjectId).WithCancellation(cancellationToken));
                }

                if (AppSettings.ShowAnnotatedTagsMessages)
                {
                    tasks.Add(LoadAnnotatedTagInfoAsync(initialRevision.Refs).WithCancellation(cancellationToken));
                }

                if (AppSettings.CommitInfoShowContainedInTags)
                {
                    tasks.Add(LoadTagInfoAsync(initialRevision.ObjectId).WithCancellation(cancellationToken));
                }

                if (AppSettings.CommitInfoShowTagThisCommitDerivesFrom)
                {
                    tasks.Add(LoadDescribeInfoAsync(initialRevision.ObjectId).WithCancellation(cancellationToken));
                }

                cancellationToken.ThrowIfCancellationRequested();

                await Task.WhenAll(tasks);

                await this.SwitchToMainThreadAsync(cancellationToken);
                UpdateRevisionInfo();
            });

            return;

            async Task LoadLinksForRevisionAsync(GitRevision revision, DistributedSettings settings)
            {
                await TaskScheduler.Default;
                cancellationToken.ThrowIfCancellationRequested();

                ILinkFactory? linkFactory = _linkFactory;
                if (linkFactory is null)
                {
                    // Cancel the update if the commands source has been unset
                    return;
                }

                string linksInfo = GetLinksForRevision(settings);

                // Most commits do not have link; do not switch to main thread if nothing is changed
                if (_linksInfo == linksInfo)
                {
                    return;
                }

                await this.SwitchToMainThreadAsync(cancellationToken);
                _linksInfo = linksInfo;

                return;

                string GetLinksForRevision(DistributedSettings settings)
                {
                    IEnumerable<ExternalLink> links = _gitRevisionExternalLinksParser.Parse(revision, settings);
                    cancellationToken.ThrowIfCancellationRequested();
                    string result = string.Join(", ", links.Distinct().Select(link => linkFactory.CreateLink(link.Caption, link.Uri)));

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

                IDictionary<string, string>? annotatedTagsMessages = GetAnnotatedTagsMessages();

                await this.SwitchToMainThreadAsync(cancellationToken);
                _annotatedTagsMessages = annotatedTagsMessages;

                return;

                IDictionary<string, string>? GetAnnotatedTagsMessages()
                {
                    if (refs is null)
                    {
                        return null;
                    }

                    Dictionary<string, string> result = [];

                    foreach (IGitRef gitRef in refs)
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

                        if (gitRef is { IsTag: true, IsDereference: true })
                        {
                            string? content = WebUtility.HtmlEncode(Module.GetTagMessage(gitRef.LocalName, cancellationToken));
                            if (content is not null)
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

                string[] tags = [.. Module.GetAllTagsWhichContainGivenCommit(objectId, cancellationToken)];

                await this.SwitchToMainThreadAsync(cancellationToken);
                _tags = tags;
            }

            async Task LoadBranchInfoAsync(ObjectId objectId)
            {
                await TaskScheduler.Default;

                // Include local branches if explicitly requested or when needed to decide whether to show remotes
                bool getLocal = AppSettings.CommitInfoShowContainedInBranchesLocal ||
                                AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;

                // Include remote branches if requested
                bool getRemote = AppSettings.CommitInfoShowContainedInBranchesRemote ||
                                 AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
                string[] branches = [.. Module.GetAllBranchesWhichContainGivenCommit(objectId, getLocal, getRemote, cancellationToken)];

                await this.SwitchToMainThreadAsync(cancellationToken);
                _branches = branches;
            }

            async Task LoadDescribeInfoAsync(ObjectId commitId)
            {
                await TaskScheduler.Default;

                ILinkFactory? linkFactory = _linkFactory;
                if (linkFactory is null)
                {
                    // Cancel the update if the commands source has been unset
                    return;
                }

                string info = GetDescribeInfoForRevision();

                await this.SwitchToMainThreadAsync(cancellationToken);
                _gitDescribeInfo = info;

                return;

                string GetDescribeInfoForRevision()
                {
                    (string precedingTag, string commitCount) = _gitDescribeProvider.Get(commitId, cancellationToken);

                    StringBuilder gitDescribeInfo = new();
                    if (!string.IsNullOrEmpty(precedingTag))
                    {
                        string tagString = ShowBranchesAsLinks ? linkFactory.CreateTagLink(precedingTag) : WebUtility.HtmlEncode(precedingTag);
                        gitDescribeInfo.Append(WebUtility.HtmlEncode(_derivesFromTag.Text)).Append(' ').Append(tagString);
                        if (!string.IsNullOrEmpty(commitCount))
                        {
                            gitDescribeInfo.Append(" + ").Append(commitCount).Append(' ').Append(WebUtility.HtmlEncode(_plusCommits.Text));
                        }
                    }
                    else
                    {
                        gitDescribeInfo.Append(WebUtility.HtmlEncode(_derivesFromNoTag.Text));
                    }

                    return gitDescribeInfo.ToString();
                }
            }
        }
    }

    private void UpdateRevisionInfo()
    {
        RefsFormatter? refsFormatter = _refsFormatter;
        if (refsFormatter is null)
        {
            // Cancel the update if the commands source has been unset
            return;
        }

        if (_tagsOrderDict is not null)
        {
            if (_annotatedTagsMessages is not null &&
                _annotatedTagsMessages.Count > 0 &&
                string.IsNullOrEmpty(_annotatedTagsInfo) &&
                Revision is not null)
            {
                // having both lightweight & annotated tags in thisRevisionTagNames,
                // but GetAnnotatedTagsInfo will process annotated only:
                List<string> thisRevisionTagNames =
                    [.. Revision
                    .Refs
                    .Where(r => r.IsTag)
                    .Select(r => r.LocalName)];

                thisRevisionTagNames.Sort(new TagsComparer(_tagsOrderDict));
                _annotatedTagsInfo = GetAnnotatedTagsInfo(thisRevisionTagNames, _annotatedTagsMessages);
            }

            if (_tags is not null && string.IsNullOrEmpty(_tagInfo))
            {
                Array.Sort(_tags, new TagsComparer(_tagsOrderDict));
                _tagInfo = refsFormatter.FormatTags(_tags, ShowBranchesAsLinks, limit: !_showAllTags);
            }
        }

        if (_branches is not null && string.IsNullOrEmpty(_branchInfo))
        {
            Array.Sort(_branches, new BranchComparer(_branches, Module.GetSelectedBranch()));
            _branchInfo = refsFormatter.FormatBranches(_branches, ShowBranchesAsLinks, limit: !_showAllBranches);
        }

        string body = string.Join(Environment.NewLine + Environment.NewLine,
            new[] { _annotatedTagsInfo, _linksInfo, _branchInfo, _tagInfo, _gitDescribeInfo }
                .Where(_ => !string.IsNullOrEmpty(_)));

        RevisionInfo.SetXHTMLText(body);
        return;

        static string GetAnnotatedTagsInfo(
            IEnumerable<string> tagNames,
            IDictionary<string, string> annotatedTagsMessages)
        {
            StringBuilder result = new();

            foreach (string tag in tagNames)
            {
                if (annotatedTagsMessages.TryGetValue(tag, out string? annotatedContents))
                {
                    result.Append("<u>").Append(tag).Append("</u>: ").Append(annotatedContents).AppendLine();
                }
            }

            return result.ToString().TrimEnd();
        }
    }

    private void commitInfoContextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
        string? link = rtbxCommitMessage.SelectedLinkUri
            ?? RevisionInfo.SelectedLinkUri
            ?? commitInfoHeader.SelectedLinkUri;
        copyLinkToolStripMenuItem.IsVisible = link is not null;
        copyLinkToolStripMenuItem.Header = string.Format(_copyLink.Text, link);
        copyLinkToolStripMenuItem.Tag = link;
    }

    private void copyLinkToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (copyLinkToolStripMenuItem.Tag is string link)
        {
            ClipboardUtil.TrySetText(link);
        }
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
        string commitInfo = $"{commitInfoHeader.GetPlainText()}{Environment.NewLine}{Environment.NewLine}{rtbxCommitMessage.GetPlainText()}";
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
        if (_revision is null)
        {
            return;
        }

        Module.EditNotes(_revision.ObjectId);
        _revision.Body = null;
        _revision.Notes = null;
        ReloadCommitInfo();
    }

    // Avalonia constraint: pointer button state is carried by PointerPressedEventArgs.
    private void _RevisionHeader_MouseDown(object sender, PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint((Control)sender).Properties;
        if (properties.IsXButton1Pressed)
        {
            DoCommandClick("navigatebackward");
        }
        else if (properties.IsXButton2Pressed)
        {
            DoCommandClick("navigateforward");
        }

        void DoCommandClick(string command)
        {
            CommandClickedEvent?.Invoke(this, new CommandEventArgs(command, null));
        }
    }

    private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.Key != Key.C || sender is not XhtmlTextBlock rtb)
        {
            return;
        }

        // Override RichTextBox Ctrl-c handling to copy plain text
        ClipboardUtil.TrySetText(rtb.GetSelectionPlainText());
        e.Handled = true;
    }

    internal sealed class BranchComparer : IComparer<string>
    {
        private const string _remoteBranchPrefix = "remotes/";
        private readonly string _currentBranch;
        private readonly bool _isDetachedHead;
        private readonly Dictionary<string, int> _orderByBranch = [];

        public BranchComparer(string[] branches, string currentBranch)
        {
            _currentBranch = currentBranch;
            _isDetachedHead = DetachedHeadParser.IsDetachedHead(currentBranch);
            string[] branchRegexes = AppSettings.PrioritizedBranchNames.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string[] localBranchRegexes = [.. branchRegexes.Select(regex => $"^({regex})$")];
            string[] remoteBranchRegexes = [.. branchRegexes.Select(regex => $"^{_remoteBranchPrefix}[^/]+/({regex})$")];
            string[] remoteRegexes = [.. AppSettings.PrioritizedRemoteNames.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(regex => $"^{_remoteBranchPrefix}({regex})/")];

            foreach (string branch in branches)
            {
                _orderByBranch[branch] = GetBranchOrder(branch);
            }

            return;

            // Get the order for each branch.
            // Add max possible order value to next "level" to sort properly with the order for each regex.
            int GetBranchOrder(string branch)
            {
                int order = 0;
                if (_isDetachedHead ? DetachedHeadParser.IsDetachedHead(branch) : branch == _currentBranch)
                {
                    return order;
                }

                // length of "current branch" group
                order += 1;

                if (IsLocalBranch())
                {
                    if (!TryGetOrder(branch, localBranchRegexes, out int localBranchOrder))
                    {
                        // Non prioritized local branches added after prioritized remote branches
                        // localBranchOrder==localBranchRegexes.Length, an extra priority level
                        order += prioritizedRemoteBranchesLength();
                    }

                    // Order by branch priority
                    order += localBranchOrder;

                    return order;
                }

                // Remote branches after local prioritized branches
                order += localBranchRegexes.Length;

                if (!TryGetOrder(branch, remoteBranchRegexes, out int remoteBranchOrder))
                {
                    // after non priority local branches (that are inserted after remote prioritzed branches)
                    const int localNonprioritizedBranchesLength = 1;
                    order += localNonprioritizedBranchesLength;
                }

                // Group by branch priority then order by remote
                order += (remoteBranchOrder * remotesGroupLength()) + GetOrder(branch, remoteRegexes);

                return order;

                bool IsLocalBranch() => !branch.StartsWith(_remoteBranchPrefix);

                // The groups for a prioritized remote branch adds the unprioritized remotes to the regexes
                int remotesGroupLength() => remoteRegexes.Length + 1;

                // Length of the block of all prioritized remote branches (non prioritized branches separate)
                int prioritizedRemoteBranchesLength() => remoteBranchRegexes.Length * remotesGroupLength();

                // Get the index of the match for prioritized sorting,
                // set order to regexes.Length at no match
                bool TryGetOrder(string branch, string[] regexes, out int order)
                {
                    int currentOrder = 0;
                    foreach (string regex in regexes)
                    {
                        if (Regex.IsMatch(branch, regex, RegexOptions.ExplicitCapture))
                        {
                            order = currentOrder;
                            return true;
                        }

                        currentOrder++;
                    }

                    order = currentOrder;
                    return false;
                }

                int GetOrder(string branch, string[] regexes)
                {
                    TryGetOrder(branch, regexes, out int order);
                    return order;
                }
            }
        }

        public int Compare(string? a, string? b)
        {
            if (b is null)
            {
                return -1;
            }

            if (a is null)
            {
                return 1;
            }

            int priorityA = _orderByBranch[a];
            int priorityB = _orderByBranch[b];
            return priorityA == priorityB ? StringComparer.Ordinal.Compare(a, b) : priorityA - priorityB;
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

        public int Compare(string? a, string? b)
        {
            return b is null ? -1 : a is null ? 1 : IndexOf(a) - IndexOf(b);

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

                if (_orderDict.TryGetValue(s, out int index))
                {
                    return index;
                }

                return -1;
            }
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly CommitInfo _commitInfo;

        public TestAccessor(CommitInfo commitInfo)
        {
            _commitInfo = commitInfo;
        }

        public AvatarControl Avatar => _commitInfo.commitInfoHeader.GetTestAccessor().Avatar;

        public XhtmlTextBlock CommitMessage => _commitInfo.rtbxCommitMessage;

        public XhtmlTextBlock RevisionInfo => _commitInfo.RevisionInfo;

        public CommitInfoHeader Header => _commitInfo.commitInfoHeader;

        public IDictionary<string, int> GetSortedTags() => _commitInfo.GetSortedTags();

        public void LinkClicked(object sender, string linkUri) => _commitInfo.LinkClicked(sender, new LinkClickedEventArgs(linkUri));
    }
}
