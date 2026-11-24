using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Properties;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class MessageColumnProvider : ColumnProvider
{
    private record struct Settings
        (bool FillRefLabels,
        bool NotesInSeparateColumn,
        bool ShowAnnotatedTagsMessages,
        bool ShowCommitBodyInRevisionGrid,
        bool ShowGitNotes,
        bool ShowGitStatusForArtificialCommits,
        bool ShowRemoteBranches,
        bool ShowRevisionGridTooltips,
        bool ShowTags);

    public const int MaxSuperprojectRefs = 4;

    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    private readonly StringBuilder _toolTipBuilder = new(200);
    private readonly IImage _bisectGoodImage = Images.BisectGood;
    private readonly IImage _bisectBadImage = Images.BisectBad;
    private readonly IImage _fixupAndSquashImage = Images.FixupAndSquashMessageMarker;
    private readonly ICommitDataManager? _commitDataManager;
    private readonly RevisionGridControl _grid;
    private readonly IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;

    /// <summary>
    ///  Caches painted ref label hit regions per row index for mouse hit-testing.
    /// </summary>
    private readonly Dictionary<int, WeakReference<MessageCell>> _refLabelHitInfoByRow = [];

    // Pool of reusable lists to reduce allocations during scrolling.
    private readonly Stack<List<RevisionGridRefRenderer.RefLabelControl>> _hitInfoListPool = new();

    // Caches the configured push prefix per remote name to avoid repeated git-config reads during painting.
    private readonly Dictionary<string, string> _remotePrefixCache = [];

    // Caches whether the label color of a remote is unique among all remotes.
    private readonly Dictionary<string, bool> _remoteColorIsUniqueCache = [];

    // Caches the names of the configured remotes to avoid repeated git-config reads during painting.
    private IReadOnlyList<string>? _remoteNames;
    private IReadOnlyDictionary<string, AheadBehindData>? _aheadBehindDataByLocalBranch;
    private IReadOnlyDictionary<string, AheadBehindData>? _aheadBehindDataByRemoteBranch;
    private IAheadBehindDataProvider? _aheadBehindDataProvider;
    private MessageCell? _highlightedCell;
    private RevisionGridRefRenderer.RefLabelControl? _highlightedLabel;

    // The ref currently under the mouse cursor, used to draw a highlight border.
    private IGitRef? _highlightedRef;

    // The row index of the currently highlighted ref label.
    private int _highlightedRowIndex = -1;

    // The row index of the currently highlighted stash label (which has no IGitRef).
    private int _highlightedStashRow = -1;
    private Settings _settings;

    public MessageColumnProvider(
        RevisionGridControl grid,
        IGitRevisionSummaryBuilder gitRevisionSummaryBuilder,
        ICommitDataManager? commitDataManager)
        : base("Message", new GridLength(1, GridUnitType.Star), minimumWidth: 25, resizable: true)
    {
        _commitDataManager = commitDataManager;
        _grid = grid;
        _gitRevisionSummaryBuilder = gitRevisionSummaryBuilder;
    }

    public void SetAheadBehindDataProvider(IAheadBehindDataProvider? provider)
    {
        _aheadBehindDataProvider = provider;
        Clear();
    }

    public override void ApplySettings()
    {
        Column.IsVisible = true;
        _settings = new Settings(
            FillRefLabels: AppSettings.FillRefLabels,
            NotesInSeparateColumn: AppSettings.ShowGitNotesColumn,
            ShowAnnotatedTagsMessages: AppSettings.ShowAnnotatedTagsMessages,
            ShowCommitBodyInRevisionGrid: AppSettings.ShowCommitBodyInRevisionGrid,
            ShowGitNotes: AppSettings.ShowGitNotes,
            ShowGitStatusForArtificialCommits: AppSettings.ShowGitStatusForArtificialCommits,
            ShowRemoteBranches: AppSettings.ShowRemoteBranches,
            ShowRevisionGridTooltips: AppSettings.ShowRevisionGridTooltips,
            ShowTags: AppSettings.ShowTags);
    }

    public override Control CreateCell()
    {
        MessageCell panel = new(this)
        {
            ClipToBounds = true,
        };
        panel.Classes.Add("revision-message-cell");
        return panel;
    }

    public override void OnCellPainting(Control control, GitRevision revision)
    {
        MessageCell panel = (MessageCell)control;
        int rowIndex = _grid.GetRevisionIndex(revision);

        bool restoreHighlight = DetachHighlightForUpdate(panel, revision);
        panel.ContentPanel.ClearMinimumAdvances();
        panel.ContentPanel.Children.RemoveRange(0, panel.ContentPanel.Children.Count - 1);
        panel.FixupAndSquashMarker.IsVisible = false;
        panel.Body.Text = string.Empty;

        if (revision.IsArtificial)
        {
            panel.Subject.Classes.Remove("revision-emphasized");
            panel.Body.Classes.Remove("revision-emphasized");
            DrawArtificialRevision(panel, revision);

            panel.Subject.Text = string.Empty;
            panel.Revision = revision;
            panel.Indicator.Update(revision);
            ToolTip.SetTip(panel, _settings.ShowGitStatusForArtificialCommits
                && _grid.GetChangeCount(revision.ObjectId) is ArtificialCommitChangeCount changeCount
                    ? changeCount.GetSummary()
                    : _settings.ShowRevisionGridTooltips ? revision.Subject : null);
            RestoreHighlightAfterUpdate(panel, restoreHighlight);
            if (rowIndex >= 0)
            {
                _refLabelHitInfoByRow[rowIndex] = new WeakReference<MessageCell>(panel);
            }

            return;
        }

        List<IGitRef> superprojectRefs = [];
        if (_grid.TryGetSuperProjectInfo(out SuperProjectInfo? superProjectInfo))
        {
            // Draw super project references (for submodules)
            foreach (Control label in DrawSuperprojectInfo(revision, superProjectInfo))
            {
                DrawRef(panel, label);
            }

            if (superProjectInfo.Refs is not null && !revision.ObjectId.IsZero
                && superProjectInfo.Refs.TryGetValue(revision.ObjectId, out IReadOnlyList<IGitRef>? refs))
            {
                superprojectRefs.AddRange(refs);
            }
        }

        IReadOnlyList<IGitRef> gitRefs = SortRefs(revision.Refs.Where(FilterRef));
        Dictionary<string, IGitRef> trackedRemotes = BuildTrackedRemoteMap(gitRefs);

        // When there is only one local branch on this commit, remote-ref labels can omit the branch name if equal.
        int localBranchCount = gitRefs.Count(gitRef => gitRef.IsHead);
        string? singleLocalBranchName = localBranchCount == 1 ? trackedRemotes.Keys.FirstOrDefault() : null;

        foreach (IGitRef gitRef in gitRefs)
        {
            // Remote refs that are tracked by a local branch in this row
            // are drawn condensed immediately after that local branch instead.
            if (trackedRemotes.ContainsValue(gitRef))
            {
                continue;
            }

            IGitRef? superprojectRef = superprojectRefs.FirstOrDefault(superGitRef => gitRef.CompleteName == superGitRef.CompleteName);
            if (superprojectRef is not null)
            {
                superprojectRefs.Remove(superprojectRef);
            }

            // If this branch is at its tracked remote, draw them condensed.
            if (gitRef.IsHead && trackedRemotes.TryGetValue(gitRef.Name, out IGitRef? remote))
            {
                DrawBranchWithNestledRemote(gitRef, superprojectRef, remote, remote.Remote);
                continue;
            }

            // If this branch has ahead/behind information, draw that info as virtual label of the tracked/tracking branch.
            (string aheadBehind, string trackedCompleteName, bool isGone) = GetAheadBehind(gitRef, withCounts: false);
            if (aheadBehind.Length > 0)
            {
                NestledVirtualRef nestledRef = new(gitRef, trackedCompleteName, trackingBranchIsGone: isGone);
                DrawBranchWithNestledRemote(gitRef, superprojectRef, nestledRef, aheadBehind);
                continue;
            }

            DrawSeparateRef(gitRef, superprojectRef);
        }

        foreach (Control label in DrawSuperprojectRefs(superprojectRefs))
        {
            DrawRef(panel, label);
        }

        if (revision.IsStash || revision.IsAutostash)
        {
            string stashLabel = revision.IsAutostash
                ? revision.Subject
                : (revision.ReflogSelector
                    ?? throw new InvalidOperationException($"{nameof(revision.ReflogSelector)} must not be null"))[5..];
            DrawRef(panel, RevisionGridRefRenderer.CreateSpecialLabel(stashLabel, RefLabelIcon.Stash, dashed: false));
        }

        string[] lines = revision.IsAutostash ? [] : GetCommitMessageLines(revision);
        DrawCommitMessage(panel, revision, lines);
        panel.Revision = revision;
        panel.Indicator.Update(revision);
        RestoreHighlightAfterUpdate(panel, restoreHighlight);

        // Register hit-boxes.
        if (rowIndex >= 0)
        {
            _refLabelHitInfoByRow[rowIndex] = new WeakReference<MessageCell>(panel);
        }

        return;

        // Builds a map of local branch name → remote ref that tracks it. No I/O is performed.
        static Dictionary<string, IGitRef> BuildTrackedRemoteMap(IReadOnlyList<IGitRef> refs)
        {
            IReadOnlyList<IGitRef> localBranches = [.. refs.Where(r => r.IsHead)];
            if (localBranches.Count == 0)
            {
                return [];
            }

            Dictionary<string, IGitRef> remoteByLocal = [];
            foreach (IGitRef remote in refs)
            {
                if (!remote.IsRemote)
                {
                    continue;
                }

                foreach (IGitRef local in localBranches)
                {
                    if (!local.IsTrackingRemote(remote))
                    {
                        continue;
                    }

                    if (!remoteByLocal.TryAdd(local.LocalName, remote))
                    {
                        throw new InvalidOperationException($"Multiple remote refs {remote.Name} and {remoteByLocal[local.LocalName].Name} claim they were tracked by local branch '{local.LocalName}'.");
                    }
                }
            }

            return remoteByLocal;
        }

        // Draws a local branch capsule with its tracked remote capsule nestled against it, appearing as a single visual group.
        void DrawBranchWithNestledRemote(IGitRef gitRef, IGitRef? superprojectRef, IGitRef nestledRef, string nestledName)
        {
            // Draw the gitRef with a '>' / '<' right edge that meets the nestledRef's matching left indent.
            (RefLabelShape shape1, RefLabelShape shape2) = gitRef.IsRemote
                ? (RefLabelShape.NotchRight, RefLabelShape.PointLeft)
                : (RefLabelShape.PointRight, RefLabelShape.NotchLeft);
            RevisionGridRefRenderer.RefLabelControl label = RevisionGridRefRenderer.CreateLabel(
                gitRef, gitRef.Name, shape1, _settings.FillRefLabels, dashed: superprojectRef is not null);

            // Draw the nestled directly via CreateLabel with showHeadIndicator: false — the nestled remote never shows a head indicator.
            NestledVirtualRef? virtualRef = nestledRef as NestledVirtualRef;
            RevisionGridRefRenderer.RefLabelControl nestledLabel = RevisionGridRefRenderer.CreateLabel(
                nestledRef, nestledName, shape2, _settings.FillRefLabels, showHeadIndicator: false,
                dashed: virtualRef is not null,
                fontWeight: virtualRef is null ? FontWeight.Normal : virtualRef.TrackingBranchIsGone ? FontWeight.Bold : FontWeight.Normal);

            // Register hit-boxes.
            DrawRef(panel, new RevisionGridRefRenderer.NestledRefLabelPanel(label, nestledLabel));
            TryReplaceRemoteNameWithIcon(label);
            TryReplaceRemoteNameWithIcon(nestledLabel);
        }

        void DrawSeparateRef(IGitRef gitRef, IGitRef? superprojectRef)
        {
            if (gitRef.IsBisectGood)
            {
                DrawRef(panel, DrawImage(_bisectGoodImage));
                return;
            }

            if (gitRef.IsBisectBad)
            {
                DrawRef(panel, DrawImage(_bisectBadImage));
                return;
            }

            bool showFullNameOnHighlight = singleLocalBranchName is not null
                && gitRef.IsRemote
                && gitRef.LocalName == GetRemotePrefix(gitRef.Module, gitRef.Remote) + singleLocalBranchName;
            string name = showFullNameOnHighlight ? gitRef.Remote : gitRef.Name;
            RefLabelShape shape = gitRef.IsTag ? RefLabelShape.PointLeft : RefLabelShape.Rect;
            RevisionGridRefRenderer.RefLabelControl label = RevisionGridRefRenderer.CreateLabel(
                gitRef, name, shape, _settings.FillRefLabels, dashed: superprojectRef is not null,
                highlightedLabel: showFullNameOnHighlight ? gitRef.Name : null);
            if (_settings.ShowAnnotatedTagsMessages && gitRef.IsTag && gitRef.IsDereference)
            {
                label.AppendLabel(" [...]");
            }

            DrawRef(panel, label);
            TryReplaceRemoteNameWithIcon(label);
        }

        bool DetachHighlightForUpdate(MessageCell messageCell, GitRevision updatedRevision)
        {
            if (!ReferenceEquals(_highlightedCell, messageCell))
            {
                return false;
            }

            if (_highlightedRowIndex != _grid.GetRevisionIndex(updatedRevision))
            {
                SetHighlight(cell: null, label: null);
                return false;
            }

            if (_highlightedLabel is not null)
            {
                _highlightedLabel.IsHighlighted = false;
                _highlightedLabel = null;
            }

            messageCell.Cursor = null;
            return true;
        }

        void RestoreHighlightAfterUpdate(MessageCell messageCell, bool restore)
        {
            if (!restore)
            {
                return;
            }

            RevisionGridRefRenderer.RefLabelControl? label = messageCell.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .FirstOrDefault(candidate => _highlightedRef is not null
                        ? Equals(candidate.GitRef, _highlightedRef)
                        : candidate.GitRef is null
                            && candidate.Icon == RefLabelIcon.Stash
                            && _highlightedStashRow == _grid.GetRevisionIndex(revision));
            if (label is null)
            {
                SetHighlight(cell: null, label: null);
                return;
            }

            _highlightedLabel = label;
            _highlightedLabel.IsHighlighted = true;
            messageCell.Cursor = HandCursor;
        }
    }

    public override void OnCellFormatting(Control control, GitRevision revision)
    {
        // Set the grid cell's accessibility text
        AutomationProperties.SetName(control, revision.Subject.Trim());
    }

    public override bool TryGetToolTip(double? x, GitRevision revision, IGitRef? highlightRef, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        _toolTipBuilder.Clear();

        if (highlightRef is not null)
        {
            if (highlightRef is NestledVirtualRef aheadBehindRef)
            {
                bool realRefIsRemote = !aheadBehindRef.IsRemote;
                string realRefLocalName = aheadBehindRef.MergeWith;
                string realRefCompleteName = realRefIsRemote ? GitRefName.GetFullRemoteName(realRefLocalName, aheadBehindRef.TrackingRemote) : GitRefName.GetFullBranchName(realRefLocalName);
                string realRefName = realRefIsRemote ? realRefCompleteName[GitRefName.RefsRemotesPrefix.Length..] : realRefLocalName;
                AheadBehindData? data = GetAheadBehindData(realRefIsRemote, realRefCompleteName);
                _toolTipBuilder.Append('[').Append(realRefName).Append(']');
                if (realRefIsRemote)
                {
                    _toolTipBuilder.AppendLine().AppendFormat(TranslatedStrings.IsTrackedBy_Branch_AheadBehind, data?.Branch, data?.ToDisplay());
                }
                else
                {
                    string? remoteBranch = data?.RemoteRef[GitRefName.RefsRemotesPrefix.Length..];
                    if (data?.AheadCount == AheadBehindData.Gone)
                    {
                        _toolTipBuilder.AppendLine()
                                       .AppendFormat(TranslatedStrings.WasTracking_Remote, remoteBranch);
                    }
                    else
                    {
                        _toolTipBuilder.Append("   ").AppendLine(data?.ToDisplay())
                                       .AppendFormat(TranslatedStrings.IsTracking_Remote, remoteBranch);
                    }
                }
            }
            else
            {
                _toolTipBuilder.Append('[').Append(highlightRef.Name).Append(']');
                if (highlightRef.IsRemote)
                {
                    if (GetAheadBehindData(isRemote: true, highlightRef.CompleteName) is { } data)
                    {
                        _toolTipBuilder.AppendLine().AppendFormat(TranslatedStrings.IsTrackedBy_Branch_AheadBehind, data.Branch, data.ToDisplay());
                    }
                    else if (_settings.ShowRevisionGridTooltips)
                    {
                        _toolTipBuilder.AppendLine().Append(TranslatedStrings.IsRemoteBranch);
                    }
                    else
                    {
                        toolTip = null;
                        return false;
                    }
                }
                else if (highlightRef.IsHead)
                {
                    if (GetAheadBehindData(isRemote: false, highlightRef.CompleteName) is { } data)
                    {
                        string remoteBranch = data.RemoteRef[GitRefName.RefsRemotesPrefix.Length..];
                        if (data.AheadCount == AheadBehindData.Gone)
                        {
                            _toolTipBuilder.AppendLine()
                                           .AppendFormat(TranslatedStrings.WasTracking_Remote, remoteBranch);
                        }
                        else
                        {
                            _toolTipBuilder.Append("   ").AppendLine(data.ToDisplay())
                                           .AppendFormat(TranslatedStrings.IsTracking_Remote, remoteBranch);
                        }
                    }
                    else if (_settings.ShowRevisionGridTooltips)
                    {
                        _toolTipBuilder.AppendLine().Append(TranslatedStrings.IsLocalBranch);
                    }
                    else
                    {
                        toolTip = null;
                        return false;
                    }
                }
                else if (highlightRef.IsTag)
                {
                    if (_settings.ShowRevisionGridTooltips)
                    {
                        _toolTipBuilder.AppendLine().Append(TranslatedStrings.IsTag);
                    }
                    else
                    {
                        toolTip = null;
                        return false;
                    }
                }
            }

            if (!_settings.ShowRevisionGridTooltips)
            {
                toolTip = _toolTipBuilder.ToString();
                return true;
            }

            _toolTipBuilder.AppendLine().AppendLine();
        }

        if (!revision.IsArtificial && (revision.HasMultiLineMessage || revision.Refs.Count != 0))
        {
            // The body is not stored for older commits (to save memory)
            string bodySummary = _gitRevisionSummaryBuilder.BuildSummary(GetBody(revision))
                ?? revision.Subject + (revision.HasMultiLineMessage ? TranslatedStrings.BodyNotLoaded : "");
            int initialLength = bodySummary.Length + 10;
            _toolTipBuilder.EnsureCapacity(initialLength);

            _toolTipBuilder.Append(bodySummary);

            if (revision.Refs.Count != 0)
            {
                if (_toolTipBuilder.Length != 0)
                {
                    _toolTipBuilder.AppendLine();
                    _toolTipBuilder.AppendLine();
                }

                foreach (IGitRef gitRef in SortRefs(revision.Refs))
                {
                    if (gitRef.IsBisectGood)
                    {
                        _toolTipBuilder.AppendLine(TranslatedStrings.MarkBisectAsGood);
                    }
                    else if (gitRef.IsBisectBad)
                    {
                        _toolTipBuilder.AppendLine(TranslatedStrings.MarkBisectAsBad);
                    }
                    else
                    {
                        _toolTipBuilder.Append('[').Append(gitRef.Name).Append(']');
                        if (GetAheadBehindData(gitRef.IsRemote, gitRef.CompleteName) is { } data)
                        {
                            _toolTipBuilder.Append("   ").Append(data.ToDisplay(reverse: gitRef.IsRemote));
                        }

                        _toolTipBuilder.AppendLine();
                    }
                }
            }

            toolTip = _toolTipBuilder.ToString();
            return true;
        }

        if (_settings.ShowGitStatusForArtificialCommits && _grid.GetChangeCount(revision.ObjectId) is ArtificialCommitChangeCount changeCount)
        {
            toolTip = _toolTipBuilder.Append(changeCount.GetSummary()).ToString();
            return true;
        }

        return base.TryGetToolTip(x, revision, highlightRef, out toolTip);
    }

    private void DrawArtificialRevision(MessageCell panel, GitRevision revision)
    {
        // Add fake "refs" for artificial commits
        RevisionGridRefRenderer.RefLabelControl artificialLabel = RevisionGridRefRenderer.CreateSpecialLabel(
            revision.Subject,
            revision.ObjectId == ObjectId.IndexId ? RefLabelIcon.CommitIndex : RefLabelIcon.WorkingDirectory,
            dashed: false);
        double fontSize = panel.Subject.FontSize > 0 ? panel.Subject.FontSize : 12;
        Typeface typeface = new(panel.Subject.FontFamily, panel.Subject.FontStyle, panel.Subject.FontWeight);
        double max = Math.Max(Measure(ResourceManager.TranslatedStrings.Workspace), Measure(ResourceManager.TranslatedStrings.Index));

        // Avalonia retains the capsule separately from the status-count allocation.
        panel.ContentPanel.SetMinimumAdvance(artificialLabel, Math.Ceiling(max) + 12);
        DrawRef(panel, artificialLabel);

        // Summary of changes
        if (!_settings.ShowGitStatusForArtificialCommits || _grid.GetChangeCount(revision.ObjectId) is not ArtificialCommitChangeCount changeCount)
        {
            return;
        }

        if (changeCount.DataValid)
        {
            if (changeCount.HasChanges)
            {
                DrawArtificialCount(changeCount.Changed, Images.FileStatusModified);
                DrawArtificialCount(changeCount.New, Images.FileStatusAdded);
                DrawArtificialCount(changeCount.Deleted, Images.FileStatusRemoved);
                DrawArtificialCount(changeCount.SubmodulesChanged, Images.SubmoduleRevisionDown);
                DrawArtificialCount(changeCount.SubmodulesDirty, Images.SubmoduleDirty);
            }
            else
            {
                DrawArtificialCount(items: null, Images.RepoStateClean);
            }
        }
        else
        {
            DrawArtificialCount(items: null, Images.RepoStateUnknown);
        }

        return;

        void DrawArtificialCount(IReadOnlyList<GitItemStatus>? items, IImage icon)
        {
            if (items?.Count is 0)
            {
                return;
            }

            // Avalonia lays out the image and count as retained controls instead of a Graphics draw.
            StackPanel countPanel = new()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 3,
                Margin = new Thickness(1, 0, 5, 0),
            };
            countPanel.Children.Add(new Image
            {
                Source = icon,
                Width = 12,
                Height = 12,
                VerticalAlignment = VerticalAlignment.Center,
            });
            if (items is not null)
            {
                countPanel.Children.Add(new TextBlock
                {
                    Text = items.Count.ToString(),
                    MinWidth = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            panel.ContentPanel.Children.Insert(panel.ContentPanel.Children.Count - 1, countPanel);
        }

        double Measure(string text)
            => new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                typeface, fontSize, Brushes.Black).Width;
    }

    private static IReadOnlyList<Control> DrawSuperprojectRefs(IReadOnlyList<IGitRef> superprojectRefs)
    {
        List<Control> labels = [];
        for (int i = 0; i < Math.Min(MaxSuperprojectRefs, superprojectRefs.Count); i++)
        {
            IGitRef gitRef = superprojectRefs[i];
            string gitRefName = i < MaxSuperprojectRefs - 1 ? gitRef.Name : "…";

            // Additional superproject refs have a source palette and no actionable ref identity.
            labels.Add(RevisionGridRefRenderer.CreateLabel(
                gitRef, gitRefName, RefLabelShape.Rect, fill: false, dashed: true, isSuperprojectOnly: true));
        }

        return labels;
    }

    private static IReadOnlyList<Control> DrawSuperprojectInfo(
        GitRevision revision,
        SuperProjectInfo? superProjectInfo)
    {
        if (superProjectInfo is null)
        {
            return [];
        }

        List<Control> labels = [];
        if (superProjectInfo.CurrentCommit == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel(string.Empty, RefLabelIcon.Head));
        }

        if (superProjectInfo.ConflictBase == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Base", RefLabelIcon.HeadMergeSource));
        }

        if (superProjectInfo.ConflictLocal == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Local", RefLabelIcon.HeadMergeSource));
        }

        if (superProjectInfo.ConflictRemote == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Remote", RefLabelIcon.HeadMergeSource));
        }

        return labels;
    }

    private static void DrawRef(MessageCell panel, Control label)
    {
        // see note on using IsDereference in CommitInfo class
        panel.ContentPanel.Children.Insert(panel.ContentPanel.Children.Count - 1, label);
    }

    private static Image DrawImage(IImage image)
    {
        Image marker = new()
        {
            Source = image,
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        marker.Classes.Add("revision-bisect-marker");
        return marker;
    }

    private void DrawCommitMessage(MessageCell panel, GitRevision revision, string[] lines)
    {
        string commitTitle = lines.FirstOrDefault() ?? string.Empty;

        // Draw markers for fixup! and squash! commits
        panel.FixupAndSquashMarker.IsVisible = !revision.IsAutostash
            && (commitTitle.StartsWith(CommitKind.Fixup.GetPrefix(), StringComparison.Ordinal)
                || commitTitle.StartsWith(CommitKind.Squash.GetPrefix(), StringComparison.Ordinal)
                || commitTitle.StartsWith(CommitKind.Amend.GetPrefix(), StringComparison.Ordinal));
        panel.Subject.Text = revision.IsAutostash ? string.Empty : commitTitle;
        panel.MessagePanel.HasCommitMessage = lines.Length > 0;
        panel.Body.Text = !revision.IsAutostash && lines.Length > 1 && _settings.ShowCommitBodyInRevisionGrid
            ? string.Concat(lines.Skip(1).Select(line => " " + line))
            : string.Empty;

        // Draw the multi-line indicator
        bool emphasized = _grid.IsCurrentCheckout(revision);

        // Source BoldFont replaces configured Italic; NormalFont keeps its full style.
        panel.Subject.Classes.Set("revision-emphasized", emphasized);
        panel.Body.Classes.Set("revision-emphasized", emphasized);
    }

    private bool FilterRef(IGitRef gitRef)
    {
        if (gitRef.IsTag)
        {
            return _settings.ShowTags;
        }

        if (gitRef.IsRemote)
        {
            return _settings.ShowRemoteBranches;
        }

        return true;
    }

    private static IReadOnlyList<IGitRef> SortRefs(IEnumerable<IGitRef> refs)
    {
        List<IGitRef> sortedRefs = [.. refs];
        sortedRefs.Sort(CompareRefs);
        return sortedRefs;

        static int CompareRefs(IGitRef left, IGitRef right)
        {
            int result = GetRank(left).CompareTo(GetRank(right));
            return result == 0
                ? string.Compare(left.Name, right.Name, StringComparison.Ordinal)
                : result;
        }

        static int GetRank(IGitRef gitRef)
        {
            if (gitRef.IsBisect)
            {
                return 0;
            }

            if (gitRef.IsSelected)
            {
                return 1;
            }

            if (gitRef.IsSelectedHeadMergeSource)
            {
                return 2;
            }

            if (gitRef.IsHead)
            {
                return 3;
            }

            if (gitRef.IsRemote)
            {
                return 4;
            }

            return 5;
        }
    }

    private string[] GetCommitMessageLines(GitRevision revision)
        => GetBody(revision)?.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries) ?? [revision.Subject];

    private string? GetBody(GitRevision revision)
    {
        if (revision.Body is null)
        {
            if (_commitDataManager is not null
                && (_settings.ShowCommitBodyInRevisionGrid || _settings.ShowGitNotes || _settings.NotesInSeparateColumn))
            {
                _commitDataManager.InitiateDelayedLoadingOfDetails(revision);
            }

            return null;
        }

        return _settings.NotesInSeparateColumn
            ? revision.Body
            : UIExtensions.FormatBodyAndNotes(revision.Body, revision.Notes);
    }

    private string GetRemotePrefix(IGitModule module, string remoteName)
    {
        if (!_remotePrefixCache.TryGetValue(remoteName, out string? prefix))
        {
            prefix = module.GetEffectiveSetting(string.Format(SettingKeyString.RemotePrefix, remoteName));
            _remotePrefixCache[remoteName] = prefix;
        }

        return prefix;
    }

    private static System.Drawing.Color GetRefColor(IGitRef gitRef, IReadOnlyDictionary<string, System.Drawing.Color> remoteColors)
        => remoteColors.TryGetValue(gitRef.Remote, out System.Drawing.Color color) ? color : RevisionGridRefRenderer.GetHeadColor(gitRef);

    private IReadOnlyList<string> GetRemoteNames(IGitModule module)
    {
        return _remoteNames ??= module.GetAllLocalSettings()
            .Where(s => s.Setting.StartsWith(SettingKeyString.RemoteKeyPrefix, StringComparison.Ordinal) && s.Setting.EndsWith(SettingKeyString.RemoteUrlSuffix, StringComparison.Ordinal))
            .Select(s => s.Setting[SettingKeyString.RemoteKeyPrefix.Length..^SettingKeyString.RemoteUrlSuffix.Length])
            .Distinct()
            .ToList();
    }

    // The remote name can be replaced with an icon without ambiguity only if the label color identifies the remote.
    private bool IsRemoteColorUnique(IGitRef remoteRef)
    {
        if (!_remoteColorIsUniqueCache.TryGetValue(remoteRef.Remote, out bool isUnique))
        {
            System.Drawing.Color defaultColor = RevisionGridRefRenderer.GetHeadColor(remoteRef);
            IReadOnlyDictionary<string, System.Drawing.Color> colors = remoteRef.Module.GetRemoteColors();
            System.Drawing.Color remoteColor = GetRefColor(remoteRef, colors);
            isUnique = GetRemoteNames(remoteRef.Module)
                .Count(remote => (colors.TryGetValue(remote, out System.Drawing.Color color) ? color : defaultColor) == remoteColor) == 1;
            _remoteColorIsUniqueCache[remoteRef.Remote] = isUnique;
        }

        return isUnique;
    }

    // The icon is used only if it does not take more space than the remote name, so that the label does not shrink when highlighted.
    private bool TryReplaceRemoteNameWithIcon(RevisionGridRefRenderer.RefLabelControl label)
    {
        if (label.GitRef is not { IsRemote: true } remoteRef || remoteRef is NestledVirtualRef || label.Icon != RefLabelIcon.None)
        {
            return false;
        }

        string name = label.Label;
        string remote = remoteRef.Remote;
        if (!name.StartsWith(remote, StringComparison.Ordinal)
            || (name.Length > remote.Length && name[remote.Length] != '/')
            || !IsRemoteColorUnique(remoteRef))
        {
            return false;
        }

        string shortName = name.Length == remote.Length ? "" : name[(remote.Length + 1)..];
        if (RevisionGridRefRenderer.GetContentWidth(label, shortName, RefLabelIcon.RemoteForced)
            > RevisionGridRefRenderer.GetContentWidth(label, name, RefLabelIcon.None))
        {
            return false;
        }

        label.ReplaceRemoteNameWithIcon(shortName);
        return true;
    }

    /// <summary>
    ///  Performs a hit test to find which ref label (if any) contains the given point in the specified row.
    /// </summary>
    /// <returns>The matching <see cref="RevisionGridRefRenderer.RefLabelControl"/>, or <see langword="null"/> if no ref label was hit.</returns>
    public RevisionGridRefRenderer.RefLabelControl? HitTest(int rowIndex, Avalonia.Point gridClientPoint)
    {
        if (!_refLabelHitInfoByRow.TryGetValue(rowIndex, out WeakReference<MessageCell>? reference)
            || !reference.TryGetTarget(out MessageCell? cell))
        {
            _refLabelHitInfoByRow.Remove(rowIndex);
            return null;
        }

        List<RevisionGridRefRenderer.RefLabelControl> hitInfos = RentHitInfoList();
        try
        {
            hitInfos.AddRange(cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>());
            return hitInfos.FirstOrDefault(label =>
                (label.GitRef is not null || label.Icon == RefLabelIcon.Stash)
                && _grid.TranslatePoint(gridClientPoint, label) is Avalonia.Point point
                && label.Contains(point));
        }
        finally
        {
            ReturnHitInfoList(hitInfos);
        }
    }

    /// <summary>
    ///  Sets the ref or stash label to be drawn with a highlight border, triggering a repaint if the highlight changed.
    /// </summary>
    /// <returns><see langword="true"/> if the highlight state changed and a repaint is needed.</returns>
    public bool SetHighlight(Control? cell, RevisionGridRefRenderer.RefLabelControl? label)
    {
        MessageCell? messageCell = cell as MessageCell;
        if (ReferenceEquals(_highlightedCell, messageCell) && ReferenceEquals(_highlightedLabel, label))
        {
            return false;
        }

        if (_highlightedLabel is not null)
        {
            _highlightedLabel.IsHighlighted = false;
        }

        if (_highlightedCell is not null)
        {
            _highlightedCell.Cursor = null;
        }

        _highlightedCell = messageCell;
        _highlightedLabel = label;
        _highlightedRef = label?.GitRef;
        _highlightedRowIndex = label is null || messageCell?.Revision is null
            ? -1
            : _grid.GetRevisionIndex(messageCell.Revision);
        _highlightedStashRow = label is { GitRef: null, Icon: RefLabelIcon.Stash }
            ? _highlightedRowIndex
            : -1;

        if (_highlightedLabel is not null)
        {
            _highlightedLabel.IsHighlighted = true;
        }

        if (_highlightedCell is not null)
        {
            _highlightedCell.Cursor = label is null ? null : HandCursor;
        }

        _grid.UpdateLaneHighlightForRevision(label?.GitRef, messageCell?.Revision);
        return true;
    }

    public override void Clear()
    {
        SetHighlight(cell: null, label: null);
        _aheadBehindDataByLocalBranch = null;
        _aheadBehindDataByRemoteBranch = null;
        _remotePrefixCache.Clear();
        _remoteNames = null;
        _remoteColorIsUniqueCache.Clear();
        _refLabelHitInfoByRow.Clear();
    }

    /// <summary>
    ///  Returns a tuple of the ahead/behind indicator for a local or remote branch ref label
    ///  and the <see cref="IGitRef.CompleteName"/> of the tracked (for a local ref) or tracking (for a remote ref) branch.
    /// </summary>
    /// <remarks>
    ///  Uses <see cref="AheadBehindData.ToDisplay"/> for consistent formatting with the push button and left panel.
    ///  When rendering a local branch's tracked remote as a virtual label, the perspective is inverted: what the local branch
    ///  is ahead of the remote appears as the remote being behind, and vice versa — so <see cref="AheadBehindData.BehindCount"/>
    ///  and <see cref="AheadBehindData.AheadCount"/> are swapped before formatting.
    ///  Returns an empty display string for untracked refs or when the provider is unavailable.
    /// </remarks>
    private (string Display, string TrackedCompleteName, bool IsGone) GetAheadBehind(IGitRef gitRef, bool withCounts = true)
    {
        _aheadBehindDataByLocalBranch ??= _aheadBehindDataProvider?.GetData()
            ?? FrozenDictionary<string, AheadBehindData>.Empty;

        if (gitRef.IsRemote)
        {
            // Match the remote ref via AheadBehindData.RemoteRef, which holds the full refs/remotes/… name
            // regardless of whether the remote branch is named differently from the local tracking branch.
            _aheadBehindDataByRemoteBranch ??= _aheadBehindDataByLocalBranch.Values
                .DistinctBy(data => data.RemoteRef)
                .ToFrozenDictionary(data => data.RemoteRef, data => data);

            if (_aheadBehindDataByRemoteBranch.TryGetValue(gitRef.CompleteName, out AheadBehindData aheadBehind))
            {
                return (aheadBehind.ToDisplay(withCounts), GitRefName.RefsHeadsPrefix + aheadBehind.Branch, aheadBehind.AheadCount == AheadBehindData.Gone);
            }
        }
        else if (_aheadBehindDataByLocalBranch.TryGetValue(gitRef.Name, out AheadBehindData aheadBehind))
        {
            // This info is displayed in a virtual remote ref label.
            // From the remote ref's perspective, ahead/behind are swapped relative to the local branch.
            return (aheadBehind.ToDisplay(withCounts, reverse: true), aheadBehind.RemoteRef, aheadBehind.AheadCount == AheadBehindData.Gone);
        }

        return (string.Empty, string.Empty, false);
    }

    public AheadBehindData? GetAheadBehindData(bool isRemote, string completeName)
    {
        _aheadBehindDataByLocalBranch ??= _aheadBehindDataProvider?.GetData()
            ?? FrozenDictionary<string, AheadBehindData>.Empty;

        if (isRemote)
        {
            _aheadBehindDataByRemoteBranch ??= _aheadBehindDataByLocalBranch.Values
                .DistinctBy(data => data.RemoteRef)
                .ToFrozenDictionary(data => data.RemoteRef, data => data);
            return _aheadBehindDataByRemoteBranch.TryGetValue(completeName, out AheadBehindData dataByRemote)
                ? dataByRemote
                : null;
        }

        string branchName = completeName.StartsWith(GitRefName.RefsHeadsPrefix, StringComparison.Ordinal)
            ? completeName[GitRefName.RefsHeadsPrefix.Length..]
            : completeName;
        return _aheadBehindDataByLocalBranch.TryGetValue(branchName, out AheadBehindData data)
            ? data
            : null;
    }

    private List<RevisionGridRefRenderer.RefLabelControl> RentHitInfoList()
        => _hitInfoListPool.TryPop(out List<RevisionGridRefRenderer.RefLabelControl>? list) ? list : [];

    private void ReturnHitInfoList(List<RevisionGridRefRenderer.RefLabelControl> list)
    {
        list.Clear();
        _hitInfoListPool.Push(list);
    }

    private sealed class MessageContentPanel : Panel
    {
        private const int RefMarginRight = 5;

        private readonly Dictionary<Control, double> _minimumAdvances = [];

        public void ClearMinimumAdvances() => _minimumAdvances.Clear();

        public void SetMinimumAdvance(Control control, double advance)
            => _minimumAdvances[control] = advance;

        protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
        {
            double offset = 0;
            double height = 0;
            foreach (Control child in Children.Where(child => child.IsVisible))
            {
                double remainingWidth = Math.Max(0, availableSize.Width - offset);
                child.Measure(new Avalonia.Size(remainingWidth, availableSize.Height));
                offset += GetAdvance(child, remainingWidth);
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Avalonia.Size(offset, height);
        }

        protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
        {
            double offset = 0;
            foreach (Control child in Children.Where(child => child.IsVisible))
            {
                double remainingWidth = Math.Max(0, finalSize.Width - offset);
                double advance = GetAdvance(child, remainingWidth);

                // DrawImage may fit its pixels while its trailing advance extends
                // beyond the cell. Do not subtract that margin from a fitted image.
                double allocation = child is Image && advance > 0
                    ? advance
                    : Math.Min(remainingWidth, advance);
                if (child is Image image)
                {
                    // DrawImage uses integer division; framework centering would
                    // introduce half-DIP origins for odd-height cells.
                    image.Arrange(new Rect(offset,
                        ((int)finalSize.Height - (int)image.Height) / 2,
                        allocation, image.Height));
                }
                else
                {
                    child.Arrange(new Rect(offset, 0, allocation, finalSize.Height));
                }

                offset += advance;
            }

            return finalSize;
        }

        private double GetAdvance(Control child, double remainingWidth)
        {
            // A horizontal StackPanel measures refs with infinite width. DrawRefEx
            // instead clips each capsule to the remaining cell before advancing by
            // its painted width plus five; an empty capsule does not advance.
            double advance = child switch
            {
                RevisionGridRefRenderer.RefLabelControl label => GetRefAdvance(label, remainingWidth),
                RevisionGridRefRenderer.NestledRefLabelPanel pair => GetPairAdvance(pair, remainingWidth),
                Image image => GetImageAdvance(image, remainingWidth),
                MessageTextPanel => remainingWidth,
                _ => child.DesiredSize.Width,
            };

            // Artificial rows reserve the status-count span independently of the
            // capsule paint bounds. MinWidth would force that capsule past clipping.
            return Math.Max(advance, _minimumAdvances.GetValueOrDefault(child));
        }

        private static double GetImageAdvance(Image image, double remainingWidth)
        {
            // DrawImage requires a strict fit and leaves the original four-pixel
            // advance only when the image was actually painted.
            bool fits = image.Width < remainingWidth;
            image.Clip = fits ? null : new RectangleGeometry(default(Rect));
            return fits ? image.Width + image.Margin.Right : 0;
        }

        private static double GetRefAdvance(RevisionGridRefRenderer.RefLabelControl label, double remainingWidth)
        {
            // Stop drawing refs if we run out of room
            int width = label.GetCapsuleWidth(remainingWidth);
            return width > 0 ? width + RefMarginRight : 0;
        }

        private static double GetPairAdvance(RevisionGridRefRenderer.NestledRefLabelPanel pair, double remainingWidth)
        {
            RevisionGridRefRenderer.RefLabelControl first = (RevisionGridRefRenderer.RefLabelControl)pair.Children[0];
            RevisionGridRefRenderer.RefLabelControl second = (RevisionGridRefRenderer.RefLabelControl)pair.Children[1];
            int firstWidth = first.GetCapsuleWidth(remainingWidth);
            if (firstWidth <= 0)
            {
                return 0;
            }

            // DrawNestled resets from the clipped first rectangle rather than its
            // advance, then the second DrawRefEx consumes only the remaining width.
            double secondX = Math.Max(0, firstWidth - pair.PointWidth + 1);
            int secondWidth = second.GetCapsuleWidth(Math.Max(0, remainingWidth - secondX));
            return secondX + (secondWidth > 0 ? secondWidth + RefMarginRight : 0);
        }
    }

    private sealed class MessageTextPanel : Panel
    {
        private readonly Image _marker;
        private readonly TextBlock _subject;
        private readonly TextBlock _body;

        public MessageTextPanel(Image marker, TextBlock subject, TextBlock body)
        {
            _marker = marker;
            _subject = subject;
            _body = body;
            Children.Add(marker);
            Children.Add(subject);
            Children.Add(body);
        }

        public bool HasCommitMessage { get; set; }

        public bool CanRenderIndicator { get; private set; }

        protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
        {
            double offset = GetMarkerAdvance(availableSize.Width);
            double height = 0;
            foreach (TextBlock text in new[] { _subject, _body })
            {
                double remainingWidth = Math.Max(0, availableSize.Width - offset);
                Avalonia.Size textConstraint = new(remainingWidth, availableSize.Height);

                // First measurement applies the inherited/style typography before
                // the native literal advance is read from this retained control.
                text.Measure(textConstraint);
                int advance = GetTextAdvance(text);

                // EndEllipsis is decided by the source literal measurement, not
                // a different portable glyph advance for an otherwise fitting title.
                TextTrimming trimming = advance <= remainingWidth
                    ? TextTrimming.None
                    : TextTrimming.CharacterEllipsis;
                if (text.TextTrimming != trimming)
                {
                    text.TextTrimming = trimming;
                    text.Measure(textConstraint);
                }

                offset += advance;
                height = Math.Max(height, text.DesiredSize.Height);
            }

            if (_marker.IsVisible)
            {
                _marker.Measure(availableSize);
                height = Math.Max(height, _marker.DesiredSize.Height);
            }

            return new Avalonia.Size(offset, height);
        }

        protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
        {
            double offset = GetMarkerAdvance(finalSize.Width);
            if (_marker.IsVisible)
            {
                _marker.Clip = offset > 0 ? null : new RectangleGeometry(default(Rect));

                // DrawImage centers the original bitmap with integer division,
                // including truncation toward zero when the cell is shorter.
                _marker.Arrange(new Rect(0, ((int)finalSize.Height - (int)_marker.Height) / 2,
                    _marker.Width + _marker.Margin.Right, _marker.Height));
            }

            double subjectAdvance = GetTextAdvance(_subject);

            // The source paints into the entire remaining rectangle. Its returned
            // literal width advances the next painter; it does not clip this one.
            _subject.Arrange(new Rect(offset, 0,
                Math.Max(0, finalSize.Width - offset), finalSize.Height));
            offset += subjectAdvance;

            // DrawCommitMessage returns after an overlong title. Its indicator
            // remains reserved by MultilineIndicator, but is not painted.
            CanRenderIndicator = HasCommitMessage && offset <= finalSize.Width;
            _body.Arrange(new Rect(offset, 0,
                string.IsNullOrEmpty(_body.Text) ? 0 : Math.Max(0, finalSize.Width - offset), finalSize.Height));
            return finalSize;
        }

        private double GetMarkerAdvance(double availableWidth)
            => _marker.IsVisible && _marker.Width < availableWidth
                ? _marker.Width + _marker.Margin.Right
                : 0;

        // The source advances by NoPrefix/SingleLine/NoPadding measurement,
        // independently of the finite EndEllipsis allocation used to paint.
        private static int GetTextAdvance(TextBlock text)
            => (int)Math.Ceiling(WinFormsTextMeasurer.MeasureSize(text, text.Text ?? string.Empty).Width);
    }

    private sealed class MessageCell : DockPanel
    {
        private readonly MessageColumnProvider _provider;

        public MessageCell(MessageColumnProvider provider)
        {
            _provider = provider;

            // DataGridView routes input over the entire cell, including unpainted
            // rounded corners. Avalonia otherwise hit-tests only the drawn children.
            Background = Brushes.Transparent;
            FixupAndSquashMarker.Source = provider._fixupAndSquashImage;
            FixupAndSquashMarker.Classes.Add("revision-message-marker");
            Subject.Classes.Add("revision-subject");
            Subject.Margin = default;
            Body.Classes.Add("revision-body");
            MessagePanel = new MessageTextPanel(FixupAndSquashMarker, Subject, Body);
            ContentPanel.Margin = new Thickness(provider.ColumnLeftMargin, 0, 0, 0);
            ContentPanel.Children.Add(MessagePanel);
            SetDock(Indicator, Dock.Right);
            Children.Add(Indicator);
            Children.Add(ContentPanel);
            DoubleTapped += OnDoubleTapped;
        }

        public MessageContentPanel ContentPanel { get; } = new();

        public MessageTextPanel MessagePanel { get; }

        public Image FixupAndSquashMarker { get; } = new()
        {
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false,
        };

        public TextBlock Subject { get; } = CreateTextBlock();

        public TextBlock Body { get; } = CreateTextBlock();

        public MultilineIndicator Indicator { get; } = new();

        public GitRevision? Revision { get; set; }

        protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
        {
            Indicator.UpdateAvailableWidth(availableSize.Width);
            return base.MeasureOverride(availableSize);
        }

        protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
        {
            Indicator.UpdateAvailableWidth(finalSize.Width);
            Avalonia.Size result = base.ArrangeOverride(finalSize);
            if (Indicator.IsVisible)
            {
                // The source centers with integer division rather than the
                // framework's potentially half-DIP vertical alignment.
                Indicator.Arrange(new Rect(finalSize.Width - Indicator.Width,
                    ((int)finalSize.Height - (int)Indicator.Height) / 2,
                    Indicator.Width, Indicator.Height));
            }

            Indicator.Opacity = Revision is { IsArtificial: false, IsAutostash: false }
                && MessagePanel.CanRenderIndicator ? 1 : 0;
            return result;
        }

        public void ClearHighlight()
        {
            if (ReferenceEquals(_provider._highlightedCell, this))
            {
                _provider.SetHighlight(cell: null, label: null);
            }
            else
            {
                Cursor = null;
            }
        }

        private RevisionGridRefRenderer.RefLabelControl? HitTest(Func<Visual, Avalonia.Point> getPosition)
            => Revision is not null
                ? _provider.HitTest(_provider._grid.GetRevisionIndex(Revision), getPosition(_provider._grid))
                : null;

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (HitTest(e.GetPosition) is { GitRef: not null } label
                && _provider._grid.TryGoToRelatedRef(label.GitRef))
            {
                e.Handled = true;
            }
        }
    }
}
