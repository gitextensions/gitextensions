using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
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
using Point = Avalonia.Point;
using Size = Avalonia.Size;

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
    private int _revisionInfoHeight;
    private int _commitMessageHeight;
    private bool _showAllBranches;
    private bool _showAllTags;

    // ContextMenu.Opening precedes Avalonia's popup placement; retain the requesting
    // source and point locally instead of borrowing another editor's last clicked URI.
    private XhtmlTextBlock? _contextMenuSourceControl;
    private Point? _contextMenuSourcePosition;
    private TopLevel? _contextMenuRoot;
    private Point? _contextMenuPointerPosition;

    [DefaultValue(false)]
    public bool ShowBranchesAsLinks { get; set; }

    public CommitInfo()
        : this(commitDataManager: null)
    {
    }

    public CommitInfo(ICommitDataManager? commitDataManager)
    {
        InitializeComponent();

        // Avalonia's compatibility hotkey table is opt-in; WinForms enables control hotkeys through its runtime lifecycle.
        HotkeysEnabled = true;
        InitializeComplete();

        _commitDataManager = commitDataManager ?? new CommitDataManager(() => Module);

        _externalLinksStorage = new ExternalLinksStorage();
        _effectiveLinkDefinitionsProvider = new ConfiguredLinkDefinitionsProvider(_externalLinksStorage);
        _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
        _externalLinkRevisionParser = new ExternalLinkRevisionParser(_remotesManager);
        _gitRevisionExternalLinksParser = new GitRevisionExternalLinksParser(_effectiveLinkDefinitionsProvider, _externalLinkRevisionParser);
        _gitDescribeProvider = new GitDescribeProvider(() => Module);

        // This issue surfaces at 150% scale factor.
        // At this point rtbxCommitMessage.Bounds = {X = 8 Y = 8 Width = 440 Height = 0}
        // and with Height=0 we won't be receiving any ContentsResizedEvents.
        // To workaround the zero-height - force the min size.
        // Avalonia measures ContentsResized through TextLayout rather than HWND events.
        _ = AppSettings.CommitFont;
        _ = AppSettings.Font;

        // TextLayout replaces ContentsResized for these wrapped controls. Preserve the
        // source configured-font line metrics, not a fixed default-font LineHeight.
        rtbxCommitMessage.UseNativeContentsHeightMeasurement();
        RevisionInfo.UseNativeContentsHeightMeasurement();
        rtbxCommitMessage.ContentsResized += CommitMessage_ContentsResized;
        RevisionInfo.ContentsResized += RevisionInfo_ContentsResized;

        // WinForms AutoSize raises parent layout after the header's client size changes.
        // A Grid with an explicit total height cannot propagate that change through its
        // unchanged DesiredSize, so retain the same route from the actual child size.
        commitInfoHeader.SizeChanged += (_, _) => InvalidateMeasure();

        // The source first lays out its zero-contents row, then applies MinimumSize(1,1).
        // DefaultLayout caches the resulting anchored edges, independently of Margin.
        // These are authored Designer bounds and the constructor's minimum, not a text
        // metric correction or dimensions inferred from a screenshot.
        const int sourceTableWidth = 472;
        const int sourceCommitMessageWidth = 440;
        const int sourceCommitMessageMinimumSize = 1;
        rtbxCommitMessage.MinWidth = sourceCommitMessageMinimumSize;
        Thickness messageMargin = rtbxCommitMessage.Margin;
        double initialRowHeight = messageMargin.Top + messageMargin.Bottom;
        rtbxCommitMessage.SetNativeAnchorInsets(new Thickness(
            messageMargin.Left,
            messageMargin.Top,
            sourceTableWidth - messageMargin.Left - sourceCommitMessageWidth,
            initialRowHeight - messageMargin.Top - sourceCommitMessageMinimumSize));

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
        commitInfoContextMenuStrip.Closed += (_, _) => ClearContextMenuSource();
        AddHandler(ContextRequestedEvent, commitInfoContextMenuStrip_ContextRequested, RoutingStrategies.Tunnel);

        // Run the original Copy handlers before SelectableTextBlock's default action;
        // its empty-selection shortcut otherwise consumes the event without copying.
        rtbxCommitMessage.AddHandler(KeyDownEvent, RichTextBox_KeyDown, RoutingStrategies.Tunnel);
        RevisionInfo.AddHandler(KeyDownEvent, RichTextBox_KeyDown, RoutingStrategies.Tunnel);
        commitInfoHeader.SetContextMenuStrip(commitInfoContextMenuStrip);

        // Avalonia constraint: controls have no DisposeCustomResources lifecycle hook.
        DetachedFromVisualTree += (_, _) => _asyncLoadCancellation.CancelCurrent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _contextMenuRoot = TopLevel.GetTopLevel(this);
        _contextMenuRoot?.AddHandler(PointerMovedEvent, ContextMenuPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        _contextMenuRoot?.AddHandler(PointerExitedEvent, ContextMenuPointerExited, RoutingStrategies.Direct, handledEventsToo: true);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _contextMenuRoot?.RemoveHandler(PointerMovedEvent, ContextMenuPointerMoved);
        _contextMenuRoot?.RemoveHandler(PointerExitedEvent, ContextMenuPointerExited);
        _contextMenuRoot = null;
        _contextMenuPointerPosition = null;
        ClearContextMenuSource();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnRuntimeLoad()
    {
        base.OnRuntimeLoad();
        ReloadHotkeys();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        OnLayout(availableSize);
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        OnLayout(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
    {
        base.OnUICommandsSourceSet(source);

        OnRuntimeLoad();

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

        // Avalonia menus display their native gesture instead of WinForms' ShortcutKeyDisplayString.
        addNoteToolStripMenuItem.InputGesture = KeysMapper.ToKeyGesture(
            Hotkeys.FirstOrDefault(hotkey => hotkey.CommandCode == (int)FormBrowse.Command.AddNotes)?.KeyData);
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
            try
            {
                // Match the original lazy ancestor lookup when a hosted control first needs
                // repository services. Standalone designer controls have no such ancestor.
                _ = UICommandsSource;
            }
            catch (InvalidOperationException)
            {
                rtbxCommitMessage.SetXHTMLText(WebUtility.HtmlEncode(revision.Body ?? revision.Subject ?? string.Empty));
                RevisionInfo.Clear();
                return;
            }
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
        if (_contextMenuSourceControl is not { } rtb || _contextMenuSourcePosition is not { } point)
        {
            copyLinkToolStripMenuItem.IsVisible = false;
            copyLinkToolStripMenuItem.Tag = null;
            ClearContextMenuSource();
            return;
        }

        string? link = rtb.GetContextLinkAtPoint(point);
        copyLinkToolStripMenuItem.IsVisible = link is not null;
        copyLinkToolStripMenuItem.Header = string.Format(_copyLink.Text, link);
        copyLinkToolStripMenuItem.Tag = link;
        ClearContextMenuSource();
    }

    private void commitInfoContextMenuStrip_ContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        ClearContextMenuSource();
        if (e.Source is not Visual origin)
        {
            return;
        }

        XhtmlTextBlock? source = origin as XhtmlTextBlock
            ?? origin.GetVisualAncestors().OfType<XhtmlTextBlock>().FirstOrDefault();
        if (source is null)
        {
            return;
        }

        Point? position = e.TryGetPosition(source, out Point point) ? point
            : _contextMenuRoot is { } root && _contextMenuPointerPosition is { } pointer
                ? root.TranslatePoint(pointer, source)
                : null;
        _contextMenuSourceControl = source;
        _contextMenuSourcePosition = position;
    }

    private void ContextMenuPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_contextMenuRoot is { } root && e.Pointer.Type == PointerType.Mouse)
        {
            // Keyboard requests provide no pointer point. WinForms uses MousePosition,
            // not the caret; track actual input in the owning window without inventing
            // a global cursor location after the pointer leaves that window.
            _contextMenuPointerPosition = e.GetPosition(root);
        }
    }

    private void ContextMenuPointerExited(object? sender, PointerEventArgs e)
    {
        if (ReferenceEquals(e.Source, _contextMenuRoot))
        {
            _contextMenuPointerPosition = null;
        }
    }

    private void ClearContextMenuSource()
    {
        _contextMenuSourceControl = null;
        _contextMenuSourcePosition = null;
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

    private void RevisionInfo_ContentsResized(int contentsHeight)
    {
        _revisionInfoHeight = contentsHeight;
        InvalidateMeasure();
    }

    private void CommitMessage_ContentsResized(int contentsHeight)
    {
        _commitMessageHeight = contentsHeight;

        // The source's additional 150%-only workaround is not a native100 height rule.
        // Avalonia supplies its actual wrapped contents; higher-DPI equivalence is separate.
        InvalidateMeasure();
    }

    // Avalonia has no WinForms OnLayout event. Measure/arrange supply the real parent
    // constraint, while finite source columns prevent ScrollViewer's infinity from
    // disabling RichEdit-style word wrapping.
    private void OnLayout(Size availableSize)
    {
        if (!tableLayout.IsVisible)
        {
            return;
        }

        commitInfoHeader.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double clientWidth = double.IsFinite(availableSize.Width)
            ? availableSize.Width
            : Math.Max(Bounds.Width, commitInfoHeader.DesiredSize.Width);
        double width = Math.Max(clientWidth, commitInfoHeader.DesiredSize.Width);

        // The two candidate widths are one layout transaction. Publishing the full-
        // width height before the scrollbar-constrained height would invalidate the
        // parent on every unchanged pass when a word wraps only at the narrower width.
        rtbxCommitMessage.SuspendContentsResized();
        RevisionInfo.SuspendContentsResized();
        try
        {
            MeasureContents(width);
            if (GetHeights(measured: true).Sum() > availableSize.Height)
            {
                clientWidth = Math.Max(0, clientWidth - GetVerticalScrollBarWidth());
                width = Math.Max(clientWidth, commitInfoHeader.DesiredSize.Width);
                MeasureContents(width);
            }
        }
        finally
        {
            try
            {
                rtbxCommitMessage.ResumeContentsResized();
            }
            finally
            {
                RevisionInfo.ResumeContentsResized();
            }
        }

        double[] heights = GetHeights();
        double height = heights.Sum();

        // Leave the first row Auto so the header retains its original AutoSize behavior.
        for (int i = 1; i < tableLayout.RowDefinitions.Count; i++)
        {
            tableLayout.RowDefinitions[i].Height = new GridLength(heights[i]);
        }

        tableLayout.ColumnDefinitions[0].Width = new GridLength(width);
        tableLayout.Width = width;
        tableLayout.Height = height;
        return;

        double[] GetHeights(bool measured = false) =>
        [
            commitInfoHeader.DesiredSize.Height,
            (measured ? rtbxCommitMessage.ContentsHeight : _commitMessageHeight) + rtbxCommitMessage.Margin.Top + rtbxCommitMessage.Margin.Bottom
                + pnlCommitMessage.Margin.Top + pnlCommitMessage.Margin.Bottom,
            (measured ? RevisionInfo.ContentsHeight : _revisionInfoHeight) + RevisionInfo.Margin.Top + RevisionInfo.Margin.Bottom
        ];

        double GetVerticalScrollBarWidth()
        {
            if (Content is not ScrollViewer scroll)
            {
                return 0;
            }

            scroll.ApplyTemplate();
            ScrollBar? scrollbar = scroll.GetVisualDescendants().OfType<ScrollBar>()
                .FirstOrDefault(bar => bar.Orientation == Orientation.Vertical);
            return scrollbar is not null && double.IsFinite(scrollbar.Width)
                ? scrollbar.Width
                : scrollbar?.Bounds.Width ?? 0;
        }

        void MeasureContents(double columnWidth)
        {
            rtbxCommitMessage.Measure(new Size(columnWidth, double.PositiveInfinity));
            RevisionInfo.Measure(new Size(columnWidth, double.PositiveInfinity));
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

    // parity-scaffolding: Exposes the original named surfaces and menu state to focused parity tests.
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

        public MenuItem AddNoteMenuItem => _commitInfo.addNoteToolStripMenuItem;

        public ContextMenu ContextMenu => _commitInfo.commitInfoContextMenuStrip;

        public MenuItem CopyLinkMenuItem => _commitInfo.copyLinkToolStripMenuItem;

        public MenuItem ShowLocalBranchesMenuItem => _commitInfo.showContainedInBranchesToolStripMenuItem;

        public MenuItem ShowRemoteBranchesMenuItem => _commitInfo.showContainedInBranchesRemoteToolStripMenuItem;

        public MenuItem ShowRemoteBranchesIfNoLocalMenuItem => _commitInfo.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem;

        public MenuItem ShowTagsMenuItem => _commitInfo.showContainedInTagsToolStripMenuItem;

        public MenuItem ShowAnnotatedTagMessagesMenuItem => _commitInfo.showMessagesOfAnnotatedTagsToolStripMenuItem;

        public MenuItem ShowDerivedTagMenuItem => _commitInfo.showTagThisCommitDerivesFromMenuItem;

        public Grid TableLayout => _commitInfo.tableLayout;

        public Border CommitMessagePanel => _commitInfo.pnlCommitMessage;

        public int CommitMessageHeight => _commitInfo._commitMessageHeight;

        public int RevisionInfoHeight => _commitInfo._revisionInfoHeight;

        public IDictionary<string, int> GetSortedTags() => _commitInfo.GetSortedTags();

        public void LinkClicked(object sender, string linkUri) => _commitInfo.LinkClicked(sender, new LinkClickedEventArgs(linkUri));
    }
}
